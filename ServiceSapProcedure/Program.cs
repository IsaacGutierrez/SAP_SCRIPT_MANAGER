using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ServiceSapProcedure
{
    class Program
    {
        private static int count = 0;
        static void Main(string[] args)
        {
            setUp();
            Excecute();
          
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval =  24 * 60 * 60 * 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
        
            Console.ReadKey();

        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Excecute();        }

        public static void Excecute()
        {
            Console.WriteLine("Starting service");
            Process scriptProc = new Process();

            /////CAMBIAR SCRIPT
            scriptProc.StartInfo.FileName = @"BillRep.vbs";
            scriptProc.StartInfo.WorkingDirectory = ConfigurationManager.AppSettings["SCRIPT_REPOSITORY_DIRECTORY"]; // <---very important 
            scriptProc.StartInfo.Arguments = "//B //Nologo vbscript.vbs";
            scriptProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //prevent console window from popping up
            scriptProc.Start();
            scriptProc.WaitForExit(); // <-- Optional if you want program running until your script exit
            scriptProc.Close();
            Console.WriteLine("Ending service");
           
        }


        private static void setUp()
        {
            string MONITORED_DIRECTORY_FILE = ConfigurationManager.AppSettings["SOURCE_DATA_DIRECTORY"];
            string MONITORED_DIRECTORY_FILE_NORMALIZED = ConfigurationManager.AppSettings["SOURCE_DATA_DIRECTORY_NORMALIZED_FILES"];

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
            FileSystemWatcher fileSystemWatcherNormalizeFiles = new FileSystemWatcher();

            fileSystemWatcher.Path = MONITORED_DIRECTORY_FILE;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.IncludeSubdirectories = false;
            fileSystemWatcher.Filter = "*.*";

            fileSystemWatcherNormalizeFiles.Path = MONITORED_DIRECTORY_FILE_NORMALIZED;
            fileSystemWatcherNormalizeFiles.Created += FileSystemWatcherNormatized_Created;
            fileSystemWatcherNormalizeFiles.EnableRaisingEvents = true;
        }

        private static void FileSystemWatcherNormatized_Created(object sender, FileSystemEventArgs e)
        {
            /// Upload and save the file
            //string excelPath = e.FullPath;
            count = 0;
            string conString = string.Empty;
            string extension = ".xls";
            switch (extension)
            {
                case ".xls": //Excel 97-03
                    conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                    break;
                case ".xlsx": //Excel 07 or higher
                    conString = ConfigurationManager.ConnectionStrings["Excel07+ConString"].ConnectionString;
                    break;

            }
            conString = string.Format(conString, e.FullPath);
            using (OleDbConnection excel_con = new OleDbConnection(conString))
            {
                excel_con.Open();
                string sheet1 = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["TABLE_NAME"].ToString();
                System.Data.DataTable dtExcelData;

                //[OPTIONAL]: It is recommended as otherwise the data will be considered as String by default.
                using (OleDbDataAdapter oda = new OleDbDataAdapter("SELECT * FROM [" + sheet1 + "]", excel_con))
                {
                    var ds = new DataSet();
                    oda.Fill(ds);
                    dtExcelData = ds.Tables[0];
                    oda.Fill(dtExcelData);
                }
                excel_con.Close();

                string consString = ConfigurationManager.ConnectionStrings["BillRep"].ConnectionString;
                using (SqlConnection con = new SqlConnection(consString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {


                        //Set the database table name


                        /////////CAMBIAR TABLA DE SQL
                        ///PRE
                        ///LA TABLA DE LA BASE DE DATOS DEBE TENER LOS MISMOS CAMPOS DEL EXCEL
                       
                        sqlBulkCopy.DestinationTableName = "dbo.SAP_Z1026_BILLREP_ESP";
                        sqlBulkCopy.BulkCopyTimeout = 1000000000;
                        //[OPTIONAL]: Map the Excel columns with that of the database table
                        con.Open();
                        sqlBulkCopy.WriteToServer(dtExcelData);
                        con.Close();

                        Console.WriteLine("insertion done! "+ DateTime.Now.ToShortDateString());
                    }
                }
            }
        }

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created )
            {
                if (!e.Name.Contains("normalized"))
                {
                    count++;
                    normalize(e.FullPath);
                }
               
            }  
        }

        private static void normalize(string filePath)
        {
            if (!File.Exists(filePath)) return; 
            _Application docExcel = new Microsoft.Office.Interop.Excel.Application { Visible = false};
            var workbooksExcel = docExcel.Workbooks.Open(filePath);
            var worksheetExcel = (_Worksheet)workbooksExcel.ActiveSheet;

            Console.WriteLine("Path: "+ filePath);

            ((Range)worksheetExcel.Rows[1, Missing.Value]).Delete(XlDeleteShiftDirection.xlShiftUp);
            ((Range)worksheetExcel.Rows[1, Missing.Value]).Delete(XlDeleteShiftDirection.xlShiftUp);
            ((Range)worksheetExcel.Rows[1, Missing.Value]).Delete(XlDeleteShiftDirection.xlShiftUp);
            ((Range)worksheetExcel.Rows[2, Missing.Value]).Delete(XlDeleteShiftDirection.xlShiftUp);

            string MONITORED_DIRECTORY_FILE_NORMALIZED = ConfigurationManager.AppSettings["SOURCE_DATA_DIRECTORY_NORMALIZED_FILES"];
            string[] path =  filePath.Split('\\');
            string fileName = path[path.Length - 1];
           
            workbooksExcel.SaveAs(filePath+"normalized.xls",  Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                       Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlUserResolution, true,
            Missing.Value, Missing.Value, Missing.Value);

            Console.WriteLine("Path: " + filePath + "normalized.xls");

            Marshal.ReleaseComObject(docExcel);

            File.Copy(filePath + "normalized.xls", MONITORED_DIRECTORY_FILE_NORMALIZED + '\\' + "normalized_" + fileName);
        }
    }
}

