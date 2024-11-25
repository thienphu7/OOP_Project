using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;

        private int noCaptureOrPawnMoves = 0;
        private string stateString;

        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();

        public List<Move> MoveHistory { get; private set; }

        public int MoveNumber => Board.PiecePositions().Count() / 2 + 1;

        public event EventHandler<Move> MoveCompleted;

        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
            MoveHistory = new List<Move>();

            stateString = new StateString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }

        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Piece piece = Board[pos];
            IEnumerable<Move> moveCandidates = piece.GetMoves(pos, Board);
            return moveCandidates.Where(move => move.IsLegal(Board));
        }

        public void MakeMove(Move move, Clock clock, Player currentPlayer)
        {
            // Add the move to MoveHistory
            MoveHistory.Add(move);

            // Update board state
            Board.SetPawnSkipPosition(CurrentPlayer, null);
            bool captureOrPawn = move.Execute(Board);

            if (captureOrPawn)
            {
                noCaptureOrPawnMoves = 0;
                stateHistory.Clear();
            }
            else
            {
                noCaptureOrPawnMoves++;
            }

            // Check if the current player is out of time
            if (clock.IsCurrentPlayerOutOfTime())
            {
                Result = Result.Win(CurrentPlayer.Opponent(),
                    CurrentPlayer == Player.White ? EndReason.TimeoutBlackWins : EndReason.TimeoutWhiteWins);
            }

            // Generate the move notation using ToNotation
            string moveNotation = move.ToNotation(Board);

            // Switch player
            CurrentPlayer = CurrentPlayer.Opponent();
            UpdateStateString();
            CheckForGameOver();

            // Trigger the MoveCompleted event after the move is executed
            MoveCompleted?.Invoke(this, move);
        }

        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Piece piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });

            return moveCandidates.Where(move => move.IsLegal(Board));
        }

        private void CheckForGameOver()
        {
            if (!AllLegalMovesFor(CurrentPlayer).Any())
            {
                if (Board.IsInCheck(CurrentPlayer))
                {
                    Result = Result.Win(CurrentPlayer.Opponent());
                }
                else
                {
                    Result = Result.Draw(EndReason.Stalemate);
                }
            }
            else if (Board.InsufficientMaterial())
            {
                Result = Result.Draw(EndReason.InsuffiicientMaterial);
            }
            else if (FiftyMoveRule())
            {
                Result = Result.Draw(EndReason.FiftyMoveRule);
            }
            else if (ThreefoldRepetition())
            {
                Result = Result.Draw(EndReason.ThreefoldRepetition);
            }
        }

        public bool IsGameOver()
        {
            return Result != null;
        }

        private bool FiftyMoveRule()
        {
            int fullMoves = noCaptureOrPawnMoves / 2;
            return fullMoves == 50;
        }

        private void UpdateStateString()
        {
            stateString = new StateString(CurrentPlayer, Board).ToString();

            if (!stateHistory.ContainsKey(stateString))
            {
                stateHistory[stateString] = 1;
            }
            else
            {
                stateHistory[stateString]++;
            }
        }

        private bool ThreefoldRepetition()
        {
            return stateHistory.TryGetValue(stateString, out int count) && count >= 3;
        }

        public void HandleTimeOut(Player player)
        {
            Result = Result.Win(player.Opponent(),
                player == Player.White ? EndReason.TimeoutBlackWins : EndReason.TimeoutWhiteWins);
        }

        public void HandleSurrender(Player surrenderingPlayer)
        {
            if (surrenderingPlayer == Player.White)
            {
                Result = Result.Win(Player.Black, EndReason.WhiteSurrendered);
            }
            else
            {
                Result = Result.Win(Player.White, EndReason.BlackSurrendered);
            }
        }

    }
}
