using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitoring.APIResponses;
using Monitoring.CommonFunction;
using Monitoring.Models.DTO;
using Monitoring.ViewModel;
using Newtonsoft.Json;
using NuGet.Common;
using System.Security.Cryptography;

namespace Monitoring.Controllers.Devices
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]

    public class TagsController : Controller
    {

        private readonly ILogger<TagsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string ITAMApiKeyHeader;
        private readonly string ITAMApiKey;
        private readonly string eaApiKeyHeader;
        private readonly string eaApiKey;
        private readonly string baseUrl;
        private readonly string getEaBLSwUrl;
        private readonly string ITAMbaseUrl;
        private readonly string getTagsUrl;
        private readonly string updateTagUrl;
        private EndagentAPI eaAPI;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();

        public TagsController(ILogger<TagsController> logger, IConfiguration configuration, EndagentAPI _eaAPI, ITokenExtraction tokenExtraction)
        {
            _logger = logger;
            _configuration = configuration;
            ITAMApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:ITAMApiKeyHeader");
            ITAMApiKey = configuration.GetValue<string>("ApiEndPoints:ITAMApiKey");
            eaApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:eaApiKeyHeader");
            eaApiKey = configuration.GetValue<string>("ApiEndPoints:eaApiKey");
            baseUrl = configuration.GetValue<string>("ApiEndPoints:baseUrl");
            getEaBLSwUrl = configuration.GetValue<string>("ApiEndPoints:getEaBLSwUrl");
            ITAMbaseUrl = _configuration.GetValue<string>("ApiEndPoints:ITAMbaseUrl");
            getTagsUrl = _configuration.GetValue<string>("ApiEndPoints:getTagsUrl");
            updateTagUrl = _configuration.GetValue<string>("ApiEndPoints:updateTagUrl");
            eaAPI = _eaAPI;
            _tokenExtraction = tokenExtraction;
            _tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                string role = HttpContext.User.FindFirst("role")?.Value;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
                string? token = await HttpContext.GetTokenAsync("access_token");
                List<TagsDTO> tagList = await eaAPI.GetTags(orgId, ITAMbaseUrl, getTagsUrl, ITAMApiKeyHeader, ITAMApiKey);
                if (tagList == null)
                {
                    return NotFound(new { message = "Tags API error." });
                }
                return View(tagList);
            }
            catch (Exception ex)
            {
                string user = HttpContext.User.FindFirst("name")?.Value;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        public async Task<JsonResult> UpdateTag(int tagId, bool status)
        {
            try
            {
                
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
                

                string result = await eaAPI.UpdateTag(orgId, ITAMbaseUrl, updateTagUrl, tagId, status, ITAMApiKeyHeader, ITAMApiKey);
                if (result == "updated")

                {
                    return new JsonResult(new { result });
                }
                return null;
            }
            catch (Exception ex)
            {
                string user = HttpContext.User.FindFirst("name")?.Value;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                throw;
            }
        }
        public async Task<JsonResult> GetBLsoftwares()
        {
            try
            {
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
                string user = HttpContext.User.FindFirst("name")?.Value;
                string? token = await HttpContext.GetTokenAsync("access_token");
                List<BLsoftwareListDTO> BLsoftwares = await eaAPI.GetBLsoftwares(orgId, user, baseUrl, getEaBLSwUrl, eaApiKeyHeader, token);
                if (BLsoftwares != null)
                {
                    return new JsonResult(new { BLsoftwares });

                }
                return new JsonResult("error");

            }
            catch (Exception ex)
            {
                string user = HttpContext.User.FindFirst("name")?.Value;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                throw;
                // Redirect to the error handling route with a status code of 500
                // return new JsonResult(new { redirectTo = Url.Action("Index", "Error", new { statusCode = 500 }) });
            }
        }
    }

    
}
