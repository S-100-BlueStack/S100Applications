# CRSH — Coordinate Reference System Header

Reference notes for the S-100 Part 10a / S-101 field that declares which coordinate reference
system a cell's coordinates are expressed in, and how `S100.Iso8211` surfaces it.

---

## Where CRSH sits

`CRSH` is not a standalone field. It lives inside the Coordinate Reference System record
(`RCNM` = 15), of which a cell carries exactly one:

```
Coordinate Reference System record
│
├── CSID   Coordinate Reference System Record Identifier   (exactly 1)
│
└── CRSH   Coordinate Reference System Header              (1 or more)
     ├── CSAX   Coordinate System Axes      (0 or 1)
     ├── PROJ   Projection                  (0 or 1)
     ├── GDAT   Geodetic Datum              (0 or 1)
     └── VDAT   Vertical Datum              (0 or 1)
```

**The nesting is positional.** ISO/IEC 8211 has no explicit parent pointers here, so `CSAX`, `PROJ`,
`GDAT` and `VDAT` belong to the `CRSH` that precedes them in the record's field order. Reading the
record as a flat list of fields loses that association.

**There is usually more than one CRSH.** A nautical cell is normally a *compound* CRS: one component
for horizontal position and one for depth. `CSID`'s `NCRC` subfield tells you how many to expect —
`1` means a single CRS, anything higher means compound.

---

## CSID — the record identifier

| Subfield | Label | Format | Meaning |
|---|---|---|---|
| Record name | `RCNM` | `b11` | Always 15 for this record |
| Record identification number | `RCID` | `b14` | 1 — there is only one CRS record per cell |
| Number of CRS components | `NCRC` | `b11` | 1 = single CRS, >1 = compound CRS |

Data descriptive field: `RCNM!RCID!NCRC` / `(b11,b14,b11)`

---

## CRSH — the header itself

One per CRS component.

| Subfield | Label | Format | Meaning |
|---|---|---|---|
| CRS index | `CRIX` | `b11` | Which component this is: 1, 2, 3 … up to `NCRC` |
| CRS type | `CRST` | `b11` | Geographic, projected, vertical … see below |
| Coordinate system type | `CSTY` | `b11` | Ellipsoidal, Cartesian, vertical … see below |
| CRS name | `CRNM` | `A` | `WGS84`, `Depth - lowest astronomical tide` |
| CRS identifier | `CRSI` | `A` | The code within the register, e.g. the EPSG code `4326` |
| CRS source | `CRSS` | `b11` | Which register `CRSI` refers to |
| CRS source information | `SCRI` | `A` | Free text; normally empty when `CRSS` names a register |

Data descriptive field: `CRIX!CRST!CSTY!CRNM!CRSI!CRSS!SCRI` / `(3b11,2A,b11,A)`

> **Edition note.** Pre-1.0.0 drafts declared six subfields — no `CRIX`, and the last one spelled
> `SRCI` rather than `SCRI` — with the format `(2b11,2A,b11,A)`. This library never assumes either
> layout: it reads the array descriptor and format controls out of the DDR, so both parse correctly
> and the subfield labels you get back are whatever your file actually declares.

### Enumerations

These are the values that appear in published S-101 worked examples. **The lists are partial** — I
have not seen the full enumerant tables, and in particular the projected-CRS codes are missing. Take
the complete lists from S-100 Part 10a before relying on them in code.

**`CRST` — CRS type**

| Value | Meaning |
|---|---|
| 1 | 2-D geographic |
| 5 | Vertical |

**`CSTY` — coordinate system type**

| Value | Meaning |
|---|---|
| 1 | Ellipsoidal coordinate system |
| 3 | Vertical coordinate system |

**`CRSS` — CRS source**

| Value | Meaning |
|---|---|
| 2 | EPSG |
| 255 | Not applicable (typical for a vertical component defined by `VDAT` instead) |

A vertical component commonly has `CRSI` and `SCRI` empty and `CRSS` = 255, because the datum is
named by the attached `VDAT` field rather than by a register lookup.

---

## The attached fields

Formats below are as declared in the S-101 test-data DDR; check them against your own DDR, which is
authoritative for the file in front of you.

### CSAX — Coordinate System Axes

Repeating: one entry per axis. `*AXTY!AXUM` / `(2b11)`

| Subfield | Label | Meaning |
|---|---|---|
| Axis type | `AXTY` | 12 = gravity-related depth |
| Axis unit of measure | `AXUM` | 4 = metres |

### PROJ — Projection

`PROM!PRP1!PRP2!PRP3!PRP4!PRP5!FEAS!FNOR` / `(b11,7b48)`

`PROM` selects the projection method; `PRP1`–`PRP5` are its parameters; `FEAS` and `FNOR` are false
easting and false northing. All seven numeric values are 8-byte doubles.

### GDAT — Geodetic Datum

`DTNM!ELNM!ESMA!ESPT!ESPM!CMNM!CMGL` / `(2A,b48,b11,b48,A,b48)`

| Subfield | Label | Meaning |
|---|---|---|
| Datum name | `DTNM` | e.g. `World Geodetic System 1984` |
| Ellipsoid name | `ELNM` | e.g. `WGS 84` |
| Semi-major axis | `ESMA` | metres, e.g. 6378137.0 |
| Parameter type | `ESPT` | selects how the second ellipsoid parameter is expressed |
| Second parameter | `ESPM` | e.g. inverse flattening 298.257223563 |
| Prime meridian name | `CMNM` | e.g. `Greenwich` |
| Prime meridian longitude | `CMGL` | degrees from Greenwich |

### VDAT — Vertical Datum

`DTIX!DTNM!DTID!DTSR!SCRI` / `(b11,2A,b11,A)`

| Subfield | Label | Meaning |
|---|---|---|
| Datum index | `DTIX` | `b11` |
| Datum name | `DTNM` | e.g. `lowest astronomical tide` |
| Datum identifier | `DTID` | text, not binary — see the note below |
| Datum source | `DTSR` | `b11` |
| Source information | `SCRI` | free text |

> **Watch the format.** In `(b11,2A,b11,A)` the two `A` subfields are `DTNM` **and `DTID`**, so the
> datum identifier is a *text* subfield and `DTSR` is the binary one. Writing `DTID` as `b11` is an
> easy mistake — it produces a control character where you expected a number. This library will
> decode it exactly as declared, which makes the error visible rather than silent.

Datum identifier values seen in published examples: 3 = mean sea level, 12 = mean lower low water,
16 = mean high water, 23 = lowest astronomical tide.

---

## What this library gives you

`S101DatasetReader` groups the record into components rather than flattening it:

```csharp
var dataset = S101DatasetReader.Read("101DK00DEMO.000");
S101CoordinateReferenceSystem? crs = dataset.CoordinateReferenceSystem;

if (crs?.ComponentCountMismatch == true)
    Console.WriteLine($"NCRC says {crs.ComponentCount}, found {crs.Components.Count}");

foreach (S101CrsComponent component in crs!.Components)
{
    Console.WriteLine($"{component.Index}: {component.Name} " +
                      $"(type {component.CrsType}, source {component.Source}, id {component.Identifier})");

    if (component.GeodeticDatum is { } gdat)
        Console.WriteLine($"   ellipsoid {gdat.GetString("ELNM")} a={gdat.GetDouble("ESMA")}");

    if (component.VerticalDatum is { } vdat)
        Console.WriteLine($"   sounding datum {vdat.GetString("DTNM")}");

    foreach (var axis in component.Axes?.Instances ?? [])
        Console.WriteLine($"   axis {axis["AXTY"]?.AsInt64()} in unit {axis["AXUM"]?.AsInt64()}");
}
```

Typed accessors (`Index`, `CrsType`, `Name`, `Identifier`, `Source`, …) read straight from the
underlying `DataField`, so nothing is lost: `component.Header` still exposes every subfield exactly
as the DDR declared it, and `component.Fields` holds any attached field, including ones this
document does not cover.

### Serialised output

```yaml
coordinateReferenceSystem:
  RCNM: 15
  RCID: 1
  NCRC: 2
  components:
    - CRIX: 1
      CRST: 1
      CSTY: 1
      CRNM: WGS 84
      CRSI: "4326"
      CRSS: 2
      SCRI: EPSG
      CSAX:
        - AXTY: 1
          AXUM: 1
        - AXTY: 2
          AXUM: 1
      GDAT:
        DTNM: World Geodetic System 1984
        ELNM: WGS 84
        ESMA: 6378137
        ESPT: 1
        ESPM: 298.257223563
        CMNM: Greenwich
        CMGL: 0
    - CRIX: 2
      CRST: 5
      CSTY: 3
      CRNM: Depth - lowest astronomical tide
      CRSI: ""
      CRSS: 255
      SCRI: ""
      CSAX:
        - AXTY: 12
          AXUM: 4
      VDAT:
        DTIX: 1
        DTNM: lowest astronomical tide
        DTID: "23"
        DTSR: 2
        SCRI: ""
```

`ComponentCountMismatch` is also reported through `S101Dataset.Warnings`, so a cell whose `NCRC`
disagrees with its actual `CRSH` count tells you without your having to check.

### Migrating from the flat list

`S101Dataset.CoordinateReferenceSystem` used to be `IReadOnlyList<KeyValuePair<string, object?>>`.
If you have a call site written against that, `ToKeyValuePairs()` restores the old shape without
losing anything — component content is qualified by index, so a compound CRS produces no duplicate
keys:

```csharp
// before
WriteKeyValues(w, "coordinateReferenceSystem", dataset.CoordinateReferenceSystem);

// after
WriteKeyValues(w, "coordinateReferenceSystem", dataset.CoordinateReferenceSystem.ToKeyValuePairs());
```

```
RCNM                     = 15
NCRC                     = 2
CRSH[1].CRNM             = WGS 84
CRSH[1].CSAX[1].AXTY     = 1
CRSH[1].GDAT.ELNM        = WGS 84
CRSH[2].CRNM             = Depth - lowest astronomical tide
CRSH[2].VDAT.DTNM        = lowest astronomical tide
```

`ToKeyValuePairs(qualifyKeys: false)` emits bare labels instead, which is only safe when
`NCRC` is 1. There is also a per-component overload for when you are already iterating
`Components`.

---

## A caution about flattening

Until this was fixed, the reader flattened every field of the CRS record into one mapping. With a
compound CRS that emits duplicate keys, and both YAML and JSON parsers keep the last one:

```yaml
coordinateReferenceSystem:
  NCRC: 2
  CRNM: WGS 84                              # horizontal
  CRNM: Depth - lowest astronomical tide    # vertical — silently wins
```

Anything consuming that document would conclude the cell's coordinates were in a vertical CRS. If
you write your own consumer of an S-101 CRS record, keep the components separate from the start.

---

## Sources and confidence

Structure and subfield labels come from S-100 Part 10a (ISO/IEC 8211 Encoding) and the worked
examples in the S-101 ENC Product Specification 1.0.0 and 1.1.0. Field formats for `PROJ`, `GDAT`
and `VDAT` come from the IHO S-101 test-dataset notes, which are older than the published product
specification and may have drifted.

Confidence, honestly stated:

- **High** — the record structure, the CSID subfields, the CRSH subfield labels and their order.
- **Medium** — the exact format controls for `PROJ`, `GDAT` and `VDAT`, and the `CRIX` addition
  relative to older drafts. Your DDR is authoritative and the library follows it.
- **Partial** — every enumeration in this document. Only the values appearing in published examples
  are listed; the full tables are in the standard.

None of this is a substitute for the IHO documents. Verify anything here against S-100 Part 10a and
your product specification edition before depending on it.
