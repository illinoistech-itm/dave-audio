using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FinalProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminPage : Page
    {
        public AdminPage()
        {
            this.InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            usersAllowedDropdown.Items.Clear();
            usersNotAllowedDropdown.Items.Clear();
            List<User> users = SqliteTransactions.GetAllUsers();
            foreach (User user in users)
            {
                if (!user.userId.Equals("A20387795"))
                {
                    if (user.access)
                    {
                        usersAllowedDropdown.Items.Add(user.userId + ", " + user.firstName + " " + user.lastName);
                    }
                    else
                    {
                        usersNotAllowedDropdown.Items.Add(user.userId + ", " + user.firstName + " " + user.lastName);
                    }
                }        
            }
        }

        private async void doorButtonClicked(object sender, RoutedEventArgs e)
        {
            Synthesizer.Speak("Please come on in.");
            //OPEN DOOR!!!!
            MainPage.DLPin.Write(Windows.Devices.Gpio.GpioPinValue.High);
            await Task.Delay(10000);         
            MainPage.DLPin.Write(Windows.Devices.Gpio.GpioPinValue.Low);
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void denyAccessButtonClicked(object sender, RoutedEventArgs e)
        {
            if (usersAllowedDropdown.SelectedItem == null)
            {
                Synthesizer.Speak("Please select the user to which you want to deny access to the room.");
            }
            else
            {
                String[] userInfo = usersAllowedDropdown.SelectedItem.ToString().Split(',');
                ContentDialog deleteUserDialog = new ContentDialog
                {
                    Title = "Are you sure you want to deny access to " + userInfo[1] + " ?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "Cancel",
                };

                ContentDialogResult result = await deleteUserDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    SqliteTransactions.ModifyAccessByUserId(userInfo[0], false);
                    LoadUsers();
                }
                else
                {
                    deleteUserDialog.Hide();
                }
            }
        }

        private async void allowAccessButtonClicked(object sender, RoutedEventArgs e)
        {
            if (usersNotAllowedDropdown.SelectedItem == null)
            {
                Synthesizer.Speak("Please select the user you want to allow in the room.");
            }
            else
            {
                String[] userInfo = usersNotAllowedDropdown.SelectedItem.ToString().Split(',');
                ContentDialog deleteUserDialog = new ContentDialog
                {
                    Title = "Are you sure you want to allow " + userInfo[1] + " in the room?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "Cancel",
                };

                ContentDialogResult result = await deleteUserDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    SqliteTransactions.ModifyAccessByUserId(userInfo[0], true);
                    LoadUsers();
                }
                else
                {
                    deleteUserDialog.Hide();
                }
            }
        }

        private void backButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
