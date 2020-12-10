using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public enum TypecheckErrorAnswer
    {
        y_IgnoreCombination,
        Y_IgnoreWholeType,
        yF_IgnoreCombinationForFile,
        YF_IgnoreWholeTypeForFile,
        n_UnignoreCombination,
        N_UnignoreWholeType,
        nF_UnignoreCombinationForFile,
        NF_UnignoreWholeTypeForFile,
        DoNothing
    }
    
    public static class GlobalSettings
    {
        public static bool ImportStopAtMiscError = false;
        public static bool ImportStopAtSyntaxError = false;
        public static bool ImportStopAtUnsupportedError = false;
        public static bool ImportStopAtNextFile = false;
        public static bool TypecheckerStopAtNextFile = false;
        public static bool TypecheckerStopSyntaxError = false;
        public static bool ErrorHandlerAskAtError = true;
        public static TypecheckErrorAnswer ErrorHandlerDefaultAnswer = TypecheckErrorAnswer.DoNothing;
        public static bool LogIgnoredErrors = false;
        public static bool CheckIgnoreForUnspecifiedErrors = true;

        public static bool LoggerInstantOut = true;

        public static bool ResetTestrunCounter = false;

        public static bool ClearImportsAtStart = true;

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
            else
            {
                return "?";
            }
        }
    }
}
