using System.Text;

namespace S100.Iso8211;

public sealed class Iso8211ReaderOptions
{
    /// <summary>
    /// Encoding used for text subfields whose field controls carry a blank (or unrecognised)
    /// truncated escape sequence. S-100 uses ISO 10646; UTF-8 is the safe default because it is a
    /// superset of the ISO 646 that a blank sequence nominally selects.
    /// </summary>
    public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Byte order of binary (<c>b*</c>) subfields. S-57 and the S-100 ISO/IEC 8211 profile both
    /// write least-significant byte first.
    /// </summary>
    public bool LittleEndianBinary { get; set; } = true;

    /// <summary>Keep each field's raw bytes on the parsed record. Handy for diagnostics, costs memory.</summary>
    public bool KeepRawFieldData { get; set; } = true;

    /// <summary>Continue past a record that fails to parse instead of throwing.</summary>
    public bool SkipMalformedRecords { get; set; }
}

/// <summary>The data descriptive record: leader, field control field, and one definition per tag.</summary>
public sealed class DataDescriptiveRecord
{
    public required Iso8211Leader Leader { get; init; }
    public required IReadOnlyList<Iso8211DirectoryEntry> Directory { get; init; }
    public required IReadOnlyDictionary<string, FieldDefinition> FieldDefinitions { get; init; }
    public FieldDefinition? FieldControlField { get; init; }

    public IReadOnlyList<(string Parent, string Child)> TagPairs =>
        FieldControlField?.TagPairs ?? Array.Empty<(string, string)>();

    public FieldDefinition? Find(string tag) =>
        FieldDefinitions.TryGetValue(tag, out var d) ? d : null;
}

/// <summary>
/// Reads an ISO/IEC 8211 (DDF) file: the S-100 Part 10a / S-57 physical encoding. Records are
/// streamed, so multi-hundred-megabyte cells do not have to be held in memory.
/// </summary>
public sealed class Iso8211Reader : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly Iso8211ReaderOptions _options;
    private IReadOnlyList<Iso8211DirectoryEntry>? _previousDirectory;
    private int _ordinal;

    public DataDescriptiveRecord Ddr { get; }
    public Iso8211ReaderOptions Options => _options;

    public static Iso8211Reader Open(string path, Iso8211ReaderOptions? options = null)
    {
        var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, FileOptions.SequentialScan);
        try { return new Iso8211Reader(fs, leaveOpen: false, options); }
        catch { fs.Dispose(); throw; }
    }

    public Iso8211Reader(Stream stream, bool leaveOpen = false, Iso8211ReaderOptions? options = null)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
        _options = options ?? new Iso8211ReaderOptions();
        Ddr = ReadDdr();
    }

    private DataDescriptiveRecord ReadDdr()
    {
        var (leader, directory, fieldArea) = ReadRawRecord()
            ?? throw new Iso8211Exception("The stream is empty: no data descriptive record found.");

        if (!leader.IsDataDescriptiveRecord)
            throw new Iso8211Exception($"First record has leader identifier '{leader.LeaderIdentifier}', expected 'L'.");

        var definitions = new Dictionary<string, FieldDefinition>(StringComparer.OrdinalIgnoreCase);
        FieldDefinition? fcf = null;

        foreach (var entry in directory)
        {
            if (entry.Position < 0 || entry.Position + entry.Length > fieldArea.Length)
                throw new Iso8211Exception($"DDR directory entry '{entry.Tag}' points outside the field area.");

            var span = fieldArea.AsSpan(entry.Position, entry.Length);
            var def = FieldDefinition.Parse(entry.Tag, span, leader.FieldControlLength, _options.DefaultEncoding);

            if (def.IsFieldControlField) fcf = def;
            else definitions[def.Tag] = def;
        }

        return new DataDescriptiveRecord
        {
            Leader = leader,
            Directory = directory,
            FieldDefinitions = definitions,
            FieldControlField = fcf
        };
    }

    /// <summary>Streams every data record after the DDR.</summary>
    public IEnumerable<DataRecord> ReadRecords()
    {
        while (true)
        {
            (Iso8211Leader Leader, IReadOnlyList<Iso8211DirectoryEntry> Directory, byte[] FieldArea)? raw;
            try
            {
                raw = ReadRawRecord();
            }
            catch (Iso8211Exception) when (_options.SkipMalformedRecords)
            {
                yield break;
            }

            if (raw is null) yield break;

            var (leader, directory, fieldArea) = raw.Value;

            if (leader.ReusesPreviousDirectory && _previousDirectory is not null)
                directory = _previousDirectory;
            else
                _previousDirectory = directory;

            DataRecord record;
            try
            {
                record = BuildRecord(++_ordinal, leader, directory, fieldArea);
            }
            catch (Iso8211Exception) when (_options.SkipMalformedRecords)
            {
                continue;
            }

            yield return record;
        }
    }

    private DataRecord BuildRecord(
        int ordinal, Iso8211Leader leader, IReadOnlyList<Iso8211DirectoryEntry> directory, byte[] fieldArea)
    {
        var fields = new List<DataField>(directory.Count);

        foreach (var entry in directory)
        {
            if (entry.Position < 0 || entry.Position + entry.Length > fieldArea.Length)
                throw new Iso8211Exception(
                    $"Record {ordinal}: directory entry '{entry.Tag}' points outside the field area.");

            var definition = Ddr.Find(entry.Tag)
                ?? throw new Iso8211Exception(
                    $"Record {ordinal}: tag '{entry.Tag}' has no data descriptive field in the DDR.");

            var span = fieldArea.AsSpan(entry.Position, entry.Length);
            var groups = FieldDataDecoder.Decode(definition, span, _options.LittleEndianBinary);

            fields.Add(new DataField
            {
                Tag = entry.Tag,
                Definition = definition,
                Groups = groups,
                RawData = _options.KeepRawFieldData ? span.ToArray() : ReadOnlyMemory<byte>.Empty
            });
        }

        return new DataRecord { Ordinal = ordinal, Leader = leader, Fields = fields };
    }

    private (Iso8211Leader, IReadOnlyList<Iso8211DirectoryEntry>, byte[])? ReadRawRecord()
    {
        byte[] leaderBytes = new byte[Iso8211Constants.LeaderLength];
        int read = ReadAtLeast(leaderBytes, leaderBytes.Length, throwOnEnd: false);
        if (read == 0) return null;
        if (read < leaderBytes.Length)
            throw new Iso8211Exception($"Truncated leader: only {read} of 24 bytes available.");

        var leader = Iso8211Leader.Parse(leaderBytes);

        int directoryLength = leader.BaseAddressOfFieldArea - Iso8211Constants.LeaderLength;
        if (directoryLength < 0)
            throw new Iso8211Exception($"Base address {leader.BaseAddressOfFieldArea} is inside the leader.");

        byte[] directoryBytes = new byte[directoryLength];
        ReadAtLeast(directoryBytes, directoryLength, throwOnEnd: true);

        var directory = ParseDirectory(leader, directoryBytes);

        int fieldAreaLength = leader.RecordLength > 0
            ? leader.RecordLength - leader.BaseAddressOfFieldArea
            : ComputeFieldAreaLength(directory);

        if (fieldAreaLength < 0)
            throw new Iso8211Exception($"Negative field area length in record (length {leader.RecordLength}).");

        byte[] fieldArea = new byte[fieldAreaLength];
        ReadAtLeast(fieldArea, fieldAreaLength, throwOnEnd: true);

        return (leader, directory, fieldArea);
    }

    private static IReadOnlyList<Iso8211DirectoryEntry> ParseDirectory(Iso8211Leader leader, byte[] bytes)
    {
        var entries = new List<Iso8211DirectoryEntry>();
        int entryLength = leader.DirectoryEntryLength;

        for (int pos = 0; pos + entryLength <= bytes.Length; pos += entryLength)
        {
            if (bytes[pos] == Iso8211Constants.FieldTerminator) break;

            var span = bytes.AsSpan(pos, entryLength);
            string tag = Ascii.ReadString(span[..leader.SizeOfFieldTag]).Trim();
            int length = Ascii.ReadInt(span.Slice(leader.SizeOfFieldTag, leader.SizeOfFieldLength), -1);
            int position = Ascii.ReadInt(
                span.Slice(leader.SizeOfFieldTag + leader.SizeOfFieldLength, leader.SizeOfFieldPosition), -1);

            if (length < 0 || position < 0)
                throw new Iso8211Exception($"Directory entry for tag '{tag}' has a non-numeric length or position.");

            entries.Add(new Iso8211DirectoryEntry(tag, length, position));
        }

        return entries;
    }

    private static int ComputeFieldAreaLength(IReadOnlyList<Iso8211DirectoryEntry> directory)
    {
        int end = 0;
        foreach (var e in directory) end = Math.Max(end, e.Position + e.Length);
        return end;
    }

    private int ReadAtLeast(byte[] buffer, int count, bool throwOnEnd)
    {
        int total = 0;
        while (total < count)
        {
            int n = _stream.Read(buffer, total, count - total);
            if (n == 0) break;
            total += n;
        }
        if (throwOnEnd && total < count)
            throw new Iso8211Exception($"Unexpected end of stream: wanted {count} bytes, got {total}.");
        return total;
    }

    public void Dispose()
    {
        if (!_leaveOpen) _stream.Dispose();
    }
}
