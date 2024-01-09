using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace EasyGas.Services.Profiles.Commands
{
    public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand>
    {
        private readonly ProfilesDbContext _db;
        private ILogger _logger;

        public UpdateProfileCommandHandler(ProfilesDbContext db, ILogger<UpdateProfileCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }
        public CommandHandlerResult Handle(UpdateProfileCommand command)
        {
            var profile = command.Profile;
            var user = command.User;
            var patching = command.IsPatch;
            var existingProfile = _db.Profiles.SingleOrDefault(p => p.UserId == profile.UserId);
            var existingUser = _db.Users.SingleOrDefault(p => p.Id == profile.UserId);
            if (existingProfile == null || existingUser == null)
            {
                return CommandHandlerResult.Error("No user found");
            }
            
            if (!patching)
            {
                if (!string.IsNullOrEmpty(profile.Code))
                {
                    var existingCode = _db.Profiles.Any(x => x.Code == profile.Code && x.UserId != profile.UserId);
                    if (existingCode)
                    {
                        return CommandHandlerResult.Error($"Code ({profile.Code}) already exists.");
                    }
                    existingProfile.Code = profile.Code;
                }

                existingProfile.BirthDate = profile.BirthDate;
                existingProfile.FirstName = profile.FirstName;
                existingProfile.Gender = profile.Gender;
                existingProfile.LastName = profile.LastName;
                existingProfile.Email = profile.Email;
                existingProfile.GSTN = profile.GSTN;
                existingProfile.PAN = profile.PAN;
                existingProfile.OffDay = profile.OffDay;

                if (profile.Mobile != null)
                {
                    var duplicateUser = _db.Users.Any(x => x.UserName == profile.Mobile && x.Id != profile.UserId);
                    if (duplicateUser)
                    {
                        return CommandHandlerResult.Error($"Username ({profile.Mobile}) already exists.");
                    }
                    existingProfile.Mobile = profile.Mobile;
                    existingUser.UserName = profile.Mobile;
                }

                _logger.LogInformation("UpdateProfileCommandHandler User Updated | userType: " + user.Type + "username: " + user.UserName);

                return CommandHandlerResult.OkDelayed(this,
                x => new CreateProfileResponse
                {
                    UserId = existingProfile.UserId,
                    ProfileId = existingProfile.Id
                });
            }
            else return PatchProfile(existingProfile, existingUser, profile, user);
        }

        private CommandHandlerResult PatchProfile(UserProfile existingProfile, User existingUser, UserProfile profile, User user)
        {

            if (profile.BirthDate.HasValue)
            {
                existingProfile.BirthDate = profile.BirthDate;
            }
            if (!string.IsNullOrEmpty(profile.FirstName)) {
                existingProfile.FirstName = profile.FirstName;
            }
            if (!string.IsNullOrEmpty(profile.LastName))
            {
                existingProfile.LastName = profile.LastName;
            }
            if (profile.Gender != Gender.NotSpecified)
            {
                existingProfile.Gender = profile.Gender;
            }

            if (!string.IsNullOrEmpty(profile.Email))
            {
                existingProfile.Email = profile.Email;
            }
            if (!string.IsNullOrEmpty(profile.GSTN))
            {
                existingProfile.GSTN = profile.GSTN;
            }
            if (!string.IsNullOrEmpty(profile.PAN))
            {
                existingProfile.PAN = profile.PAN;
            }
            /*
            if (!string.IsNullOrEmpty(profile.DeviceId))
            {
                existingProfile.DeviceId = profile.DeviceId;
            }
            */

            if (!string.IsNullOrEmpty(profile.Code))
            {
                var existingCode = _db.Profiles.Any(x => x.Code == profile.Code && x.UserId != profile.UserId);
                if (existingCode)
                {
                    return CommandHandlerResult.Error($"Code ({profile.Code}) already exists.");
                }
            }

            if (!string.IsNullOrEmpty(profile.ReferralCode) && string.IsNullOrEmpty(existingProfile.ReferralCode))
            {
                int? referredUserId = null;
                var refCode = profile.ReferralCode.ToLower();
                var referredUser = _db.Users.Include(p => p.Profile).Where(x => x.Profile.MyReferralCode == refCode && x.Id != existingProfile.UserId).FirstOrDefault();
                if (referredUser == null)
                {
                    //referral code can be mobile number also
                    referredUser = _db.Users.Where(x => x.UserName == profile.ReferralCode && x.Id != existingProfile.UserId).FirstOrDefault();
                    if (referredUser == null)
                    {
                        return CommandHandlerResult.Error($"Referral Code ({profile.ReferralCode}) does not exist.");
                    }
                    referredUserId = referredUser.Id;
                }
                else
                {
                    referredUserId = referredUser.Id;
                }
                if (referredUser != null)
                {
                    existingProfile.ReferralCode = profile.ReferralCode;
                    existingProfile.ReferredByUserId = referredUserId;

                    //attach distributor
                    if (referredUser.Type == Shared.Enums.UserType.DISTRIBUTOR)
                    {
                        existingUser.BusinessEntityId = referredUser.BusinessEntityId;
                        existingUser.BusinessEntityAttachedAt = DateMgr.GetCurrentIndiaTime();
                        _logger.LogInformation("Customer {userId} registered using business entity {referralCode} of {businessEntityId}", existingProfile.UserId, existingProfile.ReferralCode, existingUser.BusinessEntityId);
                    }
                }
            }

            if (!existingProfile.AgreedTerms && profile.AgreedTerms)
            {
                existingProfile.AgreedTerms = true;
            }

            if (!existingUser.OtpValidated && user.OtpValidated)
            {
                existingUser.OtpValidated = true;
                existingUser.OtpValidatedAt = DateMgr.GetCurrentIndiaTime();
                existingUser.OtpValidatedBy = user.OtpValidatedBy;
            }

            //if (profile.Mobile != null)
            //{
            //    existing.Mobile = profile.Mobile;
            //}

            return CommandHandlerResult.OkDelayed(this,
                x => new CreateProfileResponse
                {
                    UserId = existingProfile.UserId,
                    ProfileId = existingProfile.Id
                });
        }
    }
}
