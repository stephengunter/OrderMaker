using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Views
{
	public class TradeSettingsView
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public int Interval { get; set; }

		public bool DayTrade { get; set; }

		public string FileName { get; set; }

		public int Offset { get; set; }

	}

	public class AccountView
	{
		public string Id { get; set; }
		public List<PositionView> Positions { get; set; } = new List<PositionView>();
		public List<DealView> Deals { get; set; } = new List<DealView>();
		
		public string Number { get; set; }
		public string Symbol { get; set; }
		public int Lots { get; set; }

		public int OI { get; set; }

		public TradeSettingsView TradeSettings { get; set; }

	}

	public class PositionView
	{
		public string ProductId { get; set; }
		public string BS { get; set; }
		public int Qty { get; set; }
	}

	public class DealView
	{
		public string AccountId { get; set; }

		public string Date { get; set; }
		public string Time { get; set; }
		public string ProductId { get; set; }
		public string BS { get; set; }

		public int Price { get; set; }
		public int Qty { get; set; }
	}
	
}
