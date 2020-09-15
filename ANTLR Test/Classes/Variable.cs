using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test
{
    public class VariableBase { }

    public class Variable<T> : VariableBase
    {
        public T Value { get; set; }
        public Variable(T initialValue)
        {
            Value = initialValue;
        }
        public override string ToString()
        {
            return "Value: " + Value.ToString();
        }
    }
    
    public class CharVariable : Variable<char>
    {
        public CharVariable(char initialValue) : base(initialValue) { }
        public static bool IsThis(VariableBase variable)
        {
            return variable.GetType() == typeof(CharVariable);
        }
    }

    public class IntVariable : Variable<int>
    {
        public IntVariable(int initialValue) : base(initialValue) { }
        public static bool IsThis(VariableBase variable)
        {
            return variable.GetType() == typeof(IntVariable);
        }
    }

    public class DecimalVariable : Variable<double>
    {
        public DecimalVariable(double initialValue) : base(initialValue) { }
        public static bool IsThis(VariableBase variable)
        {
            return variable.GetType() == typeof(DecimalVariable);
        }
    }

    public class StringVariable : Variable<string>
    {
        public StringVariable(string initialValue) : base(initialValue) { }
        public static bool IsThis(VariableBase variable)
        {
            return variable.GetType() == typeof(StringVariable);
        }
    }

    public class BoolVariable : Variable<bool>
    {
        public BoolVariable(bool initialValue) : base(initialValue) { }
        public static bool IsThis(VariableBase variable)
        {
            return variable.GetType() == typeof(BoolVariable);
        }
    }

    public class EmptyVariable : VariableBase
    {
        public static bool IsThis(VariableBase variable)
        {
            return variable.GetType() == typeof(EmptyVariable);
        }
        public override string ToString()
        {
            return "Empty";
        }
    }

    public class CellCopyContent : VariableBase
    {
        private DataRepository repository;
        public VariableBase Value
        {
            get
            {
                return repository.GetCellContent(Cell);
            }
        }
        public Tuple<int, int> Cell { get; set; }
        public CellCopyContent(DataRepository repo, Tuple<int, int> cell)
        {
            repository = repo;
            Cell = cell;
        }
        public static bool IsThis(VariableBase variable)
        {
            return variable.GetType() == typeof(CellCopyContent);
        }
    }
}
