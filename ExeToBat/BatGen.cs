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

        static void Main(string[] args)
        {
            MainMenu();
        }

        static void MainMenu()
        {
            List<string> options = new List<string> { "Files", "Generate" };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("ExeToBat > Main");
            }

            void DisplayEntry(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool HandleEntry(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals("Files"):
                        ChooseSource();
                        break;

                    case var i when i.Equals("Generate"):
                        BuildBat(Sources, "output.bat");
                        break;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, HandleEntry, DisplayTitle, DisplayEntry, ExitEntry:"Exit");
        }

        static void ChooseSource()
        {
            void DisplayTitle(List<SourceFile> sources)
            {
                Console.WriteLine("ExeToBat > Main > Files");
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(sources.Count).Length, ' '), "Add Files");
            }

            void DisplayEntry(List<SourceFile> sources, SourceFile source, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(sources.Count).Length, ' '), Path.GetFileName(source.File));
            }

            bool ZeroMethod(List<SourceFile> sources)
            {
                AddSource();
                return false;
            }

            bool HandleEntry(List<SourceFile> sources, int index)
            {
                ManageSource(sources[index]);
                return false;
            }

            List<SourceFile> UpdateObjects(List<SourceFile> sources)
            {
                return Sources;
            }

            ListToMenu(Sources, HandleEntry, DisplayTitle, DisplayEntry, ZeroMethod, UpdateObjects);
        }

        static void AddSource()
        {
            string input = "";
            bool IsInputValid = false;
            while (!IsInputValid)
            {

                Console.Clear();
                Console.WriteLine("ExeToBat > Main > Files > Add");
                Console.Write("\n");
                Console.Write("{0}> ", "File/Folder");
                input = Console.ReadLine();

                input.Trim();
                input = input.Replace("\"", "");
                if (!string.IsNullOrEmpty(input))
                {
                    switch (input)
                    {
                        case var i when Directory.Exists(input):
                            IsInputValid = true;
                            foreach (string file in Directory.GetFiles(input))
                            {
                                Sources.Add(new SourceFile(file));
                            }
                            break;

                        case var i when File.Exists(input):
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
            List<string> options = new List<string> { "Edit", "Position", "Delete" };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("ExeToBat > Main > Files > {0}", Path.GetFileName(source.File));
            }

            void DisplayEntry(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool HandleEntry(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals("Edit"):
                        ModifySource(source);
                        break;

                    case var i when i.Equals("Position"):
                        EditPosition(source);
                        break;

                    case var i when i.Equals("Delete"):
                        Sources.Remove(source);
                        return true;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options, HandleEntry, DisplayTitle, DisplayEntry);
        }

        static void ModifySource(SourceFile source)
        {

            List<string> options() {
                List<string> opts = new List<string> { "File", "Extraction directory", "Execute after extraction" };
                if (source.Execute) { opts.AddRange( new List<string> { "Parameters", "Wait for exit" } ); }
                if (source.Wait) { opts.Add("Delete after execution");  }
                return opts;
            }

            Dictionary<string, string> ValueMap = new Dictionary<string, string>
            {
                { "File", "File" },
                { "Execute after extraction", "Execute" },
                { "Wait for exit", "Wait"},
                { "Delete after execution", "Delete"},
                { "Extraction directory", "Directory" },
                { "Parameters", "Parameters" },
            };

            void DisplayTitle(List<string> Options)
            {
                Console.WriteLine("ExeToBat > Main > Files > {0} > Edit", Path.GetFileName(source.File));
            }

            int MaxLength = options().Select(x => x.Length).Max();
            void DisplayEntry(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1} | {2}", i, o.PadRight(MaxLength, ' '), source.GetType().GetProperty(ValueMap[o]).GetValue(source).ToString());
            }

            bool HandleEntry(List<string> Options, int index)
            {
                switch (Options[index])
                {
                    case var i when i.Equals("File"):
                        
                        break;

                    case var i when i.Equals("Execute after extraction"):
                        source.Execute = !source.Execute;
                        break;

                    case var i when i.Equals("Extraction directory"):
                        EditExtraction(source);
                        break;

                    case var i when i.Equals("Parameters"):
                        EditParameters(source);
                        break;

                    case var i when i.Equals("Wait for exit"):
                        source.Wait = !source.Wait;
                        break;

                    case var i when i.Equals("Delete after execution"):
                        source.Delete = !source.Delete;
                        break;

                    default:
                        ResetInput();
                        break;
                }
                return false;
            }

            ListToMenu(options(), HandleEntry, DisplayTitle, DisplayEntry);

        }

        static void EditExtraction(SourceFile source)
        {
            string input = "";
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
                input = Console.ReadLine();

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
            string input = "";
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Clear();
                Console.WriteLine("ExeToBat > Main > Files > {0} > Edit > Parameters", Path.GetFileName(source.File));
                Console.Write("\n");
                Console.Write("{0}> ", "Parameters");
                input = Console.ReadLine();

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
            string input = "";
            int index = -1;
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Clear();
                Console.WriteLine("ExeToBat > Main > Files > {0} > Position : {1}", Path.GetFileName(source.File), Sources.IndexOf(source));

                Console.Write("\n");
                Console.Write("{0}> ", "New index");
                input = Console.ReadLine();

                if (int.TryParse(input, out index))
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

            if(Sources.Any())
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
                new System.Threading.ManualResetEvent(false).WaitOne(500);

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
                return string.Format("{0}{1}{2}", "res_", new Random().Next(1000, 10000), ".b64");
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

                lengthRemaining = lengthRemaining - lengthToUse;
            }

            return stringArray;
        }


        /// <summary>
        /// A function that displays a list as an enumerated menu on the Cli. Items can be chosen and will be processed by passed functions.
        /// <para>Rest in peace, Cloe.</para>
        /// </summary>
        /// <param name="Entries">A list of objects that will be displayed as choices.</param>
        /// <param name="DisplayTitle">The function that displays the title. It should include displaying the 0th entry if you want to use it.</param>
        /// <param name="DisplayEntry">The function that displays an entry. The default function can display strings, for any other objects you will have to pass a custom one.</param>
        /// <param name="HandleEntry">The function that handles the chosen entry.</param>
        /// <param name="ZeroEntry">The 0th entry. It is different from the passed list and can be used for example to create new entries.</param>
        /// <param name="RefreshEntries">Pass a function that will handle updating the list of objects here.</param>
        /// <param name="UserCanAbort">Defines if the user can exit the menu.</param>
        /// <param name="ExitEntry">The string that is displayed for the entry that closes the menu.</param>
        public static void ListToMenu<T>(List<T> Entries, Func<List<T>, int, bool> HandleEntry, Action<List<T>> DisplayTitle = null, Action<List<T>, T, int, int> DisplayEntry = null, Func<List<T>, bool> ZeroEntry = null, Func<List<T>, List<T>> RefreshEntries = null, bool UserCanAbort = true, string ExitEntry = null)
        {

            DisplayTitle = DisplayTitle ?? ((List<T> entries) => { });
            DisplayEntry = DisplayEntry ?? ((List<T> entries, T entry, int index_, int num) => { Console.WriteLine("[{0}] {1}", Convert.ToString(num).PadLeft(Convert.ToString(entries.Count).Length, ' '), entry); });
            RefreshEntries = RefreshEntries ?? ((List<T> entries) => { return entries; });
            ZeroEntry = ZeroEntry ?? ((List<T> entries) => { ResetInput(); return false; });
            ExitEntry = ExitEntry ?? "Back";

            char ExitKey = 'q';
            string Prompt = "Choose";


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
                            DisplayEntry(Entries, entry, Entries.IndexOf(entry), num);
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

                if (UserCanAbort)
                {
                    Console.WriteLine("[{0}] {1}", Convert.ToString(ExitKey).PadLeft(Convert.ToString(Entries.Count).Length, ' '), ExitEntry);
                }

                Console.WriteLine();

                bool InputIsValid = false;
                while (!InputIsValid)
                {
                    Console.Write("{0}> {1}", Prompt, readInput);
                    ConsoleKeyInfo input = Console.ReadKey();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    int choiceNum = -1;
                    switch (input)
                    {
                        case var key when key.KeyChar.Equals(ExitKey):
                            if (UserCanAbort)
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
                                    readInput = readInput + Convert.ToString(choiceNum);
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
        }



        public static void YesNoMenu(string title, Action Yes, Action No)
        {
            bool IsInputValid = false;
            while (!IsInputValid)
            {
                Console.Write("{0}? [{1}]> ", title, "Y/N");
                string Input = Console.ReadKey().KeyChar.ToString();
                new System.Threading.ManualResetEvent(false).WaitOne(20);
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
            new System.Threading.ManualResetEvent(false).WaitOne(150);
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
