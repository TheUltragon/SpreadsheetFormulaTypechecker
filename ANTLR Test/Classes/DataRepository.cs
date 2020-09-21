using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public static class DictionaryExtensions
    {
        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }

    public class CellValue
    {
        private ValueBase _value;
        public ValueBase Value {
            get
            {
                if(_value == null && Expression != null)
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
        public SpreadsheetParser.ExpContext Expression { get; set; }
        private SpreadsheetVisitor _visitor;
        public CellValue(SpreadsheetVisitor visitor, ValueBase value)
        {
            _visitor = visitor;
            Value = value;
            Expression = null;
        }

        public CellValue(SpreadsheetVisitor visitor, SpreadsheetParser.ExpContext expression)
        {
            _visitor = visitor;
            Value = null;
            Expression = expression;
        }

        public CellValue(SpreadsheetVisitor visitor, ValueBase value, SpreadsheetParser.ExpContext expression)
        {
            _visitor = visitor;
            Value = value;
            Expression = expression;
        }

        public void Eval()
        {
            if(Expression != null)
            {
                _visitor.Visit(Expression);
                Value = _visitor.LastExpValue;
            }
        }
    }

    public class CellType
    {
        private VarType _type;
        public VarType Type
        {
            get
            {
                if (_type == VarType.None && Expression != null)
                {
                    Eval();
                }
                return _type;
            }
            private set
            {
                _type = value;
            }
        }
        public SpreadsheetParser.ExpContext Expression { get; set; }
        private SpreadsheetVisitor _visitor;
        public CellType(SpreadsheetVisitor visitor, VarType value)
        {
            _visitor = visitor;
            Type = value;
            Expression = null;
        }

        public CellType(SpreadsheetVisitor visitor, SpreadsheetParser.ExpContext expression)
        {
            _visitor = visitor;
            Type = VarType.None;
            Expression = expression;
        }

        public CellType(SpreadsheetVisitor visitor, VarType tp, SpreadsheetParser.ExpContext expression)
        {
            _visitor = visitor;
            Type = tp;
            Expression = expression;
        }

        public void Eval()
        {
            if (Expression != null)
            {
                _visitor.Visit(Expression);
                Type = _visitor.LastType;
            }
        }
    }


    public class DataRepository
    {
        public Dictionary<Tuple<int, int>, CellValue> Cells = new Dictionary<Tuple<int, int>, CellValue>();
        public Dictionary<Tuple<int, int>, CellType> CellTypes = new Dictionary<Tuple<int, int>, CellType>();
        public Dictionary<string, ValueBase> Variables = new Dictionary<string, ValueBase>();
        public Dictionary<string, VarType> VariableTypes = new Dictionary<string, VarType>();
        public Type CurrentExpType;

        public CellValue GetCellContent(Tuple<int, int> cell)
        {
            return Cells.GetValue<Tuple<int, int>, CellValue>(cell);
        }

        public CellType GetCellType(Tuple<int, int> cell)
        {
            return CellTypes.GetValue<Tuple<int, int>, CellType>(cell);
        }
    }

    
}
