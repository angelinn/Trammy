using Mapsui.Utilities;

namespace TramlineFive.Maui.Views;

public partial class VirtualTablesViewCell : Border
{
    private static readonly ConcurrentHashSet<int> animations = new();
    private readonly HashSet<int> localAnimations = new();

    public VirtualTablesViewCell()
    {
        InitializeComponent();
        Task _ = Animate();

        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, EventArgs e)
    {
        foreach (int animation in localAnimations)
            animations.TryRemove(animation);
    }

    private async Task Animate()
    {
        Label icon = this.GetVisualTreeDescendants().OfType<Label>().Last(l => l.FontFamily == "MaterialIconsOutlinedRegular.otf");//.First(l => l.Text == "&#xe52e;");

        animations.Add(icon.GetHashCode());
        localAnimations.Add(icon.GetHashCode());

        while (animations.Contains(icon.GetHashCode()))
        {
            await icon.FadeTo(0.2, 1000);
            System.Diagnostics.Debug.WriteLine("Faded to 0 " + GetHashCode());

            await Task.Delay(1000);
            await icon.FadeTo(1, 1000);
            System.Diagnostics.Debug.WriteLine("Faded to 1 " + GetHashCode());
            await Task.Delay(1000);
        }
    }
}
