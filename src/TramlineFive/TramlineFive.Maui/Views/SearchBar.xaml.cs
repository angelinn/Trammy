
using CommunityToolkit.Mvvm.Messaging;
using TramlineFive.Common.Messages;

namespace TramlineFive.Maui.Views;

public partial class SearchBar : Border
{
    public SearchBar()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<StopSelectedMessage>(this, (r, m) =>
        {
            txtSearch.IsEnabled = false;
            txtSearch.IsEnabled = true;
        });
    }
}
