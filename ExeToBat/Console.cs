using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using static ExeToBat.Generator;
using static System.ConsoleUtils;

namespace ExeToBat
{
    class Console
    {
        static void Main() => new Console().MainMenu();

        public Console() { }

        private readonly Generator generator = new Generator();

        private void MainMenu()
        {
            Dictionary<string, Action> options = new Dictionary<string, Action>
            {
                { "Files", ChooseSource },
                { "Generate", Generate },
            };

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (Options) => System.Console.WriteLine("ExeToBat"),
                DisplayEntry = (Options, index, i) =>
                {
                    System.Console.WriteLine("[{0}] {1}", i, Options[index]);
                },
                HandleEntry = (Options, index) =>
                {
                    options[Options[index]]();
                    return false;
                },
                ExitEntry = "Exit",
            }.Show();
        }

        private void ChooseSource()
        {
            new ListMenu<SourceFile>(generator.Sources)
            {
                DisplayTitle = (sources) =>
                {
                    System.Console.WriteLine("ExeToBat > Files");
                    System.Console.WriteLine(
                        "[{0}] ({1})",
                        Convert.ToString(0).PadLeft(Convert.ToString(sources.Count).Length, ' '),
                        "Add Files"
                    );
                },
                DisplayEntry = (sources, index, i) =>
                    System.Console.WriteLine(
                        "[{0}] {1}",
                        Convert.ToString(i).PadLeft(Convert.ToString(sources.Count).Length, ' '),
                        Path.GetFileName(sources[index].Path)
                    ),
                ZeroEntry = (sources) =>
                {
                    AddSource();
                    return false;
                },
                HandleEntry = (sources, index) =>
                {
                    ManageSource(sources[index]);
                    return false;
                },
                RefreshEntries = (sources) => generator.Sources,
            }.Show();
        }

        private void AddSource()
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                System.Console.Clear();
                System.Console.WriteLine("ExeToBat > Files > Add");
                System.Console.Write("\n");
                System.Console.Write("{0}> ", "File/Folder");
                string input = System.Console.ReadLine();

                input.Trim();
                input = input.Replace("\"", "");
                if (!string.IsNullOrEmpty(input))
                {
                    switch (input)
                    {
                        case var i when Directory.Exists(i):
                            IsInputValid = true;
                            foreach (string file in Directory.GetFiles(input))
                            {
                                generator.Sources.Add(new SourceFile(file));
                            }
                            break;
                        case var i when File.Exists(i):
                            IsInputValid = true;
                            generator.Sources.Add(new SourceFile(input));
                            break;
                        default:
                            ResetInput();
                            break;
                    }
                }
                else
                {
                    IsInputValid = true;
                }
            }
        }

        private void ManageSource(SourceFile source)
        {
            Dictionary<string, Func<bool>> options = new Dictionary<string, Func<bool>>
            {
                {
                    "Edit",
                    () =>
                    {
                        ModifySource(source);
                        return false;
                    }
                },
                {
                    "Position",
                    () =>
                    {
                        EditPosition(source);
                        return false;
                    }
                },
                {
                    "Delete",
                    () =>
                    {
                        generator.Sources.Remove(source);
                        return true;
                    }
                },
            };

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (Options) =>
                    System.Console.WriteLine(
                        "ExeToBat > Files > {0}",
                        Path.GetFileName(source.Path)
                    ),
                DisplayEntry = (Options, index, i) =>
                    System.Console.WriteLine("[{0}] {1}", i, Options[index]),
                HandleEntry = (Options, index) => options[Options[index]](),
            }.Show();
        }

        private void ModifySource(SourceFile source)
        {
            List<(string, string, Action)> options()
            {
                List<(string, string, Action)> result = new List<(string, string, Action)>()
                {
                    ("File", source.Path, () => { }),
                    ("Extraction directory", source.Directory, () => EditExtraction(source)),
                    (
                        "Execute after extraction",
                        source.Execute.ToString(),
                        () => source.Execute = !source.Execute
                    ),
                };
                if (source.Execute)
                {
                    result.Add(("Parameters", source.Parameters, () => EditParameters(source)));
                    result.Add(
                        ("Wait for exit", source.Wait.ToString(), () => source.Wait = !source.Wait)
                    );
                }
                if (source.Execute && source.Wait)
                {
                    result.Add(
                        (
                            "Delete after execution",
                            source.Delete.ToString(),
                            () => source.Delete = !source.Delete
                        )
                    );
                }
                return result;
            }

            new ListMenu<(string, string, Action)>(options())
            {
                DisplayTitle = (Options) =>
                    System.Console.WriteLine(
                        "ExeToBat > Files > {0} > Edit",
                        Path.GetFileName(source.Path)
                    ),
                DisplayEntry = (Options, index, i) =>
                {
                    int MaxLength = options().ConvertAll(e => e.Item1.Length).Max();
                    System.Console.WriteLine(
                        "[{0}] {1} | {2}",
                        i,
                        Options[index].Item1.PadRight(MaxLength, ' '),
                        Options[index].Item2
                    );
                },
                HandleEntry = (Options, index) =>
                {
                    Options[index].Item3();
                    return false;
                },
                RefreshEntries = (Options) => options(),
            }.Show();
        }

        private void EditExtraction(SourceFile source)
        {
            System.Console.Clear();
            System.Console.WriteLine(
                "ExeToBat > Files > {0} > Edit > Extraction",
                Path.GetFileName(source.Path)
            );
            System.Console.WriteLine();
            System.Console.WriteLine("Documentation: ");
            System.Console.WriteLine("https://ss64.com/nt/syntax-variables.html");
            System.Console.WriteLine("https://ss64.com/nt/syntax-args.html");
            System.Console.WriteLine();
            System.Console.Write("{0}> ", "Directory");
            string input = System.Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                source.Directory = input;
            }
        }

        private void EditParameters(SourceFile source)
        {
            System.Console.Clear();
            System.Console.WriteLine(
                "ExeToBat > Files > {0} > Edit > Parameters",
                Path.GetFileName(source.Path)
            );
            System.Console.WriteLine();
            System.Console.Write("{0}> ", "Parameters");
            string input = System.Console.ReadLine();

            input.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                source.Parameters = input;
            }
        }

        private void EditPosition(SourceFile source)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                System.Console.Clear();
                System.Console.WriteLine(
                    "ExeToBat > Files > {0} > Position : {1}",
                    Path.GetFileName(source.Path),
                    generator.Sources.IndexOf(source)
                );

                System.Console.WriteLine();
                System.Console.Write("{0}> ", "New index");
                string input = System.Console.ReadLine();

                if (int.TryParse(input, out int index))
                {
                    if (index < generator.Sources.Count)
                    {
                        generator.Sources.Remove(source);
                        generator.Sources.Insert(index, source);
                        IsInputValid = true;
                    }
                    else
                    {
                        ResetInput();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(input))
                    {
                        IsInputValid = true;
                    }
                    else
                    {
                        ResetInput();
                    }
                }
            }
        }

        private void Generate()
        {
            System.Console.Clear();
            System.Console.WriteLine("ExeToBat > Generate");

            generator.Generation += OnGenerate;

            generator.Generate("output.bat");

            generator.Generation -= OnGenerate;

            System.Console.WriteLine("Press anything...");
            System.Console.ReadKey();
        }

        private void OnGenerate(object sender, GeneratorEvent e)
        {
            switch (e)
            {
                case GenerationStartEvent s:
                    System.Console.WriteLine("Starting generation...");
                    System.Console.WriteLine("{0} files scheduled", s.Files.Count);
                    break;
                case ReadingFileEvent s:
                    System.Console.WriteLine("[{0}] Reading file", s.File.Path);
                    break;
                case WritingFilePartEvent s:
                    System.Console.Write(
                        "[{0}] writing part {1}/{2}\r",
                        s.File.Path,
                        s.Part.ToString().PadLeft(s.Max.ToString().Length, '0'),
                        s.Max
                    );
                    break;
                case WritingFileDecoderEvent s:
                    System.Console.WriteLine();
                    System.Console.WriteLine("[{0}] Writing decode mechanism", s.File.Path);
                    break;
                case WritingFileExecuteEvent s:
                    System.Console.WriteLine("[{0}] Writing execute mechanism", s.File.Path);
                    break;
                case WritingFileWaitEvent s:
                    System.Console.WriteLine("[{0}] Writing wait mechanism", s.File.Path);
                    break;
                case WritingFileDeleteEvent s:
                    System.Console.WriteLine("[{0}] Writing delete mechanism", s.File.Path);
                    break;
                case WritingFileCompleteEvent s:
                    System.Console.WriteLine("[{0}] Finshed generating!", s.File.Path);
                    break;
                case GenerationCompleteEvent s:
                    System.Console.WriteLine("Finished generation! Result written to {0}", s.Path);
                    break;
                case GenerationEmptyEvent _:
                    System.Console.WriteLine("No files specified");
                    break;
                case GenerationFailedEvent s:
                    System.Console.Write("Generation failed with: {0}", s.Error.ToString());
                    break;
            }
        }
    }
}
