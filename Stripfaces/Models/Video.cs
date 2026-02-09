using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stripfaces.Models
{
    [Table("videos")]  // Add this
    public class Video
    {
        [Key]
        [Column("VideoId")]  // Add this
        public int VideoId { get; set; }

        [Required]
        [StringLength(200)]
        [Column("title")]  // Add this
        public string? Title { get; set; }

        [Column("description")]  // Add this
        public string? Description { get; set; }

        [Display(Name = "Video File")]
        [Column("file_path")]  // Add this
        public string? FilePath { get; set; }

        [Display(Name = "Thumbnail")]
        [Column("thumbnail_path")]  // Add this
        public string? ThumbnailPath { get; set; }

        [Display(Name = "Duration (seconds)")]
        [Column("duration")]  // Add this
        public int? Duration { get; set; }

        [Display(Name = "File Size (bytes)")]
        [Column("file_size")]  // Add this
        public long? FileSize { get; set; }

        [Column("views")]  // Add this
        public int Views { get; set; } = 0;

        [Column("model_id")]  // Add this
        public int ModelId { get; set; }

        [Column("uploaded_by")]  // Add this
        public int UploadedById { get; set; }

        [Column("uploaded_at")]  // Add this
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [Column("tags")]  // Add this
        public string? Tags { get; set; }

        [Display(Name = "Approved")]
        [Column("is_approved")]  // Add this
        public bool IsApproved { get; set; } = true;

        [Display(Name = "Featured")]
        [Column("is_featured")]  // Add this
        public bool IsFeatured { get; set; }

        // Navigation properties
        [ForeignKey("ModelId")]
        public virtual Model Model { get; set; }

        [ForeignKey("UploadedById")]
        public virtual User UploadedBy { get; set; }
    }
}