using Blackjack.Controls;
using GameCardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UtilitiesLib;

namespace Blackjack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<PlayerPanel> PlayerPanels { get; set; }
        public GameManager GameManager { get; set; }

        public MainWindow()
        {
            PlayerPanels = new List<PlayerPanel>();
            GameManager = new GameManager();
            InitializeComponent();
        }

        private void BtnNewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameWindow ngw = new NewGameWindow();
            ngw.ShowDialog();
            (Player dealer, List<Player> players) = ngw.StartingPlayers;

            PlayerPanel dealerPanel = new PlayerPanel(dealer);
            players.ForEach(p => PlayerPanels.Add(new PlayerPanel(p)));

            DealerSection.Children.Add(dealerPanel);
            Player firstPlayer = GameManager.ContinueRound(ShowWinner);

            if (firstPlayer != null)
            {
                PlayerSection.Children.Add(
                    PlayerPanels.FirstOrDefault(p => p.Player.PlayerId == firstPlayer.PlayerId));
            }
            LabelNbrOfDecks.Content = $"Decks: {GameManager.Deck.Multiplier}";
        }

        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnHit_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Hämta den aktiva spelaren, genom att titta på vilken panel som
            // just nu visas i PlayerSection och hämta den panelens Player.
            Player currentPlayer = PlayerSection.Children();
            GameManager.Hit(currentPlayer.PlayerId, );
        }

        private void BtnStand_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowWinner(List<Player> winners)
        {
            string winnersStr = "";
            winners.ForEach(w => winnersStr += $"{w.Name}, ");
            LabelWinnerIs.Content = $"Winner is {winnersStr}";
        }

        private void ShowGameNotStarted()
        {
            MessageBox.Show("Game has not started");
        }
    }
}
