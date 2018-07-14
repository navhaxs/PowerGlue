namespace PowerGlue.Models
{
    class Config
    {
        public static bool GetWacherEnabled()
        {
            IniFile ini = new IniFile(Constants.INI_PATH);
            if (bool.TryParse(ini.IniReadValue("Monitor", "Enabled"), out bool result))
            {
                return result;
            }
            return false;
        }

        public static void SetWacherEnabled(bool value)
        {
            IniFile ini = new IniFile(Constants.INI_PATH);
            ini.IniWriteValue("Monitor", "Enabled", value.ToString());
        }

        public static DisplayMeta LoadConfig()
        {
            var result = new DisplayMeta();

            IniFile ini = new IniFile(Constants.INI_PATH);
            result.DeviceName = ini.IniReadValue("MatchCriteria", "DeviceName");
            result.FriendlyName = ini.IniReadValue("MatchCriteria", "FriendlyName");
            result.EDIDManufactureCode = ini.IniReadValue("MatchCriteria", "EDIDManufactureCode");
            if (int.TryParse(ini.IniReadValue("MatchCriteria", "EDIDManufactureId"), out int EDIDManufactureId))
            {
                result.EDIDManufactureId = EDIDManufactureId;
            }
            if (int.TryParse(ini.IniReadValue("MatchCriteria", "EDIDProductCode"), out int EDIDProductCode))
            {
                result.EDIDProductCode = EDIDProductCode;
            }

            return result;
        }

        public static void WriteConfig(DisplayMeta displayMeta)
        {
            IniFile ini = new IniFile(Constants.INI_PATH);
            ini.IniWriteValue("MatchCriteria", "DeviceName", displayMeta.DeviceName);
            ini.IniWriteValue("MatchCriteria", "FriendlyName", displayMeta.FriendlyName);
            ini.IniWriteValue("MatchCriteria", "EDIDManufactureCode", displayMeta.EDIDManufactureCode);
            ini.IniWriteValue("MatchCriteria", "EDIDManufactureId", displayMeta.EDIDManufactureId.ToString());
            ini.IniWriteValue("MatchCriteria", "EDIDProductCode", displayMeta.EDIDProductCode.ToString());
        }
    }
}
