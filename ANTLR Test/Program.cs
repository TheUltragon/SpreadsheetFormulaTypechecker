using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ANTLR_Test.Classes;

namespace ANTLR_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetActive(true);
            Logger.SetOutputFile("Log.txt");

            Import();
            TestSuite();
        }

        static void TestSuite()
        {
            List<string> files = new List<string>();
            files = files.Concat(Directory.EnumerateFiles("Testsuite\\Good")).ToList();
            files = files.Concat(Directory.EnumerateFiles("Testsuite\\Bad")).ToList();
            Logger.Debug($"Going to parse {files.Count} files");
            Logger.Debug("");

            foreach (var file in files)
            {
                Logger.Debug("====================================================");
                Logger.Debug($"Parsing file {file}");
                Logger.Debug("====================================================");
                StreamReader reader = File.OpenText(file);
                ErrorHandler handler = new ErrorHandler();
                AntlrInputStream inputStream = new AntlrInputStream(reader);
                SpreadsheetLexer spreadsheetLexer = new SpreadsheetLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
                SpreadsheetParser spreadsheetParser = new SpreadsheetParser(commonTokenStream);

                SpreadsheetParser.SpreadSheetContext context = spreadsheetParser.spreadSheet();
                SpreadsheetVisitor visitor = new TypecheckVisitor(handler);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                Logger.Debug(reader.ReadToEnd());
                Logger.Debug("");
                Logger.Debug("Parsing has returned result: " + visitor.Visit(context));
                Logger.Debug("");
                foreach (var value in visitor.Repository.CellTypes)
                {
                    Logger.Debug($"{value.Key.Item1}, {value.Key.Item2}: {value.Value.Type.ToString()}");
                }
                Console.ReadLine();
                Logger.Debug("");
            }
        }

        static void Import()
        {
            List<string> files = new List<string>();
            files = files.Concat(Directory.EnumerateFiles("Data\\Corpus")).ToList();
            Logger.Debug($"Going to import {files.Count} files");
            Logger.Debug("");

            foreach (var file in files)
            {
                SpreadSheetImport.ImportFile(file);
            }
        }
    }
}
