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


    //========================================
    // Value, Cell, Variable
    //========================================

    public class AbstractVarFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.VarExpContext);

        public override bool Translate()
        {
            //Visitor.Visit(Exp);
            //var type = Visitor.LastType;
            //Node = new AbstractTypeNode(this, type);
            Logger.DebugLine("Error: Abstracting Var Formula not defined! (Per convention of paper)", 5);
            return false;
        }
    }


    public class AbstractValueFormula : AbstractFormula
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


    //========================================
    // Operators
    //========================================

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

    public class AbstractMultFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.MultExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.MultExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractMultNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractDivFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.DivExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.DivExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractDivNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractModFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.ModExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.ModExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractModNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractSmallerFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.SmallerExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.SmallerExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractSmallerNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractGreaterFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.GreaterExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.GreaterExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractGreaterNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractSmallerEqFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.SmallerEqExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.SmallerEqExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractSmallerEqNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractGreaterEqFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.GreaterEqExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.GreaterEqExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractGreaterEqNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractEqualFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.EqualExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.EqualExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractEqualNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractUnequalFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.UnequalExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.UnequalExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractUnequalNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractOrFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.OrExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.OrExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, CellIndex, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, CellIndex, out bool successRight);
            if (successLeft && successRight)
            {
                Node = new AbstractOrNode(this, leftFormula.Node, rightFormula.Node);
                return true;
            }
            return false;
        }
    }

    public class AbstractNotFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.NotExpContext);

        public override bool Translate()
        {
            var addExp = (SpreadsheetParser.NotExpContext)Exp;
            var paramFormula = Formulas.TranslateFormula(addExp.param, CellIndex, out bool success);
            if (success)
            {
                Node = new AbstractNotNode(this, paramFormula.Node);
                return true;
            }
            return false;
        }
    }







    //========================================
    // Functions
    //========================================

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

                success &= AbstractExp.TranslateFunction();

                if (success)
                {
                    Node = AbstractExp.Node;
                }
                else
                {
                    Logger.DebugLine($"Error - Couldnt Translate Function Formula, TranslateFunction returned false.", 5);
                }
                return success;
            }
            else
            {
                Logger.DebugLine($"Error - Couldnt Translate Function Formula, formula Type {Fexp.GetType()} not registered yet.", 5);
                return false;
            }
        }

        public virtual Type FunctionType { get; }

        public virtual bool TranslateFunction() { return false;  }
    }

    public class AbstractProdFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.ProdFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.ProdFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = exp.anyArg();
            foreach(var context in args._expr)
            {
                if(context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }
            
            if (success)
            {
                Node = new AbstractProdFuncNode(this, formulaNodes);
            }

            return success;
        }
    }


    public class AbstractSumFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.SumFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.SumFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = exp.anyArg();
            foreach (var context in args._expr)
            {
                if (context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }

            if (success)
            {
                Node = new AbstractSumFuncNode(this, formulaNodes);
            }
            return success;
        }
    }

    public class AbstractOrFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.OrFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.OrFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = exp.anyArg();
            foreach (var context in args._expr)
            {
                if (context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }

            if (success)
            {
                Node = new AbstractOrFuncNode(this, formulaNodes);
            }
            return success;
        }
    }

    public class AbstractAndFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.AndFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.AndFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = exp.anyArg();
            foreach (var context in args._expr)
            {
                if (context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }

            if (success)
            {
                Node = new AbstractAndFuncNode(this, formulaNodes);
            }
            return success;
        }
    }

    public class AbstractAverageFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.AverageFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.AverageFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = exp.anyArg();
            foreach (var context in args._expr)
            {
                if (context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }

            if (success)
            {
                Node = new AbstractAverageFuncNode(this, formulaNodes);
            }
            return success;
        }
    }

    public class AbstractMinFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.MinFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.MinFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = exp.anyArg();
            foreach (var context in args._expr)
            {
                if (context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }

            if (success)
            {
                Node = new AbstractMinFuncNode(this, formulaNodes);
            }
            return success;
        }
    }

    public class AbstractMaxFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.MaxFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.MaxFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            var args = exp.anyArg();
            foreach (var context in args._expr)
            {
                if (context != null)
                {
                    var formula = Formulas.TranslateFormula(context, CellIndex, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }

            if (success)
            {
                Node = new AbstractMaxFuncNode(this, formulaNodes);
            }
            return success;
        }
    }

    public class AbstractIfFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.IfFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.IfFuncContext)Fexp;
            bool success = true;
            var args = exp.threeArg();

            var arg1 = args.first;
            var formula1 = Formulas.TranslateFormula(arg1, CellIndex, out bool successFormula);
            var node1 = formula1.Node;
            success &= successFormula;

            var arg2 = args.second;
            var formula2 = Formulas.TranslateFormula(arg2, CellIndex, out successFormula);
            var node2 = formula2.Node;
            success &= successFormula;

            var arg3 = args.third;
            var formula3 = Formulas.TranslateFormula(arg3, CellIndex, out successFormula);
            var node3 = formula3.Node;
            success &= successFormula;

            if (success)
            {
                Node = new AbstractIfFuncNode(this, node1, node2, node3);
            }
            return success;
        }
    }

    public class AbstractRoundupFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.RoundupFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.RoundupFuncContext)Fexp;
            bool success = true;
            var args = exp.twoArg();

            var arg1 = args.first;
            var formula1 = Formulas.TranslateFormula(arg1, CellIndex, out bool successFormula);
            var node1 = formula1.Node;
            success &= successFormula;

            var arg2 = args.second;
            var formula2 = Formulas.TranslateFormula(arg2, CellIndex, out successFormula);
            var node2 = formula2.Node;
            success &= successFormula;

            if (success)
            {
                Node = new AbstractRoundupFuncNode(this, node1, node2);
            }
            return success;
        }
    }

    public class AbstractIsblankFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.IsblankFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.IsblankFuncContext)Fexp;
            bool success = true;
            var args = exp.oneArg();

            var arg1 = args.exp();
            var formula1 = Formulas.TranslateFormula(arg1, CellIndex, out bool successFormula);
            var node1 = formula1.Node;
            success &= successFormula;

            if (success)
            {
                Node = new AbstractIsblankFuncNode(this, node1);
            }
            return success;
        }
    }

    public class AbstractIsnaFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.IsnaFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.IsnaFuncContext)Fexp;
            bool success = true;
            var args = exp.oneArg();

            var arg1 = args.exp();
            var formula1 = Formulas.TranslateFormula(arg1, CellIndex, out bool successFormula);
            var node1 = formula1.Node;
            success &= successFormula;

            if (success)
            {
                Node = new AbstractIsnaFuncNode(this, node1);
            }
            return success;
        }
    }


    public class AbstractNFuncFormula : AbstractFunctionFormula
    {
        public override Type FunctionType => typeof(SpreadsheetParser.NFuncContext);

        public override bool TranslateFunction()
        {
            var exp = (SpreadsheetParser.NFuncContext)Fexp;
            bool success = true;
            var args = exp.oneArg();

            var arg1 = args.exp();
            var formula1 = Formulas.TranslateFormula(arg1, CellIndex, out bool successFormula);
            var node1 = formula1.Node;
            success &= successFormula;

            if (success)
            {
                Node = new AbstractNFuncNode(this, node1);
            }
            return success;
        }
    }

}
