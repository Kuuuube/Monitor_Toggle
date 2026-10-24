using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace monitor_toggle;

[PluginName("Monitor Toggle Simple")]
public sealed class MonitorToggleBindingSimple : IStateBinding
{
    internal static bool IsActive { set; get; }
    internal static Vector2 offset = new();
    internal static Vector2 multiplier = new(1, 1);

    public static string[] ValidModes => new[] { "Toggle", "Hold", "Cycle" };

    [Property("Mode"), PropertyValidated(nameof(ValidModes)), DefaultPropertyValue("Toggle"), ToolTip
            ("The monitor switching mode to use. Toggle, Hold, or Cycle.\n" +
            "Toggle activates when the binding is pressed and deactivates when the binding is pressed a second time.\n" +
            "Hold activates when the binding is pressed and deactivates when the binding is released.\n" +
            "Cycle activates when the binding is pressed and always stays active. When the binding is pressed again it changes to the next setting.\n" +
            "Specify multiple settings for cycle mode by separating them with a comma.")]
    public string? Mode { set; get; }

    public void Press(TabletReference tablet, IDeviceReport report)
    {
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
}