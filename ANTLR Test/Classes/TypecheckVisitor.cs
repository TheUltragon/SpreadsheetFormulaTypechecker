using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLR_Test.Classes
{

    public class SpreadsheetVisitor : SpreadsheetBaseVisitor<bool>
    {
        public ErrorHandler Handler { get; protected set; }
        public DataRepository Repository { get; protected set; }
        public ValueBase LastExpValue { get; protected set; }
        public ValueBase LastValue { get; protected set; }
        public VarType LastFuncType { get; protected set; }
        public VarType LastType { get; protected set; }

        public SpreadsheetVisitor(ErrorHandler handler)
        {
            Handler = handler;
            Repository = new DataRepository();
            LastType = VarType.None;
        }

        public SpreadsheetVisitor(ErrorHandler handler, DataRepository repository)
        {
            Handler = handler;
            Repository = repository;
            LastType = VarType.None;
        }
    }

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
            foreach (var child in context.children)
            {
                result &= Visit(child);
               
            }
            return result;
        }


        // ==========================================
        // Statements
        // ==========================================

        //Value assignment to Cell
        public override bool VisitCellValueStm([NotNull] SpreadsheetParser.CellValueStmContext context)
        {
            Logger.DebugLine("Visit Cell Value Stm");
            bool result = true;

            result &= Visit(context.left);
            var leftVal = LastExpValue;

            result &= Visit(context.right);
            var rightVal = LastExpValue;

            result &= Visit(context.content);
            var contentVal = new CellValue(this, LastExpValue);
            var contentType = new CellType(this, LastType);

            if (IntValue.IsThis(leftVal) && IntValue.IsThis(rightVal))
            {
                int left = ((IntValue)leftVal).Value;
                int right = ((IntValue)rightVal).Value;
                Logger.DebugLine($"Add Cell {left}, {right}, {contentVal.ToString()}");
                Repository.Cells[new Tuple<int, int>(left, right)] = contentVal;
                Repository.CellTypes[new Tuple<int, int>(left, right)] = contentType;
            }
            else
            {
                result = Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    false, 
                    ErrorType.CellAdressWrongType, 
                    $"Cell Eq Stm Adress not integer types {leftVal.GetVarType().ToString()} and {rightVal.GetVarType().ToString()}", 
                    $"Cell Statement has Adress expressions not of type int! Found expressions: {leftVal.GetVarType().ToString()} and {rightVal.GetVarType().ToString()}"
                );
            }

            return result;
        }

        //Formula assignment to Cell
        public override bool VisitCellFormulaStm([NotNull] SpreadsheetParser.CellFormulaStmContext context)
        {
            Logger.DebugLine("Visit Cell Formula Stm");
            bool result = true;

            result &= Visit(context.left);
            var leftVal = LastExpValue;

            result &= Visit(context.right);
            var rightVal = LastExpValue;

            var contentVal = new CellValue(this, context.content);
            var contentType = new CellType(this, context.content);

            if (IntValue.IsThis(leftVal) && IntValue.IsThis(rightVal))
            {
                int left = ((IntValue)leftVal).Value;
                int right = ((IntValue)rightVal).Value;
                Logger.DebugLine($"Add Cell {left}, {right}, {contentVal.ToString()}");
                Repository.Cells[new Tuple<int, int>(left, right)] = contentVal;
                Repository.CellTypes[new Tuple<int, int>(left, right)] = contentType;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    false, 
                    ErrorType.CellAdressWrongType, 
                    $"Cell Stm Adress not integer types {leftVal.GetVarType().ToString()} and {rightVal.GetVarType().ToString()}", 
                    $"Cell Equal Statement has Adress expressions not of type int! Found expressions: {leftVal.GetVarType().ToString()} and {rightVal.GetVarType().ToString()}"
                );
            }
            return result;
        }

        public override bool VisitEvalStm([NotNull] SpreadsheetParser.EvalStmContext context)
        {
            Logger.DebugLine("Visit Eval Stm");
            bool result = true;
            return result;
        }

        public override bool VisitEmptyStm([NotNull] SpreadsheetParser.EmptyStmContext context)
        {
            Logger.DebugLine("Visit Empty Stm");
            return true;
        }

        public override bool VisitAssignStm([NotNull] SpreadsheetParser.AssignStmContext context)
        {
            Logger.DebugLine("Visit Assign Stm");
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

            //Check, wether assignment declared type matches expression type and are not None
            bool resultCheckTypes = varType == expType;
            bool resultCheckTypeNotNone = varType != VarType.None;
            if (!resultCheckTypes)
            {
                resultCheckTypes = Handler.ThrowError(context.Start.Line, context.Start.Column, true, ErrorType.IncompatibleTypesAssignment, $"Assignment incompatible types {varType.ToString()} and {expType.ToString()}", $"The variable of this assignment expects type {varType.ToString()} but was assigned an expression of type {expType.ToString()}.");
            }
            if (!resultCheckTypeNotNone)
            {
                resultCheckTypeNotNone = Handler.ThrowError(context.Start.Line, context.Start.Column, false, ErrorType.ExpectedOtherType, $"Assignment not type None", $"The assignment has assigned {id} an expression with type None.");
            }
            result &= resultCheckTypes && resultCheckTypeNotNone;
            if (result)
            {
                //Add var of id with value lastExp to DataRepository
                Repository.Variables.Add(id, LastExpValue);
                Repository.VariableTypes.Add(id, expType);
            }

            return result;
        }

        public override bool VisitIfStm([NotNull] SpreadsheetParser.IfStmContext context)
        {
            Logger.DebugLine("Visit If Stm");
            bool result = true;

            result &= Visit(context.check);
            bool resultCheckType = LastExpValue.GetVarType() == VarType.Bool;
            if (!resultCheckType)
            {
                resultCheckType = Handler.ThrowError(context.check.Start.Line, context.check.Start.Column, false, ErrorType.ExpectedOtherType, "If Stm check type " + LastExpValue.GetVarType().ToString(), $"The check expression type of this if clause is of type {LastExpValue.GetVarType().ToString()} instead of bool.");
            }
            result &= resultCheckType;

            result &= Visit(context.falseStm);
            result &= Visit(context.trueStm);

            return result;
        }

        public override bool VisitWhileStm([NotNull] SpreadsheetParser.WhileStmContext context)
        {
            Logger.DebugLine("Visit While Stm");
            bool result = true;

            result &= Visit(context.check);

            bool resultCheckType = LastExpValue.GetVarType() == VarType.Bool;
            if(!resultCheckType)
            {
                resultCheckType = Handler.ThrowError(context.check.Start.Line, context.check.Start.Column, false, ErrorType.ExpectedOtherType, "While Stm check type " + LastExpValue.GetVarType().ToString(), $"The check expression type of this while clause is of type {LastExpValue.GetVarType().ToString()} instead of bool.");
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

            if(leftType == rightType && (leftType.IsNumeric() || leftType == VarType.String || leftType == VarType.Date || leftType == VarType.Currency)){
                LastType = leftType;
            }
            else if(leftType.IsNumeric() && rightType.IsNumeric())
            {
                LastType = VarTypeExtensions.GetHighestNumericType(leftType, rightType);
            }
            else if(leftType.IsText() && rightType.IsText())
            {
                //Char and Char or Char and string always returns a string
                LastType = VarType.String;
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line, 
                    context.Start.Column, 
                    true, 
                    ErrorType.IncompatibleTypesExpression, 
                    $"Add Exp incompatible types {leftType.ToString()} and {rightType.ToString()}", 
                    $"This addition expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}."
                );
                LastType = leftType;
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
                    $"This and expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
            }

            return result;
        }

        public override bool VisitCellExp([NotNull] SpreadsheetParser.CellExpContext context)
        {
            Logger.DebugLine("Visit Cell Exp");
            bool result = true;

            result &= Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.right);
            var rightType = LastType;

            if (leftType == rightType && leftType == VarType.Int)
            {
                //TODO: Do Type inference for Cell Type
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    false,
                    ErrorType.CellAdressWrongType,
                    $"Cell Exp Adress not integer types {leftType.ToString()} and {rightType.ToString()}",
                    $"Cell Expression has Adress expressions not of type int! Found expressions: {leftType.ToString()} and {rightType.ToString()}"
                );
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
                LastType = VarTypeExtensions.GetHighestNumericType(leftType, rightType);
            }
            else
            {
                result &= Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.IncompatibleTypesExpression,
                    $"Div Exp incompatible types {leftType.ToString()} and {rightType.ToString()}",
                    $"This Division expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}."
                );
                LastType = leftType;
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
                    $"This equality expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                    $"This greater equal expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                    $"This greater expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                    $"This greater expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                    $"This multiplication expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}."
                );
                LastType = leftType;
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
                LastType = leftType;
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
                    $"This or expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                    $"This smaller equal expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                    $"This smaller expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                    $"This subtraction expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}."
                );
                LastType = leftType;
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
                    $"This inequality expression has 2 incompatible expressions attached with types {leftType.ToString()} and {rightType.ToString()}. It expected 2 bool types."
                );
                LastType = leftType;
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
                LastType = VarType.None;
            }
            return result;
        }



        // ==========================================
        // Additive Expressions
        // ==========================================

        public override bool VisitBaseAExp([NotNull] SpreadsheetParser.BaseAExpContext context)
        {
            Logger.DebugLine("Visit Base A Exp");
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
                    $"Expected a numeric expression in base additive expression, but got type {type.ToString()} instead."
                );
            }
            return result;
        }

        public override bool VisitNegAExp([NotNull] SpreadsheetParser.NegAExpContext context)
        {
            Logger.DebugLine("Visit Neg A Exp");
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
                    $"Expected a numeric expression in negative additive expression, but got type {type.ToString()} instead."
                );
            }
            return result;
        }

        public override bool VisitPosAExp([NotNull] SpreadsheetParser.PosAExpContext context)
        {
            Logger.DebugLine("Visit Pos A Exp");
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
                    $"Expected a numeric expression in positive additive expression, but got type {type.ToString()} instead."
                );
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

        public override bool VisitCharTp([NotNull] SpreadsheetParser.CharTpContext context)
        {
            LastType = VarType.Char;
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
            Logger.DebugLine("Visit Isblank Exp");
            SpreadsheetParser.OneArgContext args = context.oneArg();
            LastType = VarType.Bool;
            return Visit(context.oneArg().exp());
        }

        public override bool VisitIfFunc([NotNull] SpreadsheetParser.IfFuncContext context)
        {
            Logger.DebugLine("Visit If Exp");
            bool result = true;

            var args = context.threeArg();
            var firstArg = args.GetChild(0);
            Visit(firstArg);
            VarType firstType = LastType;

            var secondArg = args.GetChild(1);
            Visit(secondArg);
            VarType secondType = LastType;

            var thirdArg = args.GetChild(2);
            Visit(thirdArg);
            VarType thirdType = LastType;

            bool checkResult = firstType == VarType.Bool;
            if (!checkResult)
            {
                checkResult = Handler.ThrowError(
                    context.Start.Line,
                    context.Start.Column,
                    true,
                    ErrorType.ExpectedOtherType,
                    $"IfFunc unexpected check type {firstType.ToString()}",
                    $"Expected a bool expression as the check expression in this if function, but got type {firstType.ToString()} instead."
                );
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
                    $"Expected that second and third expression of this if function are equal, but got types {secondType.ToString()} and {thirdType.ToString()} instead."
                );
            }

            result &= checkResult && equalResult;
            LastType = secondType;

            return result;
        }

        public override bool VisitProdFunc([NotNull] SpreadsheetParser.ProdFuncContext context)
        {
            Logger.DebugLine("Visit Prod Exp");
            bool result = true;
            var args = context.anyArg();
            VarType type = VarType.None;
            foreach (var child in args.children)
            {
                Visit(child);
                if(type != VarType.None && !LastType.IsNumeric())
                {
                    result = Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        ErrorType.ExpectedOtherType,
                        $"Prod Func wrong type {LastType.ToString()}",
                        $"The Product function expected an argument of type numeric, but got of type {LastType.ToString()} instead");
                }
                else
                {
                    type = VarTypeExtensions.GetHighestNumericType(LastType, type);
                }
            }

            if (result)
            {
                LastType = type;
            }

            return result;
        }

        public override bool VisitSumFunc([NotNull] SpreadsheetParser.SumFuncContext context)
        {
            Logger.DebugLine("Visit Sum Exp");
            bool result = true;
            var args = context.anyArg();
            VarType type = VarType.None;
            foreach (var child in args.children)
            {
                Visit(child);
                if (type != VarType.None && !LastType.IsNumeric())
                {
                    result = Handler.ThrowError(
                        context.Start.Line,
                        context.Start.Column,
                        true,
                        ErrorType.ExpectedOtherType,
                        $"Prod Func wrong type {LastType.ToString()}",
                        $"The Product function expected an argument of type numeric, but got of type {LastType.ToString()} instead");
                }
                else
                {
                    type = VarTypeExtensions.GetHighestNumericType(LastType, type);
                }
            }

            if (result)
            {
                LastType = type;
            }

            return result;
        }


        // ==========================================
        // Values
        // ==========================================

        public override bool VisitValueExp([NotNull] SpreadsheetParser.ValueExpContext context)
        {
            var result = Visit(context.val);
            LastExpValue = LastValue;
            return result;
        }

        public override bool VisitCharVal([NotNull] SpreadsheetParser.CharValContext context)
        {
            char value = Char.Parse(context.CHAR().GetText());
            LastValue = new CharValue(value);
            LastType = VarType.Char;
            return true;
        }

        public override bool VisitIntVal([NotNull] SpreadsheetParser.IntValContext context)
        {
            int value = int.Parse(context.INT().GetText());
            LastValue = new IntValue(value);
            LastType = VarType.Int;
            return true;
        }

        public override bool VisitEmptyVal([NotNull] SpreadsheetParser.EmptyValContext context)
        {
            LastValue = new EmptyValue();
            LastType = VarType.Unknown;
            return true;
        }

        public override bool VisitTrueVal([NotNull] SpreadsheetParser.TrueValContext context)
        {
            LastValue = new BoolValue(true);
            LastType = VarType.Bool;
            return true;
        }

        public override bool VisitFalseVal([NotNull] SpreadsheetParser.FalseValContext context)
        {
            LastValue = new BoolValue(false);
            LastType = VarType.Bool;
            return true;
        }

        public override bool VisitDateVal([NotNull] SpreadsheetParser.DateValContext context)
        {
            DateTime value = DateTime.Parse(context.DATE().GetText());
            LastValue = new DateValue(value);
            LastType = VarType.Date;
            return true;
        }

        public override bool VisitDecimalVal([NotNull] SpreadsheetParser.DecimalValContext context)
        {
            double value = double.Parse(context.DECIMAL().GetText());
            LastValue = new DecimalValue(value);
            LastType = VarType.Decimal;
            return true;
        }

        public override bool VisitStringVal([NotNull] SpreadsheetParser.StringValContext context)
        {
            string value = context.STRING().GetText();
            LastValue = new StringValue(value);
            LastType = VarType.String;
            return true;
        }

        public override bool VisitDollarsVal([NotNull] SpreadsheetParser.DollarsValContext context)
        {
            double value = double.Parse(context.DOLLARS().GetText());
            LastValue = new CurrencyValue(value, CurrencyValue.CurrencyType.Dollars);
            LastType = VarType.Currency;
            return true;
        }

        public override bool VisitEurosVal([NotNull] SpreadsheetParser.EurosValContext context)
        {
            double value = double.Parse(context.EUROS().GetText());
            LastValue = new CurrencyValue(value, CurrencyValue.CurrencyType.Euros);
            LastType = VarType.Currency;
            return true;
        }
    }
}
