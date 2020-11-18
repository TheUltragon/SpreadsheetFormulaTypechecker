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
        private Tuple<int, int> _parentCell;
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
        public void SetParentCell(Tuple<int, int> address)
        {
            _parentCell = address;
        }
        public void Eval()
        {
            if (Expression != null)
            {
                _visitor.CurrentAddress = _parentCell;
                _visitor.Visit(Expression);
                Type = _visitor.LastType;
            }
        }
    }


    public class DataRepository
    {
        public Dictionary<Tuple<int, int>, CellType> CellTypes = new Dictionary<Tuple<int, int>, CellType>();
        public Dictionary<Tuple<int, int>, VarType> CellTypeAssigns = new Dictionary<Tuple<int, int>, VarType>();
        public Dictionary<string, VarType> VariableTypes = new Dictionary<string, VarType>();
        public AbstractFormulas Formulas { get; private set; }

        public DataRepository(SpreadsheetVisitor visitor)
        {
            Formulas = new AbstractFormulas(visitor);
        }

        public CellType GetCellType(Tuple<int, int> cell)
        {
            return CellTypes.GetValue<Tuple<int, int>, CellType>(cell);
        }

        public VarType GetCellTypeAssign(Tuple<int, int> cell)
        {
            return CellTypeAssigns.GetValue<Tuple<int, int>, VarType>(cell, VarType.Empty);
        }

        public VarType GetCurrentCellType(Tuple<int, int> cell)
        {
            var tp = GetCellType(cell);
            if(tp != null)
            {
                return tp.Type;
            }
            else
            {
                return GetCellTypeAssign(cell);
            }
        }
    }

    
}
