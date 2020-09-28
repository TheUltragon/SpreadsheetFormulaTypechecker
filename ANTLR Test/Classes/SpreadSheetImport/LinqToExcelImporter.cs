using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToExcel;
using LinqToExcel.Query;

namespace ANTLR_Test.Classes
{
    public class LinqToExcelImporter : SpreadSheetImporter
    {
        public override void ImportFile(string path)
        {
            Logger.DebugLine($"Importing File {path}");
            var excel = new ExcelQueryFactory(path);
            excel.TrimSpaces = TrimSpacesType.Both;
            List<string> worksheets = excel.GetWorksheetNames().ToList();
            Logger.DebugLine($"Importing {worksheets.Count} worksheets");
            //Each worksheet, in worksheet each row, in row each cell
            foreach (var worksheet in worksheets)
            {
                var rowCounter = 1;
                Logger.DebugLine($"Importing {excel.WorksheetNoHeader(worksheet).Count()} rows");
                foreach (var row in excel.WorksheetNoHeader(worksheet))
                {
                    Logger.DebugLine($"Importing {row.Count()} cells");
                    var cellCounter = 1;
                    foreach (var cell in row)
                    {
                        if(cell.Value is string)
                        {
                            string content = (string)cell.Value;
                            Logger.DebugLine($"Cell {cellCounter} has content: {content}");
                        }
                        else
                        {
                            Logger.DebugLine($"Cell {cellCounter} is not string! Value: {cell.Value.ToString()}");
                        }

                        //Do something with content


                        cellCounter++;
                    }
                    rowCounter++;
                }
            }
        }
    }
}
