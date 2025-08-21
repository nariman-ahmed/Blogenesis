using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blogenesis.Models;

public class BlogModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    [Required]
    public int ReadTimeMinutes { get; set; }

    [Required]
    [MaxLength(50)]
    public string Subject { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    // Foreign key for User
    public string UserId { get; set; } = string.Empty;

    // Navigation properties
    public virtual UserModel? User { get; set; }
    public virtual ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
    public virtual ICollection<LikeModel> Likes { get; set; } = new List<LikeModel>();
}
