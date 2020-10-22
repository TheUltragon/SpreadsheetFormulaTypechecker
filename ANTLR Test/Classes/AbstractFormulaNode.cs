using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public abstract class AbstractFormulaNode
    {
        public abstract AbstractFormulaNode Simplify();
    }

    public class AbstractErrorNode : AbstractFormulaNode
    {
        public string ErrorText;

        public AbstractErrorNode(string text)
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

        public AbstractTypeNode(VarType type)
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
        public bool IsRelative;

        public AbstractCellNode(Tuple<int, int> index, bool isRelative)
        {
            CellIndex = index;
            IsRelative = isRelative;
        }

        public override AbstractFormulaNode Simplify()
        {
            return this;
        }

        public override string ToString()
        {
            return $"CellNode: Index: {CellIndex.ToString()}, isRelative: {IsRelative}";
        }
    }

    public abstract class AbstractFunctionNode : AbstractFormulaNode
    {
    }

    public class AbstractProductNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractProductNode(List<AbstractFormulaNode> children)
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
                return new AbstractTypeNode(highestType);
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

        public AbstractSumNode(List<AbstractFormulaNode> children)
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
                return new AbstractTypeNode(highestType);
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
    }

    public class AbstractAddNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractAddNode(AbstractFormulaNode child1, AbstractFormulaNode child2)
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
                    return new AbstractTypeNode(tp1);
                }
                else if (tp1.IsNumeric() && tp2.IsNumeric())
                {
                    return new AbstractTypeNode(VarTypeExtensions.GetHighestNumericType(tp1, tp2));
                }
                else if (tp1.IsText() && tp2.IsText())
                {
                    //Char and Char or Char and string always returns a string
                    return new AbstractTypeNode(VarType.String);
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

        public AbstractSubNode(AbstractFormulaNode child1, AbstractFormulaNode child2)
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
                    return new AbstractTypeNode(tp1);
                }
                else if (tp1.IsNumeric() && tp2.IsNumeric())
                {
                    return new AbstractTypeNode(VarTypeExtensions.GetHighestNumericType(tp1, tp2));
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
