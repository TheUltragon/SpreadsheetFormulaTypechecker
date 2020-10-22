
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace ANTLR_Test.Classes.SpreadSheetImport
{
    class ExcelFormulaVisitor : ExcelFormulaBaseVisitor<string>
    {
        public override string VisitAddExp([NotNull] ExcelFormulaParser.AddExpContext context)
        {
            return base.VisitAddExp(context);
        }

        public override string VisitAndExp([NotNull] ExcelFormulaParser.AndExpContext context)
        {
            return base.VisitAndExp(context);
        }

        public override string VisitAnyArg([NotNull] ExcelFormulaParser.AnyArgContext context)
        {
            return base.VisitAnyArg(context);
        }

        public override string VisitBaseAdress([NotNull] ExcelFormulaParser.BaseAdressContext context)
        {
            return base.VisitBaseAdress(context);
        }

        public override string VisitBothLockAdress([NotNull] ExcelFormulaParser.BothLockAdressContext context)
        {
            return base.VisitBothLockAdress(context);
        }

        public override string VisitCelladress([NotNull] ExcelFormulaParser.CelladressContext context)
        {
            return base.VisitCelladress(context);
        }

        public override string VisitCellExp([NotNull] ExcelFormulaParser.CellExpContext context)
        {
            return base.VisitCellExp(context);
        }

        public override string VisitCharVal([NotNull] ExcelFormulaParser.CharValContext context)
        {
            return base.VisitCharVal(context);
        }

        public override string VisitColumnLockAdress([NotNull] ExcelFormulaParser.ColumnLockAdressContext context)
        {
            return base.VisitColumnLockAdress(context);
        }

        public override string VisitDateVal([NotNull] ExcelFormulaParser.DateValContext context)
        {
            return base.VisitDateVal(context);
        }

        public override string VisitDecimalVal([NotNull] ExcelFormulaParser.DecimalValContext context)
        {
            return base.VisitDecimalVal(context);
        }

        public override string VisitDivExp([NotNull] ExcelFormulaParser.DivExpContext context)
        {
            return base.VisitDivExp(context);
        }

        public override string VisitDollarsVal([NotNull] ExcelFormulaParser.DollarsValContext context)
        {
            return base.VisitDollarsVal(context);
        }

        public override string VisitEmptyVal([NotNull] ExcelFormulaParser.EmptyValContext context)
        {
            return base.VisitEmptyVal(context);
        }

        public override string VisitEqualExp([NotNull] ExcelFormulaParser.EqualExpContext context)
        {
            return base.VisitEqualExp(context);
        }

        public override string VisitEurosVal([NotNull] ExcelFormulaParser.EurosValContext context)
        {
            return base.VisitEurosVal(context);
        }

        public override string VisitExcelExpr([NotNull] ExcelFormulaParser.ExcelExprContext context)
        {
            return base.VisitExcelExpr(context);
        }

        public override string VisitExp([NotNull] ExcelFormulaParser.ExpContext context)
        {
            return base.VisitExp(context);
        }

        public override string VisitFalseVal([NotNull] ExcelFormulaParser.FalseValContext context)
        {
            return base.VisitFalseVal(context);
        }

        public override string VisitFexp([NotNull] ExcelFormulaParser.FexpContext context)
        {
            return base.VisitFexp(context);
        }

        public override string VisitFunctionExp([NotNull] ExcelFormulaParser.FunctionExpContext context)
        {
            return base.VisitFunctionExp(context);
        }

        public override string VisitGreaterEqExp([NotNull] ExcelFormulaParser.GreaterEqExpContext context)
        {
            return base.VisitGreaterEqExp(context);
        }

        public override string VisitGreaterExp([NotNull] ExcelFormulaParser.GreaterExpContext context)
        {
            return base.VisitGreaterExp(context);
        }

        public override string VisitIfFunc([NotNull] ExcelFormulaParser.IfFuncContext context)
        {
            return base.VisitIfFunc(context);
        }

        public override string VisitIntVal([NotNull] ExcelFormulaParser.IntValContext context)
        {
            return base.VisitIntVal(context);
        }

        public override string VisitIsblankFunc([NotNull] ExcelFormulaParser.IsblankFuncContext context)
        {
            return base.VisitIsblankFunc(context);
        }

        public override string VisitModExp([NotNull] ExcelFormulaParser.ModExpContext context)
        {
            return base.VisitModExp(context);
        }

        public override string VisitMultExp([NotNull] ExcelFormulaParser.MultExpContext context)
        {
            return base.VisitMultExp(context);
        }

        public override string VisitNotExp([NotNull] ExcelFormulaParser.NotExpContext context)
        {
            return base.VisitNotExp(context);
        }

        public override string VisitOneArg([NotNull] ExcelFormulaParser.OneArgContext context)
        {
            return base.VisitOneArg(context);
        }

        public override string VisitOrExp([NotNull] ExcelFormulaParser.OrExpContext context)
        {
            return base.VisitOrExp(context);
        }

        public override string VisitProdFunc([NotNull] ExcelFormulaParser.ProdFuncContext context)
        {
            return base.VisitProdFunc(context);
        }

        public override string VisitRowLockAdress([NotNull] ExcelFormulaParser.RowLockAdressContext context)
        {
            return base.VisitRowLockAdress(context);
        }

        public override string VisitSmallerEqExp([NotNull] ExcelFormulaParser.SmallerEqExpContext context)
        {
            return base.VisitSmallerEqExp(context);
        }

        public override string VisitSmallerExp([NotNull] ExcelFormulaParser.SmallerExpContext context)
        {
            return base.VisitSmallerExp(context);
        }

        public override string VisitStringVal([NotNull] ExcelFormulaParser.StringValContext context)
        {
            return base.VisitStringVal(context);
        }

        public override string VisitSubExp([NotNull] ExcelFormulaParser.SubExpContext context)
        {
            return base.VisitSubExp(context);
        }

        public override string VisitSumFunc([NotNull] ExcelFormulaParser.SumFuncContext context)
        {
            return base.VisitSumFunc(context);
        }

        public override string VisitThreeArg([NotNull] ExcelFormulaParser.ThreeArgContext context)
        {
            return base.VisitThreeArg(context);
        }

        public override string VisitTrueVal([NotNull] ExcelFormulaParser.TrueValContext context)
        {
            return base.VisitTrueVal(context);
        }

        public override string VisitTwoArg([NotNull] ExcelFormulaParser.TwoArgContext context)
        {
            return base.VisitTwoArg(context);
        }

        public override string VisitUnequalExp([NotNull] ExcelFormulaParser.UnequalExpContext context)
        {
            return base.VisitUnequalExp(context);
        }

        public override string VisitValue([NotNull] ExcelFormulaParser.ValueContext context)
        {
            return base.VisitValue(context);
        }

        public override string VisitValueExp([NotNull] ExcelFormulaParser.ValueExpContext context)
        {
            return base.VisitValueExp(context);
        }
    }
}
