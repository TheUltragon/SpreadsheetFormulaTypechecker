using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToExcel;
using LinqToExcel.Query;

namespace ANTLR_Test.Classes
{
    public static class SpreadSheetImport
    {
        public static void ImportFile(string path)
        {
            Logger.Debug($"Importing File {path}");
            var excel = new ExcelQueryFactory(path);
            excel.TrimSpaces = TrimSpacesType.Both;
            List<string> worksheets = excel.GetWorksheetNames().ToList();
            Logger.Debug($"Importing {worksheets.Count} worksheets");
            //Each worksheet, in worksheet each row, in row each cell
            foreach (var worksheet in worksheets)
            {
                var rowCounter = 1;
                Logger.Debug($"Importing {excel.WorksheetNoHeader(worksheet).Count()} rows");
                foreach (var row in excel.WorksheetNoHeader(worksheet))
                {
                    Logger.Debug($"Importing {row.Count()} cells");
                    var cellCounter = 1;
                    foreach (var cell in row)
                    {
                        if(cell.Value is string)
                        {
                            string content = (string)cell.Value;
                            Logger.Debug($"Cell {cellCounter} has content: {content}");
                        }
                        else
                        {
                            Logger.Debug($"Cell {cellCounter} is not string! Value: {cell.Value.ToString()}");
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
