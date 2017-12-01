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
using CiscoAnyconnectControl.Model.Annotations;
using CiscoAnyconnectControl.UI.Command;

namespace CiscoAnyconnectControl.UI.View
{
    /// <summary>
    /// Interaction logic for SelectGroupModalWindow.xaml
    /// </summary>
    public partial class SelectGroupModalWindow : Window
    {
        private readonly ComboBox _cbGroups;
        public SelectGroupModalWindow([NotNull]IEnumerable<string> options)
        {
            InitCommands();
            InitializeComponent();
            _cbGroups = (ComboBox) FindName("CbGroups");
            if (_cbGroups == null) throw new Exception("Error initializing window, CbGroups not found.");
            _cbGroups.ItemsSource = options;
        }

        public SelectGroupModalWindow() : this(new List<string> {"ExampleItem"}) { }

        public string SelectedGroup { get; private set; }
        public int SelectedGroupIndex { get; private set; }

        private void InitCommands()
        {
            //TODO replace CanExecute with something useful
            OkCommand = new RelayCommand(() => true, () =>
            {
                DialogResult = true;
                SelectedGroup = (string) _cbGroups.SelectedItem;
                SelectedGroupIndex = _cbGroups.SelectedIndex;
                Close();
            });

            CancelCommand = new RelayCommand(() => true, () =>
            {
                DialogResult = false;
                SelectedGroup = null;
                Close();
            });
        }

        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }
    }
}
