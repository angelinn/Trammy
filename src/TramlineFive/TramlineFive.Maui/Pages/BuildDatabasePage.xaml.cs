using CsvHelper;
using Microsoft.Maui.Controls;
using SQLite;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.Maui.Pages;

public partial class BuildDatabasePage : ContentPage
{
    public BuildDatabasePage()
    {
        InitializeComponent();
        Task.Run(() => BuildDatabaseAsync());
    }

    private async Task BuildDatabaseAsync()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = "������� �� GTFS �����...";
                ProgressBar.Progress = 0;
            });

            // Download GTFS zip
            var client = new HttpClient();
            var zipBytes = await client.GetByteArrayAsync("https://gtfs.sofiatraffic.bg/api/v1/static");
            var zipPath = Path.Combine(FileSystem.CacheDirectory, "gtfs.zip");
            await File.WriteAllBytesAsync(zipPath, zipBytes);

            // Extract
            var extractPath = Path.Combine(FileSystem.CacheDirectory, "gtfs");
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            ZipFile.ExtractToDirectory(zipPath, extractPath);

            // Create database
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gtfs.db");
            var db = new SQLiteConnection(dbPath);

            db.CreateTable<Stop>();
            db.CreateTable<Route>();
            db.CreateTable<Trip>();
            db.CreateTable<StopTime>();
            db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS IX_Trip_Stop ON StopTime(TripId, StopId)");

            // Insert CSVs
            await InsertCsvAsync<Stop>(db, Path.Combine(extractPath, "stops.txt"), "stops");
            await InsertCsvAsync<Route>(db, Path.Combine(extractPath, "routes.txt"), "routes");
            await InsertCsvAsync<Trip>(db, Path.Combine(extractPath, "trips.txt"), "trips");
            await InsertCsvAsync<StopTime>(db, Path.Combine(extractPath, "stop_times.txt"), "stop_times");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = "Database build complete!";
                ProgressBar.Progress = 1;
                App.Current.Windows[0].Page = new AppShell();
                //Shell.Current.GoToAsync("//Main");
            });

        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = $"Error: {ex.Message}";
            });
        }
    }

    private async Task InsertCsvAsync<T>(SQLiteConnection db, string filePath, string tableName)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,       // Ignore missing header validation
            PrepareHeaderForMatch = (args) => args.Header.Replace("_", "").ToLower() // Normalize
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = $"������ �� {tableName}...";
        });

        var records = csv.GetRecords<T>().ToList();
        int total = records.Count;
        int count = 0;

        await Task.Run(() =>
        {
            db.RunInTransaction(() =>
            {
                foreach (var record in records)
                {
                    db.InsertOrReplace(record);
                    count++;
                    if (count % 100 == 0) // update UI every 100 records
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ProgressBar.Progress = (double)count / total;
                            StatusLabel.Text = $"��������� �� {tableName}: {count}/{total}";
                        });
                    }
                }
            });
        });

        // Ensure final progress update
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProgressBar.Progress = 1;
            StatusLabel.Text = $"������ {tableName}";
        });
    }
}
