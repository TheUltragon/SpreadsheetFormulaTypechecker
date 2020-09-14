using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test
{
    public static class DictionaryExtensions
    {
        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
    

    class DataRepository
    {
        public Dictionary<Tuple<int, int>, CellContentBase> Cells = new Dictionary<Tuple<int, int>, CellContentBase>();
        public Type CurrentExpType;

        public CellContentBase GetCellContent(Tuple<int, int> cell)
        {
            return Cells.GetValue<Tuple<int, int>, CellContentBase>(cell);
        }
    }

    class CellContentBase { }

    class CellContent<T> : CellContentBase
    {
        public T Value { get; set; }
        CellContent(T initialValue)
        {
            Value = initialValue;
        }
    }

    class CellCopyContent : CellContentBase
    {
        private DataRepository repository;
        public CellContentBase Value {
            get
            {
                return repository.GetCellContent(Cell);
            }
        }
        public Tuple<int, int> Cell { get; set; }
        CellCopyContent(DataRepository repo, Tuple<int, int> cell)
        {
            repository = repo;
            Cell = cell;
        }
    }
}
