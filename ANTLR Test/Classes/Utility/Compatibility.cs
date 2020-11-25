using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    //External class instead of part of VarTypeExtensions, because symbolic for compatibility operators in Inference Rules 
    public static class Compatibility
    {
        public static bool IsCompatible(Types type1, Types type2)
        {
            if (type1 == type2)
            {
                return true;
            }
            else if (type1.AllNumeric() && type2.AllNumeric())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Types GetHigherType(Types type1, Types type2)
        {
            if (type1.AllNumeric() && type2.AllNumeric())
            {
                return Types.ImplicitConversion(type1, type2);
            }
            else
            {
                return type1;
            }
        }

        public static bool HasAllowedVarType(VarType type, List<VarType> allowedVarTypes)
        {
            return allowedVarTypes.Contains(type);
        }
    }
}
