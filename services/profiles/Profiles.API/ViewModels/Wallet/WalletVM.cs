using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{

    public class WalletVM
    {
        public Decimal Amount { get; set; }

        public string WalletName { get; set; }

        public string WalletPin { get; set; }

        // from profile services.
        public int UniqueId { get; set; }

        public string UserName { get; set; } // phone number of the user

        public WalletType WalletType { get; set; }

        public int TenantId { get; set; }
        public string TenantUId { get; set; }

        public int WalletHierarchyUniqueId { get; set; }

        public string MiscInfo { get; set; } // set some generic info

        public CouponType? CouponType { get; set; }
        public string CouponCode { get; set; }

        public decimal? CouponAmount { get; set; }

        //public bool? CouponActive { get; set; }

        public int? RefId { get; set; }// is referal Id
        public string ReferalUserName { get; set; }
        public string ReferralCode { get; set; }
        public decimal? ReferalAmount { get; set; }
        public CouponType? ReferralCouponType { get; set; }
        public int? UsageCount { get; set; }
        public WalletType? ReferredByUserType { get; set; }
    }

    public enum WalletType
    {
        NONE = 0,
        ENDUSER, // Customer , direct payemtn from tenant .
        BANK_FI,
        SVCVENDOR, // account payable, keeps amount we pay to service Vendors  // can be debited/credited only by tenant Admin directly
        TENANT_RECV, // account/wallet  we keep to get money( money earned) // can be debited only by TenantAdmin
        TENANT_EXPENSE,
        TENANT_WALLET, // tenant keep it's money
        SU_WALLET,  // superuser wallet  for sending  money to  SmartPundits, can be debited only by superAdmin  directly
        RELAY_POINT, // relay point user wallet
        DISTRIBUTOR, // distributor wallet // direct payment from tenant 
        DRIVER,
        COMPANY_WALLET,
        DRIVER_AMBASSADOR,
        ALDS,
        TENANT_ALDS,
        ENDUSER_ALDS,
        TENANT_LUB,
        ENDUSER_LUB,
        TENANT_CARSERVICE,
        CARSERVICE,
        ENDUSER_CARSERVICE,
        EASYGAS_DEALER,
        IOCL_TENANT,
        IOCL_ALDS,
        IOCL_ALDSENDUSER,
        IOCL_MARSHALL,
    }

    public class ReferralCouponActivateVM
    {
        public int ReferalId { get; set; }
        public int RefereeUniqueId { get; set; }
    }

    public class ValidateCouponVM
    {
        public int UserId { get; set; }
        public int CouponId { get; set; }
        public string CouponName { get; set; }
        public CouponType CouponType { get; set; }
        public WalletType WalletType { get; set; }
        public decimal MaxAmount { get; set; }
        public int? UsageTimes { get; set; }
        public bool FirstOrder { get; set; }
    }

    public class CouponValidateResult
    {
        public long CouponId { get; set; }
        public string CouponCode { get; set; }
        //public CouponType CouponType { get; set; }
        public bool Valid { get; set; }
        public decimal MaxAmount { get; set; }
    }
}
