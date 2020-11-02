using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public abstract class AbstractFormula
    {
        public SpreadsheetVisitor Visitor { get; set; }
        public SpreadsheetParser.ExpContext Exp { get; set; }
        public Dictionary<Tuple<int, int>, AbstractFormula> CellFormulas { get; set; }
        public AbstractFormulas Formulas { get; set; }
        public Tuple<int, int> CellIndex { get; set; }
        public AbstractFormulaNode Node { get; protected set; }

        public abstract Type ExpressionType { get; }

        public AbstractFormula() { }

        //Takes Exp and Translates it into an AbstractFormulaNode. Saves the result in Node
        public abstract bool Translate();

        //Takes Node and Simplifies it (e.g.: Int + Int = Int). Best Case would be to just get a variable type
        public void Simplify()
        {
            Node = Node.Simplify();
        }

        public override string ToString()
        {
            if(Node != null)
            {
                return Node.ToString();
            }
            else
            {
                return "Empty Abstract Formula";
            }
        }
    }

    public class AbstractVarFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.ValueExpContext);

        public override bool Translate()
        {
            Visitor.Visit(Exp);
            var type = Visitor.LastType;
            Node = new AbstractTypeNode(this, type);
            return true;
        }

        
    }

    enum Relativity
    {
        Pos, 
        Neg, 
        None
    }

    public class AbstractCellFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.CellExpContext);

        public override bool Translate()
        {
            var cellExp = (SpreadsheetParser.CellExpContext)Exp;
            Relativity leftRelative = getRelativity(cellExp.left);
            Relativity rightRelative = getRelativity(cellExp.right);

            var left = getValueExp(cellExp.left);
            var right = getValueExp(cellExp.right);
            if (left is SpreadsheetParser.ValueExpContext && right is SpreadsheetParser.ValueExpContext)
            {
                var leftParam = (SpreadsheetParser.ValueExpContext)left;
                var rightParam = (SpreadsheetParser.ValueExpContext)right;
                if (leftParam.val is SpreadsheetParser.IntValContext && rightParam.val is SpreadsheetParser.IntValContext)
                {
                    var leftVal = int.Parse(leftParam.val.GetText());
                    if(leftRelative == Relativity.Neg)
                    {
                        leftVal *= -1;
                    }
                    var rightVal = int.Parse(rightParam.val.GetText());
                    if (rightRelative == Relativity.Neg)
                    {
                        rightVal *= -1;
                    }

                    Node = new AbstractCellNode(this, new Tuple<int, int>(leftVal, rightVal), isRelative(leftRelative), isRelative(rightRelative));
                }
                else
                {
                    var text = "Typecheck Error - Cell Expression has indices of type other than int.";
                    Node = new AbstractErrorNode(this, text);
                    return false;
                }
            }
            else
            {
                var text = "Cell Expression has indices of type other than simple values - this is not supported yet.";
                Node = new AbstractErrorNode(this, text);
                return false;
            }
            return true;
        }

        private SpreadsheetParser.ExpContext getValueExp(SpreadsheetParser.AexpContext context)
        {
            if(context is SpreadsheetParser.BaseAExpContext)
            {
                return ((SpreadsheetParser.BaseAExpContext)context).param;
            }
            else if(context is SpreadsheetParser.PosAExpContext)
            {
                return ((SpreadsheetParser.PosAExpContext)context).param;
            }
            else if (context is SpreadsheetParser.NegAExpContext)
            {
                return ((SpreadsheetParser.NegAExpContext)context).param;
            }
            else
            {
                Logger.DebugLine("Error - Couldnt getValueExp from AexpContext!");
                return null;
            }
        }

        private Relativity getRelativity(SpreadsheetParser.AexpContext context)
        {
            if (context is SpreadsheetParser.BaseAExpContext)
            {
                return Relativity.None;
            }
            else if (context is SpreadsheetParser.NegAExpContext)
            {
                return Relativity.Neg;
            }
            else if (context is SpreadsheetParser.PosAExpContext)
            {
                return Relativity.Pos;
            }
            else
            {
                Logger.DebugLine("Error: Reached end of getRelativity if else");
                return Relativity.None;
            }
        }

        private bool isRelative(Relativity relative)
        {
            return relative != Relativity.None;
        }
    }

    public class AbstractAddFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.AddExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.AddExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if(successLeft && successRight)
            {
                Node = new AbstractAddNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractSubFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.SubExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.SubExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractSubNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public static class AbstractFunctionFormulaContent
    {
        private static bool initialized = false;
        private static Dictionary<Type, Type> formulas = new Dictionary<Type, Type>();
        public static Dictionary<Type, Type> Formulas
        {
            get
            {
                Init();
                return formulas;
            }
        }
        public static void Init()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            //TODO: Slow in practice (?), and only needs to be calculated once (list doesnt change)!
            var AbstractFunctionFormulaTypes = ReflectiveEnumerator.GetEnumerableOfType<AbstractFunctionFormula>();
            foreach (var tp in AbstractFunctionFormulaTypes)
            {
                Register(tp.GetType(), tp.FunctionType);
            }
        }

        public static void Register(Type functionFormulaType, Type expType)
        {
            formulas.Add(expType, functionFormulaType);
        }
    }

    public class AbstractFunctionFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.FunctionExpContext);

        private AbstractFunctionFormula AbstractExp;

        public SpreadsheetParser.FexpContext Fexp => ((SpreadsheetParser.FunctionExpContext)Exp).fun;
        

        public override bool Translate()
        {
            bool success = AbstractFunctionFormulaContent.Formulas.TryGetValue(Fexp.GetType(), out Type formulaType);
            if (success)
            {
                AbstractExp = (AbstractFunctionFormula)Activator.CreateInstance(formulaType);
                AbstractExp.Exp = Exp;
                AbstractExp.Visitor = Visitor;
                AbstractExp.CellFormulas = CellFormulas;
                AbstractExp.Formulas = Formulas;

                AbstractExp.TranslateFunction();

                Node = AbstractExp.Node;
                return true;
            }
            else
            {
                Logger.DebugLine($"Error - Couldnt Translate Function Formula, formula Type {Fexp.GetType()} not registered yet.");
                return false;
            }
        }

        public virtual Type FunctionType { get; }

        public virtual void TranslateFunction() { }
    }

    public class AbstractProductFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.ProdFuncContext);

        public override void TranslateFunction()
        {
            var prodExp = (SpreadsheetParser.ProdFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = prodExp.anyArg();
            foreach(var context in args._expr)
            {
                Logger.Debug("Translation, ");
                if(context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }
            
            if (success)
            {
                Node = new AbstractProductNode(this, formulaNodes);
            }
        }
    }


    public class AbstractSumFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.SumFuncContext);

        public override void TranslateFunction()
        {
            var sumExp = (SpreadsheetParser.SumFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = sumExp.anyArg();
            foreach (var context in args._expr)
            {
                Logger.Debug("Translation, ");
                if (context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }

            if (success)
            {
                Node = new AbstractProductNode(this, formulaNodes);
            }
        }
    }
}
