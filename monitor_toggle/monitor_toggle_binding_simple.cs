using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace monitor_toggle;

[PluginName("Monitor Toggle Simple")]
public sealed class MonitorToggleBindingSimple : IStateBinding
{
    internal static bool IsActive { set; get; }
    internal static TargetDisplay TargetDisplayInfo = new();

    public static string[] ValidModes => new[] { "Toggle", "Hold", "Cycle" };

    [Property("Mode"), PropertyValidated(nameof(ValidModes)), DefaultPropertyValue("Toggle"), ToolTip
            ("The monitor switching mode to use. Toggle, Hold, or Cycle.\n" +
            "Toggle activates when the binding is pressed and deactivates when the binding is pressed a second time.\n" +
            "Hold activates when the binding is pressed and deactivates when the binding is released.\n" +
            "Cycle activates when the binding is pressed and always stays active. When the binding is pressed again it changes to the next setting.\n" +
            "Specify multiple settings for cycle mode by separating them with a comma.")]
    public string? Mode { set; get; }

    [Property("Display Index"), DefaultPropertyValue("0"), ToolTip("The display index to map to.")]
    public string? DisplayIndex { set; get; }

    public void Press(TabletReference tablet, IDeviceReport report)
    {
        var displays = _virtualScreen?.Displays.ToArray();
        var targetDisplay = displays?.First((x) => x.Index.ToString() == DisplayIndex);
        if (targetDisplay != null)
        {
            TargetDisplayInfo = new TargetDisplay
            {
                Width = targetDisplay.Width,
                Height = targetDisplay.Height,
                X = targetDisplay.Position.X,
                Y = targetDisplay.Position.Y
            };
        }

        if (Mode == "Toggle")
        {
            IsActive = !IsActive;
        }
        else if (Mode == "Hold")
        {
            IsActive = true;
        }
    }

    public void Release(TabletReference tablet, IDeviceReport report)
    {
        if (Mode == "Hold")
        {
            IsActive = false;
        }
    }

    private IVirtualScreen? _virtualScreen;

    [Resolved]
    public System.IServiceProvider ServiceProvider
    {
        set => _virtualScreen = (IVirtualScreen)value.GetService(typeof(IVirtualScreen))!;
    }

    public struct TargetDisplay
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
