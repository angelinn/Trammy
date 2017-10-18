using SkgtService.Models;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkgtService
{
    public static class SkgtManager
    {
        public static ISkgtParser Parser { get; } = new DesktopSkgtParser();
        public static Line SelectedLine { get; set; }
        public static event EventHandler<IEnumerable<string>> OnTimingsReceived;
        public static void SendTimings(object sender, IEnumerable<string> timings)
        {
            OnTimingsReceived?.Invoke(sender, timings);
        }
    }
}
