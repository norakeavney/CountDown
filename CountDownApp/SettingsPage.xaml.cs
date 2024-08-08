using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace CountDownApp
{
    // Partial class definition for the SettingsPage
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent(); // Initialize the page components
        }

        // Event handler for the theme toggle button click
        void onThemeClicked(object sender, EventArgs e)
        {
            // Toggle between light and dark theme
            if (Application.Current.UserAppTheme == AppTheme.Light)
            {
                // Change to dark theme
                Application.Current.Resources["ButtonStyle"] = Application.Current.Resources["DarkThemeButton"];
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                // Change to light theme
                Application.Current.Resources["ButtonStyle"] = Application.Current.Resources["LightThemeButton"];
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }

        // Event handler for the clear data button click
        private async void onClearDataClicked(object sender, EventArgs e)
        {
            var historyFilePath = Path.Combine(FileSystem.AppDataDirectory, "history.json"); // Path to the history file

            // Check if the history file exists
            if (File.Exists(historyFilePath))
            {
                try
                {
                    File.Delete(historyFilePath); // Delete the history file
                    await DisplayAlert("Success", "History data cleared.", "OK"); // Show success alert
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to clear history data: {ex.Message}", "OK"); // Show error alert
                }
            }
            else
            {
                await DisplayAlert("No History", "No game history found to clear.", "OK"); // Show alert if no history file exists
            }
        }

        // Event handler for the home button click
        private async void onHomeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage()); // Navigate to the MainPage
        }
    }
}
