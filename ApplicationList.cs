using System;
using System.Collections.Generic;

namespace Monitoring.Data
{
    public partial class ApplicationList
    {
        public ApplicationList()
        {
            ApplicationPermissions = new HashSet<ApplicationPermission>();
        }

        public int AppsListId { get; set; }
        public string AppsListName { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Icons { get; set; } = null!;

        public virtual ICollection<ApplicationPermission> ApplicationPermissions { get; set; }
    }
}
