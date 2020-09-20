using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ExeToBat
{
    static class Generator
    {
        public const int chunk = 8000;

        public static List<SourceFile> Sources = new List<SourceFile>();

        static void Main() => MainMenu();

        static void MainMenu()
        {
            Dictionary<string, Action> options = new Dictionary<string, Action> {
                { "Files", ChooseSource },
                { "Generate", () => BuildBat(Sources, "output.bat") }
            };

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (List<string> Options) => Console.WriteLine("ExeToBat > Main"),
                DisplayEntry = (List<string> Options, int index, int i) =>
                {
                    Console.WriteLine("[{0}] {1}", i, Options[index]);
                },
                HandleEntry = (List<string> Options, int index) =>
                {
                    if (options.ContainsKey(Options[index]))
                    {
                        options[Options[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },
                ExitEntry = "Exit",

            }.Show();
        }

        static void ChooseSource()
        {
            new ListMenu<SourceFile>(Sources)
            {
                DisplayTitle = (List<SourceFile> sources) =>
                {
                    Console.WriteLine("ExeToBat > Main > Files");
                    Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(sources.Count).Length, ' '), "Add Files");
                },
                DisplayEntry = (List<SourceFile> sources, int index, int i) =>
                    Console.WriteLine("[{0}] {1}",
                        Convert.ToString(i).PadLeft(
                        Convert.ToString(sources.Count).Length, ' '),
                        Path.GetFileName(sources[index].File)),
                ZeroEntry = (List<SourceFile> sources) =>
                {
                    AddSource();
                    return false;
                },
                HandleEntry = (List<SourceFile> sources, int index) =>
                {
                    ManageSource(sources[index]);
                    return false;
                },
                RefreshEntries = (List<SourceFile> sources) => Sources,

            }.Show();
        }

        static void AddSource()
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {

                Console.Clear();
                Console.WriteLine("ExeToBat > Main > Files > Add");
                Console.Write("\n");
                Console.Write("{0}> ", "File/Folder");
                string input = Console.ReadLine();

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
                                Sources.Add(new SourceFile(file));
                            }
                            break;

                        case var i when File.Exists(i):
                            IsInputValid = true;
                            Sources.Add(new SourceFile(input));
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

        static void ManageSource(SourceFile source)
        {
            Dictionary<string, Func<bool>> options = new Dictionary<string, Func<bool>>
            {
                {"Edit", () => {
                    ModifySource(source);
                    return false;
                }
                    },
                { "Position", () => {
                    EditPosition(source);
                    return false;
                }
                },
                { "Delete", () => {
                    Sources.Remove(source);
                    return true;
                }
                },
            };

            new ListMenu<string>(options.Keys.ToList())
            {
                DisplayTitle = (List<string> Options) => Console.WriteLine("ExeToBat > Main > Files > {0}", Path.GetFileName(source.File)),
                DisplayEntry = (List<string> Options, int index, int i) => Console.WriteLine("[{0}] {1}", i, Options[index]),
                HandleEntry = (List<string> Options, int index) =>
                {
                    if (options.ContainsKey(Options[index]))
                    {
                        return options[Options[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },

            }.Show();
        }

        static void ModifySource(SourceFile source)
        {

            // this could be solved better with an enum

            Dictionary<string, Action> options()
            {
                Dictionary<string, Action> opts = new Dictionary<string, Action> {
                    {"File", () => { } },
                    {"Extraction directory", () => EditExtraction(source)},
                    {"Execute after extraction", () => source.Execute = !source.Execute },
                };
                if (source.Execute)
                {
                    opts.Add("Parameters", () => EditParameters(source));
                    opts.Add("Wait for exit", () => source.Wait = !source.Wait);
                }
                if (source.Execute && source.Wait) { opts.Add("Delete after execution", () => source.Delete = !source.Delete); }
                return opts;
            }

            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { "File", "File" },
                { "Extraction directory", "Directory" },
                { "Execute after extraction", "Execute" },
                { "Parameters", "Parameters" },
                { "Wait for exit", "Wait"},
                { "Delete after execution", "Delete"},
            };

            new ListMenu<string>(options().Keys.ToList())
            {
                DisplayTitle = (List<string> Options) => Console.WriteLine("ExeToBat > Main > Files > {0} > Edit", Path.GetFileName(source.File)),
                DisplayEntry = (List<string> Options, int index, int i) =>
                {
                    int MaxLength = options().Keys.Select(x => x.Length).Max();
                    Console.WriteLine("[{0}] {1} | {2}", i, Options[index].PadRight(MaxLength, ' '), source.GetType().GetProperty(ValueMap[Options[index]]).GetValue(source).ToString());
                },
                HandleEntry = (List<string> Options, int index) =>
                {
                    if (options().ContainsKey(Options[index]))
                    {
                        options()[Options[index]]();
                    }
                    else
                    {
                        ResetInput();
                    }

                    return false;
                },
                RefreshEntries = (List<string> Options) => options().Keys.ToList(),

            }.Show();

        }

        static void EditExtraction(SourceFile source)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Clear();
                Console.WriteLine("ExeToBat > Main > Files > {0} > Edit > Extraction", Path.GetFileName(source.File));
                Console.Write("\n");
                Console.WriteLine("Documentation: ");
                Console.WriteLine("https://ss64.com/nt/syntax-variables.html");
                Console.WriteLine("https://ss64.com/nt/syntax-args.html");
                Console.Write("\n");
                Console.Write("{0}> ", "Directory");
                string input = Console.ReadLine();

                input.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    source.Directory = input;
                    IsInputValid = true;
                }
                else
                {
                    IsInputValid = true;
                }
            }
        }

        static void EditParameters(SourceFile source)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Clear();
                Console.WriteLine("ExeToBat > Main > Files > {0} > Edit > Parameters", Path.GetFileName(source.File));
                Console.Write("\n");
                Console.Write("{0}> ", "Parameters");
                string input = Console.ReadLine();

                input.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    source.Parameters = input;
                    IsInputValid = true;
                }
                else
                {
                    IsInputValid = true;
                }
            }
        }

        static void EditPosition(SourceFile source)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Clear();
                Console.WriteLine("ExeToBat > Main > Files > {0} > Position : {1}", Path.GetFileName(source.File), Sources.IndexOf(source));

                Console.Write("\n");
                Console.Write("{0}> ", "New index");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int index))
                {
                    if (index < Sources.Count)
                    {
                        Sources.Remove(source);
                        Sources.Insert(index, source);
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

        static void BuildBat(List<SourceFile> sources, string outputFile)
        {
            Console.Clear();
            Console.WriteLine("ExeToBat > Main > Generate");

            if (Sources.Any())
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    Console.WriteLine("[Preparing] basic batch structure...");
                    writer.WriteLine("@echo off");
                    writer.WriteLine(":: Auto-generated batch file by ExeToBat ::");
                    writer.WriteLine("");

                    foreach (SourceFile source in sources)
                    {
                        Console.WriteLine("[ Reading ] {0}", source.File);
                        List<string> fileChunks = Convert.ToBase64String(File.ReadAllBytes(source.File)).Chunks(chunk).ToList();
                        string tempFile = Path.Combine("%temp%", source.Resource);
                        writer.WriteLine("(");

                        int pos = 0;
                        foreach (string part in fileChunks)
                        {
                            pos++;
                            Console.Write("[ Writing ] {0} part {1}/{2}\r", Path.GetFileName(source.File), pos.ToString().PadLeft(fileChunks.Count.ToString().Length, '0'), fileChunks.Count);
                            writer.WriteLine(string.Format("echo {0}", part));
                        }

                        Console.WriteLine();
                        writer.WriteLine(string.Format(") >> \"{0}\"", tempFile));
                        writer.WriteLine("");

                        Console.WriteLine("[ Writing ] decode mechanism");
                        writer.WriteLine(string.Format("certutil -decode \"{0}\" \"{1}\" >nul 2>&1", tempFile, Path.Combine(source.Directory, Path.GetFileName(source.File))));
                        writer.WriteLine(string.Format("del /f /q \"{0}\" >nul 2>&1", tempFile));
                        writer.WriteLine("");

                        if (source.Execute)
                        {
                            string wait;
                            if (source.Wait) { wait = " /wait"; } else { wait = " "; }
                            Console.WriteLine("[ Writing ] execute mechanism");
                            writer.WriteLine(string.Format("start{0} \"\" \"cmd /c {1}\" {2}", wait, Path.Combine(source.Directory, Path.GetFileName(source.File)), source.Parameters));
                            if (source.Wait)
                            {
                                Console.WriteLine("[ Writing ] wait mechanism");
                                if (source.Delete)
                                {
                                    Console.WriteLine("[ Writing ] delete mechanism");
                                    writer.WriteLine(string.Format("del /f /q \"{0}\" >nul 2>&1", Path.Combine(source.Directory, Path.GetFileName(source.File))));
                                    writer.WriteLine("");
                                }
                            }

                            writer.WriteLine("");

                        }

                        writer.Flush();
                        Console.WriteLine("[Generated] {0}", Path.GetFileName(source.File));

                    }

                    Console.WriteLine("[Generated] All done");

                    Console.WriteLine("Press anything...");
                    Console.ReadKey();

                }
            }
            else
            {
                Console.WriteLine("No files specified");
                Wait(500);

            }


        }

        public class SourceFile
        {
            public string File { get; set; }
            public bool Execute { get; set; } = false;
            public bool Wait { get; set; } = false;
            public bool Delete { get; set; } = false;
            public string Resource { get; set; } = GenTemp();
            public string Directory { get; set; } = "%~dp0";
            public string Parameters { get; set; } = "";


            public SourceFile(string file)
            {
                File = file;
            }

            static public string GenTemp()
            {
                return string.Format($"res_{new Random().Next(1000, 10000)}.b64");
            }

        }

        static string[] Chunks(this string toSplit, int chunkSize)
        {
            int stringLength = toSplit.Length;

            int chunksRequired = (int)Math.Ceiling(stringLength / (decimal)chunkSize);
            var stringArray = new string[chunksRequired];

            int lengthRemaining = stringLength;

            for (int i = 0; i < chunksRequired; i++)
            {
                int lengthToUse = Math.Min(lengthRemaining, chunkSize);
                int startIndex = chunkSize * i;
                stringArray[i] = toSplit.Substring(startIndex, lengthToUse);

                lengthRemaining -= lengthToUse;
            }

            return stringArray;
        }



        public static void Wait(int ms)
        {
            using (System.Threading.ManualResetEvent wait = new System.Threading.ManualResetEvent(false))
            {
                wait.WaitOne(ms);
            }
        }

        public class ListMenu<T>
        {

            /// <summary>
            /// CLOE (Command-line List Options Enumerator).
            /// <para>Turns a List<T> into a menu of options. Each list item is asigned a number. Provides several callbacks to customise the menu.</para>
            /// </summary>
            /// <param name="entries">List of objects you want to display</param>
            public ListMenu(List<T> entries)
            {
                Entries = entries;
            }

            /// <summary>
            /// The prompt that is displayed to the user.
            /// </summary>
            public string Prompt = "Choose";

            /// <summary>
            /// The string to be displayed for the option to exit the menu.
            /// </summary>
            public string ExitEntry = "Back";

            /// <summary>
            /// The key the user has to press to exit the menu.
            /// </summary>
            public char ExitKey = 'q';

            /// <summary>
            /// Wether or not the user can exit the menu.
            /// </summary>
            public bool UserCanExit = true;


            private List<T> Entries;

            /// <summary>
            /// The function that processes the chosen menu entries.
            /// </summary>
            public Func<List<T>, int, bool> HandleEntry = (entries, index) =>
            {
                Console.Clear();
                Console.WriteLine(entries[index]);
                Wait(200);
                return false;
            };

            /// <summary>
            /// The function that displays the menu title.
            /// </summary>
            public Action<List<T>> DisplayTitle = (entries) => { };

            /// <summary>
            /// The function that displays the entry to the user.
            /// </summary>
            public Action<List<T>, int, int> DisplayEntry = (entries, index, num) =>
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(entries.Count).Length, ' '), entries[index]);
            };

            /// <summary>
            /// The function to update the list of entries.
            /// </summary>
            public Func<List<T>, List<T>> RefreshEntries = (entries) =>
            {
                return entries;
            };

            /// <summary>
            /// The function that is called when 0th entry in the list is chosen.
            /// <para>Display this entry with the title function.</para>
            /// </summary>
            public Func<List<T>, bool> ZeroEntry = (entries) =>
            {
                ResetInput();
                return false;
            };

            /// <summary>
            /// Display the menu.
            /// </summary>
            /// <returns></returns>
            public ListMenu<T> Show()
            {
                string readInput = string.Empty;
                bool MenuExitIsPending = false;
                while (!MenuExitIsPending)
                {
                    Console.Clear();
                    int printedEntries = 0;
                    Entries = RefreshEntries(Entries);
                    DisplayTitle(Entries);
                    if (Entries.Any())
                    {
                        int num = 0;
                        foreach (T entry in Entries)
                        {
                            num++;
                            if (string.IsNullOrEmpty(readInput) || Convert.ToString(num).StartsWith(readInput))
                            {
                                DisplayEntry(Entries, Entries.IndexOf(entry), num);
                                printedEntries++;
                            }

                            if (Entries.Count > Console.WindowHeight - 5)
                            {
                                if (printedEntries >= Console.WindowHeight - (5 + 1))
                                {
                                    Console.WriteLine("[{0}] +{1}", ".".PadLeft(Convert.ToString(Entries.Count).Length, '.'), Entries.Count);
                                    break;
                                }
                            }
                            else
                            {
                                if (printedEntries == Console.WindowHeight - 5)
                                {
                                    break;
                                }
                            }

                        }
                    }

                    if (UserCanExit)
                    {
                        Console.WriteLine("[{0}] {1}", Convert.ToString(ExitKey).PadLeft(Convert.ToString(Entries.Count).Length, ' '), ExitEntry);
                    }

                    Console.WriteLine();

                    bool InputIsValid = false;
                    while (!InputIsValid)
                    {
                        Console.Write("{0}> {1}", Prompt, readInput);
                        ConsoleKeyInfo input = Console.ReadKey();
                        Wait(20);
                        int choiceNum = -1;
                        switch (input)
                        {
                            case var key when key.KeyChar.Equals(ExitKey):
                                if (UserCanExit)
                                {
                                    Console.WriteLine();
                                    InputIsValid = true;
                                    MenuExitIsPending = true;
                                }
                                else
                                {
                                    Console.WriteLine();
                                    ResetInput();
                                }
                                break;

                            case var key when key.Key.Equals(ConsoleKey.Backspace):
                                if (!string.IsNullOrEmpty(readInput))
                                {
                                    Console.Write("\b");
                                    readInput = readInput.Remove(readInput.Length - 1);
                                }
                                InputIsValid = true;
                                break;

                            case var key when key.Key.Equals(ConsoleKey.Enter):
                                if (!string.IsNullOrEmpty(readInput))
                                {
                                    if (HandleEntry(Entries, (Convert.ToInt32(readInput) - 1)))
                                    {
                                        MenuExitIsPending = true;
                                    }
                                    readInput = string.Empty;
                                }
                                InputIsValid = true;
                                break;

                            case var key when int.TryParse(key.KeyChar.ToString(), out choiceNum):
                                Console.WriteLine();
                                if (string.IsNullOrEmpty(readInput) && choiceNum.Equals(0))
                                {
                                    InputIsValid = true;
                                    if (ZeroEntry(Entries))
                                    {
                                        MenuExitIsPending = true;
                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(readInput + Convert.ToString(choiceNum)) <= Entries.Count)
                                    {
                                        InputIsValid = true;
                                        int matchingEntries = 0;
                                        readInput = new System.Text.StringBuilder().Append(readInput).Append(Convert.ToString(choiceNum)).ToString();
                                        for (int i = 0; i < Entries.Count; i++)
                                        {
                                            if (Convert.ToString(i + 1).StartsWith(readInput) || Convert.ToString(i + 1) == readInput) { matchingEntries++; }
                                        }
                                        if ((readInput.Length == Convert.ToString(Entries.Count).Length) || (matchingEntries == 1))
                                        {
                                            if (HandleEntry(Entries, (Convert.ToInt32(readInput) - 1)))
                                            {
                                                MenuExitIsPending = true;
                                            }
                                            readInput = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        ResetInput();
                                    }
                                }
                                break;

                            default:
                                Console.WriteLine();
                                ResetInput();
                                break;
                        }
                    }
                }
                return this;
            }
        }

        /// <summary>
        /// A simple template to create Yes or No menus.
        /// </summary>
        /// <param name="title">The title of the menu.</param>
        /// <param name="Yes">The function to be called upon Yes</param>
        /// <param name="No">The function to be called upon No</param>
        public static void YesNoMenu(string title, Action Yes, Action No)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Write("{0}? [{1}]> ", title, "Y/N");
                string Input = Console.ReadKey().KeyChar.ToString();
                Wait(20);
                Console.Write("\n");
                if (string.Equals(Input, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    IsInputValid = true;
                    Yes();

                }
                else if (string.Equals(Input, "N", StringComparison.OrdinalIgnoreCase))
                {
                    IsInputValid = true;
                    No();
                }
                else
                {
                    ResetInput();
                }
            }
        }

        public static void ResetInput(string error = "Input Invalid")
        {
            Console.Write(string.Format("[{0}] {1}", "Error", error));
            Wait(150);
            ClearCurrentConsoleLine();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

    }

}
