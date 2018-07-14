# PowerGlue 
#### A tool to force PowerPoint to output on the projector display, regardless if Windows messes up the display number!

> PowerPoint stores the output display as `DisplayMonitor` in the registry. For example,  this value could be  `\\.\DISPLAY2` which could refer a projector you've got hooked up.
> 
> It turns out that this `\\.\DISPLAY2` identifier is **not** unique to the display itself! Windows can reassign it to some other physical display (say, a third monitor which isn't the main projector). Things like which order you power-on the devices can affect this.

PowerGlue is a tool which attempts to **always set the correct `DisplayMonitor` value.**

### Usage

1. Download the tool and save it somewhere - a config file will get created in this folder

2. Run the tool. Select which display is the main projector.

> Via a bunch of Windows API's and EDID info, the tool will retrieve the up-to-date `\\.\DISPLAYXXX` value and write it to PowerPoint's registry. Test it out - pressing F5 in PowerPoint should now show the output on your chosen display. (Changes shoud take effect immediately)

3. Enable the log-in task and event watcher

> This will run a tiny service in the system tray which will fire on log-in, and every time a Windows monitor change is detected.

### Options

`--silent` will hide the balloon message. You can manually add this to the arguments of the log-in event (in Task Scheduler).
