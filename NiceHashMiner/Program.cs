using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NiceHashMiner.Utils;
using NiceHashMiner.Configs;
using NiceHashMiner.Forms;
using NiceHashMiner.Enums;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace NiceHashMiner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            //Console.OutputEncoding = System.Text.Encoding.Unicode;
            // #0 set this first so data parsing will work correctly
            Globals.JsonSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
            };

            // #1 first initialize config
            ConfigManager.InitializeConfig();

            // #2 check if multiple instances are allowed
            bool startProgram = true;
            if (ConfigManager.GeneralConfig.AllowMultipleInstances == false) {
                try {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName)) {
                        if (process.Id != current.Id) {
                            startProgram = false;
                        }
                    }
                } catch { }
            }

            if (startProgram) {
                if (ConfigManager.GeneralConfig.LogToFile) {
                    Logger.ConfigureWithFile();
                }

                if (ConfigManager.GeneralConfig.DebugConsole) {
                    Helpers.AllocConsole();
                }

                // init active display currency after config load
                ExchangeRateAPI.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

                // #2 then parse args
                var commandLineArgs = new CommandLineParser(argv);

                Helpers.ConsolePrint("NICEHASH", "Starting up NiceHashMiner v" + Application.ProductVersion);
                bool tosChecked = ConfigManager.GeneralConfig.agreedWithTOS == Globals.CURRENT_TOS_VER;
                if (!tosChecked || !ConfigManager.GeneralConfigIsFileExist() && !commandLineArgs.IsLang) {
                    Helpers.ConsolePrint("NICEHASH", "No config file found. Running NiceHash Miner for the first time. Choosing a default language.");
                    Application.Run(new Form_ChooseLanguage());
                }

                // Init languages
                International.Initialize(ConfigManager.GeneralConfig.Language);

                if (commandLineArgs.IsLang) {
                    Helpers.ConsolePrint("NICEHASH", "Language is overwritten by command line parameter (-lang).");
                    International.Initialize(commandLineArgs.LangValue);
                    ConfigManager.GeneralConfig.Language = commandLineArgs.LangValue;
                }

                // check WMI
                if (Helpers.IsWMIEnabled()) {
                    if (ConfigManager.GeneralConfig.agreedWithTOS == Globals.CURRENT_TOS_VER) {
                        if (InitMinerSettings())
                        {
                            Application.Run(new Form_Main());
                        }                        
                    }
                }
                else {
                    MessageBox.Show(International.GetText("Program_WMI_Error_Text"),
                                                            International.GetText("Program_WMI_Error_Title"),
                                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static bool InitMinerSettings()
        {
            if (ConfigManager.GeneralConfig.AuthDetails == null)
            {
                AuthForm authForm = new AuthForm();
                DialogResult result = authForm.ShowDialog();
                if (result == DialogResult.Cancel)
                {
                    return false;
                }
                ConfigManager.GeneralConfig.AuthDetails = authForm.AuthDetails;
                ConfigManager.GeneralConfig.WorkerName = authForm.AuthDetails.User.Username;
            }
            MinerSettings minerSettings = ExchangeRateAPI.FetchMinerSettings();
            ConfigManager.GeneralConfig.BitcoinAddress = minerSettings.BitcoinAddress;
            ConfigManager.GeneralConfig.ServiceLocation = 0;
            return true;
        }

    }
}
