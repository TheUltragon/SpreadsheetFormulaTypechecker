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
        public abstract void ImportFile(string path, string output);

        public  void ImportFiles(List<string> files, string output)
        {
            //List<string> files = new List<string>();
            //files = files.Concat(Directory.EnumerateFiles("Data\\Corpus")).ToList();
            Logger.DebugLine($"Going to import {files.Count} files", 5);
            Logger.DebugLine("");

            foreach (var file in files)
            {
                if (Path.GetExtension(file).Equals(".txt"))
                {
                    Logger.DebugLine("===================================", 10);
                    Console.WriteLine($"Skipping {file}");
                    Logger.DebugLine("===================================", 10);
                }
                else
                {
                    Logger.DebugLine("===================================", 10);
                    Logger.DebugLine($"Going to import file {file}",10);
                    if (GlobalSettings.ImportStopAtNextFile)
                    {
                        Console.WriteLine($"Enter to continue");
                        Console.ReadLine();
                    }
                       
                    Logger.DebugLine($"Importing File {file}", 10);
                    ImportFile(file, output);
                    Logger.DebugLine($"Finished Importing File {file}", 10);
                    Logger.DebugLine("===================================", 10);
                    Logger.SaveToFile();
                }
            }
        }
    }
}
