using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogenesis.DTO
{
    public class CreateWithAiDto
    {
        public string Subject { get; set; }
        public string Tone { get; set; }
        public string Length { get; set; }
    }
}