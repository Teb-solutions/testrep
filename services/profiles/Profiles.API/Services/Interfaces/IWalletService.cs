using EasyGas.Services.Profiles.Models;
using Profiles.API.ViewModels.Wallet;
using System.Threading.Tasks;

namespace Profiles.API.Services
{
    public interface IWalletService
    {
        Task<bool> CreateWallet(WalletVM wallet);
        Task<UserCouponSummary> GetCouponSummary(int userId);
        Task<CouponModel> GetActiveReferralCoupon();

        Task<bool> OrderCreationTransaction(WalletTransactionCouponVM walletTrans);
        Task<bool> OrderCompletionTransaction(WalletOrderCouponCompletionVM walletTrans);
        Task<bool> OrderCancelTransaction(WalletTransactionCouponVM walletTrans);
    }
}
