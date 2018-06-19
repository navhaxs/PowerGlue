# PowerGlue
Force PowerPoint to output on the display you want it to output on.

### What does this solve?

Sometimes PowerPoint unexpectedly outputs onto the wrong monitor. 

Say you more than two displays on your Windows machine: the laptop screen, one output to a projector, and an extra third monitor. You *really* want the PowerPoint to output on the projector - but for some reason, **occasionally** PowerPoint decides to output on wrong monitor instead.

This tool fixes that behaviour.

### How to use

Download the tool and save it somewhere with write access to that folder (it writes a  config file).

Run the tool. When you select a monitor, the changes will get applied.

Test that the changes work by starting a PowerPoint presentation and making sure that the output appears on the display you expect.

Hit 'Enable Auto-start' so the tool will re-apply your chosen preference everytime someone logs onto the PC.

### Technical info for the curious

PowerPoint stores the DisplayMonitor in the registry, for example the value `\\.\DISPLAY2` which might happen to refer to some `DELL` monitor you've got hooked up.

This `\\.\DISPLAY2` identifier is **not** unique to the display.

If you were to mess around with the Windows display settings, re-attach displays in a different order and change around with cloned/extended arrangements, `\\.\DISPLAY2` might end up identifying say your other `ViewSonic` monitor.

Now the next time you launch PowerPoint, it will use the `ViewSonic` monitor instead - which isn't what you originally expected!

### How this tool works

After select your favourite output monitor with the tool GUI, this tool will store the monitor name string (e.g. `DELL U2515H`).

It will use this string to figure out what the corresponding  `\\.\DISPLAYxxx` identifier happens to be for that day, and writes it the corresponding PowerPoint registry key.

The tool is designed to re-apply the above procedure on login. A task is created in task scheduler for the current user to execute `PowerGlue.exe --startup`  (the silent mode flag for the tool).

### 
