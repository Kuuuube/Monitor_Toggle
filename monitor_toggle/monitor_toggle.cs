using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;
using System;
using System.Numerics;
using OpenTabletDriver.Plugin;

namespace monitor_toggle;

[PluginName("Monitor Toggle")]
public sealed class monitor_toggle : monitor_toggle_base
{
    public override event Action<IDeviceReport> Emit;

    private ITabletReport absolute_mode(ITabletReport report)
    {
        if (monitor_toggle_binding.is_active)
        {
            report.Position = from_unit_screen(to_unit_screen(report.Position + monitor_toggle_binding.offset, monitor_toggle_binding.offset) * monitor_toggle_binding.multiplier, monitor_toggle_binding.offset);
        }
        return report;
    }

    private Vector2 current_offset = new();
    private bool last_is_active = false;

    private ITabletReport relative_mode(ITabletReport report)
    {
        if (current_offset != monitor_toggle_binding.offset && monitor_toggle_binding.is_active)
        {
            report.Position += monitor_toggle_binding.offset - current_offset;
            current_offset = monitor_toggle_binding.offset;
        }
        else if (monitor_toggle_binding.is_active != last_is_active)
        {
            report.Position -= current_offset;
            current_offset = new Vector2();
        }
        last_is_active = monitor_toggle_binding.is_active;

        if (monitor_toggle_binding.is_active)
        {
            report.Position *= monitor_toggle_binding.multiplier;
        }
        return report;
    }

    public override void Consume(IDeviceReport value)
    {
        if (value is ITabletReport report)
        {
            OutputModeType output_mode = get_output_mode();
            if (output_mode == OutputModeType.absolute)
            {
                report = absolute_mode(report);
            }
            else if (output_mode == OutputModeType.relative)
            {
                report = relative_mode(report);
            }
            value = report;
        }

        Emit?.Invoke(value);
    }

    public override PipelinePosition Position => PipelinePosition.PostTransform;
}
