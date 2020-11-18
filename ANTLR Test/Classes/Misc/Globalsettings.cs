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
        n_UnignoreCombination,
        N_UnignoreWholeType,
        DoNothing
    }
    
    public static class GlobalSettings
    {
        public static bool ImportStopAtMiscError = false;
        public static bool ImportStopAtSyntaxError = true;
        public static bool ImportStopAtUnsupportedError = false;
        public static bool ImportStopAtNextFile = false;
        public static bool TypecheckerStopAtNextFile = false;
        public static bool TypecheckerStopSyntaxError = true;
        public static bool ErrorHandlerAskAtError = true;
        public static TypecheckErrorAnswer ErrorHandlerDefaultAnswer = TypecheckErrorAnswer.DoNothing;
        public static bool LogIgnoredErrors = false;
        public static bool CheckIgnoreForUnspecifiedErrors = true;

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
            else if (answer == TypecheckErrorAnswer.n_UnignoreCombination)
            {
                return "n";
            }
            else if (answer == TypecheckErrorAnswer.N_UnignoreWholeType)
            {
                return "N";
            }
            else
            {
                return "?";
            }
        }
    }
}
