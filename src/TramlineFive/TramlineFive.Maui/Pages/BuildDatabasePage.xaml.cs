using CsvHelper;
using Microsoft.Maui.Controls;
using SkgtService;
using SQLite;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.Maui.Pages;

public partial class BuildDatabasePage : ContentPage
{
    public BuildDatabasePage()
    {
        InitializeComponent();
        Task.Run(() => BuildDatabaseAsync());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DeviceDisplay.Current.KeepScreenOn = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing(); 
        DeviceDisplay.Current.KeepScreenOn = false;
    }

    private async Task BuildDatabaseAsync()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = "Сваляне на GTFS данни...";
                ProgressBar.Progress = 0;
            });

            string zipPath = Path.Combine(FileSystem.CacheDirectory, "gtfs.zip");
            string extractPath = Path.Combine(FileSystem.CacheDirectory, "gtfs");
            GTFSDownloader downloader = new GTFSDownloader("https://gtfs.sofiatraffic.bg/api/v1/static", zipPath, extractPath);
            await downloader.DownloadStaticDataAsync();
            downloader.ExtractStaticData();

            await GTFSContext.CreateDatabaseFromStaticDataAsync(extractPath, (progress) =>
            {
                Dispatcher.Dispatch(() =>
                {
                    ProgressBar.Progress = progress.Progress;
                    StatusLabel.Text = progress.Status;
                });
            });

            Dispatcher.Dispatch(() =>
            {
                App.Current.Windows[0].Page = new AppShell();
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Грешка", $"Възникна грешка при изграждането на базата данни: {ex.Message}", "ОК");
            });
        }
    }
}
