# Monitor Toggle Plugin for [OpenTabletDriver](https://github.com/OpenTabletDriver/OpenTabletDriver) [![](https://img.shields.io/github/downloads/Kuuuube/monitor_toggle/total.svg)](https://github.com/Kuuuube/monitor_toggle/releases/latest)

Bindings for switching between monitors.

## Quick Start Guide

- Enable the `Monitor Toggle` filter

- Click the `...` next to a binding and select `Monitor Toggle`

- Configure the settings for your binding

## Binding Settings

**Mode:** The monitor switching mode to use. Toggle, Hold, or Cycle.

Toggle activates when the binding is pressed and deactivates when the binding is pressed a second time.

Hold activates when the binding is pressed and deactivates when the binding is released.

Cycle activates when the binding is pressed and always stays active. When the binding is pressed again it changes to the next setting. Specify multiple settings for cycle mode by separating them with a comma. See [Cycle Mode](#cycle-mode) for setup instructions.

**Offset X:** The amount in pixels to offset the cursor's X position by.

**Offset Y:** The amount in pixels to offset the cursor's Y position by.

**Width Multiplier:** In absolute mode, the multiplier to change the monitor area's width by. In relative mode, the multiplier to change the X sensitivity by.

**Height Multiplier:** In absolute mode, the multiplier to change the monitor area's height by. In relative mode, the multiplier to change the Y sensitivity by.

## Cycle Mode

Cycle mode should be used with multiple sets of settings for each value. The values for each part of the cycle should be separated by a comma `,`.

For example, the following settings will cycle between (0,0) offset and (1,1) multiplier, (1920, 0) offset and (1,1) multiplier, (3840, 0) offset and (1,1) multiplier. It will then loop back to to (0,0) offset and (1,1) multiplier.

All values must be filled for each part of the cycle. Note that even though Offset Y, Width Multiplier, and Height Multiplier do not change, they are still included three times.

> [!WARNING]
> When specifying decimal values, only use `.` as a separator. Do not use `,`. (`0.1`, `1.7`, `123.123`, etc)

![](./cycle_example.png)

## Calculating Offsets and Multipliers

- In the OTD `Output` tab, right click the `Display` area, go to `Set to display`, and select one of the displays you need to toggle to. Note down the `Width`, `Height`, `X`, and `Y` shown by OTD.

    Repeat this step for all monitors you need to be able to switch to.

- Set your display area to whatever you want to be the default. Note down these values too.

- Calculate monitor toggle's values using the following formulas. `Default` will refer to the monitor you have set as default in the OTD display settings and `Toggle` will refer to the monitor being toggled to:

    - `Offset X` = `Toggle_X - Default_X`

    - `Offset Y` = `Toggle_Y - Default_Y`

    - `Width Multiplier` = `Toggle_Width / Default_Width`

    - `Height Multiplier` = `Toggle_Height / Default_Height`

    If you are using a cycle bind, base ALL monitors off the same default.
