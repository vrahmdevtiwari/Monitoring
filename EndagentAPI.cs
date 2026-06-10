using Monitoring.Models;
using Monitoring.Models.DTO;
using Monitoring.ViewModel;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Monitoring.APIResponses
{
    public class EndagentAPI
    {
        private readonly ILogger<EndagentAPI> _logger;
        public EndagentAPI(ILogger<EndagentAPI> logger)
        {
            _logger = logger;
        }
        public async Task<PatchResponseDto> TriggerSyncWithPatchesAsync(string url,  List<string> familyList, string token, string orgId)
        {
            if (familyList == null || !familyList.Any())
                return new PatchResponseDto();

            HttpClientHandler handler = new HttpClientHandler();

            // Disable SSL validation (DEV ONLY)
            handler.ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;
            using var client = new HttpClient(handler);

            // Pass Bearer token
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Send family list as JSON in body
            var content = new StringContent(
                JsonConvert.SerializeObject(familyList),
                Encoding.UTF8,
                "application/json");

            // POST request to API1 SyncByOrgId
            var response = await client.PostAsync($"{url}/{orgId}", content);

            response.EnsureSuccessStatusCode();

            // Read response JSON
            var json = await response.Content.ReadAsStringAsync();

            // Deserialize only the patch response part
            var wrapper = JsonConvert.DeserializeObject<SyncResponseWrapper>(json);

            return wrapper != null
                ? new PatchResponseDto
                {
                    OSPatches = wrapper.osPatches ?? new List<CentralOSPatchesDTO>(),
                    SoftwarePatches = wrapper.softwarePatches ?? new List<CentralSoftwarepatchDTO>()
                }
                : new PatchResponseDto();
        }
        // Helper class to match controller response from API1
        private class SyncResponseWrapper
        {
            public bool success { get; set; }
            public string message { get; set; }
            public List<CentralOSPatchesDTO>? osPatches { get; set; }
            public List<CentralSoftwarepatchDTO>? softwarePatches { get; set; }
        }

        public async Task<PatchResponseDto> FetchOSPatchesAndSoftwares(string url, List<string> familyList, string token, string orgId)
        {
            if (familyList == null || !familyList.Any())
                return new PatchResponseDto();

            HttpClientHandler handler = new HttpClientHandler();

            // Disable SSL validation (DEV ONLY)
            handler.ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;
            using var client = new HttpClient(handler);

            // Pass Bearer token
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            //// Send family list as JSON in body
            //var content = new StringContent(
            //    JsonConvert.SerializeObject(familyList),
            //    Encoding.UTF8,
            //    "application/json");

            // POST request to API1 SyncByOrgId
            var response = await client.GetAsync($"{url}/{orgId}");

            response.EnsureSuccessStatusCode();

            // Read response JSON
            var json = await response.Content.ReadAsStringAsync();

            // Deserialize only the patch response part
            var wrapper = JsonConvert.DeserializeObject<SyncResponseWrapper>(json);

            return wrapper != null
                ? new PatchResponseDto
                {
                    OSPatches = wrapper.osPatches ?? new List<CentralOSPatchesDTO>(),
                    SoftwarePatches = wrapper.softwarePatches ?? new List<CentralSoftwarepatchDTO>()
                }
                : new PatchResponseDto();
        }

        public async Task<List<ProductFamiliesVM>> GetProductFamilies(String url,string bearerToken)
        {
            try
            {
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Do NOT use in Production (after implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL validation (DEV ONLY)
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}");


                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            List<ProductFamiliesVM> families =
                                JsonConvert.DeserializeObject<List<ProductFamiliesVM>>(responseContent);

                            return families;
                        }
                    }

                    return new List<ProductFamiliesVM>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error occurred in GetProductFamilies");
                return new List<ProductFamiliesVM>();
            }
        }
        public async Task<List<ProductFamiliesVM>> GetPatchesByFamily(String url, string bearerToken)
        {
            try
            {
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Do NOT use in Production (after implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL validation (DEV ONLY)
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            List<ProductFamiliesVM> families =
                                JsonConvert.DeserializeObject<List<ProductFamiliesVM>>(responseContent);

                            return families;
                        }
                    }

                    return new List<ProductFamiliesVM>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error occurred in GetProductFamilies");
                return new List<ProductFamiliesVM>();
            }
        }

        public async Task<APIResponse> SaveAsync(string url, string bearerToken,string orgId, string userId, string? families, string type="os")
        {
            try
            {
                string osfamilies = string.Empty, softwarefamilies = string.Empty;
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Do NOT use in Production (after implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();
                // Disable SSL validation (DEV ONLY)
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", bearerToken);

                    if (type == "os")
                        osfamilies = families;
                    if(type=="software")
                        softwarefamilies=families;

                    var payload = new
                    {
                        OrgId = orgId,
                        UserId = userId,
                        OsPatchesSelected = osfamilies,
                        SoftwarePatchesSelected = softwarefamilies
                    };

                    string jsonPayload = JsonConvert.SerializeObject(payload);
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                            return apiResponse;
                        }
                    }

                    return new APIResponse
                    {
                        Status = false,
                        Message = $"Request failed with status code: {response.StatusCode}",
                        StatusCode = ((int)response.StatusCode).ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error occurred in SaveAsync");
                return new APIResponse
                {
                    Status = false,
                    Message = ex.Message,
                    StatusCode = "500"
                };
            }
        }

        public async Task<List<EADeviceDTO>> GetEADevice(string orgId, string baseUrl, string endPoint, string apikeyheader, string apikey)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Add(apikeyheader, apikey);
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{endPoint}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            // Deserialize the JSON response into a list of EADeviceDTO objects
                            List<EADeviceDTO> devices = JsonConvert.DeserializeObject<List<EADeviceDTO>>(responseContent);
                            foreach (var device in devices)
                            {
                                if (device.MappedTo == "UnMapped")
                                {
                                    device.MappedSince = "-";
                                }
                                else if (device.MappedSince != null)
                                {
                                    DateTime dateTime = DateTime.Parse(device.MappedSince);
                                    string dateString = dateTime.ToString("yyyy-MM-dd");
                                    device.MappedSince = dateString;
                                }

                            }

                            // Return the list of EADeviceDTO objects
                            return devices;
                        }
                    }
                    return null;
                }
                //JSONString = JsonConvert.SerializeObject(ds.Tables[0]);
                //return Json(JSONString);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetEADevices functionality");
            }
            return null;
        }

        public async Task<DeviceInformationViewModel> GetSystemOverview(string orgId, string user, string baseUrl, string getSysOverviewUrl, string eaApiTokenHeader, string? eaApiToken, string assetID)
        {
            try
            {
                string org = orgId;

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    //httpClient.SetBearerToken(eaApiToken);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getSysOverviewUrl}/{user}/{org}/{assetID}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            // Deserialize the JSON response into a list of EADeviceDTO objects
                            DeviceInformationViewModel device = JsonConvert.DeserializeObject<DeviceInformationViewModel>(responseContent);

                            // Return the list of EADeviceDTO objects
                            return device;
                        }
                    }
                    return null;
                }
                //JSONString = JsonConvert.SerializeObject(ds.Tables[0]);
                //return Json(JSONString);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetSystemOverviews functionality");
            }
            return null;
        }

        public async Task<List<AppsDTO>> GetApps(string orgId, string user, string baseUrl, string getEaAppsUrl, string eaApiTokenHeader, string? eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getEaAppsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (responseContent != null)
                        {
                            List<AppsDTO> apps = JsonConvert.DeserializeObject<List<AppsDTO>>(responseContent);
                            return apps;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetApps functionality");
            }
            return null;
        }

        public async Task<List<PatchesDTO>> GetPatches(string orgId, string user, string baseUrl, string getEaUpdatesUrl, string eaApiTokenHeader, string? eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getEaUpdatesUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (responseContent != null)
                        {
                            List<PatchesDTO> patches = JsonConvert.DeserializeObject<List<PatchesDTO>>(responseContent);
                            foreach (PatchesDTO patch in patches)
                            {
                                if (patch.InstalledOn != "-")
                                {
                                    string[] dateString = patch.InstalledOn.Split(' ');

                                    // Check if there are at least two parts (date and time)
                                    if (dateString.Length >= 2)
                                    {
                                        // Get the date part (the first part) and format it as "dd/MM/yyyy"
                                        string[] datePart = dateString[0].Split("/");
                                        if (datePart.Length >= 2)
                                        {
                                            string date = datePart[1] + "-" + datePart[0] + "-" + datePart[2];
                                        }
                                    }
                                }
                            }
                            return patches;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetPatches functionality");
            }
            return null;
        }
        public async Task<string> GetPatchStatusAsync(string objId, string url, string token)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{url}/{objId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        
        public async Task<List<PortsDTO>> GetPorts(string orgId, string user, string baseUrl, string getEaPortsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getEaPortsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<PortsDTO> patches = JsonConvert.DeserializeObject<List<PortsDTO>>(responseContent);
                            return patches;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetPorts functionality");
            }
            return null;
        }

        public async Task<List<TagsDTO>> GetTags(string orgId, string baseUrl, string endPoint, string apikeyheader, string apikey)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Add(apikeyheader, apikey);
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{endPoint}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            // Deserialize the JSON response into a list of EADeviceDTO objects
                            List<TagsDTO> tags = JsonConvert.DeserializeObject<List<TagsDTO>>(responseContent);


                            // Return the list of EADeviceDTO objects
                            return tags;
                        }
                    }
                    return null;
                }
                //JSONString = JsonConvert.SerializeObject(ds.Tables[0]);
                //return Json(JSONString);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetTags functionality");
            }
            return null;
        }

        public async Task<string> UpdateTag(string orgId, string baseUrl, string endPoint, int tagId, bool action, string apikeyheader, string apikey)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Add(apikeyheader, apikey);
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{endPoint}/{orgid}-{tagId}-{action}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            // Deserialize the JSON response into a list of EADeviceDTO objects
                            string status = JsonConvert.DeserializeObject<string>(responseContent);


                            // Return the list of EADeviceDTO objects
                            return status;
                        }
                    }
                    return null;
                }
                //JSONString = JsonConvert.SerializeObject(ds.Tables[0]);
                //return Json(JSONString);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_UpdateTags functionality");
            }
            return null;
        }

        public async Task<List<BLsoftwareListDTO>> GetBLsoftwares(string orgId, string user, string baseUrl, string getEaBLSwUrl, string eaApiTokenHeader, string eaApiToken)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getEaBLSwUrl}/{user}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<BLsoftwareListDTO> blsw = JsonConvert.DeserializeObject<List<BLsoftwareListDTO>>(responseContent);
                            return blsw;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetBLSoftwares functionality");
            }
            return null;
        }

        public async Task<string> GetActiveStatus(string orgId, string user, string assetId, string baseUrl, string endPoint, string eaApiTokenHeader, string? eaApiToken)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{endPoint}/{orgId}/{user}/{assetId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (responseContent != null)
                        {
                            ActiveStatusDTO activeStatus = JsonConvert.DeserializeObject<ActiveStatusDTO>(responseContent);
                            return activeStatus.Status;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetActiveStatus functionality");
            }
            return null;
        }

        public async Task<List<AssetApprovalDTO>> GetAssetApprovals(string orgId, string baseUrl, string endPoint, string apikeyheader, string apikey)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Add(apikeyheader, apikey);
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{endPoint}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            // Deserialize the JSON response into a list of EADeviceDTO objects
                            List<AssetApprovalDTO> approvals = JsonConvert.DeserializeObject<List<AssetApprovalDTO>>(responseContent);


                            // Return the list of EADeviceDTO objects
                            return approvals;
                        }
                    }
                    return null;
                }
                //JSONString = JsonConvert.SerializeObject(ds.Tables[0]);
                //return Json(JSONString);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetAssetApprovals functionality");
            }
            return null;
        }

        public async Task<string> ApproveMonitoring(int orgId, string assetId, string isApproved, string baseUrl, string endPoint, string apikeyheader, string apikey)
        {
            int orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Add(apikeyheader, apikey);
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{endPoint}/{orgid}/{assetId}/{isApproved}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            // Deserialize the JSON response into a list of EADeviceDTO objects
                            string status = JsonConvert.DeserializeObject<string>(responseContent);


                            // Return the list of EADeviceDTO objects
                            return status;
                        }
                    }
                    return null;
                }
                //JSONString = JsonConvert.SerializeObject(ds.Tables[0]);
                //return Json(JSONString);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_ApprovedMonitoring functionality");
            }
            return null;
        }

        public async Task<List<ServicesDTO>> GetServices(string orgId, string user, string baseUrl, string getServiceDetailsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getServiceDetailsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<ServicesDTO> services = JsonConvert.DeserializeObject<List<ServicesDTO>>(responseContent);
                            return services;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetServices functionality");
            }
            return null;
        }

        public async Task<List<ProcessesDTO>> GetProcesses(string orgId, string user, string baseUrl, string getProcessDetailsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getProcessDetailsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<ProcessesDTO> processes = JsonConvert.DeserializeObject<List<ProcessesDTO>>(responseContent);
                            return processes;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetProcesses functionality");
            }
            return null;
        }

        public async Task<List<ProcessorDTO>> GetProcessors(string orgId, string user, string baseUrl, string getProcessorDetailsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getProcessorDetailsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<ProcessorDTO> processors = JsonConvert.DeserializeObject<List<ProcessorDTO>>(responseContent);
                            return processors;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetProcessors functionality");
            }
            return null;
        }

        public async Task<List<RAMDetailsDTO>> GetRAMDetails(string orgId, string user, string baseUrl, string getRAMDetailsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getRAMDetailsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<RAMDetailsDTO> rAMDetails = JsonConvert.DeserializeObject<List<RAMDetailsDTO>>(responseContent);
                            return rAMDetails;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetRAMDetails functionality");
            }
            return null;
        }

        public async Task<List<ScheduledTasksDTO>> GetScheduledTasks(string orgId, string user, string baseUrl, string getScheduledTasksUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getScheduledTasksUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<ScheduledTasksDTO> scheduledTasks = JsonConvert.DeserializeObject<List<ScheduledTasksDTO>>(responseContent);
                            return scheduledTasks;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetScheduledTasks functionality");
            }
            return null;
        }

        public async Task<List<StorageVolumesDTO>> GetStorageVolumes(string orgId, string user, string baseUrl, string getStorageVolumesUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getStorageVolumesUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<StorageVolumesDTO> storageVolumes = JsonConvert.DeserializeObject<List<StorageVolumesDTO>>(responseContent);
                            return storageVolumes;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetStorageVolumes functionality");
            }
            return null;
        }

        public async Task<List<GraphicCardsDTO>> GetGraphicCards(string orgId, string user, string baseUrl, string getGraphicCardsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getGraphicCardsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<GraphicCardsDTO> details = JsonConvert.DeserializeObject<List<GraphicCardsDTO>>(responseContent);
                            return details;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetGraphicCards functionality");
            }
            return null;
        }

        public async Task<List<NetworkAdaptersDTO>> GetNetworkAdapters(string orgId, string user, string baseUrl, string getNetworkAdaptersUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getNetworkAdaptersUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<NetworkAdaptersDTO> details = JsonConvert.DeserializeObject<List<NetworkAdaptersDTO>>(responseContent);
                            return details;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetNetworkAdapter functionality");
            }
            return null;
        }

        public async Task<List<RAIDControllersDTO>> GetRAIDControllers(string orgId, string user, string baseUrl, string getRAIDControllersUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getRAIDControllersUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<RAIDControllersDTO> details = JsonConvert.DeserializeObject<List<RAIDControllersDTO>>(responseContent);
                            return details;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetRAIDController functionality");
            }
            return null;
        }

        public async Task<List<PhysicalDrivesDTO>> GetPhysicalDrives(string orgId, string user, string baseUrl, string getPhysicalDrivesUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getPhysicalDrivesUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<PhysicalDrivesDTO> details = JsonConvert.DeserializeObject<List<PhysicalDrivesDTO>>(responseContent);
                            return details;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetPhysicalDrivers functionality");
            }
            return null;
        }

        public async Task<OtherSpecificationDTO> GetOtherSpecifications(string orgId, string user, string baseUrl, string getOtherSpecificationsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getOtherSpecificationsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<OtherSpecificationDTO> detailsList = JsonConvert.DeserializeObject<List<OtherSpecificationDTO>>(responseContent);

                            // Assuming you are expecting a list of details, you might need to handle this appropriately based on your API response.

                            // For example, if you are expecting a single object in the array, you can get the first item:
                            OtherSpecificationDTO details = detailsList.FirstOrDefault();

                            return details;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetOthersSpecifications functionality");
            }
            return null;

        }

        public async Task<List<AccountDTO>> GetAccounts(string orgId, string user, string baseUrl, string getAccountsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getAccountsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<AccountDTO> accounts = JsonConvert.DeserializeObject<List<AccountDTO>>(responseContent);
                            return accounts;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetAccounts functionality");
            }
            return null;
        }

        public async Task<List<ActivePortDTO>> GetActivePorts(string orgId, string user, string baseUrl, string getActivePortsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getActivePortsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<ActivePortDTO> activePorts = JsonConvert.DeserializeObject<List<ActivePortDTO>>(responseContent);
                            return activePorts;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetActivePorts functionality");
            }
            return null;
        }

        public async Task<List<ActiveNetworkDetailDTO>> GetActiveNetworkDetails(string orgId, string user, string baseUrl, string getActiveNetworkDetailsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getActiveNetworkDetailsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<ActiveNetworkDetailDTO> activePorts = JsonConvert.DeserializeObject<List<ActiveNetworkDetailDTO>>(responseContent);
                            return activePorts;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetActiveNetworkDetails functionality");
            }
            return null;
        }

        public async Task<List<DiskDetailDTO>> GetDiskDetails(string orgId, string user, string baseUrl, string getDiskDetailsUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getDiskDetailsUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<DiskDetailDTO> diskDetails = JsonConvert.DeserializeObject<List<DiskDetailDTO>>(responseContent);
                            return diskDetails;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_DiskDetails functionality");
            }
            return null;
        }

        public async Task<ResourceUtilDTO> GetResourceUtil(string orgId, string user, string baseUrl, string getResourceUtilUrl, string eaApiTokenHeader, string eaApiToken, string assetid)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getResourceUtilUrl}/{user}/{orgid}/{assetid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<ResourceUtilDTO> result = JsonConvert.DeserializeObject<List<ResourceUtilDTO>>(responseContent);
                            return result.FirstOrDefault();
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetResourceUtil functionality");
            }
            return null;
        }

        public async Task<List<NotificationDTO>> GetNotifications(string orgId, string user, string baseUrl, string getNotificationsUrl, string eaApiTokenHeader, string eaApiToken)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getNotificationsUrl}/{user}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();



                        if (responseContent != null)
                        {
                            List<NotificationDTO> notifications = JsonConvert.DeserializeObject<List<NotificationDTO>>(responseContent);
                            return notifications;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetNotifications functionality");
            }
            return null;
        }

        public async Task<string> IsReadNotification(string orgId, string user, string baseUrl, string IsReadNotificationUrl, string eaApiTokenHeader, string eaApiToken, int id)
        {
            string orgid = orgId;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{IsReadNotificationUrl}/{user}/{orgid}/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return "ok";
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_IsReadNotification functionality");
            }
            return null;
        }

        public async Task<string> MarkAllAsRead(string orgId, string user, string baseUrl, string MarkAllAsReadUrl, string eaApiTokenHeader, string eaApiToken)
        {
            string orgid = orgId;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{MarkAllAsReadUrl}/{user}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return "ok";
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_MarkAllAsRead functionality");
            }
            return null;
        }

        public async Task<string> DeleteNotification(string orgId, string user, string baseUrl, string DeleteNotificationUrl, string eaApiTokenHeader, string eaApiToken, int id)
        {
            string orgid = orgId;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.DeleteAsync($"{baseUrl}{DeleteNotificationUrl}/{user}/{orgid}/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return "ok";
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_DeleteNotification functionality");
            }
            return null;
        }

        public async Task<List<DeviceDTO>> GetDevices(string orgId, string user, string baseUrl, string getDevicesUrl, string eaApiTokenHeader, string eaApiToken)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getDevicesUrl}/{user}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<DeviceDTO> devices = JsonConvert.DeserializeObject<List<DeviceDTO>>(responseContent);
                            return devices;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetDevices functionality");
            }

            return null;
        }

        public async Task<bool> ApproveDevice(string id, string user, string baseUrl, string approveDeviceUrl, string eaApiTokenHeader, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{approveDeviceUrl}/{user}/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_ApproveDevices functionality");
            }
            return false;
        }

        public async Task<bool> DeleteDevice(string id, string user, string baseUrl, string deleteDeviceUrl, string eaApiTokenHeader, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.DeleteAsync($"{baseUrl}{deleteDeviceUrl}/{user}/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_DeleteDevice functionality");
            }
            return false;
        }

        // Dev: Viraj; Date: 06-04-2024
        public async Task<string> GetCurrentVersion(string DeviceID, string baseUrl, string getCurrentVersionUrl)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getCurrentVersionUrl}/{DeviceID}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return responseContent;
                    }
                    return "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetCurrentVersion functionality");
            }
            return null;
        }

        // Dev: Viraj; Date: 06-04-2024
        public async Task<string> IsInQueue(string baseUrl, string isInQueueWithDeviceIDUrl, string DeviceID)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{isInQueueWithDeviceIDUrl}/{DeviceID}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return responseContent;
                    }
                    return "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_IsInQueue functionality");
            }
            return null;
        }

        // Dev: Viraj; Date: 06-04-2024
        public async Task<string> GetLatestVersion(string baseUrl, string getLatestVersionUrl)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getLatestVersionUrl}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        return responseContent;
                    }
                    return "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetLastVersion functionality");
            }
            return null;
        }

        // Dev: Viraj; Date: 06-04-2024
        public async Task AddDeviceInPatchQueue(int DeviceID, string baseUrl, string addDeviceInPatchQueueUrl)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{addDeviceInPatchQueueUrl}/{DeviceID}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                    }
                   
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_AddDeviceInPatchQueue functionality");
            }
        }

        // Dev: Viraj; Date: 09-05-2024
        public async Task<List<DeviceDTO>> GetDevicesForUpdate(string orgId, string user, string baseUrl, string getDevicesForUpdateUrl, string eaApiTokenHeader, string eaApiToken)
        {
            string orgid = orgId;
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getDevicesForUpdateUrl}/{user}/{orgid}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<DeviceDTO> devices = JsonConvert.DeserializeObject<List<DeviceDTO>>(responseContent);
                            return devices;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetDevicesForUpdate functionality");
            }
            return null;
        }

        // Dev: Viraj; Date:24-05-2024; Consuming the GetMastersDataForEndAgentForm API
        public async Task<List<GetMasters>> GetMastersDataFromITAM(int orgId, string url, string itamAPIKeyHeader, string itamAPIKey)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                                        httpClient.DefaultRequestHeaders.Add(itamAPIKeyHeader, itamAPIKey);
                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        List<GetMasters> mastersData = JsonConvert.DeserializeObject<List<GetMasters>>(responseContent);
                        return mastersData;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetMastersDataFromITAM functionality");
            }
            return null;
        }

        // Dev: Viraj; Date:24-05-2024; Consuming the AddWorkstation API
        public async Task<string> AddWorkstation(AddWorkstationDTO workstation, string url, string itamAPIKeyHeader, string itamAPIKey)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                                        httpClient.DefaultRequestHeaders.Add(itamAPIKeyHeader, itamAPIKey);
                    

                    HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{url}", workstation);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return responseContent;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_AddWorkStation functionality");
            }
            return null;
        }

        // Dev: Viraj; Date:24-05-2024; Consuming the GetMastersDataForEndAgentForm API
        public async Task<BIOSDetailsDTO> GetBIOS(string ID, string token, string url)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{ID}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        BIOSDetailsDTO mastersData = JsonConvert.DeserializeObject<BIOSDetailsDTO>(responseContent);
                        return mastersData;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetBIOS functionality");
            }
            return null;
        }
        
        // Dev: Viraj; Date:24-05-2024; Check if BIOS_SN and TrackerID already exists
        public async Task<string> CheckBIOS(ITAMBIOSDetailsDTO biosDetails, string orgID, string url, string itamAPIKeyHeader, string itamAPIKey)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                                        httpClient.DefaultRequestHeaders.Add(itamAPIKeyHeader, itamAPIKey);
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{url}/{orgID}", biosDetails);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return responseContent;
                    }                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_CheckBIOS functionality");
            }
            return null;
        }
		
		// Dev: Viraj; Date:24-05-2024; Get All AssetIDs for unique AssetID validation
        public async Task<List<string>> GetAssetIDs(int orgId, string url, string itamAPIKeyHeader, string itamAPIKey)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                                        httpClient.DefaultRequestHeaders.Add(itamAPIKeyHeader, itamAPIKey);
                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        List<string> mastersData = JsonConvert.DeserializeObject<List<string>>(responseContent);
                        return mastersData;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetAssetId functionality");
            }
            return null;
        }

        public async Task<bool> UploadFile(FileUploadDTO file, string token, string url)
        {
            try
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using (HttpClient httpClient = new HttpClient(httpClientHandler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Serialize FileUploadDTO to JSON
                    var jsonContent = JsonConvert.SerializeObject(file);

                    // Prepare the request content
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Response Content: {responseContent}");
                        return true;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error Response: {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_UploadFile functionality");
            }
            return false;
        }

        public async Task<List<FileUploadDTO>> LoadPatches(string orgId, string url, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<FileUploadDTO> devices = JsonConvert.DeserializeObject<List<FileUploadDTO>>(responseContent);
                            return devices;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_LoadPatch functionality");
            }
            return null;
        }

        public async Task<List<EligiblePatches>> GetEligiblePatches(string orgId, string url, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<EligiblePatches> patches = JsonConvert.DeserializeObject<List<EligiblePatches>>(responseContent);
                            return patches;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetEligiblePatch functionality");
            }
            return null;
        }

        public async Task<OSPatches> GetAvailableOrInstalledPatches(string orgId, string url,string kbtype, string assetId, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{orgId}/{assetId}/{kbtype}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            OSPatches patches = JsonConvert.DeserializeObject<OSPatches>(responseContent);
                            return patches;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetEligiblePatch functionality");
            }
            return null;
        }

        // Dev : Srikanth Erukulla - 30-01-2026
        public async Task<T> GetAsync<T>(string url, string bearerToken)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        (sender, cert, chain, sslPolicyErrors) => true
                };

                using HttpClient httpClient = new HttpClient(handler);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return default;

                string responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent))
                    return default;

                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling GET API");
                return default;
            }
        }

        public async Task<bool> AddinUpdatePatchQueue(List<UpdatePatchQueueTempDTO> patches, string url, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{url}", patches);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_AddInUpdatePatchQueue functionality");
            }
            return false;
        }

        public async Task<List<UpdatePatchQueueDTO>> GetPatchLogs(string orgId, string url, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<UpdatePatchQueueDTO> patches = JsonConvert.DeserializeObject<List<UpdatePatchQueueDTO>>(responseContent);
                            return patches;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetPatchLog functionality");
            }
            return null;
        }

        public async Task<PatchResponseDto> GetPatchesbyfamily(string families, string url, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}?familyIds={families}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            PatchResponseDto patches = JsonConvert.DeserializeObject<PatchResponseDto>(responseContent);
                            return patches;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetPatchLog functionality");
            }
            return null;
        }

        public async Task<APIResponse> AddDesktopPolicy(string url, string bearerToken, DesktopPoliciesDTO desktopPolicies)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", bearerToken);

                    string jsonPayload = JsonConvert.SerializeObject(desktopPolicies);
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                            return apiResponse;
                        }
                    }

                    return new APIResponse
                    {
                        Status = false,
                        Message = $"Request failed with status code: {response.StatusCode}",
                        StatusCode = ((int)response.StatusCode).ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error occurred in SaveAsync");
                return new APIResponse
                {
                    Status = false,
                    Message = ex.Message,
                    StatusCode = "500"
                };
            }
        }

        public async Task<List<WhitelistAppsVM>?> GetDesktopPolicies(string url, string eaApiToken,string orgId)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{url}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<WhitelistAppsVM>? patches = JsonConvert.DeserializeObject<List<WhitelistAppsVM>>(responseContent);
                            return patches;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetPatchLog functionality");
            }
            return null;
        }

        public async Task<APIResponse> CreateGroup(string url, string eaApiToken, AssignGroupRequestVM model)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            using(var httpClient = new HttpClient(handler))
            {
                httpClient.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",eaApiToken);
                string jsonPayload = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                        return apiResponse;
                    }
                }

                return new APIResponse
                {
                    Status = false,
                    Message = $"Request failed with status code: {response.StatusCode}",
                    StatusCode = ((int)response.StatusCode).ToString()
                };
            }
        }
        public async Task<APIResponse> UpdateGroup(string url, string eaApiToken, AssignGroupByGroupIdVM model)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            using (var httpClient = new HttpClient(handler))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                string jsonPayload = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                        return apiResponse;
                    }
                }

                return new APIResponse
                {
                    Status = false,
                    Message = $"Request failed with status code: {response.StatusCode}",
                    StatusCode = ((int)response.StatusCode).ToString()
                };
            }
        }
        public async Task<APIResponse> DeleteDeviceFromEndPointGroup(DeleteDeviceEndPointGroupVM model, string baseUrl, string deletegroup, string eaApiToken)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            using (var httpClient = new HttpClient(handler))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                string jsonPayload = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync($"{baseUrl}{deletegroup}", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                        return apiResponse;
                    }
                }

                return new APIResponse
                {
                    Status = false,
                    Message = $"Request failed with status code: {response.StatusCode}",
                    StatusCode = ((int)response.StatusCode).ToString()
                };
            }
        }
        public async Task<APIResponse> SaveEndPointGroupingScheduledTime(GroupPatchScheduleTimeVM model, string baseUrl, string saveGroupPatchScheduledTime, string eaApiToken)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            using (var httpClient = new HttpClient(handler))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                string jsonPayload = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync($"{baseUrl}{saveGroupPatchScheduledTime}", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                        return apiResponse;
                    }
                }

                return new APIResponse
                {
                    Status = false,
                    Message = $"Request failed with status code: {response.StatusCode}",
                    StatusCode = ((int)response.StatusCode).ToString()
                };
            }
        }
        public async Task<List<DeviceNameForEndpointGroupVM>> GetDevicesByWithoutEndpointGroup(string orgId, string baseUrl, string getgroup, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getgroup}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<DeviceNameForEndpointGroupVM> devices = JsonConvert.DeserializeObject<List<DeviceNameForEndpointGroupVM>>(responseContent);
                            return devices;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetDevices functionality");
            }

            return null;
        }

        public async Task<List<EndpointGroupVM>> GetEndpointGroupNames(string orgId, string baseUrl, string getgroupnames, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getgroupnames}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<EndpointGroupVM> devices = JsonConvert.DeserializeObject<List<EndpointGroupVM>>(responseContent);
                            return devices;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetDevices functionality");
            }

            return null;
        }
        public async Task<List<DeviceNameForEndpointGroupVM>> GetDevicesByGroupId(string orgId,string groupId, string baseUrl, string getdevicebygroupid, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}{getdevicebygroupid}/{orgId}/{groupId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            List<DeviceNameForEndpointGroupVM> devices = JsonConvert.DeserializeObject<List<DeviceNameForEndpointGroupVM>>(responseContent);
                            return devices;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetDevices functionality");
            }

            return null;
        }

        public async Task<KPISViewModel> GetPatchKPIs(string orgId, string baseUrl, string eaApiToken)
        {
            try
            {
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //Donot Use in Production (after Implementing SSL)
                HttpClientHandler handler = new HttpClientHandler();

                // Disable SSL/TLS validation by accepting all certificates
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eaApiToken);
                    //httpClient.SetBearerToken(eaApiToken);

                    HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}/{orgId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (responseContent != null)
                        {
                            KPISViewModel devices = JsonConvert.DeserializeObject<KPISViewModel>(responseContent);
                            return devices;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "error occured in EndAgentAPI_GetDevices functionality");
            }

            return null;
        }
    }
}
