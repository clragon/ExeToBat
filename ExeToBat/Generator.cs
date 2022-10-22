using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ExeToBat
{
    public class Generator
    {
        public Generator() { }

        public Generator(GeneratorConfig config)
        {
            LoadConfig(config);
        }

        public const int ChunkSize = 8000;

        public List<SourceFile> Sources = new List<SourceFile>();

        public class SourceFile
        {
            /// <summary>
            /// Represents a file that is later embedded in a bat.
            /// </summary>
            /// <param name="path"></param> The path of the file.
            public SourceFile(string path)
            {
                Path = path;
            }

            public string Path { get; set; }
            public bool Execute { get; set; } = false;
            public bool Wait { get; set; } = false;
            public bool Delete { get; set; } = false;
            public string Resource { get; set; } = GetTempFileName();
            public string Directory { get; set; } = "%~dp0";
            public string Parameters { get; set; } = "";

            static public string GetTempFileName()
            {
                return string.Format($"res_{new Random().Next(1000, 10000)}.b64");
            }
        }

        public class GeneratorConfig
        {
            /// <summary>
            /// The configuration for a Generator.
            /// </summary>
            /// <param name="sources"></param>
            public GeneratorConfig(List<SourceFile> sources)
            {
                Sources = sources;
            }

            public List<SourceFile> Sources { get; private set; }

            public string ToJson()
            {
                return JsonSerializer.Serialize(this);
            }

            public static GeneratorConfig FromJson(string raw)
            {
                return JsonSerializer.Deserialize<GeneratorConfig>(raw);
            }
        }

        /// <summary>
        /// Exports the variables of this Generator as a configuration.
        /// </summary>
        /// <returns></returns>
        public GeneratorConfig SaveConfig()
        {
            return new GeneratorConfig(Sources);
        }

        /// <summary>
        /// Loads a configuration into this Generator.
        /// </summary>
        /// <param name="config"></param>
        public void LoadConfig(GeneratorConfig config)
        {
            Sources = config.Sources;
        }

        /// <summary>
        /// Sends progress updates about ongoing generation task.
        /// </summary>
        public event EventHandler<GeneratorEvent> Generation;

        protected virtual void OnGeneration(GeneratorEvent e)
        {
            EventHandler<GeneratorEvent> handler = Generation;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Generates a batch file with all specified source files.
        /// </summary>
        /// <param name="outputFile"></param> Target output file path.
        public void Generate(string outputFile)
        {
            if (Sources.Any())
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    OnGeneration(new GenerationStartEvent(Sources));
                    writer.WriteLine("@echo off");
                    writer.WriteLine(":: Auto-generated batch file by ExeToBat ::");
                    writer.WriteLine("");

                    foreach (SourceFile source in Sources)
                    {
                        OnGeneration(new ReadingFileEvent(source));
                        List<string> fileChunks = Convert
                            .ToBase64String(File.ReadAllBytes(source.Path))
                            .Chunks(ChunkSize)
                            .ToList();
                        string tempFile = Path.Combine("%temp%", source.Resource);
                        writer.WriteLine("(");

                        int pos = 0;
                        foreach (string part in fileChunks)
                        {
                            pos++;
                            OnGeneration(new WritingFilePartEvent(source, pos, fileChunks.Count));
                            writer.WriteLine(string.Format("echo {0}", part));
                        }

                        writer.WriteLine(string.Format(") >> \"{0}\"", tempFile));
                        writer.WriteLine("");

                        OnGeneration(new WritingFileDecoderEvent(source));
                        writer.WriteLine(
                            string.Format(
                                "certutil -decode \"{0}\" \"{1}\" >nul 2>&1",
                                tempFile,
                                Path.Combine(source.Directory, Path.GetFileName(source.Path))
                            )
                        );
                        writer.WriteLine(string.Format("del /f /q \"{0}\" >nul 2>&1", tempFile));
                        writer.WriteLine("");

                        if (source.Execute)
                        {
                            string wait;
                            if (source.Wait)
                            {
                                wait = " /wait";
                            }
                            else
                            {
                                wait = " ";
                            }

                            OnGeneration(new WritingFileExecuteEvent(source));
                            writer.WriteLine(
                                string.Format(
                                    "start{0} \"\" \"cmd /c {1}\" {2}",
                                    wait,
                                    Path.Combine(source.Directory, Path.GetFileName(source.Path)),
                                    source.Parameters
                                )
                            );
                            if (source.Wait)
                            {
                                OnGeneration(new WritingFileWaitEvent(source));
                                if (source.Delete)
                                {
                                    OnGeneration(new WritingFileDeleteEvent(source));
                                    writer.WriteLine(
                                        string.Format(
                                            "del /f /q \"{0}\" >nul 2>&1",
                                            Path.Combine(
                                                source.Directory,
                                                Path.GetFileName(source.Path)
                                            )
                                        )
                                    );
                                    writer.WriteLine("");
                                }
                            }

                            writer.WriteLine("");
                        }

                        writer.Flush();
                        OnGeneration(new WritingFileCompleteEvent(source));
                    }

                    OnGeneration(new GenerationCompleteEvent(outputFile));
                }
            }
            else
            {
                OnGeneration(new GenerationEmptyEvent());
            }
        }

        public abstract class GeneratorEvent : EventArgs { }

        public abstract class GeneratorFileEvent : GeneratorEvent
        {
            public SourceFile File { get; protected set; }
        }

        public class GenerationStartEvent : GeneratorEvent
        {
            public GenerationStartEvent(List<SourceFile> files)
            {
                Files = files;
            }

            public List<SourceFile> Files { get; private set; }
        }

        public class ReadingFileEvent : GeneratorFileEvent
        {
            public ReadingFileEvent(SourceFile file)
            {
                File = file;
            }
        }

        public class WritingFilePartEvent : GeneratorFileEvent
        {
            public WritingFilePartEvent(SourceFile file, int part, int max)
            {
                File = file;
                Part = part;
                Max = max;
            }

            public int Part { get; private set; }
            public int Max { get; private set; }
        }

        public class WritingFileDecoderEvent : GeneratorFileEvent
        {
            public WritingFileDecoderEvent(SourceFile file)
            {
                File = file;
            }
        }

        public class WritingFileExecuteEvent : GeneratorFileEvent
        {
            public WritingFileExecuteEvent(SourceFile file)
            {
                File = file;
            }
        }

        public class WritingFileWaitEvent : GeneratorFileEvent
        {
            public WritingFileWaitEvent(SourceFile file)
            {
                File = file;
            }
        }

        public class WritingFileDeleteEvent : GeneratorFileEvent
        {
            public WritingFileDeleteEvent(SourceFile file)
            {
                File = file;
            }
        }

        public class WritingFileCompleteEvent : GeneratorFileEvent
        {
            public WritingFileCompleteEvent(SourceFile file)
            {
                File = file;
            }
        }

        public class GenerationCompleteEvent : GeneratorEvent
        {
            public GenerationCompleteEvent(string path)
            {
                Path = path;
            }

            public string Path { get; private set; }
        }

        public class GenerationEmptyEvent : GeneratorEvent
        {
            public GenerationEmptyEvent() { }
        }

        public class GenerationFailedEvent : GeneratorEvent
        {
            public GenerationFailedEvent(Exception error)
            {
                Error = error;
            }

            public Exception Error { get; private set; }
        }
    }

    internal static class StringChunks
    {
        public static string[] Chunks(this string toSplit, int chunkSize)
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
    }
}
