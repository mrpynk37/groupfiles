namespace groupfiles
{
    using System.Collections.Generic;

    using CommandLine;

    public class Options
    {
        [Option("dir", Required = true)]
        public string Dir { get; set; }

        [Option("masks", Required = false, Separator = ',')]
        public IEnumerable<string> Masks { get; set; }

        [Option("recursive", Required = false)]
        public bool Recursive { get; set; }
    }
}