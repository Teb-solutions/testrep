using EasyGas.Services.Profiles.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Profiles.API.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Profiles.API.Infrastructure
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiSettings _appSettings;
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenMiddleware> _logger;

        public TokenMiddleware(RequestDelegate next, IOptions<ApiSettings> appSettings,
            ITokenService tokenService,
            IIdentityService identityService, ILogger<TokenMiddleware> logger)
        {
            _next = next;
            _appSettings = appSettings.Value;
            _identityService = identityService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("x-bearer-nochange"))
            {
                if (context.Request.Headers.ContainsKey("Authorization"))
                {
                    var currentBearerToken = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (currentBearerToken != null)
                    {
                        var currentToken = currentBearerToken.Split(" ")[1];
                        if (!string.IsNullOrEmpty(currentToken))
                        {
                            var currentJwt = new JwtSecurityTokenHandler().ReadJwtToken(currentToken);
                            var issuer = currentJwt.Claims.Where(p => p.Type == "iss").FirstOrDefault().Value;
                            if (issuer != null)
                            {
                                if (issuer != _appSettings.JwtTokenIssuer)
                                {
                                    // generate custom token with user details
                                    var userDetails = await _tokenService.GetUserDetailsAsync(currentJwt);
                                    if (userDetails != null)
                                    {
                                        // create custom jwt token with user details
                                        var jwtToken = _tokenService.GenerateJwtToken(userDetails);
                                        context.Request.Headers["Authorization"] = $"Bearer {jwtToken}";
                                        _logger.LogInformation($"Profiles.TokenMiddleware Custom jwt token generated {jwtToken}");
                                        //await _next(context);
                                    }
                                    else
                                    {
                                        _logger.LogCritical($"Profiles.TokenMiddleware Not Authorized {currentToken}");
                                        await ReturnErrorResponse(context, "User not found. Please contact admin.");

                                        /*
                                        // if token is b2e, add new user in profile service as approval pending
                                        if (_identityService.IsB2E())
                                        {
                                            var cognitoDataToken = context.Request.Headers["x-amzn-oidc-data"].FirstOrDefault();

                                            if (!string.IsNullOrEmpty(cognitoDataToken))
                                            {
                                                var createUserResponse = await _tokenService.CreateUnApprovedUserInProfilesAsync(cognitoToken, cognitoDataToken);
                                                if (createUserResponse.isSuccess)
                                                {
                                                    _logger.LogInformation($"EasyGasAdminAws.TokenMiddleware New backend user created {createUserResponse.profileResponse.UserId} {cognitoToken}");
                                                    await ReturnErrorResponse(context, "User is pending approval from admin. Please contact admin for approval.");
                                                }
                                                else
                                                {
                                                    _logger.LogCritical($"EasyGasAdminAws.TokenMiddleware New user creation error: {createUserResponse.validationDetails?.Detail} token: {cognitoToken} data token: {cognitoDataToken}");
                                                    await ReturnErrorResponse(context, createUserResponse.validationDetails?.Detail);
                                                }
                                            }
                                        }
                                        */
                                    }
                                }
                            }
                            else
                            {
                                await ReturnErrorResponse(context, "Invalid User.");
                            }
                        }
                    }
                }
            }

            await _next(context);
        }

        private async Task ReturnErrorResponse(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(message);
            await context.Response.StartAsync();
        }
    }
}
