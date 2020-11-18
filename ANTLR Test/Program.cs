using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ANTLR_Test.Classes;
using Antlr4.Runtime.Misc;

namespace ANTLR_Test
{
    public enum Testrun
    {
        Positive,
        Error,
        Import,
        ImportTest,
        HandmadeImport,
        All
    }
    class Program
    {
        static void Main(string[] args)
        {
            Testrun testing = Testrun.Positive;
            Logger.SetActive(true);
            //Logger.SetOutputFile("Data\\Log.txt");
            Logger.SetMinDebugLevelToConsole(5);

            Logger.DebugLine($"Program has started. Testrun: {testing.ToString()}", 10);

            //Main Part for the Program
            if (testing == Testrun.All)
            {
                Import(testing);
                TestSuite(testing);
            }
            else if (testing == Testrun.Import)
            {
                Import(testing);
                TestSuite(testing);
            }
            else if (testing == Testrun.ImportTest)
            {
                ImportTest();
                Import(testing);
                TestSuite(testing);
            }
            else if (testing == Testrun.HandmadeImport)
            {
                Import(testing);
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
            else if (testing == Testrun.Import || testing == Testrun.ImportTest || testing == Testrun.HandmadeImport)
            {
                files.AddRange(ListFilesRecursively("Data\\Imports"));
            }
            else if (testing == Testrun.All)
            {
                files.AddRange(ListFilesRecursively("Testsuite\\Good"));
                files.AddRange(ListFilesRecursively("Testsuite\\Bad"));
                files.AddRange(ListFilesRecursively("Data\\Imports"));
            }
            Logger.DebugLine($"Going to parse {files.Count} files", 10);
            Logger.DebugLine("", 10);

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
                if (GlobalSettings.TypecheckerStopAtNextFile)
                {
                    Logger.DebugLine("Enter to continue", 10);
                    Console.ReadLine();
                }
                
                Logger.DebugLine("====================================================", 10);
                StreamReader reader = File.OpenText(file);
                ErrorHandler handler = new ErrorHandler();
                AntlrInputStream inputStream = new AntlrInputStream(reader);
                SpreadsheetLexer spreadsheetLexer = new SpreadsheetLexer(inputStream);
                CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
                SpreadsheetParser spreadsheetParser = new SpreadsheetParser(commonTokenStream);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                Logger.DebugLine(reader.ReadToEnd(), 5);
                Logger.DebugLine("====================================================", 5);
                reader.BaseStream.Seek(0, SeekOrigin.Begin);


                SpreadsheetParser.SpreadSheetContext context = spreadsheetParser.spreadSheet();
                if(spreadsheetParser.NumberOfSyntaxErrors > 0)
                {
                    Logger.DebugLine("===================================", 10);
                    Logger.DebugLine("Found Syntax Error - Dont processing file", 10);
                    if (GlobalSettings.TypecheckerStopSyntaxError)
                    {
                        Logger.DebugLine("Enter to continue", 10);
                        Console.ReadLine();
                    }
                    Logger.DebugLine("===================================", 10);
                    continue;
                }

                SpreadsheetVisitor visitor = new TypecheckVisitor(handler);
                
                Logger.DebugLine("", 0);
                PrintContext(context, 0, 0);
                Logger.DebugLine("", 0);
                bool result = visitor.Visit(context);
                Logger.DebugLine("Basic Typechecking has returned result: " + result, 10);
                Logger.DebugLine("===================================", 10);
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
                    Logger.DebugLine("===================================", 5);
                }
                Logger.DebugLine("", 10);

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

        static void Import(Testrun testing)
        {
            ExcelReaderImporter excelReaderImporter = new ExcelReaderImporter();
            LinqToExcelImporter linqToExcelImporter = new LinqToExcelImporter();
            ExcelInteropImporter excelInteropImporter = new ExcelInteropImporter();

            string output = "Data\\Imports";
            List<string> files = new List<string>();
            if(testing == Testrun.Import)
            {
                files.AddRange(ListFilesRecursively("C:\\Users\\Win10\\OneDrive\\Dropbox\\StudFiles\\Bachelorarbeit\\Static Analysis of Spreadsheets\\Literatur\\Sekundäre Literatur\\EUSES\\spreadsheets"));
            }
            else if(testing == Testrun.ImportTest)
            {
                files.AddRange(Directory.EnumerateFiles("Data\\Corpus"));
            }
            else if (testing == Testrun.HandmadeImport)
            {
                files.AddRange(Directory.EnumerateFiles("Data\\Handmade Examples"));
            }

            //excelReaderImporter.ImportFiles();
            //linqToExcelImporter.ImportFiles();
            excelInteropImporter.ImportFiles(files, output);
        }
        static void ImportTest()
        {
            SyntaxErrorListener errorListener = new SyntaxErrorListener();
            string formula = "=MAX(B6-40;0)";
            AntlrInputStream inputStream = new AntlrInputStream((string)formula);
            ExcelFormulaLexer spreadsheetLexer = new ExcelFormulaLexer(inputStream);
            spreadsheetLexer.AddErrorListener(errorListener);
            CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
            ExcelFormulaParser excelFormulaParser = new ExcelFormulaParser(commonTokenStream);

            ExcelFormulaParser.ExcelExprContext context = excelFormulaParser.excelExpr();
            if (SyntaxErrorListener.HasError)
            {
                Logger.DebugLine($"Found Lexer Error - Dont processing formula {(string)formula}", 10);
                return;
            }
            if (excelFormulaParser.NumberOfSyntaxErrors > 0 )
            {
                Logger.DebugLine($"Found Syntax Error - Dont processing formula {(string)formula}", 10);
                return;
            }
            ExcelFormulaVisitor visitor = new ExcelFormulaVisitor();
            string formulaText = visitor.Visit(context);

            Logger.DebugLine($"FormulaText: {formulaText}", 10);
        }
    }

    public class SyntaxErrorListener : IAntlrErrorListener<int>
    {
        public static bool HasError = false;

        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            HasError = true;
        }
    }
}
