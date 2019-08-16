using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DiffusionKeywordAggregator;

namespace ChartView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        HashSet<string> words = new HashSet<string>();
        public MainWindow()
        {
            InitializeComponent();
            consoleBox.IsReadOnly = true;
            consoleBox.Text = "";
            consoleBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            consoleBox.FontSize = 12;

            if (Application.Current == null)
                new Application();
            keywordBox.Text = "apple";
         }
        public delegate void UpdateTextCallback(string message);
        string prev = "";

        private void UpdateText(string message)
        {
            string res = GenericGatherAgent.pub.res;
            if (res != message)
                consoleBox.AppendText(res + "\n");
            prev = res;
        }


        private async void updateBox()
        {
            while (true)
            {
                await Task.Delay(500);
                consoleBox.Dispatcher.Invoke(
                    new UpdateTextCallback(UpdateText),
                    prev
                    );

            }
        }

        private void LetsGoButton_Click(object sender, RoutedEventArgs e)
        {
            if (keywordBox.Text.Length < 0)
            {

            }
            else
            {
                if (words.Contains(keywordBox.Text))
                {
                    MessageBox.Show("Word " + keywordBox.Text + " is already being tracked, try another word");
                    return;
                }
                words.Add(keywordBox.Text);
                RedditAgent r = new RedditAgent(keywordBox.Text);
                TwitterAgent t = new TwitterAgent(keywordBox.Text);

                _ = Task.Run(() => {
                    r.Run();
                    t.Run();
                });

                var a = MessageBox.Show("Publishing results of " + keywordBox.Text);
                Thread update = new Thread(new ThreadStart(updateBox));
                update.Start();

            }
        }
        public void OnWindowclose(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode); // Prevent memory leak
        }
    }
}
