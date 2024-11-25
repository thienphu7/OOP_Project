using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ChessUI
{
    public class MoveRecord
    {
        public string MoveNumber { get; set; }
        public string WhiteMove { get; set; }
        public string BlackMove { get; set; }
    }

    public partial class HistoryPanel : UserControl
    {
        public ObservableCollection<MoveRecord> Moves { get; set; }

        public HistoryPanel()
        {
            InitializeComponent();
            Moves = new ObservableCollection<MoveRecord>();
            DataContext = this;

            MoveList.ItemsSource = Moves;
        }

        public void AddMove(string moveNotation, bool isWhiteTurn, int moveNumber)
        {
            Console.WriteLine($"Adding move: {moveNotation}, IsWhiteTurn: {isWhiteTurn}, MoveNumber: {moveNumber}");

            if (isWhiteTurn)
            {
                // Add a new row for White's move
                Moves.Add(new MoveRecord
                {
                    MoveNumber = $"{moveNumber}.", // First column
                    WhiteMove = moveNotation,     // Second column
                    BlackMove = ""                // Third column (empty for now)
                });
            }
            else
            {
                // Add Black's move to the last row
                if (Moves.Count > 0)
                {
                    var lastRow = Moves[Moves.Count - 1];
                    Moves[Moves.Count - 1] = new MoveRecord
                    {
                        MoveNumber = lastRow.MoveNumber,
                        WhiteMove = lastRow.WhiteMove,
                        BlackMove = moveNotation // Update Black's move
                    };
                }
            }
        }

        public void ResetMoves()
        {
            Moves.Clear();
        }
    }
}
