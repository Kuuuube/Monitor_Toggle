using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;
using System;
using System.Numerics;

namespace monitor_toggle;

[PluginName("Monitor Toggle")]
public sealed class MonitorToggle : MonitorToggleBase
{
    public override event Action<IDeviceReport>? Emit;

    private ITabletReport AbsoluteMode(ITabletReport report)
    {
        if (MonitorToggleBinding.IsActive || MonitorToggleBindingSimple.IsActive)
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
        if (currentOffset != offset && (MonitorToggleBinding.IsActive || MonitorToggleBindingSimple.IsActive))
        {
            report.Position += offset - currentOffset;
            currentOffset = offset;
        }
        else if ((MonitorToggleBinding.IsActive || MonitorToggleBindingSimple.IsActive) != lastIsActive)
        {
            report.Position -= currentOffset;
            currentOffset = new Vector2();
        }
        lastIsActive = MonitorToggleBinding.IsActive || MonitorToggleBindingSimple.IsActive;

        if (MonitorToggleBinding.IsActive || MonitorToggleBindingSimple.IsActive)
        {
            report.Position *= multiplier;
        }
        return report;
    }

    private Vector2 GetOffset()
    {
        if (MonitorToggleBinding.IsActive) return MonitorToggleBinding.offset;
        if (MonitorToggleBindingSimple.IsActive)
        {
            var currentOffsets = GetCurrentDisplayOffsets();
            return new Vector2(
                MonitorToggleBindingSimple.TargetDisplayInfo.CenterOffsetX - currentOffsets.X,
                MonitorToggleBindingSimple.TargetDisplayInfo.CenterOffsetY - currentOffsets.Y
            );
        }
        return new();
    }

    private Vector2 GetMultiplier()
    {
        if (MonitorToggleBinding.IsActive) return MonitorToggleBinding.multiplier;
        if (MonitorToggleBindingSimple.IsActive)
        {
            var currentSize = GetCurrentDisplaySize();
            return new Vector2(
                MonitorToggleBindingSimple.TargetDisplayInfo.Width / currentSize.X,
                MonitorToggleBindingSimple.TargetDisplayInfo.Height / currentSize.Y
            );
        }
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
