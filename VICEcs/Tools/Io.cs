using System;
using System.Data.Odbc;
using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Move;

namespace VICEcs.Tools {
    /// <summary>
    ///     Small class to help print various objects.
    /// </summary>
    public static class Io {
        // A toString type function for the parameter square.
        public static string SqToString(int sq) {
            var sqToPrint = string.Format("{0}{1}",
                Convert.ToChar('a' + Conversion.getFilesBrd(sq)),
                Convert.ToChar('1' + Conversion.getRanksBrd(sq)));
            return sqToPrint;
        }

        // A toString type function for the parameter move.
        public static string MoveToString(int move) {
            string moveToPrint;

            var fromFile = Conversion.getFilesBrd(MoveOperations.FromSq(move));
            var fromRank = Conversion.getRanksBrd(MoveOperations.FromSq(move));
            var toFile = Conversion.getFilesBrd(MoveOperations.ToSq(move));
            var toRank = Conversion.getRanksBrd(MoveOperations.ToSq(move));

            var promoted = MoveOperations.Promoted(move);

            if (promoted != 0) {
                var pieceChar = 'q';
                if (Data.IsPieceKnight(promoted)) {
                    pieceChar = 'n';
                } else if (Data.IsPieceRookQueen(promoted) && !Data.IsPieceBishopQueen(promoted)) {
                    pieceChar = 'r';
                } else if (!Data.IsPieceRookQueen(promoted) && Data.IsPieceBishopQueen(promoted)) {
                    pieceChar = 'b';
                }

                moveToPrint = string.Format("{0}{1}{2}{3}{4}", Convert.ToChar('a' + fromFile),
                    Convert.ToChar('1' + fromRank),
                    Convert.ToChar('a' + toFile), Convert.ToChar('1' + toRank),
                    Convert.ToChar(pieceChar));
            } else {
                moveToPrint = string.Format("{0}{1}{2}{3}", Convert.ToChar('a' + fromFile),
                    Convert.ToChar('1' + fromRank),
                    Convert.ToChar('a' + toFile), Convert.ToChar('1' + toRank));
            }

            return moveToPrint;
        }

        /// <summary>
        ///     Translates a move to an integer
        /// </summary>
        /// <param name="board"></param>
        /// <param name="ptrChar"></param>
        /// <returns></returns>
        public static int ParseMove(Board board, char[] ptrChar) {
            if (ptrChar[1] > '8' || ptrChar[1] < '1') {
                return Variables.NO_MOVE;
            }

            if (ptrChar[3] > '8' || ptrChar[3] < '1') {
                return Variables.NO_MOVE;
            }

            if (ptrChar[0] > 'h' || ptrChar[0] < 'a') {
                return Variables.NO_MOVE;
            }

            if (ptrChar[2] > 'h' || ptrChar[2] < 'a') {
                return Variables.NO_MOVE;
            }
            
            // Use ASCII values as integers.
            int from = Conversion.FR2SQ(ptrChar[0] - 'a', ptrChar[1] - '1');
            int to = Conversion.FR2SQ(ptrChar[2] - 'a', ptrChar[3] - '1');
            
            Debug.Assert(Validators.SqOnBoard(from), String.Format("The from {0} sq is invalid", Io.SqToString(from)));
            Debug.Assert(Validators.SqOnBoard(to),
                String.Format("The to {0} sq is invalid", Io.SqToString(to)));
            
            MoveList moveList = new MoveList();
            MoveGen.GenerateAllMoves(board, moveList, false);

            for (int i = 0; i < moveList.Count; ++i) {
                int move = moveList.Moves[i].Move;

                // If the move has been located in the move list, it means it's a valid move.
                if (MoveOperations.FromSq(move) == from && MoveOperations.ToSq(move) == to) {
                    int promotedPce = MoveOperations.Promoted(move);
                    if (promotedPce != (int) Piece.EMPTY) {
                        if (Data.IsPieceRookQueen(promotedPce) &&
                            !Data.IsPieceBishopQueen(promotedPce) && ptrChar[4] == 'r') {
                            return move;
                        } else if (!Data.IsPieceRookQueen(promotedPce) &&
                                   Data.IsPieceBishopQueen(promotedPce) && ptrChar[4] == 'b') {
                            return move;
                        } else if (Data.IsPieceKnight(promotedPce) && ptrChar[4] == 'k') {
                            return move;
                        } else if (Data.IsPieceBishopQueen(promotedPce) &&
                                   Data.IsPieceRookQueen(promotedPce) && ptrChar[4] == 'q') {
                            return move;
                        }
                        continue;
                    }

                    return move;
                }
            }
            
            return Variables.NO_MOVE;
        }

        // A toString type function for the parameter MoveList.
        public static void PrMoveList(MoveList list) {
            Console.Write("MoveList:\n");

            for (var i = 0; i < list.Count; ++i) {
                var move = list.Moves[i].Move;
                var score = list.Moves[i].Score;

                Console.Write("Move:{0} > {1} (score:{2})\n", i + 1, Io.MoveToString(move), score);
            }

            Console.Write("MoveList Total {0} Moves:\n\n", list.Count);
        }

        public static unsafe void PrPawns(ulong[] pawnKey, int colour) {
            var pointerHelper = pawnKey[colour];
            var pointer = &pointerHelper;
            var counter = 1;
            var t_colour = "";
            switch (colour) {
            case (int) Colour.WHITE:
                t_colour = "White";
                break;
            case (int) Colour.BLACK:
                t_colour = "Black";
                break;
            case (int) Colour.BOTH:
                t_colour = "Both";
                break;
            default:
                throw new FormatException(String.Format("Invalid colour {0}", (Colour) colour));
            }
        }
    }
}