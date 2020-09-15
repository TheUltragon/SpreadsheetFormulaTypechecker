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


    public class DataRepository
    {
        public Dictionary<Tuple<int, int>, VariableBase> Cells = new Dictionary<Tuple<int, int>, VariableBase>();
        public Type CurrentExpType;

        public VariableBase GetCellContent(Tuple<int, int> cell)
        {
            return Cells.GetValue<Tuple<int, int>, VariableBase>(cell);
        }
    }

    
}
