using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Helpers;
using Core.Views;

namespace Core
{
	public class HuaNanDDSCOrderMaker : BaseOrderMaker, IOrderMaker
	{
		public ProviderName Name => ProviderName.HUA_NAN;

		const string ORDER_CANCELED = "0002";

		private HNTradeAPI.DDSCTradeAPI _API = new HNTradeAPI.DDSCTradeAPI();

		string _currentAccountId = "";
		string CurrentAccountId => _currentAccountId;

		OrderActionType _orderActionType = OrderActionType.None;
		OrderActionType CurrentAction => _orderActionType;

		public HuaNanDDSCOrderMaker(string ip, string sid, string password) : base(ip, sid, password)
		{

			_API.LoginStatus += new HNTradeAPI.DDSCTradeAPI.loginStatusEventHandler(OnAPILoginStatusChanged);
			_API.Error += new HNTradeAPI.DDSCTradeAPI.errorEventHandler(OnAPIError);
			
			_API.OrderReplyData += new HNTradeAPI.DDSCTradeAPI.orderReplyDataEventHandler(OnAPIOrderReplyData);
			_API.QueryOrderReplyData += new HNTradeAPI.DDSCTradeAPI.orderReplyDataEventHandler(OnAPIOrderReplyData);

			_API.PositionData += new HNTradeAPI.DDSCTradeAPI.positionDataEventHandler(OnAPIPositionData);
			_API.PositionNoData += new HNTradeAPI.DDSCTradeAPI.positionNoDataEventHandler(OnAPIPositionNoData);
			
			_API.QueryMatchReplyData += new HNTradeAPI.DDSCTradeAPI.matchReplyDataEventHandler(OnAPIMatchReplyData);

			
		}

		

		public void SetUIForm(System.Windows.Forms.Form form) { }

		private void OnAPIOrderReplyData(string TRADEDATE, string ORDERNO, string ORDERTIME, string BROKERID, string INVESTORACNO, string BS, string PRODUCTID, string ORDERPRICE, string ORDERQTY, string MATCHQTY, string NOMATCHQTY, string DELQTY, string ORDERSTATUS, string STATUSCODE, string NETWORKID, string PRODUCTKIND, string OPENOFFSETFLAG, string ORDERCONDITION, string ORDERTYPE, string SEQ, string DTRADE, string MDATE, string ORDERKIND)
		{
			string status = STATUSCODE.Trim();
			if (status.EqualTo("error"))
			{
				OnExceptionHappend(new OrderError(STATUSCODE, ORDERSTATUS));
				return;
			}
			else if (status == ORDER_CANCELED)
			{
				return;
			}

			if (INVESTORACNO == CurrentAccountId)
			{
				if (CurrentAction == OrderActionType.Delete)
				{
					if (NOMATCHQTY.ToInt() > 0)
					{
						//未成交的單子
						_API.CancelOrder(CurrentAccountId, ORDERNO);
						RequestAccountPositions(CurrentAccountId);
					}
				}
			}
			
		}


		public void Connect() => Login();

		public void DisConnect() => Logout();

		public bool Connectted => GetConnectionStatus() == ConnectionStatus.CONNECTED;

		void QueryOrders(string account)
		{
			_API.GetOrderReply(account, "", "", "", "");
		}

		public string ClearOrders(string symbol, string account)
		{
			//大台 TXF 小台 MXF
			string productId = GetProductId(symbol);

			int wb = _API.GetWB(account, productId);  //買進未成交口數
			int ws = _API.GetWS(account, productId);  //賣出未成交口數

			if (wb == 0 && ws == 0) return productId;

			_orderActionType = OrderActionType.Delete;
			QueryOrders(account);
			return String.Empty;
		}

		public void MakeOrder(string symbol, string account, decimal price, int lots, bool dayTrade)
		{
			//大台 TXF 小台 MXF
			string productId = ClearOrders(symbol, account);
			if (String.IsNullOrEmpty(productId)) return;


			_orderActionType = OrderActionType.Create;

			string bs = lots > 0 ? "B" : "S";
			string strPrice = price > 0 ? Convert.ToInt32(price).ToString() : "0";
			string qty = Math.Abs(lots).ToString();
			string otype = price > 0 ? "L" : "P";  //L:限價, M:市價, P:一定範圍市價

			
			string fir = price > 0 ? "R" : "I";  //R:ROD, F:FOK, I:IOC
			string rtype = dayTrade ? "2" : "";  // "0":新倉, "1":平倉, "2":期貨當沖, "":自動
												 

			_API.NewOrder(account, productId, bs, strPrice, qty, otype, fir, rtype);

			OnActionExecuted($"MakeOrder: {bs} {qty} {productId} , price: {strPrice} ");
		}


		public void RequestAccountPositions(string accountId, string symbol = "")
		{
			var account = FindAccount(accountId);

			_currentAccountId = accountId;
			account.Positions = new List<PositionView>();

			_API.GetPosition(accountId);
		}

		public void FetchDeals(string accountId)
		{
			var account = FindAccount(accountId);
			account.Deals = new List<DealView>();

			_API.GetMatchReply(accountId, "", "", "", "");

		}


		void Login()
		{
			SetConnectionStatus(ConnectionStatus.CONNECTING);
			string company = "";
			
			_API.Login(company, SID, Password, IP);
			
		}

		void Logout()
		{
			_API.Logout();
			OnActionExecuted("Logout");
		}

		//未平倉查詢回報
		private void OnAPIPositionData(string Company, string Actno, string PRODUCTID, string PRODUCTDESC, string BS, string POSITION_QTY, string POSITION_PRICE, string YES_POSITION, string NOW_QTY, string MARKET_PRICE, string CLOSE_PRICE, string B_MATCH_PRICE, string S_MATCH_PRICE, string B_MATCH_QTY, string S_MATCH_QTY, string FLOAT_PROFIT, string U_FLOAT_PROFIT, string INCOME_BALANCE, string U_INCOME_BALANCE, string COMTYPE, string KIND_ID, string STRIKE_PRICE, string CP, string MONTH, string YEAR)
		{
			var account = FindAccount(Actno);
			account.Positions.Add(new PositionView
			{
				 ProductId = PRODUCTID,
				 Qty = POSITION_QTY.ToInt(),
				 BS = BS
			});

			OnAccountPositionUpdated(new AccountEventArgs(_currentAccountId));
		}

		//成交查詢回報
		private void OnAPIMatchReplyData(string TRADEDATE, string ORDERNO, string MATCHTIME, string BROKERID, string INVESTORACNO, string BS, string PRODUCTID, string MATCHPRICE, string MATCHQTY, string BS1, string PRODUCTID1, string MATCHPRICE1, string BS2, string PRODUCTID2, string MATCHPRICE2, string NETWORKID, string PRODUCTKIND, string OPENOFFSETFLAG, string MATCHSEQ, string DTRADE, string MDATE, string ORDERKIND)
		{
			try
			{
				var account = FindAccount(INVESTORACNO);
				account.Deals.Add(new DealView
				{
					AccountId = account.Id,
					Date = TRADEDATE,
					Time = MATCHTIME,
					ProductId = PRODUCTID,
					Price = MATCHPRICE.ToInt(),
					Qty = MATCHQTY.ToInt(),
					BS = BS.Trim()
				});
			}
			catch (Exception ex)
			{
				OnExceptionHappend(ex);
			}
			
		}


		private void OnAPIPositionNoData()
		{
			OnAccountPositionUpdated(new AccountEventArgs(_currentAccountId));
		}

		private void OnAPILoginStatusChanged(string msg)
		{
			if (_API.loginStatusFlag)
			{
				//取得帳號
				InitAccounts(_API.Accounts.ToList());

				SetConnectionStatus(ConnectionStatus.CONNECTED);
				SetReady(true);
			}
			else SetConnectionStatus(ConnectionStatus.DISCONNECTED);


			OnActionExecuted($"OnAPILoginStatusChanged: {msg}");
		}

		private void OnAPIError(string msg)
		{
			OnActionExecuted($"OnAPIError: {msg}");
		}

		void OnActionExecuted(string action, string code = "", string msg = "")
			=> OnActionExecuted(new ActionEventArgs(action, code, msg));




	}
}
