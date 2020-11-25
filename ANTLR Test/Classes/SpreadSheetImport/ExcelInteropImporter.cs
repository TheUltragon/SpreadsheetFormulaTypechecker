using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using Antlr4.Runtime;

namespace ANTLR_Test.Classes
{
    class ExcelInteropImporter : SpreadSheetImporter
    {
        string convertedText = "";
        //Mostly copied from: https://www.dotnetperls.com/excel
        public override void ImportFile(string path, string output)
        {
            Application app = new Application();
            try
            {
                Workbook workBook = app.Workbooks.Open(Path.GetFullPath(path));
                handleWorkBook(workBook, path, output);

                workBook.Close(false, path, null);
                Marshal.ReleaseComObject(workBook);
            }
            catch(Exception e)
            {
                Logger.DebugLine($"Exception - Couldnt Import file from workbook at path {path}. Skipping workbook.", 10);
                Logger.DebugLine($"Exception Text: {e.ToString()}", 1);
                Logger.DebugLine($"Exception Stacktrace: {e.StackTrace}", 1);
                if (GlobalSettings.ImportStopAtMiscError)
                {
                    Console.WriteLine($"Enter to continue", 10);
                    Console.ReadLine();
                }
            }
            
        }

        private void handleWorkBook(Workbook workBook, string path, string output)
        {
            int numSheets = workBook.Sheets.Count;
            Logger.DebugLine($"Sheets: {numSheets}", 5);

            //
            // Iterate through the sheets. They are indexed starting at 1.
            //
            for (int sheetNum = 1; sheetNum < numSheets + 1; sheetNum++)
            {
                Logger.DebugLine($"Importing Sheet {sheetNum}", 5);
                Worksheet sheet;
                try
                {
                    sheet = (Worksheet)workBook.Sheets[sheetNum];
                }
                catch (InvalidCastException e)
                {
                    Logger.DebugLine($"Exception - Couldnt cast sheet to worksheet. Skipping worksheet.", 10);
                    Logger.DebugLine($"Exception Text: {e.ToString()}", 1);
                    Logger.DebugLine($"Exception Stacktrace: {e.StackTrace}", 1);
                    if (GlobalSettings.ImportStopAtMiscError)
                    {
                        Console.WriteLine($"Enter to continue", 10);
                        Console.ReadLine();
                    }
                    continue;
                }
                //
                // Take the used range of the sheet. Finally, get an object array of all
                // of the cells in the sheet (their values). You can do things with those
                // values. See notes about compatibility.
                //
                Range excelRange = sheet.UsedRange;
                object[,] valueArray = (object[,])excelRange.get_Value(
                    XlRangeValueDataType.xlRangeValueDefault);
                object[,] formulaArray = new object[1, 1];
                if (excelRange.Formula is string)
                {
                    formulaArray[0, 0] = (string)excelRange.Formula;
                }
                else
                {
                    formulaArray = (object[,])excelRange.Formula;
                }

                //
                // Do something with the data in the array with a custom method.
                //
                if (valueArray != null && formulaArray != null)
                {
                    convertedText = "";

                    ProcessObjects(valueArray, formulaArray);
                    //Add Implicit eval at end of file
                    convertedText += "eval\n";

                    Logger.DebugLine($"=========================================", 10);
                    Logger.DebugLine($"Imported File Workbook {sheetNum}", 10);
                    Logger.DebugLine($"{convertedText}", 5);
                    Logger.DebugLine($"=========================================", 10);

                    string newFileName = Path.GetFileName(path).Split('.').First() + $"_WB{sheetNum}.xl";
                    string newPath = Path.Combine(output, newFileName);
                    File.WriteAllText(newPath, convertedText);
                    Logger.DebugLine("Wrote Import to file " + newPath, 10);
                }
                else
                {
                    Logger.Debug("ValueArray or FormulaArray was null!");
                }
            }
        }

        private void ProcessObjects(object[,] valueArray, object[,] formulaArray)
        {
            var xV = valueArray.GetLength(0);
            var yV = valueArray.GetLength(1);
            var xF = formulaArray.GetLength(0);
            var yF = formulaArray.GetLength(1);

            Logger.DebugLine($"xV: {xV}, xF: {xF}, yV: {yV}, yF: {yF}");

            Logger.DebugLine($"Processing Values", 5);
            printArray(valueArray, "Value");
            Logger.DebugLine($"Processing Formulas", 5);
            printArray(formulaArray, "Formula");
            Logger.DebugLine($"Comparing Values and Formulas", 5);
            compareArrays(valueArray, formulaArray);
        }

        private void printArray(object[,] array, string name)
        {
            var x = array.GetLength(0);
            var y = array.GetLength(1);

            for (int i = 1; i <= x; i++)
            {
                for (int j = 1; j <= y; j++)
                {
                    var value = array[i, j];
                    Logger.Debug($"{i}, {j}: ");
                    printObject(value, name);
                    Logger.DebugLine("");
                }
            }
        }

        private void printObject(object value, string name)
        {
            if (value is string)
            {
                Logger.Debug($"string {name} {(string)value}");
            }
            else if (value is int)
            {
                Logger.Debug($"int {name} {(int)value}");
            }
            else if (value is double)
            {
                Logger.Debug($"double {name} {(double)value}");
            }
            else if (value is DateTime)
            {
                Logger.Debug($"DateTime {name} {(DateTime)value}");
            }
            else if (value != null)
            {
                Logger.Debug($"unknown {name} {value.ToString()}, type: {value.GetType()}", 1);
            }
            else
            {
                Logger.Debug($"unknown {name} Null");
            }
        }

        private void addConvertedValue(object value, int i, int j)
        {
            convertedText += $"C[{j}|{i}] = ";
            if (value is string)
            {
                convertedText += $"\"{(string)value}\"";
            }
            else if(value is DateTime)
            {
                convertedText += ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            else if(value is double)
            {
                convertedText += ((double)value).ToString("G", CultureInfo.InvariantCulture);
            }
            else
            {
                convertedText += value.ToString();
            }

            convertedText += "\n";
        }

        private void compareArrays(object[,] valueArray, object[,] formulaArray)
        {
            var x = valueArray.GetLength(0);
            var y = valueArray.GetLength(1);
            var xF = formulaArray.GetLength(0);
            var yF = formulaArray.GetLength(1);

            if (x != xF || y != yF)
            {
                Logger.DebugLine("Compare Arrays: x and y dimensions dont match between arrays", 1);
                Logger.DebugLine($"x: {x}, xF: {xF}, y: {y}, yF: {yF}", 1);
            }

            for (int i = 1; i <= x; i++)
            {
                for (int j = 1; j <= y; j++)
                {
                    var value = valueArray[i, j];
                    var formula = formulaArray[i, j];

                    var equals = false;

                    if(value != null && formula != null)
                    {
                        if (value is double)
                        {
                            string valueString = ((double)value).ToString("G", CultureInfo.InvariantCulture);
                            equals = ((string)formula).Equals(valueString);
                            var valueStringComma = valueString.Replace(".", ",");
                            var valueStringDot = valueString.Replace(",", ".");
                            var formulaStringComma = ((string)formula).Replace(".", ",");
                            var formulaStringDot = ((string)formula).Replace(",", ".");
                            Logger.DebugLine($"Value is double, value: {valueString}, formula: {(string)formula}, equals: {equals}", 1);
                            if (!equals)
                            {
                                equals = ((string)formula).Equals(valueStringComma) || ((string)formula).Equals(valueStringDot);
                                Logger.DebugLine($"Dot Comma conversion check, valueStringComma: {valueStringComma}, valueStringDot: {valueStringDot}, formula: {(string)formula}, equals: {equals}", 1);
                            }
                            if (!equals)
                            {
                                string valueStringInstalled = ((double)value).ToString("G", CultureInfo.InstalledUICulture);
                                equals = ((string)formula).Equals(valueStringInstalled);
                                Logger.DebugLine($"Installed UI Culture ({CultureInfo.InstalledUICulture}) Equal check, value: {valueStringInstalled}, formula: {(string)formula}, equals: {equals}", 1);
                            }
                            if (!equals)
                            {
                                var culture = CultureInfo.CreateSpecificCulture("de-DE");
                                string valueStringInstalled = ((double)value).ToString("G", culture);
                                equals = ((string)formula).Equals(valueStringInstalled);
                                Logger.DebugLine($"Specific Culture ({culture}) Equal check, value: {valueStringInstalled}, formula: {(string)formula}, equals: {equals}", 1);
                            }
                            if (!equals)
                            {
                                string valueStringDoubleFixedPoint = ((double)value).ToString(FormatStrings.DoubleFixedPoint, CultureInfo.InvariantCulture);
                                equals = ((string)formula).Equals(valueStringDoubleFixedPoint);
                                Logger.DebugLine($"Format DoubleFixedPoint with Culture ({CultureInfo.InvariantCulture}) Equal check, value: {valueStringDoubleFixedPoint}, formula: {(string)formula}, equals: {equals}", 1);
                            }

                            if (!equals && double.TryParse((string)formula, out double parsedFormula))
                            {
                                equals = IsDoubleEqual((double)value, parsedFormula);
                                Logger.DebugLine($"Parsed formula to double, value: {(double)value}, parsedFormula: {parsedFormula}, equals: {equals}", 1);
                            }

                            if(!equals && (double.TryParse(formulaStringComma, out parsedFormula) || double.TryParse(formulaStringDot, out parsedFormula))){
                                equals = IsDoubleEqual((double)value, parsedFormula);
                                Logger.DebugLine($"Parsed formula dot comma to double, value: {(double)value}, parsedFormula: {parsedFormula}, equals: {equals}", 1);
                            }
                        }
                        else if(value is DateTime)
                        {
                            Logger.DebugLine($"Value is Datetime, value: {value.ToString()}, formula: {(string)formula}", 1);
                            if (int.TryParse((string)formula, out int result))
                            {
                                DateTime date = new DateTime(1900, 1, 1).AddDays(result);
                                equals = true;
                                Logger.DebugLine($"Formula is int, date: {date.ToString()}, value date: {(DateTime)value}", 1);
                            }
                            else if(DateTime.TryParse((string)formula, out DateTime resultDate))
                            {
                                equals = true;
                                Logger.DebugLine($"Formula is datetime, resultDate: {resultDate.ToString()}, value date: {(DateTime)value}", 1);

                            }
                            else if(double.TryParse((string)formula, out double resultDouble))
                            {
                                equals = true;
                                //DateTime date = new DateTime(1900, 1, 1).AddDays(resultDouble);
                                Logger.DebugLine($"Formula is double, resultDouble: {resultDouble.ToString()}, value date: {(DateTime)value}", 1);
                            }
                            else
                            {
                                Logger.DebugLine($"Couldnt compare DateTime, value: {value.ToString()}, formula: {(string)formula}", 2);
                                equals = false;
                            }
                        }
                        else
                        {
                            equals = formula.Equals(value.ToString());
                        }
                    }

                    if (equals)
                    {
                        Logger.DebugLine($"{j}, {i}: Value equals Formula");
                        addConvertedValue(value, i, j);
                    }
                    else if (value == formula)
                    {
                        Logger.DebugLine($"{j}, {i}: Value == Formula");
                        addConvertedValue(value, i, j);
                    }
                    else if (value == null && string.IsNullOrEmpty((string)formula))
                    {
                        Logger.DebugLine($"{j}, {i}: Value equals Formula: Both empty");
                    }
                    else
                    {
                        printObject(value, "Value");
                        Logger.Debug("----");
                        printObject(formula, "Formula");
                        Logger.DebugLine("");
                        Logger.DebugLine($"{j}, {i}: Value does not equal Formula", 1);

                        SyntaxErrorListener errorListener = new SyntaxErrorListener();
                        AntlrInputStream inputStream = new AntlrInputStream((string)formula);
                        ExcelFormulaLexer spreadsheetLexer = new ExcelFormulaLexer(inputStream);
                        spreadsheetLexer.RemoveErrorListeners();
                        spreadsheetLexer.AddErrorListener(errorListener);
                        CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
                        ExcelFormulaParser excelFormulaParser = new ExcelFormulaParser(commonTokenStream);

                        ExcelFormulaParser.ExcelExprContext context = excelFormulaParser.excelExpr();
                        if (errorListener.HasError)
                        {
                            Logger.DebugLine($"Found Lexer Error - Dont process formula at {i}, {j} : {(string)formula}", 5);
                            if (GlobalSettings.ImportStopAtSyntaxError)
                            {
                                Console.WriteLine($"Lexer Error - Enter to continue", 10);
                                Console.ReadLine();
                            }
                            continue;
                        }
                        if (excelFormulaParser.NumberOfSyntaxErrors > 0)
                        {
                            Logger.DebugLine($"Found Syntax Error - Dont process formula at {i}, {j} : {(string)formula}", 5);
                            if (GlobalSettings.ImportStopAtSyntaxError)
                            {
                                Console.WriteLine($"Syntax Error - Enter to continue", 10);
                                Console.ReadLine();
                            }
                            continue;
                        }
                        ExcelFormulaVisitor visitor = new ExcelFormulaVisitor();
                        string formulaText = visitor.Visit(context);

                        if (visitor.Error)
                        {
                            continue;
                        }
                        Logger.DebugLine($"FormulaText: {formulaText}", 1);

                        convertedText += $"C[{j}|{i}] = {{{formulaText}}}\n";

                    }
                }
            }
        }

        private bool IsDoubleEqual(double double1, double double2)
        {
            double difference = double1 - double2;
            return Math.Abs(difference) < 0.001;
        }

        private bool compareDatesWithLeway(DateTime value, DateTime date, int daysLeway)
        {
            var time = value - date;
            return Math.Abs(time.Days) <= daysLeway;
        }
    }
}
