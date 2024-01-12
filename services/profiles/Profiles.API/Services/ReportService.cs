using EasyGas.Shared.Enums;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Services
{
    public class ReportService : IReportService
    {
        private readonly ProfilesDbContext _db;
        public ReportService(ProfilesDbContext db)
        {
            _db = db;
        }
        public async Task<string> CreateExcel (TransactionSummaryVM model)
        {
            try
            {
                
                var dataTable = ToDataTable(model.Orders);
                DataSet dataSet = new DataSet();
                dataSet.Tables.Add(dataTable);
                if (!Directory.Exists("exl\\"))
                {
                    Directory.CreateDirectory("exl\\");
                }
                var dist = _db.Users.Include(u => u.Profile).Include(u => u.Addresses).Where(p => p.Id == model.DistributorId && p.Type == UserType.DISTRIBUTOR).FirstOrDefault() ?? throw new ArgumentNullException(nameof(model.DistributorId), "Distributor could not be found");
                var _name = dist.Profile.FirstName + "_" + model.Month.ToString() + "_" + model.Year.ToString() + ".xlsx";
                string workbookpath = Directory.GetCurrentDirectory();
                workbookpath += "\\exl\\" + _name;
                int i = 1;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    foreach(DataTable table in dataSet.Tables)
                    {
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(table.TableName);
                        worksheet.Cells["A1"].LoadFromDataTable(table, true);
                        worksheet.Cells.Style.Font.SetFromFont("Calibri", 10);
                        worksheet.Cells.AutoFitColumns();
                        //Format the header    
                        using (ExcelRange objRange = worksheet.Cells["A1:XFD1"])
                        {
                            
                            objRange.Style.Font.Bold = true;
                            objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            
                            
                        }
                        i++;
                    }
                    if (File.Exists(workbookpath))
                    {
                        File.Delete(workbookpath);
                    }
                    FileStream fileStream = File.Create(workbookpath);
                    fileStream.Close();
                    File.WriteAllBytes(workbookpath, excelPackage.GetAsByteArray());
                }
                return _name;
            }
            catch(Exception ex)
            {
                return ex.Message.ToString();
            }
            

        }

        /*
        public Task<byte[]> GetExcelFile(List<OrderModel> orderList)
        {
            try
            {
                var dataTable = ToDataTable(orderList);
                DataSet dataSet = new DataSet();
                dataSet.Tables.Add(dataTable);

                var fileName = "Test.xlsx";
                int i = 1;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    foreach (DataTable table in dataSet.Tables)
                    {
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(table.TableName);
                        worksheet.Cells["A1"].LoadFromDataTable(table, true);
                        worksheet.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));
                        worksheet.Cells.AutoFitColumns();
                        //Format the header    
                        using (ExcelRange objRange = worksheet.Cells["A1:XFD1"])
                        {
                            objRange.Style.Font.Bold = true;
                            objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }
                        i++;
                    }

                    
                    return Task.FromResult(excelPackage.GetAsByteArray());
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }
        */

        private System.Data.DataTable ToDataTable<T>(List<T> items)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
                
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}
