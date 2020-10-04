using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
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
            var abstractFormula = TranslateFormula(formula, out bool success);
            if (success)
            {
                CellFormulas.Add(cell, abstractFormula);
            }
        }

        public AbstractFormula TranslateFormula(SpreadsheetParser.ExpContext formula, out bool success)
        {
            success = formulaDict.TryGetValue(formula.GetType(), out Type formulaType);
            if (success)
            {
                var abstractFormula = (AbstractFormula)Activator.CreateInstance(formulaType);
                abstractFormula.Exp = formula;
                abstractFormula.Visitor = Visitor;
                abstractFormula.CellFormulas = CellFormulas;
                abstractFormula.Formulas = this;

                abstractFormula.Translate();
                //abstractFormula.Simplify();   //Simplify is own function, dont partially simplify throughout translation (might be performance boost though)
                return abstractFormula;
            }
            else
            {
                Logger.DebugLine($"Error - Couldnt Translate Formula, formula Type {formula.GetType()} not registered yet.");
                return null;
            }
        }
    }
}
