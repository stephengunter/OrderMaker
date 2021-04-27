using Core.Helpers;
using Core.Security;
using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Linq;

namespace WinApp
{
    public interface ISettingsManager
    {
        KeyValueConfigurationCollection BasicSettings { get; }
        string GetSettingValue(string key);
        string LogFilePath { get; }

        List<TradeSettings> TradeSettings { get; }
        TradeSettings FindTradeSettings(string id);

        bool CheckBasicSetting();
        string AddUpdateAppSettings(string key, string value);

        void AddUpdateTradeSettings(TradeSettings tradeSettings);
        void RemoveTradeSettings(TradeSettings tradeSettings);
    }

    public class SettingsManager : ISettingsManager
    {
        private KeyValueConfigurationCollection _settings;

        string TradeSettingsFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        string TradeSettingsPath => Path.Combine(TradeSettingsFolder, "trade.json");

        List<TradeSettings> _tradeSettings = new List<TradeSettings>();
        

        public SettingsManager()
        {
            this._settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                                                 .AppSettings.Settings;

            LoadTradeSettings();
        }

        public string GetSettingValue(string key) => _settings[key].Value;

        public string LogFilePath => GetSettingValue(AppSettingsKey.LogFile);

        public KeyValueConfigurationCollection BasicSettings => _settings;        

        public bool CheckBasicSetting()
        {
            string sid = GetSettingValue("SID");
            if (String.IsNullOrEmpty(sid)) return false;

            string password = GetSettingValue("Password");
            if (String.IsNullOrEmpty(password)) return false;

            try
            {
                password = CryptoGraphy.DecryptCipherTextToPlainText(GetSettingValue("Password"));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public List<TradeSettings> TradeSettings => _tradeSettings;
        public TradeSettings FindTradeSettings(string id) => TradeSettings.FirstOrDefault(x => x.Id == id);

        void LoadTradeSettings()
        {
            Directory.CreateDirectory(TradeSettingsFolder);

            if (!File.Exists(TradeSettingsPath)) File.Create(TradeSettingsPath).Close();

            string content = "";
            using (StreamReader sr = new StreamReader(TradeSettingsPath))
            {
                content = sr.ReadToEnd();
            }

            var tradeSettings = JsonConvert.DeserializeObject<List<TradeSettings>>(content);
            if (tradeSettings.HasItems()) this._tradeSettings = tradeSettings;

        }

        public string AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

                return "";
            }
            catch (ConfigurationErrorsException)
            {
                return "寫入設定檔失敗";
            }
        }

        public void AddUpdateTradeSettings(TradeSettings tradeSettings)
        {
            if (String.IsNullOrEmpty(tradeSettings.Id))
            {
                //新增
                tradeSettings.Id = Guid.NewGuid().ToString();
                _tradeSettings.Add(tradeSettings);
            }
            else
            {
                var idx = _tradeSettings.FindIndex(x => x.Id == tradeSettings.Id);
                _tradeSettings[idx] = tradeSettings;
            }

            SaveTradeSettings();

        }

        public void RemoveTradeSettings(TradeSettings tradeSettings)
        {
            var idx = _tradeSettings.FindIndex(x => x.Id == tradeSettings.Id);
            _tradeSettings.RemoveAt(idx);

            SaveTradeSettings();

        }

        void SaveTradeSettings()
        {
            using (StreamWriter file = File.CreateText(TradeSettingsPath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _tradeSettings);
            }
        }

    }
}
