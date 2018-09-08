using System;
using System.Diagnostics;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessDefinitions;
using VICEcs.Tools;

namespace VICEcs.ChessObjects {
    public static class Attack {
        private static readonly int[] BI_DIR = {-9, -11, 11, 9};
        private static readonly int[] KI_DIR = {-1, -10, 1, 10, -9, -11, 11, 9};
        private static readonly int[] KN_DIR = {-8, -19, -21, -12, 8, 19, 21, 12};
        private static readonly int[] RK_DIR = {-1, -10, 1, 10};

        /// <summary>
        /// Validates whether the parameter square is attacked by input side.
        /// </summary>
        /// <param name="sq"> The square to validate </param>
        /// <param name="side"> The side to check for attacks </param>
        /// <param name="board"> The current board </param>
        /// <returns> True or false depending on whether the sq is attacked </returns>
        /// <exception cref="Exception"></exception>
        public static bool IsSqAttacked(int sq, int side, Board.Board board) {
            Debug.Assert(Validators.SqOnBoard(sq), String.Format("Square {0} on board is invalid", sq));
            Debug.Assert(Validators.SideValid(side), String.Format("Side {0} on board is invalid", side));
            Debug.Assert(BoardOperations.CheckBoard(board), String.Format("Checkboard is false"));
            
            // first checking pawns
            if (side == (int) Colour.WHITE) {
                if (board[sq - 9] == (int) Piece.wP || board[sq - 11] == (int) Piece.wP) {
                    return true;
                }
            } else {
                // side black
                if (board[sq + 9] == (int) Piece.bP || board[sq + 11] == (int) Piece.bP) {
                    return true;
                }
            }
            
            // knights
            for (var i = 0; i < Attack.KN_DIR.Length; ++i) {
                var piece = board[sq + Attack.KN_DIR[i]];
                if (piece == (int) Piece.EMPTY || piece == (int) Square.NO_SQ) {
                    continue;
                }

                if (Data.IsPieceKnight(piece) && (int) Data.PIECE_COLOURS[piece] == side) {
                    return true;
                }
            }

            // Rooks queens
            for (var i = 0; i < Attack.RK_DIR.Length; ++i) {
                var square = sq + Attack.RK_DIR[i];
                var piece = board[square];
                while (Validators.IsSq(square)) {
                    if (piece != (int) Piece.EMPTY) {
                        if (Data.IsPieceRookQueen(piece) &&
                            (int) Data.PIECE_COLOURS[piece] == side) {
                            return true;
                        }

                        break;
                    }

                    square += Attack.RK_DIR[i];
                    piece = board[square];
                }
            }

            // Bishop queens
            for (var i = 0; i < Attack.BI_DIR.Length; ++i) {
                var square = sq + Attack.BI_DIR[i];
                var piece = board[square];
                while (Validators.IsSq(square)) {
                    if (piece != (int) Piece.EMPTY) {
                        if (Data.IsPieceBishopQueen(piece) &&
                            (int) Data.PIECE_COLOURS[piece] == side) {
                            return true;
                        }

                        break;
                    }

                    square += Attack.BI_DIR[i];
                    piece = board[square];
                }
            }

            // Kings
            for (var i = 0; i < Attack.KI_DIR.Length; ++i) {
                var piece = board[sq + Attack.KI_DIR[i]];
                if (piece == (int) Piece.EMPTY || piece == (int) Square.NO_SQ) {
                    continue;
                }

                if (Data.IsPieceKing(piece) && (int) Data.PIECE_COLOURS[piece] == side) {
                    return true;
                }
            }

            return false;
        }
    }
}