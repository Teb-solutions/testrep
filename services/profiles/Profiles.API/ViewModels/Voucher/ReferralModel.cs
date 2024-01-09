using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Models
{
    public class ReferralModel
    {
        public int CouponId { get; set; }
        public decimal ReferrerAmount { get; set; }
        public decimal RefereeAmount { get; set; }
        public string CouponCode { get; set; }
        public int? MaxTries { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RulesCouponModel
    {
        public int CouponId { get; set; }
        public string CouponName { get; set; }
        public string CouponCode { get; set; }
        public CouponType CouponType { get; set; }
        public Decimal Amount { get; set; }
        public RuleType RuleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? MaxTries { get; set; }
        public bool? IsActive { get; set; }
    }

    public enum RuleType
    {
        COUPON = 1,
        AMBASSADOR,
        REFERAL
    }

    public class CouponModel
    {
        public int CouponId { get; set; }
        public string CouponName { get; set; }
        public CouponType CouponType { get; set; }
        public string CouponCode { get; set; }
        public decimal ReferrerAmount { get; set; }
        public decimal ReferreeAmount { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsActive { get; set; }
    }

}
