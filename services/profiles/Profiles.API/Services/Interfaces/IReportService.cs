using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Services
{
    public interface IReportService
    {
        Task<string> CreateExcel(TransactionSummaryVM model);
        //Task<byte[]> GetExcelFile(List<OrderModel> orderList);
    }
}
