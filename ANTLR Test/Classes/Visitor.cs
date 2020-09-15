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
        public VariableBase LastExpValue { get; private set; }
        public VariableBase LastValue { get; private set; }



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
            if(IntVariable.IsThis(leftVal) && IntVariable.IsThis(rightVal))
            {
                int left = ((IntVariable)leftVal).Value;
                int right = ((IntVariable)rightVal).Value;
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
            LastValue = new CharVariable(value);
            return true;
        }

        public override bool VisitIntVal([NotNull] SpreadsheetParser.IntValContext context)
        {
            int value = int.Parse(context.INT().GetText());
            LastValue = new IntVariable(value);
            return true;
        }

        public override bool VisitEmptyVal([NotNull] SpreadsheetParser.EmptyValContext context)
        {
            LastValue = new EmptyVariable();
            return true;
        }
    }
}
