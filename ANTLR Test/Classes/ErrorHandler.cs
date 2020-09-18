using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ANTLR_Test.Classes
{
    public class ErrorHandlerData
    {
        public List<Tuple<ErrorType, string>> IgnoredErrors;
        public List<Tuple<ErrorType, string>> UnIgnoredErrors;
        public bool LogIgnoredErrors;
        public bool CheckIgnoreForUnspecifiedErrors;

        public ErrorHandlerData()
        {
            IgnoredErrors = new List<Tuple<ErrorType, string>>();
            LogIgnoredErrors = false;
        }
    }

    public enum ErrorType
    {
        ExpectedOtherType,
        IncompatibleTypesExpression,
    }

    public class ErrorHandler
    {
        public ErrorHandlerData data;
        private string path => "Data/ErrorHandler.json";
        public void Load()
        {
            StreamReader reader = File.OpenText(path);
            string jsonString = reader.ReadToEnd();
            data = JsonConvert.DeserializeObject<ErrorHandlerData>(jsonString);
        }

        public void Save()
        {
            string jsonString = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, jsonString);
        }

        public ErrorHandler()
        {
            Load();
        }

        //Returns, wether the error really was an error (true) or wether it was ignored (false)
        public bool ThrowError(ErrorType type, string specifier, string payload)
        {
            //Check, wether this error should be ignored
            if (!CheckIfErrorToBeThrown(type, specifier)){
                //Check if ignored errors should be reported
                if(data.LogIgnoredErrors)
                {
                    Console.WriteLine($"Ignored Error of type {type} and specifier {specifier}: {payload}");
                }
                return false;
            }

            Console.WriteLine($"Error of type {type} and specifier {specifier}: {payload}");
            //Check, wether unspecified errors should be ignore checked
            if(!checkIfErrorContainedInList(data.UnIgnoredErrors, type, specifier) && data.CheckIgnoreForUnspecifiedErrors)
            {
                return checkIgnoreForUnspecifiedError(type, specifier);
            }

            return true;
        }

        private bool checkIgnoreForUnspecifiedError(ErrorType type, string specifier)
        {
            Console.WriteLine($"Should this error be ignored?\n   'y' to ignore this type and specifier combination, 'Y' to ignore the whole type, 'n' to keep reporting this type-specifier combination and 'N' to keep reporting this error type, regardless of combination.");
            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("Y"))
                {
                    data.IgnoredErrors.Add(new Tuple<ErrorType, string>(type, "*"));
                    return false;
                }
                else if (input.Equals("y"))
                {
                    data.IgnoredErrors.Add(new Tuple<ErrorType, string>(type, specifier));
                    return false;
                }
                else if (input.Equals("N"))
                {
                    data.UnIgnoredErrors.Add(new Tuple<ErrorType, string>(type, "*"));
                    return true;
                }
                else if (input.Equals("n"))
                {
                    data.UnIgnoredErrors.Add(new Tuple<ErrorType, string>(type, specifier));
                    return true;
                }
                else
                {
                    Console.WriteLine($"Unknown input, please input either 'Y', 'y', 'N' or 'n'.");
                }
            }
        }

        public bool CheckIfErrorToBeThrown(ErrorType type, string specifier)
        {
            if(checkIfErrorContainedInList(data.UnIgnoredErrors, type, specifier))
            {
                return true;
            }
            else if(checkIfErrorContainedInList(data.IgnoredErrors, type, specifier))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool checkIfErrorContainedInList(List<Tuple<ErrorType, string>> list, ErrorType type, string specifier)
        {
            if(list.Contains(new Tuple<ErrorType, string>(type, "*")))
            {
                return true;
            }
            else if (list.Contains(new Tuple<ErrorType, string>(type, specifier)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
