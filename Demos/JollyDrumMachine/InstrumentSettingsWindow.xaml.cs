using JollyDrumMachine.Models;
using System.Windows;

namespace JollyDrumMachine
{
    public partial class InstrumentSettingsWindow : Window
    {
        public InstrumentSettingsWindow(DrumInstrument instrument)
        {
            InitializeComponent();
            DataContext = instrument;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
