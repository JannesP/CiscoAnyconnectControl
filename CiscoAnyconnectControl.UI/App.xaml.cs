﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CiscoAnyconnectControl.UI.View;

namespace CiscoAnyconnectControl.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!CheckIfFirstInstance())
            {
                App.Current.Shutdown(1);
                return;
            }
            /*if (!CheckForCiscoProcesses())
            {
                App.Current.Shutdown(2);
                return;
            }*/

            //TODO parse command line arguments
            //TODO display tray icon

            var window = new MainWindow();
            window.Show();
            window.Closed += Window_Closed;
            App.Current.MainWindow = window;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            base.OnStartup(e);
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
            } catch(Exception ex) { Utility.Util.TraceException("Exception creating mutex.", ex); }
            return true;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                this._mutex?.Dispose();
                this._mutex = null;
                IEnumerable<Process> exe = Process.GetProcesses().Where((p) => p.ProcessName == "vpncli");
                foreach (Process proc in exe)
                {
                    //TODO maybe change to work with service if needed
                    proc.Kill();
                }
            }
            catch (Exception ex) { Utility.Util.TraceException("Unhandled exception in unhandled exception handler.", ex); }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown(0);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this._mutex?.Dispose();
            this._mutex = null;
            //TODO hide tray icon
            base.OnExit(e);
        }
    }
}