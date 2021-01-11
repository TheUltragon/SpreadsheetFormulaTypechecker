using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ANTLR_Test.Classes
{
    public class Error : IEquatable<Error>
    {
        public ErrorType Type;
        public string Specifier;

        public Error(ErrorType type, string specifier)
        {
            Type = type;
            Specifier = specifier;
        }

        public bool Equals(Error other)
        {
            return Type == other.Type && Specifier == other.Specifier;
        }

    }
    public class ErrorHandlerPersistentData
    {
        public List<Error> IgnoredErrors;
        public List<Error> UnIgnoredErrors;

        public ErrorHandlerPersistentData()
        {
            IgnoredErrors = new List<Error>();
            UnIgnoredErrors = new List<Error>();
        }
    }

    public class ErrorHandlerTransientData
    {
        public List<Error> FileIgnoredErrors;
        public List<Error> FileUnIgnoredErrors;
        public int ErrorInvocations;
        public int Errors;

        public ErrorHandlerTransientData()
        {
            FileIgnoredErrors = new List<Error>();
            FileUnIgnoredErrors = new List<Error>();
        }
    }

    public enum ErrorType
    {
        VariableNotDeclared,
        CellAdressWrongType,
        CellAdressRelativeNotFound,
        CellWrongType,
        ExpectedOtherType,
        UnexpectedEmptyType,
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
        public ErrorHandlerPersistentData data;
        public ErrorHandlerTransientData fileData;
        private string path => "Persistent/ErrorHandler.json";
        public void Load()
        {
            //Load Persistent data
            if (!File.Exists(path))
            {
                data = new ErrorHandlerPersistentData();
                return;
            }
            using(StreamReader reader = File.OpenText(path))
            {
                string jsonString = reader.ReadToEnd();
                data = JsonConvert.DeserializeObject<ErrorHandlerPersistentData>(jsonString);
                if (data == null)
                {
                    data = new ErrorHandlerPersistentData();
                }
            }
        }

        public void Save()
        {
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, jsonString);
        }

        public ErrorHandler()
        {
            Load();
            Reset();
            Save();
        }
        public void Reset()
        {
            fileData = new ErrorHandlerTransientData();
        }

        //Returns, wether the error really was an error (false) or wether it was ignored (true)
        public bool ThrowError(int line, int column, bool ignoreable, ErrorType type, string specifier, string payload, Types combinedTypes)
        {
            fileData.ErrorInvocations++;
            if (null != combinedTypes && (combinedTypes.HasType(VarType.RuntimeError) || combinedTypes.HasType(VarType.TypeError)))
            {
                Logger.DebugLine("Error has Type RuntimeError or TypeError as cause, skipping", 4);
                return false;
            }
            Logger.DebugLine($"", 5);
            //Check, wether this error should be ignored
            if (ignoreable && !CheckIfErrorToBeThrown(type, specifier)){
                //Check if ignored errors should be reported
                if(GlobalSettings.LogIgnoredErrors)
                {
                    Logger.DebugLine($"Ignored Error at {line},{column} of type {type} and specifier {specifier}: {payload}", 5);
                }
                return true;
            }

            //Check, wether unspecified errors should be ignore checked
            if(ignoreable && !checkIfErrorContainedInList(data.UnIgnoredErrors, type, specifier) && !checkIfErrorContainedInList(fileData.FileUnIgnoredErrors, type, specifier) && GlobalSettings.CheckIgnoreForUnspecifiedErrors)
            {
                Logger.DebugLine($"Error at {line},{column} of type '{type}' and specifier '{specifier}': {payload}", -1);
                bool result = checkIgnoreForUnspecifiedError(type, specifier);
                Save();
                if (!result)
                {
                    fileData.Errors++;
                }
                return result;
            }
            else
            {
                Logger.DebugLine($"Error at {line},{column} of type '{type}' and specifier '{specifier}': {payload}", 5);
            }

            fileData.Errors++;
            return false;
        }

        private bool checkIgnoreForUnspecifiedError(ErrorType type, string specifier)
        {
            Console.WriteLine($"Should this error be ignored?" +
                $"\n  To ignore this instance and mark it as no error do" + 
                $"\n    'y' for type and specifier combination, " +
                $"\n    'Y' for the whole type, " +
                $"\n    'yF' for the type and specifier combination only for this file, " +
                $"\n    'YF' for the whole type only for this file, " +
                $"\n    'y?' only for once." +
                $"\n  To not ignore this instance and mark it as an error do" +
                $"\n    'n' for type and specifier combination, " +
                $"\n    'N' for the whole type, " +
                $"\n    'nF' for the type and specifier combination only for this file, " +
                $"\n    'NF' for the whole type only for this file, " +
                $"\n    'n?' only for once.");
            while (true)
            {
                string input = "?";
                if (GlobalSettings.ErrorHandlerAskAtError)
                {
                    input = Console.ReadLine();
                }
                else
                {
                    input = GlobalSettings.ConvertErrorAnswerToInput(GlobalSettings.ErrorHandlerDefaultAnswer);
                }


                if (input.Equals("Y"))
                {
                    data.IgnoredErrors.Add(new Error(type, "*"));
                    return true;
                }
                else if (input.Equals("y"))
                {
                    data.IgnoredErrors.Add(new Error(type, specifier));
                    return true;
                }
                if (input.Equals("YF"))
                {
                    fileData.FileIgnoredErrors.Add(new Error(type, "*"));
                    return true;
                }
                else if (input.Equals("yF"))
                {
                    fileData.FileIgnoredErrors.Add(new Error(type, specifier));
                    return true;
                }
                else if (input.Equals("y?"))
                {
                    return true;
                }
                else if (input.Equals("N"))
                {
                    data.UnIgnoredErrors.Add(new Error(type, "*"));
                    return false;
                }
                else if (input.Equals("n"))
                {
                    data.UnIgnoredErrors.Add(new Error(type, specifier));
                    return false;
                }
                else if (input.Equals("NF"))
                {
                    fileData.FileUnIgnoredErrors.Add(new Error(type, "*"));
                    return false;
                }
                else if (input.Equals("nF"))
                {
                    fileData.FileUnIgnoredErrors.Add(new Error(type, specifier));
                    return false;
                }
                else if (input.Equals("n?"))
                {
                    return false;
                }
                else
                {
                    Console.WriteLine($"Unknown input, please input either 'Y', 'y', 'N', 'n' or '?'.");
                }
            }
        }

        public bool CheckIfErrorToBeThrown(ErrorType type, string specifier)
        {
            if(checkIfErrorContainedInList(data.UnIgnoredErrors, type, specifier))
            {
                return true;
            }
            else if (checkIfErrorContainedInList(fileData.FileUnIgnoredErrors, type, specifier))
            {
                return true;
            }
            else if(checkIfErrorContainedInList(data.IgnoredErrors, type, specifier))
            {
                return false;
            }
            else if (checkIfErrorContainedInList(fileData.FileIgnoredErrors, type, specifier))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool checkIfErrorContainedInList(List<Error> list, ErrorType type, string specifier)
        {
            if(list.Contains(new Error(type, "*")))
            {
                return true;
            }
            else if (list.Contains(new Error(type, specifier)))
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
