/*
 * Created by SharpDevelop.
 * User: ASUS
 * Date: 02/17/2026
 * Time: 18:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace bomb_game
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		private Random random = new Random();
		private int bombWireIndex = -1;
		private int attemptsRemaining = 5;
		private bool gameActive = false;
		private DispatcherTimer gameTimer;
		private int timeRemaining;
		private int timeLimit;
		private int score;
		private DateTime gameStartTime;
		
		public Window1()
		{
			InitializeComponent();
			InitializeTimer();
		}
		
		void play_button_Click(object sender, RoutedEventArgs e)
		{
			SwitchToPanel("Play");
			InitializeGame();
		}
		
		void settings_button_Click(object sender, RoutedEventArgs e)
		{
			SwitchToPanel("Settings");
		}
		
		void help_button_Click(object sender, RoutedEventArgs e)
		{
			SwitchToPanel("Help");
		}
		
		void about_button_Click(object sender, RoutedEventArgs e)
		{
			SwitchToPanel("About");
		}
		
		void BackToMenu_Click(object sender, RoutedEventArgs e)
		{
			SwitchToPanel("MainMenu");
		}
		
		void Wire_Click(object sender, RoutedEventArgs e)
		{
			if (!gameActive) return;
			
			Button clickedWire = sender as Button;
			int wireIndex = -1;
			
			if (clickedWire == Wire1) wireIndex = 0;
			else if (clickedWire == Wire2) wireIndex = 1;
			else if (clickedWire == Wire3) wireIndex = 2;
			else if (clickedWire == Wire4) wireIndex = 3;
			else if (clickedWire == Wire5) wireIndex = 4;
			
			if (wireIndex == -1) return;
			
			// Play wire pulse animation on each click
			PlayWirePulseAnimation();
			
			if (wireIndex == bombWireIndex)
			{
				// Correct wire - bomb defused!
				gameTimer.Stop();
				CalculateScore();
				ShowWinScreen();
				gameActive = false;
				DisableAllWires();
				PlaySuccessAnimation();
			}
			else
			{
				// Wrong wire - reduce attempts and change bomb wire
				attemptsRemaining--;
				AttemptsText.Text = "Score: " + attemptsRemaining;
				
				// Change bomb wire to a new random position (different from current)
				int newBombWire;
				do
				{
					newBombWire = random.Next(0, 5);
				} while (newBombWire == bombWireIndex);
				
				bombWireIndex = newBombWire;
				
				if (attemptsRemaining <= 0)
				{
					gameTimer.Stop();
					ShowLoseScreen();
					gameActive = false;
					DisableAllWires();
					RevealBombWire();
					PlayExplosionAnimation();
				}
				else
				{
					StatusText.Text = "❌ Wrong wire! The bomb wire has moved! Try again!";
					UpdateTimerDisplay();
					// Play bomb shake animation for wrong choice
					PlayBombShakeAnimation();
					// Keep all wires enabled for next attempt
				}
			}
		}
		
		private void PlayExplosionAnimation()
		{
			try
			{
				Storyboard explosion = (Storyboard)FindResource("ExplosionAnimation");
				if (explosion != null)
				{
					// Reset explosion image
					ExplosionImage.Width = 0;
					ExplosionImage.Height = 0;
					ExplosionImage.Opacity = 0;
					
					// Play the explosion animation
					explosion.Begin();
				}
			}
			catch
			{
				// If animation fails, just show the explosion image
				ExplosionImage.Width = 400;
				ExplosionImage.Height = 400;
				ExplosionImage.Opacity = 1;
			}
		}
		
		private void PlayBombShakeAnimation()
		{
			try
			{
				Storyboard shake = (Storyboard)FindResource("BombShakeAnimation");
				if (shake != null)
				{
					shake.Begin();
				}
			}
			catch
			{
				// If animation fails, do nothing
			}
		}
		
		private void PlayWirePulseAnimation()
		{
			try
			{
				Storyboard pulse = (Storyboard)FindResource("WirePulseAnimation");
				if (pulse != null)
				{
					pulse.Begin();
				}
			}
			catch
			{
				// If animation fails, do nothing
			}
		}
		
		private void PlaySuccessAnimation()
		{
			try
			{
				Storyboard success = (Storyboard)FindResource("SuccessAnimation");
				if (success != null)
				{
					success.Begin();
				}
			}
			catch
			{
				// If animation fails, do nothing
			}
		}
		
		private void SwitchToPanel(string panelName)
		{
			// Hide all panels
			MainMenuPanel.Visibility = Visibility.Collapsed;
			PlayPanel.Visibility = Visibility.Collapsed;
			SettingsPanel.Visibility = Visibility.Collapsed;
			HelpPanel.Visibility = Visibility.Collapsed;
			AboutPanel.Visibility = Visibility.Collapsed;
			
			// Show selected panel
			switch (panelName)
			{
				case "MainMenu":
					MainMenuPanel.Visibility = Visibility.Visible;
					break;
				case "Play":
					PlayPanel.Visibility = Visibility.Visible;
					break;
				case "Settings":
					SettingsPanel.Visibility = Visibility.Visible;
					break;
				case "Help":
					HelpPanel.Visibility = Visibility.Visible;
					break;
				case "About":
					AboutPanel.Visibility = Visibility.Visible;
					break;
			}
		}
		
		private void InitializeTimer()
		{
			gameTimer = new DispatcherTimer();
			gameTimer.Interval = TimeSpan.FromSeconds(1);
			gameTimer.Tick += GameTimer_Tick;
		}
		
		private void GameTimer_Tick(object sender, EventArgs e)
		{
			timeRemaining--;
			UpdateTimerDisplay();
			
			if (timeRemaining <= 0)
			{
				// Time's up - bomb explodes!
				gameTimer.Stop();
				ShowLoseScreen();
				gameActive = false;
				DisableAllWires();
				RevealBombWire();
				PlayExplosionAnimation();
			}
		}
		
		private void CalculateScore()
		{
			TimeSpan timeTaken = DateTime.Now - gameStartTime;
			int baseScore = 100;
			int timeBonus = Math.Max(0, (timeLimit - (int)timeTaken.TotalSeconds) * 2);
			int attemptBonus = attemptsRemaining * 10;
			score = baseScore + timeBonus + attemptBonus;
		}
		
		private void ShowWinScreen()
		{
			StatusText.Text = "🎉 CONGRATULATIONS! You have good luck! 🎉";
			StatusText.Foreground = new SolidColorBrush(Colors.Gold);
			
			// Show score after a delay
			DispatcherTimer scoreTimer = new DispatcherTimer();
			scoreTimer.Interval = TimeSpan.FromSeconds(2);
			scoreTimer.Tick += (s, e) => {
				StatusText.Text = string.Format("🏆 FINAL SCORE: {0} 🏆", score);
				scoreTimer.Stop();
				ShowGameButtons();
			};
			scoreTimer.Start();
		}
		
		private void ShowLoseScreen()
		{
			StatusText.Text = "💥 BOOM! Bomb Exploded! 💥";
			StatusText.Foreground = new SolidColorBrush(Colors.Red);
			
			// Show comforting message after explosion
			DispatcherTimer comfortTimer = new DispatcherTimer();
			comfortTimer.Interval = TimeSpan.FromSeconds(3);
			comfortTimer.Tick += (s, e) => {
				StatusText.Text = "Don't give up! Try again or go back home.";
				StatusText.Foreground = new SolidColorBrush(Colors.White);
				comfortTimer.Stop();
				ShowGameButtons();
			};
			comfortTimer.Start();
		}
		
		private void ShowGameButtons()
		{
			GameOverButtons.Visibility = Visibility.Visible;
		}
		
		private void TryAgain_Click(object sender, RoutedEventArgs e)
		{
			InitializeGame();
		}
		
		private void BackToMenuFromGame_Click(object sender, RoutedEventArgs e)
		{
			SwitchToPanel("MainMenu");
		}
		
		private void UpdateTimerDisplay()
		{
			if (timeRemaining <= 5)
			{
				StatusText.Text = string.Format("⏰ {0}s remaining! HURRY!", timeRemaining);
				StatusText.Foreground = new SolidColorBrush(Colors.Red);
			}
			else if (timeRemaining <= 10)
			{
				StatusText.Text = string.Format("⏰ Time: {0}s", timeRemaining);
				StatusText.Foreground = new SolidColorBrush(Colors.Orange);
			}
			else
			{
				StatusText.Text = string.Format("⏰ Time: {0}s", timeRemaining);
				StatusText.Foreground = new SolidColorBrush(Colors.Lime);
			}
			
			// Also update score display
			AttemptsText.Text = string.Format("Score: {0}", score);
		}
		
		private int GetTimeLimitFromDifficulty()
		{
			var selectedItem = DifficultyComboBox.SelectedItem as ComboBoxItem;
			if (selectedItem != null)
			{
				switch (selectedItem.Content.ToString())
				{
					case "Easy":
						return 20;
					case "Medium":
						return 15;
					case "Hard":
						return 10;
				}
			}
			return 15; // Default to medium
		}
		
		private void InitializeGame()
		{
			// Reset game state
			attemptsRemaining = 5;
			gameActive = true;
			bombWireIndex = random.Next(0, 5); // Random wire 0-4
			timeLimit = GetTimeLimitFromDifficulty();
			timeRemaining = timeLimit;
			score = 0;
			gameStartTime = DateTime.Now;
			
			// Reset UI
			AttemptsText.Text = "Score: " + attemptsRemaining;
			UpdateTimerDisplay();
			
			// Enable and reset all wires
			ResetAllWires();
			
			// Start the timer
			gameTimer.Start();
		}
		
		private void ResetAllWires()
		{
			Wire1.IsEnabled = true;
			Wire1.Opacity = 1.0;
			Wire2.IsEnabled = true;
			Wire2.Opacity = 1.0;
			Wire3.IsEnabled = true;
			Wire3.Opacity = 1.0;
			Wire4.IsEnabled = true;
			Wire4.Opacity = 1.0;
			Wire5.IsEnabled = true;
			Wire5.Opacity = 1.0;
		}
		
		private void DisableAllWires()
		{
			Wire1.IsEnabled = false;
			Wire2.IsEnabled = false;
			Wire3.IsEnabled = false;
			Wire4.IsEnabled = false;
			Wire5.IsEnabled = false;
		}
		
		private void RevealBombWire()
		{
			switch (bombWireIndex)
			{
				case 0:
					Wire1.BorderBrush = new SolidColorBrush(Colors.Red);
					Wire1.BorderThickness = new Thickness(5);
					break;
				case 1:
					Wire2.BorderBrush = new SolidColorBrush(Colors.Red);
					Wire2.BorderThickness = new Thickness(5);
					break;
				case 2:
					Wire3.BorderBrush = new SolidColorBrush(Colors.Red);
					Wire3.BorderThickness = new Thickness(5);
					break;
				case 3:
					Wire4.BorderBrush = new SolidColorBrush(Colors.Red);
					Wire4.BorderThickness = new Thickness(5);
					break;
				case 4:
					Wire5.BorderBrush = new SolidColorBrush(Colors.Red);
					Wire5.BorderThickness = new Thickness(5);
					break;
			}
		}
		
		void play_button_Copy3_Click(object sender, RoutedEventArgs e)
		{
			
		}
	}
}