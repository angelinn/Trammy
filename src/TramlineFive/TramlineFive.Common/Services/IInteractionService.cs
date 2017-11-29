using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Services
{
    public interface IInteractionService
    {
        Task<bool> DisplayAlertAsync(string title, string message, string ok, string cancel = "");
        void ChangeTab(int index);
        void DisplayToast(string message);
    }
}
