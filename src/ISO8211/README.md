# S100.Iso8211

A C# library that reads ISO/IEC 8211 (DDF) files — the physical encoding behind IHO **S-100 Part 10a / S-101** and **S-57** — and converts them to **YAML** (default) or JSON, including the full header and the features with assembled geometry.

Targets `net8.0`, no external dependencies (`System.Text.Json` only).

```
docs/CRSH.md                   reference notes for the coordinate reference system record
src/S100.Iso8211               the library
  Serialization/               YAML + JSON writers behind one abstraction
  S101/                        the S-101 semantic layer
tools/S100.Iso8211.Cli         s100json command line converter
tests/S100.Iso8211.SelfTest    builds a synthetic S-101 cell, reads it back, asserts 74 checks
```

```bash
dotnet build
dotnet run --project tests/S100.Iso8211.SelfTest    # → "All checks passed."
```

---

## Two layers, deliberately separated

**Layer 0 — `S100.Iso8211.Serialization`** is a thin `IStructuredWriter` abstraction with a JSON implementation over `Utf8JsonWriter` and a hand-written block-style YAML emitter. The document writers target the interface, so the two formats never drift apart.

**Layer 1 — `S100.Iso8211`** is a complete, product-agnostic ISO/IEC 8211:1994 reader. It knows nothing about charts. This layer is fully specified by the standard, so it should be correct for any conforming file: S-101, S-102 exchange sets, S-57 ENCs, SDTS, whatever.

**Layer 2 — `S100.Iso8211.S101`** interprets those records as an S-101 dataset: metadata, vector records, features, geometry. S-101 field tags and record-name codes have moved between editions, so **everything version-specific lives in `S101Codes.cs` as plain constants**. If your product specification edition differs, you edit that one file rather than hunting through parsing logic.

Where I could avoid depending on edition-specific detail, I did:

- Records are classified by **which identifier field they carry** (`FRID`, `PRID`, `SRID`…), not by guessing from a numeric code.
- Reference resolution keys on `(RCNM, RCID)` read from each record's *own* identifier field, so no hard-coded `RCNM → type` table is needed for lookups.
- Coordinate fields are found by **subfield label** (`XCOO` / `YCOO` / `ZCOO`), so `C2IT`, `C3IL`, `C2FT`, the older `C2DI`/`C3DF` spellings and vendor variants all work without a tag list.

---

## Usage

### Lossless dump of everything

```csharp
using var reader = Iso8211Reader.Open("101DK00DEMO.000");

Iso8211DocumentWriter.WriteToFile(reader, "cell.raw.yaml");   // format inferred from extension
Iso8211DocumentWriter.WriteToFile(reader, "cell.raw.txt", OutputFormat.Json);   // or forced
Iso8211DocumentWriter.Write(reader, stream);                  // defaults to YAML
```

Emits the DDR leader, the field control field with its tag pairs, every data descriptive field (name, structure/type codes, array descriptor, format controls, resolved subfields), then every record.

### Features with geometry

```csharp
var dataset = S101DatasetReader.Read("101DK00DEMO.000", new S101ReaderOptions
{
    FeatureTypeNames = featureCatalogueCodes,   // optional: code → name
    AttributeNames   = attributeCatalogueCodes  // optional: code → name
});

foreach (var f in dataset.Features)
    Console.WriteLine($"{f.FeatureObjectId} {f.FeatureTypeName} {f.Geometry?.Type}");

S101DocumentWriter.WriteToFile(dataset, "cell.features.yaml");
S101DocumentWriter.WriteToFile(dataset, "cell.features.json");
```

### Walking records directly

```csharp
using var reader = Iso8211Reader.Open(path);

foreach (var record in reader.ReadRecords())          // streamed, not buffered
{
    if (record["FRID"] is not { } frid) continue;
    long? rcid = frid.GetInt64("RCID");

    foreach (var attr in record["ATTR"]?.Instances ?? [])
        Console.WriteLine($"{attr["ATLB"]?.AsString()} = {attr["ATVL"]?.AsString()}");
}
```

### CLI

```bash
s100json cell.000                              # YAML features + header → cell.000.yaml
s100json cell.000 -m raw -o dump.yaml          # lossless
s100json cell.000 -f json -m geojson -o -      # plain RFC 7946, JSON, to stdout
s100json cell.000 -o cell.json                 # format inferred from the extension
s100json cell.000 --big-endian --skip-malformed
```

Format resolution: an explicit `-f/--format` wins, then the output file extension, then YAML.

---

## What the reader handles

| Aspect | Support |
|---|---|
| DDR and DR leaders | Full, including the entry map and `'R'` leaders that reuse the previous directory |
| Record length `00000` | Falls back to computing the field area from the directory |
| Format controls | `A(n)`, `I(n)`, `R(n)`, `B(n)`, `X(n)`, `b11`–`b48`, bare `A`/`I`/`R`, repeat counts, nested groups `3(b11,A)` |
| Variable-length subfields | UT/FT delimited |
| Format cycling | Formats repeat when there are more labels than format items |
| Repeating fields (`*`) | Parsed until the field data is exhausted |
| **Concatenated fields** (`\`) | Fixed head group + repeating tail, e.g. `VDID\*YCOO!XCOO!ZCOO` |
| Binary byte order | Little-endian by default (S-57 / S-100 profile); switchable |
| Character sets | Truncated escape sequence → UTF-8, ISO 8859-1, UCS-2; blank defaults to UTF-8 |
| Memory | Records streamed one at a time; the S-101 layer holds the cell in memory because features reference spatial records by id |

The coordinate reference system record is read as a list of components rather than a flat mapping,
because a cell is normally compound — a horizontal CRS plus a vertical one. `NCRC` is checked against
the number of `CRSH` fields present and a mismatch is reported in `Warnings`.
[docs/CRSH.md](docs/CRSH.md) documents the record in full, including how to migrate a call site
written against the previous flat-list shape via `ToKeyValuePairs()`.

Geometry assembly covers point, multi point, curve (`PTAS` begin/end node + interior vertices), composite curve (`CUCO`, honouring `ORNT`), and surface (`RIAS` rings, exterior/interior by `USAG`, rings auto-closed). Multiple spatial associations collapse into `MultiPoint` / `MultiLineString` / `MultiPolygon`, or a `GeometryCollection` when the types differ. Cycles are detected and reported rather than hanging.

Dangling references become per-feature `warnings` in the JSON instead of exceptions — a bad reference in one feature shouldn't kill a whole conversion.

---

## Output shapes

Identical structure in both formats — the self-test parses the YAML and the JSON and asserts the
two documents are equal. Raw mode, field values:

- field whose only group repeats → **array of objects** (`ATTR`, `SPAS`, `C2IL`)
- field with a single non-repeating group → **object** (`FRID`, `FOID`, `DSID`)
- concatenated field → object with the fixed subfields plus a `values` array

```yaml
C3IL:
  VDID: 23
  values:
    - YCOO: 557100000
      XCOO: 126100000
      ZCOO: 12500
```

Feature mode is a GeoJSON `FeatureCollection` with a `header` object added (dropped entirely with `-m geojson`). Complex attributes nest; repeated attributes become arrays:

```yaml
attributes:
  lightSector:
    sectorLimitOne:
      - "340"
      - "330"
  featureName: Køge Bugt N
```

### About the YAML

Block style, two-space indent, no anchors or aliases. Scalars go unquoted only when that is
unambiguously safe — the first character must be a letter or underscore, which rules out every
numeric, timestamp and indicator form in one test — and are double-quoted otherwise. So a text
subfield holding `340` or `true` survives as a string rather than turning into a number or a
boolean on the way back in. Empty mappings and sequences collapse to `{}` and `[]` rather than
becoming null.

The emitter is ~200 lines with no third-party dependency, which keeps the library dependency-free
and the escaping rules auditable. If you would rather have YamlDotNet's emitter, implementing
`IStructuredWriter` over it is a drop-in swap.

---

## Assumptions worth checking against your data

These are the places where I had to commit to an interpretation. Each is one line to change.

1. **Coordinate scaling** — `real = origin + integer / multiplicationFactor`, using `DCOX`/`DCOY`/`DCOZ` and `CMFX`/`CMFY`/`CMFZ` from `DSSI`. Scaling is applied only when the subfield format is an integer type, so floating-point coordinate fields pass through untouched. Override via `S101ReaderOptions.Transform`.

2. **Attribute nesting** — `PAIX` is treated as the 1-based row number of the parent attribute within the same `ATTR` field, `0` meaning top level. This matches the S-101 complex-attribute examples, but check it against a real cell with nested attributes.

3. **Feature type subfield** — read from `NFTC`, falling back to `OBJC`, `FTYP`, `FCID`, `OBJL`. Add yours to the list in `BuildFeature` if it differs.

4. **CRS components** — `CSAX`, `PROJ`, `GDAT` and `VDAT` are attached to the `CRSH` that precedes
   them in field order, since ISO 8211 carries no explicit parent pointer. See
   [docs/CRSH.md](docs/CRSH.md).

5. **`CRID` ambiguity** — Curve Record Identifier in S-101 1.0.0, but the CRS record identifier in pre-1.0 drafts. Disambiguated by checking `RCNM` (15 = CRS).

6. **Surface rings** — one `RIAS` entry per ring. If your data splits a single ring across several `RIAS` entries, `RingsFor` in `GeometryAssembler.cs` needs to accumulate instead.

7. **Feature/attribute names** need the S-101 Feature Catalogue, which is out of scope here. Supply the code→name maps via `S101ReaderOptions` and the JSON uses readable names.

---

## Verification

The self-test builds a synthetic cell from scratch with `Iso8211TestWriter` (a small ISO 8211 encoder, also handy for making fixtures), then reads it back and asserts on the leader, tag pairs, format cycling, concatenated fields, UTF-8 round-tripping, binary decoding of `b11`/`b12`/`b14`/`b24`/`b48`, coordinate scaling, complex attribute nesting, and the assembled Point / LineString / Polygon / MultiPoint geometry — including that a polygon ring built from three separate curves joined through a composite curve comes out closed and correctly ordered.

Serialisation is covered too: 17 scalar-quoting cases, YAML indentation and structure checks, and
extension-based format inference. During development I additionally parsed both outputs with PyYAML
and asserted the YAML and JSON documents deserialise to the same object graph.

It's a console app rather than xUnit so it runs with no package restore. Converting it to xUnit is mechanical if you'd rather it lived in CI as a normal test project.

The one thing I could not do is run it against a real S-101 cell. The ISO 8211 layer I'd expect to be solid; the S-101 layer is where I'd want your eyes, particularly points 1–3 above.
