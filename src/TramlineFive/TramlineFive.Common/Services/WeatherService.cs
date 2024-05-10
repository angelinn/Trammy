using OpenMeteo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Services;

public class Forecast
{
    public float? Current { get; set; }
    public string Time { get; set; }
    public float? RH { get; set; }
}

public class WeatherService
{
    private readonly OpenMeteoClient client = new OpenMeteoClient();

    public async Task<Forecast> GetWeather(string place)
    {
        var data = await client.QueryAsync(place);
        DateTime time = DateTime.Parse(data.Current.Time, new CultureInfo("bg-BG"), DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

        return new Forecast()
        {
            Current = data.Current.Temperature,
            RH = data.Current.Relativehumidity_2m,
            Time = time.ToLocalTime().ToString("dd MMMM HH:mm", new CultureInfo("bg-BG"))
        };
    }
}
