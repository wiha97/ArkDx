using ArkDx.Data;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArkDx.WPF
{
    class Logger
    {
        private MainWindow win;
        private Colour col = new Colour();

        void GetWin()
        {
            win = Application.Current.Windows.OfType<MainWindow>().First();
        }
        public void MsgLog(string text)
        {
            GetWin();
            TextBlock block = new TextBlock()
            {
                Text = text,
                Foreground = col.Green()
            };
            win.logStack.Children.Add(block);
        }
        public void BadLog(string text)
        {
            try
            {
                GetWin();
                TextBox block = new TextBox()
                {
                    Text = text,
                    Foreground = col.LightGray(),
                    Background = col.Red(),
                    BorderThickness = new Thickness(0),
                    IsReadOnly = true
                };
                win.logStack.Children.Add(block);
            }
            catch (Exception e)
            {

            }
        }
        public void Log(string text)
        {
            GetWin();
            TextBox block = new TextBox()
            {
                Text = text,
                Foreground = col.LightGray(),
                Background = null,
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            win.logStack.Children.Add(block);
        }
        public void ListErrors()
        {

        }
        public void ErrorMsg(string error)
        {
            MessageBoxImage img = MessageBoxImage.Error;
            MessageBoxButton btn = MessageBoxButton.OKCancel;
            MessageBox.Show(error, "Shisno!", btn, img);
        }
        public void FirstMsg()
        {
            MessageBoxImage img = MessageBoxImage.Information;
            MessageBoxButton btn = MessageBoxButton.OK;
            MessageBox.Show("Settings file missing!\nPlease fill out the settings for more usability.", "First launch?", btn, img);
        }
        public void UpdateMsg(string text, string version, string dl)
        {
            MessageBoxImage img = MessageBoxImage.Information;
            MessageBoxButton btn = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show($"Changelog:\n\n{text}\n\nWould you like to download it? (will open a browser)\n{dl}", $"Version {version} is available!", btn, img);
            if(result == MessageBoxResult.Yes)
            {
                Process.Start(dl);
            }
        }
    }
}
