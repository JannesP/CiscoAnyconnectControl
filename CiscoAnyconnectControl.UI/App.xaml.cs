using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CiscoAnyconnectControl.IPC.DTOs;
using CiscoAnyconnectControl.Model;
using CiscoAnyconnectControl.Model.DAL;
using CiscoAnyconnectControl.UI.IpcClient;
using CiscoAnyconnectControl.UI.Utility;
using CiscoAnyconnectControl.UI.View;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyconnectControl.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _mutex;

        private enum ErrorCode
        {
            FailedIpc,
            NotFirstInstance
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //TODO: install service if not installed.
            //TODO: check for cisco autostart

            bool trayStart = false;

            //parse command line arguments
            foreach (string arg in e.Args)
            {
                Trace.TraceInformation("Parsing arg: {0} ...", arg);
                switch (arg)
                {
                    case "-tray":
                        OSUtil.Instance.ShowTrayIcon();
                        trayStart = true;
                        break;
                    case "-autoconnect":
                        /*try
                        {
                            bool connected = IpcClient.VpnControlClient.Instance.ConnectAsync()
                                .Wait(TimeSpan.FromSeconds(30));
                            if (connected)
                            {
                                VpnControlClient.Instance.Service?.Connect(
                                    VpnDataModelTo.FromModel(VpnDataFile.Instance.VpnDataModel));
                                VpnControlClient.Instance.DisconnectAsync(TimeSpan.FromSeconds(5), true).Wait();
                                App.Current.Shutdown();
                                return;
                            }
                            else
                            {
                                App.Current.Shutdown((int)ErrorCode.FailedIpc);
                                return;
                            }
                        }
                        catch (AggregateException ex)
                        {
                            Util.TraceException("Error connecting VpnControlClient:", ex.InnerExceptions[0]);
                            App.Current.Shutdown((int)ErrorCode.FailedIpc);
                            return;
                        }*/
                        break;
                }
            }
            if (!CheckIfFirstInstance())
            {
                App.Current.Shutdown((int)ErrorCode.NotFirstInstance);
                return;
            }
            if (!trayStart)
            {
                CreateAndOrShowMainWindow();
            }
            App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            OSUtil.Instance.TrayIconDoubleClick += App_TrayIconDoubleClick;
            OSUtil.Instance.TrayExitClick += App_TrayExitClick;
            base.OnStartup(e);
        }

        private void App_TrayExitClick(object sender, EventArgs e)
        {
            App.Current.Shutdown(0);
        }

        private void CreateAndOrShowMainWindow()
        {
            if (App.Current.MainWindow == null)
            {
                var window = new MainWindow();
                window.Show();
                window.Closed += Window_Closed;
                App.Current.MainWindow = window;
            }
            else
            {
                App.Current.MainWindow.Activate();
            }
        }

        private void App_TrayIconDoubleClick(object sender, EventArgs e)
        {
            CreateAndOrShowMainWindow();
        }

        private bool CheckIfFirstInstance()
        {
            this._mutex = new Mutex(false, "e35d4009-816a-454f-80cc-190e27f95384");
            try
            {
                if (!this._mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Another instance is already running. Showing that isnt implemented though.");
                    return false;
                }
            } catch(Exception ex) { Util.TraceException("Exception creating mutex.", ex); }
            return true;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                OSUtil.Instance.HideTrayIcon();
                /*IEnumerable<Process> exe = Process.GetProcesses().Where((p) => p.ProcessName == "vpncli");
                foreach (Process proc in exe)
                {
                    //TODO maybe change to work with service if needed
                    proc.Kill();
                }*/
            }
            catch (Exception ex)
            {
                Util.TraceException("Unhandled exception in unhandled exception handler.", ex);
            }
            finally
            {
                this._mutex?.Dispose();
                this._mutex = null;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (SettingsFile.Instance.SettingsModel.CloseToTray)
            {
                OSUtil.Instance.ShowTrayIcon();
                App.Current.MainWindow = null;
            }
            else
            {
                App.Current.Shutdown(0);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            OSUtil.Instance.HideTrayIcon();
            this._mutex?.Dispose();
            this._mutex = null;
            base.OnExit(e);
        }
    }
}
