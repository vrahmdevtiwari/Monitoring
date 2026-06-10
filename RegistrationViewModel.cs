using System.ComponentModel.DataAnnotations;

namespace Monitoring.ViewModel
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Please Enter UserId")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Please Enter FirstName")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Please Enter LastName")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Please Enter Email Address")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please Enter MobileNumber")]
        [Phone(ErrorMessage = "Please Enter Mobile Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile Number should be 10 digits")]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = "Please Enter Designation")]
        public string Designation { get; set; }
        [Required(ErrorMessage = "Please Enter Address")]
        [RegularExpression(@"^[a-zA-Z0-9.:;,' ?/\\&*()#@_ \s-]+$", ErrorMessage = "Address contain only some special characters(_,-,.,:,\\,/,&,#,*,@)")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Please Enter Password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please Enter Password")]
        [Compare("Password", ErrorMessage = "Password Mismatch")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Please Select User Roles")]
        public string Role { get; set; }
    }
}
