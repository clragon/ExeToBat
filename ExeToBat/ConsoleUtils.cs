using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class ConsoleUtils
    {
        /// <summary>
        /// Pauses the thread for a duration.
        /// </summary>
        /// <param name="ms"></param> The duration to pause.
        public static void Wait(int ms)
        {
            using (Threading.ManualResetEvent wait = new Threading.ManualResetEvent(false))
            {
                wait.WaitOne(ms);
            }
        }

        /// <summary>
        /// Clears the current line of the Console.
        /// </summary>
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        /// <summary>
        /// Deletes the last line of the console and displays an error for a short duration.
        /// </summary>
        /// <param name="error"></param> The error to display.
        public static void ResetInput(string error = "Input Invalid")
        {
            Console.Write(string.Format("[{0}] {1}", "Error", error));
            Wait(150);
            ClearCurrentConsoleLine();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            ClearCurrentConsoleLine();
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

        public class ListMenu<T>
        {
            /// <summary>
            /// Command-line List Options Enumerator
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
                Console.WriteLine(
                    "[{0}] {1}",
                    Convert.ToString(num).PadLeft(Convert.ToString(entries.Count).Length, ' '),
                    entries[index]
                );
            };

            /// <summary>
            /// The function to update the list of entries.
            /// </summary>
            public Func<List<T>, List<T>> RefreshEntries = (entries) => entries;

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
                            if (
                                string.IsNullOrEmpty(readInput)
                                || Convert.ToString(num).StartsWith(readInput)
                            )
                            {
                                DisplayEntry(Entries, Entries.IndexOf(entry), num);
                                printedEntries++;
                            }

                            if (Entries.Count > Console.WindowHeight - 5)
                            {
                                if (printedEntries >= Console.WindowHeight - (5 + 1))
                                {
                                    Console.WriteLine(
                                        "[{0}] +{1}",
                                        ".".PadLeft(Convert.ToString(Entries.Count).Length, '.'),
                                        Entries.Count
                                    );
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
                        Console.WriteLine(
                            "[{0}] {1}",
                            Convert
                                .ToString(ExitKey)
                                .PadLeft(Convert.ToString(Entries.Count).Length, ' '),
                            ExitEntry
                        );
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
                                    if (
                                        Convert.ToInt32(readInput + Convert.ToString(choiceNum))
                                        <= Entries.Count
                                    )
                                    {
                                        InputIsValid = true;
                                        int matchingEntries = 0;
                                        readInput = new System.Text.StringBuilder()
                                            .Append(readInput)
                                            .Append(Convert.ToString(choiceNum))
                                            .ToString();
                                        for (int i = 0; i < Entries.Count; i++)
                                        {
                                            if (
                                                Convert.ToString(i + 1).StartsWith(readInput)
                                                || Convert.ToString(i + 1) == readInput
                                            )
                                            {
                                                matchingEntries++;
                                            }
                                        }
                                        if (
                                            (
                                                readInput.Length
                                                == Convert.ToString(Entries.Count).Length
                                            ) || (matchingEntries == 1)
                                        )
                                        {
                                            if (
                                                HandleEntry(
                                                    Entries,
                                                    (Convert.ToInt32(readInput) - 1)
                                                )
                                            )
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
    }
}
