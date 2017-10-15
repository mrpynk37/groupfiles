using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace groupfiles
{
    static class Program
    {
        private static object locker = new Object();
        
        static void Main(string[] args)
        {
            var options = CommandLine.Parser.Default
                .ParseArguments<Options>(args)
                .MapResult<Options, Options>(x => x, (err) => new Options());            
            Console.WriteLine(options.Dir);

            var files = ParseFiles(options.Dir, options.Masks);
            var length = files.Count();
            var counter = 0;
            
            var tasks = files.Select(f =>
            {
                return Task.Run(() =>
                {
                    Console.Write($"Start work on file {f} \n");
                    var fileInfo = new FileInfo(f);
                    var originalSize = fileInfo.Length;
                    var dirName = $"{options.Dir}\\{fileInfo.LastWriteTime.Year}-{fileInfo.LastWriteTime.Month:D2}";
                    if (!Directory.Exists(dirName))
                    {
                        lock (locker)
                        {
                            if (!Directory.Exists(dirName))
                            {
                                Directory.CreateDirectory(dirName);
                            }
                        }
                    }

                    File.Move(fileInfo.FullName, dirName + $"\\" + fileInfo.Name);
                }).ContinueWith(prev => {
                    if(prev.IsCompleted){
                        Console.WriteLine($"({Interlocked.Increment(ref counter)}/{length}) File {f} is ready");
                    }
                });
            });

            Task.WaitAll(tasks.ToArray());
        }

        private static IEnumerable<string> ParseFiles(string inputDir, IEnumerable<string> masks)
        {
            masks = masks == null || masks.Count() == 0 ? new List<string> { "*.*" } : masks;
            var regexs = masks.Select(mask => new Regex(mask));
            Console.WriteLine("Start read files");
            if (Directory.Exists(inputDir))
            {
                return masks.SelectMany(mask => Directory.EnumerateFiles(inputDir, mask));
            }

            throw new Exception("Directory does not exist");
        }
    }
}
