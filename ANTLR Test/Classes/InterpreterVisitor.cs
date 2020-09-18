using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLR_Test.Classes
{

    public class InterpreterVisitor : SpreadsheetVisitor
    {
        // ==========================================
        // CSTR
        // ==========================================

        public InterpreterVisitor(ErrorHandler handler) : base(handler) { }

        public InterpreterVisitor(ErrorHandler handler, DataRepository repository) : base(handler, repository) { }

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
            Console.WriteLine("Visit CellStm");
            var leftResult = Visit(context.left);
            var leftVal = LastExpValue;
            var rightResult = Visit(context.right);
            var rightVal = LastExpValue;
            var contentResult = Visit(context.content);
            var contentVal = LastExpValue;
            if(IntValue.IsThis(leftVal) && IntValue.IsThis(rightVal))
            {
                int left = ((IntValue)leftVal).Value;
                int right = ((IntValue)rightVal).Value;
                Console.WriteLine($"Add Cell {left}, {right}, {contentVal.ToString()}");
                Repository.Cells[new Tuple<int, int>(left, right)] = contentVal;
                return leftResult && rightResult && contentResult;
            }
            else
            {
                Console.WriteLine($"Cell Stm has Cell expressions not of type int! On line {context.Start.Line}, column {context.Start.Column}");
                return false;
            }
        }

        public override bool VisitCellEqStm([NotNull] SpreadsheetParser.CellEqStmContext context)
        {
            Console.WriteLine("Visit CellStm");
            var leftResult = Visit(context.left);
            var leftVal = LastExpValue;
            var rightResult = Visit(context.right);
            var rightVal = LastExpValue;
            var contentVal = new ExpValue(this, context.content);
            if (IntValue.IsThis(leftVal) && IntValue.IsThis(rightVal))
            {
                int left = ((IntValue)leftVal).Value;
                int right = ((IntValue)rightVal).Value;
                Console.WriteLine($"Add Cell {left}, {right}, {contentVal.ToString()}");
                Repository.Cells[new Tuple<int, int>(left, right)] = contentVal;
                return leftResult && rightResult;
            }
            else
            {
                Console.WriteLine($"Cell Eq Stm has Cell expressions not of type int! On line {context.Start.Line}, column {context.Start.Column}");
                return false;
            }
        }

        public override bool VisitEvalStm([NotNull] SpreadsheetParser.EvalStmContext context)
        {
            bool result = true;
            foreach (var item in Repository.Cells.Values)
            {
                if (ExpValue.IsThis(item))
                {
                    result &= ((ExpValue)item).Eval();
                }
            }

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
            result &= varType == expType && varType != VarType.None;

            if(result)
            {
                //Add var of id with value lastExp to DataRepository
                Repository.Variables.Add(id, LastExpValue);
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
