using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stripfaces.Models
{
    public class User
    {
        [Key]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("username")]  // Add this
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Column("email")]  // Add this
        public string Email { get; set; }

        [Required]
        [Column("password")]  // Add this
        public string Password { get; set; }

        [StringLength(10)]
        [Column("role")]  // Add this
        public string Role { get; set; } = "user";

        [Column("created_at")]  // Add this
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Video> UploadedVideos { get; set; }
    }
}