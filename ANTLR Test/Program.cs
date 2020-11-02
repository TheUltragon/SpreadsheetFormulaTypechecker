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
    public enum Testrun
    {
        Positive,
        Error,
        Import,
        All
    }
    class Program
    {
        static void Main(string[] args)
        {
            Testrun testing = Testrun.Positive;
            Logger.SetActive(true);
            Logger.SetOutputFile("Data\\Log.txt");
            Logger.SetMinDebugLevelToConsole(1);

            Logger.DebugLine($"Program has started. Testrun: {testing.ToString()}", 10);

            //Main Part for the Program
            if (testing == Testrun.All)
            {
                Import();
                TestSuite(testing);
            }
            else if (testing == Testrun.Import)
            {
                Import();
                TestSuite(testing);
            }
            else if(testing == Testrun.Error)
            {
                TestSuite(testing);
            }
            else if (testing == Testrun.Positive)
            {
                TestSuite(testing);
            }


            Logger.SaveToFile();
            Logger.DebugLine("Program has ended. Press Enter to close it", 10);
            Console.ReadLine();
        }

        static List<string> ListFilesRecursively(string sDir)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    var newFiles = ListFilesRecursively(d);
                    files.AddRange(newFiles);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return files;
        }

        static void TestSuite(Testrun testing)
        {
            List<string> files = new List<string>();
            if(testing == Testrun.Positive)
            {
                files.AddRange(ListFilesRecursively("Testsuite\\Good"));
            }
            else if(testing == Testrun.Error)
            {
                files.AddRange(ListFilesRecursively("Testsuite\\Bad"));
            }
            if (testing == Testrun.Import)
            {
                files.AddRange(ListFilesRecursively("Data\\Imports"));
            }
            else if (testing == Testrun.All)
            {
                files.AddRange(ListFilesRecursively("Testsuite\\Good"));
                files.AddRange(ListFilesRecursively("Testsuite\\Bad"));
                files.AddRange(ListFilesRecursively("Data\\Imports"));
            }
            Logger.DebugLine($"Going to parse {files.Count} files");
            Logger.DebugLine("");

            foreach (var file in files)
            {

                var ext = Path.GetExtension(file);
                if(ext != ".xl")
                {
                    Logger.DebugLine("====================================================", 10);
                    Logger.DebugLine($"Skipping file {file}, invalid extension!", 10);
                    Logger.DebugLine("====================================================", 10);
                    continue;
                }

                Logger.DebugLine("====================================================", 10);
                Logger.DebugLine($"Going to parse file {file}", 10);
                Logger.DebugLine("Enter to continue", 10);
                Console.ReadLine();
                Logger.DebugLine("====================================================", 10);
                StreamReader reader = File.OpenText(file);
                ErrorHandler handler = new ErrorHandler();
                AntlrInputStream inputStream = new AntlrInputStream(reader);
                SpreadsheetLexer spreadsheetLexer = new SpreadsheetLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
                SpreadsheetParser spreadsheetParser = new SpreadsheetParser(commonTokenStream);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                Logger.DebugLine(reader.ReadToEnd(), 1);
                Logger.DebugLine("====================================================", 10);
                reader.BaseStream.Seek(0, SeekOrigin.Begin);


                SpreadsheetParser.SpreadSheetContext context = spreadsheetParser.spreadSheet();
                if(spreadsheetParser.NumberOfSyntaxErrors > 0)
                {
                    Logger.DebugLine("===================================", 10);
                    Logger.DebugLine("Found Syntax Error - Dont processing file", 10);
                    Logger.DebugLine("Enter to continue", 10);
                    Console.ReadLine();
                    Logger.DebugLine("===================================", 10);
                    continue;
                }

                SpreadsheetVisitor visitor = new TypecheckVisitor(handler);
                
                Logger.DebugLine("", 10);
                PrintContext(context, 0, 0);
                Logger.DebugLine("", 0);
                bool result = visitor.Visit(context);
                Logger.DebugLine("Parsing has returned result: " + result, 10);
                if (result)
                {
                    Logger.DebugLine("", 5);
                    Logger.DebugLine("", 5);
                    Logger.DebugLine("CellTypes:", 5);
                    foreach (var value in visitor.Repository.CellTypes)
                    {
                        Logger.DebugLine($"{value.Key.Item1}, {value.Key.Item2}: {value.Value.Type.ToString()}", 5);
                    }
                    Logger.DebugLine("", 5);
                    Logger.DebugLine("CellFormulas:", 5);
                    foreach(var formula in visitor.Repository.Formulas.CellFormulas)
                    {
                        Logger.DebugLine($"{formula.Key.Item1}, {formula.Key.Item2}: {formula.Value.ToString()}", 5);
                    }
                }
                
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
