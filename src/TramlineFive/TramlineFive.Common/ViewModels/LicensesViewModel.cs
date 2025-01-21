using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TramlineFive.Common.ViewModels;

public class LicensesViewModel : BaseViewModel
{
    public class LicenseInfo
    {
        public string PackageId { get; set; }
        public string License { get; set; }
    }

    public List<LicenseInfo> Licenses { get; private set; } = new();

    public async Task Initialize()
    {
        if (Licenses.Count > 0)
            return;

        await AppendLicenseFile("Licenses.MAUI.json");
        await AppendLicenseFile("Licenses.Common.json");
        await AppendLicenseFile("Licenses.Data.json");
        await AppendLicenseFile("Licenses.Service.json");

        OnPropertyChanged(nameof(Licenses));
    }

    private async Task AppendLicenseFile(string file)
    {
        using Stream mauiLicenses = await ApplicationService.OpenResourceFileAsync(file);
        using StreamReader reader = new StreamReader(mauiLicenses);

        string json = await reader.ReadToEndAsync();

        Licenses.AddRange(JsonConvert.DeserializeObject<List<LicenseInfo>>(json));
    }
}
