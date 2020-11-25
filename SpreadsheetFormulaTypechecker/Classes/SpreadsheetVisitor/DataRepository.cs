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
        private Types _type;
        public Types Type
        {
            get
            {
                if (_type.HasUndefined() && Expression != null)
                {
                    if (marked)
                    {
                        Type = new Types(VarType.Empty);
                    }
                    else
                    {
                        //Mark Cell
                        marked = true;
                        _visitor.Repository.MarkedCells.Add(this);

                        //Eval Cell
                        Logger.DebugLine($"Eval");
                        Eval();
                    }
                }
                else
                {
                    Logger.DebugLine($"_type.HasUndefined(): {_type.HasUndefined()}", 1);
                    Logger.DebugLine($"Expression != null: {Expression != null}", 1);
                }
                return _type;
            }
            private set
            {
                _type = value;
            }
        }

        public bool marked;
        public SpreadsheetParser.ExpContext Expression { get; set; }
        private SpreadsheetVisitor _visitor;
        private Tuple<int, int> _parentCell;
        public CellType(SpreadsheetVisitor visitor, Types value)
        {
            _visitor = visitor;
            Type = value;
            Expression = null;
            marked = false;
        }

        public CellType(SpreadsheetVisitor visitor, SpreadsheetParser.ExpContext expression)
        {
            _visitor = visitor;
            Type = new Types();
            Expression = expression;
            marked = false;
        }

        public CellType(SpreadsheetVisitor visitor, Types tp, SpreadsheetParser.ExpContext expression)
        {
            _visitor = visitor;
            Type = tp;
            Expression = expression;
            marked = false;
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

        internal void ResetMark()
        {
            marked = false;
        }
    }


    public class DataRepository
    {
        public Dictionary<Tuple<int, int>, CellType> CellTypes = new Dictionary<Tuple<int, int>, CellType>();
        public Dictionary<Tuple<int, int>, Types> CellTypeAssigns = new Dictionary<Tuple<int, int>, Types>();
        public Dictionary<string, Types> VariableTypes = new Dictionary<string, Types>();
        public AbstractFormulas Formulas { get; private set; }

        public List<CellType> MarkedCells = new List<CellType>();

        public DataRepository(SpreadsheetVisitor visitor)
        {
            Formulas = new AbstractFormulas(visitor);
        }

        public void ResetMarkedCells()
        {
            foreach(var cell in MarkedCells)
            {
                cell.ResetMark();
            }
            MarkedCells.Clear();
        }

        public CellType GetCellType(Tuple<int, int> cell)
        {
            Logger.DebugLine("CellType", 1);
            return CellTypes.GetValue<Tuple<int, int>, CellType>(cell);
        }

        public Types GetCellTypeAssign(Tuple<int, int> cell)
        {
            Logger.DebugLine("CellTypeAssign", 1);
            return CellTypeAssigns.GetValue<Tuple<int, int>, Types>(cell, new Types());
        }

        public Types GetCurrentCellType(Tuple<int, int> cell)
        {
            Logger.DebugLine("CurrentCellType", 1);
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
