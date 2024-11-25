namespace ChessLogic
{
    //This class represents a position or square on the chess board:
    public class Position
    {
        //The class needs two properties, one for the row, and another one for the column:
        public int Row { get; }
        public int Column { get; }

        //Adding a Constructor which takes a row and a column:
        public Position(int row, int column)
        {
            // Storing row and column in the properties:
            Row = row;
            Column = column;
        }

        //Adding a square color method:
        public Player SquareColor()
        {
            if ((Row + Column) % 2 == 0)
            {
                return Player.White;
            }
            //Otherwise, if the sum is odd, then the squre is black:
            return Player.Black;
        }

/*
Equals and GetHashCode need to be overwritten so that the Position class can be used as the key in a dictionary. 
Instead of doing so manually, Visual Studio can be used to generate these methods by creating an empty line, pressing 'Ctrl .', and clicking 'Generate Equals and GetHashCode...'. 
Ensure that both row and column are selected, then check 'Generate operators'.
*/
        public override bool Equals(object obj)
        {
            return obj is Position position &&
                   Row == position.Row &&
                   Column == position.Column;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(Position left, Position right)
        {
            return EqualityComparer<Position>.Default.Equals(left, right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }

        public static Position operator +(Position pos, Direction dir)
        {
            return new Position(pos.Row + dir.RowDelta, pos.Column + dir.ColumnDelta);  
        }

        public override string ToString()
        {
            char file = (char)('a' + Column); // Convert column index to file (a-h)
            int rank = 8 - Row;               // Convert row index to rank (1-8)
            return $"{file}{rank}";
        }
    }
}
