using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin;
using System;

namespace monitor_toggle;

[PluginName("Monitor Toggle")]
public sealed class MonitorToggleBinding : IStateBinding
{
    internal static bool IsActive { set; get; }
    internal static Vector2 offset = new();
    internal static Vector2 multiplier = new(1, 1);

    int arrayIndex = -1; //temp start value
    internal static Vector2[] offsetArray = new Vector2[] { };
    internal static Vector2[] multiplierArray = new Vector2[] { };

    public static string[] ValidModes => new[] { "Toggle", "Hold", "Cycle" };

    [Property("Mode"), PropertyValidated(nameof(ValidModes)), DefaultPropertyValue("Toggle"), ToolTip
            ("The monitor switching mode to use. Toggle, Hold, or Cycle.\n" +
            "Toggle activates when the binding is pressed and deactivates when the binding is pressed a second time.\n" +
            "Hold activates when the binding is pressed and deactivates when the binding is released.\n" +
            "Cycle activates when the binding is pressed and always stays active. When the binding is pressed again it changes to the next setting.\n" +
            "Specify multiple settings for cycle mode by separating them with a comma.")]
    public string? Mode { set; get; }

    [Property("Offset X"), DefaultPropertyValue("0"), Unit("px"), ToolTip
            ("The amount in pixels to offset the cursor's X position by.")]
    public string? OffsetX { set; get; }

    [Property("Offset Y"), DefaultPropertyValue("0"), Unit("px"), ToolTip
            ("The amount in pixels to offset the cursor's Y position by.")]
    public string? OffsetY { set; get; }

    [Property("Width Multiplier"), DefaultPropertyValue("1"), ToolTip
            ("In absolute mode, the multiplier to increase the monitor area's width by.\n" +
            "In relative mode, the multiplier to increase the X sensitivity by.")]
    public string? WidthMultiplier { set; get; }

    [Property("Height Multiplier"), DefaultPropertyValue("1"), ToolTip
            ("In absolute mode, the multiplier to increase the monitor area's height by.\n" +
            "In relative mode, the multiplier to increase the Y sensitivity by.")]
    public string? HeightMultiplier { set; get; }

    public void Press(TabletReference tablet, IDeviceReport report)
    {
        Vector2[] newOffsetArray;
        Vector2[] newMultiplierArray;

        try
        {
            newOffsetArray = ToVector2Array(OffsetX!, OffsetY!);
            newMultiplierArray = ToVector2Array(WidthMultiplier!, HeightMultiplier!);
        }
        catch (Exception exception)
        {
            Log.Exception(exception);
            Log.WriteNotify("Monitor Toggle", "Error parsing binding settings. Offset X: " + OffsetX + ", Offset Y: " + OffsetY + ", Width Multiplier: " + WidthMultiplier + ", Height Multiplier: " + HeightMultiplier, LogLevel.Error);
            return;
        }

        if (!newOffsetArray.SequenceEqual(offsetArray) || !newMultiplierArray.SequenceEqual(multiplierArray) || Mode == "Cycle")
        {
            offsetArray = newOffsetArray;
            multiplierArray = newMultiplierArray;
            if (arrayIndex + 1 >= offsetArray.Length || arrayIndex + 1 >= multiplierArray.Length || arrayIndex == -1)
            {
                arrayIndex = 0;
            }
            else if (Mode == "Cycle")
            {
                arrayIndex += 1;
            }
            offset = offsetArray[arrayIndex];
            multiplier = multiplierArray[arrayIndex];

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

    private Vector2[] ToVector2Array(string inputString1, string inputString2)
    {
        // `CultureInfo.InvariantCulture` MUST be included to ensure decimal separators are parsed with `.` instead of `,`
        float[] xArray = inputString1.Split(',').Select(str => float.Parse(str.Trim(), CultureInfo.InvariantCulture)).ToArray();
        float[] yArray = inputString2.Split(',').Select(str => float.Parse(str.Trim(), CultureInfo.InvariantCulture)).ToArray();

        List<Vector2> tempList = new() { };
        foreach (var element in xArray.Zip(yArray, (x, y) => new { x = x, y = y }))
        {
            tempList.Add(new Vector2(element.x, element.y));
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
}