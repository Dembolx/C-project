using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace saper_pythoniarz
{
    public partial class GameWindow : Window
    {
        // Zmienne globalne

        private int widthsize;
        private int heightsiza;
        private Button[,] buttons;
        private int[,] tablica;
        private bool[,] tablica_odkryte;
        private bool[,] tablica_oznaczone;
        private int bomby = 0;
        private bool first_move = true;
        private bool koniec = false;
        private int czas = 0;
        Label lebelek_bomby;
        Label Labelek_czas;
        Button button_reset;
        private DispatcherTimer timer;
        Grid grid = new Grid();

        public GameWindow(int width, int height)
        {
            InitializeComponent();

            // Przypisanie wielkości planszy
            widthsize = width;
            heightsiza = height;
            buttons = new Button[widthsize, heightsiza];
            tablica = new int[widthsize + 2, heightsiza + 2];
            tablica_odkryte = new bool[widthsize + 2, heightsiza + 2];
            tablica_oznaczone = new bool[widthsize + 2, heightsiza + 2];
            bomby = widthsize * heightsiza / 7; 

            // Ustawienie szerokości i wysokości okna
            this.Width = width * 50 + 100; // Szerokość pola * ilość kolumn + margines
            this.Height = height * 50 + 100; // Wysokość pola * ilość wierszy + margines

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            AddLabels();
        } // Gotowe !

        private void GameGrid_Loaded(object sender, RoutedEventArgs e)
        {
            CreateGameBoard();
        } // Gotowe !

        private void Tablica_z_Bombami(int wi, int he)
        {

            int p_bomby = bomby;

            for (int w = 0; w < widthsize + 2; w++)
            {
                for (int h = 0; h < heightsiza + 2; h++)
                {
                    tablica[w, h] = 10;
                    tablica_odkryte[w, h] = false;
                    tablica_oznaczone[w, h] = false;
                }
            }

            for (int w = 1; w < widthsize + 1; w++)
            {
                for (int h = 1; h < heightsiza + 1; h++)
                {
                    tablica[w, h] = 0;
                    tablica_odkryte[w, h] = false;
                    tablica_oznaczone[w, h] = false;
                }
            }

            Random random = new Random();

            while (0 < p_bomby)
            {
                int wit = random.Next(1, widthsize + 1);
                int hei = random.Next(1, heightsiza + 1);

                // Jeśli pierwsze pole, na które gracz kliknie, jest polem z bombą, losuje jeszcze raz
                if ((wit != wi && hei != he) && tablica[wit, hei] != 9)
                {
                    tablica[wit, hei] = 9;
                    p_bomby--;
                }
            }

            for (int w = 1; w < widthsize + 1; w++)
            {
                for (int h = 1; h < heightsiza + 1; h++)
                {
                    if (tablica[w, h] != 9)
                    {
                        tablica[w, h] = Ile_bomb_w_poblirzu(w, h);
                    }
                }
            }
        } // Gotowe !

        private void CreateGameBoard()
        {
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Center;

            for (int w = 0; w < widthsize + 2; w++)
            {
                for (int h = 0; h < heightsiza + 2; h++)
                {
                    Button button = new Button();

                    if (w == 0 || h == 0 || h == heightsiza + 1 || w == widthsize + 1)
                    {
                        button.Opacity = 0;
                    }
                    else
                    {
                        button.Click += Button_Click;
                        button.Tag = new Tuple<int, int>(w, h);
                        button.MouseRightButtonDown += Button_RightClick;
                    }

                    button.Width = 40;
                    button.Height = 40;
                    button.Background = null;
                    button.HorizontalAlignment = HorizontalAlignment.Left;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    button.Margin = new Thickness((w * 50), (h * 50), 0, 0);
                    Grid.SetRow(button, w);
                    Grid.SetColumn(button, h);
                    grid.Children.Add(button);
                }
            }
            this.Content = grid;
        } // Gotowe !


        private int Ile_bomb_w_poblirzu(int wi, int he)
        {
            int ile_bomb = 0;

            for (int w = wi - 1; w <= wi + 1; w++)
            {
                for (int h = he - 1; h <= he + 1; h++)
                {
                    if (tablica[w, h] == 9)
                    {
                        ile_bomb++;
                    }
                }
            }

            return ile_bomb;
        } // Gotowe !

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            var position = (Tuple<int, int>)clickedButton.Tag;
            int col = position.Item1;
            int row = position.Item2;

            if (first_move)
            {
                first_move = false;
                timer.Start();
                Tablica_z_Bombami(col, row);
            }

            if (!tablica_odkryte[col, row] && !tablica_oznaczone[col, row])
            {
                switch (tablica[col, row])
                {
                    case 0:
                        OdkryjSasiada(col, row);
                        break;
                    case 9:
                        tablica_odkryte[col, row] = true;
                        clickedButton.Content = "X"; // Oznacz bombę
                        clickedButton.Background = Brushes.IndianRed;
                        koniec = true;
                        timer.Stop();
                        DisableButtonsInGrid();
                        MessageBox.Show("Przegrałeś!!");
                        break;
                    default:
                        tablica_odkryte[col, row] = true;
                        clickedButton.Content = tablica[col, row].ToString();
                        clickedButton.Background = Brushes.LightGreen;
                        break;
                }

                // Sprawdź, czy gracz wygrał
                if (CheckWin())
                {
                    koniec = true;
                    timer.Stop();
                    DisableButtonsInGrid();
                    MessageBox.Show("Gratulacje! Wygrałeś!!");
                }
            }
        } // Gotowe !

        private void OdkryjSasiada(int wi, int he)
        {
            if (tablica_odkryte[wi, he] || tablica_oznaczone[wi, he])
            {
                return;
            }
            
            Button currentButton = GetButtonFromCoordinates(wi, he);

            if (currentButton != null) // Sprawdzenie, czy przycisk został znaleziony
            {
                if (tablica[wi, he] == 0)
                {
                    tablica_odkryte[wi, he] = true;
                    currentButton.Content = tablica[wi, he].ToString();
                    currentButton.Background = Brushes.LightGreen;

                    // Jeśli pole nie jest bombą, to sprawdzamy, czy sąsiadujące pola są zerami
                    for (int w = wi - 1; w <= wi + 1; w++)
                    {
                        for (int h = he - 1; h <= he + 1; h++)
                        {
                            if (w >= 0 && w < widthsize + 2 && h >= 0 && h < heightsiza + 2)
                            {
                                OdkryjSasiada(w, h);
                            }
                        }
                    }
                }
                else if (tablica[wi, he] != 9)
                {
                    tablica_odkryte[wi, he] = true;
                    currentButton.Content = tablica[wi, he].ToString();
                    currentButton.Background = Brushes.LightGreen;
                }
                else
                {
                    tablica_odkryte[wi, he] = false;
                    return;
                }
            }
            else
            {
                MessageBox.Show("Nie ma buttona");
            }
        } // Gotowe !

        private Button GetButtonFromCoordinates(int row, int col)
        {
            foreach (UIElement element in grid.Children)
            {
                if (element is Button button)
                {
                    int buttonCol = (int)button.GetValue(Grid.ColumnProperty);
                    int buttonRow = (int)button.GetValue(Grid.RowProperty);

                    if (buttonCol == col && buttonRow == row)
                    {
                        return button;
                    }
                }
            }

            // Obsługa przypadku, gdy nie znaleziono przycisku dla określonych współrzędnych
            return null;
        } // Gotowe !


        private void Button_RightClick(object sender, MouseButtonEventArgs e)
        {
            Button clickedButton = (Button)sender;
            var position = (Tuple<int, int>)clickedButton.Tag;
            int col = position.Item1;
            int row = position.Item2;

            if (!tablica_odkryte[col, row] && !first_move)
            {
                if (!tablica_oznaczone[col, row])
                {
                    if(bomby != 0)
                    {
                        clickedButton.Content = "🚩"; // Oznacz flagą
                        clickedButton.Background = Brushes.LightGoldenrodYellow;
                        tablica_oznaczone[col, row] = true;
                        bomby--;
                        UpdateBombCountLabel();
                    }
                }
                else
                {
                    clickedButton.Content = ""; // Usuń oznaczenie flagi
                    clickedButton.Background = null;
                    tablica_oznaczone[col, row] = false;
                    bomby++;
                    UpdateBombCountLabel();
                }
            }
        } // Gotowe !

        private bool CheckWin()
        {
            // Sprawdź, czy wszystkie pola niebędące bombami zostały odkryte
            for (int w = 1; w < widthsize + 1; w++)
            {
                for (int h = 1; h < heightsiza + 1; h++)
                {
                    if (tablica[w, h] != 9 && !tablica_odkryte[w, h])
                    {
                        return false;
                    }
                }
            }
            return true;
        } // Gotowe !

        private void AddLabels()
        {
            // Tworzenie nowego obiektu Grid
            Grid labelGrid = new Grid();
            labelGrid.HorizontalAlignment = HorizontalAlignment.Center;
            labelGrid.VerticalAlignment = VerticalAlignment.Top;

            // Utwórz trzy Label
            Label label1 = new Label();
            label1.Content = "Czas: 0s";
            Labelek_czas = label1;
            label1.Width = 200; 
            label1.Margin = new Thickness(10, 0, 10, 0); 


            Button Button_Reset = new Button();
            button_reset = Button_Reset;
            button_reset.Click += RestartButton_Click;
            Button_Reset.Content = "Reset";
            Button_Reset.Width = 100; 
            Button_Reset.Margin = new Thickness(10, 0, 10, 0); 


            Label label3 = new Label();
            label3.Content = "Ilosc bomb: " + bomby;
            lebelek_bomby = label3;
            label3.Width = 200; 
            label3.Margin = new Thickness(10, 0, 10, 0); 

            // Ustawienie kolumn labeli
            ColumnDefinition columnDefinition1 = new ColumnDefinition();
            labelGrid.ColumnDefinitions.Add(columnDefinition1);

            ColumnDefinition columnDefinition2 = new ColumnDefinition();
            labelGrid.ColumnDefinitions.Add(columnDefinition2);

            ColumnDefinition columnDefinition3 = new ColumnDefinition();
            labelGrid.ColumnDefinitions.Add(columnDefinition3);

            Grid.SetColumn(label1, 0);
            Grid.SetColumn(Button_Reset, 1);
            Grid.SetColumn(label3, 2);

            labelGrid.Children.Add(label1);
            labelGrid.Children.Add(Button_Reset);
            labelGrid.Children.Add(label3);

            // Dodanie Grida z etykietami do głównego Grida
            grid.Children.Add(labelGrid);
        } // Gotowe !

        private void UpdateBombCountLabel()
        {
            lebelek_bomby.Content =  "Ilość bomb: " + bomby;
        } // Gotowe !

        private void Timer_Tick(object sender, EventArgs e)
        {
            czas++;
            UpdateLabelContent();
        } // Gotowe !

        private void UpdateLabelContent()
        {
            Labelek_czas.Content = "Czas: " + czas + "s";
        } // Gotowe !

        private void RestartApplication()
        {
            // Pobierz ścieżkę do pliku wykonywalnego aplikacji
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            // Uruchom ponownie aplikację
            Process.Start(appPath);

            // Zamknij bieżące okno aplikacji
            Application.Current.Shutdown();
        } // Gotowe !

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            RestartApplication();
        } // Gotowe !

        private void DisableButtonsInGrid()
        {
            foreach (var child in grid.Children)
            {
                if (child is Button)
                {
                    Button button = (Button)child;
                    button.IsEnabled = false;
                }
            }
        }

    }
}
