using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Services
{
    public interface IInteractionService
    {
        Task DisplayAlertAsync(string title, string message, string cancel);
        void ChangeTab(int index);
    }
}
