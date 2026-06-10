using Identity.Common.Implementation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitoring.APIResponses;
using Monitoring.CommonFunction;
using Monitoring.ViewModel;


namespace Monitoring.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WhitelistSoftwareController : Controller
    {
        private readonly ILogger<WhitelistSoftwareController> _logger;
        private readonly IConfiguration _configuration;
        private EndagentAPI eaAPI;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();
        private readonly string _baseUrl;
        private readonly string _addDesktopPolicy;
        private readonly string _getDesktopPolicy;
        public WhitelistSoftwareController(ILogger<WhitelistSoftwareController> logger, IConfiguration configuration, EndagentAPI _eaAPI, ITokenExtraction tokenExtraction)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenExtraction = tokenExtraction;
            _tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();
            eaAPI = _eaAPI;
            _baseUrl = configuration.GetValue<string>("ApiEndPoints:baseUrl");
            _addDesktopPolicy = configuration.GetValue<string>("ApiEndPoints:CreatePolicy");
            _getDesktopPolicy = configuration.GetValue<string>("ApiEndPoints:BlockAndUbBlock");
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.FirstName = _tokenProperty.FirstName;
            ViewBag.LastName = _tokenProperty.LastName;
            ViewBag.Email = _tokenProperty.Email;
            ViewBag.Username = _tokenProperty.Name;

            string url = $"{_baseUrl}{_getDesktopPolicy}";
            var _desktopPolicies = await eaAPI.GetDesktopPolicies(url, _tokenProperty.Token, _tokenProperty.OrgId);

            return View(_desktopPolicies);
        }

        
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> Create_Post(DesktopPoliciesDTO modal)
        {
            modal.OrgId = _tokenProperty.OrgId;

            if (ModelState.IsValid)
            {
                string url = $"{_baseUrl}{_addDesktopPolicy}";
                var result = await eaAPI.AddDesktopPolicy(url, _tokenProperty.Token, modal);

                if (result.Status)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Desktop policy added successfully"
                    });
                }
            }

            return PartialView("Create", modal);
        }
    }
}
