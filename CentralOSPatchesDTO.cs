namespace Monitoring.Models.DTO
{
    public class CentralOSPatchesDTO
    {
       
        public string? Id { get; set; }

        public string UpdateId { get; set; }
              
        public string UpdateOS { get; set; }
        public string? OSVersion { get; set; }

        public string BitRate { get; set; }


        public string? Title { get; set; }

 
        public string? Product { get; set; }

 
        public string? Classification { get; set; }

        public string KBNumber { get; set; }

        public string? KBNumberDescription { get; set; }

     
        public string? ProductFamily { get; set; }

        public string? Platform { get; set; }

    
        public string? Version { get; set; }


        public string? Size { get; set; }

        public string? BuildNumber { get; set; }


        public string? Articles { get; set; }

   
        public string? ReleaseDate { get; set; }
        public string? FileName { get; set; }

        public string? PatchPath { get; set; }

        // ✅ ADD THESE (VERY IMPORTANT)

  
        public DateTime? CreatedAt { get; set; }


        public DateTime? ModifiedAt { get; set; }

        public bool IsDeleted { get; set; }    }
}
