using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public abstract class AbstractFormula
    {
        public SpreadsheetVisitor Visitor { get; set; }
        public SpreadsheetParser.ExpContext Exp { get; set; }
        public Dictionary<Tuple<int, int>, AbstractFormula> CellFormulas { get; set; }
        public AbstracFormulaNode Node { get; private set; }

        public AbstractFormula() { }

        //Takes Exp and Translates it into an AbstractFormulaNode. Saves the result in Node
        public abstract void Translate();

        //Takes Node and Simplifies it (e.g.: Int + Int = Int). Best Case would be to just get a variable type
        public abstract void Simplify();
    }

    public class AbstractFormulas
    {
        public Dictionary<Tuple<int, int>, AbstractFormula> CellFormulas = new Dictionary<Tuple<int, int>, AbstractFormula>();
        Dictionary<Type, Type> formulaDict = new Dictionary<Type, Type>();
        public SpreadsheetVisitor Visitor { get; set; }

        public AbstractFormulas(SpreadsheetVisitor visitor)
        {
            Visitor = visitor;
            //TODO: Register all AbstractFormula Types once finished
        }

        public void Register(Type formulaType, Type expType)
        {
            formulaDict.Add(expType, formulaType);
        }
        public void AddFormula(Tuple<int, int> cell, SpreadsheetParser.ExpContext formula)
        {
            bool success = formulaDict.TryGetValue(formula.GetType(), out Type formulaType);
            if (success)
            {
                var abstractFormula = (AbstractFormula)Activator.CreateInstance(formulaType);
                abstractFormula.Exp = formula;
                abstractFormula.Visitor = Visitor;
                abstractFormula.CellFormulas = CellFormulas;

                abstractFormula.Translate();
                abstractFormula.Simplify();

                CellFormulas.Add(cell, abstractFormula);
            }
        }
    }
}
