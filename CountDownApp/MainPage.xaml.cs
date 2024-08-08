namespace CountDownApp
{
    // Partial class definition for the MainPage
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadWordList(); // Load the word list when the page is initialized
        }

        // Event handler for the Start New Game button click
        private async void OnStartNewGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GamePage()); // Navigate to the GamePage
        }

        // Event handler for the Game History button click
        private async void OnGameHistoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistoryPage()); // Navigate to the HistoryPage
        }

        // Event handler for the Settings button click
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage()); // Navigate to the SettingsPage
        }

        // Load the word list from a local file or download it if it doesn't exist
        private async void LoadWordList()
        {
            var localFolder = FileSystem.AppDataDirectory; // Get the local folder path
            var localFilePath = Path.Combine(localFolder, "words.txt"); // Define the local file path

            // Check if the word list file exists
            if (!File.Exists(localFilePath))
            {
                await DownloadWordList(localFilePath); // Download the word list if the file does not exist
            }
        }

        // Download the word list from the specified URL and save it to a file
        private async Task DownloadWordList(string filePath)
        {
            var client = new HttpClient();
            var wordsUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/cdwords.txt"; // URL to download the word list

            try
            {
                var response = await client.GetAsync(wordsUrl); // Send a GET request to the URL
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(); // Read the response content as a string
                    File.WriteAllText(filePath, content); // Write the content to the local file
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Downloading the Word List: {ex.Message}"); // Log the error message
            }
        }
    }
}
