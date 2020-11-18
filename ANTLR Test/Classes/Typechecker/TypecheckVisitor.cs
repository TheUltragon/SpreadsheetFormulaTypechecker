using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLR_Test.Classes
{
    public class TypecheckVisitor : SpreadsheetVisitor
    {

        // ==========================================
        // CSTR
        // ==========================================

        public TypecheckVisitor(ErrorHandler handler) : base(handler) { }

        public TypecheckVisitor(ErrorHandler handler, DataRepository repository) : base(handler, repository) { }

        // ==========================================
        // SpreadSheet Visit
        // ==========================================

        public override bool VisitSpreadSheet([NotNull] SpreadsheetParser.SpreadSheetContext context)
        {
            Logger.DebugLine("Visit SpreadSheet");
            bool result = true;
            foreach (var child in context._statements)
            {
                result &= Visit(child);
               
            }
            return result;
        }


        // ==========================================
        // Statements
        // ==========================================

        public override bool VisitCellTypeStm([NotNull] SpreadsheetParser.CellTypeStmContext context)
        {
            Logger.DebugLine("Visit Cell Type Stm");
            LastType = VarType.Unknown;
            bool result = true;

            result &= Visit(context.left);
            var leftType = LastType;
            var leftVal = LastIntValue;

            result &= Visit(context.right);
            var rightType = LastType;
            var rightVal = LastIntValue;


            if (leftType == rightType && leftType == VarType.Int)
            {
                int left = leftVal;
                int right = rightVal;
                CurrentAddress = new Tuple<int, int>(left, right);

                result &= Visit(context.tp);
                var contentType = LastType;

                Logger.DebugLine($"Add Cell Type {left}, {right}, {contentType.ToString()}");
                Repository.CellTypeAssigns[CurrentAddress] = contentType;
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    false,
                    ErrorType.CellAdressWrongType,
                    $"Cell Type Stm Adress not integer types {leftType.ToString()} and {rightType.ToString()}",
                    $"Cell Statement has Adress expressions not of type int! Found expressions: {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}"
                );
            }

            return result;
        }


        //Value assignment to Cell
        public override bool VisitCellValueStm([NotNull] SpreadsheetParser.CellValueStmContext context)
        {
            Logger.DebugLine("Visit Cell Value Stm");
            LastType = VarType.Unknown;
            bool result = true;

            result &= Visit(context.left);
            var leftType = LastType;
            var leftVal = LastIntValue;

            result &= Visit(context.right);
            var rightType = LastType;
            var rightVal = LastIntValue;

            if (leftType == rightType && leftType == VarType.Int)
            {
                int left = leftVal;
                int right = rightVal;
                var address = new Tuple<int, int>(left, right);
                CurrentAddress = address;
                result &= Visit(context.content);

                if (Repository.CellTypeAssigns.TryGetValue(address, out VarType type) && type != LastType)
                {
                    result = Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        ErrorType.CellWrongType,
                        $"Cell Value Stm has different type {type.ToString()} assigned than {LastType.ToString()}",
                        $"Cell has Type {type.ToString()} assigned, but now gets assigned an expression {context.content.GetText()} of type {LastType.ToString()}"
                    );
                }
                if (result)
                {
                    var contentType = new CellType(this, LastType);
                    contentType.SetParentCell(address);

                    Logger.DebugLine($"Add Cell Value {left}, {right}, {contentType.ToString()}");
                    Repository.CellTypes[address] = contentType;
                    Repository.Formulas.RemoveFormula(address);
                }
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    false, 
                    ErrorType.CellAdressWrongType, 
                    $"Cell Eq Stm Adress not integer types {leftType.ToString()} and {rightType.ToString()}", 
                    $"Cell Statement has Adress expressions not of type int! Found expressions: {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}"
                );
            }

            return result;
        }

        //Formula assignment to Cell
        public override bool VisitCellFormulaStm([NotNull] SpreadsheetParser.CellFormulaStmContext context)
        {
            Logger.DebugLine("Visit Cell Formula Stm");
            LastType = VarType.Unknown;
            bool result = true;

            result &= Visit(context.left);
            var leftType = LastType;
            var leftVal = LastIntValue;

            result &= Visit(context.right);
            var rightType = LastType;
            var rightVal = LastIntValue;


            if (leftType == rightType && leftType == VarType.Int)
            {
                int left = leftVal;
                int right = rightVal;
                var address = new Tuple<int, int>(left, right);
                CurrentAddress = address;

                var contentType = new CellType(this, context.content);
                contentType.SetParentCell(address);

                Logger.DebugLine($"Add Cell Formula {left}, {right}, {contentType.ToString()}");
                Repository.CellTypes[address] = contentType;
                Repository.Formulas.AddFormula(address, context.content);
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    false, 
                    ErrorType.CellAdressWrongType, 
                    $"Cell Stm Adress not integer types {leftType.ToString()} and {rightType.ToString()}", 
                    $"Cell Equal Statement has Adress expressions not of type int! Found expressions: {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}"
                );
            }
            return result;
        }

        public override bool VisitEvalStm([NotNull] SpreadsheetParser.EvalStmContext context)
        {
            CurrentAddress = null;
            Logger.DebugLine("Visit Eval Stm");
            LastType = VarType.Unknown;
            bool result = true;
            return result;
        }

        public override bool VisitEmptyStm([NotNull] SpreadsheetParser.EmptyStmContext context)
        {
            CurrentAddress = null;
            Logger.DebugLine("Visit Empty Stm");
            LastType = VarType.Unknown;
            return true;
        }

        public override bool VisitAssignStm([NotNull] SpreadsheetParser.AssignStmContext context)
        {
            CurrentAddress = null;
            Logger.DebugLine("Visit Assign Stm");
            LastType = VarType.Unknown;
            bool result = true;

            //Type
            LastType = VarType.None;
            result &= Visit(context.type());
            VarType varType = LastType;

            //Identifier
            string id = context.IDENT().GetText();

            //Expression
            LastType = VarType.None;
            result &= Visit(context.exp());
            VarType expType = LastType;

            //Check, wether assignment declared type matches expression type and are not None or other unallowed type
            bool resultCheckTypes = varType == expType;
            bool resultCheckTypeNotNone = varType != VarType.None && varType != VarType.Unknown && varType != VarType.Empty;
            if (!resultCheckTypes)
            {
                resultCheckTypes = Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    true, 
                    ErrorType.IncompatibleTypesAssignment, 
                    $"Assignment incompatible types {varType.ToString()} and {expType.ToString()}", 
                    $"The variable of this assignment expects type {varType.ToString()} but was assigned an expression {context.exp().GetText()} of type {expType.ToString()}."
                    );
            }
            if (!resultCheckTypeNotNone)
            {
                resultCheckTypeNotNone = Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    false, 
                    ErrorType.ExpectedOtherType, 
                    $"Assignment not type None", 
                    $"The assignment has assigned {id} an expression {context.exp().GetText()} with unallowed type {varType.ToString()}."
                    );
            }
            result &= resultCheckTypes && resultCheckTypeNotNone;
            if (result)
            {
                //Add var of id with value lastExp to DataRepository
                Repository.VariableTypes.Add(id, expType);
            }

            return result;
        }

        public override bool VisitIfStm([NotNull] SpreadsheetParser.IfStmContext context)
        {
            CurrentAddress = null;
            Logger.DebugLine("Visit If Stm");
            LastType = VarType.Unknown;
            bool result = true;

            result &= Visit(context.check);
            bool resultCheckType = LastType == VarType.Bool;
            if (!resultCheckType)
            {
                resultCheckType = Handler.ThrowError(
                    context.check.Start.Line, 
                    context.check.Start.Column, 
                    false, 
                    ErrorType.ExpectedOtherType, 
                    $"If Stm check type {LastType.ToString()}", 
                    $"The check expression {context.check.GetText()} of this if clause is of type {LastType.ToString()} instead of bool.");
            }
            result &= resultCheckType;

            result &= Visit(context.falseStm);
            result &= Visit(context.trueStm);

            return result;
        }

        public override bool VisitWhileStm([NotNull] SpreadsheetParser.WhileStmContext context)
        {
            CurrentAddress = null;
            Logger.DebugLine("Visit While Stm");
            LastType = VarType.Unknown;
            bool result = true;

            result &= Visit(context.check);

            bool resultCheckType = LastType == VarType.Bool;
            if(!resultCheckType)
            {
                resultCheckType = Handler.ThrowError(
                    context.check.Start.Line, 
                    context.check.Start.Column, 
                    false, 
                    ErrorType.ExpectedOtherType, 
                    $"While Stm check type {LastType.ToString()}", 
                    $"The check expression {context.check.GetText()} of this while clause is of type {LastType.ToString()} instead of bool."
                    );
            }
            result &= resultCheckType;

            result &= Visit(context.loopStm);

            return result;
        }

        // ==========================================
        // Expressions
        // ==========================================

        public override bool VisitExp([NotNull] SpreadsheetParser.ExpContext context)
        {
            return base.VisitExp(context);
        }

        public override bool VisitAddExp([NotNull] SpreadsheetParser.AddExpContext context)
        {
            Logger.DebugLine("Visit Add Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if(leftType == rightType && (leftType.IsNumeric() || leftType == VarType.Date || leftType == VarType.Currency)){
                LastType = leftType;
            }
            else if(leftType.IsNumeric() && rightType.IsNumeric())
            {
                LastType = VarTypeExtensions.GetHighestNumericType(leftType, rightType);
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    true, 
                    ErrorType.IncompatibleTypesExpression, 
                    $"Add Exp incompatible types {leftType.ToString()} and {rightType.ToString()}", 
                    $"This addition expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitAndExp([NotNull] SpreadsheetParser.AndExpContext context)
        {
            Logger.DebugLine("Visit And Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && leftType == VarType.Bool)
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"And Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This and expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        

        public override bool VisitCellExp([NotNull] SpreadsheetParser.CellExpContext context)
        {
            Logger.DebugLine("Visit Cell Exp");
            bool result = true;

            
            result &= Visit(context.left);
            var leftType = LastType;
            var leftVal = LastIntValue;
            var leftRelativity = LastRelativity;

            result &= Visit(context.right);
            var rightType = LastType;
            var rightVal = LastIntValue;
            var rightRelativity = LastRelativity;

            if (leftType == rightType && leftType == VarType.Int)
            {
                int left = leftVal;
                int right = rightVal;
                Tuple<int,int> address = CalculateAddress(context, CurrentAddress, left, leftRelativity, right, rightRelativity, out bool resultTemp);
                result &= resultTemp;
                if(Repository.CellTypes.TryGetValue(address, out CellType type))
                {
                    LastType = type.Type;
                }
                else
                {
                    LastType = VarType.Empty;
                }
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    false,
                    ErrorType.CellAdressWrongType,
                    $"Cell Exp Adress not integer types {leftType.ToString()} and {rightType.ToString()}",
                    $"Cell Expression has Adress expressions not of type int! Found expressions: {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}"
                );
                LastType = VarType.TypeError;
            }

            return result;
        }
        private enum AddressType
        {
            Left,
            Right
        }
        private Tuple<int, int> CalculateAddress(SpreadsheetParser.CellExpContext context, Tuple<int, int> currentAddress, int left, RelativityType leftRelativity, int right, RelativityType rightRelativity, out bool result)
        {
            result = true;
            bool resultTemp;
            int leftVal = CalculateAddress(context, currentAddress, left, leftRelativity, AddressType.Left, out resultTemp);
            result &= resultTemp;
            int rightVal = CalculateAddress(context, currentAddress, right, rightRelativity, AddressType.Right, out resultTemp);
            result &= resultTemp;
            return new Tuple<int, int>(leftVal, rightVal);
        }

        private int CalculateAddress(SpreadsheetParser.CellExpContext context, Tuple<int, int> currentAddress, int value, RelativityType relativity, AddressType type, out bool result)
        {
            if(relativity == RelativityType.None)
            {
                result = true;
                return value;
            }
            else if(currentAddress != null)
            {
                int addressValue = currentAddress.Item1;
                if(type == AddressType.Right)
                {
                    addressValue = currentAddress.Item2;
                }
                if(relativity == RelativityType.Positive)
                {
                    result = true;
                    return addressValue + value;
                }
                else if(relativity == RelativityType.Negative)
                {
                    result = true;
                    return addressValue - value;
                }
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.CellAdressRelativeNotFound,
                    $"Expected Base Cell Adress",
                    $"The Cell Expression had a relative cell address, but it wasnt part of a cell statement to know which address it should be relative to."
                );
                return value;
            }

            Logger.DebugLine("Error: CalculateAddress fallthrough");
            result = false;
            return value;
        }

        public override bool VisitConcatExp([NotNull] SpreadsheetParser.ConcatExpContext context)
        {
            Logger.DebugLine("Visit Concat Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && leftType == VarType.String)
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"Concat Exp unexpected types {leftType.ToString()} and {rightType.ToString()}",
                    $"This Concat expression expected 2 string types but got expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitDivExp([NotNull] SpreadsheetParser.DivExpContext context)
        {
            Logger.DebugLine("Visit Div Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && (leftType.IsNumeric() ||  leftType == VarType.Currency))
            {
                LastType = leftType;
            }
            else if (leftType.IsNumeric() && rightType.IsNumeric())
            {
                LastType = VarType.Decimal;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Div Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This Division expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitEqualExp([NotNull] SpreadsheetParser.EqualExpContext context)
        {
            Logger.DebugLine("Visit Equal Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType)
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Equal Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This equality expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitGreaterEqExp([NotNull] SpreadsheetParser.GreaterEqExpContext context)
        {
            Logger.DebugLine("Visit Greater Eq Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && (leftType.IsNumeric() || leftType == VarType.Date || leftType == VarType.Currency))
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Greater Eq Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This greater equal expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitGreaterExp([NotNull] SpreadsheetParser.GreaterExpContext context)
        {
            Logger.DebugLine("Visit Greater Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && (leftType.IsNumeric() || leftType == VarType.String || leftType == VarType.Date || leftType == VarType.Currency))
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Greater Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This greater expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitModExp([NotNull] SpreadsheetParser.ModExpContext context)
        {
            Logger.DebugLine("Visit Mod Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && leftType.IsNumeric())
            {
                LastType = leftType;
            }
            else if (leftType.IsNumeric() && rightType.IsNumeric())
            {
                LastType = VarTypeExtensions.GetHighestNumericType(leftType, rightType);
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Greater Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This greater expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitMultExp([NotNull] SpreadsheetParser.MultExpContext context)
        {
            Logger.DebugLine("Visit Mult Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && (leftType.IsNumeric() || leftType == VarType.Currency))
            {
                LastType = leftType;
            }
            else if (leftType.IsNumeric() && rightType.IsNumeric())
            {
                LastType = VarTypeExtensions.GetHighestNumericType(leftType, rightType);
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Mult Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This multiplication expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitNotExp([NotNull] SpreadsheetParser.NotExpContext context)
        {
            Logger.DebugLine("Visit Not Exp");
            var result = Visit(context.param);
            var leftType = LastType;

            if (leftType == VarType.Bool)
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Not Exp incompatible types {leftType.ToString()}",
                    $"This not expression has the incompatible type {leftType.ToString()}. It expected 1 bool type."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitOrExp([NotNull] SpreadsheetParser.OrExpContext context)
        {
            Logger.DebugLine("Visit Or Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && leftType == VarType.Bool)
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Or Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This or expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitSmallerEqExp([NotNull] SpreadsheetParser.SmallerEqExpContext context)
        {
            Logger.DebugLine("Visit Smaller Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && (leftType.IsNumeric() || leftType == VarType.Date || leftType == VarType.Currency))
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Smaller Eq Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This smaller equal expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitSmallerExp([NotNull] SpreadsheetParser.SmallerExpContext context)
        {
            Logger.DebugLine("Visit Smaller Eq Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && (leftType.IsNumeric() || leftType == VarType.Date || leftType == VarType.Currency))
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Smaller Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This smaller expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitSubExp([NotNull] SpreadsheetParser.SubExpContext context)
        {
            Logger.DebugLine("Visit Sub Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && (leftType.IsNumeric()  || leftType == VarType.Date || leftType == VarType.Currency))
            {
                LastType = leftType;
            }
            else if (leftType.IsNumeric() && rightType.IsNumeric())
            {
                LastType = VarTypeExtensions.GetHighestNumericType(leftType, rightType);
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Sub Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This subtraction expression has 2 incompatible expressions attached with expressions {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitUnequalExp([NotNull] SpreadsheetParser.UnequalExpContext context)
        {
            Logger.DebugLine("Visit Unequal Exp");
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType)
            {
                LastType = leftType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"UnEqual Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This inequality expression has 2 incompatible expressions attached with types {context.left.GetText()} : {leftType.ToString()} and {context.right.GetText()} : {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitVarExp([NotNull] SpreadsheetParser.VarExpContext context)
        {
            Logger.DebugLine("Visit Var Exp");
            //Identifier
            string id = context.IDENT().GetText();
            VarType type = VarType.None;
            bool result = Repository.VariableTypes.TryGetValue(id, out type);
            if (result)
            {
                LastType = type;
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.VariableNotDeclared,
                    $"Var not declared yet",
                    $"The variable with identifier {id} hasnt been declared prior to its useage here."
                );
                LastType = VarType.TypeError;
            }
            return result;
        }



        // ==========================================
        // Additive Expressions
        // ==========================================

        public override bool VisitBaseAExp([NotNull] SpreadsheetParser.BaseAExpContext context)
        {
            Logger.DebugLine("Visit Base A Exp");
            LastRelativity = RelativityType.None;
            bool result = true;
            Visit(context.param);
            VarType type = LastType;
            result = VarTypeExtensions.IsNumeric(type);
            if (result)
            {
                LastType = type;
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"AExp unexpected type {type.ToString()}",
                    $"Expected a numeric expression in base additive expression, but got expression {context.param.GetText()} : {type.ToString()} instead."
                );
                LastType = VarType.TypeError;
            }
            return result;
        }

        public override bool VisitNegAExp([NotNull] SpreadsheetParser.NegAExpContext context)
        {
            Logger.DebugLine("Visit Neg A Exp");
            LastRelativity = RelativityType.Negative;
            bool result = true;
            Visit(context.param);
            VarType type = LastType;
            result = VarTypeExtensions.IsNumeric(type);
            if (result)
            {
                LastType = type;
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"AExp unexpected type {type.ToString()}",
                    $"Expected a numeric expression in negative additive expression, but got expression {context.param.GetText()} : {type.ToString()} instead."
                );
                LastType = VarType.TypeError;
            }
            return result;
        }

        public override bool VisitPosAExp([NotNull] SpreadsheetParser.PosAExpContext context)
        {
            Logger.DebugLine("Visit Pos A Exp");
            LastRelativity = RelativityType.Positive;
            bool result = true;
            Visit(context.param);
            VarType type = LastType;
            result = VarTypeExtensions.IsNumeric(type);
            if (result)
            {
                LastType = type;
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"AExp unexpected type {type.ToString()}",
                    $"Expected a numeric expression in positive additive expression, but got expression {context.param.GetText()} : {type.ToString()} instead."
                );
                LastType = VarType.TypeError;
            }
            return result;
        }

        



        // ==========================================
        // Types
        // ==========================================

        public override bool VisitBoolTp([NotNull] SpreadsheetParser.BoolTpContext context)
        {
            LastType = VarType.Bool;
            return true;
        }


        public override bool VisitCurrencyTp([NotNull] SpreadsheetParser.CurrencyTpContext context)
        {
            LastType = VarType.Currency;
            return true;
        }

        public override bool VisitDateTp([NotNull] SpreadsheetParser.DateTpContext context)
        {
            LastType = VarType.Date;
            return true;
        }

        public override bool VisitDecimalTp([NotNull] SpreadsheetParser.DecimalTpContext context)
        {
            LastType = VarType.Decimal;
            return true;
        }

        public override bool VisitIntTp([NotNull] SpreadsheetParser.IntTpContext context)
        {
            LastType = VarType.Int;
            return true;
        }

        public override bool VisitStringTp([NotNull] SpreadsheetParser.StringTpContext context)
        {
            LastType = VarType.String;
            return true;
        }


        // ==========================================
        // Functions
        // ==========================================

        public override bool VisitFunctionExp([NotNull] SpreadsheetParser.FunctionExpContext context)
        {
            return base.VisitFunctionExp(context);
        }

        public override bool VisitIsblankFunc([NotNull] SpreadsheetParser.IsblankFuncContext context)
        {
            Logger.DebugLine("Visit Isblank Func Exp");
            SpreadsheetParser.OneArgContext args = context.oneArg();
            var result = Visit(context.oneArg().exp());
            LastType = VarType.Bool;
            return result;
        }

        public override bool VisitIsnaFunc([NotNull] SpreadsheetParser.IsnaFuncContext context)
        {
            Logger.DebugLine("Visit Isna Func Exp");
            SpreadsheetParser.OneArgContext args = context.oneArg();
            var result = Visit(context.oneArg().exp());
            LastType = VarType.Bool;
            return result;
        }

        public override bool VisitIfFunc([NotNull] SpreadsheetParser.IfFuncContext context)
        {
            Logger.DebugLine("Visit If Func Exp");
            bool result = true;

            var args = context.threeArg();
            var firstArg = args.first;
            Visit(firstArg);
            VarType firstType = LastType;

            var secondArg = args.second;
            Visit(secondArg);
            VarType secondType = LastType;

            var thirdArg = args.third;
            Visit(thirdArg);
            VarType thirdType = LastType;

            bool checkResult = firstType == VarType.Bool;
            LastType = secondType;
            if (!checkResult)
            {
                checkResult = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"IfFunc unexpected check type {firstType.ToString()}",
                    $"Expected a bool expression as the check expression in this if function, but got expression {firstArg.GetText()} : {firstType.ToString()} instead."
                );
                LastType = VarType.TypeError;
            }
            bool equalResult = secondType == thirdType;
            if (!equalResult)
            {
                equalResult = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"IfFunc incompatible types {secondType.ToString()} and {thirdType.ToString()}",
                    $"Expected that second and third expression of this if function are equal, but got expressions {secondArg.GetText()} : {secondType.ToString()} and {thirdArg.GetText()} : {thirdType.ToString()} instead."
                );
                LastType = VarType.TypeError;
            }

            result &= checkResult && equalResult;

            return result;
        }

        public override bool VisitProdFunc([NotNull] SpreadsheetParser.ProdFuncContext context)
        {
            Logger.DebugLine("Visit Prod Func Exp");
            bool result = true;
            var args = context.anyArg();
            VarType type = VarType.Int;
            foreach(var child in args._expr)
            {
                Visit(child);
                if(type != VarType.Empty && !LastType.IsNumeric())
                {
                    ErrorType error = ErrorType.ExpectedOtherType;
                    if(LastType == VarType.Empty)
                    {
                        error = ErrorType.UnexpectedEmptyType;
                    }
                    result &= Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        error,
                        $"Prod Func wrong type {LastType.ToString()}",
                        $"The Product function expected all arguments of type numeric, but got one expression {child.GetText()} of type {LastType.ToString()} instead");
                }
                else
                {
                    var originalType = type;
                    type = VarTypeExtensions.GetHighestNumericType(LastType, originalType);
                    Logger.DebugLine($"Child: {child.GetText()}, originalType: {originalType}, LastType: {LastType}, type: {type}");
                }
            }

            if (result)
            {
                LastType = type;
                Logger.DebugLine($"ResultType: {LastType}");
            }
            else
            {
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitSumFunc([NotNull] SpreadsheetParser.SumFuncContext context)
        {
            Logger.DebugLine("Visit Sum Func Exp");
            bool result = true;
            var args = context.anyArg();
            VarType type = VarType.Int;
            foreach(var child in args._expr)
            {
                Visit(child);
                if (type != VarType.Empty && !LastType.IsNumeric())
                {
                    ErrorType error = ErrorType.ExpectedOtherType;
                    if (LastType == VarType.Empty)
                    {
                        error = ErrorType.UnexpectedEmptyType;
                    }
                    result &= Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        error,
                        $"Sum Func wrong type {LastType.ToString()}",
                        $"The Sum function expected all arguments of type numeric, but got one expression {child.GetText()} of type {LastType.ToString()} instead");
                }
                else
                {
                    var oldType = type;
                    type = VarTypeExtensions.GetHighestNumericType(LastType, oldType);
                    Logger.DebugLine($"Sum: LastType: {LastType}, oldType: {oldType}, type: {type}");
                }
            }

            if (result)
            {
                LastType = type;
            }
            else
            {
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitAverageFunc([NotNull] SpreadsheetParser.AverageFuncContext context)
        {
            Logger.DebugLine("Visit Average Func Exp");
            bool result = true;
            var args = context.anyArg();
            VarType type = VarType.Int;
            foreach (var child in args._expr)
            {
                Visit(child);
                if (type != VarType.Empty && !LastType.IsNumeric())
                {
                    ErrorType error = ErrorType.ExpectedOtherType;
                    if (LastType == VarType.Empty)
                    {
                        error = ErrorType.UnexpectedEmptyType;
                    }
                    result &= Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        error,
                        $"Average Func wrong type {LastType.ToString()}",
                        $"The Average function expected all arguments of type numeric, but got one expression {child.GetText()} of type {LastType.ToString()} instead");
                }
                else
                {
                    var oldType = type;
                    type = VarTypeExtensions.GetHighestNumericType(LastType, oldType);
                    Logger.DebugLine($"Sum: LastType: {LastType}, oldType: {oldType}, type: {type}");
                }
            }

            if (result)
            {
                LastType = type;
            }
            else
            {
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitMaxFunc([NotNull] SpreadsheetParser.MaxFuncContext context)
        {
            Logger.DebugLine("Visit Max Func Exp");
            bool result = true;
            var args = context.anyArg();
            VarType type = VarType.Int;
            foreach (var child in args._expr)
            {
                Visit(child);
                if (type != VarType.Empty && !LastType.IsNumeric())
                {
                    ErrorType error = ErrorType.ExpectedOtherType;
                    if (LastType == VarType.Empty)
                    {
                        error = ErrorType.UnexpectedEmptyType;
                    }
                    result &= Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        error,
                        $"Max Func wrong type {LastType.ToString()}",
                        $"The Max function expected all arguments of type numeric, but got one expression {child.GetText()} of type {LastType.ToString()} instead");
                }
                else
                {
                    var oldType = type;
                    type = VarTypeExtensions.GetHighestNumericType(LastType, oldType);
                    Logger.DebugLine($"Sum: LastType: {LastType}, oldType: {oldType}, type: {type}");
                }
            }

            if (result)
            {
                LastType = type;
            }
            else
            {
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitMinFunc([NotNull] SpreadsheetParser.MinFuncContext context)
        {
            Logger.DebugLine("Visit Min Func Exp");
            bool result = true;
            var args = context.anyArg();
            VarType type = VarType.Int;
            foreach (var child in args._expr)
            {
                Visit(child);
                if (type != VarType.Empty && !LastType.IsNumeric())
                {
                    ErrorType error = ErrorType.ExpectedOtherType;
                    if (LastType == VarType.Empty)
                    {
                        error = ErrorType.UnexpectedEmptyType;
                    }
                    result &= Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        error,
                        $"Min Func wrong type {LastType.ToString()}",
                        $"The Min function expected all arguments of type numeric, but got one expression {child.GetText()} of type {LastType.ToString()} instead");
                }
                else
                {
                    var oldType = type;
                    type = VarTypeExtensions.GetHighestNumericType(LastType, oldType);
                    Logger.DebugLine($"Sum: LastType: {LastType}, oldType: {oldType}, type: {type}");
                }
            }

            if (result)
            {
                LastType = type;
            }
            else
            {
                LastType = VarType.TypeError;
            }

            return result;
        }

        public override bool VisitRoundupFunc([NotNull] SpreadsheetParser.RoundupFuncContext context)
        {
            Logger.DebugLine("Visit Roundup Func Exp");
            bool result = true;

            var args = context.twoArg();
            var firstArg = args.first;
            Visit(firstArg);
            VarType firstType = LastType;

            var secondArg = args.second;
            Visit(secondArg);
            VarType secondType = LastType;

            bool firstCheckResult = firstType.IsNumeric();
            LastType = firstType;
            if (!firstCheckResult)
            {
                firstCheckResult = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"Roundup unexpected first type {firstType.ToString()}",
                    $"Expected a numeric expression as the first parameter in this roundup function, but got expression {firstArg.GetText()} : {firstType.ToString()} instead."
                );
                LastType = VarType.TypeError;

            }
            bool secondCheckResult = secondType == VarType.Int;
            if (!secondCheckResult)
            {
                secondCheckResult = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"Roundup unexpected second type {secondType.ToString()}",
                    $"Expected an integer expression as the second parameter in this roundup function, but got expression {secondArg.GetText()} : {secondType.ToString()} instead."
                );
                LastType = VarType.TypeError;
            }

            result &= firstCheckResult && secondCheckResult;

            return result;
        }

        public override bool VisitNFunc([NotNull] SpreadsheetParser.NFuncContext context)
        {
            Logger.DebugLine("Visit N Func Exp");
            SpreadsheetParser.OneArgContext args = context.oneArg();
            var result = Visit(context.oneArg().exp());
            if(LastType != VarType.RuntimeError && LastType != VarType.TypeError && !LastType.IsNumeric())
            {
                LastType = VarType.Int;
            }
            return result;
        }

        // ==========================================
        // Values
        // ==========================================

        public override bool VisitValueExp([NotNull] SpreadsheetParser.ValueExpContext context)
        {
            var result = Visit(context.val);
            return result;
        }

        public override bool VisitIntVal([NotNull] SpreadsheetParser.IntValContext context)
        {
            int value = int.Parse(context.INT().GetText());
            LastIntValue = value;
            LastType = VarType.Int;
            return true;
        }

        public override bool VisitEmptyVal([NotNull] SpreadsheetParser.EmptyValContext context)
        {
            LastType = VarType.Empty;
            return true;
        }

        public override bool VisitTrueVal([NotNull] SpreadsheetParser.TrueValContext context)
        {
            LastType = VarType.Bool;
            return true;
        }

        public override bool VisitFalseVal([NotNull] SpreadsheetParser.FalseValContext context)
        {
            LastType = VarType.Bool;
            return true;
        }

        public override bool VisitDateVal([NotNull] SpreadsheetParser.DateValContext context)
        {
            DateTime value = DateTime.Parse(context.DATE().GetText());
            LastType = VarType.Date;
            return true;
        }

        public override bool VisitDecimalVal([NotNull] SpreadsheetParser.DecimalValContext context)
        {
            double value = double.Parse(context.DECIMAL().GetText());
            LastType = VarType.Decimal;
            return true;
        }

        public override bool VisitStringVal([NotNull] SpreadsheetParser.StringValContext context)
        {
            string value = context.STRING().GetText();
            LastType = VarType.String;
            return true;
        }

        public override bool VisitDollarsVal([NotNull] SpreadsheetParser.DollarsValContext context)
        {
            double value = double.Parse(context.DOLLARS().GetText());
            LastType = VarType.Currency;
            return true;
        }

        public override bool VisitEurosVal([NotNull] SpreadsheetParser.EurosValContext context)
        {
            double value = double.Parse(context.EUROS().GetText());
            LastType = VarType.Currency;
            return true;
        }
    }
}
