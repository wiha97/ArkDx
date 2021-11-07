using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ArkDx.Data;
using ArkDx.WPF;

namespace ArkDx
{
    public partial class MainWindow : Window
    {
        private WPFWindow wpf = new WPFWindow();
        public MainWindow()
        {
            InitializeComponent();
            wpf.Startup();
        }
        private void btnApp(object sender, RoutedEventArgs e)
        {
            Process.Start(Settings.AppData);    //  Opens AppData folder
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Settings.ArkPath);    // Opens archive folder
        }

        private void btnArch(object sender, RoutedEventArgs e)
        {
            wpf.GetArk();
        }

        private void btnClear(object sender, RoutedEventArgs e)
        {
            logStack.Children.Clear();  //  Clears log
        }

        private void autoCheck(object sender, RoutedEventArgs e)
        {
            wpf.TimerOn();  //  Starts autorefresh
        }

        private void unAutoCheck(object sender, RoutedEventArgs e)
        {
            wpf.TimerOff(); //  Stops autorefresh
        }
        private void libBtn(object sender, RoutedEventArgs e)
        {
            Process.Start("Library");   //  Opens Library folder
        }

        private void applyBtn_Click(object sender, RoutedEventArgs e)
        {
            wpf.ApplySettings();    //  Applies settings
        }

        private void setPander_Collapsed(object sender, RoutedEventArgs e)
        {
            setPander.Width = 100;
            setPander.Height = 25;
        }

        private void setPander_Expanded(object sender, RoutedEventArgs e)
        {
            setPander.Width = 219;
            setPander.Height = 480; //  240
        }

        private void multiBox_Checked(object sender, RoutedEventArgs e)
        {
            wpf.MultiOn();  //  Enables multithreading
        }

        private void multiBox_Unchecked(object sender, RoutedEventArgs e)
        {
            wpf.MultiOff(); //  Disabled multithreading
        }

        private void clipBtn_Copy_Click(object sender, RoutedEventArgs e)
        {
            Settings.AddClip(clipBox.Text); //  Adds clip path
        }

        private void soundBtn_Click(object sender, RoutedEventArgs e)
        {
            wpf.SetSound(SoundCombo.SelectedItem.ToString());
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            wpf.Update();
        }

        private void repoBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(urlBox.Text);
        }

        private void SoundCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            soundBox.Text = Settings.ErrorValue(SoundCombo.SelectedItem.ToString());
        }

        private void playSoundBtn_Click(object sender, RoutedEventArgs e)
        {
            Sound sound = new Sound();
            sound.Custom(Settings.ErrorValue(SoundCombo.SelectedItem.ToString()));
        }

        private void spCheck_Checked(object sender, RoutedEventArgs e)
        {
            Settings.MedalSource = @"Library\spmedals.txt";
            medalCombo.ItemsSource = File.ReadAllLines(Settings.MedalSource);
        }

        private void spCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.MedalSource = @"Library\mpmedals.txt";
            medalCombo.ItemsSource = File.ReadAllLines(Settings.MedalSource);
        }

        private void medalCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            medalBox.Text = medalCombo.SelectedItem.ToString();
        }

        private void medalBtn_Click(object sender, RoutedEventArgs e)
        {
            int idx = medalCombo.SelectedIndex;
            string[] mArr = File.ReadAllLines(Settings.MedalSource);
            mArr[idx] = medalBox.Text;
            File.WriteAllLines(Settings.MedalSource, mArr);
            medalCombo.ItemsSource = File.ReadAllLines(Settings.MedalSource);
            medalCombo.SelectedIndex = idx;
        }
    }
}
