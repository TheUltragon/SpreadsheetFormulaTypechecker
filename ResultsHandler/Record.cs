using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultsHandler
{

    public class Property
    {
        public double MeanValue;
        public double Variance;
        public double StandardDeviation;
        public double ConfidenceInterval90;
        public double ConfidenceInterval95;
        public double ConfidenceInterval99;

        public bool Empty()
        {
            return MeanValue < 0.0001 && MeanValue > -0.0001;
        }

        public Property()
        {
            MeanValue = 0;
            Variance = 0;
            StandardDeviation = 0;
            ConfidenceInterval90 = 0;
            ConfidenceInterval95 = 0;
            ConfidenceInterval99 = 0;
        }

        public Property(double value)
        {
            MeanValue = value;
            Variance = 0;
            StandardDeviation = 0;
            ConfidenceInterval90 = 0;
            ConfidenceInterval95 = 0;
            ConfidenceInterval99 = 0;
        }

        public Property(Property other)
        {
            MeanValue = other.MeanValue;
            Variance = other.Variance;
            StandardDeviation = other.StandardDeviation;
            ConfidenceInterval90 = other.ConfidenceInterval90;
            ConfidenceInterval95 = other.ConfidenceInterval95;
            ConfidenceInterval99 = other.ConfidenceInterval99;
        }

        public Property DividedCopy(double divisor)
        {
            Property copy = new Property(this);
            copy.MeanValue /= divisor;
            copy.StandardDeviation /= divisor;
            copy.Variance /= divisor;
            copy.ConfidenceInterval90 /= divisor;
            copy.ConfidenceInterval95 /= divisor;
            copy.ConfidenceInterval99 /= divisor;
            return copy;
        }

        public Property(List<Property> properties)
        {
            MeanValue = 0;
            Variance = 0;
            StandardDeviation = 0;
            ConfidenceInterval90 = 0;
            ConfidenceInterval95 = 0;
            ConfidenceInterval99 = 0;

            //Calc Meanvalue
            foreach (var prop in properties)
            {
                MeanValue += prop.MeanValue;
            }
            MeanValue /= properties.Count;

            //Calc Variance
            foreach (var prop in properties)
            {
                var diff = prop.MeanValue - MeanValue;
                Variance += diff * diff;
            }
            Variance /= properties.Count;

            //Calc StandardDeviation
            StandardDeviation = Math.Sqrt(Variance);

            //Calc Confidence Intervals
            ConfidenceInterval90 = CalcConfidenceInterval(1.645, properties.Count);
            ConfidenceInterval95 = CalcConfidenceInterval(1.960, properties.Count);
            ConfidenceInterval99 = CalcConfidenceInterval(2.576, properties.Count);
        }

        private double CalcConfidenceInterval(double z, int count)
        {
            var ci = z * StandardDeviation / Math.Sqrt((double)count);
            return ci;
        }
    }





    public class Record
    {
        public string Name = "";
        public Property Filesize = new Property();
        public Property Cells = new Property();
        public Property Formulas = new Property();
        public Property FormulasPercent = new Property();
        public Property Errors = new Property();
        public Property ErrorInvocations = new Property();
        public Property TypecheckTime = new Property();
        public Property TypecheckTimeSec = new Property();
        public Property ImportTime = new Property();
        public Property ImportTimeSec = new Property();
        public Property TotalTime = new Property();
        public Property TotalTimeSec = new Property();
        public Property ImportTimePercent = new Property();

        private bool isCombined = false;

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(Name) &&
                    Filesize.MeanValue > 0.0 &&
                    Cells.MeanValue > 0.0 &&
                    TypecheckTime.MeanValue > 0.0 &&
                    ImportTime.MeanValue > 0.0 &&
                    TotalTime.MeanValue > 0.0 &&
                    ImportTimePercent.MeanValue > 0.0;
        }

        public bool IsNonChecked()
        {
            return IsComplete() &&
                    ImportTimePercent.MeanValue > 0.4;
        }

        public bool NameContainsComma()
        {
            return Name.Contains(",");
        }

        public bool IsAcceptable()
        {
            return IsNonChecked() &&
                    Formulas.MeanValue > 0 &&
                    FormulasPercent.MeanValue > 0.0 &&
                    !NameContainsComma() &&
                    TotalTimeSec.MeanValue <= 100;    
        }

        public static Record combineRecords(List<Record> records)
        {
            if(records.Count == 0)
            {
                return new Record();
            }
            //long typecheckTime = 0;
            //long importTime = 0;
            //long fileSize = 0;
            //long cells = 0;
            //long formulas = 0;
            //long errors = 0;
            //long errorInvocations = 0;

            //foreach(var record in records)
            //{
            //    typecheckTime += record.TypecheckTime;
            //    importTime += record.ImportTime;
            //    fileSize += record.Filesize;
            //    cells += record.Cells;
            //    formulas += record.Formulas;
            //    errors += record.Errors;
            //    errorInvocations += record.ErrorInvocations;
            //}
            //typecheckTime /= records.Count;
            //importTime /= records.Count;
            //fileSize /= records.Count;
            //cells /= records.Count;
            //formulas /= records.Count;
            //errors /= records.Count;
            //errorInvocations /= records.Count;

            string name = records.FirstOrDefault().Name;
            if(!records.All(t => t.Name.Equals(name)))
            {
                name = "Combined record";
            }

            Record result = new Record
            {
                Name = name,
                Filesize = new Property(records.Select(t => t.Filesize).ToList()),
                Cells = new Property(records.Select(t => t.Cells).ToList()),
                Formulas = new Property(records.Select(t => t.Formulas).ToList()),
                FormulasPercent = new Property(records.Select(t => t.FormulasPercent).ToList()),
                Errors = new Property(records.Select(t => t.Errors).ToList()),
                ErrorInvocations = new Property(records.Select(t => t.ErrorInvocations).ToList()),
                TypecheckTime = new Property(records.Select(t => t.TypecheckTime).ToList()),
                TypecheckTimeSec = new Property(records.Select(t => t.TypecheckTimeSec).ToList()),
                ImportTime = new Property(records.Select(t => t.ImportTime).ToList()),
                ImportTimeSec = new Property(records.Select(t => t.ImportTimeSec).ToList()),
                TotalTime = new Property(records.Select(t => t.TotalTime).ToList()),
                TotalTimeSec = new Property(records.Select(t => t.TotalTimeSec).ToList()),
                ImportTimePercent = new Property(records.Select(t => t.ImportTimePercent).ToList()),
                isCombined = true,
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

        public string CSVHeader(bool includeName = true)
        {
            string result = "";
            if (includeName)
            {
                result += $"name,";
            }
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
            if (isCombined)
            {
                result += $",typecheckTimeCI,";
                result += $"importTimeCI,";
                result += $"totalTimeCI,";
                result += $"importTimePercentCI,";
            }
            return result;
        }

        public string ToCSVString(bool includeName = true)
        {
            string result = "";
            if (includeName)
            {
                result += $"{Name},";
            }
            result += $"{Filesize.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{Cells.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{Formulas.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{FormulasPercent.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{Errors.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{ErrorInvocations.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{TypecheckTimeSec.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{ImportTimeSec.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{TotalTimeSec.MeanValue.ToString("G", CultureInfo.InvariantCulture)},";
            result += $"{ImportTimePercent.MeanValue.ToString("G", CultureInfo.InvariantCulture)}";
            if (isCombined)
            {
                result += $",{TypecheckTimeSec.ConfidenceInterval90.ToString("G", CultureInfo.InvariantCulture)},";
                result += $"{ImportTimeSec.ConfidenceInterval90.ToString("G", CultureInfo.InvariantCulture)},";
                result += $"{TotalTimeSec.ConfidenceInterval90.ToString("G", CultureInfo.InvariantCulture)},";
                result += $"{ImportTimePercent.ConfidenceInterval90.ToString("G", CultureInfo.InvariantCulture)}";
            }
            return result;
        }


    }
}
