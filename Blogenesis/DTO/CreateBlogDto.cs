using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blogenesis.DTO
{
    public class CreateBlogDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int ReadTimeMinutes { get; set; }

        [Required]
        [MaxLength(50)]
        public string Subject { get; set; } = string.Empty;
        
        public bool IsPublished { get; set; }
    }
}