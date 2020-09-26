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
        private static StreamWriter FileOut;

        public static void SetActive(bool active)
        {
            Active = active;
        }

        public static void SetOutputFile(string path)
        {
            OutputFilePath = path;
            File.WriteAllText(path, "");
            //FileOut = new StreamWriter(path);
        }

        public static void Debug(string text)
        {
            if (Active)
            {
                Console.WriteLine(text);
                if (!string.IsNullOrEmpty(OutputFilePath))
                {
                    File.AppendAllText(OutputFilePath, text + "\n");
                }
            }
        }
    }
}
