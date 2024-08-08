using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CountDownApp
{
    // Partial class definition for the HistoryPage
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage()
        {
            InitializeComponent();
            LoadHistory(); // Load the game history when the page is initialized
        }

        // Asynchronously load the game history from a JSON file
        private async void LoadHistory()
        {
            var historyFilePath = Path.Combine(FileSystem.AppDataDirectory, "history.json"); // Path to the history file

            // Check if the history file exists
            if (File.Exists(historyFilePath))
            {
                var historyJson = await File.ReadAllTextAsync(historyFilePath); // Read the file content as a string
                var historyEntries = JsonSerializer.Deserialize<List<GameHistoryEntry>>(historyJson); // Deserialize the JSON content into a list of GameHistoryEntry objects

                // Check if the deserialization was successful
                if (historyEntries != null)
                {
                    // Iterate through each entry in the history and create a view for it
                    foreach (var entry in historyEntries)
                    {
                        var entryView = new StackLayout
                        {
                            Padding = new Thickness(10), // Set padding for each entry view
                            BackgroundColor = Colors.White, // Set background color for each entry view
                            Children =
                            {
                                // Add labels to the entry view to display the game history details
                                new Label { Text = $"Player One: {entry.PlayerOneName}", FontSize = 18, TextColor = Colors.Black },
                                new Label { Text = $"Player Two: {entry.PlayerTwoName}", FontSize = 18, TextColor = Colors.Black },
                                new Label { Text = $"Player One Score: {entry.PlayerOneScore}", FontSize = 18, TextColor = Colors.Black },
                                new Label { Text = $"Player Two Score: {entry.PlayerTwoScore}", FontSize = 18, TextColor = Colors.Black },
                                new Label { Text = $"Timestamp: {entry.Timestamp}", FontSize = 14, TextColor = Colors.Gray }
                            }
                        };

                        // Add the entry view to the stack layout in the XAML
                        historyStackLayout.Children.Add(entryView);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load history entries.", "OK"); // Show an error alert if deserialization fails
                }
            }
            else
            {
                await DisplayAlert("No History", "No game history found.", "OK"); // Show an alert if the history file does not exist
            }
        }
    }
}
