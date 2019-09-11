using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;

namespace ServiceSapProcedure.Repositories
{
    public class Excel
    {
        string path = "";
        _Application excel = new _Excel.Application();
        Workbook wb;
        Worksheet ws;

        public Excel(string path, int Sheet){
            this.path = path;
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[Sheet];
        }

        public int CantidadFilas()
        {
            Range range = ws.UsedRange;
            int lastUsedRow = range.Row + range.Rows.Count - 1;
            return lastUsedRow;
        }

        public string[,] LeerRango()
        {
            Range range = ws.UsedRange;
            int lastUsedRow = range.Row + range.Rows.Count - 1;
            int lastUsedColumns = range.Column + range.Columns.Count - 1;
            object[,] holder = range.Value2;
            
            string[,] returnstring = new string[lastUsedRow, lastUsedColumns];
            for (int c = 6; c <= lastUsedRow; c++)
            {
                //OrgVt
                if (holder[c, 3] != null)
                    returnstring[c - 1, 0] = holder[c, 3].ToString();
                else
                    returnstring[c - 1, 0] = "";

                //Se
                if (holder[c, 4] != null)
                    returnstring[c - 1, 1] = holder[c, 4].ToString();
                else
                    returnstring[c - 1, 1] = "";

                //Ce.
                if (holder[c, 5] != null)
                    returnstring[c - 1, 1] = holder[c, 5].ToString();
                else
                    returnstring[c - 1, 1] = "";


            }
            return returnstring;
        }

        public void Save()
        {
            wb.Save();
        }

        public void SaveAs(string path)
        {
            wb.SaveAs(path);
        }

        public void Close()
        {
            wb.Close();
        }

    }
}