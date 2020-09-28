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
            //Logger.SetOutputFile("Log.txt");

            Import();
            TestSuite();
        }

        static void TestSuite()
        {
            List<string> files = new List<string>();
            files = files.Concat(Directory.EnumerateFiles("Testsuite\\Good")).ToList();
            files = files.Concat(Directory.EnumerateFiles("Testsuite\\Bad")).ToList();
            Logger.DebugLine($"Going to parse {files.Count} files");
            Logger.DebugLine("");

            foreach (var file in files)
            {
                Logger.DebugLine("====================================================");
                Logger.DebugLine($"Parsing file {file}");
                Logger.DebugLine("====================================================");
                StreamReader reader = File.OpenText(file);
                ErrorHandler handler = new ErrorHandler();
                AntlrInputStream inputStream = new AntlrInputStream(reader);
                SpreadsheetLexer spreadsheetLexer = new SpreadsheetLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
                SpreadsheetParser spreadsheetParser = new SpreadsheetParser(commonTokenStream);

                SpreadsheetParser.SpreadSheetContext context = spreadsheetParser.spreadSheet();
                SpreadsheetVisitor visitor = new TypecheckVisitor(handler);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                Logger.DebugLine(reader.ReadToEnd());
                Logger.DebugLine("");
                Logger.DebugLine("Parsing has returned result: " + visitor.Visit(context));
                Logger.DebugLine("");
                foreach (var value in visitor.Repository.CellTypes)
                {
                    Logger.DebugLine($"{value.Key.Item1}, {value.Key.Item2}: {value.Value.Type.ToString()}");
                }
                Console.ReadLine();
                Logger.DebugLine("");
            }
        }

        static void Import()
        {
            ExcelReaderImporter excelReaderImporter = new ExcelReaderImporter();
            LinqToExcelImporter linqToExcelImporter = new LinqToExcelImporter();
            ExcelInteropImporter excelInteropImporter = new ExcelInteropImporter();

            //excelReaderImporter.ImportFiles();
            //linqToExcelImporter.ImportFiles();
            excelInteropImporter.ImportFiles();
        }
    }
}
