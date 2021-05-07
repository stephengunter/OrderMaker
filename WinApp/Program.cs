using Core.Helpers;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using WinApp.Test;

namespace WinApp
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            

            var settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                                                 .AppSettings.Settings;

            string mode = settings[AppSettingsKey.Mode].Value;


            Application.ThreadException +=
            new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            // Add handler to handle the exception raised by additional threads
            AppDomain.CurrentDomain.UnhandledException +=
            new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if(mode.EqualTo("BasicTest")) Application.Run(new BasicTestForm());
            else if(mode.EqualTo("ApiTest")) Application.Run(new API_TestForm());
            else if (mode.EqualTo("StrategyTest")) Application.Run(new StrategyTestForm());
            else Application.Run(new Main());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            // All exceptions thrown by the main thread are handled over this method
            LogError(e.Exception);

            throw e.Exception;
        }


        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // All exceptions thrown by additional threads are handled in this method
            var ex = e.ExceptionObject as Exception;
            LogError(ex);

            //throw ex;

            // Suspend the current thread for now to stop the exception from throwing.
            //Thread.CurrentThread.Suspend();
        }

        static void LogError(Exception ex)
        {
            string date = DateTime.Today.ToDateNumber().ToString();
            string filePath = $"{ConfigurationManager.AppSettings[AppSettingsKey.LogFile]}{date}.txt";

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(String.Format("{0} {1}", DateTime.Now, ex.ToString()));
            }

        }


    }
}
