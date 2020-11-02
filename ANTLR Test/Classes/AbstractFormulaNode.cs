using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public abstract class AbstractFormulaNode
    {
        public AbstractFormula ParentFormula;
        public AbstractFormulaNode(AbstractFormula formula)
        {
            ParentFormula = formula;
        }
        public abstract AbstractFormulaNode Simplify();
    }

    public class AbstractErrorNode : AbstractFormulaNode
    {
        public string ErrorText;

        public AbstractErrorNode(AbstractFormula formula, string text) : base(formula)
        {
            ErrorText = text;
            Logger.Debug("Created ErrorNode: " + text);
        }

        public override AbstractFormulaNode Simplify()
        {
            return this;
        }

        public override string ToString()
        {
            return $"ErrorNode: {ErrorText}";
        }
    }

    public class AbstractTypeNode : AbstractFormulaNode
    {
        public VarType Type;

        public AbstractTypeNode(AbstractFormula formula, VarType type) : base(formula)
        {
            Type = type;
        }

        public override AbstractFormulaNode Simplify()
        {
            return this;
        }

        public override string ToString()
        {
            return $"TypeNode: {Type.ToString()}";
        }
    }

    public class AbstractCellNode : AbstractFormulaNode
    {
        public Tuple<int, int> CellIndex;
        public bool IsLeftRelative;
        public bool IsRightRelative;

        public AbstractCellNode(AbstractFormula formula, Tuple<int, int> index, bool leftRelative, bool rightRelative) : base(formula)
        {
            CellIndex = index;
            IsLeftRelative = leftRelative;
            IsRightRelative = rightRelative;
        }

        public override AbstractFormulaNode Simplify()
        {
            var cellType = ParentFormula.Visitor.Repository.GetCurrentCellType(getThisCellIndex());
            if(cellType != VarType.Empty && cellType != VarType.Unknown && cellType != VarType.None)
            {
                return new AbstractTypeNode(ParentFormula, cellType);
            }
            else
            {
                return this;
            }
        }

        public override string ToString()
        {
            return $"CellNode: Index: {CellIndex.ToString()}, LeftRelative: {IsLeftRelative}, RightRelative: {IsRightRelative}";
        }

        public Tuple<int, int> getThisCellIndex()
        {
            var addLeft = IsLeftRelative ? ParentFormula.CellIndex.Item1 : 0;
            var addRight = IsRightRelative ? ParentFormula.CellIndex.Item2 : 0;
            Tuple<int, int> index = new Tuple<int, int>(CellIndex.Item1 + addLeft, CellIndex.Item2 + addRight);
            return index;
           
        }
    }

    public abstract class AbstractFunctionNode : AbstractFormulaNode
    {
        public AbstractFunctionNode(AbstractFormula formula) : base(formula) { }
    }

    public class AbstractProductNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractProductNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
        {
            Children = children;
        }

        public override AbstractFormulaNode Simplify()
        {
            List<AbstractFormulaNode> newChildren = new List<AbstractFormulaNode>();
            bool simplifySuccess = true;
            VarType highestType = VarType.Int;
            foreach (var child in Children)
            {
                var newChild = child.Simplify();
                newChildren.Add(newChild);
                if(newChild is AbstractTypeNode && simplifySuccess)
                {
                    var tp = ((AbstractTypeNode)newChild).Type;
                    if (VarTypeExtensions.IsNumeric(tp)){
                        var originalHighest = highestType;
                        highestType = VarTypeExtensions.GetHighestNumericType(tp, originalHighest);
                    }
                    else
                    {
                        Logger.DebugLine("Typecheck Error on Simplify Product Function - Type " + tp.ToString(), 10);
                        simplifySuccess = false;
                    }
                }
                else
                {
                    simplifySuccess = false;
                }
            }

            //If all children were AbstractTypeNodes with numeric type, then this node can be simplified to the highest of these types
            if(simplifySuccess)
            {
                return new AbstractTypeNode(ParentFormula, highestType);
            }
            else
            {
                Children = newChildren;
                return this;
            }
        }

        public override string ToString()
        {
            string result = "ProductNode: Children: ";
            foreach(var child in Children)
            {
                result += child.ToString() + ", ";
            }

            return result;
        }
    }

    public class AbstractSumNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractSumNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
        {
            Children = children;
        }

        public override AbstractFormulaNode Simplify()
        {
            List<AbstractFormulaNode> newChildren = new List<AbstractFormulaNode>();
            bool simplifySuccess = true;
            VarType highestType = VarType.Int;
            foreach (var child in Children)
            {
                var newChild = child.Simplify();
                newChildren.Add(newChild);
                if (newChild is AbstractTypeNode && simplifySuccess)
                {
                    var tp = ((AbstractTypeNode)newChild).Type;
                    if (VarTypeExtensions.IsNumeric(tp))
                    {
                        var originalHighest = highestType;
                        highestType = VarTypeExtensions.GetHighestNumericType(tp, originalHighest);
                    }
                    else
                    {
                        Logger.DebugLine("Typecheck Error on Simplify Sum Function - Type " + tp.ToString(), 10);
                        simplifySuccess = false;
                    }
                }
                else
                {
                    simplifySuccess = false;
                }
            }

            //If all children were AbstractTypeNodes with numeric type, then this node can be simplified to the highest of these types
            if (simplifySuccess)
            {
                return new AbstractTypeNode(ParentFormula, highestType);
            }
            else
            {
                Children = newChildren;
                return this;
            }
        }

        public override string ToString()
        {
            string result = "SumNode: Children: ";
            foreach (var child in Children)
            {
                result += child.ToString() + ", ";
            }

            return result;
        }
    }




    public abstract class AbstractOperatorNode : AbstractFormulaNode
    {
        public AbstractOperatorNode(AbstractFormula formula) : base(formula) { }
    }

    public class AbstractAddNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractAddNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
        {
            Children = new Tuple<AbstractFormulaNode, AbstractFormulaNode>(child1,child2);
        }

        public override AbstractFormulaNode Simplify()
        {
            var child1 = Children.Item1.Simplify();
            var child2 = Children.Item2.Simplify();
            Children = new Tuple<AbstractFormulaNode, AbstractFormulaNode>(child1, child2);
            if(Children.Item1 is AbstractTypeNode && Children.Item2 is AbstractTypeNode)
            {
                VarType tp1 = ((AbstractTypeNode)Children.Item1).Type;
                VarType tp2 = ((AbstractTypeNode)Children.Item2).Type;
                if(tp1 == tp2)
                {
                    return new AbstractTypeNode(ParentFormula, tp1);
                }
                else if (tp1.IsNumeric() && tp2.IsNumeric())
                {
                    return new AbstractTypeNode(ParentFormula, VarTypeExtensions.GetHighestNumericType(tp1, tp2));
                }
                else if (tp1.IsText() && tp2.IsText())
                {
                    //Char and Char or Char and string always returns a string
                    return new AbstractTypeNode(ParentFormula, VarType.String);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"AddNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractSubNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractSubNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
        {
            Children = new Tuple<AbstractFormulaNode, AbstractFormulaNode>(child1, child2);
        }

        public override AbstractFormulaNode Simplify()
        {
            var child1 = Children.Item1.Simplify();
            var child2 = Children.Item2.Simplify();
            Children = new Tuple<AbstractFormulaNode, AbstractFormulaNode>(child1, child2);
            if (Children.Item1 is AbstractTypeNode && Children.Item2 is AbstractTypeNode)
            {
                VarType tp1 = ((AbstractTypeNode)Children.Item1).Type;
                VarType tp2 = ((AbstractTypeNode)Children.Item2).Type;
                if (tp1 == tp2)
                {
                    return new AbstractTypeNode(ParentFormula, tp1);
                }
                else if (tp1.IsNumeric() && tp2.IsNumeric())
                {
                    return new AbstractTypeNode(ParentFormula, VarTypeExtensions.GetHighestNumericType(tp1, tp2));
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"SubNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }
}
