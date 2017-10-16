using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService.Models
{
    public class Captcha
    {
        public string StringContent { get; set; }
        public byte[] BinaryContent { get; set; }
    }
}
