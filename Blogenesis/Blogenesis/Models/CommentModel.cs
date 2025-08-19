using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogenesis.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int BlogId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual UserModel? User { get; set; }
        public virtual BlogModel? Blog { get; set; }
    }
}