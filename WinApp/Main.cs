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


namespace WinApp
{
    public partial class Main : Form
    {
        private ILogger _logger;
        private ISettingsManager _settingsManager;
        private ITimeManager _timeManager;
        private IOrderMaker _orderMaker;

        bool _closed = false;

        #region  Helper
        bool _basicSettingOK = false;
        bool CheckBasicSettings() => _basicSettingOK = _settingsManager.CheckBasicSetting();
        
        List<TradeSettings> TradeSettings => _settingsManager.TradeSettings;
        bool HasTradeSettings => TradeSettings.HasItems();
        TradeSettings FindTradeSettings(string id) => _settingsManager.FindTradeSettings(id);
        #endregion

        #region  UI
        UcStatus ucStatus;
        List<Uc_Strategy> _uc_StrategyList = new List<Uc_Strategy>();
        #endregion

        public Main()
        {
            _settingsManager = Factories.CreateSettingsManager();
            _logger = LoggerFactory.Create(_settingsManager.LogFilePath);

            this._timeManager = ServiceFactory.CreateTimeManager(_settingsManager.GetSettingValue(AppSettingsKey.Begin),
                _settingsManager.GetSettingValue(AppSettingsKey.End));

            InitializeComponent();

            CheckBasicSettings();
            InitBasicUI();
            if (!_basicSettingOK)
            {
                OnEditConfig(null, null);
                return;
            }

            InitOrderMaker();

            if (!HasTradeSettings) this.tpStrategy.Controls.Add(UIHelpers.CreateLabel("您還沒有設定策略. 請先設定策略才可同步下單.", Color.Red, DockStyle.Fill), 0, 0);            
            else this.tpStrategy.Controls.Add(UIHelpers.CreateLabel("策略設定", Color.Black, DockStyle.Fill), 0, 0);
            
            InitStrategyUI();
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
            _orderMaker.ExceptionHappend += OnOrderMakerExceptionHappend;
            _orderMaker.ActionExecuted += OrderMaker_ActionExecuted;

        }

        #region OrderMaker Event Handlers
        private void OnOrderMakerReady(object sender, EventArgs e)
        {
            _logger.LogInfo($"OrderMakerReady. Provider: {_orderMaker.Name}");
        }
        private void OnOrderMakerExceptionHappend(object sender, EventArgs e)
        {
            try
            {
                var args = e as ExceptionEventArgs;
                _logger.LogException(args.Exception);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);

            }
        }
        private void OrderMaker_ActionExecuted(object sender, EventArgs e)
        {
            try
            {
                var args = e as ActionEventArgs;
                if (String.IsNullOrEmpty(args.Action)) _logger.LogInfo($"{args.Code} - {args.Msg}");
                else _logger.LogInfo($"{args.Action} - {args.Code} - {args.Msg}");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);

            }


        }
        private void OrderMaker_ConnectionStatusChanged(object sender, EventArgs e)
        {
            if (_closed) return;

            try
            {

                if (ucStatus == null) return;
                var args = e as ConnectionStatusEventArgs;
                if (args.Status != ConnectionStatus.CONNECTING) ucStatus.CheckConnect();

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);

            }
        }
        #endregion

        #region Event Handlers
        private void Main_Load(object sender, EventArgs e)
        {
            if (_orderMaker != null)
            {
                InitStatusUI();
                _orderMaker.Connect();
            } 
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            _closed = true;

            if (_orderMaker != null) _orderMaker.DisConnect();

            Thread.Sleep(1500);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (_orderMaker != null) _orderMaker.DisConnect();
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
        
        private void OnAddStrategy(object sender, EventArgs e)
        {
            this.editStrategy = new EditStrategy();
            this.editStrategy.Init(new TradeSettings());
            this.editStrategy.OnSave += this.OnSaveStrategy;

            this.editStrategy.ShowDialog();
        }

        private void OnEditStrategy(object sender, EventArgs e)
        {
            try
            {
                var args = e as EditStrategyEventArgs;
                var tradeSettings = FindTradeSettings(args.TradeSettings.Id);

                var clone = tradeSettings.DeepCloneByJson();

                this.editStrategy = new EditStrategy();
                this.editStrategy.Init(clone);
                this.editStrategy.OnSave += this.OnSaveStrategy;
                this.editStrategy.OnRemove += this.OnRemoveStrategy;

                this.editStrategy.ShowDialog();

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Show("修改策略設定失敗");
            }
        }

        private void OnRemoveStrategy(object sender, EventArgs e)
        {
            try
            {
                var args = e as EditStrategyEventArgs;

                _settingsManager.AddUpdateTradeSettings(args.TradeSettings);

                OnSettinsChanged();

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Show("刪除策略失敗");
            }
        }

        private void OnSaveStrategy(object sender, EventArgs e)
        {
            try
            {
                var args = e as EditStrategyEventArgs;
                var tradeSettings = args.TradeSettings;

                _settingsManager.AddUpdateTradeSettings(tradeSettings);

                OnSettinsChanged();

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Show("儲存策略設定失敗");
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
        void InitStrategyUI()
        {
            this._uc_StrategyList = new List<Uc_Strategy>();
            for (int i = 0; i < TradeSettings.Count; i++)
            {
                var uc_Strategy = new Uc_Strategy(_orderMaker, _settingsManager.FindTradeSettings(TradeSettings[i].Id), _timeManager, _logger);
                uc_Strategy.OnEdit += new System.EventHandler(this.OnEditStrategy);


                int height = uc_Strategy.ClientSize.Height;
                this._uc_StrategyList.Add(uc_Strategy);


                fpanelStrategies.Height += height + 3;
                this.fpanelStrategies.Controls.Add(uc_Strategy);
                fpanelStrategies.Controls.SetChildIndex(uc_Strategy, 0);
            }


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

        

        
    }
}
