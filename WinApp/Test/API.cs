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
using Core;
using Core.Factories;
using Core.Helpers;
using Core.Logging;
using WinApp.UI;
using WinApp.Helpers;
using Core.Models;
using Core.Views;
using Core.Services;
using WinApp.Models;
using Core.Security;

namespace WinApp
{

    public partial class API_TestForm : Form
    {
        private ILogger _logger;
        private ISettingsManager _settingsManager;
        private IOrderMaker _orderMaker;

        #region  Helper
        List<TradeSettings> TradeSettings => _settingsManager.TradeSettings;
        TradeSettings FindTradeSettings(string id) => _settingsManager.FindTradeSettings(id);
        #endregion

        List<AccountView> _accounts;
        AccountView Account { get; set; }

        public API_TestForm()
        {
            _settingsManager = Factories.CreateSettingsManager();
            _logger = LoggerFactory.Create(_settingsManager.LogFilePath);


            string name = _settingsManager.GetSettingValue(AppSettingsKey.OrderMaker);
            string ip = _settingsManager.GetSettingValue(AppSettingsKey.OrderMakerIP);
            string sid = _settingsManager.GetSettingValue(AppSettingsKey.SID);
            string pw = CryptoGraphy.DecryptCipherTextToPlainText(_settingsManager.GetSettingValue(AppSettingsKey.Password));

            _orderMaker = ProviderFactory.Create(name, ip, sid, pw);
            _orderMaker.Ready += this.OnOrderMakerReady;
            _orderMaker.ActionExecuted += this.OnActionExcuted;
            if (_orderMaker.Name == ProviderName.HUA_NAN)
            {
                _orderMaker.AccountPositionUpdated += OrderMaker_AccountPositionUpdated;
            }

            InitializeComponent();
        }

        private void API_TestForm_Load(object sender, EventArgs e)
        {

            if (_orderMaker.Connectted) this.OnOrderMakerReady(null, null);
            else _orderMaker.Connect();
        }

        private void OnOrderMakerReady(object sender, EventArgs e)
        {
            _logger.LogInfo($"OrderMakerReady. Provider: {_orderMaker.Name}");

            InitUI();

            LoadAccounts();
        }

        void InitUI()
        {
            
        }


        void LoadAccounts()
        {
            _accounts = new List<AccountView>();
            foreach (var item in TradeSettings)
            {
                foreach (var acc in item.Accounts)
                {
                    _accounts.Add(acc.ToView(item));
                }
            }

            this.cbxAccount.Items.Clear();

            foreach (var item in _accounts)
            {
                this.cbxAccount.Items.Add(item.Number);
            }

            this.cbxAccount.SelectedIndex = 0;
        }

        private void cbxAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string accNumber = (string)this.cbxAccount.SelectedItem;

            Account = _accounts.FirstOrDefault(x => x.Number == accNumber);

            GetData();
        }


        void GetData()
        {
            lblSymbol.Text = Account.Symbol;

            if (_orderMaker.Name == ProviderName.HUA_NAN)
            {
                _orderMaker.RequestAccountPositions(Account.Number, Account.Symbol);
            }
            else
            {
                Account.OI = _orderMaker.GetAccountPositions(Account.Number, Account.Symbol);

                Render();
            }
           
        }

        void Render()
        {

            lblOI.Text = Account.OI.ToString();
            if (Account.OI > 0) lblOI.ForeColor = Color.Red;
            else if (Account.OI == 0) lblOI.ForeColor = Color.Black;
            else lblOI.ForeColor = Color.Green;


            lblSymbol.Text = Account.Symbol;

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetData();
        }

        private void OnActionExcuted(object sender, EventArgs e)
        {
            try
            {
                var args = e as ActionEventArgs;
                _logger.LogInfo($"ActionExcuted: {args.Action} , Code: {args.Code} , Msg: {args.Msg}");

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);

            }
        }

        private void OrderMaker_AccountPositionUpdated(object sender, EventArgs e)
        {
            try
            {
                var args = e as AccountEventArgs;
                string accountId = args.Account;

                if (Account.Number.EqualTo(args.Account))
                {
                    Account.OI = _orderMaker.GetAccountPositions(Account.Number, Account.Symbol);

                    Render();
                }

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);

            }
        }


        void MakeOrder(bool buy)
        {
            var price = txtPrice.Text.ToDecimal();
            if (price <= 0)
            {
                MessageBox.Show("價格錯誤");
                return;
            }

            int lots = buy ? Account.Lots : 0 - Account.Lots;

            int offset = Account.TradeSettings.Offset;

            if (lots > 0) _orderMaker.MakeOrder(Account.Symbol, Account.Number, price + offset, lots, Account.TradeSettings.DayTrade); //買進
            else if (lots < 0) _orderMaker.MakeOrder(Account.Symbol, Account.Number, price - offset, lots, Account.TradeSettings.DayTrade); //賣出

            GetData();
        }

        private void btnBuy_Click(object sender, EventArgs e) => MakeOrder(true);

        private void btnSell_Click(object sender, EventArgs e) => MakeOrder(false);


    }

}
