﻿using System;
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
        public override void ImportFile(string path)
        {
            Application app = new Application();
            Workbook workBook = app.Workbooks.Open(Path.GetFullPath(path));

            handleWorkBook(workBook, path);

            workBook.Close(false, path, null);
            Marshal.ReleaseComObject(workBook);
        }

        private void handleWorkBook(Workbook workBook, string path)
        {
            int numSheets = workBook.Sheets.Count;
            Logger.DebugLine($"Sheets: {numSheets}", 1);

            //
            // Iterate through the sheets. They are indexed starting at 1.
            //
            for (int sheetNum = 1; sheetNum < numSheets + 1; sheetNum++)
            {
                Logger.DebugLine($"Importing Sheet {sheetNum}", 1);
                Worksheet sheet = (Worksheet)workBook.Sheets[sheetNum];

                //
                // Take the used range of the sheet. Finally, get an object array of all
                // of the cells in the sheet (their values). You can do things with those
                // values. See notes about compatibility.
                //
                Range excelRange = sheet.UsedRange;
                object[,] valueArray = (object[,])excelRange.get_Value(
                    XlRangeValueDataType.xlRangeValueDefault);
                object[,] formulaArray = new object[1,1];
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
                if(valueArray != null && formulaArray != null)
                {
                    convertedText = "";

                    ProcessObjects(valueArray, formulaArray);
                    //Add Implicit eval at end of file
                    convertedText += "eval\n";

                    Logger.DebugLine($"=========================================", 10);
                    Logger.DebugLine($"Imported File Workbook {sheetNum}", 10);
                    Logger.DebugLine($"{convertedText}", 10);
                    Logger.DebugLine($"=========================================", 10);

                    string newPath = path.Replace("Corpus", "Imports").Split('.').First() + $"_WB{sheetNum}.xl";
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

            Logger.DebugLine($"Processing Values", 1);
            printArray(valueArray, "Value");
            Logger.DebugLine($"Processing Formulas", 1);
            printArray(formulaArray, "Formula");
            Logger.DebugLine($"Comparing Values and Formulas", 1);
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

            if(x != xF || y != yF)
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
                            equals = formula.Equals(((double)value).ToString("G", CultureInfo.InvariantCulture));
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

                        AntlrInputStream inputStream = new AntlrInputStream((string)formula);
                        ExcelFormulaLexer spreadsheetLexer = new ExcelFormulaLexer(inputStream);
                        CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
                        ExcelFormulaParser excelFormulaParser = new ExcelFormulaParser(commonTokenStream);

                        ExcelFormulaParser.ExcelExprContext context = excelFormulaParser.excelExpr();
                        ExcelFormulaVisitor visitor = new ExcelFormulaVisitor();
                        string formulaText = visitor.Visit(context);
                        Logger.DebugLine($"FormulaText: {formulaText}", 1);

                        convertedText += $"C[{j}|{i}] = ({formulaText})\n";

                    }
                }
            }
        }
    }
}
