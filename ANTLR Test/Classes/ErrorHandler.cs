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
            UnIgnoredErrors = new List<Tuple<ErrorType, string>>();
            LogIgnoredErrors = false;
            CheckIgnoreForUnspecifiedErrors = true;
        }
    }

    public enum ErrorType
    {
        VariableNotDeclared,
        CellAdressWrongType,
        CellAdressRelativeNotFound,
        CellWrongType,
        ExpectedOtherType,
        IncompatibleTypesExpression,
        IncompatibleTypesAssignment,
        IncompatibleCurrencies,
    }

    public static class ErrorTypeExtension
    {
        public static string ToString(this ErrorType type)
        {
            return Enum.GetName(typeof(ErrorType), type);
        }
    }

    public class ErrorHandler
    {
        public ErrorHandlerData data;
        private string path => "Data/ErrorHandler.json";
        public void Load()
        {
            using(StreamReader reader = File.OpenText(path))
            {
                string jsonString = reader.ReadToEnd();
                data = JsonConvert.DeserializeObject<ErrorHandlerData>(jsonString);
                if (data == null)
                {
                    data = new ErrorHandlerData();
                }
            }
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

        //Returns, wether the error really was an error (false) or wether it was ignored (true)
        public bool ThrowError(int line, int column, bool ignoreable, ErrorType type, string specifier, string payload)
        {
            Logger.DebugLine($"");
            //Check, wether this error should be ignored
            if (ignoreable && !CheckIfErrorToBeThrown(type, specifier)){
                //Check if ignored errors should be reported
                if(data.LogIgnoredErrors)
                {
                    Logger.DebugLine($"Ignored Error at {line},{column} of type {type} and specifier {specifier}: {payload}");
                }
                return true;
            }

            Logger.DebugLine($"Error at {line},{column} of type '{type}' and specifier '{specifier}': {payload}");
            //Check, wether unspecified errors should be ignore checked
            if(ignoreable && !checkIfErrorContainedInList(data.UnIgnoredErrors, type, specifier) && data.CheckIgnoreForUnspecifiedErrors)
            {
                bool result = checkIgnoreForUnspecifiedError(type, specifier);
                if (result)
                {
                    Save();
                }
                return result;
            }

            return false;
        }

        private bool checkIgnoreForUnspecifiedError(ErrorType type, string specifier)
        {
            Logger.DebugLine($"Should this error be ignored?" +
                $"\n    'y' to ignore this type and specifier combination, " +
                $"\n    'Y' to ignore the whole type, " +
                $"\n    'n' to keep reporting this type-specifier combination and " +
                $"\n    'N' to keep reporting this error type, regardless of combination.");
            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("Y"))
                {
                    data.IgnoredErrors.Add(new Tuple<ErrorType, string>(type, "*"));
                    return true;
                }
                else if (input.Equals("y"))
                {
                    data.IgnoredErrors.Add(new Tuple<ErrorType, string>(type, specifier));
                    return true;
                }
                else if (input.Equals("N"))
                {
                    data.UnIgnoredErrors.Add(new Tuple<ErrorType, string>(type, "*"));
                    return false;
                }
                else if (input.Equals("n"))
                {
                    data.UnIgnoredErrors.Add(new Tuple<ErrorType, string>(type, specifier));
                    return false;
                }
                else
                {
                    Logger.DebugLine($"Unknown input, please input either 'Y', 'y', 'N' or 'n'.");
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
