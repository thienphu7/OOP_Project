namespace ChessLogic
{
    public class Result
    {
        public Player Winner { get; }
        public EndReason Reason { get; }

        // Constructor to initialize Result with Winner and Reason
        public Result(Player winner, EndReason reason)
        {
            Winner = winner;
            Reason = reason;
        }

        // Factory method for a winning Result with default Checkmate end reason
        public static Result Win(Player winner, EndReason endReason = EndReason.Checkmate)
        {
            return new Result(winner, endReason);
        }

        // Factory method for a draw Result with a specified reason
        public static Result Draw(EndReason reason)
        {
            return new Result(Player.None, reason);
        }
    }
}
