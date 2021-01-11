using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ANTLR_Test.Classes
{
    public enum TypecheckErrorAnswer
    {
        //Ignore the Error
        y_IgnoreCombination = 1,
        Y_IgnoreWholeType,
        yF_IgnoreCombinationForFile,
        YF_IgnoreWholeTypeForFile,
        y_IgnoreOnce,

        //Dont ignore the Error and mark it instead
        n_UnignoreCombination = 11,
        N_UnignoreWholeType,
        nF_UnignoreCombinationForFile,
        NF_UnignoreWholeTypeForFile,
        n_UnignoreOnce,
    }

    public class GlobalSettingsData
    {
        public int MinConsoleLevel = 5;
        public bool LoggerActive = true;
        public bool LoggerInstantOut = true;

        public bool ImportStopAtMiscError = false;
        public bool ImportStopAtSyntaxError = false;
        public bool ImportStopAtUnsupportedError = false;
        public bool ImportStopAtNextFile = false;

        public bool TypecheckerStopAtNextFile = false;
        public bool TypecheckerStopSyntaxError = false;

        public bool ErrorHandlerAskAtError = true;
        public TypecheckErrorAnswer ErrorHandlerDefaultAnswer = TypecheckErrorAnswer.y_IgnoreOnce;
        public bool LogIgnoredErrors = false;
        public bool CheckIgnoreForUnspecifiedErrors = true;

        public bool ResetTestrunCounter = true;
        public bool ClearImportsAtStart = true;
        public string ImportPathEuses = "";
    }
    
    public static class GlobalSettings
    {
        private static string _path => "Persistent/GlobalSettings.json";

        public static GlobalSettingsData data = new GlobalSettingsData();

        public static int MinConsoleLevel => data.MinConsoleLevel;
        public static bool LoggerActive => data.LoggerActive;
        public static bool ImportStopAtMiscError => data.ImportStopAtMiscError;
        public static bool ImportStopAtSyntaxError => data.ImportStopAtSyntaxError;
        public static bool ImportStopAtUnsupportedError => data.ImportStopAtUnsupportedError;
        public static bool ImportStopAtNextFile => data.ImportStopAtNextFile;
        public static bool TypecheckerStopAtNextFile => data.TypecheckerStopAtNextFile;
        public static bool TypecheckerStopSyntaxError => data.TypecheckerStopSyntaxError;
        public static bool ErrorHandlerAskAtError => data.ErrorHandlerAskAtError;
        public static TypecheckErrorAnswer ErrorHandlerDefaultAnswer => data.ErrorHandlerDefaultAnswer;
        public static bool LogIgnoredErrors => data.LogIgnoredErrors;
        public static bool CheckIgnoreForUnspecifiedErrors => data.CheckIgnoreForUnspecifiedErrors;
        public static bool LoggerInstantOut => data.LoggerInstantOut;
        public static bool ResetTestrunCounter => data.ResetTestrunCounter;
        public static bool ClearImportsAtStart => data.ClearImportsAtStart;
        public static string ImportPathCorpus
        {
            get
            {
                if (string.IsNullOrEmpty(data.ImportPathEuses) || !Directory.Exists(data.ImportPathEuses))
                {
                    data.ImportPathEuses = GetImportEusesFromUser();
                    Save();
                }
                return data.ImportPathEuses;
            }
        }

        public static void Load()
        {
            //Load Persistent data
            if (!File.Exists(_path))
            {
                data = new GlobalSettingsData();
                return;
            }
            using (StreamReader reader = File.OpenText(_path))
            {
                string jsonString = reader.ReadToEnd();
                data = JsonConvert.DeserializeObject<GlobalSettingsData>(jsonString);
                if (data == null)
                {
                    data = new GlobalSettingsData();
                }
            }
        }

        public static void Save()
        {
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_path, jsonString);
        }

        public static void ShowSettingsPath()
        {
            Console.WriteLine($"The Settings can be found at: \n{Path.GetFullPath(_path)}");
        }

        public static string GetImportEusesFromUser()
        {
            Console.WriteLine($"======================================================");
            Console.WriteLine($"Please Input a valid Path to the spreadsheet corpus:");
            while (true)
            {
                string path = Console.ReadLine();
                if (Directory.Exists(path))
                {
                    Console.WriteLine($"======================================================");
                    return path;
                }
                Console.WriteLine($"Path did not lead to a directory, please input a valid path:");
            }
        }

        public static string ConvertErrorAnswerToInput(TypecheckErrorAnswer answer)
        {
            if (answer == TypecheckErrorAnswer.y_IgnoreCombination)
            {
                return "y";
            }
            else if (answer == TypecheckErrorAnswer.Y_IgnoreWholeType)
            {
                return "Y";
            }
            else if (answer == TypecheckErrorAnswer.yF_IgnoreCombinationForFile)
            {
                return "yF";
            }
            else if (answer == TypecheckErrorAnswer.YF_IgnoreWholeTypeForFile)
            {
                return "YF";
            }
            else if (answer == TypecheckErrorAnswer.y_IgnoreOnce)
            {
                return "y?";
            }
            else if (answer == TypecheckErrorAnswer.n_UnignoreCombination)
            {
                return "n";
            }
            else if (answer == TypecheckErrorAnswer.N_UnignoreWholeType)
            {
                return "N";
            }
            else if(answer == TypecheckErrorAnswer.nF_UnignoreCombinationForFile)
            {
                return "nF";
            }
            else if (answer == TypecheckErrorAnswer.NF_UnignoreWholeTypeForFile)
            {
                return "NF";
            }
            else if (answer == TypecheckErrorAnswer.n_UnignoreOnce)
            {
                return "n?";
            }
            else
            {
                return "y?";
            }
        }
    }
}
