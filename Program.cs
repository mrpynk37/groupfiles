using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;

namespace groupfiles
{
    static class Program
    {
        static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
            var options = result.MapResult<Options, Options>(x => x, (err) => new Options());
            Console.WriteLine(options.Dir);

            var files = ParseFiles(options.Dir, options.Masks);
            var tasks = files.Select(f =>
            {
                Console.WriteLine($"Create task for file {f}");
                return Task.Run(() =>
                {
                    var fileInfo = new FileInfo(f);
                    var dirName = $"{options.Dir}\\{fileInfo.LastWriteTime.Year}-{fileInfo.LastWriteTime.Month:D2}";
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    File.Move(fileInfo.FullName, dirName + $"\\" + fileInfo.Name);
                    Console.WriteLine($"File {fileInfo.Name} is ready");
                });
            });
            Task.WaitAll(tasks.ToArray());
        }

        private static IEnumerable<string> ParseFiles(string inputDir, IEnumerable<string> masks)
        {
            var regexs = masks.Select(mask => new Regex(mask));
            Console.WriteLine("Start read files");
            if (Directory.Exists(inputDir))
            {
                return (masks ?? new List<string> { "*.*" }).SelectMany(mask => Directory.EnumerateFiles(inputDir, mask));
            }

            throw new Exception("Directory does not exist");
        }
    }
}
