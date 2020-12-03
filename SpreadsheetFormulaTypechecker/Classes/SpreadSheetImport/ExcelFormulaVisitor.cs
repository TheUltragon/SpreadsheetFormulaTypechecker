
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLR_Test.Classes
{
    class ExcelFormulaVisitor : ExcelFormulaBaseVisitor<string>
    {
        private Tuple<int, int> lastAdress = new Tuple<int, int>(1,1);
        public bool Error = false;

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
                ThrowUnsupportedError();
                return "";
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

        public override string VisitBracketExp([NotNull] ExcelFormulaParser.BracketExpContext context)
        {
            var exp = Visit(context.expr);
            return $"({exp})";
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

        public override string VisitNegExp([NotNull] ExcelFormulaParser.NegExpContext context)
        {
            var param = Visit(context.param);

            return $"-{param}";
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

        public override string VisitPosExp([NotNull] ExcelFormulaParser.PosExpContext context)
        {
            var param = Visit(context.param);

            return $"{param}";
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

        private Tuple<int, int> convertAdressToRowColumn(string adressText)
        {
            string cleanedText = adressText.Replace("$", "");
            string columnText = new string(cleanedText.SkipWhile(c => !char.IsDigit(c)).ToArray());
            string rowText = cleanedText.Replace(columnText, "");
            int row = convertRowToIndex(rowText);
            int column = int.Parse(columnText);
            return new Tuple<int, int>(row, column);
        }

        public override string VisitBaseAdress([NotNull] ExcelFormulaParser.BaseAdressContext context)
        {
            var adressText = context.CELLADRESS().GetText();
            Tuple<int, int> result = convertAdressToRowColumn(adressText);
            //var row = convertRowToIndex(result.Item1);
            //var column = result.Item2;

            lastAdress = result;
            return $"C[{result.Item1},{result.Item2}]";
        }

        public override string VisitSheetAdress([NotNull] ExcelFormulaParser.SheetAdressContext context)
        {
            //TODO: Implement reference to other sheet
            Logger.DebugLine("Sheet Adress visited, not supported yet in typechecker!", 1);
            ThrowUnsupportedError();
            return "";
        }

        public void ThrowUnsupportedError()
        {
            Error = true;
            if (GlobalSettings.ImportStopAtUnsupportedError)
            {
                Console.WriteLine($"Enter to continue", 10);
                Console.ReadLine();
            }
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

        public override string VisitIsnaFunc([NotNull] ExcelFormulaParser.IsnaFuncContext context)
        {
            var args = Visit(context.oneArg());
            return $"ISNA{args}";
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

        public override string VisitOrFunc([NotNull] ExcelFormulaParser.OrFuncContext context)
        {
            var args = Visit(context.anyArg());
            return $"OR{args}";
        }

        public override string VisitAndFunc([NotNull] ExcelFormulaParser.AndFuncContext context)
        {
            var args = Visit(context.anyArg());
            return $"AND{args}";
        }

        public override string VisitAverageFunc([NotNull] ExcelFormulaParser.AverageFuncContext context)
        {
            var args = Visit(context.anyArg());
            return $"AVERAGE{args}";
        }

        public override string VisitMaxFunc([NotNull] ExcelFormulaParser.MaxFuncContext context)
        {
            var args = Visit(context.anyArg());
            return $"MAX{args}";
        }

        public override string VisitMinFunc([NotNull] ExcelFormulaParser.MinFuncContext context)
        {
            var args = Visit(context.anyArg());
            return $"MIN{args}";
        }

        public override string VisitRoundupFunc([NotNull] ExcelFormulaParser.RoundupFuncContext context)
        {
            var args = Visit(context.twoArg());
            return $"ROUNDUP{args}";
        }

        public override string VisitRoundFunc([NotNull] ExcelFormulaParser.RoundFuncContext context)
        {
            var args = Visit(context.twoArg());
            return $"ROUND{args}";
        }

        public override string VisitNFunc([NotNull] ExcelFormulaParser.NFuncContext context)
        {
            var args = Visit(context.oneArg());
            return $"N{args}";
        }


        //Unsupported Functions

        public override string VisitSumifFunc([NotNull] ExcelFormulaParser.SumifFuncContext context)
        {
            //TODO: Implement reference to other sheet
            Logger.DebugLine("SUMIF Function expression visited, not supported yet in typechecker!", 1);
            ThrowUnsupportedError();
            return "";
        }

        public override string VisitVlookupFunc([NotNull] ExcelFormulaParser.VlookupFuncContext context)
        {
            //TODO: Implement reference to other sheet
            Logger.DebugLine("VLOOKUP Function expression visited, not supported yet in typechecker!", 1);
            ThrowUnsupportedError();
            return "";
        }

        public override string VisitNaFunc([NotNull] ExcelFormulaParser.NaFuncContext context)
        {
            //TODO: Implement reference to other sheet
            Logger.DebugLine("NA Function expression visited, not supported yet in typechecker!", 1);
            ThrowUnsupportedError();
            return "";
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

            for (int i = left.Item1; i <= right.Item1; i++)
            {
                for (int j = left.Item2; j <= right.Item2; j++)
                {
                    if (counter == 0)
                    {
                        childText += $"C[{i},{j}]";
                    }
                    else
                    {
                        childText += $", C[{i},{j}]";
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

        //====================================================
        //Error Handling
        //====================================================

        public override string VisitErrorNode([NotNull] IErrorNode node)
        {
            ThrowUnsupportedError();
            throw new Exception($"Error Node reached with text {node.GetText()}.");
        }
    }
}
