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
    public override event Action<IDeviceReport>? Emit;

    private ITabletReport AbsoluteMode(ITabletReport report)
    {
        if (MonitorToggleBinding.IsActive || MonitorToggleBindingAuto.IsActive)
        {
            var offset = GetOffset();
            var multiplier = GetMultiplier();

            report.Position = FromUnitScreen(ToUnitScreen(report.Position + offset, offset) * multiplier, offset);
        }
        return report;
    }

    private Vector2 currentOffset = new();
    private bool lastIsActive = false;

    private ITabletReport RelativeMode(ITabletReport report)
    {
        var offset = GetOffset();
        var multiplier = GetMultiplier();
        if (currentOffset != offset && (MonitorToggleBinding.IsActive || MonitorToggleBindingAuto.IsActive))
        {
            report.Position += offset - currentOffset;
            currentOffset = offset;
        }
        else if ((MonitorToggleBinding.IsActive || MonitorToggleBindingAuto.IsActive) != lastIsActive)
        {
            report.Position -= currentOffset;
            currentOffset = new Vector2();
        }
        lastIsActive = MonitorToggleBinding.IsActive || MonitorToggleBindingAuto.IsActive;

        if (MonitorToggleBinding.IsActive || MonitorToggleBindingAuto.IsActive)
        {
            report.Position *= multiplier;
        }
        return report;
    }

    private Vector2 GetOffset()
    {
        if (MonitorToggleBinding.IsActive) return MonitorToggleBinding.offset;
        if (MonitorToggleBindingAuto.IsActive) return MonitorToggleBindingAuto.offset;
        return new();
    }

    private Vector2 GetMultiplier()
    {
        if (MonitorToggleBinding.IsActive) return MonitorToggleBinding.multiplier;
        if (MonitorToggleBindingAuto.IsActive) return MonitorToggleBindingAuto.multiplier;
        return new();
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
