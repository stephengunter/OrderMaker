using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public class RemoveAccountEventArgs : EventArgs
	{
		public RemoveAccountEventArgs(string id)
		{
			this.Id = id;
		}

		public string Id { get; private set; }
	}


	public class EditStrategyEventArgs : EventArgs
	{
		public EditStrategyEventArgs(TradeSettings tradeSettings)
		{
			this.TradeSettings = tradeSettings;
		}

		public TradeSettings TradeSettings { get; private set; }
	}
}
