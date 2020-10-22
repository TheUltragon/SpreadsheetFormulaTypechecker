using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public abstract class SpreadSheetImporter
    {
        public abstract void ImportFile(string path);

        public  void ImportFiles()
        {
            List<string> files = new List<string>();
            files = files.Concat(Directory.EnumerateFiles("Data\\Corpus")).ToList();
            Logger.DebugLine($"Going to import {files.Count} files", 1);
            Logger.DebugLine("");

            foreach (var file in files)
            {
                Console.WriteLine($"Enter to continue with file {file}");
                Console.ReadLine();
                Logger.DebugLine($"Importing File {file}", 1);
                ImportFile(file);
                Logger.DebugLine($"Finished Importing File {file}", 1);
                Logger.DebugLine("===================================", 10);
                Logger.DebugLine("Enter to continue", 10);
                Console.ReadLine();
                Logger.DebugLine("===================================", 10);
                Logger.SaveToFile();

            }
        }
    }
}
