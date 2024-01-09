using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profiles.API.Controllers;
using Profiles.API.Models;
using Wkhtmltopdf.NetCore;

namespace EasyGas.Services.Profiles.BizLogic
{
    
    public class InvoiceMgr : BaseApiController
    {
        private ProfilesDbContext _db;
        private readonly ILogger _logger;
        private readonly IGeneratePdf _generatePdf;
        private readonly IOptions<ApiSettings> _apiSettings;

        public InvoiceMgr(ProfilesDbContext db,
            IOptions<ApiSettings> apiSettings,
            IGeneratePdf generatePdf,
            ILogger<InvoiceMgr> logger,
            ICommandBus bus):base(bus)
        {
            _db = db;
            _generatePdf = generatePdf;
            _apiSettings = apiSettings;
            _logger = logger;
        }

        public async Task<CreateInvoiceResponse> AddInvoiceAndDigitalVoucher(CustomerOrderDeliveredIntegrationEvent @event)
        {
            CreateInvoiceResponse response = new CreateInvoiceResponse()
            {
                IsSuccess = false
            };

            try
            {
                var customer = await _db.Users.Include(p => p.Profile).Where(p => p.Id == @event.UserId).FirstOrDefaultAsync();
                BusinessEntity relaypoint = await _db.BusinessEntities.Include(p => p.Branch).Where(p => p.Id == @event.BusinessEntityId).FirstOrDefaultAsync();

                if (customer == null || relaypoint == null)
                {
                    _logger.LogCritical("InvoiceMgr.AddInvoiceAndDigitalVoucher Invalid payload | {@event}", @event);
                    response.Message = "Invalid customer or relaypoint.";
                    return response;
                }

                var invoiceHtmlString = GetInvoiceHtml(@event, customer, relaypoint);

                bool invoicePdfcreated = false;
                if (!string.IsNullOrEmpty(invoiceHtmlString))
                {
                    invoicePdfcreated = SaveInvoiceAsPdfToBlob(invoiceHtmlString, @event.InvoiceFilename);
                }
                
                if (!invoicePdfcreated)
                {
                    //_logger.LogCritical("InvoiceMgr.SaveInvoiceAsPdfToBlob | Failed | " + invoiceModel.InvoiceNumber + " uploaded to blob failed | orderId: " + orderId + ", invoiceId: " + invoice.Id);
                    response.Message = "Could not create pdf and upload to blob.";
                    return response;
                }

                foreach(var voucher in @event.Vouchers)
                {
                    var voucherHtmlString = GetDigitalVoucherHtml(@event, customer, voucher);

                    bool voucherPdfcreated = false;
                    if (!string.IsNullOrEmpty(voucherHtmlString))
                    {
                        voucherPdfcreated = SaveVoucherAsPdfToBlob(voucherHtmlString, voucher.VoucherFilename);
                    }

                    if (!voucherPdfcreated)
                    {
                        //_logger.LogCritical("InvoiceMgr.SaveInvoiceAsPdfToBlob | Failed | " + invoiceModel.InvoiceNumber + " uploaded to blob failed | orderId: " + orderId + ", invoiceId: " + invoice.Id);
                        response.Message = $"Could not create voucher pdf for {voucher.VoucherCode} and upload to blob.";
                        return response;
                    }
                }

                response.IsSuccess = true;
                response.InvoiceHtmlString = invoiceHtmlString;
                response.InvoicePdfUrl = _apiSettings.Value.BlobStorageBaseUrl + _apiSettings.Value.BlobCustomerInvoicePdfContainer + "/" + @event.InvoiceFilename;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("OrderMgr.AddInvoice Exception | orderId: " +@event.OrderId + " | " + ex.ToString());
                response.Message = "Some internal error has occured.";
                return response;
            }
        }

        public string GetInvoiceHtml(CustomerOrderDeliveredIntegrationEvent @event,User customer, BusinessEntity relaypoint)
        {
            try
            {
                var url = "Templates\\invoice.html";

                var distDetails = relaypoint.Location;
                if (!string.IsNullOrEmpty(relaypoint.PinCode))
                {
                    distDetails += " - " + relaypoint.PinCode;
                }

                string customerNo;
                if (!string.IsNullOrEmpty(customer.Profile.Code))
                {
                    customerNo = customer.Profile.Code;
                }
                else
                {
                    customerNo = customer.Id.ToString().PadLeft(4, '0');
                }

                var custAddress = @event.CustomerLocation;
                if (!string.IsNullOrEmpty(@event.CustomerPincode))
                {
                    custAddress += " - " + @event.CustomerPincode;
                }

                WebClient client = new WebClient();
                String htmlCode = client.DownloadString(url);

                //_logger.LogInformation("InvoiceGenerator htmlCode Template: " + htmlCode);

                var itemTableRowString = "<tr style=\"border:1px solid #0000\"><td class=\"sl\" style=\"border:1px solid #0000; text-align:center; \">{Sl}</td><td class=\"prodDescr\" style=\"border:1px solid #0000; text-align:center;\">{Item}</td><td class=\"hsn\" style=\"border:1px solid #0000; text-align:center;\">{Hsn}</td><td class=\"quantity\" style=\"border:1px solid #0000; text-align:center;\">{Quantity}</td><td class=\"uom\" style=\"border:1px solid #0000; text-align:center;\">{UOM}</td><td class=\"rate\" style=\"width: 80px; border:1px solid #0000; text-align:center;\" >{Rate}</td><td class=\"netRate\" style=\"border:1px solid #0000; text-align:center;\">{NetRate}</td><td class=\"taxableValue\" style=\"border:1px solid #0000; text-align:center;\">{TaxableValue}</td></tr>";
                htmlCode = htmlCode.Replace("{Inv-No}", @event.Code);
                htmlCode = htmlCode.Replace("{Inv-Date}", @event.DeliveredAt.ToString("dd/MM/yyyy"));
                htmlCode = htmlCode.Replace("{PO-Date}", @event.DeliveredAt.ToString("dd/MM/yyyy"));
                htmlCode = htmlCode.Replace("{PO-No}", @event.Code);
                htmlCode = htmlCode.Replace("{Eway-No}", @event.Code);
                htmlCode = htmlCode.Replace("{Order-No}", @event.Code);
                htmlCode = htmlCode.Replace("{Order-Date}", @event.DeliveredAt.ToString("dd/MM/yyyy"));
                htmlCode = htmlCode.Replace("{Address}", custAddress);
                htmlCode = htmlCode.Replace("{PlaceOfSupply}", relaypoint.Branch.Name);
                htmlCode = htmlCode.Replace("{CustomerNo}", customerNo);
                htmlCode = htmlCode.Replace("{UserName}", customer.Profile.GetFullName());
                htmlCode = htmlCode.Replace("{PhoneNumber}", customer.Profile.Mobile);
                htmlCode = htmlCode.Replace("{Email}", customer.Profile.Email);
                htmlCode = htmlCode.Replace("{DistributorName}", relaypoint.Name);
                htmlCode = htmlCode.Replace("{State}", relaypoint.Branch.Name);
                htmlCode = htmlCode.Replace("{DistributorDetails}", distDetails);
                htmlCode = htmlCode.Replace("{DistributorGst}", string.IsNullOrEmpty(relaypoint.GSTN) ? "N/A" : relaypoint.GSTN);
                htmlCode = htmlCode.Replace("{DistributorPan}", string.IsNullOrEmpty(relaypoint.PAN) ? "N/A" : relaypoint.PAN);

                var itemTable = "";
                int i = 1;
                double taxPercent = 0.05;
                foreach (var item in @event.Items)
                {
                    string uom = " ST ";

                    var tableRow = itemTableRowString;
                    //double taxablePrice = (double)item.Price - ((double)item.Price * taxPercent);
                    //double unitPrice = (double)item.UnitPrice - ((double)item.UnitPrice * taxPercent);
                    double taxablePrice = (double)item.Price / (1 + taxPercent);
                    double unitPrice = (double)item.UnitPrice / (1 + taxPercent);
                    tableRow = tableRow.Replace("{Sl}", i.ToString());
                    tableRow = tableRow.Replace("{Item}", item.ProductName);
                    tableRow = tableRow.Replace("{Hsn}", "");
                    tableRow = tableRow.Replace("{Quantity}", item.Quantity.ToString());
                    tableRow = tableRow.Replace("{Rate}", Math.Round(unitPrice, 2).ToString());
                    tableRow = tableRow.Replace("{UOM}", uom);
                    tableRow = tableRow.Replace("{NetRate}", Math.Round(taxablePrice, 2).ToString());
                    tableRow = tableRow.Replace("{TaxableValue}", Math.Round(taxablePrice, 2).ToString());
                    itemTable += tableRow;
                    i++;
                }

                double productNetRate = (double)@event.TotalProductPrice / (1 + taxPercent);
                itemTable += "<tr><td colspan=\"6\" style=\"text-align:center; font-weight:bold;\">Total Product Price</td><td style=\"text-align:center; font-weight: bold \">{ProductNetRate}</td><td style=\"text-align:center; font-weight: bold \">{ProductNetRate}</td></tr>";
                itemTable = itemTable.Replace("{ProductNetRate}", Math.Round(productNetRate, 2).ToString());

                double deliveryNetRate = 0;
                if (@event.TotalDeliveryPrice > 0)
                {
                    deliveryNetRate = (double)@event.TotalDeliveryPrice / (1 + taxPercent);
                    itemTable += "<tr><td colspan=\"6\" style=\"text-align:center; font-weight:bold;\">Delivery Price</td><td style=\"text-align:center; font-weight: bold \">{DeliveryNetRate}</td><td style=\"text-align:center; font-weight: bold \">{DeliveryNetRate}</td></tr>";
                    itemTable = itemTable.Replace("{DeliveryNetRate}", Math.Round(deliveryNetRate, 2).ToString());
                }

                if (@event.TotalDiscountPrice > 0)
                {
                    itemTable += "<tr><td colspan=\"6\" style=\"text-align:center; font-weight:bold;\">Discount Price</td><td style=\"text-align:center; \">{DiscountNetRate}</td><td style=\"text-align:center; \">-{DiscountNetRate}</td></tr>";
                    itemTable = itemTable.Replace("{DiscountNetRate}", Math.Round(@event.TotalDiscountPrice, 2).ToString());
                }

                htmlCode = htmlCode.Replace("{TableBody}", itemTable);

                double totalTaxPrice = (productNetRate + deliveryNetRate) * taxPercent;
                htmlCode = htmlCode.Replace("{TaxAmount}", Math.Round(totalTaxPrice, 2).ToString());

                htmlCode = htmlCode.Replace("{CGSTAmount}", Math.Round(totalTaxPrice / 2, 2).ToString());
                htmlCode = htmlCode.Replace("{SGSTAmount}", Math.Round(totalTaxPrice / 2, 2).ToString());

                htmlCode = htmlCode.Replace("{TotalAmount}", Math.Round(@event.TotalPrice, 2).ToString());
                var amntInWords = NumericToStringFormatter.ConvertNumericToWords((double)Math.Round(@event.TotalPrice, 2));
                htmlCode = htmlCode.Replace("{TotalAmountInWords}", amntInWords);

                return htmlCode;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("InvoiceGenerator ex " + ex.Message);
                return "";
            }
        }

        public string GetDigitalVoucherHtml(CustomerOrderDeliveredIntegrationEvent @event, User customer, OrderDigitalVoucherEvent voucher )
        {
            try
            {
                var url = "Templates\\voucher.html";
                var deliveredAt = @event.DeliveredAt;
                decimal totalAmount = voucher.Deposit;

                var customerNo = customer.Profile.Code;
                var custAddress = @event.CustomerLocation + "-" + @event.CustomerPincode;

                WebClient client = new WebClient();
                String htmlCode = client.DownloadString(url);

                _logger.LogInformation("InvoiceGenerator htmlCode Template: " + htmlCode);

                htmlCode = htmlCode.Replace("{VoucherCode}",voucher.VoucherCode);
                htmlCode = htmlCode.Replace("{OrderCode}", @event.Code);
                htmlCode = htmlCode.Replace("{ConnectionDate}", deliveredAt.ToString("dd/MM/yyyy"));
                htmlCode = htmlCode.Replace("{Address}", custAddress);
                htmlCode = htmlCode.Replace("{UserName}", customer.Profile.GetFullName());
                htmlCode = htmlCode.Replace("{PhoneNumber}", customer.Profile.Mobile);
                htmlCode = htmlCode.Replace("{Email}", customer.Profile.Email);
                htmlCode = htmlCode.Replace("{Amount}", Math.Round(totalAmount, 2).ToString());
                var amntInWords = NumericToStringFormatter.ConvertNumericToWords((double)Math.Round(totalAmount, 2));
                htmlCode = htmlCode.Replace("{TotalAmountInWords}", amntInWords);
                //_logger.LogInformation("InvoiceGenerator last htmlCode " + htmlCode);
                return htmlCode;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("VoucherHtmlGenerator ex " + ex.Message);
                return "";
            }
        }

        private bool SaveInvoiceAsPdfToBlob(string invoiceHtmlString, string invoiceFilename)
        {
            try
            {
                //var htmlString = InvoiceGenerator(invoice, order, itemList, dist);
                var pdfBytes = _generatePdf.GetPDF(invoiceHtmlString);
                var command = new UploadInvoiceCommand(invoiceFilename, pdfBytes, "application/pdf");
                var uploadToBlobResult = ProcessCommand(command);
                var uploadToBlobCommandResult = uploadToBlobResult as CommandResult;
                if (uploadToBlobCommandResult.IsOk)
                {
                    _logger.LogInformation("InvoiceMgr.SaveInvoiceAsPdfToBlob " + invoiceFilename + " uploaded to blob Success ");
                    return true;
                }
                else
                {
                    _logger.LogCritical("InvoiceMgr.SaveInvoiceAsPdfToBlob " + invoiceFilename + " uploaded to blob Failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("InvoiceMgr.SaveInvoiceAsPdfToBlob " + invoiceFilename + " uploaded to blob failed exception: " + ex.ToString());
            }
            return false;
        }

        private bool SaveVoucherAsPdfToBlob(string invoiceHtmlString, string invoiceFilename)
        {
            try
            {
                //var htmlString = InvoiceGenerator(invoice, order, itemList, dist);
                var pdfBytes = _generatePdf.GetPDF(invoiceHtmlString);
                var command = new UploadVoucherPdfCommand(invoiceFilename, pdfBytes, "application/pdf");
                var uploadToBlobResult = ProcessCommand(command);
                var uploadToBlobCommandResult = uploadToBlobResult as CommandResult;
                if (uploadToBlobCommandResult.IsOk)
                {
                    _logger.LogInformation("InvoiceMgr.SaveVoucherAsPdfToBlob " + invoiceFilename + " uploaded to blob Success ");
                    return true;
                }
                else
                {
                    _logger.LogCritical("InvoiceMgr.SaveVoucherAsPdfToBlob " + invoiceFilename + " uploaded to blob Failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("InvoiceMgr.SaveVoucherAsPdfToBlob " + invoiceFilename + " uploaded to blob failed exception: " + ex.ToString());
            }
            return false;
        }
    }
    
}
