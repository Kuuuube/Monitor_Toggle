using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using System;
using System.Linq;
using System.Numerics;

namespace monitor_toggle;
public abstract class monitor_toggle_base : IPositionedPipelineElement<IDeviceReport>
{
    public Vector2 to_unit_screen(Vector2 input, Vector2 active_offset)
    {
        if (output_mode_type == OutputModeType.absolute && absolute_output_mode != null)
        {
            var display = absolute_output_mode.Output;
            var offset = absolute_output_mode.Output.Position;
            var shiftoffX = offset.X - (display.Width / 2);
            var shiftoffY = offset.Y - (display.Height / 2);
            return new Vector2((input.X - shiftoffX - active_offset.X) / display.Width * 2 - 1, (input.Y - shiftoffY - active_offset.Y) / display.Height * 2 - 1);
        }

        try_resolve_output_mode();
        return default;
    }

    public Vector2 from_unit_screen(Vector2 input, Vector2 active_offset)
    {
        if (output_mode_type == OutputModeType.absolute && absolute_output_mode != null)
        {
            var display = absolute_output_mode.Output;
            var offset = absolute_output_mode.Output.Position;
            var shiftoffX = offset.X - (display.Width / 2);
            var shiftoffY = offset.Y - (display.Height / 2);
            return new Vector2((input.X + 1) / 2 * display.Width + shiftoffX + active_offset.X, (input.Y + 1) / 2 * display.Height + shiftoffY + active_offset.Y);
        }

        try_resolve_output_mode();
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
    private OutputModeType output_mode_type;
    private AbsoluteOutputMode absolute_output_mode;
    private RelativeOutputMode relative_output_mode;
    private void try_resolve_output_mode()
    {
        if (driver is Driver drv)
        {
            IOutputMode output = drv.InputDevices
                .Where(dev => dev?.OutputMode?.Elements?.Contains(this) ?? false)
                .Select(dev => dev?.OutputMode).FirstOrDefault();

            if (output is AbsoluteOutputMode abs_output) {
                absolute_output_mode = abs_output;
                output_mode_type = OutputModeType.absolute;
                return;
            }
            if (output is RelativeOutputMode rel_output) {
                relative_output_mode = rel_output;
                output_mode_type = OutputModeType.relative;
                return;
            }
            output_mode_type = OutputModeType.unknown;
        }
    }

    public abstract event Action<IDeviceReport> Emit;
    public abstract void Consume(IDeviceReport value);
    public abstract PipelinePosition Position { get; }
}

enum OutputModeType {
    absolute,
    relative,
    unknown
}