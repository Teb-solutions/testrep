
using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;

namespace Profiles.API.ViewModels.Wallet
{
    public class WalletTransactionCouponVM
    {
        public int TenantUniqueId { get; set; }
        // public long DistributorUniqueId { get; set; }
        public int CustomerUniqueId { get; set; }
        public decimal Amount { get; set; }
        public string Details { get; set; }
        //public OrderType OrderType { get; set; }
        public int OrderId { get; set; }
        public string CouponName { get; set; }
        public CouponType CouponType { get; set; }
        public long WalletCouponId { get; set; }
        public WalletType? CustomerWalletType { get; set; }
        public string WalletUserName { get; set; }
        public string TenantUID { get; set; }
        public int BranchId { get; set; }
    }

    public class WalletOrderCouponCompletionVM
    {
        public long TenantUniqueId { get; set; }
        public long DistributorUniqueId { get; set; }
        public decimal Amount { get; set; }
        public string Details { get; set; }
        //public OrderType OrderType { get; set; }
        public int OrderId { get; set; }
        public string CouponName { get; set; }
        public long? ReferalUniqueId { get; set; }
        public long UniqueId { get; set; }
        public string TenantUId { get; set; }
        //public int BranchId { get; set; }
    }
}
