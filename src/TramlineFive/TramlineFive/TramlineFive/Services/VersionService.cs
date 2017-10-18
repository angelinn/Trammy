using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Services
{
    public class VersionService
    {
        public static async Task<bool> CheckForUpdates()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("TramlineFive.Xamarin"));
            IReadOnlyList<Release> res = await client.Repository.Release.GetAll("betrakiss", "TramlineFive.Xamarin");
            Release r = res.First();
            
            return true;
        }
    }
}
