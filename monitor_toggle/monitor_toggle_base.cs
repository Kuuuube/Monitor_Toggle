using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using System;
using System.Linq;
using System.Numerics;

namespace monitor_toggle;

public abstract class MonitorToggleBase : IPositionedPipelineElement<IDeviceReport>
{
    public OutputModeType GetOutputMode()
    {
        TryResolveOutputMode();
        return outputModeType;
    }

    public Vector2 ToUnitScreen(Vector2 input, Vector2 activeOffset)
    {
        if (outputModeType == OutputModeType.absolute && absoluteOutputMode != null)
        {
            var display = absoluteOutputMode.Output;
            var offset = absoluteOutputMode.Output.Position;
            var shiftoffX = offset.X - (display.Width / 2);
            var shiftoffY = offset.Y - (display.Height / 2);
            return new Vector2((input.X - shiftoffX - activeOffset.X) / display.Width * 2 - 1, (input.Y - shiftoffY - activeOffset.Y) / display.Height * 2 - 1);
        }

        TryResolveOutputMode();
        return default;
    }

    public Vector2 FromUnitScreen(Vector2 input, Vector2 activeOffset)
    {
        if (outputModeType == OutputModeType.absolute && absoluteOutputMode != null)
        {
            var display = absoluteOutputMode.Output;
            var offset = absoluteOutputMode.Output.Position;
            var shiftoffX = offset.X - (display.Width / 2);
            var shiftoffY = offset.Y - (display.Height / 2);
            return new Vector2((input.X + 1) / 2 * display.Width + shiftoffX + activeOffset.X, (input.Y + 1) / 2 * display.Height + shiftoffY + activeOffset.Y);
        }

        TryResolveOutputMode();
        return default;
    }

    protected static Vector2 clamp(Vector2 input)
    {
        return new Vector2(
            Math.Clamp(input.X, -1, 1),
            Math.Clamp(input.Y, -1, 1)
        );
    }

    [Resolved]
    public IDriver driver;
    private OutputModeType outputModeType;
    private AbsoluteOutputMode absoluteOutputMode;
    private RelativeOutputMode relativeOutputMode;
    private void TryResolveOutputMode()
    {
        if (driver is Driver drv)
        {
            IOutputMode output = drv.InputDevices
                .Where(dev => dev?.OutputMode?.Elements?.Contains(this) ?? false)
                .Select(dev => dev?.OutputMode).FirstOrDefault();

            if (output is AbsoluteOutputMode absOutput)
            {
                absoluteOutputMode = absOutput;
                outputModeType = OutputModeType.absolute;
                return;
            }
            if (output is RelativeOutputMode relOutput)
            {
                relativeOutputMode = relOutput;
                outputModeType = OutputModeType.relative;
                return;
            }
            outputModeType = OutputModeType.unknown;
        }
    }

    public abstract event Action<IDeviceReport> Emit;
    public abstract void Consume(IDeviceReport value);
    public abstract PipelinePosition Position { get; }
}

public enum OutputModeType
{
    absolute,
    relative,
    unknown
}