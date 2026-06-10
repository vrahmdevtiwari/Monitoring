using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Cli.Utils.CommandParsing;
using Monitoring.APIResponses;
using Monitoring.CommonFunction;
using Monitoring.Controllers.Devices;
using Monitoring.Models;
using Monitoring.Models.DTO;
using Monitoring.ViewModel;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;

namespace Monitoring.Controllers
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class CISRepoController : Controller
    {
        private readonly ILogger<CISRepoController> _logger;
        private readonly IConfiguration _configuration;
        private EndagentAPI eaAPI;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();
        private readonly string GetProductFamilyCIS;
        private readonly string GetPatchesByFamily;
        private readonly string PostCISOptions;
        private readonly string GetCISOption;
        private readonly string SyncbyorgID;
        private readonly string GetOSPatchAndSoftware;
        public CISRepoController(ILogger<CISRepoController> logger, IConfiguration configuration, EndagentAPI _eaAPI, ITokenExtraction tokenExtraction)
        {
            _logger = logger;
            _configuration = configuration;            
            _tokenExtraction = tokenExtraction;
            _tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();                      
            eaAPI = _eaAPI;
            GetProductFamilyCIS = configuration.GetValue<string>("ApiEndPoints:GetProductFamilyCIS");
            GetPatchesByFamily = configuration.GetValue<string>("ApiEndPoints:GetPatchesByFamily");
            PostCISOptions = configuration.GetValue<string>("ApiEndPoints:AddCISOptions");
            GetCISOption = configuration.GetValue<string>("ApiEndPoints:GetCISOption");
            GetOSPatchAndSoftware = configuration.GetValue<string>("ApiEndPoints:GetOSPatchAndSoftware");
            SyncbyorgID = configuration.GetValue<string>("ApiEndPoints:syncbyorgid");
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.FirstName = _tokenProperty.FirstName;
            ViewBag.LastName = _tokenProperty.LastName;
            ViewBag.Email = _tokenProperty.Email;
            ViewBag.Username = _tokenProperty.Name;

            string? token = await HttpContext.GetTokenAsync("access_token");
            var families = await eaAPI.GetProductFamilies(GetProductFamilyCIS, token);
            var _pflist = await GetCISFamilies();
            
            var _result = families.Select(x => new ProductFamiliesVM()
            {
                Name = x.Name,
                UniqueCode = x.UniqueCode,
                IsSelected = (_pflist != null && _pflist.Count() > 0) ? _pflist.Any(p => p.Trim().Equals(x.UniqueCode.ToString().Trim(), StringComparison.OrdinalIgnoreCase)) : false
            }).ToList();

            return View(_result);
        }        
        [HttpPost]
        public async Task<IActionResult> SaveUserFamilies([FromBody] ProductFamilySelectedVM model)
        {
            string? token = await HttpContext.GetTokenAsync("access_token");
            if (model?.Families == null || !model.Families.Any())
                return BadRequest("No options selected.");

            string families = string.Join(",", model.Families);

            // 1️⃣ Save user selection
            APIResponse response = await eaAPI.SaveAsync(PostCISOptions,_tokenProperty.Token,_tokenProperty.OrgId,_tokenProperty.Name,families,model.Type);

            if (!response.Status)
            {
                return Json(new
                {
                    success = false,
                    message = response.Message
                });
            }
            //  Call Repo API
            var patches = await eaAPI.GetPatchesbyfamily(families, GetPatchesByFamily, token);

            //  Return everything to UI
            return Json(new
            {
                success = true,
                message = response.Message,
                osPatches = patches?.OSPatches,
                softwarePatches = patches?.SoftwarePatches
            });
        }

        private async Task<List<string>> GetCISFamilies()
        {
            string url = $"{GetCISOption}/{_tokenProperty.OrgId}";
            var response = await eaAPI.GetAsync<CISDTO>(url, _tokenProperty.Token);
            if (response != null && response.OrgId != null && (response.OsPatchesSelected != null || response.SoftwarePatchesSelected != null))
                return response.OsPatchesSelected.Split(',').ToList();

            return null;
        }
        [HttpGet]
        public async Task<IActionResult> GetOSPatchAndSoftwares()
        {
            List<string> selectedFamilies = await GetCISFamilies();
            if (selectedFamilies == null || !selectedFamilies.Any())
                return BadRequest("No families selected.");

            var response = await eaAPI.FetchOSPatchesAndSoftwares(GetOSPatchAndSoftware, selectedFamilies, _tokenProperty.Token, _tokenProperty.OrgId);
            //var livePatches = await eaAPI.GetLivePatchesAsync(selectedFamilies);
            return Ok(new
            {
                success = true,
                message = "Sync completed successfully",
                osPatches = response.OSPatches,
                softwarePatches = response.SoftwarePatches
            });

        }

        //[HttpGet]
        //public async Task<IActionResult> SyncNow()
        //{
        //    //Console.WriteLine("\n--------------------------------------------------------------------------\n"+DateTime.Now+ "\n--------------------------------------------------------------------------\n");
        //    List<string> selectedFamilies = await GetCISFamilies();

        //    if (selectedFamilies == null || !selectedFamilies.Any())
        //        return BadRequest("No families selected.");

        //    var response = await eaAPI.TriggerSyncWithPatchesAsync(SyncbyorgID, selectedFamilies, _tokenProperty.Token, _tokenProperty.OrgId);
        //    //var livePatches = await eaAPI.GetLivePatchesAsync(selectedFamilies);
        //    return Ok(new { success = true, message = "Sync completed successfully",
        //        osPatches = response.OSPatches,
        //        softwarePatches = response.SoftwarePatches
        //    });
        //}

        [HttpGet]
        public async Task<IActionResult> SyncNow()
        {
            try
            {
                List<string> selectedFamilies = await GetCISFamilies();

                if (selectedFamilies == null || !selectedFamilies.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        alreadySynced = false,
                        noFamilies = true,
                        message = "No families selected. Please save first.",
                        osPatches = new List<object>(),
                        softwarePatches = new List<object>()
                    });
                }

                var response = await eaAPI.TriggerSyncWithPatchesAsync(
                    SyncbyorgID, selectedFamilies, _tokenProperty.Token, _tokenProperty.OrgId);

                bool hasPatches = (response?.OSPatches?.Any() == true) ||
                                  (response?.SoftwarePatches?.Any() == true);

                return Ok(new
                {
                    success = true,
                    alreadySynced = !hasPatches,
                    noFamilies = false,
                    message = hasPatches ? "Sync completed successfully" : "No new patch found",
                    osPatches = response?.OSPatches,
                    softwarePatches = response?.SoftwarePatches
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SyncNow");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to sync. Please try again."
                });
            }
        }



    }
}
