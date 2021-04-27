using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public interface IPositionInfo
    {
        int Position { get; set; }
        decimal MarketPrice { get; set; }
    }


    public class PositionFile : IPositionInfo
    {
		public int Position { get; set; }

		public decimal MarketPrice { get; set; }

		public DateTime Time { get; set; }

	}

	public class TradeSettings
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public int Interval { get; set; }

		public bool DayTrade { get; set; }

		public string FileName { get; set; }

		public int Offset { get; set; }

		public List<AccountSettings> Accounts { get; set; } = new List<AccountSettings>();

		public AccountSettings FindAccountSettings(string account) => Accounts.FirstOrDefault(x => x.Account == account);

	}

	public class AccountSettings
	{
		public TradeSettings TradeSettings { get; set; }

		public string Id { get; set; } = Guid.NewGuid().ToString();

		public string Account { get; set; }

		public string Symbol { get; set; }

		//倍數
		public int Lot { get; set; }
	}
}
