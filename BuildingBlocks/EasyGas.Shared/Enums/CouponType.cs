using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EasyGas.Shared.Enums
{
    public enum CouponType
    {
        [Display(Name = "Special Coupon")]
        COUPON = 1,
        [Display(Name = "Ambassador Referral")]
        AMBASSADOR_REFERAL,
        [Display(Name = "Ambassador Referree")]
        AMBASSADOR_REFEREE,
        [Display(Name = "Referral Bonus")]
        REFERAL_BONUS,
        [Display(Name = "Referree Bonus")]
        REFEREE_BONUS
    }
}
