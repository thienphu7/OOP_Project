namespace ChessLogic
{
    public enum EndReason
    {
        Checkmate,
        Stalemate,
        FiftyMoveRule,
        InsuffiicientMaterial,
        ThreefoldRepetition,
        TimeoutBlackWins, // Black wins due to White timeout
        TimeoutWhiteWins  // White wins due to Black timeout
    }
}
