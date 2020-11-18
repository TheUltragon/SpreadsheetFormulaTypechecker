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

        public static string ListNodesToString(List<AbstractFormulaNode> list)
        {
            string result = "";
            bool first = true;
            foreach (var elem in list)
            {
                if (first)
                {
                    result += elem.ToString();
                    first = false;
                }
                else
                {
                    result += ", " + elem.ToString();

                }
            }
            return result;
        }
    }

    //================================
    //Basic Nodes
    //================================

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



    //================================
    //Operator Nodes
    //================================

    public abstract class AbstractOperatorNode : AbstractFormulaNode
    {
        public AbstractOperatorNode(AbstractFormula formula) : base(formula) { }
    }

    public class AbstractAddNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractAddNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, Compatibility.GetHigherType(tp1, tp2));
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, Compatibility.GetHigherType(tp1, tp2));
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


    public class AbstractMultNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractMultNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, Compatibility.GetHigherType(tp1, tp2));
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"MultNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractDivNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractDivNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (tp1.IsNumeric() && tp2.IsNumeric())
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Decimal);
                }
                else if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, Compatibility.GetHigherType(tp1, tp2));
                }

            }

            return this;
        }

        public override string ToString()
        {
            string result = $"DivNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }


    public class AbstractModNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractModNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, Compatibility.GetHigherType(tp1, tp2));
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"ModNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }


    public class AbstractSmallerNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractSmallerNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"SmallerNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractGreaterNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractGreaterNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"GreaterNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractGreaterEqNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractGreaterEqNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"GreaterEqNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractSmallerEqNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractSmallerEqNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"SmallerEqNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }


    public class AbstractEqualNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractEqualNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"EqualNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractUnequalNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractUnequalNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (Compatibility.IsCompatible(tp1, tp2))
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"UnequalNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractAndNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractAndNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (tp1 == tp2 && tp1 == VarType.Bool)
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"AndNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }

    public class AbstractOrNode : AbstractOperatorNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractOrNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
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
                if (tp1 == tp2 && tp1 == VarType.Bool)
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"OrNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }


    public class AbstractNotNode : AbstractOperatorNode
    {
        public AbstractFormulaNode Child;

        public AbstractNotNode(AbstractFormula formula, AbstractFormulaNode child) : base(formula)
        {
            Child = child;
        }

        public override AbstractFormulaNode Simplify()
        {
            var child = Child.Simplify();
            Child = child;
            if (Child is AbstractTypeNode)
            {
                VarType tp = ((AbstractTypeNode)Child).Type;
                if (tp == VarType.Bool)
                {
                    return new AbstractTypeNode(ParentFormula, VarType.Bool);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"NotNode: Child: {Child.ToString()}";
            return result;
        }
    }




    //================================
    //Function Nodes
    //================================


    public abstract class AbstractFunctionNode : AbstractFormulaNode
    {
        public AbstractFunctionNode(AbstractFormula formula) : base(formula) { }
    }

    public class AbstractProdFuncNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractProdFuncNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
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
                    if (Compatibility.IsCompatible(tp, highestType))
                    {
                        highestType = Compatibility.GetHigherType(tp, highestType);
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
            string result = "ProdFuncNode: Children: ";
            result += ListNodesToString(Children);
            return result;
        }
    }

    public class AbstractSumFuncNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractSumFuncNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
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
                    if (Compatibility.IsCompatible(tp, highestType))
                    {
                        highestType = Compatibility.GetHigherType(tp, highestType);
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
            string result = "SumFuncNode: Children: ";
            result += ListNodesToString(Children);

            return result;
        }
    }


    public class AbstractAndFuncNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractAndFuncNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
        {
            Children = children;
        }

        public override AbstractFormulaNode Simplify()
        {
            List<AbstractFormulaNode> newChildren = new List<AbstractFormulaNode>();
            bool simplifySuccess = true;
            VarType highestType = VarType.Bool;
            foreach (var child in Children)
            {
                var newChild = child.Simplify();
                newChildren.Add(newChild);
                if (newChild is AbstractTypeNode && simplifySuccess)
                {
                    var tp = ((AbstractTypeNode)newChild).Type;
                    if (Compatibility.IsCompatible(tp, highestType))
                    {
                        highestType = Compatibility.GetHigherType(tp, highestType);
                    }
                    else
                    {
                        Logger.DebugLine("Typecheck Error on Simplify And Function - Type " + tp.ToString(), 10);
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
            string result = "AndFuncNode: Children: ";
            result += ListNodesToString(Children);

            return result;
        }
    }


    public class AbstractOrFuncNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractOrFuncNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
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
                    if (Compatibility.IsCompatible(tp, highestType))
                    {
                        highestType = Compatibility.GetHigherType(tp, highestType);
                    }
                    else
                    {
                        Logger.DebugLine("Typecheck Error on Simplify Or Function - Type " + tp.ToString(), 10);
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
            string result = "OrFuncNode: Children: ";
            result += ListNodesToString(Children);

            return result;
        }
    }



    public class AbstractAverageFuncNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractAverageFuncNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
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
                    if (Compatibility.IsCompatible(tp, highestType))
                    {
                        highestType = Compatibility.GetHigherType(tp, highestType);
                    }
                    else
                    {
                        Logger.DebugLine("Typecheck Error on Simplify Average Function - Type " + tp.ToString(), 10);
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
            string result = "AverageFuncNode: Children: ";
            result += ListNodesToString(Children);

            return result;
        }
    }

    public class AbstractMaxFuncNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractMaxFuncNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
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
                    if (Compatibility.IsCompatible(tp, highestType))
                    {
                        highestType = Compatibility.GetHigherType(tp, highestType);
                    }
                    else
                    {
                        Logger.DebugLine("Typecheck Error on Simplify Max Function - Type " + tp.ToString(), 10);
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
            string result = "MaxFuncNode: Children: ";
            result += ListNodesToString(Children);

            return result;
        }
    }




    public class AbstractMinFuncNode : AbstractFunctionNode
    {
        public List<AbstractFormulaNode> Children;

        public AbstractMinFuncNode(AbstractFormula formula, List<AbstractFormulaNode> children) : base(formula)
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
                    if (Compatibility.IsCompatible(tp, highestType))
                    {
                        highestType = Compatibility.GetHigherType(tp, highestType);
                    }
                    else
                    {
                        Logger.DebugLine("Typecheck Error on Simplify Min Function - Type " + tp.ToString(), 10);
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
            string result = "MinFuncNode: Children: ";
            result += ListNodesToString(Children);

            return result;
        }
    }

    public class AbstractRoundupFuncNode : AbstractFunctionNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractRoundupFuncNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2) : base(formula)
        {
            Children = new Tuple<AbstractFormulaNode, AbstractFormulaNode> (child1, child2);
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
                if (tp1.IsNumeric() && tp2 == VarType.Int)
                {
                    return new AbstractTypeNode(ParentFormula, tp1);
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"RoundupFuncNode: Children: {Children.Item1.ToString()}, {Children.Item2.ToString()}";
            return result;
        }
    }


    public class AbstractIfFuncNode : AbstractFunctionNode
    {
        public Tuple<AbstractFormulaNode, AbstractFormulaNode, AbstractFormulaNode> Children;

        public AbstractIfFuncNode(AbstractFormula formula, AbstractFormulaNode child1, AbstractFormulaNode child2, AbstractFormulaNode child3) : base(formula)
        {
            Children = new Tuple<AbstractFormulaNode, AbstractFormulaNode, AbstractFormulaNode>(child1, child2, child3);
        }

        public override AbstractFormulaNode Simplify()
        {
            var child1 = Children.Item1.Simplify();
            var child2 = Children.Item2.Simplify();
            var child3 = Children.Item3.Simplify();
            Children = new Tuple<AbstractFormulaNode, AbstractFormulaNode, AbstractFormulaNode>(child1, child2, child3);
            if (Children.Item1 is AbstractTypeNode && Children.Item2 is AbstractTypeNode && Children.Item3 is AbstractTypeNode)
            {
                VarType tp1 = ((AbstractTypeNode)Children.Item1).Type;
                VarType tp2 = ((AbstractTypeNode)Children.Item2).Type;
                VarType tp3 = ((AbstractTypeNode)Children.Item3).Type;
                if (tp1 == VarType.Bool && Compatibility.IsCompatible(tp2, tp3))
                {
                    return new AbstractTypeNode(ParentFormula, Compatibility.GetHigherType(tp2, tp3));
                }
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"IfFuncNode: Children: If: {Children.Item1.ToString()}, Then: {Children.Item2.ToString()} Else: {Children.Item3.ToString()}";
            return result;
        }
    }


    public class AbstractIsblankFuncNode : AbstractFunctionNode
    {
        public AbstractFormulaNode Child;

        public AbstractIsblankFuncNode(AbstractFormula formula, AbstractFormulaNode child) : base(formula)
        {
            Child = child;
        }

        public override AbstractFormulaNode Simplify()
        {
            var child = Child.Simplify();
            Child = child;
            if (!(Child is AbstractErrorNode))
            {
                return new AbstractTypeNode(ParentFormula, VarType.Bool);
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"IsblankFuncNode: Child: {Child.ToString()}";
            return result;
        }
    }


    public class AbstractIsnaFuncNode : AbstractFunctionNode
    {
        public AbstractFormulaNode Child;

        public AbstractIsnaFuncNode(AbstractFormula formula, AbstractFormulaNode child) : base(formula)
        {
            Child = child;
        }

        public override AbstractFormulaNode Simplify()
        {
            var child = Child.Simplify();
            Child = child;
            if (!(Child is AbstractErrorNode))
            {
                return new AbstractTypeNode(ParentFormula, VarType.Bool);
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"IsnaFuncNode: Child: {Child.ToString()}";
            return result;
        }
    }

    public class AbstractNFuncNode : AbstractFunctionNode
    {
        public AbstractFormulaNode Child;

        public AbstractNFuncNode(AbstractFormula formula, AbstractFormulaNode child) : base(formula)
        {
            Child = child;
        }

        public override AbstractFormulaNode Simplify()
        {
            var child = Child.Simplify();
            Child = child;
            if (Child is AbstractTypeNode)
            {
                AbstractTypeNode typeNode = (AbstractTypeNode)Child;
                if(typeNode.Type == VarType.TypeError || typeNode.Type.IsNumeric()){
                    return typeNode;
                }
                return new AbstractTypeNode(ParentFormula, VarType.Int);
            }

            return this;
        }

        public override string ToString()
        {
            string result = $"NFuncNode: Child: {Child.ToString()}";
            return result;
        }
    }
}
