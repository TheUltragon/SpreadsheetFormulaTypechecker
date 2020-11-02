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

            //Register all AbstractFormula derivates
            //TODO: Slow in practice (?), and only needs to be calculated once (list doesnt change)!
            var AbstractFunctionFormulaTypes = ReflectiveEnumerator.GetEnumerableOfType<AbstractFormula>();
            foreach (var tp in AbstractFunctionFormulaTypes)
            {
                Register(tp.GetType(), tp.ExpressionType);
            }
        }

        public void Register(Type formulaType, Type expType)
        {
            formulaDict.Add(expType, formulaType);
        }
        public void AddFormula(Tuple<int, int> cell, SpreadsheetParser.ExpContext formula)
        {
            var abstractFormula = TranslateFormula(formula, cell, out bool success);
            if (success)
            {
                Logger.DebugLine($"Registered abstractFormula {abstractFormula.ToString()}");
                abstractFormula.Simplify();
                Logger.DebugLine($"Simplified: {abstractFormula.ToString()}");

                CellFormulas.Add(cell, abstractFormula);
            }
        }

        public void Simplify()
        {
            foreach(var formula in CellFormulas.Values)
            {
                formula.Simplify();
            }
        }

        public AbstractFormula TranslateFormula(SpreadsheetParser.ExpContext formula, Tuple<int, int> cellIndex, out bool success)
        {
            if(formula == null)
            {
                Logger.DebugLine($"Error - Formula was null.");
                success = false;
                return null;
            }
            success = formulaDict.TryGetValue(formula.GetType(), out Type formulaType);
            if (success)
            {
                var abstractFormula = (AbstractFormula)Activator.CreateInstance(formulaType);
                abstractFormula.Exp = formula;
                abstractFormula.Visitor = Visitor;
                abstractFormula.CellFormulas = CellFormulas;
                abstractFormula.Formulas = this;
                abstractFormula.CellIndex = cellIndex;

                bool result = abstractFormula.Translate();
                if (result)
                {
                    //abstractFormula.Simplify();   //Simplify is own function, dont partially simplify throughout translation (might be performance boost though)
                    return abstractFormula;
                }
                else
                {
                    success = false;
                    return null;
                }
            }
            else
            {
                Logger.DebugLine($"Error - Couldnt Translate Formula, formula Type {formula.GetType()} not registered yet.");
                return null;
            }
        }
    }
}
