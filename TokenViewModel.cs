namespace Monitoring.ViewModel
{
    public class TokenViewModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string OrgId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Sid { get; set; } = string.Empty;
        public string Jtid { get; set; } = string.Empty;

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        public string Email { get; set; }
        public int OrganizationId
        {
            get
            {
                return Convert.ToInt32(OrgId);
            }
        }
        public string Token { get; set; } = string.Empty;
    }
}
