using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Reservations;

namespace Client {
    public partial class MainWindow : Window {
        private static readonly string[] Months = {"Janvier", "Février", "Mars", "Avril", "Mai", "Juin", "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"};
        
        private Dictionary<string, List<int>> _reservations;
        private Dictionary<string, List<int>> _inversions;

        private readonly Importer _reservationsImporter = new Importer("https://github.com/GiteSabotDeVenus/data", @"data\reservations", "data");
        private readonly Exporter _reservationsExporter = new Exporter("https://github.com/GiteSabotDeVenus/data", @"data\reservations");

        private int _showedMonth = 1;
        private int _showedYear = 1970;

        public MainWindow() {
            InitializeComponent();
        }
        
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            _reservationsImporter.ImportReservations();
            _reservations = _reservationsImporter.GetReservations();
            _inversions = new Dictionary<string, List<int>>();
            ShowDates(DateTime.Now.Year, DateTime.Now.Month);
        }
        
        private void SaveButton_OnClick(object sender, RoutedEventArgs e) {
            ProgressBar.Value = 0;
            
            foreach (Button button in DaysGrid.Children) {
                button.IsEnabled = false;
            }
            
            ProgressTextBlock.Text = DateTime.Now + " - Enregistrement...";
            
            _reservationsExporter.SetReservations(_reservations, _inversions);
            
            ProgressTextBlock.Text = DateTime.Now + " - Export...";
            
            _reservationsExporter.ExportReservations();
            
            ProgressBar.Value = 100;
            ProgressTextBlock.Text = DateTime.Now + " - Opération terminée.";

            foreach (Button button in DaysGrid.Children) {
                button.IsEnabled = true;
            }
        }
        
        private void PreviousMonthButton_OnClick(object sender, RoutedEventArgs e) {
            if (_showedMonth - 1 < 1) {
                ShowDates(_showedYear - 1, 12);
            } else {
                ShowDates(_showedYear, _showedMonth - 1);
            }
        }

        private void NextMonthButton_OnClick(object sender, RoutedEventArgs e) {
            if (_showedMonth + 1 > 12) {
                ShowDates(_showedYear + 1, 1);
            } else {
                ShowDates(_showedYear, _showedMonth + 1);
            }
        }

        private void ShowDates(int year, int month) {
            DaysGrid.Children.Clear();
            _showedMonth = month;
            _showedYear = year;
            
            CalendarTitleTextBlock.Text = Months[month-1] + " " + year;
            
            var reservations = _reservations.ContainsKey($"{month - 1}-{year}") ? _reservations[$"{month - 1}-{year}"] : new List<int>();
            var inversions = _inversions.ContainsKey($"{month - 1}-{year}") ? _inversions[$"{month - 1}-{year}"] : new List<int>();
            
            var number = 1;
            var max = DateTime.DaysInMonth(year, month);
            
            for (var y = 1; y < 10; y+=2) {
                if (number > max) {
                    break;
                }
                
                for (var x = 1; x < 14; x+=2) {
                    if (number > max) {
                        break;
                    }
                    
                    ShowDate(x, y, number, reservations, inversions);
                    number++;
                }
            }
        }

        private void ShowDate(int x, int y, int number, List<int> reservations, List<int> inversions) {
            var day = new Button();
                    
            day.Name = "Day" + number;
            day.Content = number;
                    
            day.Width = 50;
            day.Height = 50;
                    
            day.FontSize = 24;
            day.FontWeight = FontWeights.Bold;

            day.HorizontalAlignment = HorizontalAlignment.Center;
            day.VerticalAlignment = VerticalAlignment.Center;
                    
            day.SetValue(Grid.RowProperty, y);
            day.SetValue(Grid.ColumnProperty, x);

            if ((reservations.Contains(number) && !(inversions.Contains(number))) || (!reservations.Contains(number) && inversions.Contains(number))) {
                day.Background = Brushes.Crimson;
            } else {
                day.Background = Brushes.Chartreuse;
            }

            day.Click += (sender, e) => {
                var inversionKey = $"{_showedMonth - 1}-{_showedYear}";
                List<int> currentInversions = _inversions.ContainsKey(inversionKey) ? _inversions[inversionKey] : new List<int>();
                
                int dayNumber = int.Parse(day.Content.ToString());
                
                if (currentInversions.Contains(dayNumber)) {
                    currentInversions.Remove(dayNumber);
                } else {
                    currentInversions.Add(dayNumber);
                }
                
                _inversions[inversionKey] = currentInversions;
                ShowDates(_showedYear, _showedMonth);
            };
                            
            DaysGrid.Children.Add(day);
        }
        
    }
}
