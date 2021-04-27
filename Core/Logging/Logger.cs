using Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Views;
using Newtonsoft.Json;

namespace Core.Logging
{
	public interface ILogger
	{
		void LogException(Exception ex);
		void LogInfo(string msg = "");

		void SaveDeals(IEnumerable<DealView> deals);
		IEnumerable<DealView> FetchAllDeals();

		string FilePath { get; }
	}

	public class Logger : ILogger
	{
		private readonly string _filePath;
		private string _dealsFilePath;

		public Logger(string folderPath)
		{
            string fileName = $"{DateTime.Today.ToDateNumber().ToString()}.txt";
			this._filePath = Path.Combine(folderPath, fileName);

			System.IO.Directory.CreateDirectory(folderPath);

			InitDealsFile(folderPath);

		}

		void InitDealsFile(string folderPath)
		{
			//deals
			var dealsfilePath = Path.Combine(folderPath, "deals.json");
			if (!File.Exists(dealsfilePath)) File.Create(dealsfilePath).Close();
			_dealsFilePath = dealsfilePath;
		}

		public string FilePath => _filePath;

		public void LogException(Exception ex)
		{
			try
			{
				using (StreamWriter sw = File.AppendText(FilePath))
				{
					sw.WriteLine(String.Format("{0} {1}", DateTime.Now, ex.ToString()));
				}
			}
			catch (Exception)
			{

			
			}
			
		}

		public void LogInfo(string msg = "")
		{
			try
			{
				using (StreamWriter sw = File.AppendText(FilePath))
				{
					sw.WriteLine(String.Format("{0} {1}", DateTime.Now, msg));
				}
			}
			catch (Exception)
			{


			}
		}

		public void SaveDeals(IEnumerable<DealView> deals)
		{
			if (deals.IsNullOrEmpty()) return;

			var list = FetchAllDeals().ToList();
			list.AddRange(deals);

			System.IO.File.WriteAllText(_dealsFilePath, JsonConvert.SerializeObject(list));
		}

		public IEnumerable<DealView> FetchAllDeals()
		{
			var doc = System.IO.File.ReadAllText(_dealsFilePath);

			var list = JsonConvert.DeserializeObject<IEnumerable<DealView>>(doc);


			return list.IsNullOrEmpty() ? new List<DealView>() : list;
		}


	}
}
