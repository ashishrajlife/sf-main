using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stripfaces.Models
{
    [Table("models")]  // Add this
    public class Model
    {
        [Key]
        [Column("ModelId")]  // Add this
        public int ModelId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]  // Add this
        public string Name { get; set; }

        [Column("bio")]  // Add this
        public string? Bio { get; set; }

        [Display(Name = "Profile Picture")]
        [Column("profile_pic")]  // Add this
        public string? ProfilePic { get; set; }

        [Display(Name = "Active")]
        [Column("is_active")]  // Add this
        public bool IsActive { get; set; } = true;

        [Column("created_at")]  // Add this
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Video> Videos { get; set; }
    }
}