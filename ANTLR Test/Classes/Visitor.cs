using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLR_Test
{

    public class SpreadsheetVisitor : SpreadsheetBaseVisitor<bool>
    {
        public DataRepository Repository { get; private set; }
        public ValueBase LastExpValue { get; private set; }
        public ValueBase LastValue { get; private set; }



        public SpreadsheetVisitor()
        {
            Repository = new DataRepository();
        }

        public SpreadsheetVisitor(DataRepository repository)
        {
            Repository = repository;
        }

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
            return true;
        }

        public override bool VisitIntVal([NotNull] SpreadsheetParser.IntValContext context)
        {
            int value = int.Parse(context.INT().GetText());
            LastValue = new IntValue(value);
            return true;
        }

        public override bool VisitEmptyVal([NotNull] SpreadsheetParser.EmptyValContext context)
        {
            LastValue = new EmptyValue();
            return true;
        }
    }
}
