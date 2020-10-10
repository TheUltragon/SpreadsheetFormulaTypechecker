using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ANTLR_Test.Classes;

namespace ANTLR_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetActive(true);
            //Logger.SetOutputFile("Log.txt");
            Logger.SetMinDebugLevelToConsole(0);

            //Import();
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
                Logger.DebugLine("====================================================", 1);
                Logger.DebugLine($"Parsing file {file}", 1);
                Logger.DebugLine("====================================================", 1);
                StreamReader reader = File.OpenText(file);
                ErrorHandler handler = new ErrorHandler();
                AntlrInputStream inputStream = new AntlrInputStream(reader);
                SpreadsheetLexer spreadsheetLexer = new SpreadsheetLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
                SpreadsheetParser spreadsheetParser = new SpreadsheetParser(commonTokenStream);

                SpreadsheetParser.SpreadSheetContext context = spreadsheetParser.spreadSheet();
                SpreadsheetVisitor visitor = new TypecheckVisitor(handler);
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                Logger.DebugLine(reader.ReadToEnd(), 1);
                Logger.DebugLine("", 1);
                PrintContext(context, 1, 0);
                Logger.DebugLine("", 1);
                Logger.DebugLine("Parsing has returned result: " + visitor.Visit(context), 1);
                Logger.DebugLine("", 1);
                foreach (var value in visitor.Repository.CellTypes)
                {
                    Logger.DebugLine($"{value.Key.Item1}, {value.Key.Item2}: {value.Value.Type.ToString()}", 1);
                }
                Console.ReadLine();
                Logger.DebugLine("", 1);
            }
        }

        static void PrintContext(IParseTree context, int debugLevel, int depth)
        {
            string prefix = "";
            for(int i = 0; i<depth; i++)
            {
                prefix += " ";
            }

            Logger.DebugLine(prefix + context.GetText(), debugLevel);
            for(int i = 0; i<context.ChildCount; i++)
            {
                var child = context.GetChild(i);
                PrintContext(child, debugLevel, depth + 1);
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
