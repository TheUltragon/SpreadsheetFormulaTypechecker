using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{

    //Mostly copied from: https://social.msdn.microsoft.com/Forums/de-DE/2c1adc81-7279-465d-a932-9f9d84bac978/excel-tabelle-mit-oledb-auslesen?forum=visualcsharpde
    class ExcelReaderImporter : SpreadSheetImporter
    {
        private string _newFile;

        public override void ImportFile(string path)
        {
            DataSet data = GetExcelData(path);
            for(int i = 0; i<data.Tables.Count; i++)
            {
                var table = data.Tables[i];
                for(int j = 0; j< table.Rows.Count; j++)
                {
                    var row = table.Rows[j];
                    for(int k = 0; k < table.Columns.Count; k++)
                    {
                        var field = row.Field<string>(k);
                        if (!string.IsNullOrEmpty(field))
                        {
                            Logger.DebugLine($"Field [{j}, {k}]: {field}");
    
                            //Do something with field

                        }
                    }
                }
            }
        }

        public DataSet GetExcelData(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new NullReferenceException("Der Dateiname darf nicht NULL oder Leer sein !");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Die angegebene Datei wurde nicht gefunden", filePath,
                                                new Exception("Datei nicht gefunden"));

            return GetData(filePath);
        }

        private DataSet GetData(string filePath)
        {
            var set = new DataSet(Path.GetFileName(filePath));
            using (var connection = new OleDbConnection(GetExcelConnectionString(filePath)))
            {
                try
                {
                    connection.Open();
                }
                catch (OleDbException ex)
                {
                    if (ex.Message.Contains("exklusiv"))
                        return GetData(CopyFileAndReturnNewConnection(filePath));

                }
                var tablenames = GetDataSheetName(connection.GetSchema("Tables"));

                foreach (string s in tablenames)
                {
                    var select = String.Format("SELECT * FROM [{0}]", s.Replace("'", ""));
                    try
                    {
                        using (var adapter = new OleDbDataAdapter(select, connection))
                        {
                            var dt = new DataTable(s);
                            adapter.Fill(dt);
                            set.Tables.Add(dt);
                        }
                    }
                    catch (Exception ex)
                    {

                        Logger.DebugLine($"{select} konnte nicht ausgeführt werden : {ex.Message}");
                    }
                }
            }
            if (!string.IsNullOrEmpty(_newFile))
                File.Delete(_newFile);
            _newFile = null;


            return set;
        }


        private string CopyFileAndReturnNewConnection(string filePath)
        {
            _newFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath));
            File.Copy(filePath, _newFile, true);
            return _newFile;
        }

        private IEnumerable<string> GetDataSheetName(DataTable table)
        {
            return from DataRow row in table.Rows select row["TABLE_NAME"].ToString();
        }

        private string GetExcelConnectionString(string filePath)
        {
            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=\"{0}\";Extended Properties=\"Excel 8.0;IMEX=1\"", filePath);
        }
    }
}
