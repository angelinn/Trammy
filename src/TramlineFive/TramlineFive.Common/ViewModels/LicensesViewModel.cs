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
    public List<IGrouping<string, Project>> Licenses { get; private set; }

    public async Task Initialize()
    {
        using Stream licensesStream = typeof(SettingsViewModel).Assembly.GetManifestResourceStream("TramlineFive.Common.licenses.yaml");
        using StreamReader reader = new StreamReader(licensesStream);

        string yaml = await reader.ReadToEndAsync();

        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance)
            .Build();

        Licenses = deserializer.Deserialize<Projects>(yaml).Licenses.GroupBy(l => l.License).ToList();
        OnPropertyChanged("Licenses");
    }
}
