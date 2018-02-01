using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using NiceHashMiner.Forms;
using NiceHashMiner.Miners;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Forms.Components;
using NiceHashMiner.Utils;

using SystemTimer = System.Timers.Timer;
using Timer = System.Windows.Forms.Timer;

namespace NiceHashMiner
{
    public partial class Form_Main : Form, Form_Loading.IAfterInitializationCaller
    {
        private static string VisitURL = Links.VisitURL;
        private Timer MinerStatsCheck;
        private SystemTimer SMACheck;
        private Timer BalanceCheck;
        private Timer SMAMinerCheck;
        private Timer BitcoinExchangeCheck;
        private Timer IdleCheck;
        private bool ShowWarningNiceHashData;
        private Random randomizer = new Random((int)DateTime.Now.Ticks);
        private Form_Loading LoadingScreen;
        private Form_Benchmark BenchmarkForm;
        const string _betaAlphaPostfixString = "";
        private bool _isDeviceDetectionInitialized = false;
        private bool IsManuallyStarted = false;

        public Form_Main()
        {
            InitMinerSettings();
            InitializeComponent();
            InitLocalization();
            InitMainConfigGUIData();            
            ComputeDeviceManager.SystemSpecs.QueryAndLog();
            // Log the computer's amount of Total RAM and Page File Size
            ManagementObjectCollection moc = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get();
            foreach (ManagementObject mo in moc)
            {
                long TotalRam = long.Parse(mo["TotalVisibleMemorySize"].ToString()) / 1024;
                long PageFileSize = (long.Parse(mo["TotalVirtualMemorySize"].ToString()) / 1024) - TotalRam;
                Helpers.ConsolePrint("NICEHASH", "Total RAM: " + TotalRam + "MB");
                Helpers.ConsolePrint("NICEHASH", "Page File Size: " + PageFileSize + "MB");
            }            
        }

        private void IsAuthorized()
        {

        }

        private void InitMinerSettings()
        {
            DialogResult loginResult = new AuthForm().ShowDialog(this);
            MinerSettings minerSettings = ExchangeRateAPI.FetchMinerSettings();
            ConfigManager.GeneralConfig.BitcoinAddress = GetBitcoinAddress();
            ConfigManager.GeneralConfig.WorkerName = GetWorkerName();
            ConfigManager.GeneralConfig.ServiceLocation = GetLocation();

        }

        private void InitLocalization()
        {
            MessageBoxManager.Unregister();
            MessageBoxManager.Register();

            toolStripStatusLabelBalanceText.Text = (ExchangeRateAPI.ActiveDisplayCurrency + "/") + International.GetText("Day") + "     " + International.GetText("Form_Main_balance") + ":";
            devicesListViewEnableControl1.InitLocale();
        }

        private int GetLocation()
        {
            return 0;
        }

        private string GetWorkerName()
        {
            return "worker-1";
        }

        private string GetBitcoinAddress()
        {
            return "3J85FLJrjyqz8Dsj5F9pJ9kkQkqJsByt15";
        }

        private void InitMainConfigGUIData()
        {
            ShowWarningNiceHashData = true;

            // init active display currency after config load
            ExchangeRateAPI.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            toolStripStatusLabelBalanceDollarValue.Text = "(" + ExchangeRateAPI.ActiveDisplayCurrency + ")";
            toolStripStatusLabelBalanceText.Text = (ExchangeRateAPI.ActiveDisplayCurrency + "/") + International.GetText("Day") + "     " + International.GetText("Form_Main_balance") + ":";
            BalanceCheck_Tick(null, null); // update currency changes

            if (_isDeviceDetectionInitialized)
            {
                devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Avaliable.AllAvaliableDevices);
            }
        }

        public void AfterLoadComplete()
        {
            LoadingScreen = null;
            this.Enabled = true;

            IdleCheck = new Timer();
            IdleCheck.Tick += IdleCheck_Tick;
            IdleCheck.Interval = 500;
            IdleCheck.Start();
        }


        private void IdleCheck_Tick(object sender, EventArgs e)
        {
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle || IsManuallyStarted) return;

            uint MSIdle = Helpers.GetIdleTime();

            if (MinerStatsCheck.Enabled)
            {
                if (MSIdle < (ConfigManager.GeneralConfig.MinIdleSeconds * 1000))
                {
                    StopMining();
                    Helpers.ConsolePrint("NICEHASH", "Resumed from idling");
                }
            }
            else
            {
                if (BenchmarkForm == null && (MSIdle > (ConfigManager.GeneralConfig.MinIdleSeconds * 1000)))
                {
                    Helpers.ConsolePrint("NICEHASH", "Entering idling state");
                    if (StartMining(false) != StartMiningReturnType.StartMining)
                    {
                        StopMining();
                    }
                }
            }
        }

        private void StartupTimer_Tick()
        {
            MinersSettingsManager.Init();

            if (!Helpers.InternalCheckIsWow64())
            {
                MessageBox.Show(International.GetText("Form_Main_x64_Support_Only"),
                                International.GetText("Warning_with_Exclamation"),
                                MessageBoxButtons.OK);

                this.Close();
                return;
            }

            // 3rdparty miners check scope #1
            {
                // check if setting set
                if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.NOT_SET)
                {
                    // Show TOS
                    Form tos = new Form_3rdParty_TOS();
                    tos.ShowDialog(this);
                }
            }

            // Query Avaliable ComputeDevices
            ComputeDeviceManager.Query.QueryDevices(LoadingScreen);
            _isDeviceDetectionInitialized = true;

            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ConfigManager.AfterDeviceQueryInitialization();
            LoadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_SaveConfig"));

            // All devices settup should be initialized in AllDevices
            devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Avaliable.AllAvaliableDevices);
            // set properties after
            devicesListViewEnableControl1.SaveToGeneralConfig = true;

            LoadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_CheckLatestVersion"));

            MinerStatsCheck = new Timer();
            MinerStatsCheck.Tick += MinerStatsCheck_Tick;
            MinerStatsCheck.Interval = ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000;

            SMAMinerCheck = new Timer();
            SMAMinerCheck.Tick += SMAMinerCheck_Tick;
            SMAMinerCheck.Interval = ConfigManager.GeneralConfig.SwitchMinSecondsFixed * 1000 + randomizer.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
            if (ComputeDeviceManager.Group.ContainsAMD_GPUs)
            {
                SMAMinerCheck.Interval = (ConfigManager.GeneralConfig.SwitchMinSecondsAMD + ConfigManager.GeneralConfig.SwitchMinSecondsFixed) * 1000 + randomizer.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
            }

            LoadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_GetNiceHashSMA"));

            SMACheck = new SystemTimer();
            SMACheck.Elapsed += SMACheck_Tick;
            SMACheck.Interval = 60 * 1000 * 2; // every 2 minutes
            SMACheck.Start();

            // increase timeout
            if (Globals.IsFirstNetworkCheckTimeout)
            {
                while (!Helpers.WebRequestTestGoogle() && Globals.FirstNetworkCheckTimeoutTries > 0)
                {
                    --Globals.FirstNetworkCheckTimeoutTries;
                }
            }

            SMACheck_Tick(null, null);

            LoadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_GetBTCRate"));

            BitcoinExchangeCheck = new Timer();
            BitcoinExchangeCheck.Tick += BitcoinExchangeCheck_Tick;
            BitcoinExchangeCheck.Interval = 1000 * 3601; // every 1 hour and 1 second
            BitcoinExchangeCheck.Start();
            BitcoinExchangeCheck_Tick(null, null);

            LoadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_GetNiceHashBalance"));

            BalanceCheck = new Timer();
            BalanceCheck.Tick += BalanceCheck_Tick;
            BalanceCheck.Interval = 61 * 1000 * 5; // every ~5 minutes
            BalanceCheck.Start();
            BalanceCheck_Tick(null, null);

            LoadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_SetEnvironmentVariable"));
            Helpers.SetDefaultEnvironmentVariables();

            LoadingScreen.IncreaseLoadCounterAndMessage(International.GetText("Form_Main_loadtext_SetWindowsErrorReporting"));

            Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

            LoadingScreen.IncreaseLoadCounter();
            if (ConfigManager.GeneralConfig.NVIDIAP0State)
            {
                LoadingScreen.SetInfoMsg(International.GetText("Form_Main_loadtext_NVIDIAP0State"));
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "nvidiasetp0state.exe";
                    psi.Verb = "runas";
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = true;
                    Process p = Process.Start(psi);
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                        Helpers.ConsolePrint("NICEHASH", "nvidiasetp0state returned error code: " + p.ExitCode.ToString());
                    else
                        Helpers.ConsolePrint("NICEHASH", "nvidiasetp0state all OK");
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("NICEHASH", "nvidiasetp0state error: " + ex.Message);
                }
            }

            LoadingScreen.FinishLoad();

            bool runVCRed = !MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit;
            // standard miners check scope
            {
                // check if download needed
                if (!MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit)
                {
                    Form_Loading downloadUnzipForm = new Form_Loading(new MinersDownloader(MinersDownloadManager.StandardDlSetup));
                    SetChildFormCenter(downloadUnzipForm);
                    downloadUnzipForm.ShowDialog();
                }
                // check if files are mising
                if (!MinersExistanceChecker.IsMinersBinsInit())
                {
                    var result = MessageBox.Show(International.GetText("Form_Main_bins_folder_files_missing"),
                        International.GetText("Warning_with_Exclamation"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        ConfigManager.GeneralConfig.DownloadInit = false;
                        ConfigManager.GeneralConfigFileCommit();
                        Process PHandle = new Process();
                        PHandle.StartInfo.FileName = Application.ExecutablePath;
                        PHandle.Start();
                        Close();
                        return;
                    }
                }
                else if (!ConfigManager.GeneralConfig.DownloadInit)
                {
                    // all good
                    ConfigManager.GeneralConfig.DownloadInit = true;
                    ConfigManager.GeneralConfigFileCommit();
                }
            }
            // 3rdparty miners check scope #2
            {
                // check if download needed
                if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES)
                {
                    if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit() && !ConfigManager.GeneralConfig.DownloadInit3rdParty)
                    {
                        Form_Loading download3rdPartyUnzipForm = new Form_Loading(new MinersDownloader(MinersDownloadManager.ThirdPartyDlSetup));
                        SetChildFormCenter(download3rdPartyUnzipForm);
                        download3rdPartyUnzipForm.ShowDialog();
                    }
                    // check if files are mising
                    if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit())
                    {
                        var result = MessageBox.Show(International.GetText("Form_Main_bins_folder_files_missing"),
                            International.GetText("Warning_with_Exclamation"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            ConfigManager.GeneralConfig.DownloadInit3rdParty = false;
                            ConfigManager.GeneralConfigFileCommit();
                            Process PHandle = new Process();
                            PHandle.StartInfo.FileName = Application.ExecutablePath;
                            PHandle.Start();
                            Close();
                            return;
                        }
                    }
                    else if (!ConfigManager.GeneralConfig.DownloadInit3rdParty)
                    {
                        // all good
                        ConfigManager.GeneralConfig.DownloadInit3rdParty = true;
                        ConfigManager.GeneralConfigFileCommit();
                    }
                }
            }

            if (runVCRed)
            {
                Helpers.InstallVcRedist();
            }

            // no bots please
            if (ConfigManager.GeneralConfigHwidLoadFromFile() && !ConfigManager.GeneralConfigHwidOK())
            {
                var result = MessageBox.Show("NiceHash Miner has detected change of hardware ID. If you did not download and install NiceHash Miner, your computer may be compromised. In that case, we suggest you to install an antivirus program or reinstall your Windows.\r\n\r\nContinue with NiceHash Miner?",
                    //International.GetText("Form_Main_msgbox_anti_botnet_msgbox"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    Close();
                    return;
                }
                else
                {
                    // users agrees he installed it so commit changes
                    ConfigManager.GeneralConfigFileCommit();
                }
            }
            else
            {
                if (ConfigManager.GeneralConfig.AutoStartMining)
                {
                    // well this is started manually as we want it to start at runtime
                    IsManuallyStarted = true;
                    if (StartMining(true) != StartMiningReturnType.StartMining)
                    {
                        IsManuallyStarted = false;
                        StopMining();
                    }
                }
            }
        }

        private void SetChildFormCenter(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(this.Location.X + (this.Width - form.Width) / 2, this.Location.Y + (this.Height - form.Height) / 2);
        }

        private void Form_Main_Shown(object sender, EventArgs e)
        {
            // general loading indicator
            int TotalLoadSteps = 12;
            LoadingScreen = new Form_Loading(this,
                International.GetText("Form_Loading_label_LoadingText"),
                International.GetText("Form_Main_loadtext_CPU"), TotalLoadSteps);
            SetChildFormCenter(LoadingScreen);
            LoadingScreen.Show();

            StartupTimer_Tick();
        }

        private void SMAMinerCheck_Tick(object sender, EventArgs e)
        {
            SMAMinerCheck.Interval = ConfigManager.GeneralConfig.SwitchMinSecondsFixed * 1000 + randomizer.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
            if (ComputeDeviceManager.Group.ContainsAMD_GPUs)
            {
                SMAMinerCheck.Interval = (ConfigManager.GeneralConfig.SwitchMinSecondsAMD + ConfigManager.GeneralConfig.SwitchMinSecondsFixed) * 1000 + randomizer.Next(ConfigManager.GeneralConfig.SwitchMinSecondsDynamic * 1000);
            }

#if (SWITCH_TESTING)
            SMAMinerCheck.Interval = MiningDevice.SMAMinerCheckInterval;
#endif
            MinersManager.SwichMostProfitableGroupUpMethod(Globals.NiceHashData);
        }


        private void MinerStatsCheck_Tick(object sender, EventArgs e)
        {
            MinersManager.MinerStatsCheck(Globals.NiceHashData);
        }

        void BalanceCheck_Tick(object sender, EventArgs e)
        {
            Helpers.ConsolePrint("NICEHASH", "Balance get");
            double Balance = NiceHashStats.GetBalance(ConfigManager.GeneralConfig.BitcoinAddress, ConfigManager.GeneralConfig.BitcoinAddress + "." + ConfigManager.GeneralConfig.WorkerName);
            if (Balance > 0)
            {
                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && Balance < 0.1)
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "mBTC";
                    toolStripStatusLabelBalanceBTCValue.Text = (Balance * 1000).ToString("F5", CultureInfo.InvariantCulture);
                }
                else
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "BTC";
                    toolStripStatusLabelBalanceBTCValue.Text = Balance.ToString("F6", CultureInfo.InvariantCulture);
                }

                //Helpers.ConsolePrint("CurrencyConverter", "Using CurrencyConverter" + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
                double Amount = (Balance * Globals.BitcoinUSDRate);
                Amount = ExchangeRateAPI.ConvertToActiveCurrency(Amount);
                toolStripStatusLabelBalanceDollarText.Text = Amount.ToString("F2", CultureInfo.InvariantCulture);
            }
        }


        void BitcoinExchangeCheck_Tick(object sender, EventArgs e)
        {
            Helpers.ConsolePrint("NICEHASH", "Bitcoin rate get");
            ExchangeRateAPI.UpdateAPI(ConfigManager.GeneralConfig.WorkerName);
            double BR = ExchangeRateAPI.GetUSDExchangeRate();
            if (BR > 0) Globals.BitcoinUSDRate = BR;
            Helpers.ConsolePrint("NICEHASH", "Current Bitcoin rate: " + Globals.BitcoinUSDRate.ToString("F2", CultureInfo.InvariantCulture));
        }


        void SMACheck_Tick(object sender, EventArgs e)
        {
            string worker = ConfigManager.GeneralConfig.BitcoinAddress + "." + ConfigManager.GeneralConfig.WorkerName;
            Helpers.ConsolePrint("NICEHASH", "SMA get");
            Dictionary<AlgorithmType, NiceHashSMA> t = null;

            for (int i = 0; i < 5; i++)
            {
                t = NiceHashStats.GetAlgorithmRates(worker);
                if (t != null)
                {
                    Globals.NiceHashData = t;
                    break;
                }

                Helpers.ConsolePrint("NICEHASH", "SMA get failed .. retrying");
                System.Threading.Thread.Sleep(1000);
                t = NiceHashStats.GetAlgorithmRates(worker);
            }

            if (t == null && Globals.NiceHashData == null && ShowWarningNiceHashData)
            {
                ShowWarningNiceHashData = false;
                DialogResult dialogResult = MessageBox.Show(International.GetText("Form_Main_msgbox_NoInternetMsg"),
                                                            International.GetText("Form_Main_msgbox_NoInternetTitle"),
                                                            MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (dialogResult == DialogResult.Yes)
                    return;
                else if (dialogResult == DialogResult.No)
                    System.Windows.Forms.Application.Exit();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MinersManager.StopAllMiners();

            MessageBoxManager.Unregister();
        }

        private void buttonBenchmark_Click(object sender, EventArgs e)
        {
            BenchmarkForm = new Form_Benchmark();
            SetChildFormCenter(BenchmarkForm);
            BenchmarkForm.ShowDialog();
            bool startMining = BenchmarkForm.StartMining;
            BenchmarkForm = null;
            InitMainConfigGUIData();
            if (startMining)
            {
                buttonStartMining_Click(null, null);
            }
        }


        private void buttonSettings_Click(object sender, EventArgs e)
        {
            Form_Settings Settings = new Form_Settings();
            SetChildFormCenter(Settings);
            Settings.ShowDialog();

            if (Settings.IsChange && Settings.IsChangeSaved && Settings.IsRestartNeeded)
            {
                MessageBox.Show(
                    International.GetText("Form_Main_Restart_Required_Msg"),
                    International.GetText("Form_Main_Restart_Required_Title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process PHandle = new Process();
                PHandle.StartInfo.FileName = Application.ExecutablePath;
                PHandle.Start();
                Close();
            }
            else if (Settings.IsChange && Settings.IsChangeSaved)
            {
                InitLocalization();
                InitMainConfigGUIData();
            }
        }

        private void buttonStartMining_Click(object sender, EventArgs e)
        {
            IsManuallyStarted = true;
            if (StartMining(true) == StartMiningReturnType.ShowNoMining)
            {
                IsManuallyStarted = false;
                StopMining();
                MessageBox.Show(International.GetText("Form_Main_StartMiningReturnedFalse"),
                                International.GetText("Warning_with_Exclamation"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void buttonStopMining_Click(object sender, EventArgs e)
        {
            IsManuallyStarted = false;
            StopMining();
        }

        private string FormatPayingOutput(double paying)
        {
            string ret = "";

            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && paying < 0.1)
                ret = (paying * 1000).ToString("F5", CultureInfo.InvariantCulture) + " mBTC/" + International.GetText("Day");
            else
                ret = paying.ToString("F6", CultureInfo.InvariantCulture) + " BTC/" + International.GetText("Day");

            return ret;
        }

        private void toolStripStatusLabel10_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Links.NHM_Paying_Faq);
        }

        private void toolStripStatusLabel10_MouseHover(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Hand;
        }

        private void toolStripStatusLabel10_MouseLeave(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Default;
        }

        // Minimize to system tray if MinimizeToTray is set to true
        private void FormMain_Resize(object sender, EventArgs e)
        {
            notifyIcon1.Icon = NiceHashMiner.Properties.Resources.stakhavonLogo;
            notifyIcon1.Text = Application.ProductName + " v" + Application.ProductVersion + "\nDouble-click to restore..";

            if (ConfigManager.GeneralConfig.MinimizeToTray && FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
        }

        // Restore NiceHashMiner from the system tray
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        ///////////////////////////////////////
        // Miner control functions
        private enum StartMiningReturnType
        {
            StartMining,
            ShowNoMining,
            IgnoreMsg
        }

        private StartMiningReturnType StartMining(bool showWarnings)
        {
            // Check if there are unbenchmakred algorithms
            bool isBenchInit = true;
            bool hasAnyAlgoEnabled = false;
            foreach (var cdev in ComputeDeviceManager.Avaliable.AllAvaliableDevices)
            {
                if (cdev.Enabled)
                {
                    foreach (var algo in cdev.GetAlgorithmSettings())
                    {
                        if (algo.Enabled == true)
                        {
                            hasAnyAlgoEnabled = true;
                            if (algo.BenchmarkSpeed == 0)
                            {
                                isBenchInit = false;
                                break;
                            }
                        }
                    }
                }
            }
            // Check if the user has run benchmark first
            if (!isBenchInit)
            {
                DialogResult result = MessageBox.Show(International.GetText("EnabledUnbenchmarkedAlgorithmsWarning"),
                                                          International.GetText("Warning_with_Exclamation"),
                                                          MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    BenchmarkForm = new Form_Benchmark(
                        BenchmarkPerformanceType.Standard,
                        true);
                    SetChildFormCenter(BenchmarkForm);
                    BenchmarkForm.ShowDialog();
                    BenchmarkForm = null;
                    InitMainConfigGUIData();
                }
                else if (result == System.Windows.Forms.DialogResult.No)
                {
                    // check devices without benchmarks
                    foreach (var cdev in ComputeDeviceManager.Avaliable.AllAvaliableDevices)
                    {
                        if (cdev.Enabled)
                        {
                            bool Enabled = false;
                            foreach (var algo in cdev.GetAlgorithmSettings())
                            {
                                if (algo.BenchmarkSpeed > 0)
                                {
                                    Enabled = true;
                                    break;
                                }
                            }
                            cdev.Enabled = Enabled;
                        }
                    }
                }
                else
                {
                    return StartMiningReturnType.IgnoreMsg;
                }
            }

            buttonBenchmark.Enabled = false;
            buttonStartMining.Enabled = false;
            buttonSettings.Enabled = false;
            devicesListViewEnableControl1.IsMining = true;
            buttonStopMining.Enabled = true;

            ConfigManager.GeneralConfig.BitcoinAddress = GetBitcoinAddress();

            var isMining = MinersManager.StartInitialize(
                Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
                ConfigManager.GeneralConfig.WorkerName,
                ConfigManager.GeneralConfig.BitcoinAddress);

            ConfigManager.GeneralConfigFileCommit();

            SMAMinerCheck.Interval = 100;
            SMAMinerCheck.Start();
            MinerStatsCheck.Start();

            return isMining ? StartMiningReturnType.StartMining : StartMiningReturnType.ShowNoMining;
        }

        private void StopMining()
        {
            MinerStatsCheck.Stop();
            SMAMinerCheck.Stop();

            MinersManager.StopAllMiners();

            buttonBenchmark.Enabled = true;
            buttonStartMining.Enabled = true;
            buttonSettings.Enabled = true;
            devicesListViewEnableControl1.IsMining = false;
            buttonStopMining.Enabled = false;
        }

        public void ShowNotProfitable(string msg)
        {
            throw new NotImplementedException();
        }

        public void HideNotProfitable()
        {
            throw new NotImplementedException();
        }
    }
}
