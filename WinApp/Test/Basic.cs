using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Factories;
using Core.Helpers;
using Core.Logging;
using WinApp.UI;
using WinApp.Helpers;
using Core.Models;
using Core;
using Core.Services;
using Core.Security;

namespace WinApp.Test
{
    public partial class BasicTestForm : Form
    {
        private ILogger _logger;
        private ISettingsManager _settingsManager;
        private ITimeManager _timeManager;
        private IOrderMaker _orderMaker;

        bool _closed = false;

        #region  Helper
        bool _basicSettingOK = false;
        bool CheckBasicSettings() => _basicSettingOK = _settingsManager.CheckBasicSetting();
        #endregion

        #region  UI
        UcStatus ucStatus;
        #endregion

        public BasicTestForm()
        {
            _settingsManager = Factories.CreateSettingsManager();
            _logger = LoggerFactory.Create(_settingsManager.LogFilePath);

            this._timeManager = ServiceFactory.CreateTimeManager(_settingsManager.GetSettingValue(AppSettingsKey.Begin),
                _settingsManager.GetSettingValue(AppSettingsKey.End));

            InitializeComponent();

            CheckBasicSettings();
            if (_basicSettingOK)
            {
                InitOrderMaker();
            }
            else
            {
                OnEditConfig(null, null);
            }

            InitBasicUI();
            InitStatusUI();
        }

        void InitOrderMaker()
        {
            string name = _settingsManager.GetSettingValue(AppSettingsKey.OrderMaker);
            string ip = _settingsManager.GetSettingValue(AppSettingsKey.OrderMakerIP);
            string sid = _settingsManager.GetSettingValue(AppSettingsKey.SID);
            string pw = CryptoGraphy.DecryptCipherTextToPlainText(_settingsManager.GetSettingValue(AppSettingsKey.Password));

            _orderMaker = ProviderFactory.Create(name, ip, sid, pw);

            _orderMaker.Ready += OnOrderMakerReady;
            _orderMaker.ConnectionStatusChanged += OrderMaker_ConnectionStatusChanged;
            //_orderMaker.ExceptionHappend += OnOrderMakerExceptionHappend;

            //_orderMaker.ActionExecuted += OrderMaker_ActionExecuted;

        }

        #region Form Event Handlers
        private void BasicTestForm_Load(object sender, EventArgs e)
        {
            if (_orderMaker.Connectted) this.OnOrderMakerReady(null, null);
            else _orderMaker.Connect();
        }
        #endregion


        #region Event Handlers
        private void OnOrderMakerReady(object sender, EventArgs e)
        {
            _logger.LogInfo($"OrderMakerReady. Provider: {_orderMaker.Name}");
        }

        private void OnEditConfig(object sender, EventArgs e)
        {
            this.editConfig = new EditConfig(_settingsManager, _timeManager);
            this.editConfig.ConfigChanged += this.OnConfig_Changed;

            this.editConfig.ShowDialog();
        }

        


        private void OnConfig_Changed(object sender, EventArgs e = null) => OnSettinsChanged();

        private void btnLogs_Click(object sender, EventArgs e)
        {
            Process.Start(_logger.FilePath);
        }

        private void OrderMaker_ConnectionStatusChanged(object sender, EventArgs e)
        {
            if (_closed) return;

            try
            {

                if (ucStatus == null) return;
                var args = e as ConnectionStatusEventArgs;
                if(args.Status != ConnectionStatus.CONNECTING) ucStatus.CheckConnect();

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);

            }
        }

        
        #endregion



        void InitBasicUI()
        {

            if (_basicSettingOK) this.tpTop.Controls.Add(UIHelpers.CreateLabel("基本設定", Color.Black, DockStyle.Fill), 0, 0);
            else this.tpTop.Controls.Add(UIHelpers.CreateLabel("您還沒有完成基本設定", Color.Red, DockStyle.Fill), 0, 0);

        }
        void InitStatusUI()
        {
            this.ucStatus = new UcStatus(_settingsManager, _timeManager, _orderMaker, _logger);
            this.panel1.Controls.Add(this.ucStatus);
        }


        void OnSettinsChanged()
        {
            MessageBox.Show("設定檔已變更, 程式將重新啟動.");
            Application.ExitThread();
            ReStart();
        }

        void ReStart()
        {
            Thread thtmp = new Thread(new ParameterizedThreadStart(Run));
            object appName = Application.ExecutablePath;
            Thread.Sleep(3000);
            thtmp.Start(appName);

        }

        private void Run(Object obj)
        {
            Process ps = new Process();
            ps.StartInfo.FileName = obj.ToString();
            ps.Start();

        }

        private void BasicTestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _closed = true;

            if (_orderMaker != null) _orderMaker.DisConnect();

            Thread.Sleep(1500);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (_orderMaker != null) _orderMaker.DisConnect();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            Process.Start(_logger.FilePath);
        }
    }
}
