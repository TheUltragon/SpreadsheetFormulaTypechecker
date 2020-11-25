using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ANTLR_Test.Classes
{
    public static class Logger
    {
        private static bool Active = false;
        private static string OutputFilePath = "";
        private static string Output = "";
        private static int MinLevelToConsole = 0;

        public static void SetActive(bool active)
        {
            Active = active;
        }

        public static void SetMinDebugLevelToConsole(int level)
        {
            MinLevelToConsole = level;
        }

        public static void SetOutputFile(string path)
        {
            OutputFilePath = path;
            File.WriteAllText(path, "");
        }

        public static void DebugLine(string text, int level = 0)
        {
            Debug(text + "\n", level);
        }

        public static void Debug(string text, int level = 0)
        {
            if (Active)
            {
                if(MinLevelToConsole <= level)
                {
                    Console.Write(text);
                    if (!string.IsNullOrEmpty(OutputFilePath))
                    {
                        Output += text;
                        if (GlobalSettings.LoggerInstantOut)
                        {
                            SaveToFile();
                        }
                    }
                }
            }
        }

        public static void SaveToFile()
        {
            if (!string.IsNullOrEmpty(OutputFilePath))
            {
                Console.Write("Saving to file...");
                File.AppendAllText(OutputFilePath, Output);
                Console.Write("Done saving");
            }
            Output = "";
        }
    }
}
