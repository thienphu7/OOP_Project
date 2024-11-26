using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    //The Board class stores all the active pieces of the game and provide various helper methods:
    public class Board
    {
        //Adding a rectangular array for storing the pieces:
        private readonly Piece[,] pieces = new Piece[8, 8]; //It must have 8 rows & 8 columns. That's because a chess board is 8x8.

        private readonly Dictionary<Player, Position> pawnSkipPositions = new Dictionary<Player, Position>
        {
            { Player.White, null },
            { Player.Black, null }
        };

        public Piece this[int row, int col]
        {
            get { return pieces[row, col]; }
            set { pieces[row, col] = value; }
        }

        public Piece this[Position pos]
        {
            get { return this[pos.Row, pos.Column]; }
            set { this[pos.Row, pos.Column] = value; }
        }

        public Position GetPawnSkipPosition(Player player)
        {
            return pawnSkipPositions[player];
        }

        public void SetPawnSkipPosition(Player player, Position pos)
        {
            pawnSkipPositions[player] = pos;
        }

        //Adding a static method called initial:
        public static Board Initial() //This method should return a board with all the pieces set up correctly to start the game.
        {
            Board board = new Board();
            board.AddStartPieces();
            return board;
        }

        private void AddStartPieces()
        {
            //Adding the black pieces in the top row in default:
            this[0, 0] = new Rook(Player.Black);
            this[0, 1] = new Knight(Player.Black);
            this[0, 2] = new Bishop(Player.Black);
            this[0, 3] = new Queen(Player.Black);
            this[0, 4] = new King(Player.Black);
            this[0, 5] = new Bishop(Player.Black);
            this[0, 6] = new Knight(Player.Black);
            this[0, 7] = new Rook(Player.Black);

            //Adding the white pieces in the below row in default:
            this[7, 0] = new Rook(Player.White);
            this[7, 1] = new Knight(Player.White);
            this[7, 2] = new Bishop(Player.White);
            this[7, 3] = new Queen(Player.White);
            this[7, 4] = new King(Player.White);
            this[7, 5] = new Bishop(Player.White);
            this[7, 6] = new Knight(Player.White);
            this[7, 7] = new Rook(Player.White);

            //Adding the pawns for both players using a loop:
            for (int c = 0; c < 8; c++)
            {
                this[1, c] = new Pawn(Player.Black);
                this[6, c] = new Pawn(Player.White);
            }
        }

        //To take a position and returns true if and only if that position is inside the board:
        public static bool IsInside(Position pos)
        {
            return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
        }

        public bool IsEmpty(Position pos)
        {
            return this[pos] == null;
        }

        public IEnumerable<Position> PiecePositions()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);

                    if (!IsEmpty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        public IEnumerable<Position> PiecePositionsFor(Player player)
        {
            return PiecePositions().Where(pos => this[pos].Color == player);
        }

        public bool IsInCheck(Player player)
        {
            return PiecePositionsFor(player.Opponent()).Any(pos =>
            {
                Piece piece = this[pos];
                return piece.CanCaptureOpponentKing(pos, this);
            });
        }

        public Board Copy()
        {
            Board copy = new Board();

            foreach (Position pos in PiecePositions())
            {
                copy[pos] = this[pos].Copy();
            }

            return copy;
        }

        public Counting CountPieces()
        {
            Counting counting = new Counting();

            foreach (Position pos in PiecePositions())
            {
                Piece piece = this[pos];
                counting.Increment(piece.Color, piece.Type);
            }

            return counting;
        }

        public bool InsufficientMaterial()
        {
            Counting counting = CountPieces();

            return IsKingVKing(counting) || IsKingBishopVKing(counting) ||
                   IsKingKnightVKing(counting) || IsKingBishopVKingBishop(counting);
        }

        private static bool IsKingVKing(Counting counting)
        {
            return counting.TotalCount == 2;
        }

        private static bool IsKingBishopVKing(Counting counting)
        {
            return counting.TotalCount == 3 && (counting.White(PieceType.Bishop) == 1 || counting.Black(PieceType.Bishop) == 1);
        }

        private static bool IsKingKnightVKing(Counting counting)
        {
            return counting.TotalCount == 3 && (counting.White(PieceType.Knight) == 1 || counting.Black(PieceType.Knight) == 1);
        }

        private bool IsKingBishopVKingBishop(Counting counting)
        {
            if (counting.TotalCount != 4)
            {
                return false;
            }

            if (counting.White(PieceType.Bishop) != 1 || counting.Black(PieceType.Bishop) != 1)
            {
                return false;
            }

            Position wBishopPos = FindPiece(Player.White, PieceType.Bishop);
            Position bBishopPos = FindPiece(Player.Black, PieceType.Bishop);

            return wBishopPos.SquareColor() == bBishopPos.SquareColor();
        }

        private Position FindPiece(Player color, PieceType type)
        {
            return PiecePositionsFor(color).First(pos => this[pos].Type == type);
        }

        private bool IsUnmovedKingAndRook(Position kingPos, Position rookPos)
        {
            if (IsEmpty(kingPos) || IsEmpty(rookPos))
            {
                return false;
            }

            Piece king = this[kingPos];
            Piece rook = this[rookPos];

            return king.Type == PieceType.King && rook.Type == PieceType.Rook &&
                   !king.HasMoved && !rook.HasMoved;
        }

        public bool CastleRightKS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 7)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 7)),
                _ => false
            };
        }

        public bool CastleRightQS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 0)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 0)),
                _ => false
            };
        }

        private bool HasPawnInPosition(Player player, Position[] pawnPositions, Position skipPos)
        {
            foreach (Position pos in pawnPositions.Where(IsInside))
            {
                Piece piece = this[pos];
                if (piece == null || piece.Color != player || piece.Type != PieceType.Pawn)
                {
                    continue;
                }

                EnPassant move = new EnPassant(pos, skipPos);
                if (move.IsLegal(this))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanCaptureEnPassant(Player player)
        {
            Position skipPos = GetPawnSkipPosition(player.Opponent());

            if (skipPos == null)
            {
                return false;
            }

            Position[] pawnPositions = player switch
            {
                Player.White => new Position[] { skipPos + Direction.SouthWest, skipPos + Direction.SouthEast },
                Player.Black => new Position[] { skipPos + Direction.NorthWest, skipPos + Direction.NorthEast },
                _ => Array.Empty<Position>()
            };

            return HasPawnInPosition(player, pawnPositions, skipPos);
        }

        private bool interactionEnabled = true;

        public void EnableInteraction()
        {
            interactionEnabled = true;
        }

        public void DisableInteraction()
        {
            interactionEnabled = false;
        }

        public bool IsInteractionEnabled()
        {
            return interactionEnabled;
        }

        // Check if multiple pieces of the same type can move to the same destination
        public bool IsAmbiguousMove(Piece movingPiece, Position from, Position to)
        {
            foreach (var pos in PiecePositionsFor(movingPiece.Color))
            {
                Piece piece = this[pos];
                if (piece.Type == movingPiece.Type && pos != from)
                {
                    foreach (var move in piece.GetMoves(pos, this))
                    {
                        if (move.ToPos == to)
                        {
                            return true; // Another piece of the same type can move to the same destination
                        }
                    }
                }
            }
            return false;
        }

        public IEnumerable<Position> GetAmbiguousPositions(Piece movingPiece, Position from, Position to)
        {
            List<Position> ambiguousPositions = new List<Position>();

            foreach (var pos in PiecePositionsFor(movingPiece.Color))
            {
                Piece piece = this[pos];
                if (piece.Type == movingPiece.Type && pos != from)
                {
                    foreach (var move in piece.GetMoves(pos, this))
                    {
                        if (move.ToPos == to)
                        {
                            ambiguousPositions.Add(pos);
                            break; // Avoid duplicate checks for the same position
                        }
                    }
                }
            }

            return ambiguousPositions;
        }

        // Check if two pieces of the same type are in the same file
        public bool IsSameFile(Piece movingPiece, Position from, Position to)
        {
            foreach (var pos in PiecePositionsFor(movingPiece.Color))
            {
                Piece piece = this[pos];

                if (piece.Type == movingPiece.Type && pos != from)
                {
                    foreach (var move in piece.GetMoves(pos, this))
                    {
                        if (move.ToPos == to && pos.Column == from.Column)
                        {
                            return true; // Two pieces of the same type are in the same file
                        }
                    }
                }
            }
            return false;
        }
    }
}
