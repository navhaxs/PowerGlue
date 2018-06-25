using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGlue.Models
{
    class Config
    {
        public static DisplayMeta LoadConfig()
        {
            var result = new DisplayMeta();

            IniFile ini = new IniFile(MainApp.INI_PATH);
            result.DeviceName = ini.IniReadValue("Match", "DeviceName");
            result.FriendlyName = ini.IniReadValue("Match", "FriendlyName");
            result.EDIDManufactureCode = ini.IniReadValue("Match", "EDIDManufactureCode");
            result.EDIDManufactureId = int.Parse(ini.IniReadValue("Match", "EDIDManufactureId"));
            result.EDIDProductCode = int.Parse(ini.IniReadValue("Match", "EDIDProductCode"));

            return result;
        }

        public static void WriteConfig(DisplayMeta displayMeta)
        {
            IniFile ini = new IniFile(MainApp.INI_PATH);
            ini.IniWriteValue("Match", "DeviceName", displayMeta.DeviceName);
            ini.IniWriteValue("Match", "FriendlyName", displayMeta.FriendlyName);
            ini.IniWriteValue("Match", "EDIDManufactureCode", displayMeta.EDIDManufactureCode);
            ini.IniWriteValue("Match", "EDIDManufactureId", displayMeta.EDIDManufactureId.ToString());
            ini.IniWriteValue("Match", "EDIDProductCode", displayMeta.EDIDProductCode.ToString());
        }
    }
}
