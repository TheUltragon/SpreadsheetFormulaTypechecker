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
            var files = Directory.EnumerateFiles(ResultsInputPath);
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

            OrderedRecords = OrganizeRecords(CleanedUnorderedRecords);
            acceptableRecords = CountAcceptable(OrderedRecords.Select(t => t.Value).ToList(), true);
            int commaNameRecords = CountCommaNames(OrderedRecords.Select(t => t.Value).ToList(), true);
            Console.WriteLine($"");
            Console.WriteLine($"Organized Records: {OrderedRecords.Count}");
            Console.WriteLine($"Acceptable Records: {acceptableRecords}");
            Console.WriteLine($"Comma Name Records: {commaNameRecords}");

            FinalRecords = OrderedRecords.Where(t => t.Value.IsAcceptable()).Select(t => t.Value).ToList();

            Dictionary<long, List<Record>> LogRecords = OrderRecordsInLogFormat(FinalRecords);
            List<Record> averagedLogRecords = LogRecords.Select(t => Record.combineRecords(t.Value)).ToList();

            WriteRecordsToFile(FinalRecords, "output.csv");
            WriteRecordsToFile(averagedLogRecords, "logOutput.csv");

            Console.ReadLine();
        }

        private static void WriteRecordsToFile(List<Record> records, string file)
        {
            string output = $"{Record.CSVHeader()}\n";
            foreach (var record in records)
            {
                output += $"{record.ToCSVString()}\n";
            }
            Directory.CreateDirectory(ResultsOutputPath);
            File.WriteAllText(Path.Combine(ResultsOutputPath, file), output);
        }

        private static Dictionary<long, List<Record>> OrderRecordsInLogFormat(List<Record> finalRecords)
        {
            Dictionary<long, List<Record>> result = new Dictionary<long, List<Record>>();
            //Initialize Result Dictionary
            for (int i = 1; i < 6; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    result.Add((long)Math.Pow(j,i), new List<Record>());
                }
            }
            //Go through each Record and put it into right spot of Dictionary
            foreach(var record in finalRecords)
            {
                for(int i = 0; i < result.Count; i++)
                {
                    var entry = result.ElementAt(i);
                    long max = entry.Key;
                    Console.WriteLine($"Result Max: {max}");
                    if(record.Formulas <= max)
                    {
                        Console.WriteLine($"Result Found: {record.Formulas}");
                        entry.Value.Add(record);
                        continue;
                    }
                }
            }

            return result;
        }

        private static double CalcImportPercentAverage(List<Record> Records)
        {
            double test = Records.Sum(t => t.ImportTimePercent) / Records.Count;
            Console.WriteLine($"Test Average ImportPercent: {test}");

            double sum = 0.0;
            foreach(var rec in Records)
            {
                sum += rec.ImportTimePercent;
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
                        tempRecord.Filesize = resultLong;
                    }
                    else if(GetLineContent(line, CellsLineSearch, "", out resultLong))
                    {
                        tempRecord.Cells = resultLong;
                    }
                    else if (GetLineContent(line, FormulasLineSearch, "", out resultLong))
                    {
                        tempRecord.Formulas = resultLong;
                    }
                    else if (GetLineContent(line, ErrorsLineSearch, "", out resultLong))
                    {
                        tempRecord.Errors = resultLong;
                    }
                    else if (GetLineContent(line, InvocationsLineSearch, "", out resultLong))
                    {
                        tempRecord.ErrorInvocations = resultLong;
                    }
                    else if (GetLineContent(line, TypecheckLineSearch, "", out resultLong))
                    {
                        tempRecord.TypecheckTime = resultLong;
                    }
                    else if (GetLineContent(line, ImportLineSearch, "", out resultLong))
                    {
                        tempRecord.ImportTime = resultLong;
                    }
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
