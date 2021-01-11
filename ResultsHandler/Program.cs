using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ResultsHandler
{
    class Program
    {
        public static List<Record> UnorderedRecords = new List<Record>();
        public static Dictionary<string, Record> OrderedRecords = new Dictionary<string, Record>();
        public static List<Record> FinalRecords = new List<Record>();

        public static string ResultsInputPath => @"C:\Users\Win10\source\repos\BNFCTest\ANTLR Test\bin\Debug\Persistent\Logs";
        public static string ResultsInputPath2 => @"C:\Users\Admin\source\repos\BNFCTest2\SpreadsheetFormulaTypechecker\bin\Release\Persistent\Logs";
        public static string ResultsOutputPath => @"Output";

        private static string StartLineSearchPrefix => "========= ";
        private static string StartLineSearchSuffix => " =========";
        private static string SizeLineSearch => "Filesize (KB): ";
        private static string CellsLineSearch => "Cells Total: ";
        private static string FormulasLineSearch => "Formulas Total: ";
        private static string FormulasPercentLineSearch => "Formulas Percent: ";
        private static string ErrorsLineSearch => "Errors Total: ";
        private static string InvocationsLineSearch => "Error Invocations Total: ";
        private static string TypecheckLineSearch => "Typecheck time: ";
        private static string ImportLineSearch => "Import time: ";
        private static string TotalTimeLineSearch => "Total time: ";
        private static string ImportPercentLineSearch => "Import time Percent: ";

        private static int contentLength = 17;


        static void Main(string[] args)
        {
            List<string> files = new List<string>();
            if (Directory.Exists(ResultsInputPath))
            {
                files.AddRange(Directory.EnumerateFiles(ResultsInputPath));
            }
            if (Directory.Exists(ResultsInputPath2))
            {
                files.AddRange(Directory.EnumerateFiles(ResultsInputPath2));
            }
            foreach (var file in files)
            {
                var lines = File.ReadLines(file);
                ReadLines(lines.ToList());
            }

            int completeRecords = CountComplete(UnorderedRecords, false);
            int nonCheckedRecords = CountNonChecked(UnorderedRecords);
            int acceptableRecords = CountAcceptable(UnorderedRecords, false);
            Console.WriteLine($"Records: {UnorderedRecords.Count}");
            Console.WriteLine($"Complete Records: {completeRecords}");
            Console.WriteLine($"NonChecked Records: {nonCheckedRecords}");
            Console.WriteLine($"Acceptable Records: {acceptableRecords}");

            List<Record> CleanedUnorderedRecords = UnorderedRecords.Where(r => r.IsNonChecked()).ToList();

            //Count Different Record Classes
            OrderedRecords = OrganizeRecords(CleanedUnorderedRecords);
            acceptableRecords = CountAcceptable(OrderedRecords.Select(t => t.Value).ToList(), false);
            int commaNameRecords = CountCommaNames(OrderedRecords.Select(t => t.Value).ToList(), false);
            Console.WriteLine($"");
            Console.WriteLine($"Organized Records: {OrderedRecords.Count}");
            Console.WriteLine($"Acceptable Records: {acceptableRecords}");
            Console.WriteLine($"Comma Name Records: {commaNameRecords}");

            //Get final records from IsAcceptable property in each record
            //FinalRecords = OrderedRecords.Where(t => t.Value.IsAcceptable()).Select(t => t.Value).ToList();
            FinalRecords = CleanedUnorderedRecords.Where(t => t.IsAcceptable()).ToList();

            //Records with at least one error
            var FinalRecordsError = FinalRecords.Where(t => t.Errors.MeanValue > 0).ToList();

            //Get Records in log distance
            Dictionary<long, List<Record>> LogRecords = OrderRecordsInLogFormat(FinalRecords, t => t.Formulas);
            List<Record> averagedLogRecords = LogRecords.Select(t => Record.combineRecords(t.Value)).ToList();

            //Get records in formulas histogram baskets
            Dictionary<long, List<Record>> BasketRecords = OrderRecordsInBasketFormat(FinalRecords, 9, 500, t => t.Formulas);
            List<Record> averagedBasketRecords = BasketRecords.Select(t => { var c = Record.combineRecords(t.Value); c.Formulas.MeanValue = t.Key; return c; }).ToList();

            //Get Records in log distance
            Dictionary<long, List<Record>> LogRecordsCells = OrderRecordsInLogFormat(FinalRecords, t => t.Cells);
            List<Record> averagedLogRecordsCells = LogRecordsCells.Select(t => Record.combineRecords(t.Value)).ToList();

            //Get records in cells histogram baskets
            Dictionary<long, List<Record>> BasketRecordsCells = OrderRecordsInBasketFormat(FinalRecords, 10, 1500, t => t.Cells);
            List<Record> averagedBasketRecordsCells = BasketRecordsCells.Select(t => { var c = Record.combineRecords(t.Value); c.Cells.MeanValue = t.Key; return c; }).ToList();
           
            //Get records in filesize histogram baskets
            Dictionary<long, List<Record>> BasketRecordsFilesize = OrderRecordsInBasketFormat(FinalRecords, 10, 250, t => t.Filesize);
            List<Record> averagedBasketRecordsFilesize = BasketRecordsFilesize.Select(t => { var c = Record.combineRecords(t.Value); c.Filesize.MeanValue = t.Key; return c; }).ToList();


            //Write Records to CSV files
            WriteRecordsToFile(FinalRecords, "output.csv");
            WriteRecordsToFile(FinalRecordsError, "errorOutput.csv");
            WriteRecordsToFile(averagedLogRecords, "logOutput.csv", false);
            WriteRecordsToFile(averagedBasketRecords, "basketOutput.csv", false);
            WriteRecordsToFile(averagedLogRecordsCells, "logOutputCells.csv", false);
            WriteRecordsToFile(averagedBasketRecordsCells, "basketOutputCells.csv", false);
            WriteRecordsToFile(averagedBasketRecordsFilesize, "basketOutputFilesize.csv", false);

            Console.WriteLine($"Total Time for testrun: {FinalRecords.Sum(t => t.TotalTimeSec.MeanValue)}");

            Console.WriteLine($"Press Enter to Exit");
            Console.ReadLine();
        }

        private static Dictionary<long, List<Record>> OrderRecordsInBasketFormat(List<Record> finalRecords, int baskets, int maxCount, Func<Record, Property> propertySelector)
        {
            Dictionary<long, List<Record>> result = new Dictionary<long, List<Record>>();
            //Initialize Result Dictionary
            for (int i = 0; i < baskets; i++)
            {
                long max = ((i+1) * maxCount) / baskets;
                result.Add(max, new List<Record>());
            }

            List<Record> records = finalRecords.Where(t => propertySelector(t).MeanValue <= (double)maxCount).ToList();

            result = OrderRecordsInBaskets(records, result, propertySelector);

            int counter = 0;
            foreach(var rec in result)
            {
                counter += rec.Value.Count;
                Console.WriteLine($"Basket {rec.Key}: {rec.Value.Count}, counter: {counter}");
            }

            return result;
        }

        private static Dictionary<long, List<Record>> OrderRecordsInLogFormat(List<Record> finalRecords, Func<Record, Property> propertySelector)
        {
            Dictionary<long, List<Record>> result = new Dictionary<long, List<Record>>();
            //Initialize Result Dictionary
            for (int i = 0; i < 7; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    result.Add((long)(j * Math.Pow(10, i)), new List<Record>());
                }
            }
            //Go through each Record and put it into right spot of Dictionary
            result = OrderRecordsInBaskets(finalRecords, result, propertySelector);

            return result;
        }

        private static Dictionary<long, List<Record>> OrderRecordsInBaskets(List<Record> finalRecords, Dictionary<long, List<Record>> result, Func<Record, Property> propertySelector)
        {
            //Go through each Record and put it into right spot of Dictionary
            foreach (var record in finalRecords)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    var entry = result.ElementAt(i);
                    long max = entry.Key;
                    //Console.WriteLine($"Result Max: {max}");
                    if (propertySelector(record).MeanValue <= (double)max)
                    {
                        //Console.WriteLine($"Result Found: {record.Formulas}");
                        entry.Value.Add(record);
                        break;
                    }
                }
            }

            return result;
        }

        private static void WriteRecordsToFile(List<Record> records, string file, bool includeName = true)
        {
            string output = $"{records[0].CSVHeader(includeName)}\n";
            foreach (var record in records)
            {
                output += $"{record.ToCSVString(includeName)}\n";
            }
            Directory.CreateDirectory(ResultsOutputPath);
            File.WriteAllText(Path.Combine(ResultsOutputPath, file), output);
        }

        

        private static double CalcImportPercentAverage(List<Record> Records)
        {
            double test = Records.Sum(t => t.ImportTimePercent.MeanValue) / Records.Count;
            //Console.WriteLine($"Test Average ImportPercent: {test}");

            double sum = 0.0;
            foreach(var rec in Records)
            {
                sum += rec.ImportTimePercent.MeanValue;
            }
            return sum / Records.Count;


        }

       

        private static Dictionary<string, Record> OrganizeRecords(List<Record> unorderedRecords)
        {
            Dictionary<string, List<Record>> OrganizedRecords = new Dictionary<string, List<Record>>();
            foreach(var record in unorderedRecords)
            {
                string name = record.Name;
                if(OrganizedRecords.TryGetValue(name, out List<Record> value)){
                    value.Add(record);
                }
                else
                {
                    List<Record> list = new List<Record>();
                    list.Add(record);
                    OrganizedRecords.Add(name, list);
                }
            }

            Dictionary<string, Record> Result = new Dictionary<string, Record>();
            foreach(var pair in OrganizedRecords)
            {
                Result.Add(pair.Key, Record.combineRecords(pair.Value));
            }

            return Result;
        }

        private static int CountCommaNames(List<Record> list, bool print)
        {
            int counter = 0;
            if(print)
               Console.WriteLine("Records with Comma in name:");
            foreach (var record in list)
            {
                if (record.NameContainsComma())
                {
                    counter++;
                    Console.WriteLine(record);
                }
            }
            return counter;
        }

        private static int CountAcceptable(List<Record> unorderedRecords, bool print)
        {
            int counter = 0;
            if(print)
                Console.WriteLine("Complete but Unnaccaptable Records:");
            foreach(var record in unorderedRecords)
            {
                if (record.IsAcceptable())
                {
                    counter++;
                }
                else if (print && record.IsComplete())
                {
                    Console.WriteLine(record);
                }
            }
            return counter;
        }

        private static int CountNonChecked(List<Record> unorderedRecords)
        {
            int counter = 0;
            foreach (var record in unorderedRecords)
            {
                if (record.IsNonChecked())
                {
                    counter++;
                }
            }
            return counter;
        }

        private static int CountComplete(List<Record> unorderedRecords, bool print)
        {
            int counter = 0;
            if(print)
                Console.WriteLine("Uncomplete Records:");
            foreach (var record in unorderedRecords)
            {
                if (record.IsComplete())
                {
                    counter++;
                }
                else if(print)
                {
                    Console.WriteLine(record);
                }
            }
            return counter;
        }

        static void ReadLines(List<string> lines)
        {
            var orderedLines = OrderLines(lines);

            //PrintOrderedLines(orderedLines);

            foreach(var record in orderedLines)
            {
                Record tempRecord = new Record();
                foreach(var line in record)
                {
                    if(GetLineContent(line, StartLineSearchPrefix, StartLineSearchSuffix, out string result))
                    {
                        tempRecord.Name = result;
                    }
                    else if(GetLineContent(line, SizeLineSearch, "", out long resultLong))
                    {
                        tempRecord.Filesize = new Property(resultLong);
                    }
                    else if(GetLineContent(line, CellsLineSearch, "", out resultLong))
                    {
                        tempRecord.Cells = new Property(resultLong);
                    }
                    else if (GetLineContent(line, FormulasLineSearch, "", out resultLong))
                    {
                        tempRecord.Formulas = new Property(resultLong);
                    }
                    else if (GetLineContent(line, ErrorsLineSearch, "", out resultLong))
                    {
                        tempRecord.Errors = new Property(resultLong);
                    }
                    else if (GetLineContent(line, InvocationsLineSearch, "", out resultLong))
                    {
                        tempRecord.ErrorInvocations = new Property(resultLong);
                    }
                    else if (GetLineContent(line, TypecheckLineSearch, "", out resultLong))
                    {
                        tempRecord.TypecheckTime = new Property(resultLong);
                        tempRecord.TypecheckTimeSec = new Property(tempRecord.TypecheckTime.MeanValue / 1000.0);
                    }
                    else if (GetLineContent(line, ImportLineSearch, "", out resultLong))
                    {
                        tempRecord.ImportTime = new Property(resultLong);
                        tempRecord.ImportTimeSec = new Property(tempRecord.ImportTime.MeanValue / 1000.0);
                    }
                }
                if(!tempRecord.Formulas.Empty() && !tempRecord.Cells.Empty())
                {
                    tempRecord.FormulasPercent = new Property(tempRecord.Formulas.MeanValue / tempRecord.Cells.MeanValue);
                }
                if(!tempRecord.TypecheckTime.Empty() && !tempRecord.ImportTime.Empty())
                {
                    tempRecord.TotalTime = new Property(tempRecord.TypecheckTime.MeanValue + tempRecord.ImportTime.MeanValue);
                    tempRecord.TotalTimeSec = new Property(tempRecord.TotalTime.MeanValue / 1000.0);
                    tempRecord.ImportTimePercent = new Property(tempRecord.ImportTime.MeanValue / tempRecord.TotalTime.MeanValue);
                }
                UnorderedRecords.Add(tempRecord);
            }
        }


        static List<List<string>> OrderLines(List<string> lines)
        {
            int counter = 0;
            List<string> lineBatch = new List<string>();
            List<List<string>> orderedLines = new List<List<string>>();

            foreach (var line in lines)
            {
                counter++;

                if (counter % contentLength == 0)
                {
                    orderedLines.Add(lineBatch);
                    lineBatch = new List<string>();
                }
                lineBatch.Add(line);
            }
            return orderedLines;
        }

        static void PrintOrderedLines(List<List<string>> orderedLines)
        {
            foreach(var record in orderedLines)
            {
                Console.WriteLine("----------------------------------------------------");
                foreach(var line in record)
                {
                    Console.WriteLine(line);
                }
            }
        }


        static bool GetLineContent(string text, string prefix, string suffix, out string result)
        {
            int find = text.IndexOf(prefix, 0, StringComparison.Ordinal);
            if(find > -1)
            {
                result = text.Replace(prefix, "");
                if (!string.IsNullOrEmpty(suffix))
                {
                    result = result.Replace(suffix, "");
                }
                return true;
            }
            result = "";
            return false;
        }

        static bool GetLineContent(string text, string prefix, string suffix, out long resultLong)
        {
            int find = text.IndexOf(prefix, 0, StringComparison.Ordinal);
            if (find > -1)
            {
                string result = text.Replace(prefix, "");
                if (!string.IsNullOrEmpty(suffix))
                {
                    result = result.Replace(suffix, "");
                }
                return long.TryParse(result, out resultLong);
            }
            resultLong = 0;
            return false;
        }

        static bool GetLineContent(string text, string prefix, string suffix, out double resultDouble)
        {
            int find = text.IndexOf(prefix, 0, StringComparison.Ordinal);
            if (find > -1)
            {
                string result = text.Replace(prefix, "");
                if (!string.IsNullOrEmpty(suffix))
                {
                    result = result.Replace(suffix, "");
                }
                return double.TryParse(result, out resultDouble);
            }
            resultDouble = 0.0;
            return false;
        }
    }
}
