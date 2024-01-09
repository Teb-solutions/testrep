using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class ImportMgr
    {
        private readonly ProfilesDbContext _db;
        private readonly ILogger _logger;

        public ImportMgr(ProfilesDbContext db, ILoggerFactory loggerFactory)
        {
            _db = db;
           _logger = loggerFactory.CreateLogger<ImportMgr>();
        }

        /*
        public async Task<CommandResult> ImportVardhamanCustomers(VardhamanCustomerVM model)
        {
            List<string> validationErrors = new List<string>();
            DateTime now = DateMgr.GetCurrentIndiaTime();
            int savedCustomers = 0;
            int row = 1;
            try
            {
                var tenant = _db.Tenants.Where(p => p.Id == model.TenantId).FirstOrDefault();
                var branch = _db.Branches.Where(p => p.Id == model.BranchId).FirstOrDefault();
                if (tenant == null)
                {
                    validationErrors.Add("Tenant is invalid.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                if (branch == null)
                {
                    validationErrors.Add("Branch is invalid.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }

                foreach (var importCust in model.CustomerList)
                {
                    string errorString = row + " : ";
                    bool isValid = true;
                    if (String.IsNullOrEmpty(importCust.CODE))
                    {
                        errorString += "Code is empty ";
                        isValid = false;
                    }
                    if (String.IsNullOrEmpty(importCust.PARTY_NAME))
                    {
                        errorString += "Party Name is empty ";
                        isValid = false;
                    }
                    if (String.IsNullOrEmpty(importCust.ADDRESS))
                    {
                        errorString += "Address is empty ";
                        isValid = false;
                    }
                    if (String.IsNullOrEmpty(importCust.LAT))
                    {
                        errorString += "Lat is empty ";
                        isValid = false;
                    }
                    if (String.IsNullOrEmpty(importCust.LNG))
                    {
                        errorString += "Lng is empty ";
                        isValid = false;
                    }

                    float? lat = null;
                    float? lng = null;
                    try
                    {
                        lat = (float)Convert.ToDecimal(importCust.LAT);
                        lng = (float)Convert.ToDecimal(importCust.LNG);
                    }
                    catch(Exception formatEx)
                    {
                        errorString += "Lat/Lng format is invalid ";
                        isValid = false;
                    }
                    if (lng < -180 || lng > 180 || lat < -90 || lat > 90)
                    {
                        errorString += "Lat/Lng is invalid ";
                        isValid = false;
                    }

                    int pincode = 0; 
                    try
                    {
                        if (!string.IsNullOrEmpty(importCust.PIN))
                        {
                            pincode = Convert.ToInt32(importCust.PIN);
                        }
                    }
                    catch (Exception formatEx2)
                    {
                        errorString += "Pincode is invalid ";
                        isValid = false;
                    }

                    int codeLength = importCust.CODE.Length;
                    int padCount = 10 - codeLength;
                    var username = importCust.CODE.PadLeft(importCust.CODE.Length + padCount, '0');
                    var exists = _db.Users.Where(p => p.UserName == username).Any();
                    if (exists)
                    {
                        errorString += "A customer with code " + username + " already exists ";
                        isValid = false;
                    }
                    if (isValid)
                    {
                        
                        User user = new User
                        {
                            BranchId = model.BranchId,
                            TenantId = model.TenantId,
                            CreationType = CreationType.USER,
                            Type = UserType.CUSTOMER,
                            UserName = username,
                            Password = Helper.HashPassword(importCust.CODE),
                            CreatedAt = now,
                            UpdatedAt = now,
                            CreatedBy = "admin import"
                        };
                        _db.Users.Add(user);
                        await _db.SaveChangesAsync();
                        
                        UserProfile profile = new UserProfile
                        {
                            UserId = user.Id,
                            FirstName = importCust.PARTY_NAME,
                            Gender = Gender.NotSpecified,
                            Mobile = username,
                            Source = Source.ADMIN_WEB,
                            CreatedAt = now,
                            UpdatedAt = now,
                            //Salesman = importCust.SALESMANALTERCODE + " " + importCust.SALESMAN,
                        };
                        _db.Profiles.Add(profile);
                        await _db.SaveChangesAsync();

                        UserAddress address = new UserAddress
                        {
                            UserId = user.Id,
                            Lat = (double)lat,
                            Lng = (double)lng,
                            Location = importCust.ADDRESS_COMBINED,
                            BuildingNo = importCust.ADDRESS,
                            StreetNo = importCust.ADDRESS1,
                            Landmark = importCust.ADDRESS2,
                            District = importCust.ADDRESS3,
                            PinCode = pincode,
                        };
                        
                        _db.Addresses.Add(address);
                        await _db.SaveChangesAsync();
                        savedCustomers += 1;
                    }
                    else
                    {
                        validationErrors.Add(errorString);
                    }

                    row++;
                }
            }
            catch(Exception ex)
            {
                validationErrors.Add(row + " exception has occured | "+ ex.ToString());
            }

            if (validationErrors.Count() > 0)
            {
                validationErrors.Add("Total " + savedCustomers + " customers imported");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
            if (savedCustomers > 0)
            {
                return new CommandResult(System.Net.HttpStatusCode.OK, "Total " + savedCustomers + " customers imported");
            }
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }

        public async Task<CommandResult> ImportVardhamanOrders(VardhamanOrderVM model)
        {
            //removing tracking at this time 
            _db.ChangeTracker.AutoDetectChangesEnabled = false;
            List<string> validationErrors = new List<string>();
            DateTime now = DateMgr.GetCurrentIndiaTime();
            int savedCustomers = 0;
            int row = 1;
            int MODEDIV = 300;
            try
            {
                var deliverySlot = _db.DeliverySlots.Where(p => p.Id == model.DeliverySlotId).FirstOrDefault();
                var tenant = _db.Tenants.Where(p => p.Id == model.TenantId).FirstOrDefault();
                var branch = _db.Branches.Where(p => p.Id == model.BranchId).FirstOrDefault();
                if (deliverySlot == null)
                {
                    validationErrors.Add("Delivery Slot is invalid.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                if (tenant == null)
                {
                    validationErrors.Add("Tenant is invalid.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                if (branch == null)
                {
                    validationErrors.Add("Branch is invalid.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                var paymentMode = _db.PaymentModes.FirstOrDefault();
                if (paymentMode == null)
                {
                    validationErrors.Add("Payment Mode is not added.");
                    return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
                }
                var orderDatesList = model.OrderList.GroupBy(p => p.Date).Select(p => p.Key).ToList();
                var orderDict = new Dictionary<DateTime, List<Order>>();
                foreach( var orderDate in orderDatesList)
                {
                    orderDict[orderDate] = _db.Orders.Where(p => p.DeliveryFrom >= orderDate.Date && p.DeliveryFrom < orderDate.Date.AddDays(1)).ToList();
                }
                List<User> allUsers = _db.Users.Include(p => p.Addresses).Where(p => p.TenantId == tenant.Id && p.BranchId == branch.Id).ToList();
                //List<User> allAddresses = _db.Addresses.Where(p => p.TenantId == tenant.Id && p.BranchId == branch.Id).ToList();

                foreach (var importOrder in model.OrderList)
                {
                    string errorString = row + " - " + importOrder.Code + " : ";
                    bool isValid = true;
                    if (String.IsNullOrEmpty(importOrder.Code))
                    {
                        errorString += "Code is empty ";
                        isValid = false;
                    }

                    int codeLength = importOrder.Code.Length;
                    int padCount = 10 - codeLength;
                    var username = importOrder.Code.PadLeft(importOrder.Code.Length + padCount, '0');
                    var customer = allUsers.Where(p => p.UserName == username).FirstOrDefault();
                    var address = new UserAddress();
                    if (customer == null)
                    {
                        errorString += "Customer with code " + username + " does not exist in this branch";
                        isValid = false;
                    }
                    else
                    {
                        if (customer.Addresses == null || customer.Addresses.Count == 0)
                        {
                            errorString += "Customer does not have address ";
                            isValid = false;
                        }
                        else
                        {
                            address = customer.Addresses.First();
                            if (orderDict.ContainsKey(importOrder.Date))
                            {
                                bool orderExists = orderDict[importOrder.Date].Any(p => p.UserId == customer.Id && p.DeliverySlotId == deliverySlot.Id);
                                if (orderExists)
                                {
                                    errorString += "Order for customer " + importOrder.Code + " already exists";
                                    isValid = false;
                                }
                            }

                        }
                    }

                    if (isValid)
                    {
                        Order newOrder = new Order()
                        {
                            UserId = customer.Id,
                            TenantId = customer.TenantId,
                            BranchId = customer.BranchId,
                            //Address = currentOrder.Address,
                            PaymentModeId = paymentMode.Id,
                            Quantity = 1,
                            Status = OrderStatus.APPROVED,
                            TotalAmount = (float)importOrder.InvAmount,
                            Type = OrderType.REFILL,
                            Capacity = 0,
                            DeliverySlotId = deliverySlot.Id,
                            DeliveryFrom = importOrder.Date.AddSeconds(deliverySlot.FromSec),
                            DeliveryTo = importOrder.Date.AddSeconds(deliverySlot.ToSec),
                            //Code = newOrder.GetUniqueCode(),
                            AssignedManually = false,
                            Source = Source.ADMIN_WEB,
                            Priority = PulzPriority.NORMAL,
                            PrioritySetAt = DateMgr.GetCurrentIndiaTime(),
                            CreatedAt = DateMgr.GetCurrentIndiaTime(),
                            UpdatedAt = DateMgr.GetCurrentIndiaTime()
                        };
                        newOrder.Code = newOrder.GetUniqueCode();

                        _db.Orders.Add(newOrder);
                      //  await _db.SaveChangesAsync();

                        OrderAddress orderAddress = new OrderAddress
                        {
                            BuildingNo = address.BuildingNo,
                            District = address.District,
                            Landmark = address.Landmark,
                            Lat = address.Lat,
                            Lng = address.Lng,
                            Location = address.Location,
                            PhoneAlternate = address.PhoneAlternate,
                            PinCode = address.PinCode,
                            State = address.State,
                            StreetNo = address.StreetNo,
                            OrderId = newOrder.Id,
                            CreatedAt = DateMgr.GetCurrentIndiaTime(),
                            UpdatedAt = DateMgr.GetCurrentIndiaTime()
                        };
                        _db.OrderAddresses.Add(orderAddress);
                        

                        savedCustomers += 1;
                        //doing saving only 1s in 300
                        if (savedCustomers % MODEDIV == 0)
                        {
                            await _db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        validationErrors.Add(errorString);
                    }

                    row++;
                }
                //there can be some recode left for saving . save it now 
                if (savedCustomers % MODEDIV != 0)
                {
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                validationErrors.Add(row + " exception has occured | " + ex.ToString());
            }

            //bringing back entity tracking 
            _db.ChangeTracker.AutoDetectChangesEnabled = true;

            if (validationErrors.Count() > 0)
            {
                validationErrors.Add("Total " + savedCustomers + " orders imported");
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
            if (savedCustomers > 0)
            {
                return new CommandResult(System.Net.HttpStatusCode.OK, "Total " + savedCustomers + " orders imported");
            }
            return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
        }
        */
    }
}
