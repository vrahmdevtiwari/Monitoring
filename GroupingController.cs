using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Monitoring.APIResponses;
using Monitoring.CommonFunction;
using Monitoring.Models.DTO;
using Monitoring.ViewModel;

namespace Monitoring.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroupingController : Controller
    {
        private readonly ILogger<GroupingController> _logger;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();
        private readonly IConfiguration _configuration;
        private EndagentAPI eaAPI;

        private readonly string baseUrl;
        private readonly string getDevicesUrl;
        private readonly string eaApiKeyHeader;
        private readonly string createGroup;
        private readonly string updateGroup;
        private readonly string updateEPGScheduledTime;
        private readonly string deletedevicefromGroup;
        private readonly string getDevicesWithoutGroup;
        private readonly string getGroupNames;
        private readonly string getDeviceByGroupdId;

        public GroupingController(ILogger<GroupingController> logger, IConfiguration configuration, ITokenExtraction tokenExtraction, EndagentAPI _eaAPI)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenExtraction = tokenExtraction;
            _tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();
            eaAPI = _eaAPI;

            baseUrl = _configuration.GetValue<string>("ApiEndPoints:baseUrl");
            getDevicesUrl = _configuration.GetValue<string>("ApiEndPoints:getDevicesUrl");
            eaApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:eaApiKeyHeader");
            createGroup = configuration.GetValue<string>("ApiEndPoints:CreateEndpointGroup");
            updateGroup = configuration.GetValue<string>("ApiEndPoints:UpdateEndpointGroup");
            updateEPGScheduledTime = configuration.GetValue<string>("ApiEndPoints:UpdateEndPointGroupScheduledTime");
            deletedevicefromGroup = configuration.GetValue<string>("ApiEndPoints:DeleteDevicesByGroupId");
            getDevicesWithoutGroup = configuration.GetValue<string>("ApiEndPoints:GetDevicesWithoutEndpointGroup");
            getGroupNames = configuration.GetValue<string>("ApiEndPoints:GetGroupName");
            getDeviceByGroupdId = configuration.GetValue<string>("ApiEndPoints:GetDeviceByGroupId");
        }


        public IActionResult Index()
        {
            ViewBag.FirstName = _tokenProperty.FirstName;
            ViewBag.LastName = _tokenProperty.LastName;
            ViewBag.Email = _tokenProperty.Email;
            ViewBag.Username = _tokenProperty.Name;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDevices()
        {
            string role = _tokenProperty.Role;

            // Now 'values' should contain ["admin", "5"]

            if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                return RedirectToAction("AccessDenied", "Account");

            List<DeviceNameForEndpointGroupVM> eaDevices = await eaAPI.GetDevicesByWithoutEndpointGroup(_tokenProperty.OrgId, baseUrl, getDevicesWithoutGroup, _tokenProperty.Token);

            if (eaDevices == null || eaDevices.Count() == 0)
            {
                _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
                return Json(eaDevices);
            }

            return Json(eaDevices);

        }
        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            string role = _tokenProperty.Role;

            if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                return RedirectToAction("AccessDenied", "Account");

            List<EndpointGroupVM> eaDevices = await eaAPI.GetEndpointGroupNames(_tokenProperty.OrgId, baseUrl, getGroupNames, _tokenProperty.Token);

            if (eaDevices == null || eaDevices.Count() == 0)
            {
                _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
                return Json(eaDevices);
            }

            return Json(eaDevices);

        }
        [HttpGet]
        public async Task<IActionResult> GetDeviceByGroupId(string groupId)
        {
            string role = _tokenProperty.Role;

            if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                return RedirectToAction("AccessDenied", "Account");

            List<DeviceNameForEndpointGroupVM> eaDevices = await eaAPI.GetDevicesByGroupId(_tokenProperty.OrgId, groupId, baseUrl, getDeviceByGroupdId, _tokenProperty.Token);

            if (eaDevices == null || eaDevices.Count() == 0)
            {
                _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
                return Json(eaDevices);
            }

            return Json(eaDevices);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnmappedDevices(string keyword)
        {
            string orgId = _tokenProperty.OrgId;
            string? token = _tokenProperty.Token;
            var devices = await eaAPI.GetDevicesByWithoutEndpointGroup(_tokenProperty.OrgId, baseUrl, getDevicesWithoutGroup, _tokenProperty.Token);
            if(devices==null || devices.Count() == 0)
                { return Json(null); }

            var filtered = devices
                .Where(d => !string.IsNullOrEmpty(d.SystemName) &&
                             d.SystemName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(d => new { id = d.ID, systemName = d.SystemName, publicIP = d.PublicIP })
                .ToList();
            return Json(filtered);
        }

        [HttpPost]
        public async Task<IActionResult> AssignGroup([FromBody] AssignGroupRequestVM request)
        {
            request.OrgId= _tokenProperty.OrgId;
            // request.DeviceIds contains the selected IDs
            string url = $"{baseUrl}{createGroup}";
            var result = await eaAPI.CreateGroup(url, _tokenProperty.Token, request);
            if(result.Status)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAssignGroup([FromBody] AssignGroupByGroupIdVM request)
        {
            request.OrgId = _tokenProperty.OrgId;
            // request.DeviceIds contains the selected IDs
            string url = $"{baseUrl}{updateGroup}";
            var result = await eaAPI.UpdateGroup(url, _tokenProperty.Token, request);
            if (result.Status)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDeviceFromAssignGroup(string groupId,string deviceId)
        {
            var _deleteModel = new DeleteDeviceEndPointGroupVM()
            {
                OrgId = _tokenProperty.OrgId,
                DeviceId = deviceId,
                GroupId = groupId
            };
            var result = await eaAPI.DeleteDeviceFromEndPointGroup(_deleteModel, baseUrl,deletedevicefromGroup,_tokenProperty.Token);
            if (result.Status)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSchedule([FromBody] SaveScheduleRequestVM model)
        {
            var _groupingPatch = new GroupPatchScheduleTimeVM()
            {
                OrgId = _tokenProperty.OrgId,
                GroupId = model.GroupId,
                IsEnabled = model.IsEnabled,
                ScheduledTime = !string.IsNullOrEmpty(model.ScheduledTime)
                                ? DateTime.Parse(model.ScheduledTime, null, System.Globalization.DateTimeStyles.RoundtripKind)
                                : (DateTime?)null,
                GroupPatchScheduledTimeId = model.GpshistoryId
            };

            var result = await eaAPI.SaveEndPointGroupingScheduledTime(_groupingPatch, baseUrl, updateEPGScheduledTime, _tokenProperty.Token);
            if (result.Status) return Ok(result);

            return BadRequest(result);
        }
    }    
}
