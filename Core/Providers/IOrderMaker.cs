using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Views;

namespace Core
{
	public interface IOrderMaker
	{
		ProviderName Name { get; }

		bool Connectted { get; }
	
		void Connect();

		void DisConnect();

		void RequestAccountPositions(string account, string symbol= "");

		event EventHandler ExceptionHappend;

		event EventHandler Ready;

		event EventHandler ConnectionStatusChanged;

		event EventHandler ActionExecuted;

		event EventHandler AccountPositionUpdated;

		
		int GetAccountPositions(string account, string symbol);

		void MakeOrder(string symbol, string account, decimal price, int lots, bool dayTrade);

		string ClearOrders(string symbol, string account);

		void FetchDeals(string account);

		List<DealView> GetDeals();

		void SetUIForm(System.Windows.Forms.Form form);
	}

	


	public abstract class BaseOrderMaker
	{
		private readonly string _ip;
		private readonly string _sid;
		private readonly string _password;

		public BaseOrderMaker(string ip, string sid, string password)
		{
			_ip = ip;
			_sid = sid;
			_password = password;

			InitYearMonth();
		}

		protected string IP => _ip;
		protected string SID => _sid;
		protected string Password => _password;

		private ConnectionStatus _connectionStatus = ConnectionStatus.DISCONNECTED;
		protected void SetConnectionStatus(ConnectionStatus status)
		{
			if (status != _connectionStatus)
			{
				_connectionStatus = status;
				OnConnectionStatusChanged(status);
			}
		}
		protected ConnectionStatus GetConnectionStatus() => _connectionStatus;

		public event EventHandler ConnectionStatusChanged;
		void OnConnectionStatusChanged(ConnectionStatus status)
		{
			ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs(status));
		}

		private bool _ready = false;
		protected void SetReady(bool val)
		{
			if (val != _ready)
			{
				_ready = val;
				if (_ready) OnReady();
			} 
			
		}

		public event EventHandler ExceptionHappend;
		protected void OnExceptionHappend(Exception ex)
		{
			ExceptionHappend?.Invoke(this, new ExceptionEventArgs(ex));
		}


		public event EventHandler Ready;
		protected void OnReady()
		{
			Ready?.Invoke(this, new EventArgs());
		}

		public event EventHandler AccountPositionUpdated;
		protected void OnAccountPositionUpdated(AccountEventArgs e)
		{
			AccountPositionUpdated?.Invoke(this, e);
		}


		public event EventHandler ActionExecuted;
		protected void OnActionExecuted(ActionEventArgs e) => ActionExecuted?.Invoke(this, e);

		List<AccountView> _accounts = new List<AccountView>();
		protected void InitAccounts(List<string> accountIds)
		{
			_accounts = new List<AccountView>();
			for (int i = 0; i < accountIds.Count; i++)
			{
				_accounts.Add(new AccountView { Id = accountIds[i] });
			}
		}
		protected AccountView FindAccount(string id)
		{
			var account = _accounts.FirstOrDefault(x => x.Id == id);
			if (account == null) throw new Exception($"沒有這個帳號: {id}");
			return account;
		}

		public virtual int GetAccountPositions(string accountId, string symbol)
		{
			var account = FindAccount(accountId);
			if (account.Positions.IsNullOrEmpty()) return 0;

			string productId = GetProductId(symbol);
			var items = account.Positions.Where(x => x.ProductId == productId);

			if (items.IsNullOrEmpty()) return 0;

			int buyLots = items.Where(x => x.BS.EqualTo("B")).Sum(x => x.Qty);
			int sellLots = items.Where(x => x.BS.EqualTo("S")).Sum(x => x.Qty);

			return buyLots - sellLots;

		}

		public List<DealView> GetDeals() => _accounts.SelectMany(x => x.Deals).ToList();


		string _monthCode = "";
		string _yearCode = "";

		protected void InitYearMonth()
		{
			var monthDictionary = new Dictionary<int, string>
			{
				{ 1, "A" },{ 2, "B" },{ 3, "C" },
				{ 4, "D" },{ 5, "E" },{ 6, "F" },
				{ 7, "G" },{ 8, "H" },{ 9, "I" },
				{ 10, "J" },{ 11, "K" },{ 12, "L" }
			};

			var date = DateTime.Today;

			int year = date.Year;
			int month = date.Month;

			var cachDay = date.FindThirdWedOfMonth(); //本月結算日
			if (date > cachDay)
			{
				month += 1;
				if (month > 12)
				{
					year += 1;
					month = 1;
				}
			}

			_monthCode = monthDictionary[month];
			_yearCode = year.ToString().Substring(year.ToString().Length - 1, 1);

		}

		string[] _allowSymbols = new string[] { "TXF", "MXF" };
		protected string GetSymbolCode(string code)
		{
			if (_allowSymbols.Contains(code)) return code;

			var symbolCode = code.ToUpper();
			if (_allowSymbols.Contains(symbolCode)) return symbolCode;

			return symbolCode == "TX" ? "TXF" : "MXF";
		}

		Dictionary<string, string> productIdDictionary = new Dictionary<string, string>();

		protected string GetProductId(string code)
		{
			if (productIdDictionary.ContainsKey(code)) return productIdDictionary[code];

			string id = $"{GetSymbolCode(code)}{_monthCode}{_yearCode}";
			productIdDictionary.Add(code, id);

			return id; 
		}
		
	}

}
