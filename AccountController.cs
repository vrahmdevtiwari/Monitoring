using Identity.Common.Implementation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Monitoring.CommonFunction;
using Monitoring.Data;
using Monitoring.ViewModel;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Monitoring.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITokenExtraction _tokenExtraction;
        private readonly TokenViewModel tokenProperty = new();
        private readonly SSODBContext _dbContext;
        public AccountController(ITokenExtraction tokenExtraction, IConfiguration configuration, ILogger<AccountController> logger, SSODBContext dbContext)
        {
            _tokenExtraction = tokenExtraction;
            tokenProperty = _tokenExtraction.ExtractToken().GetAwaiter().GetResult();
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Signout()
        {
            // Retrieve ID token for logout request
            var idToken = await HttpContext.GetTokenAsync("id_token");

            //Construct the IdentityServer logout URL
            var logoutUri = $"{_configuration["SSOServer:Login"]}/connect/endsession";

            if (!string.IsNullOrEmpty(idToken))
            {
                logoutUri += $"?id_token_hint={idToken}";
            }

            // Redirect to IdentityServer for global logout
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = logoutUri
            }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> LogoutCallback()
        {
            // Perform any cleanup tasks here, such as clearing local session or authentication tokens

            // Redirect to a logged-out page or home page
            return RedirectToAction("Logout", "Account");
        }

        public IActionResult AccessDenied()
        {
            ViewBag.FirstName = tokenProperty.FirstName;
            ViewBag.LastName = tokenProperty.LastName;
            ViewBag.FullName = tokenProperty.FullName;
            ViewBag.Email = tokenProperty.Email;
            ViewBag.Role = tokenProperty.Role;
            ViewBag.RoleIds = tokenProperty.Role;
            ViewBag.OrgId = tokenProperty.OrgId;

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult FrontChannelLogout(string logout_id)
        {
            // Sign out locally and redirect to IdentityServer's end session
            var logoutUri = $"{_configuration["SSOServer:Login"]}/connect/endsession/callback?logout_id={logout_id}";
            return SignOut(new AuthenticationProperties { RedirectUri = logoutUri },
                           CookieAuthenticationDefaults.AuthenticationScheme,
                           OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> BackChannelLogout([FromForm] string sid)
        {
            var currentSid = User.FindFirst("sid")?.Value;
            if (currentSid == sid)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return Ok();
        }


        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,User,L1User,L2User,EndUser")]
        public async Task<IActionResult> ManageProfile()
        {
            ProfileViewModel profileViewModel = new ProfileViewModel();
            ViewData["User"] = tokenProperty.FirstName[0].ToString() + tokenProperty.LastName[0].ToString();
            ViewData["Role"] = tokenProperty.Role;

            try
            {
                var _getProfileData = new AspNetUser();
                if (tokenProperty.Role == "SuperAdmin")
                {
                    _getProfileData = await _dbContext.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == tokenProperty.Name);
                }
                else
                {
                    _getProfileData = await _dbContext.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == tokenProperty.Name && x.Organization == tokenProperty.OrganizationId);
                }
                //check if _getProfileData is null or not
                if (_getProfileData != null)
                {
                    profileViewModel = new ProfileViewModel()
                    {
                        Email = _getProfileData.Email!,
                        FirstName = _getProfileData.FirstName,
                        LastName = _getProfileData.LastName,
                        Address = _getProfileData.Address,
                        MobileNumber = _getProfileData.MobileNumber,
                        Designation = _getProfileData.Designation
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error occured in token extraction");
                return RedirectToAction("ErrorPage", new { errorInfo = "error occured in token extraction" });
            }

            return PartialView("ManageProfile", profileViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,User,L1User,L2User,EndUser")]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> ManageProfile(ProfileViewModel profileViewModel)
        {

            if (ModelState.IsValid)
            {
                var _user = new AspNetUser();
                if (tokenProperty.Role == "SuperAdmin")
                {
                    _user = await _dbContext.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == tokenProperty.Name);
                }
                else
                {
                    _user = await _dbContext.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == tokenProperty.Name && x.Organization == tokenProperty.OrganizationId);
                }
                if (_user != null)
                {
                    _user.Email = profileViewModel.Email;
                    _user.FirstName = profileViewModel.FirstName;
                    _user.LastName = profileViewModel.LastName;
                    _user.Address = profileViewModel.Address;
                    _user.MobileNumber = profileViewModel.MobileNumber;
                    _user.PhoneNumber = profileViewModel.MobileNumber;
                    int i = await _dbContext.SaveChangesAsync();
                    if (i > 0)
                        ModelState.AddModelError("Success", "Save Changes Successfully");
                    else
                        ModelState.AddModelError("Fail", "no data is updated");
                }
                else
                    ModelState.AddModelError("Fail", "Something Wrong, Please try after some time");
            }
            else
            {
                //Validation Error Message to show in Partial View (ModelState validation error not showing)
                if (string.IsNullOrEmpty(profileViewModel.FirstName))
                    ModelState.AddModelError("FirstName", "Please Enter FirstName");
                if (string.IsNullOrEmpty(profileViewModel.LastName))
                    ModelState.AddModelError("LastName", "Please Enter LastName");
                if (string.IsNullOrEmpty(profileViewModel.MobileNumber))
                    ModelState.AddModelError("MobileNumber", "Please Enter Mobile Number");
                else
                {
                    // Regular expression to validate exactly 10 digits
                    string pattern = @"^\d{10}$";
                    // Check if the mobile number matches the pattern
                    if (!Regex.IsMatch(profileViewModel.MobileNumber, pattern))
                    {
                        ModelState.AddModelError("MobileNumber", "Mobile Number should be 10 digits");
                    }
                }

                if (string.IsNullOrEmpty(profileViewModel.Designation))
                    ModelState.AddModelError("Designation", "Please Enter Designation");
                if (string.IsNullOrEmpty(profileViewModel.Address))
                    ModelState.AddModelError("Address", "Please Enter Address");
                else
                {
                    // Regular expression to validate address with special characters
                    string pattern = @"^[a-zA-Z0-9.:;,' ?/\\&*()#@_ \s-]+$";
                    if (!Regex.IsMatch(profileViewModel.Address, pattern))
                        ModelState.AddModelError("Address", "Address contain only some special characters(_,-,.,:,\\,/,&,#,*,@)");

                }
            }

            return PartialView("ManageProfile", profileViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Register()
        {
            RegistrationViewModel register = new RegistrationViewModel();

            var _rolesList = _dbContext.AspNetRoles.Where(x => x.Name != "SuperAdmin").Select(_ => new SelectListItem()
            {
                Text = _.Name,
                Value = _.Id
            }).ToList();

            if (_rolesList != null)
                ViewBag.RolesList = _rolesList;
            else
                ViewBag.RolesList = new List<SelectListItem>();

            return PartialView("Register", register);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Register([FromForm] RegistrationViewModel registrationViewModel)
        {
            var _rolesList = _dbContext.AspNetRoles.Where(x => x.Name != "SuperAdmin").Select(_ => new SelectListItem()
            {
                Text = _.Name,
                Value = _.Id
            }).ToList();

            if (_rolesList != null)
                ViewBag.RolesList = _rolesList;
            else
                ViewBag.RolesList = new List<SelectListItem>();

            bool isExist = false;
            bool isUserIdVerified = _dbContext.AspNetUsers.Any(x => x.UserName == registrationViewModel.UserId && x.Organization == tokenProperty.OrganizationId);
            
            if (isUserIdVerified)
            {
                ModelState.AddModelError("UserId", "UserId already exist");
                isExist = true;
            }
            
            if (isExist)
                return PartialView("Register", registrationViewModel);

            if (ModelState.IsValid)
            {                
                
                    AspNetUser _createUser = new AspNetUser();

                    //Convert RegistrationViewModel to AspnetUser
                    string _uniqueId = await GetUniqueId();
                    if (string.IsNullOrEmpty(_uniqueId))
                    {
                        ModelState.AddModelError("Fail", "something wrong in id generating");
                        return PartialView("Register", registrationViewModel);
                    }

                    _createUser.Id = _uniqueId;
                    _createUser.UserName = registrationViewModel.UserId;
                    _createUser.NormalizedUserName = registrationViewModel.UserId.ToUpper();
                    _createUser.Email = registrationViewModel.Email;
                    _createUser.NormalizedEmail = registrationViewModel.Email.ToUpper();
                    _createUser.Organization = tokenProperty.OrganizationId;
                    _createUser.FirstName = registrationViewModel.FirstName;
                    _createUser.LastName = registrationViewModel.LastName;
                    _createUser.Address = registrationViewModel.Address;
                    _createUser.Designation = registrationViewModel.Designation;
                    _createUser.MobileNumber = registrationViewModel.MobileNumber;
                    _createUser.PhoneNumberConfirmed = false;
                    _createUser.PhoneNumber = registrationViewModel.MobileNumber;
                    _createUser.EmailConfirmed = false;
                    _createUser.TwoFactorEnabled = false;
                    _createUser.LockoutEnabled = true;
                    _createUser.AccessFailedCount = 0;
                    _createUser.SecurityStamp = await GetUniqueId();
                    _createUser.ConcurrencyStamp = await GetUniqueId();

                    // Create an instance of PasswordHasher
                    var passwordHasher = new PasswordHasher<AspNetUser>();
                    _createUser.PasswordHash = passwordHasher.HashPassword(_createUser, registrationViewModel.Password);
                    bool isRegistered = await _dbContext.AspNetUsers.AnyAsync(x => x.UserName == registrationViewModel.Email && x.Email == registrationViewModel.Email);
                    if (!isRegistered && !string.IsNullOrEmpty(_createUser.PasswordHash))
                    {
                        try
                        {
                            _dbContext.AspNetUsers.Add(_createUser);
                            int i = await _dbContext.SaveChangesAsync();
                            if (i > 0)
                            {
                                string query = $"Insert into AspNetUserRoles(UserId, RoleId) values ('{_createUser.Id}','{registrationViewModel.Role}')";
                                int _i = await _dbContext.Database.ExecuteSqlRawAsync(query);
                                if (_i > 0)
                                    ModelState.AddModelError("Success", "Employee Account Registered");
                            }
                            else
                            {
                                ModelState.AddModelError("Fail", "Employee Account Register Failed");
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.ToString();
                            string msssg = msg;
                            ModelState.AddModelError("Fail", "Something Wrong");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Fail", "Something Wrong");
                    }
                

                //ModelState.AddModelError("Success", "Employee Account Registered");
            }
            else
            {
                //Validation Error Message to show in Partial View (ModelState validation error not showing)
                if (string.IsNullOrEmpty(registrationViewModel.UserId))
                    ModelState.AddModelError("UserId", "Please Enter UserId");
                if (string.IsNullOrEmpty(registrationViewModel.FirstName))
                    ModelState.AddModelError("FirstName", "Please Enter FirstName");
                if (string.IsNullOrEmpty(registrationViewModel.LastName))
                    ModelState.AddModelError("LastName", "Please Enter LastName");
                if (string.IsNullOrEmpty(registrationViewModel.MobileNumber))
                    ModelState.AddModelError("MobileNumber", "Please Enter Mobile Number");
                else
                {
                    // Regular expression to validate exactly 10 digits
                    string pattern = @"^\d{10}$";
                    // Check if the mobile number matches the pattern
                    if (!Regex.IsMatch(registrationViewModel.MobileNumber, pattern))
                    {
                        ModelState.AddModelError("MobileNumber", "Mobile Number should be 10 digits");
                    }
                }

                if (string.IsNullOrEmpty(registrationViewModel.Designation))
                    ModelState.AddModelError("Designation", "Please Enter Designation");
                if (string.IsNullOrEmpty(registrationViewModel.Address))
                    ModelState.AddModelError("Address", "Please Enter Address");
                else
                {
                    // Regular expression to validate address with special characters
                    string pattern = @"^[a-zA-Z0-9.:;,' ?/\\&*()#@_ \s-]+$";
                    if (!Regex.IsMatch(registrationViewModel.Address, pattern))
                        ModelState.AddModelError("Address", "Address contain only some special characters(_,-,.,:,\\,/,&,#,*,@)");

                }
                if (!string.IsNullOrEmpty(registrationViewModel.Email))
                {
                    // Regular expression for validating email addresses
                    string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                    bool isValid = Regex.IsMatch(registrationViewModel.Email, pattern, RegexOptions.IgnoreCase);
                    if (!isValid)
                        ModelState.AddModelError("Email", "Please Enter Valid Email Address");
                }

                if (string.IsNullOrEmpty(registrationViewModel.Password))
                    ModelState.AddModelError("Password", "Please Enter Password");
                if (string.IsNullOrEmpty(registrationViewModel.ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "Please Enter ConfirmPassword");

                if (registrationViewModel.Password != registrationViewModel.ConfirmPassword)
                {
                    ModelState.AddModelError("Password", "Password Mismatch");
                    ModelState.AddModelError("ConfirmPassword", "Password Mismatch");
                }

                if (string.IsNullOrEmpty(registrationViewModel.Role))
                    ModelState.AddModelError("Role", "Please Select User Role");

            }
            
            return PartialView("Register", registrationViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,User,,L1User,L2User,EndUser")]
        public IActionResult ChangePassword()
        {
            ChangePasswordViewModel changePasswordViewModel = new ChangePasswordViewModel();
            return PartialView("ChangePassword", changePasswordViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Manager,User,L1User,L2User,EndUser")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel chpwd)
        {
            ModelState.Remove("System");
            if (ModelState.IsValid)
            {
                // Srikanth - 20-11-2024
                // this added to validate password
                bool isValidationFailed = false;
                if (chpwd.OldPassword == chpwd.NewPassword)
                {
                    ModelState.AddModelError("Fail", "Current Password and New Password should not be same");
                    isValidationFailed = true;
                }

                if (isValidationFailed)
                {
                    return PartialView("ChangePassword", chpwd);
                }
                // validating old password with data in db table
                // if invalid it will throw - Invalid Current Password
                var _user = await _dbContext.AspNetUsers.FirstOrDefaultAsync(x => x.UserName == tokenProperty.Name && x.Organization == tokenProperty.OrganizationId);
                string oldPwd = _user.PasswordHash;
                if (_user != null)
                {
                    // Create an instance of PasswordHasher
                    var passwordHasher = new PasswordHasher<AspNetUser>();
                    var _pwdResult = passwordHasher.VerifyHashedPassword(_user, _user.PasswordHash, chpwd.OldPassword);
                    if (_pwdResult != PasswordVerificationResult.Success)
                    {
                        ModelState.AddModelError("Fail", "Invalid Current Password");
                        return PartialView("ChangePassword", chpwd);
                    }

                    _user.PasswordHash = passwordHasher.HashPassword(_user, chpwd.NewPassword);
                    int i = await _dbContext.SaveChangesAsync();
                    if (i > 0)
                    {
                        var history = new PasswordHistory();
                        if (chpwd.System == null)
                        {
                            // ✅ Step 2: Capture device/browser info
                            string userAgent = Request.Headers["User-Agent"].ToString();
                            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                            // Check if forwarded header exists (proxy / load balancer case)
                            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                            {
                                ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
                            }


                            // Optional: parse user-agent (basic split, or use UAParser NuGet for advanced parsing)
                            string browserName = "Unknown";
                            string browserVersion = "Unknown";
                            string deviceType = "Desktop"; // default, you could improve this with user-agent parsing
                            string operatingSystem = string.Empty;
                            if (deviceType.Equals("Desktop", StringComparison.OrdinalIgnoreCase) || deviceType.Equals("Laptop", StringComparison.OrdinalIgnoreCase))
                            {
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                    operatingSystem = "Windows";
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                    operatingSystem = "Linux";
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                                    operatingSystem = "MacOS";
                                else
                                    operatingSystem = "Unknown";
                            }

                            if (!string.IsNullOrEmpty(userAgent))
                            {
                                if (userAgent.Contains("Mobile"))
                                    deviceType = "Mobile";

                                if (userAgent.Contains("Chrome"))
                                {
                                    browserName = "Chrome";
                                    // crude version extraction
                                    var match = System.Text.RegularExpressions.Regex.Match(userAgent, @"Chrome\/([\d\.]+)");
                                    if (match.Success) browserVersion = match.Groups[1].Value;
                                }
                                else if (userAgent.Contains("Firefox"))
                                {
                                    browserName = "Firefox";
                                    var match = System.Text.RegularExpressions.Regex.Match(userAgent, @"Firefox\/([\d\.]+)");
                                    if (match.Success) browserVersion = match.Groups[1].Value;
                                }
                                else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
                                {
                                    browserName = "Safari";
                                    var match = System.Text.RegularExpressions.Regex.Match(userAgent, @"Version\/([\d\.]+)");
                                    if (match.Success) browserVersion = match.Groups[1].Value;
                                }
                            }

                            history.UserID = _user.Id;
                            history.PasswordHash = _user.PasswordHash;
                            history.DateUpdated = DateTime.UtcNow;
                            history.DeviceType = deviceType;
                            history.OperatingSystem = operatingSystem;
                            history.BrowserName = browserName;
                            history.BrowserVersion = browserVersion;
                            history.IPAddress = ipAddress;
                            history.Location = null;
                            history.Email = _user.Email;
                            history.Applications = "Monitoring";

                        }
                        else
                        {
                            history.UserID = _user.Id;
                            history.PasswordHash = _user.PasswordHash;
                            history.DateUpdated = DateTime.UtcNow;
                            history.DeviceType = chpwd.System.DeviceType;
                            history.OsManufacturer = chpwd.System.OsManufacturer;
                            history.OsName = chpwd.System.OsName;
                            history.OsVersion = chpwd.System.OsVersion;
                            history.OperatingSystem = chpwd.System.OsName;
                            history.BrowserName = chpwd.System.BrowserName;
                            history.BrowserVersion = chpwd.System.BrowserVersion;
                            history.IPAddress = chpwd.System.IP;
                            history.Latitude = chpwd.System.Latitude;
                            history.Longitude = chpwd.System.Longitude;
                            history.Org = chpwd.System.Org;
                            history.RegionName = chpwd.System.region;
                            history.City = chpwd.System.City;
                            history.Country = chpwd.System.Country;
                            history.Platform = chpwd.System.Platform;
                            history.ScreenResolution = chpwd.System.ScreenResolution;
                            history.UserAgent = chpwd.System.UserAgent;
                            history.Language = chpwd.System.Language;
                            history.Email = _user.Email;
                            history.Applications = "Monitoring";
                        }

                        _dbContext.PasswordHistory.Add(history);
                        int _i = await _dbContext.SaveChangesAsync();
                        if (_i > 0)
                            _logger.LogInformation("passwordhistory success");
                        else
                            _logger.LogInformation("passwordhistory failed");

                        bool isMailSent = SendEmail(_user.Email, "Change Password");
                        if (isMailSent)
                            ModelState.AddModelError("Success", "Change Password Successfully...");

                        //Signout call
                        // Retrieve ID token for logout request
                        var idToken = await HttpContext.GetTokenAsync("id_token");

                        //Construct the IdentityServer logout URL
                        var logoutUri = $"{_configuration["SSOServer:Login"]}/connect/endsession";

                        if (!string.IsNullOrEmpty(idToken))
                        {
                            logoutUri += $"?id_token_hint={idToken}";
                        }

                        // Redirect to IdentityServer for global logout
                        return SignOut(new AuthenticationProperties
                        {
                            RedirectUri = logoutUri
                        }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

                    }
                    else
                    {
                        ModelState.AddModelError("Fail", "Change Password Failed");
                    }
                }
                else
                {
                    ModelState.AddModelError("Fail", "please try after some time");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(chpwd.OldPassword))
                    ModelState.AddModelError("OldPassword", "Please Enter Current Password");
                if (string.IsNullOrEmpty(chpwd.NewPassword))
                    ModelState.AddModelError("NewPassword", "Please Enter New Password");
                if (string.IsNullOrEmpty(chpwd.ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "Please Enter ConfirmPassword");
                if (chpwd.NewPassword != chpwd.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Password Mismatch");
                    ModelState.AddModelError("NewPassword", "Password Mismatch");
                }
            }

            return PartialView("ChangePassword", chpwd);
        }

        // it will return only unique id
        // first generated guid value is already exist in datatable table again it round-trip and generate new guid value 
        private async Task<string> GetUniqueId()
        {
            int i = 0;
            while (true)
            {
                string _id = Guid.NewGuid().ToString();
                // check that guid value is already exit in database aspnetuser table or not
                bool isUniqueId = await _dbContext.AspNetUsers.AnyAsync(x => x.Id == _id);
                if (!isUniqueId)
                    return _id;

                // this condition is used to prevent the loop if the generated guid value is already exist in datatable table 
                if (i == 5)
                    return string.Empty;

                i++;

            }
        }

        private bool SendEmail(string userEmail, string subject)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("marvenmailtest@gmail.com", "Marven Data System");
            mailMessage.To.Add(new MailAddress(userEmail));

            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;

            // Extract username from email
            string username = userEmail.Split('@')[0];

            // Customize the email body
            string body = $@"
        <html>
            <body>
                <p>Hi {username},</p>
                <p>We noticed that the password for your Insite account was recently changed.</p>
                <p>If you made this change, no further action is required.</p>
                <p>If you did not authorize this change, please contact the system administrator immediately to secure your account.</p>
                <br/><br/><br/>
                <p>Thank you,</p>
                <p>Insite Support Team</p>
            </body>
        </html>";

            string fromMail = "marvenmailtest@gmail.com";
            string fromPassword = "tdnmrgddsfwgminr";

            mailMessage.Body = body;

            //SmtpClient client = new SmtpClient();
            //client.Credentials = new System.Net.NetworkCredential(fromMail,fromPassword);
            //client.Host = "smtp.gmail.com";
            //client.Port = 587;
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // log exception
            }
            return false;
        }

    }
}
