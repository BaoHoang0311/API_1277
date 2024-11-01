using API127.Models.Dto;
using API127.Repository.IRepository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BasicAuth.API
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        #region Property  
        readonly IUserRepository _userService;
        #endregion

        #region Constructor  
        public BasicAuthenticationHandler(IUserRepository userService,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }
        #endregion

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string username = null;
            var data = new LoginResponseDTO();
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                username = credentials.FirstOrDefault();
                var password = credentials.LastOrDefault();
                data = await _userService.Login(new LoginRequestDTO { UserName = username, Password = password });

                if (data.Success == false )
                    throw new ArgumentException("Invalid credentials");
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
            }


            var claims = new[] {
                new Claim(ClaimTypes.Name, username),
                new Claim("ID", data.User.UserId.ToString()),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

    }
}