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
    public partial class StrategyTestForm : Form
    {
        private ILogger _logger;
        private ISettingsManager _settingsManager;
        private IOrderMaker _orderMaker;
        private ITimeManager _timeManager;

        #region  Helper
        List<TradeSettings> TradeSettings => _settingsManager.TradeSettings;
        bool HasTradeSettings => TradeSettings.HasItems();
        TradeSettings FindTradeSettings(string id) => _settingsManager.FindTradeSettings(id);
        #endregion

        public StrategyTestForm()
        {
            _settingsManager = Factories.CreateSettingsManager();
            _logger = LoggerFactory.Create(_settingsManager.LogFilePath);

            this._timeManager = ServiceFactory.CreateTimeManager(_settingsManager.GetSettingValue(AppSettingsKey.Begin),
                _settingsManager.GetSettingValue(AppSettingsKey.End));

            InitializeComponent();

            if (HasTradeSettings) this.tpStrategy.Controls.Add(UIHelpers.CreateLabel("策略設定", Color.Black, DockStyle.Fill), 0, 0);
            else this.tpStrategy.Controls.Add(UIHelpers.CreateLabel("您還沒有設定策略. 請先設定策略才可同步下單.", Color.Red, DockStyle.Fill), 0, 0);

            InitOrderMaker();
            InitStrategyUI();

        }

        void InitOrderMaker()
        {
            string name = _settingsManager.GetSettingValue(AppSettingsKey.OrderMaker);
            string ip = _settingsManager.GetSettingValue(AppSettingsKey.OrderMakerIP);
            string sid = _settingsManager.GetSettingValue(AppSettingsKey.SID);
            string pw = CryptoGraphy.DecryptCipherTextToPlainText(_settingsManager.GetSettingValue(AppSettingsKey.Password));

            _orderMaker = ProviderFactory.Create(name, ip, sid, pw);

        }

        #region  UI
        List<Uc_Strategy> _uc_StrategyList = new List<Uc_Strategy>();
        #endregion

        #region Event Handlers

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
        private void OnConfig_Changed(object sender, EventArgs e = null) => OnSettinsChanged();
        #endregion

        void InitStrategyUI()
        {
            if (!HasTradeSettings) return;

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
