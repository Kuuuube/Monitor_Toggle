using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;
using System;
using System.Numerics;
using OpenTabletDriver.Plugin;

namespace monitor_toggle;

[PluginName("Monitor Toggle")]
public sealed class MonitorToggle : MonitorToggleBase
{
    public override event Action<IDeviceReport> Emit;

    private ITabletReport AbsoluteMode(ITabletReport report)
    {
        if (MonitorToggleBinding.IsActive)
        {
            report.Position = FromUnitScreen(ToUnitScreen(report.Position + MonitorToggleBinding.offset, MonitorToggleBinding.offset) * MonitorToggleBinding.multiplier, MonitorToggleBinding.offset);
        }
        return report;
    }

    private Vector2 currentOffset = new();
    private bool lastIsActive = false;

    private ITabletReport RelativeMode(ITabletReport report)
    {
        if (currentOffset != MonitorToggleBinding.offset && MonitorToggleBinding.IsActive)
        {
            report.Position += MonitorToggleBinding.offset - currentOffset;
            currentOffset = MonitorToggleBinding.offset;
        }
        else if (MonitorToggleBinding.IsActive != lastIsActive)
        {
            report.Position -= currentOffset;
            currentOffset = new Vector2();
        }
        lastIsActive = MonitorToggleBinding.IsActive;

        if (MonitorToggleBinding.IsActive)
        {
            report.Position *= MonitorToggleBinding.multiplier;
        }
        return report;
    }

    public override void Consume(IDeviceReport value)
    {
        if (value is ITabletReport report)
        {
            OutputModeType outputMode = GetOutputMode();
            if (outputMode == OutputModeType.absolute)
            {
                report = AbsoluteMode(report);
            }
            else if (outputMode == OutputModeType.relative)
            {
                report = RelativeMode(report);
            }
            value = report;
        }

        Emit?.Invoke(value);
    }

    public override PipelinePosition Position => PipelinePosition.PostTransform;
}
