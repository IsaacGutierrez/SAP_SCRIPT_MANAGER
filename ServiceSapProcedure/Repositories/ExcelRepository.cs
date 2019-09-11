using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServiceSapProcedure.Repositories
{
    public class ExcelRepository : IRepository
    {
        private String _documentFullPath;
        private Microsoft.Office.Interop.Excel.Application app;

        public void setup(string fullpath)
        {
            this._documentFullPath = fullpath;
            app = new Microsoft.Office.Interop.Excel.Application();

        }
        

        public void normalize()
        {

             Microsoft.Office.Interop.Excel.Workbook Workbook = app.Workbooks.Open(this._documentFullPath);
             app.Visible = true;
             Microsoft.Office.Interop.Excel.Worksheet ExcelWorksheet = Workbook.Sheets[0];
           //  Microsoft.Office.Interop.Excel.Range TempRange = ExcelWorksheet.Range();

        }

        public void execute()
        {

        }


        
    }
}
