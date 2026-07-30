# Manifest

Every file in `S100.Iso8211`. Verify with:

```bash
find . -name '*.cs' -o -name '*.csproj' -o -name '*.sln' -o -name '*.md' | \
  grep -v -E "bin/|obj/" | sort | xargs sha256sum | cut -c1-16
```

| File | Lines | SHA-256 (first 16) |
|---|---:|---|
| `README.md` | 201 | `875938c8f5c5c464` |
| `S100.Iso8211.sln` | 45 | `3d84cf9050e037eb` |
| `docs/CRSH.md` | 302 | `81af71f4878d6ff6` |
| `src/S100.Iso8211/DataRecord.cs` | 314 | `e5474ad23d00f0e4` |
| `src/S100.Iso8211/FieldDefinition.cs` | 205 | `b133e3879a6b5ad3` |
| `src/S100.Iso8211/Iso8211Reader.cs` | 253 | `4ae335bd46a350d5` |
| `src/S100.Iso8211/Primitives.cs` | 181 | `a40fd01f743a670c` |
| `src/S100.Iso8211/S100.Iso8211.csproj` | 14 | `5addeae50a20550f` |
| `src/S100.Iso8211/S101/GeometryAssembler.cs` | 242 | `7fce5a75a5017fcb` |
| `src/S100.Iso8211/S101/S101CrsExtensions.cs` | 72 | `be809de128b8992f` |
| `src/S100.Iso8211/S101/S101DatasetReader.cs` | 482 | `353c5ea7863303b8` |
| `src/S100.Iso8211/S101/S101DocumentWriter.cs` | 323 | `afdf43e5f30d0067` |
| `src/S100.Iso8211/S101/S101Model.cs` | 316 | `0ef755dd4e09b9d5` |
| `src/S100.Iso8211/Serialization/IStructuredWriter.cs` | 80 | `97b32b9b7c97f4cc` |
| `src/S100.Iso8211/Serialization/Iso8211DocumentWriter.cs` | 210 | `62b54411adaa9b5a` |
| `src/S100.Iso8211/Serialization/JsonStructuredWriter.cs` | 46 | `3a4b74de835fbd7d` |
| `src/S100.Iso8211/Serialization/YamlStructuredWriter.cs` | 271 | `96c7243b125da865` |
| `src/S100.Iso8211/SubfieldFormat.cs` | 223 | `e9af73b4c2f02808` |
| `tests/S100.Iso8211.SelfTest/Iso8211TestWriter.cs` | 182 | `1a7c603878c7e281` |
| `tests/S100.Iso8211.SelfTest/Program.cs` | 369 | `f883c9c37fbfaea4` |
| `tests/S100.Iso8211.SelfTest/S100.Iso8211.SelfTest.csproj` | 13 | `83762dda6209ef5b` |
| `tests/S100.Iso8211.SelfTest/SampleCell.cs` | 216 | `0efbc4f7067b3a7e` |
| `tools/S100.Iso8211.Cli/Program.cs` | 233 | `cebc067fc2cc7579` |
| `tools/S100.Iso8211.Cli/S100.Iso8211.Cli.csproj` | 14 | `ee61b5dc32f51ea7` |

24 files, 4807 lines total.

## Build state as delivered

```
dotnet build -c Release   ->  0 Warning(s), 0 Error(s)
dotnet run --project tests/S100.Iso8211.SelfTest
                          ->  74 checks, All checks passed.
```

## The four files that change together

The coordinate reference system fix spans these. A partial copy produces
`CS1503: Argument 3: cannot convert from S101CoordinateReferenceSystem to
IReadOnlyList<KeyValuePair<string, object?>>`, because a pre-fix
`S101DocumentWriter.cs` still calls `WriteKeyValues` where the current one calls
`WriteCoordinateReferenceSystem`.

- `src/S100.Iso8211/S101/S101Model.cs`
- `src/S100.Iso8211/S101/S101DatasetReader.cs`
- `src/S100.Iso8211/S101/S101DocumentWriter.cs`
- `src/S100.Iso8211/S101/S101CrsExtensions.cs`
