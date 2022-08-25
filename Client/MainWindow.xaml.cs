using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace Client {
    public partial class MainWindow : Window {
        private string[] _months = {"Janvier", "Février", "Mars", "Avril", "Mai", "Juin", "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"};
        private Dictionary<string, List<int>> _reservations = new Dictionary<string, List<int>>();
        private Dictionary<string, List<int>> _inverted = new Dictionary<string, List<int>>();

        private int _showedMonth = 1;
        private int _showedYear = 1970;

        public MainWindow() {
            InitializeComponent();
        }
        
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            Execute("rmdir /s /q data");
            ExecuteGit("clone https://github.com/GiteSabotDeVenus/data");
            string[] content = File.ReadAllLines(@"data\reservations");
            foreach (string line in content) {
                string[] data = line.Split(":");
                string date = data[0];
                List<int> days = new List<int>();
                foreach (string day in data[1].Split(";")) {
                    try {
                        days.Add(Int32.Parse(day));
                    } catch {}
                }
                _reservations[date] = days;
            }
            ShowDays(DateTime.Now.Year, DateTime.Now.Month);
        }

        private void ShowDays(int year, int month) {
            DaysGrid.Children.Clear();
            CalendarTitleTextBlock.Text = _months[month-1] + " " + year;
            _showedMonth = month;
            _showedYear = year;
            int max = DateTime.DaysInMonth(year, month);
            int number = 1;
            List<int> reservations = _reservations.ContainsKey($"{month - 1}-{year}") ? _reservations[$"{month - 1}-{year}"] : new List<int>();
            List<int> inverted = _inverted.ContainsKey($"{month - 1}-{year}") ? _inverted[$"{month - 1}-{year}"] : new List<int>() {};
            for (int y = 1; y < 10; y+=2) {
                if (number <= max) {
                    for (int x = 1; x < 14; x+=2) {
                        if (number <= max) {
                            Button day = new Button();
                            day.Name = "Day" + number;
                            day.Width = 50;
                            day.Height = 50;
                            day.Content = number;
                            day.FontSize = 24;
                            day.Background = (reservations.Contains(number) && !(inverted.Contains(number))) || (!reservations.Contains(number) && inverted.Contains(number)) ? Brushes.Crimson : Brushes.Chartreuse;
                            day.FontWeight = FontWeights.Bold;
                            day.HorizontalAlignment = HorizontalAlignment.Center;
                            day.VerticalAlignment = VerticalAlignment.Center;
                            day.SetValue(Grid.RowProperty, y);
                            day.SetValue(Grid.ColumnProperty, x);

                            day.Click += (sender, e) => {
                                List<int> currentlyInverted = _inverted.ContainsKey($"{_showedMonth - 1}-{_showedYear}") ? _inverted[$"{_showedMonth - 1}-{_showedYear}"] : new List<int>() {};
                                int dayNumber = int.Parse(day.Content.ToString());
                                if (currentlyInverted.Contains(dayNumber)) {
                                    currentlyInverted.Remove(dayNumber);
                                }
                                else {
                                    currentlyInverted.Add(dayNumber);
                                }
                                _inverted[$"{_showedMonth - 1}-{_showedYear}"] = currentlyInverted;
                                ShowDays(_showedYear, _showedMonth);
                            };
                            
                            DaysGrid.Children.Add(day);
                            number += 1;
                        }
                        else {
                            break;
                        }
                    }
                }
                else {
                    break;
                }
            }
        }

        private static void ExecuteGit(string command) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "git.exe";
            startInfo.Arguments = command;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
        
        private static void ExecuteGitInRepo(string command) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "git.exe";
            startInfo.Arguments = command;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = "data";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        private static void Execute(string command) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c" + command;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        private void PreviousMonthButton_OnClick(object sender, RoutedEventArgs e) {
            if (_showedMonth - 1 < 1) {
                ShowDays(_showedYear - 1, 12);
            }
            else {
                ShowDays(_showedYear, _showedMonth - 1);
            }
        }

        private void NextMonthButton_OnClick(object sender, RoutedEventArgs e) {
            if (_showedMonth + 1 > 12) {
                ShowDays(_showedYear + 1, 1);
            }
            else {
                ShowDays(_showedYear, _showedMonth + 1);
            }
        }
        
        private void SaveButton_OnClick(object sender, RoutedEventArgs e) {
            foreach (Button button in DaysGrid.Children) {
                button.IsEnabled = false;
            }

            ProgressBar.Maximum = _inverted.Count;
            ProgressTextBlock.Text = "Inversion des dates changées...";

            foreach (string invertedKey in _inverted.Keys) {
                List<int> invertedDays = _inverted[invertedKey];
                List<int> reservedDays = _reservations.ContainsKey(invertedKey) ? _reservations[invertedKey] : new List<int>();

                foreach (int invertedDay in invertedDays) {
                    if (reservedDays.Contains(invertedDay)) {
                        reservedDays.Remove(invertedDay);
                    }
                    else {
                        reservedDays.Add(invertedDay);
                    }

                    ProgressBar.Value += 1;
                }

                _reservations[invertedKey] = reservedDays;
            }
            
            _inverted.Clear();

            ProgressBar.Value = 0;
            ProgressBar.Maximum = _reservations.Count;
            ProgressTextBlock.Text = DateTime.Now + "Transformation des données...";

            List<string> reservationsText = new List<string>();

            foreach (string reservedKey in _reservations.Keys) {
                int month = int.Parse(reservedKey.Split("-")[0]) + 1;
                int year = int.Parse(reservedKey.Split("-")[1]);
                DateTime dateTime = DateTime.Now;
                int actualMonth = dateTime.Month;
                int actualYear = dateTime.Year;

                if ((year > actualYear) || (year == actualYear && month >= actualMonth)) {
                    if (_reservations[reservedKey].Count > 0) {
                        string reservationText = "";
                        reservationText += reservedKey + ":";
                        foreach (int day in _reservations[reservedKey]) {
                            reservationText += day + ";";
                        }
                        reservationsText.Add(reservationText);
                    }
                }
                
                ProgressBar.Value += 1;
            }
            
            ProgressTextBlock.Text = DateTime.Now + "Enregistrement...";
            
            File.WriteAllLines(@"data\reservations", reservationsText);
            
            ProgressBar.Value = 0;
            ProgressBar.Maximum = 3;
            ProgressTextBlock.Text = "Préparation à l'envoi...";
            
            ExecuteGitInRepo("add reservations");

            ProgressBar.Value = 1;

            ExecuteGitInRepo("commit -m \"Update reservations\"");
            
            ProgressBar.Value = 2;
            ProgressTextBlock.Text = DateTime.Now + "Envoi des données...";

            ExecuteGitInRepo($"push https://github.com/GiteSabotDeVenus/data --all");
            
            ProgressBar.Value = 3;

            foreach (Button button in DaysGrid.Children) {
                button.IsEnabled = true;
            }
            
            ProgressBar.Value = 0;
            ProgressTextBlock.Text = DateTime.Now + " - Opération terminée.";
        }
    }
}
