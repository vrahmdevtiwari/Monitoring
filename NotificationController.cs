using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitoring.APIResponses;
using Monitoring.Models.DTO;
using NuGet.ContentModel;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Monitoring.CommonFunction;
using Monitoring.ViewModel;

namespace Monitoring.Controllers
{
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly IConfiguration _configuration;
        public readonly string getNotificationsUrl;
        private EndagentAPI eaAPI;
        private readonly string baseUrl;
        private readonly string eaApiKeyHeader;
        private readonly string IsReadNotificationUrl;
        private readonly string MarkAllAsReadUrl;
        private readonly string DeleteNotificationUrl;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel _tokenProperty = new();

        public NotificationController(ILogger<NotificationController> logger, IConfiguration configuration, EndagentAPI _eaAPI, ITokenExtraction tokenExtraction)
        {
            _logger = logger;
            _configuration = configuration;
            getNotificationsUrl = _configuration.GetValue<string>("ApiEndPoints:getNotificationsUrl");
            eaApiKeyHeader = configuration.GetValue<string>("ApiEndPoints:eaApiKeyHeader");
            baseUrl = _configuration.GetValue<string>("ApiEndPoints:baseUrl");
            IsReadNotificationUrl = _configuration.GetValue<string>("ApiEndPoints:IsReadNotificationUrl");
            MarkAllAsReadUrl = _configuration.GetValue<string>("ApiEndPoints:MarkAllAsReadUrl");
            DeleteNotificationUrl = _configuration.GetValue<string>("ApiEndPoints:DeleteNotificationUrl");
            eaAPI = _eaAPI;
            _tokenExtraction = tokenExtraction;
            _tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                if (_tokenProperty.Role != "Admin")
                {
                    return View("AccessDenied");
                }
                
                List<NotificationDTO> notifications = await eaAPI.GetNotifications(_tokenProperty.OrgId, _tokenProperty.Name, baseUrl, getNotificationsUrl, eaApiKeyHeader, _tokenProperty.Token);
                if (notifications !=null) 
                {
                    // Sort notifications by CreatedAt descending
                    notifications = notifications.OrderByDescending(n => n.CreatedAt).ToList();
                    foreach (var notification in notifications)
                    {
                        string body = notification.Body;

                        // Define a regular expression pattern to match text within quotes, including newline characters
                        string pattern = "\"(.*?)\"";
                        // Use Regex.Matches to find all matches of the pattern in the body
                        MatchCollection matches = Regex.Matches(body, pattern);

                        // Loop through each match and replace the matched text with the same text wrapped in <strong> tags
                        foreach (Match match in matches)
                        {
                            string matchText = match.Groups[1].Value;
                            body = body.Replace(match.Value, $"<span class='notification-page-content-important'>{matchText}</span>");
                        }

                        // Replace line breaks and tabs as before
                        body = body.Replace("\n\r", "<br>").Replace("\t", "&emsp;");
                        notification.Body = body;

                    }
                }
                else
                {
                    notifications = new List<NotificationDTO>();
                }

                return View(notifications);
            }
            catch (Exception ex) 
            {
                _logger.LogInformation($"Error at Notications/Index: {ex}");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                string? role = HttpContext.User.FindFirst("role")?.Value;
                if (!role.Contains("Admin"))
                {
                    return Unauthorized(); // Return appropriate status code if user is not authorized
                }

                string? token = await HttpContext.GetTokenAsync("access_token");
                string user = HttpContext.User.FindFirst("name")?.Value;
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;

                // Get all notifications from the API
                List<NotificationDTO> notifications = await eaAPI.GetNotifications(orgId, user, baseUrl, getNotificationsUrl, eaApiKeyHeader, token);

                // check if notification is null or not, if null it will give empty notification
                if (notifications == null)
                {
                    // Handle null notifications case
                    _logger.LogInformation("Notifications list is null.");
                    return Json(new List<NotificationDTO>()); // Return an empty list or appropriate response
                }

                notifications = notifications.Where(a => a.IsRead == false).ToList(); //115 line
                // Sort notifications by CreatedAt in descending order
                notifications = notifications.OrderByDescending(n => n.CreatedAt).ToList();

                // If there are more than 99 notifications, take only the latest 20
                if (notifications.Count > 99)
                {
                    notifications = notifications.Take(99).ToList();
                }

                return Json(notifications);
            }
            catch (Exception ex)
            {
                string user = HttpContext.User.FindFirst("name")?.Value;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return StatusCode(500, "Error fetching notifications");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsReadNotification(int id)
        {
            try
            {
                string role = HttpContext.User.FindFirst("role")?.Value;
                if (!role.Contains("Admin"))
                {
                    return Unauthorized(); // Return appropriate status code if user is not authorized
                }

                string user = HttpContext.User.FindFirst("name")?.Value;
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
                string? token = await HttpContext.GetTokenAsync("access_token");

                // Get all notifications from the API
                string response = await eaAPI.IsReadNotification(orgId, user, baseUrl, IsReadNotificationUrl, eaApiKeyHeader, token, id);
                return Ok();
            }
            catch (Exception ex)
            {
                string user = HttpContext.User.FindFirst("name")?.Value;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return StatusCode(500, "Error fetching notifications");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                string role = HttpContext.User.FindFirst("role")?.Value;
                if (!role.Contains("Admin"))
                {
                    return Unauthorized(); // Return appropriate status code if user is not authorized
                }

                string user = HttpContext.User.FindFirst("name")?.Value;
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
                string? token = await HttpContext.GetTokenAsync("access_token");

                // Get all notifications from the API
                string response = await eaAPI.MarkAllAsRead(orgId, user, baseUrl, MarkAllAsReadUrl, eaApiKeyHeader, token);
                return Ok();
            }
            catch (Exception ex)
            {
                string user = HttpContext.User.FindFirst("name")?.Value;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return StatusCode(500, "Error fetching notifications");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                string role = HttpContext.User.FindFirst("role")?.Value;
                if (!role.Contains("Admin"))
                {
                    return Unauthorized(); // Return appropriate status code if user is not authorized
                }

                string user = HttpContext.User.FindFirst("name")?.Value;
                string orgId = HttpContext.User.FindFirst("OrgId")?.Value;
                string? token = await HttpContext.GetTokenAsync("access_token");

                // Get all notifications from the API
                string response = await eaAPI.DeleteNotification(orgId, user, baseUrl, DeleteNotificationUrl, eaApiKeyHeader, token, id);
                return Ok();
            }
            catch (Exception ex)
            {
                string user = HttpContext.User.FindFirst("name")?.Value;
                _logger.LogInformation($"User: {user}\nException: {ex}");
                return StatusCode(500, "Error fetching notifications");
            }
        }

    }
}
