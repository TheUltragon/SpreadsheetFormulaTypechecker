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
            else if(Count == 1)
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

        public static Types ImplicitConversion(Types a, Types b)
        {
            if(a.AllNumeric() && b.AllNumeric())
            {
                return GetHighestNumericType(a, b);
            }
            else if (a.OnlyHasType(VarType.Empty))
            {
                return b;
            }
            else if (b.OnlyHasType(VarType.Empty))
            {
                return a;
            }
            else if (b.HasUndefined())
            {
                return b;
            }
            else if (a.HasUndefined())
            {
                return a;
            }
            return a;
        }

        //Assumes both Types only have Numeric Types into them
        public static Types GetHighestNumericType(Types left, Types right)
        {
            if(!left.AllNumeric() || !right.AllNumeric())
            {
                throw new Exception("Not all numeric for Types.GetHighestNumericType!");
            }

            if(left.HasType(VarType.Decimal) || right.HasType(VarType.Decimal))
            {
                return new Types(VarType.Decimal);
            }
            else
            {
                return new Types(VarType.Int);
            }
        }


        public bool HasUndefined()
        {
            return types.Count == 0 ||
                   types.Contains(VarType.Empty) || 
                   types.Contains(VarType.None) || 
                   types.Contains(VarType.Unknown) || 
                   types.Contains(VarType.RuntimeError) || 
                   types.Contains(VarType.TypeError);
        }

        public Types Combine(Types otherType)
        {
            Types newTypes = Copy();
            newTypes.AddTypes(otherType);
            return newTypes;
        }

        public static Types Combine(Types a, Types b)
        {
            Types newTypes = a.Copy();
            newTypes.AddTypes(b);
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


        public static bool operator ==(Types a, Types b)
        {
            if(ReferenceEquals(a, b))
            {
                return true;
            }
            else if (ReferenceEquals(a, null))
            {
                return false;
            }
            else if(ReferenceEquals(b, null)){
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Types a, Types b)
        {
            return !(a == b);
        }

        public bool Equals(Types obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            else if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return this.types.SetEquals(obj.types);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Types);
        }

        public override string ToString()
        {
            if(Count == 0)
            {
                Logger.DebugLine("Count == 0");
                return "--Nothing--"; 
            }
            if(Count == 1)
            {
                var result = First.ToString();
                Logger.DebugLine("Count == 1 - Result: " + result, 1);
                return First.ToString();
            }
            else
            {
                string tps = "";
                bool first = true;
                foreach(VarType tp in types)
                {
                    Logger.Debug("I");
                    if (first)
                    {
                        tps = tp.ToString();
                        first = false;
                    }
                    else
                    {
                        tps += "," + tp.ToString();
                    }
                }
                Logger.DebugLine("Count > 1 - tps: " + tps, 1);
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
