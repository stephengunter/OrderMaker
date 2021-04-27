using Core.Logging;
using Core.Models;
using Core.Services;
using Core.Views;
using System.Collections.Generic;

namespace Core.Factories
{
	public class ServiceFactory
	{
        public static IPositionManager CreatePositionManager(IOrderMaker orderMaker, TradeSettings tradeSettings, ILogger logger)
        {
            return new PositionManager(orderMaker, tradeSettings, logger);
        }

        public static ITimeManager CreateTimeManager(string begin, string end)
        {
            return new TimeManager(begin, end);
        }
    }
}
