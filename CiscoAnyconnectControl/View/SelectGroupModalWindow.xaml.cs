using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CiscoAnyconnectControl.Command;
using CiscoAnyconnectControl.Model.Annotations;

namespace CiscoAnyconnectControl.View
{
    /// <summary>
    /// Interaction logic for SelectGroupModalWindow.xaml
    /// </summary>
    public partial class SelectGroupModalWindow : Window
    {
        private ComboBox _cbGroups;
        public SelectGroupModalWindow([NotNull]IEnumerable<string> options)
        {
            InitializeComponent();
            this._cbGroups = (ComboBox) FindName("CbGroups");
            if (this._cbGroups == null) throw new Exception("Error initializing window, CbGroups not found.");
            this._cbGroups.ItemsSource = options;

            InitCommands();
        }

        public SelectGroupModalWindow() : this(new List<string>()) { }

        public string SelectedGroup { get; private set; }

        private void InitCommands()
        {
            this.OkCommand = new RelayCommand(() => this._cbGroups.SelectedIndex != -1, () =>
            {
                this.DialogResult = true;
                this.SelectedGroup = (string) this._cbGroups.SelectedItem;
                this.Close();
            });

            this.CancelCommand = new RelayCommand(() => true, () =>
            {
                this.DialogResult = false;
                this.SelectedGroup = null;
                this.Close();
            });
        }

        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }
    }
}
