using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;
using System;

namespace Profiles.API.ViewModels.PulzConnect
{
    public class ProfileModel
    {
            public PulzProduct Product { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; }
            public UserType Type { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string Email { get; set; }
            public string Mobile { get; set; }
            public DateTime? CreatedAt { get; set; }

            public string PhotoUrl { get; set; }
            public string MyReferralCode { get; set; }

            public bool IsAmbassador { get; set; }

        public static ProfileModel FromProfile(UserProfile profile)
        {
            ProfileModel model = new ProfileModel()
            {
                UserId = profile.UserId,
                CreatedAt = profile.CreatedAt,
                Email = profile.Email,
                Mobile = profile.Mobile,
                Type = profile.User.Type,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                MyReferralCode = profile.MyReferralCode,
                PhotoUrl = profile.PhotoUrl,
                UserName = profile.User.UserName,
                Product = PulzProduct.EasyGas
            };

            return model;
        }
    }
}
