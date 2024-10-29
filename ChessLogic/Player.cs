namespace ChessLogic
{
    public enum Player
    {
        None,
        White,
        Black
    }
    public static class PlayerExtensions
    {  
        public static Player Opponent(this Player player)
        {
 /* 
This below code in the comment works fine but if you're using a recent version of .net (6.0 or above), you might see 
'Convert switch statement to expression' option and could choose it by clickling 'ctrl + .' or 'alt + enter':

            switch (player)
            {
                case Player.White:
                    return Player.Black;
                case Player.Black:
                    return Player.White;
                default:
                    return Player.None;
            }

*/
            return player switch
            {
                Player.White => Player.Black,
                Player.Black => Player.White,
                _ => Player.None,
            };
        }
    }
}

