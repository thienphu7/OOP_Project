using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChessUI
{
    public partial class HistoryPanel : UserControl
    {
        // ObservableCollection for binding moves to the ListBox
        public ObservableCollection<MoveRecord> Moves { get; set; }

        public HistoryPanel()
        {
            InitializeComponent();

            // Initialize the move history collection
            Moves = new ObservableCollection<MoveRecord>();
            MoveList.ItemsSource = Moves;
        }

        // Method to add a move
        public void AddMove(string whiteMove, string blackMove, int moveNumber)
        {
            // Alternate row color based on move number
            var backgroundColor = moveNumber % 2 == 0 ? Brushes.LightGray : Brushes.White;

            // Add the move to the collection
            Moves.Add(new MoveRecord
            {
                MoveNumber = $"{moveNumber}.",
                WhiteMove = whiteMove,
                BlackMove = blackMove,
                BackgroundColor = backgroundColor
            });
        }

        // Example method to format moves (implement full logic for your requirements)
        public string FormatMove(string piece, string destination, bool isCapture = false, bool isCheck = false, bool isCheckmate = false)
        {
            string formattedMove = "";

            // Add piece notation unless it's a pawn (no notation for pawns)
            if (!string.IsNullOrEmpty(piece) && piece != "P")
                formattedMove += piece;

            // Add capture notation if needed
            if (isCapture)
                formattedMove += "x";

            // Add destination square
            formattedMove += destination;

            // Add check or checkmate notation if needed
            if (isCheckmate)
                formattedMove += "#";
            else if (isCheck)
                formattedMove += "+";

            return formattedMove;
        }

        // Example method to call when moves are made (to be hooked to game logic)
        public void OnMoveMade(string whiteMove, string blackMove)
        {
            int moveNumber = (Moves.Count / 2) + 1;
            AddMove(whiteMove, blackMove, moveNumber);
        }
    }

    // Class representing a move record
    public class MoveRecord
    {
        public string MoveNumber { get; set; }
        public string WhiteMove { get; set; }
        public string BlackMove { get; set; }
        public Brush BackgroundColor { get; set; }
    }
}
