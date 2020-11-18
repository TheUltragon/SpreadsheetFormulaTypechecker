using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public enum VarType
    {
        Exp,
        Bool,
        String,
        Int,
        Decimal,
        Date,
        Currency,
        None,
        Unknown,
        Empty,
        TypeError,
        RuntimeError,
    }

    public class Types
    {
        public HashSet<VarType> types = new HashSet<VarType>();

        public Types()
        {
            AddType(VarType.None);
        }
        public Types(VarType tp)
        {
            AddType(tp);
        }

        public bool HasType(VarType tp)
        {
            return types.Contains(tp); 
        }

        public bool OnlyHasType(VarType tp)
        {
            return HasType(tp) && Count == 1;
        }
        
        public bool AllNumeric()
        {
            if(Count == 2)
            {
                return HasType(VarType.Int) && HasType(VarType.Decimal);
            }
            else if(.Count == 1)
            {
                return HasType(VarType.Int) || HasType(VarType.Decimal);
            }
            return false;
        }

        public VarType First => types.First();

        public int Count => types.Count;

        public bool Compatible(Types otherType)
        {
            if(AllNumeric() && otherType.AllNumeric())
            {
                return true;
            }
            else if(Count == 1 && otherType.Count == 1)
            {
                return First == otherType.First;
            }
            return false;
        }

        public void ImplicitConversion()
        {
            if (AllNumeric())
            {
                if (HasType(VarType.Decimal))
                {
                    SetToType(VarType.Decimal);
                }
                else
                {
                    SetToType(VarType.Int);
                }
            }
        }

        public Types Combine(Types otherType)
        {
            Types newTypes = Copy();
            newTypes.AddTypes(otherType);
            return newTypes;
        }

        public void AddType(VarType tp)
        {
            types.Add(tp);
            types.Remove(VarType.None);
        }

        public void AddTypes(Types tps)
        {
            foreach(var tp in tps.types)
            {
                AddType(tp);
            }
        }

        public Types Copy()
        {
            Types newTypes = new Types
            {
                types = new HashSet<VarType>(this.types)
            };
            return newTypes;
        }

        public void SetToType(VarType tp)
        {
            types = new HashSet<VarType>();
            AddType(tp);
        }

        public override string ToString()
        {
            if(Count == 1)
            {
                return First.ToString();
            }
            else
            {
                string tps = "";
                bool first = true;
                foreach(VarType tp in types)
                {
                    if (first)
                    {
                        tps = tp.ToString();
                    }
                    else
                    {
                        tps += "," + tp.ToString();
                    }
                }
                return tps;
            }
        }
    }

    public static class VarTypeExtensions
    {
        public static bool IsNumeric(this VarType type)
        {
            return type == VarType.Int || type == VarType.Decimal;
        }

        public static VarType GetHighestNumericType(VarType left, VarType right)
        {
            if(left == VarType.Decimal || right == VarType.Decimal)
            {
                return VarType.Decimal;
            }
            return VarType.Int;
        }
    }
}
