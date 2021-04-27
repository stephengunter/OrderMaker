using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Concord.API.Future.Client;
using Concord.API.Future.Client.OrderFormat;
using Core.Helpers;

namespace Core
{
	public class ConcordOrderMaker : BaseOrderMaker, IOrderMaker
	{
		public ProviderName Name => ProviderName.CONCORD;

		#region Consts
		private const string LOG_IN = "LOG_IN";
		private const string LOG_OUT = "LOG_OUT";
		private const string LOGIN_SUCCESS = "102";  //登入OK
		private const string ACCESS_FAIL = "201"; //登入主機無法連線
		private const string NOT_LOGIN = "202";  //尚未登入
		private const string CONNECTION_LOST = "210";  //下單連線中斷
		private const string LOGIN_FAILED = "211";  //下單登入驗證失敗
        #endregion

        private ucClient _API = new ucClient();
		string _strMsgCode = "";
		string _strMsg = "";

        string _bhno = "000";
		

		public ConcordOrderMaker(string ip, string sid, string password) : base(ip, sid, password)
		{
			_API.OnFGeneralReport += new ucClient.dlgFGeneralReport(API_OnFGeneralReport);
			_API.OnFErrorReport += new ucClient.dlgFErrorReport(API_OnFErrorReport);
			_API.OnFOrderReport += new ucClient.dlgFOrderReport(API_OnFOrderReport);

            
		}

		private void API_OnFGeneralReport(string strMsgCode, string strMsg)
		{
			OnActionExecuted(new ActionEventArgs("FGeneralReport", strMsgCode, strMsg));
		}
		private void API_OnFErrorReport(string strMsgCode, string strMsg)
		{
			OnActionExecuted(new ActionEventArgs("FErrorReport", strMsgCode, strMsg));
		}
		private void API_OnFOrderReport(string strMsgCode, string strMsg)
		{
			OnActionExecuted(new ActionEventArgs("FOrderRepor", strMsgCode, strMsg));
		}


        FOrderNew InitOrder(string symbol, string account, decimal price, int lots, bool dayTrade)
        {
			//大台 TXF 小台 MXF
			string productId = GetProductId(symbol);
            string rtype = dayTrade ? "2" : "";  // "0":新倉, "1":平倉, "2":期貨當沖, "":自動
			//string fir = price > 0 ? "R" : "I";  //R:ROD, F:FOK, I:IOC
			string fir = "I";  //R:ROD, F:FOK, I:IOC
			string otype = price > 0 ? "L" : "P";  // L:限價, M:市價, P:一定範圍市價

            return new FOrderNew
            {
                bhno = _bhno,
                cseq = account,
                mtype = "F", //F:期貨, O:選擇權
                sflag = "1", //期貨單式
                commo = productId,             
                fir = fir,
                rtype = rtype,
                otype = otype,
                bs = lots > 0 ? "B" : "S",
                price = price > 0 ? price : 0,
                qty = Math.Abs(lots)
            };
        }


		void OnActionExecuted(string action, string result = "")
		{
			if (String.IsNullOrEmpty(result)) result = _strMsgCode;

			if (result == ACCESS_FAIL || result == NOT_LOGIN) // 201 登入主機無法連線 202 尚未登入
			{
				SetConnectionStatus(ConnectionStatus.DISCONNECTED);
			}
			else if (_strMsgCode == CONNECTION_LOST || result == LOGIN_FAILED) // 下單連線中斷 , 登入驗證失敗
			{
				SetConnectionStatus(ConnectionStatus.DISCONNECTED);
			}

			OnActionExecuted(new ActionEventArgs(action, this._strMsgCode, this._strMsg));
		}

		public void Connect() => Login();

		public void DisConnect() => Logout();

		public bool Connectted
		{
			get
			{
				CheckConnect();
				return _strMsgCode == LOGIN_SUCCESS;
			}
		}

		

		public void MakeOrder(string symbol, string account, decimal price, int lots, bool dayTrade)
		{
            var order = InitOrder(symbol, account, price, lots, dayTrade);

            string strGUID = Guid.NewGuid().ToString();
            _strMsgCode = _API.FOrderNew(order, out _strMsg, ref strGUID);

            OnActionExecuted("MakeOrder");
        }

		public override int GetAccountPositions(string account, string symbol)
		{
			//925 - 無符合之資料
			_strMsgCode = _API.FQueryPosition(_bhno, account, "", "", "3", out _strMsg);
			OnActionExecuted("GetAccountPositions");

			if (_strMsgCode == "925") return 0;

			symbol = GetSymbolCode(symbol);

			if (_strMsgCode == "000")
			{
				int total = 0;

				foreach (string row in _strMsg.Split((Char)0x0a))
				{
					var result = row.Split(',');
					string BS = result[9];
					string symb = result[11];
					int qty = result[15].ToInt();

					if (symb == symbol)
					{
						if (BS.ToUpper() == "B") total += qty;
						else total -= qty;
					}

				}

				return total;
			}

			throw new Exception("GetAccountPositions Error.");
			
		}

		void Login()
		{
			SetConnectionStatus(ConnectionStatus.CONNECTING);

			string result = _API.Login(SID, Password, IP, out _strMsg);

			if (result == LOGIN_SUCCESS)
			{
				SetConnectionStatus(ConnectionStatus.CONNECTED);
				SetReady(true);
			}

			OnActionExecuted(LOG_IN, result);
			
		}

		void Logout()
		{
			_strMsgCode = _API.Logout(out _strMsg);
			OnActionExecuted(LOG_OUT);
		}

		void CheckConnect()
		{
			_strMsgCode = _API.FCheckConnect(out _strMsg);
			OnActionExecuted("CheckConnect");
		}

		void GetAccounts()
		{
			_strMsgCode = _API.FQueryAccountInfo(out _strMsg);
			OnActionExecuted("GetAccounts");
		}

		
		//憑證狀態
		bool GetCertStatus()
		{
			string strSDate = "";
			string strEDate = "";
			_strMsgCode = _API.GetCertStatus(SID, out strSDate, out strEDate, out _strMsg);

			OnActionExecuted("GetCertStatus");
			return (_strMsgCode == "000");
		}

		void GetSymbols()
		{
			_strMsgCode = _API.FQueryCommo(out _strMsg);
			OnActionExecuted("GetSymbols");
		}

		void GetYearMonth()
		{
			_strMsgCode = _API.FQueryCommoYM(out _strMsg);
			OnActionExecuted("GetYearMonth");
		}

		public void RefreshAccountPositions(string account)
		{
			
		}

		public void RequestAccountPositions(string account, string symbol = "")
		{
			throw new NotImplementedException();
		}

		public string ClearOrders(string symbol, string account)
		{
			throw new NotImplementedException();
		}

		public void FetchDeals(string account)
		{
			throw new NotImplementedException();
		}

		public void SetUIForm(Form form)
		{
			throw new NotImplementedException();
		}
	}
}
