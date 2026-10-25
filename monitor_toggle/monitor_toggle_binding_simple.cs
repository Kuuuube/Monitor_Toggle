using System;
using System.Collections.Generic;
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
    internal static TargetDisplay activeTargetDisplay = new();

    int arrayIndex = -1; //temp start value
    private TargetDisplay[] targetDisplaysArray = new TargetDisplay[] { };

    public static string[] ValidModes => new[] { "Toggle", "Hold", "Cycle" };

    [Property("Mode"), PropertyValidated(nameof(ValidModes)), DefaultPropertyValue("Toggle"), ToolTip
            ("The monitor switching mode to use. Toggle, Hold, or Cycle.\n" +
            "Toggle activates when the binding is pressed and deactivates when the binding is pressed a second time.\n" +
            "Hold activates when the binding is pressed and deactivates when the binding is released.\n" +
            "Cycle activates when the binding is pressed and always stays active. When the binding is pressed again it changes to the next setting.\n" +
            "Specify multiple settings for cycle mode by separating them with a comma.")]
    public string? Mode { set; get; }

    [Property("Display Indexes"), DefaultPropertyValue("0"), ToolTip("The display index (or indexes) to map to.")]
    public string? DisplayIndexes { set; get; }

    public void Press(TabletReference tablet, IDeviceReport report)
    {
        var displays = _virtualScreen?.Displays.ToArray();
        TargetDisplay[] newTargetDisplaysArray = new TargetDisplay[] { };

        try
        {
            newTargetDisplaysArray = ToTargetDisplayArray(DisplayIndexes!, displays!);
        }
        catch (Exception exception)
        {
            Log.Exception(exception);
            Log.WriteNotify("Monitor Toggle", "Error parsing binding settings. DisplayIndexes: " + DisplayIndexes, LogLevel.Error);
            return;
        }

        if (!newTargetDisplaysArray.SequenceEqual(targetDisplaysArray) || Mode == "Cycle")
        {
            targetDisplaysArray = newTargetDisplaysArray;
            if (arrayIndex + 1 >= targetDisplaysArray.Length || arrayIndex == -1)
            {
                arrayIndex = 0;
            }
            else if (Mode == "Cycle")
            {
                arrayIndex += 1;
            }
            activeTargetDisplay = newTargetDisplaysArray[arrayIndex];

            IsActive = true;
            return;
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

    private TargetDisplay[] ToTargetDisplayArray(string displayIndexes, IDisplay[] displays)
    {
        string[] indexesArray = displayIndexes.Split(',').ToArray();

        List<TargetDisplay> tempList = new() { };
        foreach (var element in indexesArray)
        {
            var targetDisplay = displays?.First((x) => x.Index == int.Parse(element.Trim()));
            if (targetDisplay != null)
            {
                tempList.Add(new TargetDisplay
                {
                    Width = targetDisplay.Width,
                    Height = targetDisplay.Height,
                    // Offsets are based on the top left corner by default (to see this: right click the Display area -> hover `Set to display`)
                    CenterOffsetX = targetDisplay.Position.X + targetDisplay.Width / 2,
                    CenterOffsetY = targetDisplay.Position.Y + targetDisplay.Height / 2
                });
            }
        }
        return tempList.ToArray();
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
        public float CenterOffsetX { get; set; }
        public float CenterOffsetY { get; set; }
    }
}
