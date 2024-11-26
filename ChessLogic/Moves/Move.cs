using System.Data.Common;

namespace ChessLogic
{
    public abstract class Move
    {
        // Abstract properties for move type and positions
        public abstract MoveType Type { get; }
        public abstract Position FromPos { get; }
        public abstract Position ToPos { get; }

        // Property to track the promoted piece (for pawn promotion)
        public Piece MovingPiece { get; private set; }
        public Piece TargetPiece { get; private set; } // Captures the piece at ToPos before the move
        public Piece PromotedPiece { get; protected set; }

        // Abstract method to execute a move
        public abstract bool Execute(Board board);

        // Set the MovingPiece before executing the move
        public void PrepareMove(Board board)
        {
            MovingPiece = board[FromPos]; // Piece to be moved
            TargetPiece = board[ToPos];   // Piece (if any) at the target position
        }

        // Check if the move is legal
        public virtual bool IsLegal(Board board)
        {
            // Get the color of the current player
            Player player = board[FromPos].Color;

            // Create a copy of the board and simulate the move
            Board boardCopy = board.Copy();
            Execute(boardCopy);

            // Check if the player's king is in check after the move
            return !boardCopy.IsInCheck(player);
        }

        // Generate the move notation (e.g., "e4", "Nxf3", "O-O-O", "xh8=Q")
        public virtual string ToNotation(Board board)
        {

            // Handle castling
            if (Type == MoveType.CastleKS) return "0-0";
            if (Type == MoveType.CastleQS) return "0-0-0";

            // Determine piece abbreviation (no abbreviation for pawns)
            string pieceNotation = "";

            pieceNotation = MovingPiece.Type switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                _ => "" // Default to no abbreviation for pawns
            };

            // Add "x" for captures
            string captureNotation = "";
            if (TargetPiece != null && TargetPiece.Color != MovingPiece.Color)
            {
                captureNotation = "x"; // Valid capture
            }

            // Handle disambiguation
            string disambiguation = "";

            if (MovingPiece.Type == PieceType.Pawn && TargetPiece != null)
            {
                // For pawns, always include the starting file when capturing
                disambiguation = $"{(char)('a' + FromPos.Column)}";
            }
            else if (board.IsAmbiguousMove(MovingPiece, FromPos, ToPos))
            {
                // Get all ambiguous positions
                var ambiguousPositions = board.GetAmbiguousPositions(MovingPiece, FromPos, ToPos);

                // Check if ambiguity is on the same file or rank
                bool sameFile = ambiguousPositions.Any(pos => pos.Column == FromPos.Column);
                bool sameRank = ambiguousPositions.Any(pos => pos.Row == FromPos.Row);

                if (sameFile && !sameRank)
                {
                    disambiguation = $"{(char)('a' + FromPos.Column)}"; // Include file only
                }
                else if (!sameFile && sameRank)
                {
                    disambiguation = $"{8 - FromPos.Row}"; // Include rank only
                }
                else
                {
                    disambiguation = $"{(char)('a' + FromPos.Column)}{8 - FromPos.Row}"; // Include both file and rank
                }
            }

            // Get destination square
            string destination = ToPos.ToString();

            // Handle pawn promotion
            string promotionNotation = "";
            if (MovingPiece.Type == PieceType.Pawn && (ToPos.Row == 0 || ToPos.Row == 7))
            {
                if (PromotedPiece != null)
                {
                    promotionNotation = $"={PromotedPiece.Type.ToString()[0].ToString().ToUpper()}";
                }
            }

            // Combine all components
            return pieceNotation + disambiguation + captureNotation + destination + promotionNotation;
        }
    }
}
