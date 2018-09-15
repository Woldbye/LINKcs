 using System;
using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Search {
    /// <summary>
    ///     Evaluate calculates the integer value of a given position on the board.
    /// </summary>
    public static class Evaluate {
        // An isolated pawn has less value than normally
        private const int PAWN_ISOLATED_VAL = -10;
        private const int ROOK_OPEN_FILE_VAL = 10;
        private const int ROOK_SEMI_OPEN_FILE_VAL = 5;
        private const int QUEEN_OPEN_FILE_VAL = 5;
        private const int QUEEN_SEMI_OPEN_FILE_VAL = 3;
        private const int BISHOP_PAIR_VAL = 30;
        private const int PAWNS_PROTECTING_KING_VAL = 20;

        // When there is less material than the end_game_limit on the board, the game has reached it's end game phase.
        private static int END_GAME_LIMIT =
            Data.PIECE_VAL[(int) Piece.wR] + 2 * Data.PIECE_VAL[(int) Piece.wN] + 2 * Data.PIECE_VAL[(int) Piece.wP];
        
        // A white pawn on rank 7 has more value than if it's on rank 2 etc.
        private static int[] PAWN_PASSED_VAL = {
            0, 5, 10, 20, 35, 60, 100, 200
        };

        /// <summary>
        ///     Each of the 64 squared piece tables indicate the value of a given piece,
        ///     with regard to the indexed square. For example a pawn on e4 has higher value than on
        ///     e2.
        /// </summary>
        private static readonly int[] pawnTable = {
            0, 0, 0, 0, 0, 0, 0, 0,
            10, 10, 0, -10, -10, 0, 10, 10,
            5, 0, 0, 5, 5, 0, 0, 5,
            0, 0, 10, 20, 20, 10, 0, 0,
            5, 5, 5, 10, 10, 5, 5, 5,
            10, 10, 10, 20, 20, 10, 10, 10,
            20, 20, 20, 30, 30, 20, 20, 20,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private static readonly int[] knightTable = {
            0, -10, 0, 0, 0, 0, -10, 0,
            0, 0, 0, 5, 5, 0, 0, 0,
            0, 0, 10, 10, 10, 10, 0, 0,
            0, 0, 10, 20, 20, 10, 0, 0,
            5, 10, 15, 20, 20, 15, 10, 5,
            5, 10, 10, 20, 20, 10, 10, 5,
            0, 0, 5, 10, 10, 5, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private static readonly int[] bishopTable = {
            0, 0, -10, 0, 0, -10, 0, 0,
            0, 0, 0, 10, 10, 0, 0, 0,
            0, 0, 10, 15, 15, 10, 0, 0,
            0, 10, 15, 20, 20, 15, 10, 0,
            0, 10, 15, 20, 20, 15, 10, 0,
            0, 0, 10, 15, 15, 10, 0, 0,
            0, 0, 0, 10, 10, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private static readonly int[] rookTable = {
            0, 0, 5, 10, 10, 5, 0, 0,
            0, 0, 5, 10, 10, 5, 0, 0,
            0, 0, 5, 10, 10, 5, 0, 0,
            0, 0, 5, 10, 10, 5, 0, 0,
            0, 0, 5, 10, 10, 5, 0, 0,
            0, 0, 5, 10, 10, 5, 0, 0,
            25, 25, 25, 25, 25, 25, 25, 25,
            0, 0, 5, 10, 10, 5, 0, 0
        };

        /// <summary>
        /// Encourage king to move towards center.
        /// </summary>
        private static readonly int[] kingEndTable = {
            -50	,	-10	,	0	,	0	,	0	,	0	,	-10	,	-50	,
            -10,	0	,	10	,	10	,	10	,	10	,	0	,	-10	,
            0	,	10	,	15	,	15	,	15	,	15	,	10	,	0	,
            0	,	10	,	15	,	20	,	20	,	15	,	10	,	0	,
            0	,	10	,	15	,	20	,	20	,	15	,	10	,	0	,
            0	,	10	,	15	,	15	,	15	,	15	,	10	,	0	,
            -10,	0	,	10	,	10	,	10	,	10	,	0	,	-10	,
            -50	,	-10	,	0	,	0	,	0	,	0	,	-10	,	-50	
        };

        /// <summary>
        /// Encourage castling
        /// </summary>
        private static readonly int[] kingOpeningTable = {
            0, 5, 5, -10, -10, 0, 10, 5,
            -10, -10, -10, -10, -10, -10, -10, -10,
            -30, -30, -30, -30, -30, -30, -30, -30,
            -70, -70, -70, -70, -70, -70, -70, -70,
            -70, -70, -70, -70, -70, -70, -70, -70,
            -70, -70, -70, -70, -70, -70, -70, -70,
            -70, -70, -70, -70, -70, -70, -70, -70,
            -70, -70, -70, -70, -70, -70, -70, -70
        };

        /// <summary>
        ///     Evaluates the input board position.
        /// </summary>
        /// <param name="board"> The board to evaluate </param>
        /// <returns> The integer score for the current position </returns>
        /// <remarks> White scores positive, and black negative. </remarks>
        public static int Position(Board.Board board) {
            if (Evaluate.MaterialDraw(board)) {
                return 0;
            }

            var score = board.Material[(int) Colour.WHITE] - board.Material[(int) Colour.BLACK];

            Evaluate.AddPawnScores(board, (int) Colour.WHITE, ref score);
            Evaluate.AddPawnScores(board, (int) Colour.BLACK, ref score);
            Evaluate.AddKnBishopKingScores(board, (int) Piece.wN, Evaluate.knightTable, ref score);
            Evaluate.AddKnBishopKingScores(board, (int) Piece.bN, Evaluate.knightTable, ref score);
            Evaluate.AddKnBishopKingScores(board, (int) Piece.wB, Evaluate.bishopTable, ref score);
            Evaluate.AddKnBishopKingScores(board, (int) Piece.bB, Evaluate.bishopTable, ref score);
            Evaluate.AddRookOrQueenScores(board, (int) Piece.wR, ref score);
            Evaluate.AddRookOrQueenScores(board, (int) Piece.bR, ref score);
            
            if (board.Material[(int) Colour.BLACK] <= Evaluate.END_GAME_LIMIT) {
                Evaluate.AddKnBishopKingScores(board, (int) Piece.wK, Evaluate.kingEndTable, ref score);
            } else {
                Evaluate.AddKnBishopKingScores(board, (int) Piece.wK, Evaluate.kingOpeningTable,
                    ref score);
            }
            
            // If black king on a castling square
            if ((board.KingSq[(int) Colour.BLACK] == (int) Square.G8) || 
                (board.KingSq[(int) Colour.BLACK] == (int) Square.C8)) {
                if (KingSafety.CountBitsSetInArea(board.Pawns[(int) Colour.BLACK], board.KingSq[(int) Colour.BLACK]) >= 3) {
                    score -= Evaluate.PAWNS_PROTECTING_KING_VAL;
                }
            }
            

            if (board.Material[(int) Colour.WHITE] <= Evaluate.END_GAME_LIMIT) {
                Evaluate.AddKnBishopKingScores(board, (int) Piece.bK, Evaluate.kingEndTable, ref score);
            } else {
                Evaluate.AddKnBishopKingScores(board, (int) Piece.bK, Evaluate.kingOpeningTable, ref score);
            }
            
            // If white king on a castling square
            if ((board.KingSq[(int) Colour.WHITE] == (int) Square.G1) || 
                (board.KingSq[(int) Colour.WHITE] == (int) Square.C1)) {
                if (KingSafety.CountBitsSetInArea(board.Pawns[(int) Colour.WHITE], board.KingSq[(int) Colour.WHITE]) >= 3) {
                    score += Evaluate.PAWNS_PROTECTING_KING_VAL;
                }
            }
                        
            // Bishop pair bonus
            if (board.PceNum[(int) Piece.wB] == 2) {
                score += Evaluate.BISHOP_PAIR_VAL;
            }

            if (board.PceNum[(int) Piece.bB] == 2) {
                score -= Evaluate.BISHOP_PAIR_VAL;
            }
            
            if (board.Side == (int) Colour.WHITE) {
                return score;
            } else {
                return -score; // BLACK to move
            }
        }

        /// <summary>
        ///     Evaluates whether the input posisition is a draw material-wise.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public static bool MaterialDraw(Board.Board board) {
            if (board.Pawns[(int) Colour.BOTH] != 0) {
                return false;
            }
            
            // If no rooks or queens
            if (board.PceNum[(int) Piece.wR] == 0 && board.PceNum[(int) Piece.bR] == 0 &&
                board.PceNum[(int) Piece.wQ] == 0 && board.PceNum[(int) Piece.bQ] == 0) {
                // No bishops
                if (board.PceNum[(int) Piece.wB] == 0 && board.PceNum[(int) Piece.bB] == 0) {
                    // Less than three knights
                    if (board.PceNum[(int) Piece.wK] < 3 && board.PceNum[(int) Piece.bK] < 3) {
                        return true;
                    }

                    // No knights
                } else if (board.PceNum[(int) Piece.wK] == 0 &&
                           board.PceNum[(int) Piece.bK] == 0) {
                    // Neither side has two bishop advantage
                    if (Math.Abs(board.PceNum[(int) Piece.wB] - board.PceNum[(int) Piece.bB]) < 2) {
                        return true;
                    }
                } else if (board.PceNum[(int) Piece.wN] < 3 &&
                           board.PceNum[(int) Piece.wB] == 0 ||
                           board.PceNum[(int) Piece.wB] == 1 &&
                           board.PceNum[(int) Piece.wN] == 0) {
                    if (board.PceNum[(int) Piece.bK] < 3 &&
                        board.PceNum[(int) Piece.bB] == 0 ||
                        board.PceNum[(int) Piece.bB] == 1 &&
                        board.PceNum[(int) Piece.bN] == 0) {
                        return true;
                    }
                }

                // If no queens
            } else if (board.PceNum[(int) Piece.wQ] == 0 &&
                       board.PceNum[(int) Piece.bQ] == 0) {
                // if excatly one of each rook.
                if (board.PceNum[(int) Piece.wR] == 1 && board.PceNum[(int) Piece.bR] == 1) {
                    // Each side less than two minor pieces.
                    if (board.MinPieces[(int) Colour.WHITE] < 2 &&
                        board.MinPieces[(int) Colour.BLACK] < 2) {
                        return true;
                    }

                    // one white rook zero black
                } else if (board.PceNum[(int) Piece.wR] == 1 &&
                           board.PceNum[(int) Piece.bR] == 0) {
                    // No white bishops or knights
                    if (board.MinPieces[(int) Colour.WHITE] == 0 &&
                        // two or one black bishop / knight
                        (board.MinPieces[(int) Colour.BLACK] == 2 ||
                         board.MinPieces[(int) Colour.BLACK] == 1)) {
                        return true;
                    }
                } else if (board.PceNum[(int) Piece.wR] == 0 &&
                           board.PceNum[(int) Piece.bR] == 1) {
                    if (board.MinPieces[(int) Colour.BLACK] == 0 &&
                        // two or one black bishop / knight
                        (board.MinPieces[(int) Colour.WHITE] == 2 ||
                         board.MinPieces[(int) Colour.WHITE] == 1)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     Decrements or increments the parameter score, in regard to the pieces position on the board.
        ///     Method should be used for knights, bishops and king.
        /// </summary>
        private static void AddKnBishopKingScores(Board.Board board, int pceType, int[] table,
            ref int score) {
            var colour = Data.PIECE_COLOURS[pceType];

            for (var i = 0; i < board.PceNum[pceType]; ++i) {
                var sq = board.PList[pceType, i];
                Debug.Assert(Validators.SqOnBoard(sq));
                var sq64 = Conversion.getSq120ToSq64(sq);
                var value = colour == Colour.WHITE
                    ? table[sq64]
                    : -table[Data.mirror64Table[sq64]];

                score += value;
            }
        }

        /// <summary>
        ///     Decrements or increments the parameter score, in regard to the pieces position on the board.
        ///     Method should be used for pawns.
        /// </summary>
        private static void AddPawnScores(Board.Board board, int colour,
            ref int score) {
            var pceType = colour == (int) Colour.WHITE ? (int) Piece.wP : (int) Piece.bP;
            for (var i = 0; i < board.PceNum[pceType]; ++i) {
                var sq = board.PList[pceType, i];
                Debug.Assert(Validators.SqOnBoard(sq));
                var sq64 = Conversion.getSq120ToSq64(sq);
                var value = colour == (int) Colour.WHITE
                    ? Evaluate.pawnTable[sq64]
                    : -Evaluate.pawnTable[Data.mirror64Table[sq64]];

                score += value;

                // If pawn we need to consider whether it's isolated and on what rank.
                if (colour == (int) Colour.WHITE) {
                    if ((EvalBitMask.GetIsolatedMask(sq64) & board.Pawns[(int) Colour.WHITE]) ==
                        0) {
                        score += Evaluate.PAWN_ISOLATED_VAL;
                    }

                    if ((EvalBitMask.GetWhitePassedMask(sq64) &
                         board.Pawns[(int) Colour.BLACK]) == 0) {
                        var rank = Conversion.getRanksBrd(sq);
                        Debug.Assert(Validators.FileOrRankValid(rank));
                        score += Evaluate.PAWN_PASSED_VAL[rank];
                    }
                } else {
                    if ((EvalBitMask.GetIsolatedMask(sq64) & board.Pawns[(int) Colour.BLACK]) ==
                        0) {
                        score -= Evaluate.PAWN_ISOLATED_VAL;
                    }

                    if ((EvalBitMask.GetBlackPassedMask(sq64) &
                         board.Pawns[(int) Colour.WHITE]) == 0) {
                        var rank = Conversion.getRanksBrd(sq);
                        Debug.Assert(Validators.FileOrRankValid(rank));
                        score -= Evaluate.PAWN_PASSED_VAL[7 - rank];
                    }
                }
            }
        }

        /// <summary>
        ///     Decrements or increments the parameter score, in regard to the pieces position on the board.
        ///     Method should be used for queens and rooks.
        /// </summary>
        private static void AddRookOrQueenScores(Board.Board board, int pceType, ref int score) {
            Debug.Assert(Data.IsPieceRookQueen(pceType));
            var colour = (int) Data.PIECE_COLOURS[pceType];

            for (var i = 0; i < board.PceNum[pceType]; ++i) {
                var sq = board.PList[pceType, i];
                Debug.Assert(Validators.SqOnBoard(sq));
                var sq64 = Conversion.getSq120ToSq64(sq);
                var value = colour == (int) Colour.WHITE
                    ? Evaluate.pawnTable[sq64]
                    : -Evaluate.pawnTable[Data.mirror64Table[sq64]];

                score += value;

                var file = Conversion.getFilesBrd(sq);
                var fileValue = 0;
                if ((EvalBitMask.GetFilesMask(file) & board.Pawns[(int) Colour.BOTH]) == 0) {
                    // Is piece Rook 
                    if (!Data.IsPieceBishopQueen(pceType) && Data.IsPieceRookQueen(pceType)) {
                        fileValue = Evaluate.ROOK_OPEN_FILE_VAL;
                    } else {
                        // Else piece is queen
                        fileValue = Evaluate.QUEEN_OPEN_FILE_VAL;
                    }
                } else if ((EvalBitMask.GetFilesMask(file) & board.Pawns[colour]) == 0) {
                    if (!Data.IsPieceBishopQueen(pceType) && Data.IsPieceRookQueen(pceType)) {
                        fileValue = Evaluate.ROOK_SEMI_OPEN_FILE_VAL;
                    } else {
                        // Piece is queen
                        fileValue = Evaluate.QUEEN_SEMI_OPEN_FILE_VAL;
                    }
                }

                fileValue = colour == (int) Colour.WHITE ? fileValue : -fileValue;
                score += fileValue;
            }
        }
    }
}