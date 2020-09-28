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
            Logger.DebugLine($"Going to import {files.Count} files");
            Logger.DebugLine("");

            foreach (var file in files)
            {
                ImportFile(file);
            }
        }
    }
}
