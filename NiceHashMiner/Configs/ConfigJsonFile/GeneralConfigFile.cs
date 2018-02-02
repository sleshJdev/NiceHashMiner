using NiceHashMiner.Configs.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner.Configs.ConfigJsonFile {
    public class GeneralConfigFile : AbstractConfigFile<GeneralConfig> {
        public GeneralConfigFile()
            : base(FOLDERS.CONFIG, "General.json", "General_old.json") {
        }
    }
}
