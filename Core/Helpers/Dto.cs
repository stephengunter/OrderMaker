using Core.Models;
using Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class DtoHelpers
    {
        public static TradeSettingsView ToView(this TradeSettings settings)
        {
            return new TradeSettingsView()
            {
                Id = settings.Id,
                Name = settings.Name,
                Interval = settings.Interval,
                DayTrade = settings.DayTrade,
                FileName = settings.FileName,
                Offset = settings.Offset
            };
        }

        public static AccountView ToView(this AccountSettings account, TradeSettings tradeSettings)
        {
            return new AccountView()
            {
                TradeSettings = tradeSettings.ToView(),
                Number = account.Account,
                Symbol = account.Symbol,
                Lots = account.Lot
            };
        }

    }
}
