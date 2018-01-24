using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TicketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task PeriodicAsync(TimeSpan interval)
        {
            while (true)
            {
                await UpdateList();
                await Task.Delay(interval, CancellationToken.None);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await PeriodicAsync(new TimeSpan(0,0,10));
        }

        private async Task UpdateList()
        {
            if (FilmsList.SelectedItems.Count > 0) return;
            FilmsList.Items.Clear();
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConfigurationManager.AppSettings["api"]);
                if (!response.IsSuccessStatusCode) return;
                var product = await response.Content.ReadAsAsync<List<Film>>();
                foreach (var film in product)
                {
                    FilmsList.Items.Add(film);
                }
            }
        }

        private void FilmsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FilmsList.SelectedIndex==-1) return;
            var item = (Film)FilmsList.SelectedItems[0];
            TimeText.Text = item.Time.ToString("dd.MM.yyyy HH:mm");
            FreeText.Text = item.FreeCount.ToString("N");
            PriceText.Text = item.Price.ToString("F");
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new HttpClient())
            {
                var item = (Film)FilmsList.SelectedItems[0];
                var url = ConfigurationManager.AppSettings["api"];
                var ticket = new Ticket
                {
                    FilmId = item.Id,
                    Time = DateTime.Now,
                    Count = Convert.ToInt32(TicketText.Text)
                };
                if (ticket.Count > 0)
                {
                    try
                    {
                        var response = await client.PostAsJsonAsync(url, new Ticket
                        {
                            FilmId = item.Id,
                            Time = DateTime.Now,
                            Count = Convert.ToInt32(TicketText.Text)
                        });
                        var content = await response.Content.ReadAsStringAsync();
                        content = content.Trim('\"');
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show(content,"Продажа билетов", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearSelected();
                        }
                        else
                        {
                            MessageBox.Show(content, "Продажа билетов", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }

        private void ClearSelected()
        {
            TicketText.Text = "0";
            TimeText.Text = "";
            FreeText.Text = "";
            PriceText.Text = "";
            FilmsList.SelectedIndex = -1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSelected();
        }
    }

    public class Film
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public int FreeCount { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    public class Ticket
    {
        public DateTime Time { get; set; }
        public int FilmId { get; set; }
        public int Count { get; set; }
    }
}

