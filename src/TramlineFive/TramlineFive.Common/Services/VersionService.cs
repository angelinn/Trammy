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
        public readonly IApplicationService applicationService;

        public VersionService(IApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        public async Task<NewVersion> CheckForUpdates()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("TramlineFive.Xamarin"));
            IReadOnlyList<Release> res = await client.Repository.Release.GetAll("angelinn", "TramlineFive.Xamarin");
            Release lastRelease = res.First();

            string version = applicationService.GetVersion();

            if (String.Compare(lastRelease.TagName, version) > 0)
                return new NewVersion { VersionNumber = lastRelease.TagName, ReleaseUrl = lastRelease.HtmlUrl };

            return null;
        }
    }
}
