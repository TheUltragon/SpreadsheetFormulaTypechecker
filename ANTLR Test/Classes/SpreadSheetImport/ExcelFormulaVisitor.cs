
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace ANTLR_Test.Classes
{
    class ExcelFormulaVisitor : ExcelFormulaBaseVisitor<string>
    {
        private Tuple<int, int> lastAdress = new Tuple<int, int>(1,1);

        //====================================================
        //Excel Expression Root
        //====================================================
        public override string VisitExcelExpr([NotNull] ExcelFormulaParser.ExcelExprContext context)
        {
            var exp = context.exp();
            if(exp != null)
            {
                return Visit(context.exp());
            }
            else
            {
                return "Not an excel expression";
            }
        }



        //====================================================
        //Expressions
        //====================================================

        public override string VisitExp([NotNull] ExcelFormulaParser.ExpContext context)
        {
            return base.VisitExp(context);
        }

        public override string VisitAddExp([NotNull] ExcelFormulaParser.AddExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} + {right}";
        }

        public override string VisitAndExp([NotNull] ExcelFormulaParser.AndExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} && {right}";
        }

        public override string VisitDivExp([NotNull] ExcelFormulaParser.DivExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} / {right}";
        }

        public override string VisitEqualExp([NotNull] ExcelFormulaParser.EqualExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} == {right}";
        }

        public override string VisitFunctionExp([NotNull] ExcelFormulaParser.FunctionExpContext context)
        {
            return Visit(context.fun);
        }

        public override string VisitGreaterEqExp([NotNull] ExcelFormulaParser.GreaterEqExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} >= {right}";
        }

        public override string VisitGreaterExp([NotNull] ExcelFormulaParser.GreaterExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} > {right}";
        }

        public override string VisitModExp([NotNull] ExcelFormulaParser.ModExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} % {right}";
        }

        public override string VisitMultExp([NotNull] ExcelFormulaParser.MultExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} * {right}";
        }

        public override string VisitNotExp([NotNull] ExcelFormulaParser.NotExpContext context)
        {
            var param = Visit(context.param);

            return $"!{param}";
        }

        public override string VisitOrExp([NotNull] ExcelFormulaParser.OrExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} || {right}";
        }

        public override string VisitSmallerEqExp([NotNull] ExcelFormulaParser.SmallerEqExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} <= {right}";
        }

        public override string VisitSmallerExp([NotNull] ExcelFormulaParser.SmallerExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} < {right}";
        }

        public override string VisitSubExp([NotNull] ExcelFormulaParser.SubExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} - {right}";
        }

        public override string VisitUnequalExp([NotNull] ExcelFormulaParser.UnequalExpContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);

            return $"{left} != {right}";
        }

        public override string VisitValueExp([NotNull] ExcelFormulaParser.ValueExpContext context)
        {
            return Visit(context.val);
        }















        //====================================================
        //Cell Adress
        //====================================================

        private Tuple<int, int> convertAdressTextToTuple(string row, string column)
        {
            int rowIndex = int.Parse(row);
            int columnIndex = int.Parse(column);
            return new Tuple<int, int>(rowIndex, columnIndex);
        }

        private int convertRowToIndex(string row)
        {
            var counter = 0;
            var finalIndex = 0;
            foreach (char c in row)
            {
                int index = ((int)char.ToUpper(c)) - 64;
                finalIndex += index * (int)Math.Pow(26,counter);
                counter++;
            }

            return finalIndex;
        }

        private string convertRowToIndexText(string row)
        {
            var index = convertRowToIndex(row);
            Logger.DebugLine($"Converting {row} to {index}");
            return "" + index;
        }

        public override string VisitBaseAdress([NotNull] ExcelFormulaParser.BaseAdressContext context)
        {
            var rowText = context.row.Text;
            var row = convertRowToIndexText(rowText);
            var column = context.column.Text;

            lastAdress = convertAdressTextToTuple(row, column);
            return $"C[{row}|{column}]";
        }

        public override string VisitBothLockAdress([NotNull] ExcelFormulaParser.BothLockAdressContext context)
        {
            var rowText = context.row.Text;
            var row = convertRowToIndexText(rowText);
            var column = context.column.Text;

            lastAdress = convertAdressTextToTuple(row, column);
            return $"C[{row}|{column}]";
        }

        public override string VisitColumnLockAdress([NotNull] ExcelFormulaParser.ColumnLockAdressContext context)
        {
            var rowText = context.row.Text;
            var row = convertRowToIndexText(rowText);
            var column = context.column.Text;

            lastAdress = convertAdressTextToTuple(row, column);
            return $"C[{row}|{column}]";
        }

        public override string VisitRowLockAdress([NotNull] ExcelFormulaParser.RowLockAdressContext context)
        {
            var rowText = context.row.Text;
            var row = convertRowToIndexText(rowText);
            var column = context.column.Text;

            lastAdress = convertAdressTextToTuple(row, column);
            return $"C[{row}|{column}]";
        }



        //====================================================
        //Functions
        //====================================================

        public override string VisitFexp([NotNull] ExcelFormulaParser.FexpContext context)
        {
            return base.VisitFexp(context);
        }

        public override string VisitIfFunc([NotNull] ExcelFormulaParser.IfFuncContext context)
        {
            var args = Visit(context.threeArg());
            return $"IF{args}";
        }

        public override string VisitIsblankFunc([NotNull] ExcelFormulaParser.IsblankFuncContext context)
        {
            var args = Visit(context.oneArg());
            return $"ISBLANK{args}";
        }

        public override string VisitProdFunc([NotNull] ExcelFormulaParser.ProdFuncContext context)
        {
            var args = Visit(context.anyArg());
            return $"PROD{args}";
        }

        public override string VisitSumFunc([NotNull] ExcelFormulaParser.SumFuncContext context)
        {
            var args = Visit(context.anyArg());
            return $"SUM{args}";
        }


        //====================================================
        //Function Arguments
        //====================================================

        public override string VisitAnyArgBase([NotNull] ExcelFormulaParser.AnyArgBaseContext context)
        {
            int counter = 0;
            string childText = "";
            foreach (var child in context._expr)
            {
                if (counter == 0)
                {
                    childText += Visit(child);
                }
                else
                {
                    childText += ", " + Visit(child);
                }
                counter++;
            }

            return $"({childText})";
        }

        public override string VisitAnyArgSeq([NotNull] ExcelFormulaParser.AnyArgSeqContext context)
        {
            Visit(context.left);
            var left = lastAdress;
            Visit(context.right);
            var right = lastAdress;

            int counter = 0;
            string childText = "";

            for (int i = left.Item1; i<=right.Item1; i++)
            {
                for (int j = left.Item2; j <= right.Item2; j++)
                {
                    if (counter == 0)
                    {
                        childText += $"C[{i}|{j}]";
                    }
                    else
                    {
                        childText += $", C[{i}|{j}]";
                    }
                    counter++;
                }
            }

            return $"({childText})";
        }

        public override string VisitOneArg([NotNull] ExcelFormulaParser.OneArgContext context)
        {
            var arg1 = Visit(context.exp());
            return $"({arg1})";
        }

        public override string VisitThreeArg([NotNull] ExcelFormulaParser.ThreeArgContext context)
        {
            var arg1 = Visit(context.exp1);
            var arg2 = Visit(context.exp2);
            var arg3 = Visit(context.exp3);
            return $"({arg1}, {arg2}, {arg3})";
        }

        public override string VisitTwoArg([NotNull] ExcelFormulaParser.TwoArgContext context)
        {
            var arg1 = Visit(context.exp1);
            var arg2 = Visit(context.exp2);
            return $"({arg1}, {arg2})";
        }



        //====================================================
        //Values
        //====================================================

        public override string VisitValue([NotNull] ExcelFormulaParser.ValueContext context)
        {
            return base.VisitValue(context);
        }

        public override string VisitCharVal([NotNull] ExcelFormulaParser.CharValContext context)
        {
            return context.GetText();
        }

        public override string VisitDateVal([NotNull] ExcelFormulaParser.DateValContext context)
        {
            return context.GetText();
        }

        public override string VisitDecimalVal([NotNull] ExcelFormulaParser.DecimalValContext context)
        {
            return context.GetText();
        }

        public override string VisitDollarsVal([NotNull] ExcelFormulaParser.DollarsValContext context)
        {
            return context.GetText();
        }

        public override string VisitEmptyVal([NotNull] ExcelFormulaParser.EmptyValContext context)
        {
            return "\\";
        }

        public override string VisitEurosVal([NotNull] ExcelFormulaParser.EurosValContext context)
        {
            return context.GetText();
        }

        public override string VisitFalseVal([NotNull] ExcelFormulaParser.FalseValContext context)
        {
            return "false";
        }

        public override string VisitIntVal([NotNull] ExcelFormulaParser.IntValContext context)
        {
            return context.GetText();
        }

        public override string VisitStringVal([NotNull] ExcelFormulaParser.StringValContext context)
        {
            return context.GetText();
        }

        public override string VisitTrueVal([NotNull] ExcelFormulaParser.TrueValContext context)
        {
            return "true";
        }
    }
}
