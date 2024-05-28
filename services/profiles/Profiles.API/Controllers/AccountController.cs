using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Commands;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Profiles.API.Infrastructure.Services;
using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Account;
using Profiles.API.ViewModels.Mobile.Login;
using System.Net;
using System.Threading.Tasks;
using EasyGas.Shared.Enums;
using System.Collections.Generic;
using System;
using System.Text;

namespace Profiles.API.Controllers
{
    [Route("profiles/api/v1/[controller]")]
    public class AccountController : BaseApiController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IProfileQueries _profileQueries;
        private readonly OtpMgr _otpMgr;
        private readonly IIdentityService _identityService;

        public AccountController(ILogger<AccountController> logger, IIdentityService identityService, ICommandBus bus, IConfiguration cfg, IProfileQueries profileQueries, OtpMgr otpMgr)
            : base(bus)
        {
            _profileQueries = profileQueries;
            _otpMgr = otpMgr;
            _logger = logger;
            _identityService = identityService;
        }

        [Route("refresh-token")]
        [HttpPost]
        [ProducesResponseType(typeof(GrantAccessResult), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Refresh token {@request}", request);
            return await _identityService.RefreshToken(request.Token, GetIpAddress());
        }

        [Route("refresh-token-ff")]
        [HttpPost]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RefreshTokenFormFile([FromForm] RefreshTokenRequest request)
        {
            _logger.LogInformation("Refresh token from fromfile {@request}", request);
            return await _identityService.RefreshToken(request.Token, GetIpAddress());
        }

        [Route("access-token")]
        [HttpPost]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAccessToken([FromBody] LoginByPasswordModel data)
        {
            var loginmodel = new LoginModel()
            {
                Credentials = data.Password,
                GrantType = LoginModel.PasswordGrantType,
                UserName = data.UserName,
                UserType = UserType.ADMIN
            };
            return await _identityService.Authenticate(loginmodel, "Internal Communication");
        }

        [Route("access-token-basic")]
        [HttpGet]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAccessTokenByBasicAuth([FromHeader] string Authorization)
        {
            if (Authorization != null && Authorization.StartsWith("Basic"))
            {
                string encodedUsernamePassword = Authorization.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                int seperatorIndex = usernamePassword.IndexOf(':');

                var username = usernamePassword.Substring(0, seperatorIndex);
                var password = usernamePassword.Substring(seperatorIndex + 1);

                var loginmodel = new LoginModel()
                {
                    Credentials = password,
                    GrantType = LoginModel.PasswordGrantType,
                    UserName = username,
                    UserType = UserType.ADMIN
                };
                return await _identityService.Authenticate(loginmodel, "Internal Communication");
            }
            else
            {
                //Handle what happens if that isn't the case
                throw new Exception("The authorization header is either empty or isn't Basic.");
            }
        }

        [Authorize]
        [Route("change-password")]
        [HttpPut]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            int userId = _identityService.GetUserIdentity();
            return await _identityService.ChangePassword(model, userId);
        }

        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiValidationErrors), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginByPassword([FromBody] LoginModel data)
        {
            if (!ModelState.IsValid)
            {
                return CommandResult.FromValidationErrors("Username/password is invalid");
            }
            if (data.UserType != UserType.ADMIN)
            {
                return CommandResult.FromValidationErrors("User should be backend admin");
            }
            data.GrantType = LoginModel.PasswordGrantType;
            return await _identityService.Authenticate(data, GetIpAddress());
        }

        [Authorize(Roles = "TENANT_ADMIN")]
        [Route("register")]
        [HttpPost]
        [ProducesResponseType(typeof(GrantAccessResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Register([FromBody] RegisterModel request)
        {
            return await _identityService.Register(request);
        }

        [Authorize(Roles = "TENANT_ADMIN")]
        [HttpGet]
        [Route("users")]
        [ProducesResponseType(typeof(IEnumerable<BackendUserProfileModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBackendUsers([FromQuery] int tenantId, [FromQuery] int? branchId = null)
        {
            var users = await _profileQueries.GetBackendUsers(tenantId, branchId);
            return Ok(users);
        }

        [Authorize]
        [HttpGet]
        [Route("details")]
        [ProducesResponseType(typeof(IEnumerable<BackendUserProfileModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMyAccountDetails()
        {
            var cognitoUsername = _identityService.GetCognitoUsername();
            var userDetails = await _profileQueries.GetUserByCognitoUsername(cognitoUsername);
            return Ok(userDetails);
        }
    }
}
