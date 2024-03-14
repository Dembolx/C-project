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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace saper_pythoniarz
{ 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            string selectedDifficulty = ((ComboBoxItem)DifficultyComboBox.SelectedItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedDifficulty))
            {
                MessageBox.Show("Wybierz poziom trudności przed rozpoczęciem gry.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int widthsize;
            int heightsiza;
            switch (selectedDifficulty)
            {
                case "Łatwy":
                    widthsize = 8;
                    heightsiza = 8;
                    break;
                case "Średni":
                    widthsize = 16;
                    heightsiza = 16;
                    break;
                case "Trudny":
                    widthsize = 30;
                    heightsiza = 16;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GameWindow gameWindow = new GameWindow(widthsize, heightsiza);
            gameWindow.Show();
            Close();
        }
    }
}
