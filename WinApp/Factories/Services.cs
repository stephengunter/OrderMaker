using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinApp
{
    public class Factories
    {
        public static ISettingsManager CreateSettingsManager() => new SettingsManager();
    }
}
