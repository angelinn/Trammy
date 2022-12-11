using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Messages;

public class ChangePageMessage
{
    public string Page { get; set; }

    public ChangePageMessage(string page)
    {
        Page = page;
    }
}
