using Core.Helpers;
using Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Core
{
    public class FakeOrderMaker : BaseOrderMaker, IOrderMaker
    {
        public ProviderName Name => ProviderName.FAKE;

        FakeAPI _fakeAPI;

        public FakeOrderMaker(string ip, string sid, string password) : base(ip, sid, password)
        {
            _fakeAPI = new FakeAPI();
        }

        System.Windows.Forms.Form _form = null;
        public void SetUIForm(System.Windows.Forms.Form form) => _form = form;


        #region  IOrderMaker
        public bool Connectted => _fakeAPI.CheckConnect();

        public void Connect() => Login();

        public void DisConnect() => Logout();

        public void RequestAccountPositions(string accountId, string symbol = "") { }

        public override int GetAccountPositions(string accountId, string symbol) 
            => _fakeAPI.GetAccountPositions(accountId, symbol);

        public void MakeOrder(string symbol, string accountNumber, decimal price, int lots, bool dayTrade)
        {
            //大台 TXF 小台 MXF
            string productId = GetProductId(symbol);

            string bs = lots > 0 ? "B" : "S";
            string strPrice = price > 0 ? Convert.ToInt32(price).ToString() : "0";
            string qty = Math.Abs(lots).ToString();

            //OnAPIPositionData(accountNumber, productId, bs, qty);

            OnActionExecuted($"MakeOrder: {bs} {qty} {productId} , price: {strPrice} ");
        }

        public string ClearOrders(string symbol, string account)
        {
            throw new NotImplementedException();
        }

        public void FetchDeals(string account)
        {
            throw new NotImplementedException();
        }


        #endregion

        private void OnAPILoginStatusChanged(string msg)
        {
            if (String.IsNullOrEmpty(msg))
            {
                SetConnectionStatus(ConnectionStatus.CONNECTED);

                //取得帳號
                InitAccounts(_fakeAPI.GetAccounts());
                SetReady(true);
            } 
            else SetConnectionStatus(ConnectionStatus.DISCONNECTED);


            OnActionExecuted($"OnAPILoginStatusChanged: {msg}");
        }

        #region  API Functions
        void Login()
        {
            SetConnectionStatus(ConnectionStatus.CONNECTING);

            string result = _fakeAPI.Login();
            OnAPILoginStatusChanged(result);
            

            OnActionExecuted("Login", "", result);
        }
        void Logout()
        {
            string result = _fakeAPI.Logout();
            OnAPILoginStatusChanged(result);

            OnActionExecuted("Logout");
        }
        #endregion


        

        void OnActionExecuted(string action, string code = "", string msg = "")
           => OnActionExecuted(new ActionEventArgs(action, code, msg));




    }



    public class FakeAPI
    {
        Random rnd = new Random();

        public int GetAccountPositions(string account, string symbol) => rnd.Next(-9, 10);

        bool _connected = false;

        public string Login()
        {
            _connected = true;
            return "";
        } 
        public string Logout()
        {
            _connected = false;
            return "Logout";
        }
        public bool CheckConnect() => _connected;

        public List<string> GetAccounts()
        {
            return new List<string>() { "54545454", "1234567" };
        }
    }
}
