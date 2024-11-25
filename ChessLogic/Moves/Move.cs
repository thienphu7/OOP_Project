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
        public Piece PromotedPiece { get; protected set; }

        // Abstract method to execute a move
        public abstract bool Execute(Board board);

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
            // Retrieve the piece at the starting position
            Piece piece = board[ToPos];

            // Handle castling
            if (Type == MoveType.CastleKS) return "0-0";
            if (Type == MoveType.CastleQS) return "0-0-0";

            // Determine piece abbreviation (no abbreviation for pawns)
            string pieceNotation = "";

            pieceNotation = piece.Type switch
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

            if (FromPos != null && board[FromPos] != null && board[ToPos] != null)
            {
                Piece fromPiece = board[FromPos];
                Piece toPiece = board[ToPos];

                // Check if the piece at ToPos belongs to the opponent
                if (fromPiece != null && toPiece.Color != fromPiece.Color)
                {
                    captureNotation = "x"; // Add "x" to indicate capture
                }
            }

            // Get destination square
            string destination = ToPos.ToString();

            // Handle pawn promotion
            string promotionNotation = (Type == MoveType.PawnPromotion && PromotedPiece != null)
                ? $"={PromotedPiece.Type.ToString()[0].ToString().ToUpper()}"
                : "";

            // Combine all components
            return pieceNotation + captureNotation + destination + promotionNotation;
        }
    }
}
