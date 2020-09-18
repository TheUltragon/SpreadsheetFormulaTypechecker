using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public class ValueBase
    {
        public virtual VarType GetVarType()
        {
            return VarType.None;
        }
    }

    public class ValueGeneric<T> : ValueBase
    {
        public T Value { get; set; }
        public ValueGeneric(T initialValue)
        {
            Value = initialValue;
        }
        public override string ToString()
        {
            return "Value: " + Value.ToString();
        }
    }
    
    public class CharValue : ValueGeneric<char>
    {
        public CharValue(char initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(CharValue);
        }
        public override VarType GetVarType()
        {
            return VarType.Char;
        }
    }

    public class IntValue : ValueGeneric<int>
    {
        public IntValue(int initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(IntValue);
        }
        public override VarType GetVarType()
        {
            return VarType.Int;
        }
    }

    public class DecimalValue : ValueGeneric<double>
    {
        public DecimalValue(double initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(DecimalValue);
        }
        public override VarType GetVarType()
        {
            return VarType.Decimal;
        }
    }

    public class CurrencyValue : ValueGeneric<double>
    {
        public enum CurrencyType
        {
            Euros,
            Dollars
        }
        public CurrencyValue(double initialValue, CurrencyType type) : base(initialValue) { this.type = type; }
        public CurrencyType type;
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(CurrencyValue);
        }
        public override VarType GetVarType()
        {
            return VarType.Currency;
        }
    }

    public class StringValue : ValueGeneric<string>
    {
        public StringValue(string initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(StringValue);
        }
        public override VarType GetVarType()
        {
            return VarType.String;
        }
    }

    public class BoolValue : ValueGeneric<bool>
    {
        public BoolValue(bool initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(BoolValue);
        }
        public override VarType GetVarType()
        {
            return VarType.Bool;
        }
    }

    public class DateValue : ValueGeneric<DateTime>
    {
        public DateValue(DateTime initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(DateValue);
        }
        public override VarType GetVarType()
        {
            return VarType.Date;
        }
    }

    public class EmptyValue : ValueBase
    {
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(EmptyValue);
        }
        public override string ToString()
        {
            return "Empty";
        }
        public override VarType GetVarType()
        {
            return VarType.None;
        }
    }

    public class ExpValue : ValueBase
    {
        private SpreadsheetVisitor _visitor;
        private SpreadsheetParser.ExpContext _expression;
        private ValueBase _value;
        public ValueBase Value
        {
            get
            {
                if(_value == null)
                {
                    Eval();
                }
                return _value;
            }
            private set
            {
                _value = value;
            }
        }

        public bool Eval()
        {
            var result = _visitor.Visit(_expression);
            Value = _visitor.LastExpValue;
            return result;
        }
        public ExpValue(SpreadsheetVisitor visitor, SpreadsheetParser.ExpContext expression)
        {
            _visitor = visitor;
            _expression = expression;
            _value = null;
        }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(ExpValue);
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public override VarType GetVarType()
        {
            return Value.GetVarType();
        }
    }
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
        Char
    }

    public static class VarTypeExtensions
    {
        public static bool IsNumeric(this VarType type)
        {
            return type == VarType.Int || type == VarType.Decimal;
        }

        public static bool IsText(this VarType type)
        {
            return type == VarType.Char || type == VarType.String;
        }

        public static VarType GetHighestNumericType(VarType left, VarType right)
        {
            if(left == VarType.Decimal || right == VarType.Decimal)
            {
                return VarType.Decimal;
            }
            return VarType.Int;
        }

        public static VarType GetHighestTextType(VarType left, VarType right)
        {
            if (left == VarType.String || right == VarType.String)
            {
                return VarType.String;
            }
            return VarType.Char;
        }
    }
}
