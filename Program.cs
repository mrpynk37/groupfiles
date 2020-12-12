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
        private static object locker = new();

        static void Main(string[] args)
        {
            var options = Parser.Default
                .ParseArguments<Options>(args)
                .MapResult(x => x, (err) => new Options());
            Console.WriteLine(options.Dir);

            var files = ParseFiles(options.Dir, options.Masks, options.Recursive);
            var length = files.Count();
            var counter = 0;

            var tasks = files.Select(f =>
            {
                return Task.Run(() =>
                {
                    Console.Write($"Start work on file {f} \n");
                    //// Prevent control symbol from WebDAV folder
                    if (f[^1] == '\0') f = f[0..^1];
                    var fileInfo = new FileInfo(f);
                    var originalSize = fileInfo.Length;
                    var dirName = $"{options.Dir}\\" + (fileInfo.Name.Length > 9 && DateTime.TryParse(fileInfo.Name.Substring(0, 10), out DateTime date)
                        ? date.ToString("yyyy-MM")
                        : $"{fileInfo.LastWriteTime.Year}-{fileInfo.LastWriteTime.Month:D2}");
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
                }).ContinueWith(prev =>
                {
                    if (prev.IsCompleted)
                    {
                        Console.WriteLine($"({Interlocked.Increment(ref counter)}/{length}) File {f} is ready");
                    }
                });
            });

            Task.WaitAll(tasks.ToArray());
        }

        private static IEnumerable<string> ParseFiles(string inputDir, IEnumerable<string> masks, bool recursive = false)
        {
            masks = masks == null || !masks.Any() ? new List<string> { "*.*" } : masks;
            Console.WriteLine("Start read files");
            if (Directory.Exists(inputDir))
            {
                var result = new List<string>();
                result.AddRange(masks.SelectMany(mask => Directory.EnumerateFiles(inputDir)));
                if (recursive)
                {
                    var currentDirs = Directory.EnumerateDirectories(inputDir);
                    while (currentDirs != null && currentDirs.Any())
                    {
                        result.AddRange(currentDirs.SelectMany(cd => masks.SelectMany(mask => Directory.EnumerateFiles(cd, mask))));
                        currentDirs = currentDirs.SelectMany(cd => Directory.EnumerateDirectories(cd) ?? new List<string>());
                    }
                }

                return result;
            }

            throw new Exception("Directory does not exist");
        }
    }
}
