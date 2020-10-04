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
        public AbstractFormulas Formulas { get; set; }
        public AbstractFormulaNode Node { get; protected set; }

        public abstract Type ExpressionType { get; }

        public AbstractFormula() { }

        //Takes Exp and Translates it into an AbstractFormulaNode. Saves the result in Node
        public abstract void Translate();

        //Takes Node and Simplifies it (e.g.: Int + Int = Int). Best Case would be to just get a variable type
        public void Simplify()
        {
            Node = Node.Simplify();
        }
    }

    public class AbstractVarExp : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.VarExpContext);

        public override void Translate()
        {
            Visitor.Visit(Exp);
            var type = Visitor.LastType;
            Node = new AbstractTypeNode(type);
        }
    }

    public class AbstractCellExp : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.CellExpContext);

        public override void Translate()
        {
            var cellExp = (SpreadsheetParser.CellExpContext)Exp;
            bool isRelative;
            if (cellExp.left is SpreadsheetParser.BaseAExpContext && cellExp.right is SpreadsheetParser.BaseAExpContext)
            {
                isRelative = false;
            }
            else
            {
                isRelative = true;
            }

            var left = getValueExp(cellExp.left);
            var right = getValueExp(cellExp.left);
            if (left is SpreadsheetParser.ValueExpContext && right is SpreadsheetParser.ValueExpContext)
            {
                var leftParam = (SpreadsheetParser.ValueExpContext)left;
                var rightParam = (SpreadsheetParser.ValueExpContext)right;
                if (leftParam.val is SpreadsheetParser.IntValContext && rightParam.val is SpreadsheetParser.IntValContext)
                {
                    var leftVal = int.Parse(leftParam.val.GetText());
                    var rightVal = int.Parse(rightParam.val.GetText());

                    Node = new AbstractCellNode(new Tuple<int, int>(leftVal, rightVal), isRelative);
                }
                else
                {
                    var text = "Typecheck Error - Cell Expression has indices of type other than int.";
                    Node = new AbstractErrorNode(text);
                }
            }
            else
            {
                var text = "Cell Expression has indices of type other than simple values - this is not supported yet.";
                Node = new AbstractErrorNode(text);
            }
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
    }

    public class AbstractAddExp : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.AddExpContext);

        public override void Translate()
        {
            var addExp = (SpreadsheetParser.AddExpContext)Exp;
            var leftFormula = Formulas.TranslateFormula(addExp.left, out bool successLeft);
            var rightFormula = Formulas.TranslateFormula(addExp.right, out bool successRight);
            if(successLeft && successRight)
            {
                Node = new AbstractAddNode(leftFormula.Node, rightFormula.Node);
            }
        }
    }

    public abstract class AbstractFunctionExp : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.FunctionExpContext);

        Dictionary<Type, Type> formulaDict = new Dictionary<Type, Type>();
        public SpreadsheetVisitor Visitor { get; set; }
        private AbstractFunctionExp AbstractExp;

        public void Register(Type functionFormulaType, Type expType)
        {
            formulaDict.Add(expType, functionFormulaType);
        }

        public SpreadsheetParser.FexpContext Fexp
        {
            get
            {
                return ((SpreadsheetParser.FunctionExpContext)Exp).fun;
            }
        }

        public override void Translate()
        {
            //TODO: Use the right one of the registered AbstractFunctionExpressions to translate this function
            //Use AbstractExp for that
            throw new NotImplementedException();
        }

        public abstract Type FunctionType { get; }

        public abstract void TranslateFunction();
    }

    public class AbstractProductExp : AbstractFunctionExp
    {
        public override Type FunctionType => typeof(SpreadsheetParser.ProdFuncContext);

        public override void TranslateFunction()
        {
            var prodExp = (SpreadsheetParser.ProdFuncContext)Fexp;
            bool success = true;
            List<AbstractFormulaNode> formulaNodes = new List<AbstractFormulaNode>();
            foreach(var child in prodExp.children)
            {
                var formula = Formulas.TranslateFormula((SpreadsheetParser.ExpContext)child, out bool successFormula);
                formulaNodes.Add(formula.Node);
                success &= successFormula;
            }
            
            if (success)
            {
                Node = new AbstractProductNode(formulaNodes);
            }
        }
    }
}
