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
            Console.WriteLine("Visit SpreadSheet");
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

        public override bool VisitCellStm([NotNull] SpreadsheetParser.CellStmContext context)
        {
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
                Console.WriteLine($"Add Cell {left}, {right}, {contentVal.ToString()}");
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

        public override bool VisitCellEqStm([NotNull] SpreadsheetParser.CellEqStmContext context)
        {
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
                Console.WriteLine($"Add Cell {left}, {right}, {contentVal.ToString()}");
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
            bool result = true;
            return result;
        }

        public override bool VisitEmptyStm([NotNull] SpreadsheetParser.EmptyStmContext context)
        {
            return true;
        }

        public override bool VisitAssignStm([NotNull] SpreadsheetParser.AssignStmContext context)
        {
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
            }

            return result;
        }

        public override bool VisitIfStm([NotNull] SpreadsheetParser.IfStmContext context)
        {
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
        public override bool VisitAddExp([NotNull] SpreadsheetParser.AddExpContext context)
        {
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.left);
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
            var result = Visit(context.left);
            var leftType = LastType;

            result &= Visit(context.left);
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
                result = Handler.ThrowError(
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

        public override bool VisitIsblankFunc([NotNull] SpreadsheetParser.IsblankFuncContext context)
        {
            SpreadsheetParser.OneArgContext args = context.oneArg();
            LastType = VarType.Bool;
            return Visit(context.oneArg().exp());
        }

        public override bool VisitProdFunc([NotNull] SpreadsheetParser.ProdFuncContext context)
        {
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
            LastType = VarType.None;
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
