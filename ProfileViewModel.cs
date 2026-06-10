using System.ComponentModel.DataAnnotations;

namespace Monitoring.ViewModel
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Please Enter FirstName")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please Enter LastName")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please Enter Email Address")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please Enter Mobile Number")]
        [Phone(ErrorMessage = "Please Enter Valid Mobile Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile Number should be 10 digits")]
        public string MobileNumber { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please Enter Address")]
        [RegularExpression(@"^[a-zA-Z0-9.:;,' ?/\\&*()#@_ \s-]+$", ErrorMessage = "Address contain only some special characters(_,-,.,:,\\,/,&,#,*,@)")]
        public string Address { get; set; } = string.Empty;
    }
}
