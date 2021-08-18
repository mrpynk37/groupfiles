namespace groupfiles
{
    using System.Collections.Generic;

    using CommandLine;

    public class Options
    {
        [Option('d', "dir", Required = true)]
        public string Dir { get; set; }

        [Option('m', "masks", Required = false, Separator = ',')]
        public IEnumerable<string> Masks { get; set; }

        [Option('r', "recursive", Required = false)]
        public bool Recursive { get; set; }
        
        [Option('o', "overwrite", Required = false)]
        public bool Overwrite { get; set; }
    }
}