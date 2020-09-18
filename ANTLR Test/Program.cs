using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ANTLR_Test.Classes;

namespace ANTLR_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ErrorHandler handler = new ErrorHandler();
            StreamReader reader = File.OpenText("Testsuite/test-good1.xl");
            AntlrInputStream inputStream = new AntlrInputStream(reader);
            SpreadsheetLexer spreadsheetLexer = new SpreadsheetLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(spreadsheetLexer);
            SpreadsheetParser spreadsheetParser = new SpreadsheetParser(commonTokenStream);

            SpreadsheetParser.SpreadSheetContext context = spreadsheetParser.spreadSheet();
            SpreadsheetVisitor visitor = new TypecheckVisitor(handler);

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Console.WriteLine(reader.ReadToEnd());
            Console.WriteLine();
            Console.WriteLine(visitor.Visit(context));
            Console.WriteLine();
            foreach (var value in visitor.Repository.Cells)
            {
                Console.WriteLine($"{value.Key.Item1}, {value.Key.Item2}: {value.Value.ToString()}");
            }
            Console.ReadLine();
        }
    }
}
