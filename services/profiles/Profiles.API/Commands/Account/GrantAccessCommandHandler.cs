using Microsoft.EntityFrameworkCore;
using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyGas.Services.Core.Commands;
using EasyGas.Shared.Formatters;
using Microsoft.Extensions.Configuration;
using System.Text;
using EasyGas.Security;
using Profiles.API.Models;
using EasyGas.Shared.Enums;

namespace EasyGas.Services.Profiles.Commands
{
    public class GrantAccessCommandHandler : ICommandHandler<GrantAccessCommand>
    {
        private readonly ProfilesDbContext _db;
        private IJWTUtils _JWTUtils;

        public GrantAccessCommandHandler(ProfilesDbContext db, IConfiguration cfg, IJWTUtils jWTUtils)
        {
            _db = db;
            _JWTUtils = jWTUtils;
        }
        public CommandHandlerResult Handle(GrantAccessCommand command)
        {
            var existing = _db.Users.
                Include(u=> u.Tenant).
                Include(u => u.Profile).
                Include(u => u.Branch).
                Include(u => u.BusinessEntity).
                //Include(u => u.BusinessEntity.Profile).
                SingleOrDefault(p => p.UserName == command.Data.UserName && p.Type == command.Data.UserType);

            if (existing != null)
            {
                bool credentialsValid = false;
                if (command.Data.GrantType == LoginModel.PasswordGrantType)
                {
                    string hashPass = Helper.HashPassword(command.Data.Credentials);
                    if(hashPass == existing.Password)
                    {
                        credentialsValid = true;
                    }
                }
                else if(command.Data.GrantType == LoginModel.OtpGrantType)
                {
                    credentialsValid = true; // as OTP value is already validated
                }

                if(credentialsValid)
                {
                    int vehicleId = 0;
                    string vehicleRegNo = "";
                    string userType = "";
                    
                    if (existing.Type == UserType.CUSTOMER)
                    {
                        userType = "Customer";
                    }
                    else if (existing.Type == UserType.DRIVER)
                    {
                        var vehicle = _db.Vehicles.Where(p => p.DriverId == existing.Id && p.IsActive == true).FirstOrDefault();
                        if (vehicle != null)
                        {
                            vehicleId = vehicle.Id;
                            vehicleRegNo = vehicle.RegNo;
                        }
                        else
                        {
                            //return CommandHandlerResult.Error($"No vehicle assigned");
                            return new CommandHandlerResult(HttpStatusCode.Forbidden,
                                new
                                {
                                    err = new[] {"No vehicle assigned"}
                                });
                        }
                        userType = "Driver";
                    }
                    else if(existing.Type == UserType.ADMIN)
                    {
                        userType = "Admin";
                    }

                    DateTime now = DateMgr.GetCurrentIndiaTime();
                    string accessToken = _JWTUtils.GenerateJwtToken(existing);
                    RefreshToken refreshToken = _JWTUtils.GenerateRefreshToken();
                    refreshToken.UserId = existing.Id;
                    refreshToken.CreatedAt = now;
                    refreshToken.UpdatedAt = now;

                    _db.RefreshTokens.Add(refreshToken);
                    _db.SaveChanges();


                    var result = new GrantAccessResult()
                    {
                        Name = existing.Profile.GetFullName(),
                        AccessToken = _JWTUtils.GenerateJwtToken(existing),

                    };
                    existing.Profile.DeviceId = command.Data.DeviceId;

                    if (existing.Type == UserType.CUSTOMER)
                    {
                        CustomerGrantAccessResult customerResult = new CustomerGrantAccessResult()
                        {
                            AccessToken = result.AccessToken,
                            Name = existing.Profile.FirstName + " " + existing.Profile.LastName,
                            Mobile = existing.Profile.Mobile,
                            CityId = 2
                        };
                        return new CommandHandlerResult(HttpStatusCode.OK, customerResult);
                    }
                    return new CommandHandlerResult(HttpStatusCode.OK, result);
                }
                
            }
            //return CommandHandlerResult.Error($"Invalid credentials");
            return new CommandHandlerResult(HttpStatusCode.Forbidden,
                new
                {
                    err = new[] { "Invalid credentials" }
                });
        }
    }
}
