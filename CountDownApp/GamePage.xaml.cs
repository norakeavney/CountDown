using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CountDownApp
{
    // Class to represent game history entry - will be displayed in the historypage
    public class GameHistoryEntry
    {
        public string PlayerOneName { get; set; }
        public string PlayerTwoName { get; set; }
        public int PlayerOneScore { get; set; }
        public int PlayerTwoScore { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public partial class GamePage : ContentPage
    {
        private TimeSpan _timeLeft; // Time remaining in the round
        private bool _isTimerRunning; // Timer status
        private int _currentColumn; // Current column index in the letter grid
        private List<char> _gridLetters; // List of letters in the grid
        private HashSet<string> _dictionary; // Set of valid words from the dictionary
        private const string DictionaryFileName = "cdwords.txt"; // Dictionary file name
        private readonly string _dictionaryFilePath; // Path to the dictionary file
        private string _playerOneName; // Name of Player One
        private string _playerTwoName; // Name of Player Two
        private int _playerOnePoints; // Points scored by Player One
        private int _playerTwoPoints; // Points scored by Player Two
        private int _currentRound; // Current round number
        private string _playerOneWord; // Word entered by Player One
        private string _playerTwoWord; // Word entered by Player Two

        // Lists of vowels and consonants
        private static readonly List<char> Vowels = new List<char> { 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'E', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'I', 'O', 'O', 'O', 'O', 'O', 'O', 'O', 'O', 'O', 'O', 'O', 'O', 'O', 'U', 'U', 'U', 'U', 'U', };
        private static readonly List<char> Consonants = new List<char> { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

        public GamePage()
        {
            InitializeComponent();
            InitializeGrid(); // Sets up the letter grid
            //Initialize points for both players
            _playerOnePoints = 0;
            _playerTwoPoints = 0; 
            _currentRound = 1; // Start at round 1
            _isTimerRunning = false; // Timer isnt running at beginning

            _dictionaryFilePath = Path.Combine(FileSystem.AppDataDirectory, DictionaryFileName); // Set the dictionary file path

            LoadDictionaryAsync(); // Load the dictionary
            PromptForPlayerNames(); // Prompt players to enter their names
        }

        // Load the dictionary from a file or download it if it doesn't exist
        private async void LoadDictionaryAsync()
        {
            if (!File.Exists(_dictionaryFilePath))
            {
                await DownloadWordList(_dictionaryFilePath);
            }

            // Read from the local file
            try
            {
                var words = await File.ReadAllLinesAsync(_dictionaryFilePath);
                _dictionary = new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading dictionary: {ex.Message}");
                await DisplayAlert("Error", "Failed to load dictionary. Please check your internet connection.", "OK");
            }
        }

        // Download the word list from the specified URL and save it to a file
        private async Task DownloadWordList(string filePath)
        {
            var client = new HttpClient();
            var wordsUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/cdwords.txt";

            try
            {
                var response = await client.GetAsync(wordsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await File.WriteAllTextAsync(filePath, content);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network issues)
                Debug.WriteLine($"Error downloading word list: {ex.Message}");
                await DisplayAlert("Error", "Failed to download dictionary. Please check your internet connection.", "OK");
            }
        }

        // Initialize the grid - empty at start
        private void InitializeGrid()
        {
            _currentColumn = 0;
            _gridLetters = new List<char>();

            letterGrid.Children.Clear(); // Clears existing letters to avoid duplication

            for (int row = 0; row < 1; row++) // 1 row
            {
                for (int col = 0; col < 9; col++) // 9 columns
                {
                    var entry = new Entry
                    {
                        Placeholder = " ",
                        MaxLength = 1,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(5),
                        FontSize = 24, // Adjust font size to fit within the boxes
                        TextColor = Colors.Black,
                        BackgroundColor = Color.FromArgb("#FFFFFF"), // white background for the entry
                     
                    };

                    // Set the row and column for each entry
                    Grid.SetRow(entry, row);
                    Grid.SetColumn(entry, col);

                    // Add the entry to the grid
                    letterGrid.Children.Add(entry);
                }
            }
        }

        // Prompt the players to enter their names
        private async Task PromptForPlayerNames()
        {
            _playerOneName = await DisplayPromptAsync("Player One", "Please enter your name:");
            if (string.IsNullOrEmpty(_playerOneName))
            {
                _playerOneName = "Player One"; // Default name if none entered
            }

            _playerTwoName = await DisplayPromptAsync("Player Two", "Please enter your name:");
            if (string.IsNullOrEmpty(_playerTwoName))
            {
                _playerTwoName = "Player Two"; // Default name if none entered
            }

            await DisplayAlert("Welcome To Countdown", $"Player One: {_playerOneName}\nPlayer Two: {_playerTwoName}", "Let's Go!");

            UpdateRoundAndPointsDisplay(); // Update the display to show the round and points
        }

        // Start the timer
        private void StartTimer()
        {
            _timeLeft = TimeSpan.FromSeconds(30);
            _isTimerRunning = true;

            this.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (_isTimerRunning)
                {
                    _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                    timerLabel.Text = $"Timer: {_timeLeft:mm\\:ss}";

                    if (_timeLeft == TimeSpan.Zero)
                    {
                        _isTimerRunning = false;
                        timerLabel.Text = "Timer: 00:00:00";
                        ShowWordEntryPrompts();
                        return false; // Stop the timer
                    }

                    return true; // Repeat again
                }
                return false; // Stop the timer
            });
        }

        // Stop the timer
        private void StopTimer()
        {
            _isTimerRunning = false;
        }

        // Event handler for vowel button click
        private void OnVowClicked(object sender, EventArgs e)
        {
            if (_currentColumn < 9)
            {
                AddLetterToGrid(GetRandomVowel());
            }
        }

        // Event handler for consonant button click
        private void OnConClicked(object sender, EventArgs e)
        {
            if (_currentColumn < 9)
            {
                AddLetterToGrid(GetRandomConsonant());
            }
        }

        // Add a letter to the grid
        private void AddLetterToGrid(char letter)
        {
            if (_currentColumn < 9)
            {
                var entry = (Entry)letterGrid.Children[_currentColumn];
                entry.Text = letter.ToString();
                _gridLetters.Add(letter); // Add letter to the grid letters list
                _currentColumn++;
            }
        }

        // Get a random vowel
        private char GetRandomVowel()
        {
            Random random = new Random();
            int index = random.Next(Vowels.Count);
            return Vowels[index];
        }

        // Get a random consonant
        private char GetRandomConsonant()
        {
            Random random = new Random();
            int index = random.Next(Consonants.Count);
            return Consonants[index];
        }

        // Event handler for the Start Game button click
        private async void OnStartGameClicked(object sender, EventArgs e)
        {
            if (_currentColumn < 9)
            {
                await DisplayAlert("Grid not Complete", "Please select vowel or consonant to continue", "OK");
            }
            else if (_currentColumn == 9)
            {
                if (!_isTimerRunning)
                {
                    StartTimer();
                }

                if (_currentRound == 1)
                {
                    _gridLetters.Clear();
                    foreach (var child in letterGrid.Children)
                    {
                        if (child is Entry entry && !string.IsNullOrEmpty(entry.Text))
                        {
                            _gridLetters.Add(entry.Text[0]);
                        }
                    }
                }

                Debug.WriteLine("Letters in grid: " + string.Join(", ", _gridLetters));

                UpdateRoundAndPointsDisplay();
            }
        }

        // Show prompts for players to enter their words
        private async void ShowWordEntryPrompts()
        {
            _playerOneWord = await DisplayPromptAsync(_playerOneName, "Please enter a word using the given letters:");
            if (!string.IsNullOrEmpty(_playerOneWord))
            {
                await DisplayAlert("Word Entered", $"{_playerOneName} entered: {_playerOneWord}", "OK");
            }

            _playerTwoWord = await DisplayPromptAsync(_playerTwoName, "Please enter a word using the given letters:");
            if (!string.IsNullOrEmpty(_playerTwoWord))
            {
                await DisplayAlert("Word Entered", $"{_playerTwoName} entered: {_playerTwoWord}", "OK");
            }

            await ValidateAndScoreWords(_playerOneWord, _playerTwoWord);
        }

        // Validate the entered words and score them
        private async Task ValidateAndScoreWords(string playerOneWord, string playerTwoWord)
        {
            bool playerOneWordValid = ValidateWord(playerOneWord);
            bool playerTwoWordValid = ValidateWord(playerTwoWord);

            if (!playerOneWordValid)
            {
                await DisplayAlert("Invalid Word", $"{_playerOneName}'s word contains letters not in the grid or is not a valid word.", "OK");
            }

            if (!playerTwoWordValid)
            {
                await DisplayAlert("Invalid Word", $"{_playerTwoName}'s word contains letters not in the grid or is not a valid word.", "OK");
            }

            int playerOneWordLength = playerOneWordValid ? playerOneWord.Length : 0;
            int playerTwoWordLength = playerTwoWordValid ? playerTwoWord.Length : 0;

            string lengthMessage = $"{_playerOneName} Word Length: {playerOneWordLength}\n{_playerTwoName} Word Length: {playerTwoWordLength}";
            await DisplayAlert("Word Length Analysis", lengthMessage, "OK");

            // Update points based on the length of valid words
            if (playerOneWordLength > playerTwoWordLength)
            {
                _playerOnePoints += playerOneWordLength;
            }
            else if (playerTwoWordLength > playerOneWordLength)
            {
                _playerTwoPoints += playerTwoWordLength;
            }
            else if (playerTwoWordLength == playerOneWordLength)
            {
                _playerTwoPoints += playerTwoWordLength;
                _playerOnePoints += playerOneWordLength;
            }

            _currentRound++;
            if (_currentRound <= 6)
            {
                InitializeGrid();
                _currentColumn = 0; // Reset column for new letters
                StopTimer(); // Ensure the timer is stopped for the next round
                await DisplayAlert("Next Round", "Press Start Game to begin the next round.", "OK");
            }
            else
            {
                string finalMessage = $"Final Points:\n{_playerOneName}: {_playerOnePoints}\n{_playerTwoName}: {_playerTwoPoints}";
                await DisplayAlert("Game Over", finalMessage, "OK");
                await EndGame(); // Save the game history and show options to start a new game or go back to home
            }

            UpdateRoundAndPointsDisplay();
        }

        // Validate if the word is in the dictionary and uses only available letters
        private bool ValidateWord(string word)
        {
            if (_dictionary == null || !_dictionary.Contains(word))
            {
                return false; // Word is not in the dictionary
            }

            List<char> availableLetters = new List<char>(_gridLetters);
            foreach (char letter in word.ToUpper())
            {
                if (availableLetters.Contains(letter))
                {
                    availableLetters.Remove(letter);
                }
                else
                {
                    return false; // Letter not available
                }
            }
            return true; // All letters are available and the word is valid
        }

        // Show end game options to start a new game or go back to the home page
        private async Task ShowEndGameOptions()
        {
            bool startNewGame = await DisplayAlert("Game Over", "Do you want to start a new game?", "Yes", "No");

            if (startNewGame)
            {
                StartNewGame();
            }
            else
            {
                await Navigation.PopAsync(); // Go back to the home page or previous page
            }
        }

        // Start a new game
        private void StartNewGame()
        {
            _playerOnePoints = 0;
            _playerTwoPoints = 0;
            _currentRound = 1;
            InitializeGrid();
            UpdateRoundAndPointsDisplay();
            StopTimer(); // Ensure the timer is stopped at the beginning
        }

        // Update the display to show the current round and points
        private void UpdateRoundAndPointsDisplay()
        {
            roundLabel.Text = $"Round: {_currentRound}";
            playerOnePointsLabel.Text = $"{_playerOneName} Points: {_playerOnePoints}";
            playerTwoPointsLabel.Text = $"{_playerTwoName} Points: {_playerTwoPoints}";
        }

        // Save the game history to a JSON file
        private async Task SaveGameHistory(string playerOneName, string playerTwoName, int playerOneScore, int playerTwoScore)
        {
            var historyEntry = new GameHistoryEntry
            {
                PlayerOneName = playerOneName,
                PlayerTwoName = playerTwoName,
                PlayerOneScore = playerOneScore,
                PlayerTwoScore = playerTwoScore,
                Timestamp = DateTime.Now
            };

            var historyFilePath = Path.Combine(FileSystem.AppDataDirectory, "history.json");

            List<GameHistoryEntry> historyEntries;
            if (File.Exists(historyFilePath))
            {
                var existingHistoryJson = await File.ReadAllTextAsync(historyFilePath);
                historyEntries = JsonSerializer.Deserialize<List<GameHistoryEntry>>(existingHistoryJson) ?? new List<GameHistoryEntry>();
            }
            else
            {
                historyEntries = new List<GameHistoryEntry>();
            }

            historyEntries.Add(historyEntry);

            var newHistoryJson = JsonSerializer.Serialize(historyEntries);
            await File.WriteAllTextAsync(historyFilePath, newHistoryJson);
        }

        // End the game and save the history
        private async Task EndGame()
        {
            await SaveGameHistory(_playerOneName, _playerTwoName, _playerOnePoints, _playerTwoPoints);
            await ShowEndGameOptions();
        }
    }
}
