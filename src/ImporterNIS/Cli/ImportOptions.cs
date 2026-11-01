using CommandLine;

namespace S100Framework.Applications
{
    internal sealed class ImportOptions
    {
        [Option('d', "dataset", Required = false, HelpText = "")]
        public string? Dataset { get; set; }

        [Option('t', "target", Required = true, HelpText = "Target Geodatabase.")]
        public string? Target { get; set; }

        [Option('s', "source", Required = true, HelpText = "Source Geodatabase.")]
        public string? Source { get; set; }

        [Option('c', "clipping", Required = false, HelpText = "Clip features.", Default = false)]
        public bool Clipping { get; set; }



        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('f', "filter", Required = false, HelpText = "Filter datasets (RegEx).")]
        public string? Filter { get; set; } = default;

        [Option("s128", Required = false, HelpText = "Target ElectronicProducts.")]
        public string? S128 { get; set; }

        [Option('n', "notespath", Required = false, HelpText = "Path to notes files references in TXTDSC.")]
        public string? NotesPath { get; set; }

        [Option("skinofearthonly", Required = false, HelpText = "Exports only DEPARE, DRGARE, UNSARE and LNDARE.")]
        public string? SkinOfEarthOnly { get; set; }

        [Option("scaminfiles", Required = false, HelpText = "Path to folder with scamin files. Supports only Grønland and Denmark scamin files.")]
        public string? ScaminFilesPath { get; set; }


        [Option('l', "minimumDisplayScale", Default = int.MaxValue, Required = false)]
        public int? MinimumDisplayScale { get; set; }

        [Option('u', "maximumDisplayScale", Default = 0, Required = false)]
        public int? MaximumDisplayScale { get; set; }

        [Option("initialize-target", Default = true, HelpText = "Create a NEW mobile geodatabase from the bundled template.")]
        public bool InitializeTarget { get; set; }
    }

}
