﻿using System;
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

        public override void Translate()
        {
            Visitor.Visit(Exp);
            var type = Visitor.LastType;
            Node = new AbstractTypeNode(type);
        }

        
    }

    public class AbstractCellFormula : AbstractFormula
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
            var right = getValueExp(cellExp.right);
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

    public class AbstractAddFormula : AbstractFormula
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

    public abstract class AbstractFunctionFormula : AbstractFormula
    {
        public override Type ExpressionType => typeof(SpreadsheetParser.FunctionExpContext);
        private bool initialized = false;

        private AbstractFunctionFormula AbstractExp;

        public SpreadsheetParser.FexpContext Fexp => ((SpreadsheetParser.FunctionExpContext)Exp).fun;
        

        public override void Translate()
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
            }
            else
            {
                Logger.DebugLine($"Error - Couldnt Translate Function Formula, formula Type {Fexp.GetType()} not registered yet.");
                return;
            }
        }

        public abstract Type FunctionType { get; }

        public abstract void TranslateFunction();
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
            for(int i= 0; i<args.ChildCount; i++)
            {
                var context = args.GetRuleContext<SpreadsheetParser.ExpContext>(i);
                Logger.Debug("Translation, ");
                if(context != null)
                {
                    var formula = Formulas.TranslateFormula(context, out bool successFormula);
                    formulaNodes.Add(formula.Node);
                    success &= successFormula;
                }
            }
            
            if (success)
            {
                Node = new AbstractProductNode(formulaNodes);
            }
        }
    }
}
