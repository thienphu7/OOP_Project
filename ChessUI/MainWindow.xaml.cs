using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChessLogic;
using static Clock;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Image and Rectangle arrays to represent chess pieces and highlights on the board
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];

        // Dictionary to cache legal moves for a selected position
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        // Current game state and clock objects
        private GameState gameState;
        private Clock clock;

        // Position of the currently selected piece (if any)
        private Position selectedPos = null;

        // Variable to hold the selected time mode for the game
        private TimeMode selectedMode;

        // Variable to manage game state
        private bool isGameInProgress = false;

        public MainWindow()
        {            
            InitializeComponent();           
            InitializeBoard();

            // Initialize game state with starting position and white player as current
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            // Show the time control menu to select a time mode
            ShowTimeControlMenu();

            clock = new Clock(selectedMode);

            clock.OnPlayer1TimeOut += (sender, e) =>
            {
                Dispatcher.Invoke(() => gameState.HandleTimeOut(Player.White));
            };

            clock.OnPlayer2TimeOut += (sender, e) =>
            {
                Dispatcher.Invoke(() => gameState.HandleTimeOut(Player.Black));
            };

        }

        private void ShowTimeControlMenu()
        {
            // Create a time control menu instance
            TimeControlMenu timeControlMenu = new TimeControlMenu();

            // Set the content of the menu container to the time control menu
            MenuContainer.Content = timeControlMenu;

            // Subscribe to the TimeModeSelected event of the menu
            timeControlMenu.TimeModeSelected += mode =>
            {
                // Update the selected time mode
                selectedMode = mode;

                // Create a new clock based on the selected mode
                clock = new Clock(selectedMode);

                // Subscribe to clock events for time updates
                clock.OnPlayer1TimeUpdate += Clock_OnPlayer1TimeUpdate;
                clock.OnPlayer2TimeUpdate += Clock_OnPlayer2TimeUpdate;

                // Start the clock after setting the mode
                clock.StartClock();

                // Clear the menu container
                MenuContainer.Content = null;

                // Set game in progress and reset the game state
                isGameInProgress = true;
                gameState = new GameState(Player.White, Board.Initial());
                DrawBoard(gameState.Board);
                SetCursor(gameState.CurrentPlayer);
            };
        }
        private void Clock_OnPlayer1TimeUpdate(object sender, PlayerTimeEventArgs e)
        {
            // Update the time display for Player 1 (White) on the UI thread
            Dispatcher.Invoke(() => Player1TimeLabel.Content = $"White: {e.Time.ToString("mm\\:ss")}");
        }

        private void Clock_OnPlayer2TimeUpdate(object sender, PlayerTimeEventArgs e)
        {
            // Update the time display for Player 2 (Black) on the UI thread
            Dispatcher.Invoke(() => Player2TimeLabel.Content = $"Black: {e.Time.ToString("mm\\:ss")}");
        }

        private void InitializeBoard()
        {
            // Loop through all squares on the board
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    // Create an image element for the chess piece
                    Image image = new Image();
                    pieceImages[r, c] = image;

                    // Add the image to the PieceGrid container
                    PieceGrid.Children.Add(image);

                    // Create a rectangle element for highlighting squares
                    Rectangle highlight = new Rectangle();
                    highlights[r, c] = highlight;

                    // Add the rectangle to the HighlightGrid container
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen())
            {
                return;
            }

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);

            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();

            if (moveCache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;

            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };
        }

        private void HandleMove(Move move)
        {
            gameState.MakeMove(move, clock, gameState.CurrentPlayer); // Pass 'clock' and 'currentPlayer'
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            clock.SwitchTurn();

            if (gameState.IsGameOver())
            {
                ShowGameOver();
            }
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();

            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 125, 255, 125);

            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighlights()
        {
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            if (player == Player.White)
            {
                Cursor = ChessCursors.WhiteCursor;
            }
            else
            {
                Cursor = ChessCursors.BlackCursor;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(gameState);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void RestartGame()
        {
            // Reset selected variables and board highlights
            selectedPos = null;
            HideHighlights();
            moveCache.Clear();

            // Ensure any running clocks are stopped and reset
            clock.ResetClock();

            // Show the time control menu to select a new time mode
            ShowTimeControlMenu();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMenuOnScreen() && e.Key == Key.Escape)
            {
                ShowPauseMenu();
            }
        }

        private void ShowPauseMenu()
        {
            PauseMenu pauseMenu = new PauseMenu();
            MenuContainer.Content = pauseMenu;

            pauseMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;

                if (option == Option.Restart)
                {
                    RestartGame();
                }
            };
        }

        public void SetTimeMode(TimeMode selectedMode)
        {
            if (clock == null)  // Ensures clock is initialized
            {
                clock = new Clock(selectedMode);
            }
            clock.StartClock();
            EnablePieceMovement();
        }

        // Ensure this logic allows piece movement
        private void EnablePieceMovement()
        {
            isGameInProgress = true; // Activate playable state
            gameState.Board.EnableInteraction(); 
        }

        private void SurrenderBlack_Click(object sender, MouseButtonEventArgs e)
        {
            if (gameState.CurrentPlayer == Player.Black)
            {
                gameState.HandleSurrender(Player.Black);
                ShowGameOver();
            }
        }

        private void SurrenderWhite_Click(object sender, MouseButtonEventArgs e)
        {
            if (gameState.CurrentPlayer == Player.White)
            {
                gameState.HandleSurrender(Player.White);
                ShowGameOver();
            }
        }
    }
}
