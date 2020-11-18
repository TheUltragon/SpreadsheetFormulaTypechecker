using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public enum RelativityType
    {
        None,
        Positive,
        Negative,
    }

    public class SpreadsheetVisitor : SpreadsheetBaseVisitor<bool>
    {
        public ErrorHandler Handler { get; protected set; }
        public DataRepository Repository { get; protected set; }
        public int LastIntValue;
        public VarType LastType { get; protected set; }
        public Tuple<int, int> CurrentAddress { get; set; }
        public RelativityType LastRelativity { get; protected set; }

        public SpreadsheetVisitor(ErrorHandler handler)
        {
            Handler = handler;
            Repository = new DataRepository(this);
            LastType = VarType.None;
            CurrentAddress = null;
            LastRelativity = RelativityType.None;
        }

        public SpreadsheetVisitor(ErrorHandler handler, DataRepository repository)
        {
            Handler = handler;
            Repository = repository;
            LastType = VarType.None;
            CurrentAddress = null;
            LastRelativity = RelativityType.None;
        }
    }
}
