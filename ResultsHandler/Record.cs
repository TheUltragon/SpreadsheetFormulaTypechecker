using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultsHandler
{
    public class Record
    {
        public string Name = "";
        public long Filesize = 0;
        public long Cells = 0;
        public long Formulas = 0;
        public double FormulasPercent => (double)Formulas/(double)(Cells);
        public long Errors = 0;
        public long ErrorInvocations = 0;
        public long TypecheckTime = 0;
        public double TypecheckTimeSec => (double)TypecheckTime / 1000.0;
        public long ImportTime = 0;
        public double ImportTimeSec => (double)ImportTime / 1000.0;
        public long TotalTime => TypecheckTime + ImportTime;
        public double TotalTimeSec => (double)TotalTime / 1000.0;
        public double ImportTimePercent => (double)ImportTime / (double)TotalTime;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(Name) &&
                    Filesize > 0 &&
                    Cells > 0 &&
                    TypecheckTime > 0 &&
                    ImportTime > 0 &&
                    TotalTime > 0 &&
                    ImportTimePercent > 0.0;
        }

        public bool IsNonChecked()
        {
            return IsComplete() &&
                    ImportTimePercent > 0.4;
        }

        public bool NameContainsComma()
        {
            return Name.Contains(",");
        }

        public bool IsAcceptable()
        {
            return IsNonChecked() &&
                    Formulas > 0 &&
                    FormulasPercent > 0.0 &&
                    !NameContainsComma();
        }

        public static Record combineRecords(List<Record> records)
        {
            long typecheckTime = 0;
            long importTime = 0;
            long totalTime = 0;
            double importPercent = 0.0;
            foreach(var record in records)
            {
                typecheckTime += record.TypecheckTime;
                importTime += record.ImportTime;
                totalTime += record.TotalTime;
                importPercent += record.ImportTimePercent;
            }
            typecheckTime /= records.Count;
            importTime /= records.Count;
            totalTime /= records.Count;
            importPercent /= records.Count;

            Record result = new Record
            {
                Name = records.FirstOrDefault().Name,
                Filesize = records.FirstOrDefault().Filesize,
                Cells = records.FirstOrDefault().Cells,
                Formulas = records.FirstOrDefault().Formulas,
                Errors = records.FirstOrDefault().Errors,
                ErrorInvocations = records.FirstOrDefault().ErrorInvocations,
                TypecheckTime = typecheckTime,
                ImportTime = importTime,
            };

            if (!result.IsComplete())
            {
                Console.WriteLine("Error: Result Record not complete after combine: ");
                Console.WriteLine(result);
            }

            return result;
        }

        public override string ToString()
        {
            string result = "=================================================\n";
            result += $"Name: {Name}\n";
            result += $"Filesize: {Filesize}\n\n";
            result += $"Cells: {Cells}\n";
            result += $"Formulas: {Formulas}\n";
            result += $"FormulasPercent: {FormulasPercent}\n\n";
            result += $"Errors: {Errors}\n";
            result += $"ErrorInvocations: {ErrorInvocations}\n\n";
            result += $"TypecheckTime: {TypecheckTime}\n";
            result += $"ImportTime: {ImportTime}\n";
            result += $"TotalTime: {TotalTime}\n";
            result += $"ImportTimePercent: {ImportTimePercent}\n";
            result += "=================================================";
            return result;
        }

        public static string CSVHeader()
        {
            string result = "";
            result += $"name,";
            result += $"filesize,";
            result += $"cells,";
            result += $"formulas,";
            result += $"formulasPercent,";
            result += $"errors,";
            result += $"errorInvocations,";
            result += $"typecheckTime,";
            result += $"importTime,";
            result += $"totalTime,";
            result += $"importTimePercent";
            return result;
        }

        public string ToCSVString()
        {
            string result = "";
            result += $"{Name},";
            result += $"{Filesize},";
            result += $"{Cells},";
            result += $"{Formulas},";
            result += $"{FormulasPercent.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{Errors},";
            result += $"{ErrorInvocations},";
            result += $"{TypecheckTimeSec.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{ImportTimeSec.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{TotalTimeSec.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{ImportTimePercent.ToString("G", CultureInfo.InvariantCulture)}";
            return result;
        }


    }
}
