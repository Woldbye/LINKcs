﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Search;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Move {
    /// <summary>
    /// Class for generating moves.
    /// </summary>
    public static class MoveGen {
        /// <summary>
        ///     For looping through the sliding pieces. Keep looping till result is 0.
        /// </summary>
        private static readonly int[] LOOP_SLIDE_PCE = {
            (int) Piece.wB, (int) Piece.wR, (int) Piece.wQ, 0,
            (int) Piece.bB, (int) Piece.bR, (int) Piece.bQ, 0
        };

        /// <summary>
        ///     LOOP_SLIDE_INDEX(Colour.BLACK) = 4.
        /// </summary>
        private static readonly int[] LOOP_SLIDE_INDEX = {0, 4};

        /// <summary>
        ///     For looping through non sliding pieces (Knight, king)
        /// </summary>
        private static readonly int[] LOOP_NON_SLIDE_PCE = {
            (int) Piece.wN, (int) Piece.wK, 0, (int) Piece.bN, (int) Piece.bK, 0
        };

        private static readonly int[] LOOP_NON_SLIDE_INDEX = {0, 3};

        private static readonly int[,] PCE_DIR = new int[13, 8] {
            {0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0},
            {-8, -19, -21, -12, 8, 19, 21, 12},
            {-9, -11, 11, 9, 0, 0, 0, 0},
            {-1, -10, 1, 10, 0, 0, 0, 0},
            {-1, -10, 1, 10, -9, -11, 11, 9},
            {-1, -10, 1, 10, -9, -11, 11, 9},
            {0, 0, 0, 0, 0, 0, 0, 0},
            {-8, -19, -21, -12, 8, 19, 21, 12},
            {-9, -11, 11, 9, 0, 0, 0, 0},
            {-1, -10, 1, 10, 0, 0, 0, 0},
            {-1, -10, 1, 10, -9, -11, 11, 9},
            {-1, -10, 1, 10, -9, -11, 11, 9}
        };

        private static readonly int[] NUM_DIR = {0, 0, 8, 4, 4, 8, 8, 0, 8, 4, 4, 8, 8};

        public static void AddQuietMove(Board.Board board, int move, MoveList list) {
            Debug.Assert(Validators.SqOnBoard(MoveOperations.FromSq(move)));
            Debug.Assert(Validators.SqOnBoard(MoveOperations.ToSq(move)));

            
            list.Moves[list.Count].Move = move;
            /* Captures generate 1.000.000 or higher score, therefore we have to 
             * score the move just below this, to ensure this is second highest priority for search
             * after capture moves.
             */
            if (board.SearchKillers[0, board.Ply] == move) {
                list.Moves[list.Count].Score = 900000;
            } else if (board.SearchKillers[1, board.Ply] == move) {
                list.Moves[list.Count].Score = 800000;
            } else {
                int from = MoveOperations.FromSq(move);
                int to = MoveOperations.ToSq(move);
                list.Moves[list.Count].Score = board.SearchHistory[board[from], to];
            }
            
            list.Count++;
        }

        public static void AddCaptureMove(Board.Board board, int move, MoveList list) {
            Debug.Assert(Validators.SqOnBoard(MoveOperations.FromSq(move)));
            Debug.Assert(Validators.SqOnBoard(MoveOperations.ToSq(move)));
            Debug.Assert(Validators.PieceValidEmpty(MoveOperations.Captured(move)), String.Format("Invalid piece {0} was captured", MoveOperations.Captured(move)));
            
            var captured = MoveOperations.Captured(move);
            var attacker = board[MoveOperations.FromSq(move)];
            
            list.Moves[list.Count].Move = move;
            list.Moves[list.Count].Score = MvvLva.Scores[captured, attacker] + 1000000;
            list.Count++;
        }

        public static void AddEnPassantMove(Board.Board board, int move, MoveList list) {
            Debug.Assert(Validators.SqOnBoard(MoveOperations.FromSq(move)));
            Debug.Assert(Validators.SqOnBoard(MoveOperations.ToSq(move)));
            
            list.Moves[list.Count].Move = move;
            list.Moves[list.Count].Score = 105 + 1000000;
            list.Count++;
        }

        /// <summary>
        /// Generates all available moves for the current position on the board in the parameter MoveList.
        /// If the onlyCaptures parameter is true, it will only generate capture moves.
        /// </summary>
        /// <param name="board"> The current board position </param>
        /// <param name="list"> The MoveList to parse the available moves to. </param>
        /// <param name="onlyCaptures"> True if only generating for captures. </param>
        public static void GenerateAllMoves(Board.Board board, MoveList list, bool onlyCaptures) {
            Debug.Assert(BoardOperations.CheckBoard(board));
            list.Count = 0;
            
            var side = (Colour) board.Side;
            var piece = side == Colour.WHITE ? (int) Piece.wP : (int) Piece.bP;

            for (var pceNum = 0; pceNum < board.PceNum[piece]; ++pceNum) {
                var sq = board.PList[piece, pceNum];
                Debug.Assert(Validators.IsSq(sq), String.Format("Sq {0} is invalid", sq));
                var leftCaptureSq = side == Colour.WHITE ? sq + 9 : sq - 9;
                var rightCaptureSq = side == Colour.WHITE ? sq + 11 : sq - 11;

                if (!onlyCaptures) {
                    var sqInfront = side == Colour.WHITE ? sq + 10 : sq - 10;
                    if (board[sqInfront] == (int) Piece.EMPTY && Validators.SqOnBoard(sqInfront)) {
                        if (side == Colour.WHITE) {
                            MoveGen.AddWhitePawnMove(board, sq, sqInfront, list);
                            
                            // Check if the number two square infront of the pawn is empty and that the rank is 2.
                            if (Conversion.getRanksBrd(sq) == (int) Rank.RANK_2 &&
                                board[sq + 20] == (int) Piece.EMPTY) {
                                MoveGen.AddQuietMove(board,
                                    MoveOperations.CreateMove(sq, sq + 20, (int) Piece.EMPTY,
                                        (int) Piece.EMPTY, MoveOperations.MoveFlagPawnStart), list);
                            }
                        } else { // BLACK
                            MoveGen.AddBlackPawnMove(board, sq, sqInfront, list);
                            
                            // Check if the number two square infront of the pawn is empty and that the rank is 7.
                            if (Conversion.getRanksBrd(sq) == (int) Rank.RANK_7 &&
                                board[sq - 20] == (int) Piece.EMPTY) {
                                MoveGen.AddQuietMove(board,
                                    MoveOperations.CreateMove(sq, sq - 20, (int) Piece.EMPTY,
                                        (int) Piece.EMPTY, MoveOperations.MoveFlagPawnStart), list);
                            }
                        }
                    }                    
                }
                
                if (Validators.IsSq(leftCaptureSq) &&
                    Data.PIECE_COLOURS[board[leftCaptureSq]] == (Colour) ((int) side ^ 1)) {
                    if (side == Colour.WHITE) {
                        MoveGen.AddWhitePawnCapMove(board, sq, leftCaptureSq, board[leftCaptureSq],
                            list);
                    } else {
                        // BLACK
                        MoveGen.AddBlackPawnCapMove(board, sq, leftCaptureSq, board[leftCaptureSq],
                            list);
                    }
                }

                if (Validators.IsSq(rightCaptureSq) &&
                    Data.PIECE_COLOURS[board[rightCaptureSq]] == (Colour) ((int) side ^ 1)) {
                    if (side == Colour.WHITE) {
                        MoveGen.AddWhitePawnCapMove(board, sq, rightCaptureSq,
                            board[rightCaptureSq], list);
                    } else {
                        // BLACK
                        MoveGen.AddBlackPawnCapMove(board, sq, rightCaptureSq,
                            board[rightCaptureSq], list);
                    }
                }

                if (Validators.IsSq(board.EnPas)) {
                    if (leftCaptureSq == board.EnPas) {
                        MoveGen.AddEnPassantMove(board, MoveOperations.CreateMove(sq, leftCaptureSq,
                            (int) Piece.EMPTY
                            , (int) Piece.EMPTY, MoveOperations.MoveFlagEnPas), list);
                    } else if (rightCaptureSq == board.EnPas) {
                        MoveGen.AddEnPassantMove(board, MoveOperations.CreateMove(sq,
                            rightCaptureSq, (int) Piece.EMPTY
                            , (int) Piece.EMPTY, MoveOperations.MoveFlagEnPas), list);
                    }
                }
            }

            // Castling
            if (!onlyCaptures) {
                if (side == Colour.WHITE) {
                    // If there is White King Castle Permission.
                    if ((board.CastlePerm & (int) Castling.WKCA) != 0) {
                        // Check if squares are empty before we call the less efficient SqAttacked method.
                        var isSqG1F1Empty = board[(int) Square.F1] == (int) Piece.EMPTY && board[
                                                (int) Square.G1] == (int) Piece.EMPTY;
                        if (isSqG1F1Empty) {
                            var IsSqE1OrF1NotAtt =
                                !Attack.IsSqAttacked((int) Square.F1, (int) Colour.BLACK, board) &&
                                !Attack.IsSqAttacked((int) Square.E1, (int) Colour.BLACK, board);
                            if (IsSqE1OrF1NotAtt) {
                                MoveGen.AddQuietMove(board,
                                    MoveOperations.CreateMove((int) Square.E1, (int) Square.G1,
                                        (int) Piece.EMPTY, (int) Piece.EMPTY,
                                        MoveOperations.MoveFlagCastle), list);
                            }
                        }
                    }

                    // If there is White Queen Castle Permission.
                    if ((board.CastlePerm & (int) Castling.WQCA) != 0) {
                        var isSqB1C1D1Empty = board[(int) Square.B1] == (int) Piece.EMPTY &&
                                              board[(int) Square.C1] == (int) Piece.EMPTY &&
                                              board[(int) Square.D1] == (int) Piece.EMPTY;
                        if (isSqB1C1D1Empty) {
                            var isSqE1OrD1NotAtt =
                                !Attack.IsSqAttacked((int) Square.E1, (int) Colour.BLACK, board) &&
                                !Attack.IsSqAttacked((int) Square.D1, (int) Colour.BLACK, board);
                            if (isSqE1OrD1NotAtt) {
                                MoveGen.AddQuietMove(board,
                                    MoveOperations.CreateMove((int) Square.E1, (int) Square.C1,
                                        (int) Piece.EMPTY, (int) Piece.EMPTY,
                                        MoveOperations.MoveFlagCastle), list);
                            }
                        }
                    }
                } else {
                    // If there is Black King Castle Permission.
                    if ((board.CastlePerm & (int) Castling.BKCA) != 0) {
                        // Check if squares are empty before we call the less efficient SqAttacked method.
                        var isSqG8F8Empty = board[(int) Square.F8] == (int) Piece.EMPTY &&
                                            board[(int) Square.G8] == (int) Piece.EMPTY;
                        if (isSqG8F8Empty) {
                            var isSqE8F8NotAttacked =
                                !Attack.IsSqAttacked((int) Square.E8, (int) Colour.WHITE, board) &&
                                !Attack.IsSqAttacked((int) Square.F8, (int) Colour.WHITE, board);
                            if (isSqE8F8NotAttacked) {
                                MoveGen.AddQuietMove(board,
                                    MoveOperations.CreateMove((int) Square.E8, (int) Square.G8,
                                        (int) Piece.EMPTY, (int) Piece.EMPTY,
                                        MoveOperations.MoveFlagCastle), list);
                            }
                        }
                    }

                    // If there is White Queen Castle Permission.
                    if ((board.CastlePerm & (int) Castling.BQCA) != 0) {
                        // Check if squares are empty before we call the less efficient SqAttacked method.
                        var isSqB8C8D8Empty = board[(int) Square.B8] == (int) Piece.EMPTY &&
                                              board[(int) Square.C8] == (int) Piece.EMPTY &&
                                              board[(int) Square.D8] == (int) Piece.EMPTY;

                        if (isSqB8C8D8Empty) {
                            var isSqE8D8NotAttacked =
                                !Attack.IsSqAttacked((int) Square.E8, (int) Colour.WHITE, board) &&
                                !Attack.IsSqAttacked((int) Square.D8, (int) Colour.WHITE, board);
                            if (isSqE8D8NotAttacked) {
                                MoveGen.AddQuietMove(board,
                                    MoveOperations.CreateMove((int) Square.E8, (int) Square.C8,
                                        (int) Piece.EMPTY, (int) Piece.EMPTY,
                                        MoveOperations.MoveFlagCastle), list);
                            }
                        }
                    }
                }
            }
            
            // Loop sliding pieces
            var pceIndex = MoveGen.LOOP_SLIDE_INDEX[board.Side];
            // Increments AFTER it's run
            var pce = MoveGen.LOOP_SLIDE_PCE[pceIndex++];
            while (pce != 0) {
                Debug.Assert(Validators.PieceValid(pce),
                    String.Format("You have reached an invalid sliding piece {0}", pce));
                for (var pceNum = 0; pceNum < board.PceNum[pce]; ++pceNum) {
                    var sq = board.PList[pce, pceNum];

                    // Square check
                    Debug.Assert(Validators.IsSq(sq), String.Format("Sq {0} is false", sq));
                    
                    for (var index = 0; index < MoveGen.NUM_DIR[pce]; ++index) {
                        var t_sq = sq + MoveGen.PCE_DIR[pce, index];
                        // Check if offboard
                        if (!Validators.SqOnBoard(t_sq)) {
                            continue;
                        }

                        while (Validators.SqOnBoard(t_sq)) {
                            if (board[t_sq] != (int) Piece.EMPTY) {
                                // 0 EXCLUSIVE OR 1 returns 1 = COLOUR.WHITE
                                // 1 EXCLUSIVE OR 1 returns 0 = COLOUR.BLACK
                                if ((int) Data.PIECE_COLOURS[board[t_sq]] == (board.Side ^ 1)) {
                                    AddCaptureMove(board, MoveOperations.CreateMove(sq, t_sq, board[t_sq], (int) Piece.EMPTY, 0), list);
                                }

                                break;
                            }

                            if (!onlyCaptures) {
                                AddQuietMove(board,
                                    MoveOperations.CreateMove(sq, t_sq, (int) Piece.EMPTY,
                                        (int) Piece.EMPTY, 0), list);                            
                            }
                            t_sq += MoveGen.PCE_DIR[pce, index];
                        }
                    }
                }

                pce = MoveGen.LOOP_SLIDE_PCE[pceIndex++];
            }

            // Loop non_sliding pieces
            pceIndex = MoveGen.LOOP_NON_SLIDE_INDEX[board.Side];
            pce = MoveGen.LOOP_NON_SLIDE_PCE[pceIndex++];

            while (pce != 0) {
                Debug.Assert(Validators.PieceValid(pce), String.Format("You have reached an invalid sliding piece {0}", pce));
                
                for (var pceNum = 0; pceNum < board.PceNum[pce]; ++pceNum) {
                    var sq = board.PList[pce, pceNum];

                    // Square check
                    Debug.Assert(Validators.IsSq(sq), String.Format("Sq {0} is invalid", sq));

                    for (var index = 0; index < MoveGen.NUM_DIR[pce]; ++index) {
                        var t_sq = sq + MoveGen.PCE_DIR[pce, index];

                        // Check if offboard
                        if (!Validators.SqOnBoard(t_sq)) {
                            continue;
                        }

                        if (board[t_sq] != (int) Piece.EMPTY) {
                            // 0 EXCLUSIVE OR 1 returns 1 = COLOUR.WHITE
                            // 1 EXCLUSIVE OR 1 returns 0 = COLOUR.BLACK
                            if ((int) Data.PIECE_COLOURS[board[t_sq]] == (board.Side ^ 1)) {
                                AddCaptureMove(board, MoveOperations.CreateMove(sq, t_sq, board[t_sq], (int) Piece.EMPTY, 0), list);
                            }

                            continue;
                        }

                        if (!onlyCaptures) {
                            AddQuietMove(board,
                                MoveOperations.CreateMove(sq, t_sq, (int) Piece.EMPTY,
                                    (int) Piece.EMPTY, 0), list);    
                        }
                    }
                }


                pce = MoveGen.LOOP_NON_SLIDE_PCE[pceIndex++];
            }
        }
        
        private static void AddBlackPawnCapMove(Board.Board board, int from, int to, int capt,
            MoveList list) {
            Debug.Assert(Validators.PieceValidEmpty(capt), String.Format("Invalid capture square {0}", capt));
            Debug.Assert(Validators.SqOnBoard(from), String.Format("Invalid from square {0}", from));
            Debug.Assert(Validators.SqOnBoard(to), String.Format("Invalid to square {0}", to));

            // If black pawn promotion.
            if (Conversion.getRanksBrd(from) == (int) Rank.RANK_2) {
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.bQ, 0),
                    list);
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.bR, 0),
                    list);
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.bB, 0),
                    list);
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.bN, 0),
                    list);
            } else {
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.EMPTY, 0), list);
            }
        }

        private static void AddBlackPawnMove(Board.Board board, int from, int to,
            MoveList list) {
            MoveGen.AddBlackPawnCapMove(board, from, to, (int) Piece.EMPTY, list);
        }

        // Capture move
        private static void AddWhitePawnCapMove(Board.Board board, int from, int to, int capt,
            MoveList list) {
            Debug.Assert(Validators.PieceValidEmpty(capt), String.Format("Invalid capture square {0}", capt));
            Debug.Assert(Validators.SqOnBoard(from), String.Format("Invalid from square {0}", from));
            Debug.Assert(Validators.SqOnBoard(to), String.Format("Invalid to square {0}", to));

            // If White pawn promotion.
            if (Conversion.getRanksBrd(from) == (int) Rank.RANK_7) {
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.wQ, 0),
                    list);
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.wR, 0),
                    list);
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.wB, 0),
                    list);
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.wN, 0),
                    list);
            } else {
                MoveGen.AddCaptureMove(board,
                    MoveOperations.CreateMove(from, to, capt, (int) Piece.EMPTY, 0), list);
            }
        }

        private static void AddWhitePawnMove(Board.Board board, int from, int to,
            MoveList list) {
            Debug.Assert(Validators.SqOnBoard(from), String.Format("Invalid from square {0}", from));
            Debug.Assert(Validators.SqOnBoard(to), String.Format("Invalid to square {0}", to));
            
            MoveGen.AddWhitePawnCapMove(board, from, to, (int) Piece.EMPTY, list);
        }
    }
}

