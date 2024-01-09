using EasyGas.Shared.Enums;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouponType = EasyGas.Shared.Enums.CouponType;
using Profiles.API.Services;
using Profiles.API.ViewModels.Wallet;
using Profiles.API.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class WalletMgr
    {
        private readonly IOptions<ApiSettings> _apiSettings;

        ProfilesDbContext _db;
        private IWalletService _walletService;
        private readonly ILogger _logger;

        public WalletMgr(IOptions<ApiSettings> apiSettings, IWalletService walletService, ILogger<WalletMgr> logger, ProfilesDbContext db)
        {
            _db = db;
            _apiSettings = apiSettings;
            _walletService = walletService;
            _logger = logger;
        }

        public async Task<bool> CreateWallet(int userId)
        {
            try
            {
                var user = _db.Users.Include(p => p.Profile).Where(p => p.Id == userId).FirstOrDefault();
                if (user == null)
                {
                    return false;
                }
                int? walletHierarchyUniqueId = 1;
                var walletType = WalletType.ENDUSER;
                if (user.Type == UserType.DISTRIBUTOR)
                {
                    walletType = WalletType.DISTRIBUTOR;
                    walletHierarchyUniqueId = user.BranchId ?? walletHierarchyUniqueId;
                }
                else if (user.Type == UserType.RELAY_POINT)
                {
                    walletType = WalletType.RELAY_POINT;
                    walletHierarchyUniqueId = user.BranchId ?? walletHierarchyUniqueId;
                }
                else if (user.Type == UserType.CUSTOMER)
                {
                    walletType = WalletType.ENDUSER;
                    walletHierarchyUniqueId = user.BranchId ?? walletHierarchyUniqueId;
                }

                if (walletHierarchyUniqueId > 0)
                {
                    WalletVM walletVM = new WalletVM()
                    {
                        UniqueId = userId,
                        UserName = user.UserName,
                        WalletHierarchyUniqueId = (int)walletHierarchyUniqueId,
                        TenantId = user.TenantId,
                        TenantUId = _apiSettings.Value.TenantUidDefault,
                        WalletType = walletType,
                        WalletName = user.UserName,
                        WalletPin = user.UserName,
                        Amount = 0
                    };

                    bool createWalletResponse = await _walletService.CreateWallet(walletVM);
                    return createWalletResponse;
                }
                
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<bool> CreateBusinessEntityWallet(int businessEntityId)
        {
            try
            {
                var businessEntity = _db.BusinessEntities.Where(p => p.Id == businessEntityId).FirstOrDefault();
                if (businessEntity == null) { return false; }

                int? walletHierarchyUniqueId = 1;
                var walletType = WalletType.ENDUSER;
                switch (businessEntity.Type)
                {
                    case Shared.Enums.BusinessEntityType.Relaypoint:
                        walletType = WalletType.DISTRIBUTOR;
                        break;
                    case Shared.Enums.BusinessEntityType.Distributor:
                        walletType = WalletType.DISTRIBUTOR;
                        break;
                    case Shared.Enums.BusinessEntityType.Dealer:
                        walletType = WalletType.EASYGAS_DEALER;
                        break;
                    case Shared.Enums.BusinessEntityType.Alds:
                        walletType = WalletType.ALDS;
                        break;
                    case Shared.Enums.BusinessEntityType.CarWash:
                        walletType = WalletType.CARSERVICE;
                        break;
                    default:
                        break;
                }

                if (walletHierarchyUniqueId > 0)
                {
                    WalletVM walletVM = new WalletVM()
                    {
                        UniqueId = businessEntity.Id,
                        UserName = businessEntity.Email,
                        WalletHierarchyUniqueId = (int)walletHierarchyUniqueId,
                        TenantId = businessEntity.TenantId,
                        TenantUId = _apiSettings.Value.TenantUidDefault,
                        WalletType = walletType,
                        WalletName = businessEntity.Name,
                        WalletPin = businessEntity.Name,
                        Amount = 0
                    };

                    bool createWalletResponse = await _walletService.CreateWallet(walletVM);
                    return createWalletResponse;
                }

            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<bool> CreateCustomerWalletWithReferral(int userId)
        {
            try
            {
                var user = _db.Users
                         .Include(p => p.Profile)
                         .Include(p => p.Profile.ReferredByUser)
                         .ThenInclude(p => p.Profile)
                         .Where(p => p.Id == userId)
                         .FirstOrDefault(); 
                
                if (user == null)
                {
                    return false;
                }

                user.BranchId = user.BranchId > 0 ? user.BranchId : 1;
                user.TenantId = user.TenantId > 0 ? user.TenantId : 1;

                WalletVM walletVM = new WalletVM()
                {
                    UniqueId = userId,
                    UserName = user.UserName,
                    WalletHierarchyUniqueId = (int)user.BranchId,
                    TenantId = user.TenantId,
                    TenantUId = _apiSettings.Value.TenantUidDefault,
                    WalletType = WalletType.ENDUSER,
                    WalletName = user.Profile.FirstName ?? user.UserName,
                    WalletPin = user.UserName,
                    Amount = 0,
                };

                if (!string.IsNullOrEmpty(user.Profile.ReferralCode) && user.Profile.ReferredByUserId != null)
                {
                    walletVM.RefId = user.Profile.ReferredByUserId;
                    walletVM.ReferalUserName = user.Profile.ReferredByUser.Profile.Mobile;
                    walletVM.ReferredByUserType = WalletType.ENDUSER; //TODO based on referral user type

                    string ambRefStartsWith = _apiSettings.Value.AmbassadorReferralCodeStartsWith.ToLower();
                    if (user.Profile.ReferredByUser.Profile.MyReferralCode.ToLower().StartsWith(ambRefStartsWith) || user.Profile.ReferredByUser.Type == UserType.DISTRIBUTOR) // is ambassador referral or distributor
                    {
                        walletVM.ReferralCouponType = CouponType.AMBASSADOR_REFERAL;

                        /*
                        ambassadorCouponList = await _rulesService.GetAmbassadorCoupon();
                        if (ambassadorCouponList != null && ambassadorCouponList.Count > 0)
                        {
                            var ambRefereeCoupon = ambassadorCouponList.Where(p => p.CouponType == CouponType.AMBASSADOR_REFEREE).FirstOrDefault();
                            if (ambRefereeCoupon != null && ambRefereeCoupon.Amount > 0)
                            {
                                walletVM.CouponCode = ambRefereeCoupon.CouponCode;
                                //walletVM.CouponType = CouponType.AMBASSADOR_REFEREE;
                                walletVM.CouponAmount = ambRefereeCoupon.Amount;
                            }

                            var ambReferalCoupon = ambassadorCouponList.Where(p => p.CouponType == CouponType.AMBASSADOR_REFERAL).FirstOrDefault();
                            if (ambReferalCoupon != null && ambReferalCoupon.Amount > 0)
                            {
                                walletVM.RefId = user.Profile.ReferredByUserId;
                                walletVM.ReferalAmount = ambReferalCoupon.Amount;
                                walletVM.ReferralCode = ambReferalCoupon.CouponCode;
                                walletVM.ReferralCouponType = ambReferalCoupon.CouponType;
                            }
                        }
                        */
                    }
                    else
                    {
                        walletVM.ReferralCouponType = CouponType.REFERAL_BONUS;

                        /*
                        referralCouponList = await _rulesService.GetReferralCoupon();
                        if (referralCouponList != null && referralCouponList.Count > 0)
                        {
                            var refereeCoupon = referralCouponList.Where(p => p.CouponType == CouponType.REFEREE_BONUS).FirstOrDefault();
                            if (refereeCoupon != null && refereeCoupon.Amount > 0)
                            {
                                walletVM.CouponCode = refereeCoupon.CouponCode;
                                walletVM.CouponType = CouponType.REFEREE_BONUS;
                                walletVM.CouponAmount = refereeCoupon.Amount;
                            }

                            var referalCoupon = referralCouponList.Where(p => p.CouponType == CouponType.REFERAL_BONUS).FirstOrDefault();
                            if (referalCoupon != null && referalCoupon.Amount > 0)
                            {
                                walletVM.RefId = user.Profile.ReferredByUserId;
                                walletVM.ReferalAmount = referalCoupon.Amount;
                                walletVM.ReferralCode = referalCoupon.CouponCode;
                                walletVM.ReferralCouponType = referalCoupon.CouponType;
                            }
                        }
                        */
                    }
                }
                
                bool createWalletResponse = await _walletService.CreateWallet(walletVM);
                return createWalletResponse;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("WalletService.CreateCustomerWallet exception {@exception} for wallet with user {userId}", ex, userId);
            }
            return false;
        }



        public async Task<CommandResult> CreateMissingWallets(UserType userType)
        {
            List<string> validationErrors = new List<string>();
            try
            {
                int count = 0;
                var users = _db.Users.Where(p => p.Type == userType).ToList();
                foreach (var user in users)
                {
                    //break;
                    var walletResponse = await CreateWallet(user.Id);
                    if (walletResponse)
                    {
                        count += 1;
                    }
                }
                return new CommandResult(System.Net.HttpStatusCode.OK, "Added " + count + " wallets");
            }
            catch (Exception ex)
            {
                validationErrors.Add("Sorry, some error has occured. " + ex.Message);
                return CommandResult.FromValidationErrors(validationErrors.AsEnumerable());
            }
        }

        public async Task<bool> CreateOrderTransaction(int orderId, int userId, int tenantId, int branchId, string userMobile, int offerCouponId, string offerCouponName, decimal offerAmount, CouponType offerCouponType )
        {
            try
            {
                if (offerAmount <= 0)
                {
                    return false;
                }

                WalletTransactionCouponVM walletTrans = new WalletTransactionCouponVM()
                {
                    WalletCouponId = offerCouponId,
                    CouponName = offerCouponName,
                    CouponType = offerCouponType,
                    Amount = offerAmount,
                    CustomerUniqueId = userId,
                    WalletUserName = userMobile,
                    OrderId = orderId,
                    //OrderType = order.Type,
                    TenantUniqueId = tenantId,
                    TenantUID = _apiSettings.Value.TenantUidDefault,
                    BranchId = branchId,
                    Details = ""
                };

                bool createOrderResponse = await _walletService.OrderCreationTransaction(walletTrans);
                return createOrderResponse;
            }
            catch (Exception)
            {

            }
            return false;
        }

        public async Task<bool> DeliverOrderTransaction(int orderId, int userId, int tenantId, int? referredUserId, int distributorId, string offerCouponName, decimal offerAmount)
        {
            try
            {
                bool deliverOrderResponse = false;

                if (referredUserId == null)
                {
                    var user = _db.Users
                        .Include(p => p.Profile)
                        .Where(p => p.Id == userId)
                        .FirstOrDefault();
                    if (user != null)
                    {
                        referredUserId = user.Profile.ReferredByUserId;
                    }
                }

                    WalletOrderCouponCompletionVM walletTrans = new WalletOrderCouponCompletionVM()
                    {
                        UniqueId = userId,
                        ReferalUniqueId = referredUserId,
                        CouponName = offerCouponName,
                        Amount = offerAmount,
                        OrderId = orderId,
                        //OrderType = order.Type,
                        TenantUniqueId = tenantId,
                        DistributorUniqueId = distributorId,
                        TenantUId = _apiSettings.Value.TenantUidDefault,
                        //BranchId = (int)order.BranchId,
                        Details = ""
                    };

                    deliverOrderResponse = await _walletService.OrderCompletionTransaction(walletTrans);

                return deliverOrderResponse;
            }
            catch (Exception)
            {

            }
            return false;
        }

        public async Task<bool> CancelOrderTransaction(int orderId, int userId, int tenantId, int branchId, int offerCouponId, string offerCouponName, decimal offerAmount, CouponType offerCouponType)
        {
            try
            {
                WalletTransactionCouponVM walletTrans = new WalletTransactionCouponVM()
                {
                    WalletCouponId = offerCouponId,
                    CouponName = offerCouponName,
                    CouponType = offerCouponType,
                    Amount = offerAmount,
                    CustomerUniqueId = userId,
                    OrderId = orderId,
                    //OrderType = order.Type,
                    TenantUniqueId = tenantId,
                    TenantUID = _apiSettings.Value.TenantUidDefault,
                    BranchId = branchId,
                    Details = ""
                };

                bool cancelOrderResponse = await _walletService.OrderCancelTransaction(walletTrans);
                return cancelOrderResponse;
            }
            catch (Exception)
            {

            }
            return false;
        }

    }
}
