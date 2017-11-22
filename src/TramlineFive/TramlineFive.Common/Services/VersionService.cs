using GalaSoft.MvvmLight.Ioc;
using Octokit;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TramlineFive.Common.Services
{
    public class VersionService
    {
        public static async Task<NewVersion> CheckForUpdates()
        {
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("TramlineFive.Xamarin"));
                IReadOnlyList<Release> res = await client.Repository.Release.GetAll("betrakiss", "TramlineFive.Xamarin");
                Release lastRelease = res.First();

                string version = SimpleIoc.Default.GetInstance<IApplicationService>().GetVersion();

                if (String.Compare(lastRelease.TagName, version) > 0)
                    return new NewVersion { VersionNumber = lastRelease.TagName, ReleaseUrl = lastRelease.HtmlUrl };

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
