using Octokit;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TramlineFive.Common.Services.Interfaces;

namespace TramlineFive.Common.Services;

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

        Version release = new Version(lastRelease.TagName);
        Version current = new Version(version);

        if (release > current)
            return new NewVersion { VersionNumber = lastRelease.TagName, ReleaseUrl = lastRelease.HtmlUrl };

        return null;
    }
}
