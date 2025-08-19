using Microsoft.AspNetCore.Identity;

namespace Blogenesis.Models;

public class UserModel : IdentityUser
{
    // Add any additional user properties here
    public string? FullName { get; set; }
    public string? ProfilePicUrl { get; set; }
    public string? Bio { get; set; }

    // Navigation properties
    public virtual ICollection<BlogModel> Blogs { get; set; } = new List<BlogModel>();
    public virtual ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
    public virtual ICollection<LikeModel> Likes { get; set; } = new List<LikeModel>();
}
