using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TramlineFive.Common.ViewModels;

public class LicensesViewModel : BaseViewModel
{
    public Projects Licenses { get; private set; }

    public async Task Initialize()
    {
        using Stream licensesStream = typeof(SettingsViewModel).Assembly.GetManifestResourceStream("TramlineFive.Common.licenses.yaml");
        using StreamReader reader = new StreamReader(licensesStream);

        string yaml = await reader.ReadToEndAsync();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance)
            .Build();

        Licenses = deserializer.Deserialize<Projects>(yaml);
        RaisePropertyChanged("Licenses");
    }
}
