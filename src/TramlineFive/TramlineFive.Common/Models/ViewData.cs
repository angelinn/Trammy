using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Models
{
    public class ViewData
    {
        public bool IsVisible { get; set; }

        public ViewData(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
}
