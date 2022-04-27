using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TramlineFive.Common.Models
{
    public class ViewData
    {
        public bool IsVisible { get; set; }
        public ICommand Show { get; set; }
        public ICommand Hide { get; set; }

        public ViewData(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
}
