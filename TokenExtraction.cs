using Microsoft.AspNetCore.Authentication;
using Monitoring.ViewModel;
using Identity.Common.Implementation.Interface;
using Microsoft.EntityFrameworkCore;
using Monitoring.Data;

namespace Monitoring.CommonFunction
{
    public class TokenExtraction : ITokenExtraction
    {
        private readonly ILogger<TokenExtraction> _logger;
        private readonly ITokenServices _tokenServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SSODBContext _dbContext;

        public TokenExtraction(ILogger<TokenExtraction> logger, ITokenServices tokenServices, IHttpContextAccessor httpContextAccessor, SSODBContext dbContext)
        {
            _logger = logger;
            _tokenServices = tokenServices;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<TokenViewModel> ExtractToken()
        {
            try
            {
                string? accessToken = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
                var _tokendata = await _tokenServices.ReadTokenAsync(accessToken);

                // Developer: Srikanth : 28-03-2025 - to fetch data from ssologindb Aspnetuser
                var _ssoUser = _dbContext.AspNetUsers.FirstOrDefault(x => x.UserName == _tokendata.Name || x.Email == _tokendata.Name);

                TokenViewModel _token = new TokenViewModel
                {
                    ClientId = _tokendata.ClientId,
                    FirstName = _ssoUser.FirstName,
                    LastName = _ssoUser.LastName,
                    Email = _ssoUser.Email,
                    Name = _ssoUser.UserName,
                    OrgId = _ssoUser.Organization.ToString(),
                    Jtid = _tokendata.Jtid,                    
                    Role = _tokendata.Role,
                    Sid = _tokendata.Sid,
                    Sub = _tokendata.Sub
                };
                _token.Token = accessToken ?? string.Empty;
                return _token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error occured in token extraction process");
            }
            return new TokenViewModel();

        }
    }
}
