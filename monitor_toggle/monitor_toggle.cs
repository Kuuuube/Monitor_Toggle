using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;
using System;
using System.Numerics;
using OpenTabletDriver.Plugin;
using System.Linq;
using System.Collections.Generic;

namespace monitor_toggle;

[PluginName("Monitor Toggle")]
public sealed class monitor_toggle : monitor_toggle_base
{
    public override event Action<IDeviceReport> Emit;

    private ITabletReport absolute_mode(ref ITabletReport report) {
        if (monitor_toggle_binding.is_active) {
            report.Position = from_unit_screen(to_unit_screen(report.Position + monitor_toggle_binding.offset, monitor_toggle_binding.offset) * monitor_toggle_binding.multiplier, monitor_toggle_binding.offset);
        }
        return report;
    }

    private Vector2 current_offset = new Vector2();
    private bool last_is_active = false;

    private ITabletReport relative_mode(ref ITabletReport report) {
        if (current_offset != monitor_toggle_binding.offset && monitor_toggle_binding.is_active) {
            report.Position = report.Position + (monitor_toggle_binding.offset - current_offset);
            current_offset = monitor_toggle_binding.offset;
        } else if (monitor_toggle_binding.is_active != last_is_active) {
            report.Position = report.Position - current_offset;
            current_offset = new Vector2();
        }
        last_is_active = monitor_toggle_binding.is_active;

        if (monitor_toggle_binding.is_active) {
            report.Position = report.Position * monitor_toggle_binding.multiplier;
        }
        return report;
    }

    public override void Consume(IDeviceReport value) {
        if (value is ITabletReport report) {
            OutputModeType output_mode = get_output_mode();
            if (output_mode == OutputModeType.absolute) {
                absolute_mode(ref report);
            } else if (output_mode == OutputModeType.relative) {
                relative_mode(ref report);
            }
            value = report;
        }

        Emit?.Invoke(value);
    }

    public override PipelinePosition Position => PipelinePosition.PostTransform;
}

[PluginName("Monitor Toggle")]
public sealed class monitor_toggle_binding : IStateBinding
{
    internal static bool is_active { set; get; }
    internal static Vector2 offset = new Vector2();
    internal static Vector2 multiplier = new Vector2(1, 1);

    int array_index = -1; //temp start value
    internal static Vector2[] offset_array = new Vector2[] {};
    internal static Vector2[] multiplier_array = new Vector2[] {};

    public static string[] valid_modes => new[] { "Toggle", "Hold", "Cycle" };

    [Property("Mode"), PropertyValidated(nameof(valid_modes)), DefaultPropertyValue("Toggle")]
    public string mode { set; get; }

    [Property("Offset X"), DefaultPropertyValue("0"), Unit("px")]
    public string offset_x { set; get; }

    [Property("Offset Y"), DefaultPropertyValue("0"), Unit("px")]
    public string offset_y { set; get; }

    [Property("Width Multiplier"), DefaultPropertyValue("1")]
    public string width_multiplier { set; get; }

    [Property("Height Mutiplier"), DefaultPropertyValue("1")]
    public string height_multiplier { set; get; }

    public void Press(TabletReference tablet, IDeviceReport report) {
        Vector2[] new_offset_array;
        Vector2[] new_multiplier_array;

        try {
            new_offset_array = to_vector2_array(offset_x, offset_y);
            new_multiplier_array = to_vector2_array(width_multiplier, height_multiplier);
        } catch (Exception exception) {
            Log.Exception(exception);
            Log.WriteNotify("Monitor Toggle", "Error parsing binding settings. Offset X: " + offset_x + ", Offset Y: " + offset_y + ", Width Multiplier: " + width_multiplier + ", Height Multiplier: " + height_multiplier, LogLevel.Error);
            return;
        }

        if (!new_offset_array.SequenceEqual(offset_array) || !new_multiplier_array.SequenceEqual(multiplier_array) || mode == "Cycle") {
            offset_array = new_offset_array;
            multiplier_array = new_multiplier_array;
            if (array_index + 1 >= offset_array.Length || array_index + 1 >= multiplier_array.Length || array_index == -1) {
                array_index = 0;
            } else if (mode == "Cycle") {
                array_index += 1;
            }
            offset = offset_array[array_index];
            multiplier = multiplier_array[array_index];

            is_active = true;
            return;
        }

        if (mode == "Toggle") {
            is_active = !is_active;
        } else if (mode == "Hold") {
            is_active = true;
        }
    }

    private Vector2[] to_vector2_array(string input_string_1, string input_string_2) {
        float[] x_array = input_string_1.Split(',').Select(str => float.Parse(str.Trim())).ToArray();
        float[] y_array = input_string_2.Split(',').Select(str => float.Parse(str.Trim())).ToArray();

        List<Vector2> temp_list = new List<Vector2> {};
        foreach (var element in x_array.Zip(y_array, (x, y) => new {x = x, y = y})) {
            temp_list.Add(new Vector2(element.x, element.y));
        }
        return temp_list.ToArray();
    }

    public void Release(TabletReference tablet, IDeviceReport report) {
        if (mode == "Hold") {
            is_active = false;
        }
    }
}