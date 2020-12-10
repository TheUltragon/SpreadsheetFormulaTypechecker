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
using System.Diagnostics;

namespace ANTLR_Test
{
    public enum Testrun
    {
        Positive,
        Error,
        Import,
        ImportTest,
        HandmadeImport,
        HandmadeTestrun,
        All
    }
    class Program
    {
        public static int LastCells;
        public static int LastFormulas;
        public static int LastErrors;
        public static int LastErrorInvocations;

        public static string corpusPath => @"C:\Users\Niklas Kolbe\OneDrive\Dropbox\StudFiles\Bachelorarbeit\Static Analysis of Spreadsheets\Literatur\Sekundäre Literatur\EUSES\spreadsheets";
        public static string corpusPath2 => @"C:\Users\Admin\OneDrive\Dropbox\StudFiles\Bachelorarbeit\Static Analysis of Spreadsheets\Literatur\Sekundäre Literatur\EUSES\spreadsheets";
        public static string corpusPath3 => @"C:\Users\Win10\OneDrive\Dropbox\StudFiles\Bachelorarbeit\Static Analysis of Spreadsheets\Literatur\Sekundäre Literatur\EUSES\spreadsheets";

        static void Main(string[] args)
        {
            Testrun testing = Testrun.HandmadeImport;
            Logger.SetActive(true);
            //Logger.SetOutputFile("Data\\Log.txt");
            Logger.SetMinDebugLevelToConsole(7);

            Logger.DebugLine($"Program has started. Testrun: {testing.ToString()}", 10);

            if (GlobalSettings.ClearImportsAtStart)
            {
                var path = Path.Combine("Data", "Imports");
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }

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
            else if (testing == Testrun.HandmadeTestrun)
            {
                HandmadeTestrun();
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

        private static void HandmadeTestrun()
        {
            ExcelInteropImporter excelInteropImporter = new ExcelInteropImporter();

            string remainingOutput = "";
            //string input = Path.Combine("Data", "Handmade Examples");
            string input = corpusPath;
            string input2 = corpusPath2;
            string input3 = corpusPath3;
            string outputPath = Path.Combine("Data", "Imports");
            string logPath = Path.Combine("Persistent", "Logs");
            string logFilePath = Path.Combine(logPath, $"importLog_{DateTime.Now.ToFileFormatString()}.txt");
            string testrunCounterPath = Path.Combine("Persistent", "TestrunCounter.txt");
            //files.AddRange(Directory.EnumerateFiles(input));

            //Init File list
            List<string> files = new List<string>();
            files.AddRange(ListFilesRecursively(input));
            files.AddRange(ListFilesRecursively(input2));
            files.AddRange(ListFilesRecursively(input3));

            //Set Counter to previous testrun if there was one
            int testrunCounter = getTestrunCounter(testrunCounterPath);
            File.WriteAllText(testrunCounterPath, "" + testrunCounter);

            //Create Logfile and Path
            Directory.CreateDirectory(logPath);
            File.WriteAllText(logFilePath, "");

            for (int i = testrunCounter; i<files.Count; i++)
            {
                string file = files[i];
                if (Path.GetExtension(file).Equals(".txt"))
                {
                    continue;
                }
                Logger.DebugLine($"Handmade Testrun file {file} start.", 10);
                Stopwatch stopwatchImport = Stopwatch.StartNew();
                string output = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file));
                Directory.CreateDirectory(output);
                excelInteropImporter.ImportFile(file, output);

                stopwatchImport.Stop();
                Stopwatch stopwatchTypecheck = Stopwatch.StartNew();

                int cellsTotal = 0;
                int formulasTotal = 0;
                int errorsTotal = 0;
                int errorInvocationsTotal = 0;

                List<string> outputFiles = new List<string>();
                outputFiles.AddRange(Directory.EnumerateFiles(output));
                foreach (var outputFile in outputFiles)
                {
                    TypecheckFile(outputFile);

                    cellsTotal += LastCells;
                    formulasTotal += LastFormulas;
                    errorsTotal += LastErrors;
                    errorInvocationsTotal += LastErrorInvocations;
                }
                stopwatchTypecheck.Stop();
                Logger.DebugLine($"Handmade Testrun file {file} finish.", 10);

                long typecheckTime = stopwatchTypecheck.ElapsedMilliseconds;
                long importTime = stopwatchImport.ElapsedMilliseconds;
                long totalTime = typecheckTime + importTime;
                double importPercent = (double)importTime / (double)totalTime;
                long fileSize = (long)Math.Round(new FileInfo(file).Length / 1024.0);
                string importOutput = $"========= {Path.GetFileName(file)} =========\n";
                importOutput += $"Filesize (KB): {fileSize}\n";
                importOutput += $"\n";
                importOutput += $"Cells Total: {cellsTotal}\n";
                importOutput += $"Formulas Total: {formulasTotal}\n";
                importOutput += $"Formulas Percent: {(double)formulasTotal / (double)cellsTotal}\n";
                importOutput += $"\n";
                importOutput += $"Errors Total: {errorsTotal}\n";
                importOutput += $"Error Invocations Total: {errorInvocationsTotal}\n";
                importOutput += $"\n";
                importOutput += $"Typecheck time: {typecheckTime}\n";
                importOutput += $"Import time: {importTime}\n";
                importOutput += $"Total time: {totalTime}\n";
                importOutput += $"Import time Percent: {importPercent}\n";
                importOutput += "===========================================\n";
                importOutput += "\n";
                importOutput += $"\n";
                Logger.DebugLine(importOutput, 10);
                remainingOutput += importOutput;

                File.WriteAllText(testrunCounterPath, "" + i);

                try
                {
                    File.AppendAllText(logFilePath, remainingOutput);
                    remainingOutput = "";
                }
                catch(Exception e)
                {
                    Logger.DebugLine("Exception while trying to save Log output",10);
                }
            }
        }

        private static int getTestrunCounter(string testrunCounterPath)
        {
            if (File.Exists(testrunCounterPath) && !GlobalSettings.ResetTestrunCounter)
            {
                string content = File.ReadAllText(testrunCounterPath);
                if(int.TryParse(content, out int result))
                {
                    return result;
                }
            }

            return 0;
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
                TypecheckFile(file);
            }
        }

        static void TypecheckFile(string file)
        {
            var ext = Path.GetExtension(file);
            if (ext != ".xl")
            {
                Logger.DebugLine("====================================================", 10);
                Logger.DebugLine($"Skipping file {file}, invalid extension!", 10);
                Logger.DebugLine("====================================================", 10);
                return;
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
            if (spreadsheetParser.NumberOfSyntaxErrors > 0)
            {
                Logger.DebugLine("===================================", 10);
                Logger.DebugLine("Found Syntax Error - Dont processing file", 10);
                if (GlobalSettings.TypecheckerStopSyntaxError)
                {
                    Logger.DebugLine("Enter to continue", 10);
                    Console.ReadLine();
                }
                Logger.DebugLine("===================================", 10);
                return;
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
                Logger.DebugLine($"Count Cells: {visitor.Repository.CellTypes.Count}", 7);
                Logger.DebugLine($"Count Formulas: {visitor.Repository.Formulas.CellFormulas.Count}", 7);
                Logger.DebugLine("===================================", 7);
                Logger.DebugLine("", 5);
                Logger.DebugLine("CellTypes:", 5);
                foreach (var value in visitor.Repository.CellTypes)
                {
                    Logger.DebugLine($"{value.Key.Item1}, {value.Key.Item2}: {value.Value.Type.ToString()}", 5);
                }
                Logger.DebugLine("", 5);
                Logger.DebugLine("CellFormulas:", 5);
                foreach (var formula in visitor.Repository.Formulas.CellFormulas)
                {
                    Logger.Debug($"{formula.Key.Item1}, {formula.Key.Item2}: {formula.Value.ToString()}", 5);
                    formula.Value.Simplify();
                    Logger.DebugLine($" - Simplified: {formula.Value.ToString()}", 5);

                }
                Logger.DebugLine("===================================", 5);
            }
            Logger.DebugLine($"Detected Errors: {handler.fileData.Errors} (Invocations: {handler.fileData.ErrorInvocations})", 10);

            LastCells = visitor.Repository.CellTypes.Count;
            LastFormulas = visitor.Repository.Formulas.CellFormulas.Count;
            LastErrors = handler.fileData.Errors;
            LastErrorInvocations = handler.fileData.ErrorInvocations;
            Logger.DebugLine("", 10);
        }

        static void PrintContext(IParseTree context, int debugLevel, int depth)
        {
            if(debugLevel < Logger.MinLevelToConsole)
            {
                return;
            }
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
                files.AddRange(ListFilesRecursively(corpusPath));
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
            if (errorListener.HasError)
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
        public bool HasError = false;

        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] int offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            Logger.DebugLine($"line {line}:{charPositionInLine} - {msg}",4);
            HasError = true;
        }
    }
}
