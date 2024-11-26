namespace ChessLogic
{
    public class PawnPromotion : Move
    {
        public override MoveType Type => MoveType.PawnPromotion;
        public override Position FromPos { get; }
        public override Position ToPos {  get; }


        private readonly PieceType newType;

        public PawnPromotion(Position fromPos, Position toPos, PieceType newType)
        {
            FromPos = fromPos;
            ToPos = toPos;
            this.newType = newType;
        }

        private Piece CreatePromotionPiece(Player color)
        {
            return newType switch
            {
                PieceType.Knight => new Knight(color),
                PieceType.Bishop => new Bishop(color),
                PieceType.Rook => new Rook(color),
                _ => new Queen(color)
            };
        }

        public override bool Execute(Board board)
        {
            Piece pawn = board[FromPos];
            board[FromPos] = null;

            Piece promotionPiece = CreatePromotionPiece(pawn.Color);
            promotionPiece.HasMoved = true;
            board[ToPos] = promotionPiece;

            PromotedPiece = promotionPiece;
            return true;
        }

        public override string ToNotation(Board board)
        {
            // Determine if the move is a capture
            string captureNotation = board[ToPos] != null ? "x" : "";

            // Get the destination square
            string destination = ToPos.ToString();

            // Add promotion notation (e.g., =Q, =R, etc.)
            string promotionNotation = $"={newType.ToString()[0].ToString().ToUpper()}";

            // Combine the move components
            return captureNotation + destination + promotionNotation;
        }
    }
}
