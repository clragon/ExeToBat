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

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool ChoiceMethod(List<string> Options, int index)
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

            ListToMenu(options, DisplayTitle, DisplayOption, ChoiceMethod, BackString:"Exit");
        }

        static void ChooseSource()
        {
            void DisplayTitle(List<SourceFile> sources)
            {
                Console.WriteLine("ExeToBat > Main > Files");
                Console.WriteLine("[{0}] ({1})", Convert.ToString(0).PadLeft(Convert.ToString(sources.Count).Length, ' '), "Add Files");
            }

            void DisplayOption(List<SourceFile> sources, SourceFile source, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", Convert.ToString(i).PadLeft(Convert.ToString(sources.Count).Length, ' '), Path.GetFileName(source.File));
            }

            bool ZeroMethod()
            {
                AddSource();
                return false;
            }

            bool ChoiceMethod(List<SourceFile> sources, int index)
            {
                ManageSource(sources[index]);
                return false;
            }

            List<SourceFile> UpdateObjects(List<SourceFile> sources)
            {
                return Sources;
            }

            ListToMenu(Sources, DisplayTitle, DisplayOption, ChoiceMethod, ZeroMethod, UpdateObjects);
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

            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1}", i, o);
            }

            bool ChoiceMethod(List<string> Options, int index)
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

            ListToMenu(options, DisplayTitle, DisplayOption, ChoiceMethod);
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
            void DisplayOption(List<string> Options, string o, int index, int i)
            {
                Console.WriteLine("[{0}] {1} | {2}", i, o.PadRight(MaxLength, ' '), source.GetType().GetProperty(ValueMap[o]).GetValue(source).ToString());
            }

            bool ZeroMethod() { ResetInput(); return false; }

            bool ChoiceMethod(List<string> Options, int index)
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

            List<string> UpdateObjects(List<string> Options)
            {
                return options();
            }

            ListToMenu(options(), DisplayTitle, DisplayOption, ChoiceMethod, ZeroMethod:ZeroMethod, UpdateObjects:UpdateObjects);

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

                        writer.FlushAsync();
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


        public static void ListToMenu<T>(List<T> Objects, Action<List<T>> DisplayTitle, Action<List<T>, T, int, int> DisplayOptions, Func<List<T>, int, bool> ChoiceMethod, bool UserCanAbort = true, string BackString = "Back")
        {
            ListToMenu<T>(Objects, DisplayTitle, DisplayOptions, ChoiceMethod, () => { ResetInput(); return false; }, (List<T> List) => { return List; }, UserCanAbort, BackString);
        }

        public static void ListToMenu<T>(List<T> Objects, Action<List<T>> DisplayTitle, Action<List<T>, T, int, int> DisplayOptions, Func<List<T>, int, bool> ChoiceMethod, Func<bool> ZeroMethod, Func<List<T>, List<T>> UpdateObjects, bool UserCanAbort = true, string BackString = "Back")
        {
            int index = -1;
            string InputString = "";
            bool IsMenuExitPending = false;
            while (!IsMenuExitPending)
            {
                Console.Clear();
                int printedEntries = 0;
                Objects = UpdateObjects(Objects);
                DisplayTitle(Objects);
                if (Objects.Any())
                {
                    int i = 0;
                    foreach (T x in Objects)
                    {
                        i++;
                        if (InputString == "")
                        {
                            DisplayOptions(Objects, x, i - 1, i);
                            printedEntries++;
                        }
                        else
                        {
                            if (Convert.ToString(i).StartsWith(InputString) || Convert.ToString(i) == InputString)
                            {
                                DisplayOptions(Objects, x, i - 1, i);
                                printedEntries++;
                            }
                        }

                        if (Objects.Count > Console.WindowHeight - 5)
                        {
                            if (printedEntries == Console.WindowHeight - 6)
                            {
                                Console.WriteLine("[{0}]", ".".PadLeft(Convert.ToString(Objects.Count).Length, '.'));
                                break;
                            }
                        }
                        else { if (printedEntries == Console.WindowHeight - 5) { break; } }
                    }
                }

                if (UserCanAbort)
                {
                    Console.WriteLine("[{0}] {1}", "q".PadLeft(Convert.ToString(Objects.Count).Length, ' '), BackString);
                }
                Console.Write("\n");

                bool IsInputValid = false;
                while (!IsInputValid)
                {
                    Console.Write("{0}> {1}", "", InputString);
                    string input = Console.ReadKey().KeyChar.ToString();
                    new System.Threading.ManualResetEvent(false).WaitOne(20);
                    switch (input)
                    {
                        case "q":
                            if (UserCanAbort)
                            {
                                Console.Write("\n");
                                IsInputValid = true;
                                IsMenuExitPending = true;
                            }
                            else
                            {
                                Console.Write("\n");
                                ResetInput();
                            }
                            break;

                        case "\b":
                            if (!(InputString == ""))
                            {
                                Console.Write("\b");
                                InputString = InputString.Remove(InputString.Length - 1);
                            }
                            IsInputValid = true;
                            break;

                        case "\n":
                        case "\r":
                            if (InputString != "")
                            {
                                index = Convert.ToInt32(InputString) - 1;
                                InputString = "";
                                IsInputValid = true;
                                if (ChoiceMethod(Objects, index))
                                {
                                    IsMenuExitPending = true;
                                }
                            }
                            break;

                        default:
                            Console.Write("\n");
                            int choice;
                            if ((int.TryParse(input, out choice)))
                            {
                                if ((InputString == "") && (choice == 0))
                                {
                                    IsInputValid = true;
                                    if (ZeroMethod())
                                    {
                                        IsMenuExitPending = true;
                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(InputString + Convert.ToString(choice)) <= Objects.Count)
                                    {
                                        int MatchingItems = 0;
                                        InputString = InputString + Convert.ToString(choice);
                                        for (int i = 0; i < Objects.Count; i++) { if (Convert.ToString(i + 1).StartsWith(InputString) || Convert.ToString(i + 1) == InputString) { MatchingItems++; } }
                                        if ((InputString.Length == Convert.ToString(Objects.Count).Length) || (MatchingItems == 1))
                                        {
                                            index = Convert.ToInt32(InputString) - 1;
                                            InputString = "";
                                            IsInputValid = true;
                                            if (ChoiceMethod(Objects, index))
                                            {
                                                IsMenuExitPending = true;
                                            }
                                        }
                                        else
                                        {
                                            IsInputValid = true;
                                        }
                                    }
                                    else
                                    {
                                        ResetInput();
                                    }
                                }
                            }
                            else
                            {
                                ResetInput();
                            }
                            break;
                    }
                }
            }
            Console.Clear();
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
