using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test
{
    public class ValueBase { }

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
    }

    public class IntValue : ValueGeneric<int>
    {
        public IntValue(int initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(IntValue);
        }
    }

    public class DecimalValue : ValueGeneric<double>
    {
        public DecimalValue(double initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(DecimalValue);
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
    }

    public class StringValue : ValueGeneric<string>
    {
        public StringValue(string initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(StringValue);
        }
    }

    public class BoolValue : ValueGeneric<bool>
    {
        public BoolValue(bool initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(BoolValue);
        }
    }

    public class DateValue : ValueGeneric<DateTime>
    {
        public DateValue(DateTime initialValue) : base(initialValue) { }
        public static bool IsThis(ValueBase variable)
        {
            return variable.GetType() == typeof(DateValue);
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
    }

    public class ExpValue : ValueBase
    {
        private SpreadsheetBaseVisitor<bool> _visitor;
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
            set
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
        public ExpValue(SpreadsheetBaseVisitor<bool> visitor, SpreadsheetParser.ExpContext expression)
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
}
