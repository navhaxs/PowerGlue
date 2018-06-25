using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerGlue.Models
{
    [Serializable]
    public class DisplayMeta
    {
        public string FriendlyName; // e.g. DELL U2515H (may be empty string e.g. some inbuilt LCD displays)
        public string DeviceName; // e.g. DELL U2515H(DisplayPort), Wide viewing angle & High density FlexView Display 1920x1080
        public string EDIDManufactureCode; // e.g. LEN
        public int? EDIDManufactureId; // e.g.44592
        public int? EDIDProductCode; // e.g. 16547
        public string DevicePath; // Really long string in windows registry.

        public string GetDisplayName()
        {
            if (!String.IsNullOrEmpty(FriendlyName) && !String.IsNullOrEmpty(DeviceName))
            {
                return $"{FriendlyName} (Device Name: {DeviceName})";
            }
            else if (String.IsNullOrEmpty(FriendlyName) && !String.IsNullOrEmpty(DeviceName))
            {
                return $"{DeviceName}";
            }
            else if (!String.IsNullOrEmpty(FriendlyName) && String.IsNullOrEmpty(DeviceName))
            {
                return $"{FriendlyName}";
            }
            else
            {
                return null;
            }
        }
    }

    public static class DisplayHelper
    {
        public static Dictionary<string, DisplayMeta> GetDisplays()
        {
            var result = new Dictionary<string, DisplayMeta>();

            // Get Friendly Names
            var targets = WindowsDisplayAPI.DisplayConfig.PathDisplayTarget.GetDisplayTargets();
            targets.ToList().ForEach((d) =>
            {
                var newInfo = new DisplayMeta() {
                    FriendlyName = d.FriendlyName,
                    EDIDManufactureCode = d.EDIDManufactureCode,
                    EDIDManufactureId = d.EDIDManufactureId,
                    EDIDProductCode = d.EDIDProductCode
                };
                result.Add(d.DevicePath, newInfo);
            });

            // Get Device Names
            var displays = WindowsDisplayAPI.Display.GetDisplays();
            displays.ToList().ForEach((d) =>
            {
                if (result.ContainsKey(d.DevicePath))
                {
                    result[d.DevicePath].DeviceName = d.DeviceName;
                }
            });

            return result;
        }

        public static WindowsDisplayAPI.Display LookupFromPath(string devicePath)
        {
            return LookupFromMatch(new DisplayMeta() { DevicePath = devicePath });
        }

        public static WindowsDisplayAPI.Display LookupFromMatch(DisplayMeta matchArgs)
        {
            string targetDisplayPath = null;
            if (matchArgs.FriendlyName != null)
            {
                // Filter for a display that contains the friendly name string
                var filteredDisplays = WindowsDisplayAPI.DisplayConfig.PathDisplayTarget.GetDisplayTargets()
                    .ToList()
                    .Where((x) => {

                        var results = new List<bool>();

                        if (!String.IsNullOrEmpty(matchArgs.FriendlyName))
                        {
                            results.Add(x.FriendlyName == matchArgs.FriendlyName);
                        }

                        if (!String.IsNullOrEmpty(matchArgs.EDIDManufactureCode))
                        {
                            results.Add(x.EDIDManufactureCode == matchArgs.EDIDManufactureCode);
                        }

                        if (matchArgs.EDIDManufactureId != null)
                        {
                            results.Add(x.EDIDManufactureId == matchArgs.EDIDManufactureId);
                        }

                        if (matchArgs.EDIDProductCode != null)
                        {
                            results.Add(x.EDIDProductCode == matchArgs.EDIDProductCode);
                        }

                        return results.All((y) => y == true);
                    });

                if (filteredDisplays.Count() == 0)
                    throw new Exception("No matching friendly name");

                // Get the device path of the target (e.g. \\?\DISPLAY#LEN40A3#4&24eea73b&0&UID265988#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7})
                targetDisplayPath = filteredDisplays.First().DevicePath;

            }

            // Lookup the display name (something like \\.\DISPLAY1 ) from the device path
            var match = WindowsDisplayAPI.Display.GetDisplays()
                .ToList()
                .Where((x) => {

                    var results = new List<bool>();

                    if (!String.IsNullOrEmpty(targetDisplayPath))
                    {
                        results.Add(x.DevicePath == targetDisplayPath);
                    }

                    if (!String.IsNullOrEmpty(matchArgs.DeviceName))
                    {
                        results.Add(x.DeviceName == matchArgs.DeviceName);
                    }

                    if (!String.IsNullOrEmpty(matchArgs.DevicePath))
                    {
                        results.Add(x.DevicePath == matchArgs.DevicePath);
                    }

                    return results.All((y) => y == true);
                });

            if (match.Count() == 0)
                throw new Exception("Did not find display with corresponding display path");

            return match.First();
        }
    }

}
