using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitoring.APIResponses;
using Monitoring.Models.DTO;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using NuGet.ContentModel;
using Monitoring.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Monitoring.CommonFunction;
using Monitoring.ViewModel;
using System.Security.Cryptography;

namespace Monitoring.Controllers.Devices
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class WorkstationController : Controller
    {
        private readonly ILogger<WorkstationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string ITAMApiKeyHeader;
        private readonly string ITAMApiKey;
        private readonly string eaApiKeyHeader;
        private readonly string eaApiKey;
        private readonly string baseUrl;
        private readonly string getSysOverviewUrl;
        private readonly string getEaAppsUrl;
        private readonly string getEaUpdatesUrl;
        private readonly string getEaPortsUrl;
        private readonly string ITAMbaseUrl;
        private readonly string getEaDevicesUrl;
        private readonly string getActiveStatusUrl;
        private readonly string getApprovalsUrl;
        private readonly string approveMonitoringUrl;
        private readonly string getServiceDetailsUrl;
        private readonly string getProcessDetailsUrl;
        private readonly string getProcessorDetailsUrl;
        private readonly string getRAMDetailsUrl;
        private readonly string getScheduledTasksUrl;
        private readonly string getStorageVolumesUrl;
        private readonly string getRAIDControllersUrl;
        private readonly string getGraphicCardsUrl;
        private readonly string getNetworkAdaptersUrl;
        private readonly string getPhysicalDrivesUrl;
        private readonly string getOtherSpecificationsUrl;
        private readonly string getAccountsUrl;
        private readonly string getActivePortsUrl;
        private readonly string getActiveNetworkDetailsUrl;
        private readonly string getResourceUtilUrl;
        private readonly string getDiskDetailsUrl;
        private readonly string getDevicesUrl;
        private readonly string approveDeviceUrl;
        private readonly string deleteDeviceUrl;
        private readonly string isAgentInstalledUrl;
        private readonly string getDevicesForUpdateUrl;
        private readonly string getLatestVersionUrl;
        private readonly string addDeviceInPatchQueueUrl;
        private readonly string isInQueueWithDeviceIDUrl;
        private readonly string getCurrentVersionUrl;
        private readonly string getAssetIDsUrl;
        private readonly string checkBIOSUrl;
        private readonly string addWorkstationUrl;
        private readonly string updateWorkstationUrl;
        private readonly string mastersUrl;
        private readonly string getBIOSurl;
        private readonly EndagentAPI eaAPI;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();


        public WorkstationController(ILogger<WorkstationController> logger, IConfiguration configuration, ITokenExtraction tokenExtraction, EndagentAPI _eaAPI)
        {
            //"eaApiKeyHeader": "x-api-key",
            //"eaApiKey": "042E709DBBDD4FE19406298B65EEBF80",

            _logger = logger;
            _configuration = configuration;
            ITAMApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:ITAMApiKeyHeader");
            ITAMApiKey = configuration.GetValue<string>("ApiEndPoints:ITAMApiKey");
            eaApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:eaApiKeyHeader");
            eaApiKey = configuration.GetValue<string>("ApiEndPoints:eaApiKey");
            baseUrl = _configuration.GetValue<string>("ApiEndPoints:baseUrl");
            getSysOverviewUrl = _configuration.GetValue<string>("ApiEndPoints:getSysOverviewUrl");
            getEaAppsUrl = _configuration.GetValue<string>("ApiEndPoints:getEaAppsUrl");
            getEaUpdatesUrl = _configuration.GetValue<string>("ApiEndPoints:getEaUpdatesUrl");
            getEaPortsUrl = _configuration.GetValue<string>("ApiEndPoints:getEaPortsUrl");
            ITAMbaseUrl = _configuration.GetValue<string>("ApiEndPoints:ITAMbaseUrl");
            getEaDevicesUrl = _configuration.GetValue<string>("ApiEndPoints:getEaDevicesUrl");
            getActiveStatusUrl = _configuration.GetValue<string>("ApiEndPoints:getActiveStatusUrl");
            getApprovalsUrl = _configuration.GetValue<string>("ApiEndPoints:getApprovalsUrl");
            getServiceDetailsUrl = _configuration.GetValue<string>("ApiEndPoints:getServiceDetailsUrl");
            getProcessDetailsUrl = _configuration.GetValue<string>("ApiEndPoints:getProcessDetailsUrl");
            getProcessorDetailsUrl = _configuration.GetValue<string>("ApiEndPoints:getProcessorDetailsUrl");
            approveMonitoringUrl = _configuration.GetValue<string>("ApiEndPoints:approveMonitoringUrl");
            getRAMDetailsUrl = _configuration.GetValue<string>("ApiEndPoints:getRAMDetailsUrl");
            getScheduledTasksUrl = _configuration.GetValue<string>("ApiEndPoints:getScheduledTasksUrl");
            getStorageVolumesUrl = _configuration.GetValue<string>("ApiEndPoints:getStorageVolumesUrl");
            getNetworkAdaptersUrl = _configuration.GetValue<string>("ApiEndPoints:getNetworkAdaptersUrl");
            getGraphicCardsUrl = _configuration.GetValue<string>("ApiEndPoints:getGraphicCardsUrl");
            getRAIDControllersUrl = _configuration.GetValue<string>("ApiEndPoints:getRAIDControllersUrl");
            getPhysicalDrivesUrl = _configuration.GetValue<string>("ApiEndPoints:getPhysicalDrivesUrl");
            getOtherSpecificationsUrl = _configuration.GetValue<string>("ApiEndPoints:getOtherSpecificationsUrl");
            getAccountsUrl = _configuration.GetValue<string>("ApiEndPoints:getAccountsUrl");
            getActivePortsUrl = _configuration.GetValue<string>("ApiEndPoints:getActivePortsUrl");
            getActiveNetworkDetailsUrl = _configuration.GetValue<string>("ApiEndPoints:getActiveNetworkDetailsUrl");
            getResourceUtilUrl = _configuration.GetValue<string>("ApiEndPoints:getResourceUtilUrl");
            getDiskDetailsUrl = _configuration.GetValue<string>("ApiEndPoints:getDiskDetailsUrl");
            getDevicesUrl = _configuration.GetValue<string>("ApiEndPoints:getDevicesUrl");
            approveDeviceUrl = _configuration.GetValue<string>("ApiEndPoints:approveDeviceUrl");
            deleteDeviceUrl = _configuration.GetValue<string>("ApiEndPoints:deleteDeviceUrl");
            isAgentInstalledUrl = _configuration.GetValue<string>("ApiEndPoints:isAgentInstalledUrl");
            getDevicesForUpdateUrl = _configuration.GetValue<string>("ApiEndPoints:getDevicesForUpdateUrl");
            getCurrentVersionUrl = _configuration.GetValue<string>("ApiEndPoints:getCurrentVersionUrl");
            getLatestVersionUrl = _configuration.GetValue<string>("ApiEndPoints:getLatestVersionUrl");
            addDeviceInPatchQueueUrl = _configuration.GetValue<string>("ApiEndPoints:addDeviceInPatchQueueUrl");
            isInQueueWithDeviceIDUrl = _configuration.GetValue<string>("ApiEndPoints:isInQueueWithDeviceIDUrl");
            getBIOSurl = _configuration.GetValue<string>("ApiEndPoints:getBIOSurl");
            mastersUrl = _configuration.GetValue<string>("ApiEndPoints:mastersUrl");
            addWorkstationUrl = _configuration.GetValue<string>("ApiEndPoints:addWorkstationUrl");
            updateWorkstationUrl = _configuration.GetValue<string>("ApiEndPoints:updateWorkstationUrl");
            checkBIOSUrl = _configuration.GetValue<string>("ApiEndPoints:checkBIOSUrl");
            getAssetIDsUrl = _configuration.GetValue<string>("ApiEndPoints:getAssetIDsUrl");
            eaAPI = _eaAPI;

            _tokenExtraction = tokenExtraction;
            _tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();
                        
        }

  

        [HttpGet]
        public async Task<IActionResult> Index(string devType = null)
        {
            //List<DeviceList> deviceList = new List<DeviceList>();
            //try
            //{
            //    ViewBag.FirstName = _tokenProperty.FirstName;
            //    ViewBag.LastName = _tokenProperty.LastName;
            //    ViewBag.Email = _tokenProperty.Email;
            //    ViewBag.Username = _tokenProperty.Name;
            //    string role = _tokenProperty.Role;                

            //    // Now 'values' should contain ["admin", "5"]

            //    if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
            //    {
            //        return View("AccessDenied");
            //    }
            //    string orgId = _tokenProperty.OrgId;
            //    string user = _tokenProperty.Name;
            //    string? token = _tokenProperty.Token;

            //    // fetch device data through EndAgent_API
            //    List<DeviceDTO> eaDevices = await eaAPI.GetDevices(orgId, user, baseUrl, getDevicesUrl, eaApiKeyHeader, token);
            //    if (eaDevices == null)
            //    {
            //        //return NotFound(new { message = "eaDevices API error." });
            //        return View(deviceList);
            //    }

            //    eaDevices = eaDevices.Where(a => a.IsApproved == true).ToList();
            //    if (eaDevices == null)
            //    {
            //        _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
            //        //return View("Error404"); 
            //        return View(deviceList);
            //    }

            //    //List<DeviceList> deviceList = new List<DeviceList>();
            //    foreach (var device in eaDevices)
            //    {

            //        string status = await DeviceStatus(device.ID.ToString()); // Call DeviceStatus action to get status
            //        deviceList.Add(new DeviceList
            //        {
            //            SystemName = device.SystemName,
            //            LoginUser = device.LoginUser,
            //            Domain = device.Domain,
            //            Privileges = device.Privileges,
            //            Manufacturer = device.Manufacturer,
            //            OS = device.OS,
            //            PublicIP = device.PublicIP,
            //            ObjectId = device.ID,
            //            InITAM = device.InITAM.ToString(),
            //            IsApproved = device.IsApproved,
            //            BIOS = device.BIOS,
            //            Status = status // Set status based on the response
            //        });
            //    }
            //    // Dev: Viraj; Date:24-05-2024; To check if Asst exists in ITAM or not
            //    foreach (var device in deviceList)
            //    {
            //        ITAMBIOSDetailsDTO temp = new ITAMBIOSDetailsDTO();
            //        temp.AssetID = "";
            //        temp.BIOS_SN = device.BIOS;
            //        temp.TrackerID = device.ObjectId;
            //        // it will check bios_sn and trackerid in ITAM_QA db, if found it will return assetid with exists as string. ex: org1-cpu-1,exists
            //        var checkBiosOutput = await eaAPI.CheckBIOS(temp, orgId, checkBIOSUrl, ITAMApiKeyHeader, ITAMApiKey);
            //        if (checkBiosOutput == "false")
            //        {
            //            device.InITAM = "Add in ITAM Registry";
            //        }
            //        else
            //        {
            //            var checkBiosOutputVM = JsonConvert.DeserializeObject<CheckBIOSOutputViewModel>(checkBiosOutput);
            //            string check = checkBiosOutputVM.Asset;
            //            if (check.Contains("exists"))
            //            {
            //                string[] parts = check.Split(',');
            //                if (parts.Length == 2 && parts[1] == "exists")
            //                {
            //                    device.InITAM = parts[0]; // Return AssetID
            //                    device.AssetId = parts[0];
            //                }
            //            }
            //            else if ((check.Contains("link")))
            //            {
            //                string[] parts = check.Split(',');
            //                if (parts.Length == 2 && parts[1] == "link")
            //                {
            //                    device.InITAM = "Link to " + parts[0]; // Return AssetID
            //                    device.AssetId = parts[0];
            //                }
            //            }
            //            else
            //            {
            //                device.InITAM = "-";
            //            }

            //            device.DeviceType = checkBiosOutputVM.DeviceType;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string user = _tokenProperty.Name;
            //    _logger.LogInformation($"User: {user}\nException: {ex}"); 
            //    //return BadRequest();
            //}
            List<DeviceList> deviceList =await _getApprovedDetails();
            return View(deviceList);
        }

        private async Task<List<DeviceList>> _getApprovedDetails()
        {
            List<DeviceList> deviceList = new List<DeviceList>();
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    //return View("AccessDenied");
                    return null;
                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                // fetch device data through EndAgent_API
                List<DeviceDTO> eaDevices = await eaAPI.GetDevices(orgId, user, baseUrl, getDevicesUrl, eaApiKeyHeader, token);
                if (eaDevices == null)
                {
                    //return NotFound(new { message = "eaDevices API error." });
                    return deviceList;
                }

                eaDevices = eaDevices.Where(a => a.IsApproved == true).ToList();
                if (eaDevices == null)
                {
                    _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
                    //return View("Error404"); 
                    return deviceList;
                }

                //List<DeviceList> deviceList = new List<DeviceList>();
                foreach (var device in eaDevices)
                {

                    string status = await DeviceStatus(device.ID.ToString()); // Call DeviceStatus action to get status
                    deviceList.Add(new DeviceList
                    {
                        SystemName = device.SystemName,
                        LoginUser = device.LoginUser,
                        Domain = device.Domain,
                        Privileges = device.Privileges,
                        Manufacturer = device.Manufacturer,
                        OS = device.OS,
                        PublicIP = device.PublicIP,
                        ObjectId = device.ID,
                        InITAM = device.InITAM.ToString(),
                        IsApproved = device.IsApproved,
                        BIOS = device.BIOS,
                        Status = status // Set status based on the response
                    });
                }
                // Dev: Viraj; Date:24-05-2024; To check if Asst exists in ITAM or not
                foreach (var device in deviceList)
                {
                    ITAMBIOSDetailsDTO temp = new ITAMBIOSDetailsDTO();
                    temp.AssetID = "";
                    temp.BIOS_SN = device.BIOS;
                    temp.TrackerID = device.ObjectId;
                    // it will check bios_sn and trackerid in ITAM_QA db, if found it will return assetid with exists as string. ex: org1-cpu-1,exists
                    var checkBiosOutput = await eaAPI.CheckBIOS(temp, orgId, checkBIOSUrl, ITAMApiKeyHeader, ITAMApiKey);
                    
                    if (checkBiosOutput == "false")
                    {
                        device.InITAM = "Add in ITAM Registry";
                    }
                    else
                    {
                        var checkBiosOutputVM = JsonConvert.DeserializeObject<CheckBIOSOutputViewModel>(checkBiosOutput);
                        string check = checkBiosOutputVM.Asset;
                        if (check.Contains("exists"))
                        {
                            string[] parts = check.Split(',');
                            if (parts.Length == 2 && parts[1] == "exists")
                            {
                                device.InITAM = parts[0]; // Return AssetID
                                device.AssetId = parts[0];
                            }
                        }
                        else if ((check.Contains("link")))
                        {
                            string[] parts = check.Split(',');
                            if (parts.Length == 2 && parts[1] == "link")
                            {
                                device.InITAM = "Link to " + parts[0]; // Return AssetID
                                device.AssetId = parts[0];
                            }
                        }
                        else
                        {
                            device.InITAM = "-";
                        }

                        device.DeviceType = checkBiosOutputVM.DeviceType;
                    }
                }
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                //return BadRequest();
            }

            return deviceList;
        }

        public async Task<JsonResult> GetApprovedDetails()
        {
            try
            {
                var _approvedData = await _getApprovedDetails();
                if(_approvedData.Any())
                    return Json(_approvedData);

                return Json(null);
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"GetApprovedDetails: \nException: {ex}");
                return Json(null);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetApprovedDetailsPaged(int page = 1, int pageSize = 25)
        {
            try
            {
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                // 1. Get all devices once
                List<DeviceDTO> eaDevices = await eaAPI.GetDevices(orgId, user, baseUrl, getDevicesUrl, eaApiKeyHeader, token);
                if (eaDevices == null) return Json(new { data = new List<DeviceList>(), total = 0 });

                var approvedDevices = eaDevices.Where(a => a.IsApproved == true).ToList();
                int total = approvedDevices.Count;

                // 2. Apply pagination BEFORE the expensive operations
                var pagedDevices = approvedDevices
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // 3. Run DeviceStatus calls in PARALLEL (not sequential)
                var statusTasks = pagedDevices.Select(async device =>
                {
                    string status = await DeviceStatus(device.ID.ToString());
                    return new DeviceList
                    {
                        SystemName = device.SystemName,
                        LoginUser = device.LoginUser,
                        Domain = device.Domain,
                        Privileges = device.Privileges,
                        Manufacturer = device.Manufacturer,
                        OS = device.OS,
                        PublicIP = device.PublicIP,
                        ObjectId = device.ID,
                        InITAM = device.InITAM.ToString() ?? "",
                        IsApproved = device.IsApproved,
                        BIOS = device.BIOS,
                        Status = status
                    };
                });

                var deviceList = (await Task.WhenAll(statusTasks)).ToList();

                // 4. Run BIOS checks in PARALLEL
                var biosCheckTasks = deviceList.Select(async device =>
                {
                    ITAMBIOSDetailsDTO temp = new ITAMBIOSDetailsDTO
                    {
                        AssetID = "",
                        BIOS_SN = device.BIOS,
                        TrackerID = device.ObjectId
                    };

                    var checkBiosOutput = await eaAPI.CheckBIOS(temp, orgId, checkBIOSUrl, ITAMApiKeyHeader, ITAMApiKey);

                    if (checkBiosOutput == "false" || checkBiosOutput == null)
                    {
                        device.InITAM = "Add in ITAM Registry";
                    }
                    else
                    {
                        try
                        {
                            var checkBiosOutputVM = JsonConvert.DeserializeObject<CheckBIOSOutputViewModel>(checkBiosOutput);
                            string check = checkBiosOutputVM?.Asset ?? "";

                            if (check.Contains("exists"))
                            {
                                string[] parts = check.Split(',');
                                if (parts.Length == 2)
                                {
                                    device.InITAM = parts[0];
                                    device.AssetId = parts[0];
                                }
                            }
                            else if (check.Contains("link"))
                            {
                                string[] parts = check.Split(',');
                                if (parts.Length == 2)
                                {
                                    device.InITAM = "Link to " + parts[0];
                                    device.AssetId = parts[0];
                                }
                            }
                            else
                            {
                                device.InITAM = "-";
                            }

                            device.DeviceType = checkBiosOutputVM?.DeviceType ?? "";
                        }
                        catch
                        {
                            device.InITAM = "-";
                        }
                    }
                    return device;
                });

                var finalList = (await Task.WhenAll(biosCheckTasks)).ToList();

                return Json(new { data = finalList, total = total });
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"GetApprovedDetailsPaged error: {ex}");
                return Json(new { data = new List<DeviceList>(), total = 0 });
            }
        }

        [HttpGet]
        public async Task<string> DeviceStatus(string ID)
            {
            try
            {
                string role = _tokenProperty.Role;
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;


                string status = await eaAPI.GetActiveStatus(orgId, user, ID, baseUrl, getActiveStatusUrl, eaApiKeyHeader, token);

                if (status != null)
                {
                    return status;
                }
                else
                {
                    return "offline";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in DeviceStatus action: " + ex.Message);
                return "offline";
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeviceInfo(string ID)
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;
                string role = _tokenProperty.Role;
                string user = _tokenProperty.Name;
                string orgId = _tokenProperty.OrgId;
                string? token = _tokenProperty.Token;

                ViewData["ID"] = ID;

                

                // Check if the access token is expired
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var tokenExpiration = jwtToken?.ValidTo;

                if (tokenExpiration < DateTime.UtcNow)
                {
                    // If the access token has expired, redirect the user to the logout action
                    return RedirectToAction("Logout", "Account");
                }

                DeviceInformationViewModel _deviceInfo = new DeviceInformationViewModel();
                try
                {
                    _deviceInfo = await eaAPI.GetSystemOverview(orgId, user, baseUrl, getSysOverviewUrl, eaApiKeyHeader, token, ID);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"User: {user}\nException: {ex}");
                    return View("Error503");

                }
                if (_deviceInfo == null)
                {
                    _logger.LogInformation("Error in Workstation/DeviceInfo action.");
                    return View("Loading");
                }

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                
                var _getdeviceinfo = GetFullDeviceInfo(_deviceInfo);
                if(_getdeviceinfo!=null)
                    return View(_getdeviceinfo);

                return View("Error");

            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return View("Error");
            }
        }

        private EASpecificationsDTO GetFullDeviceInfo(DeviceInformationViewModel _deviceInfo)
        {
            try
            {
                // Apps
                List<AppsDTO> apps = _deviceInfo.InstalledApps;
                if (apps != null)
                {
                    foreach (var app in apps)
                    {
                        if (!String.IsNullOrEmpty(app.Size) && long.TryParse(app.Size, out long sizeBytes))
                        {
                            double totalSizeKB = sizeBytes;
                            double totalSizeMB = totalSizeKB / 1024.0;
                            double totalSizeGB = totalSizeMB / 1024.0;
                            double totalSizeTB = totalSizeGB / 1024.0;

                            if (totalSizeTB >= 1)
                            {
                                app.Size = $"{totalSizeTB:0.##} TB";
                            }
                            else if (totalSizeGB >= 1)
                            {
                                app.Size = $"{totalSizeGB:0.##} GB";
                            }
                            else if (totalSizeMB >= 1)
                            {
                                app.Size = $"{totalSizeMB:0.##} MB";
                            }
                            else if (totalSizeKB >= 1)
                            {
                                app.Size = $"{totalSizeKB:0.##} KB";
                            }
                        }
                        else
                        {
                            // Handle or log the case where parsing fails, if necessary.
                            // For now, you can skip the current iteration using continue.
                            continue;
                        }
                    }

                    ViewBag.AppsModel = apps;
                }

                ViewBag.PatchesModel = _deviceInfo.Updates;
                ViewBag.ProcessesModel = _deviceInfo.Processes;
                // Services
                List<ServicesDTO> services = _deviceInfo.Services;
                if (services != null)
                {
                    foreach (ServicesDTO service in services)
                    {
                        if (service.PID == "0")
                        {
                            service.PID = "-";
                        }
                    }
                    ViewBag.ServicesModel = services;
                }

                //Ports
                List<PortsDTO> ports = _deviceInfo.Ports;
                List<ActivePortDTO> activePorts = _deviceInfo.ActivePorts;
                List<PortDetails> portDetails = new List<PortDetails>();
                if (ports != null && activePorts != null)
                {
                    foreach (ActivePortDTO activePort in activePorts)
                    {
                        var temp = ports.FirstOrDefault(p => p.ProcessId == activePort.PID);
                        string pname;
                        if (temp != null)
                        {
                            pname = temp.ProcessName;
                        }
                        else
                        {
                            pname = "-";
                        }
                        PortDetails model = new()
                        {
                            PID = activePort.PID,
                            Proto = activePort.Proto,
                            LocalAddress = activePort.LocalAddress,
                            ForeignAddress = activePort.ForeignAddress,
                            State = activePort.State,
                            ProcessName = pname
                        };
                        portDetails.Add(model);
                    }
                    ViewBag.PortsModel = portDetails;
                }
                // RAM Details
                List<RAMDetailsDTO> rAMDetails = _deviceInfo.RAMDetails;
                if (rAMDetails != null)
                {
                    foreach (var ram in rAMDetails)
                    {
                        if (!String.IsNullOrEmpty(ram.Capacity) && long.TryParse(ram.Capacity, out long sizeBytes))
                        {
                            double totalSizeBytes = sizeBytes;
                            double totalSizeKB = totalSizeBytes / 1024.0;
                            double totalSizeMB = totalSizeKB / 1024.0;
                            double totalSizeGB = totalSizeMB / 1024.0;
                            double totalSizeTB = totalSizeGB / 1024.0;

                            if (totalSizeTB >= 1)
                            {
                                ram.Capacity = $"{totalSizeTB:0.##} TB";
                            }
                            else if (totalSizeGB >= 1)
                            {
                                ram.Capacity = $"{totalSizeGB:0.##} GB";
                            }
                            else if (totalSizeMB >= 1)
                            {
                                ram.Capacity = $"{totalSizeMB:0.##} MB";
                            }
                            else if (totalSizeKB >= 1)
                            {
                                ram.Capacity = $"{totalSizeKB:0.##} KB";
                            }
                        }
                        else
                        {
                            // Handle or log the case where parsing fails, if necessary.
                            // For now, you can skip the current iteration using continue.
                            continue;
                        }

                        var memType = ram.MemoryType;
                        switch (memType)
                        {
                            case "0": ram.MemoryType = "Unknown"; break;
                            case "1": ram.MemoryType = "Other"; break;
                            case "20": ram.MemoryType = "DDR"; break;
                            case "21": ram.MemoryType = "DDR2"; break;
                            case "22": ram.MemoryType = "DDR2 FB-DIMM "; break;
                            case "24": ram.MemoryType = "DDR3"; break;
                            case "26": ram.MemoryType = "DDR4"; break;
                        }
                    }
                    ViewBag.RAMDetailsModel = rAMDetails;
                }
                // Scheduled Task
                List<ScheduledTasksDTO> scheduledTasks = _deviceInfo.ScheduledTasks;
                if (scheduledTasks != null)
                {
                    foreach (ScheduledTasksDTO sT in scheduledTasks)
                    {
                        if (sT.CreatedDate == "01-01-0001 00:00:00")
                        {
                            sT.CreatedDate = "-";
                        }
                        if (sT.NextRunTime == "01-01-0001 00:00:00")
                        {
                            sT.NextRunTime = "-";
                        }
                        if (sT.LastRunTime == "01-01-0001 00:00:00")
                        {
                            sT.LastRunTime = "-";
                        }
                    }
                    ViewBag.ScheduledTasksModel = scheduledTasks;
                }
                // Storage Volumn
                //ViewBag.StorageVolumesModel = _deviceInfo.StorageVolumes;
                List<StorageVolumesDTO> storageVolumes = _deviceInfo.StorageVolumes;
                if (storageVolumes != null)
                {
                    ViewBag.StorageVolumesModel = storageVolumes;
                    foreach (var sV in storageVolumes)
                    {
                        if (!String.IsNullOrEmpty(sV.Capacity) && long.TryParse(sV.Capacity, out long sizeBytes))
                        {
                            double totalSizeBytes = sizeBytes;
                            double totalSizeKB = totalSizeBytes / 1024.0;
                            double totalSizeMB = totalSizeKB / 1024.0;
                            double totalSizeGB = totalSizeMB / 1024.0;
                            double totalSizeTB = totalSizeGB / 1024.0;

                            if (totalSizeTB >= 1)
                            {
                                sV.Capacity = $"{totalSizeTB:0.##} TB";
                            }
                            else if (totalSizeGB >= 1)
                            {
                                sV.Capacity = $"{totalSizeGB:0.##} GB";
                            }
                            else if (totalSizeMB >= 1)
                            {
                                sV.Capacity = $"{totalSizeMB:0.##} MB";
                            }
                            else if (totalSizeKB >= 1)
                            {
                                sV.Capacity = $"{totalSizeKB:0.##} KB";
                            }
                        }
                        else
                        {
                            // Handle or log the case where parsing fails, if necessary.
                            // For now, you can skip the current iteration using continue.
                            continue;
                        }

                        if (!String.IsNullOrEmpty(sV.FreeSpace) && long.TryParse(sV.FreeSpace, out sizeBytes))
                        {
                            double totalSizeBytes = sizeBytes;
                            double totalSizeKB = totalSizeBytes / 1024.0;
                            double totalSizeMB = totalSizeKB / 1024.0;
                            double totalSizeGB = totalSizeMB / 1024.0;
                            double totalSizeTB = totalSizeGB / 1024.0;

                            if (totalSizeTB >= 1)
                            {
                                sV.FreeSpace = $"{totalSizeTB:0.##} TB";
                            }
                            else if (totalSizeGB >= 1)
                            {
                                sV.FreeSpace = $"{totalSizeGB:0.##} GB";
                            }
                            else if (totalSizeMB >= 1)
                            {
                                sV.FreeSpace = $"{totalSizeMB:0.##} MB";
                            }
                            else if (totalSizeKB >= 1)
                            {
                                sV.FreeSpace = $"{totalSizeKB:0.##} KB";
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                //Graphic Cards
                List<GraphicCardsDTO> graphicCards = _deviceInfo.GraphicCards;
                if (graphicCards != null)
                {
                    foreach (var gCard in graphicCards)
                    {
                        if (!String.IsNullOrEmpty(gCard.AdapterRAM) && long.TryParse(gCard.AdapterRAM, out long sizeBytes))
                        {
                            double totalSizeBytes = sizeBytes;
                            double totalSizeKB = totalSizeBytes / 1024.0;
                            double totalSizeMB = totalSizeKB / 1024.0;
                            double totalSizeGB = totalSizeMB / 1024.0;
                            double totalSizeTB = totalSizeGB / 1024.0;

                            if (totalSizeTB >= 1)
                            {
                                gCard.AdapterRAM = $"{totalSizeTB:0.##} TB";
                            }
                            else if (totalSizeGB >= 1)
                            {
                                gCard.AdapterRAM = $"{totalSizeGB:0.##} GB";
                            }
                            else if (totalSizeMB >= 1)
                            {
                                gCard.AdapterRAM = $"{totalSizeMB:0.##} MB";
                            }
                            else if (totalSizeKB >= 1)
                            {
                                gCard.AdapterRAM = $"{totalSizeKB:0.##} KB";
                            }
                        }
                        else
                        {
                            // Handle or log the case where parsing fails, if necessary.
                            // For now, you can skip the current iteration using continue.
                            continue;
                        }
                    }
                    ViewBag.GraphicCardsModel = graphicCards;
                    ViewBag.GPUName = graphicCards.FirstOrDefault().Caption.ToString() ?? "-";
                }
                // NetworkAdatper and ActiveNetworkAdapter
                List<NetworkAdaptersDTO> networkAdapters = _deviceInfo.NetworkAdapters;
                if (networkAdapters != null)
                {
                    List<ActiveNetworkDetailDTO> activeNetworkDetails = _deviceInfo.ActiveNetworkDetails;
                    if (activeNetworkDetails != null)
                    {
                        foreach (ActiveNetworkDetailDTO x in activeNetworkDetails)
                        {
                            var networkAdapter = networkAdapters.FirstOrDefault(n => n.Description == x.Description);
                            if (networkAdapter != null)
                            {
                                networkAdapter.ActiveNetworkDetails = x;
                            }
                        }
                    }
                    ViewBag.NetworkAdaptersModel = networkAdapters.OrderByDescending(m => m.Status).ToList();
                }

                ViewBag.RAIDControllersModel = _deviceInfo.RAIDControllers;
                ViewBag.PhysicalDrivesModel = _deviceInfo.DiskDetails;
                ViewBag.ProcessorDetailsModel = _deviceInfo.Processors;
                // Other Specification
                OtherSpecificationDTO otherSpecs = _deviceInfo.OtherSpecifications;
                if (otherSpecs != null)
                {
                    if (!string.IsNullOrEmpty(otherSpecs.MACAAddress))
                    {
                        string oldMAC = otherSpecs.MACAAddress;
                        string newMAC = "";
                        for (int i = 0; i < oldMAC.Length; i += 2)
                        {
                            if (i > 0)
                            {
                                newMAC += ":";
                            }
                            newMAC += oldMAC.Substring(i, Math.Min(2, oldMAC.Length - i));
                        }

                        otherSpecs.MACAAddress = newMAC;
                    }

                    ViewBag.CPUName = otherSpecs.CPUName;
                    ViewBag.OS = _deviceInfo.EASpecificationDTO.OperatingSystem;
                    ViewBag.OSVersion = _deviceInfo.EASpecificationDTO.OSVersion;
                    ViewBag.OSBuildVersion = _deviceInfo.EASpecificationDTO.OSBuildVersion;
                    //ViewBag.OSVersion = otherSpecs.OSVersion;
                    //ViewBag.OSBuildVersion = otherSpecs.OSBuildVersion;
                    ViewBag.SystemUptime = otherSpecs.SystemUptime;
                    ViewBag.SystemModel = otherSpecs.SystemModel;
                    ViewBag.SystemManufacturer = otherSpecs.SystemManufacturer;
                    ViewBag.SerialNumber = otherSpecs.SerialNumber;
                    ViewBag.InstalledRAM = otherSpecs.InstalledRAM;
                    ViewBag.MACAAddress = otherSpecs.MACAAddress;
                    ViewBag.BIOSVersion = otherSpecs.BIOSVersion;
                    ViewBag.Antivirus = otherSpecs.Antivirus;
                }
                // Account
                ViewBag.Accounts = _deviceInfo.Accounts;
                EASpecificationsDTO _eaDevice = new EASpecificationsDTO();

                _eaDevice.SystemName = _deviceInfo.EASpecificationDTO.SystemName;
                _eaDevice.SystemStatus = _deviceInfo.EASpecificationDTO.SystemStatus;
                _eaDevice.OperatingSystem = _deviceInfo.EASpecificationDTO.OperatingSystem;
                _eaDevice.OSVersion = _deviceInfo.EASpecificationDTO.OSVersion;
                _eaDevice.OSBuildVersion = _deviceInfo.EASpecificationDTO.OSBuildVersion;
                
                _eaDevice.LoginUser = _deviceInfo.EASpecificationDTO.LoginUser;
                _eaDevice.LastActive = _deviceInfo.EASpecificationDTO.LastActive;
                _eaDevice.Domain = _deviceInfo.EASpecificationDTO.Domain;
                _eaDevice.Privileges = _deviceInfo.EASpecificationDTO.Privileges;
                _eaDevice.NetworkAdapter = _deviceInfo.EASpecificationDTO.NetworkAdapter;
                _eaDevice.IPv4Address = _deviceInfo.EASpecificationDTO.IPv4Address;
                _eaDevice.IPv6Address = _deviceInfo.EASpecificationDTO.IPv6Address;
                _eaDevice.Gateway = _deviceInfo.EASpecificationDTO.Gateway;
                _eaDevice.SubnetMask = _deviceInfo.EASpecificationDTO.SubnetMask;
                _eaDevice.Latitude = _deviceInfo.LocationDTO.Latitude;
                _eaDevice.Longitude = _deviceInfo.LocationDTO.Longitude;


                return _eaDevice;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        // Dev : Srikanth Erukulla - Date : 28-01-2026
        public async Task<PartialViewResult> GetDeviceInfoInPV(string ID)
        {
            try
            {
                var _deviceInfo = await eaAPI.GetSystemOverview(_tokenProperty.OrgId, _tokenProperty.Name, baseUrl, getSysOverviewUrl, eaApiKeyHeader, _tokenProperty.Token, ID);
                if (_deviceInfo != null)
                {
                    var _getdeviceinfo = GetFullDeviceInfo(_deviceInfo);
                    if (_getdeviceinfo != null)
                        return PartialView("DeviceInfoPV", _getdeviceinfo);

                    return PartialView("DeviceInfoPV", _getdeviceinfo);
                }
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"GetDeviceInfoInPV, error occured in {ex}");
            }

            return PartialView("DeviceInfoPV", null);

        }

        // Dev: Viraj; Date: 06-04-2024
        [HttpGet]
        public async Task<IActionResult> AgentManager()
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "SuperAdmin2")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var tokenExpiration = jwtToken?.ValidTo;

                if (tokenExpiration < DateTime.UtcNow)
                {
                    // If the access token has expired, redirect the user to the logout action
                    return RedirectToAction("Logout", "Account");
                }

                List<DeviceDTO> eaDevices = await eaAPI.GetDevicesForUpdate(orgId, user, baseUrl, getDevicesForUpdateUrl, eaApiKeyHeader, token);
                if (eaDevices == null)
                {
                    return NotFound(new { message = "eaDevices API error." });
                }
                eaDevices = eaDevices.Where(a => a.IsApproved == true).ToList();
                if (eaDevices == null)
                {
                    _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
                    return View("Error404");
                }

                List<AgentManagerDeviceList> deviceList = new List<AgentManagerDeviceList>();
                foreach (var device in eaDevices)
                {
                    string currentVersion = await eaAPI.GetCurrentVersion(device.ID.ToString(), baseUrl, getCurrentVersionUrl);
                    string latestVersion = await eaAPI.GetLatestVersion(baseUrl, getLatestVersionUrl);
                    if (!string.IsNullOrEmpty(latestVersion))
                    { 
                        bool canUpdate = !(latestVersion == currentVersion);
                        string inQueue = await eaAPI.IsInQueue(baseUrl, isInQueueWithDeviceIDUrl, device.ID.ToString());
                        if (canUpdate && inQueue.ToString() == "false")
                        {
                            deviceList.Add(new AgentManagerDeviceList
                            {
                                SystemName = device.SystemName,
                                LoginUser = device.LoginUser,
                                Domain = device.Domain,
                                Privileges = device.Privileges,
                                Manufacturer = device.Manufacturer,
                                OS = device.OS,
                                PublicIP = device.PublicIP,
                                ObjectId = device.ID,
                                CurrentVersion = currentVersion,
                                LatestVersion = latestVersion,
                                OrgID = device.OrgID
                            });
                        }
                    }
                }
                return View(deviceList);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        // Dev: Viraj; Date: 06-04-2024
        [HttpGet]
        public async Task<IActionResult> AgentManagerQueue()
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "SuperAdmin2")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var tokenExpiration = jwtToken?.ValidTo;

                if (tokenExpiration < DateTime.UtcNow)
                {
                    // If the access token has expired, redirect the user to the logout action
                    return RedirectToAction("Logout", "Account");
                }

                List<DeviceDTO> eaDevices = await eaAPI.GetDevicesForUpdate(orgId, user, baseUrl, getDevicesForUpdateUrl, eaApiKeyHeader, token);
                if (eaDevices == null)
                {
                    return NotFound(new { message = "eaDevices API error." });
                }
                eaDevices = eaDevices.Where(a => a.IsApproved == true).ToList();
                if (eaDevices == null)
                {
                    _logger.LogInformation("Error in Workstation/Index action while using GetEADevice.");
                    return View("Error404");
                }

                List<AgentManagerDeviceList> deviceList = new List<AgentManagerDeviceList>();
                foreach (var device in eaDevices)
                {
                    string currentVersion = await eaAPI.GetCurrentVersion(device.ID.ToString(), baseUrl, getCurrentVersionUrl);
                    string latestVersion = await eaAPI.GetLatestVersion(baseUrl, getLatestVersionUrl);
                    bool canUpdate = !(latestVersion == currentVersion);
                    string inQueue = await eaAPI.IsInQueue(baseUrl, isInQueueWithDeviceIDUrl, device.ID.ToString());
                    if (inQueue.ToString() == "true")
                    {
                        deviceList.Add(new AgentManagerDeviceList
                        {
                            SystemName = device.SystemName,
                            LoginUser = device.LoginUser,
                            Domain = device.Domain,
                            Privileges = device.Privileges,
                            Manufacturer = device.Manufacturer,
                            OS = device.OS,
                            PublicIP = device.PublicIP,
                            ObjectId = device.ID,
                        });
                    }
                }
                return View(deviceList);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }


        // Dev: Viraj; Date: 06-04-2024
        [HttpPost]
        public async Task<IActionResult> AddDeviceInPatchQueue(List<int> IDs)
        {
            if (IDs != null)
            {
                foreach (int ID in IDs)
                {
                    await eaAPI.AddDeviceInPatchQueue(ID, baseUrl, addDeviceInPatchQueueUrl);
                }
                return RedirectToAction("AgentManager");
            }
            return RedirectToAction("AgentManager");

        }


        public PartialViewResult RenderProcessesTable(List<ProcessesDTO> processesModel)
        {
            return PartialView("Processes", processesModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetResourceUtil(string assetId)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                ViewData["assetId"] = assetId;
                ResourceUtilDTO res = await eaAPI.GetResourceUtil(orgId, user, baseUrl, getResourceUtilUrl, eaApiKeyHeader, token, assetId);
                if (res == null)
                {
                    _logger.LogInformation("Error in Workstation/GetResourceUtil action.");
                    return NotFound(new { message = "GetResourceUtil NotFound" });
                }
                else
                {
                    return Ok(res);
                }
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Apps(string assetId)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                ViewData["assetId"] = assetId;
                List<AppsDTO> apps = await eaAPI.GetApps(orgId, user, baseUrl, getEaAppsUrl, eaApiKeyHeader, token, assetId);
                if (apps == null)
                {
                    _logger.LogInformation("Error in Workstation/Apps action.");
                    return NotFound(new { message = "Device Apps NotFound" });
                }
                foreach (AppsDTO app in apps)
                {
                    if (app.Size != "-")
                    {
                        app.Size = (float.Parse(app.Size) / 1000).ToString() + " MB";
                    }
                }
                apps = apps.OrderBy(app => app.AppName).ToList();
                return View(apps);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Patches(string assetId)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                ViewData["assetId"] = assetId;
                List<PatchesDTO> patches = await eaAPI.GetPatches(orgId, user, baseUrl, getEaUpdatesUrl, eaApiKeyHeader, token, assetId);
                if (patches == null)
                {
                    _logger.LogInformation("Error in Workstation/Patches action.");
                    return NotFound(new { message = "Device Patches NotFound" });
                }
                return View(patches);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Ports(string assetId)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                ViewData["assetId"] = assetId;
                List<PortsDTO> ports = await eaAPI.GetPorts(orgId, user, baseUrl, getEaPortsUrl, eaApiKeyHeader, token, assetId);
                if (ports == null)
                {
                    _logger.LogInformation("Error in Workstation/Ports action.");
                    return NotFound(new { message = "Device Patches NotFound" });
                }
                return View(ports);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Approvals()
        {
            try
            {
                ViewBag.FirstName = _tokenProperty.FirstName;
                ViewBag.LastName = _tokenProperty.LastName;
                ViewBag.Email = _tokenProperty.Email;
                ViewBag.Username = _tokenProperty.Name;

                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");
                }

                string orgId = _tokenProperty.OrgId;
                string? token = _tokenProperty.Token;
                string user = _tokenProperty.Name;

                List<DeviceDTO> approvals = await eaAPI.GetDevices(orgId, user, baseUrl, getDevicesUrl, eaApiKeyHeader, token);
                if (approvals == null)
                {
                    //return NotFound(new { message = "approvals API error." });
                    return View(new List<DeviceDTO>());
                }
                approvals = approvals.Where(a => a.IsApproved == false).ToList();
                return View(approvals);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetUnApprovedDetails()
        {
            List<DeviceDTO> approvals = await eaAPI.GetDevices(_tokenProperty.OrgId, _tokenProperty.Name, baseUrl, getDevicesUrl, eaApiKeyHeader, _tokenProperty.Token);
            if (approvals != null)
                approvals = approvals.Where(a => a.IsApproved == false).ToList();

            return Json(approvals);
        }

        [HttpGet]
        public async Task<JsonResult> GetUnApprovedDetailsPaged(int page = 1, int pageSize = 25)
        {
            try
            {
                List<DeviceDTO> approvals = await eaAPI.GetDevices(
                    _tokenProperty.OrgId, _tokenProperty.Name,
                    baseUrl, getDevicesUrl, eaApiKeyHeader, _tokenProperty.Token);

                if (approvals == null)
                    return Json(new { data = new List<DeviceDTO>(), total = 0 });

                var unapproved = approvals.Where(a => a.IsApproved == false).ToList();
                int total = unapproved.Count;

                var paged = unapproved
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Json(new { data = paged, total = total });
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"GetUnApprovedDetailsPaged error: {ex}");
                return Json(new { data = new List<DeviceDTO>(), total = 0 });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ApproveDevice(string ID)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string? token = _tokenProperty.Token;
                string user = _tokenProperty.Name;

                bool response = await eaAPI.ApproveDevice(ID, user, baseUrl, approveDeviceUrl, eaApiKeyHeader, token);
                return Json(response);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteDevice(string ID)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string? token = _tokenProperty.Token;
                string user = _tokenProperty.Name;

                bool response = await eaAPI.DeleteDevice(ID, user, baseUrl, deleteDeviceUrl, eaApiKeyHeader, token);
                return Json(response);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest();
            } 
        }

        [HttpGet]
        public async Task<IActionResult> Services(string assetId)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;


                string? token = _tokenProperty.Token;
                ViewData["assetId"] = assetId;
                List<ServicesDTO> services = await eaAPI.GetServices(orgId, user, baseUrl, getServiceDetailsUrl, eaApiKeyHeader, token, assetId);
                if (services == null)
                {
                    _logger.LogInformation("Error in Workstation/Services action.");
                    return NotFound(new { message = "Device Services NotFound" });
                }
                return View(services);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Processes(string assetId)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;


                string? token = _tokenProperty.Token;
                ViewData["assetId"] = assetId;
                List<ProcessesDTO> processes = await eaAPI.GetProcesses(orgId, user, baseUrl, getProcessDetailsUrl, eaApiKeyHeader, token, assetId);
                if (processes == null)
                {
                    _logger.LogInformation("Error in Workstation/Processes action.");
                    return NotFound(new { message = "Device Processes NotFound" });
                }
                return View(processes);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Processors(string assetId)
        {
            try
            {
                string role = _tokenProperty.Role;
                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;

                string? token = _tokenProperty.Token;
                ViewData["assetId"] = assetId;
                List<ProcessorDTO> processors = await eaAPI.GetProcessors(orgId, user, baseUrl, getProcessorDetailsUrl, eaApiKeyHeader, token, assetId);
                if (processors == null)
                {
                    _logger.LogInformation("Error in Workstation/Processors action.");
                    return NotFound(new { message = "Device Processors NotFound" });
                }
                return View(processors);
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return BadRequest(ex.ToString());
            }
        }

        public async Task<JsonResult> ApproveOrDeny(string approve, string assetId)
        {
            try
            {
                string orgId = _tokenProperty.OrgId;


                string result = await eaAPI.ApproveMonitoring(int.Parse(orgId), assetId, approve, ITAMbaseUrl, approveMonitoringUrl, ITAMApiKeyHeader, ITAMApiKey);
                if (result == "success")
                {
                    return new JsonResult(new { result });
                }
                return null;
            }
            catch (Exception ex)
            {
                string user = _tokenProperty.Name;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                throw;
            }
        }
        

		// Dev: Viraj; Date:24-05-2024; To open a form to add new workstation or liunk to existing
        [HttpGet]
        public async Task<IActionResult> AddInRegistry (string id, string bios)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;

                ViewBag.ID = id;
                //BIOSDetailsDTO model = await eaAPI.GetBIOS(id, token, getBIOSurl);
                ViewBag.BIOS = bios;
                ViewBag.Obj = id;
                List<GetMasters> data = new List<GetMasters>();
                //data = await eaAPI.GetMastersDataFromITAM(1, mastersUrl, ITAMApiKeyHeader, ITAMApiKey);

                data = await eaAPI.GetMastersDataFromITAM(Convert.ToInt32(orgId), mastersUrl, ITAMApiKeyHeader, ITAMApiKey);

                if (data == null)
                {
                    // Handle case where data retrieval fails
                    _logger.LogError("Failed to retrieve dropdown data from GetMasters endpoint");
                    //return BadRequest("Failed to retrieve dropdown data");
                    return View("Error");
                }

                var viewModel = new WorkstationViewModel
                {
                    MasterData = data,
                    WorkstationModel = new AddWorkstationDTO()
                };
                OtherSpecificationDTO otherSpecs = new OtherSpecificationDTO();
                otherSpecs = await eaAPI.GetOtherSpecifications(orgId, user, baseUrl, getOtherSpecificationsUrl, eaApiKeyHeader, token, id);
                if (otherSpecs != null)
                {
                    if (!string.IsNullOrEmpty(otherSpecs.MACAAddress))
                    {
                        string oldMAC = otherSpecs.MACAAddress;
                        string newMAC = "";
                        for (int i = 0; i < oldMAC.Length; i += 2)
                        {
                            if (i > 0)
                            {
                                newMAC += ":";
                            }
                            newMAC += oldMAC.Substring(i, Math.Min(2, oldMAC.Length - i));
                        }

                        otherSpecs.MACAAddress = newMAC;
                    }
                    viewModel.WorkstationModel.SerialNumber = otherSpecs != null ? otherSpecs.SerialNumber : (string?)null;
                    viewModel.WorkstationModel.Model = otherSpecs != null ? otherSpecs.SystemModel : (string?)null;
                    viewModel.WorkstationModel.Processor = otherSpecs != null ? otherSpecs.CPUName : (string?)null;
                    viewModel.WorkstationModel.MacAddress = otherSpecs != null ? otherSpecs.MACAAddress : (string?)null;
                    var ram = otherSpecs != null ? otherSpecs.InstalledRAM.Split(" ") : null;
                    if (ram != null && ram.Length == 2)
                    {
                        var subData = data.FirstOrDefault(d => d.Description == ram[1]);
                        if (subData != null)
                        {
                            viewModel.WorkstationModel.Memorytype = int.Parse(subData.ID);
                        }
                        viewModel.WorkstationModel.RAM = int.Parse(ram[0]);
                    }

                    // Check if OperatingSystem is a substring of any description where type is OS
                    var osData = data.FirstOrDefault(d => d.Type == "OSMaster" && otherSpecs.OSVersion.Contains(d.Description, StringComparison.OrdinalIgnoreCase));
                    if (osData != null)
                    {
                        viewModel.WorkstationModel.OperatingSystem = osData.ID;
                    }

                    var oem = data.FirstOrDefault(d => d.Type == "OEM" && otherSpecs.SystemManufacturer.Contains(d.Description, StringComparison.OrdinalIgnoreCase));
                    if (oem != null)
                    {
                        viewModel.WorkstationModel.ManufacturerName = int.Parse(oem.ID);
                    }
                }

                return PartialView(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return View("Error");
            }
        }

		// Dev: Viraj; Date:24-05-2024; To add new workstations, laptops or servers from Device List
        [HttpPost]
        public async Task<IActionResult> AddWorkStation(AddWorkstationDTO workstation, string BIOS, string Obj)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                workstation.UpdatedUser = user;
                workstation.OrgID = int.Parse(orgId);
                workstation.Mapstatus = "U";
                workstation.devstatus = 1;
                workstation.BIOS_SN = BIOS;
                workstation.TrackerID = Obj;
                workstation.Updatedtime = DateTime.Now;
                var data = await eaAPI.AddWorkstation(workstation, addWorkstationUrl, ITAMApiKeyHeader, ITAMApiKey);
                if (data != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return View("Error");
            }
        }

		// Dev: Viraj; Date:24-05-2024; To link Workstations, Laptops or Servers from Device list to already existing ITAM Assets
        // Dev: Srikanth Erukulla - 06-05-2025 - update method created new functionality, stored procedure for updateworkstation
        [HttpPost]
        public async Task<IActionResult> LinkWorkstation(string ObjectId, string BIOS, string DeviceType, string AssetId)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return View("AccessDenied");

                }

                if (string.IsNullOrEmpty(ObjectId) || string.IsNullOrEmpty(DeviceType))
                    return View("Error");

                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;
                AddWorkstationDTO workstation = new AddWorkstationDTO();
                //BIOSDetailsDTO model = await eaAPI.GetBIOS(ObjectId, token, getBIOSurl);
                workstation.deviceType = DeviceType;
                workstation.UpdatedUser = user;
                workstation.OrgID = int.Parse(orgId);
                workstation.BIOS_SN = BIOS;
                workstation.TrackerID = ObjectId;
                workstation.Updatedtime = DateTime.Now;

                var data = await eaAPI.AddWorkstation(workstation, updateWorkstationUrl, ITAMApiKeyHeader, ITAMApiKey);
                if (data != null)
                {
                    return RedirectToAction("Index","Home");
                }
            return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return View("Error");
            }
        }

		// Dev: Viraj; Date:24-05-2024; Unique AssetID VAlidation
        [HttpGet]
        public async Task<bool> CheckAssetIDs(string assetID)
        {
            try
            {
                string role = _tokenProperty.Role;

                // Now 'values' should contain ["admin", "5"]

                if (string.IsNullOrEmpty(role) || role != "Admin" && role != "SuperAdmin")
                {
                    return false;

                }
                string orgId = _tokenProperty.OrgId;
                string user = _tokenProperty.Name;
                string? token = _tokenProperty.Token;
                var assetIDs = await eaAPI.GetAssetIDs(int.Parse(orgId), getAssetIDsUrl, ITAMApiKeyHeader, ITAMApiKey); 
                if (!assetIDs.Contains(assetID))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return false;
            }
        }

    }
}
