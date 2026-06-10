using System;
using System.Collections.Generic;

namespace Monitoring.Data
{
    public partial class ApplicationPermission
    {
        public long Id { get; set; }
        public int OrgId { get; set; }
        public int AppsId { get; set; }
        public string? UserId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsActive { get; set; }

        public virtual ApplicationList Apps { get; set; } = null!;
    }
}
