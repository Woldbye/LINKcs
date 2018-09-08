using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Move {
    public class MakeMove {
        // All fields are 15, except for the rooks and kings. 
        // 15 equals 1111 in binary, which is the value for a complete castle permision.
        // Therefore we can call board.CastlePerm &= CASTLE_PERM[fromSQ] to update our castle perm.
        private static readonly int[] castlePerm = {
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 13, 15, 15, 15, 12, 15, 15, 14, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 7, 15, 15, 15, 3, 15, 15, 11, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15
        };

        // POSSIBLY BIG ERROR WITH ALL HASH METHODS. SHOULD BE A REFERENCE OF BOARD PASSED.
        private static void HashPiece(Board.Board board, int pce, int sq) {
            board.PosKey ^= Hashkeys.PieceKeys[pce, sq];
        }

        private static void HashCastle(Board.Board board) {
            board.PosKey ^= Hashkeys.CastleKeys[board.CastlePerm];
        }

        private static void HashSide(Board.Board board) {
            board.PosKey ^= Hashkeys.SideKey;
        }

        private static void HashEnPas(Board.Board board) {
            board.PosKey ^= Hashkeys.PieceKeys[(int) Piece.EMPTY, board.EnPas];
        }
        
        /// <summary>
        /// Clears the piece from the board on the parameter square.
        /// </summary>
        /// <param name="sq"> The position of the piece </param>
        /// <param name="board"> The board to remove from </param>
        public static void ClearPiece(int sq, Board.Board board) {
            Debug.Assert(Validators.IsSq(sq), String.Format("Invalid Square {0}", Io.SqToString(sq)));
            
            int pce = board[sq];
            int colour = (int) Data.PIECE_COLOURS[pce];
            int t_pceNum = -1;

            Debug.Assert(Validators.PieceValid(pce), String.Format("Invalid Piece {0} at square {1}", (Piece) pce, Io.SqToString(sq)));
            
            MakeMove.HashPiece(board, pce, sq);
            
            board[sq] = (int) Piece.EMPTY;
            // Subtract the value of the piece from the material total.
            board.Material[colour] -= Data.PIECE_VAL[pce];

            if (Data.IS_PIECE_BIG[pce]) {
                board.BigPieces[colour]--;
                if (Data.IS_PIECE_MAJ[pce]) {
                    board.MajPieces[colour]--;
                } else { // else piece is minor piece
                    board.MinPieces[colour]--;
                }
            } else { // else piece is pawn.
                int sq64 = Conversion.getSq120ToSq64(sq);
                BitBoard.ClearBit(ref board.Pawns[colour], sq64);
                BitBoard.ClearBit(ref board.Pawns[(int) Colour.BOTH], sq64);
            }
            
            for (int i = 0; i < board.PceNum[pce]; ++i) {
                // Find the correct piece by comparing squares.
                if (board.PList[pce, i] == sq) {
                    t_pceNum = i;                        
                    break;
                }
            }
            
            // Ensure that the piece has been located in the PList.
            Debug.Assert(t_pceNum != -1, String.Format("Piece {0} at square {1} wasn't found in the Board's PList", (Piece) pce, Io.SqToString(sq)));
            
            // Decrement the value of the PList.
            board.PceNum[pce]--;
            board.PList[pce, t_pceNum] = board.PList[pce, board.PceNum[pce]];
        }
        
        /// <summary>
        /// Adds the piece to the board on the parameter square.
        /// </summary>
        /// <param name="sq"> The position of the piece </param>
        /// <param name="pce"> The piece to add to the board </param>
        /// <param name="board"> The board to add to </param>
        public static void AddPiece(int sq, Board.Board board, int pce) {
            Debug.Assert(Validators.PieceValid(pce), String.Format("Invalid Piece integer value {0}", pce));
            Debug.Assert(Validators.IsSq(sq), String.Format("Invalid Square {0}", Io.SqToString(sq)));
            
            int colour = (int) Data.PIECE_COLOURS[pce];
            
            // Hash in the piece
            MakeMove.HashPiece(board, pce, sq);
            
            // Add the piece to the board
            board[sq] = pce;
            
            // Add the value of the piece to the material total.
            board.Material[colour] += Data.PIECE_VAL[pce];

            if (Data.IS_PIECE_BIG[pce]) {
                board.BigPieces[colour]++;
                if (Data.IS_PIECE_MAJ[pce]) {
                    board.MajPieces[colour]++;
                } else { // else piece is minor piece
                    board.MinPieces[colour]++;
                }
            } else { // else piece is pawn.
                int sq64 = Conversion.getSq120ToSq64(sq);
                BitBoard.SetBit(ref board.Pawns[colour], sq64);
                BitBoard.SetBit(ref board.Pawns[(int) Colour.BOTH], sq64);                
            }

            // holds the position of every piece on the board.
            // pList[(int) wN][0] = E1;
            // pList[(int) wN][1] = D4; ... ...
            // emptySquares initialized to NO_SQ.
            board.PList[pce, board.PceNum[pce]++] = sq;
        }
        
        /// <summary>
        /// Moves the piece at the from square to the to square
        /// </summary>
        /// <param name="from"> The starting square for the piece </param>
        /// <param name="to"> The new square for the piece </param>
        /// <param name="board"> The board to operate on </param>
        /// <exception cref="Exception"> Throws an exception if the from or to Square is invalid </exception>
        public static void MovePiece(int from, int to, Board.Board board) {
            Debug.Assert(Validators.IsSq(from), String.Format("Invalid From Square {0}", Io.SqToString(from)));
            Debug.Assert(Validators.IsSq(to), String.Format("Invalid To Square {0}", Io.SqToString(from)));

            int pce = board[from];
            
            Debug.Assert(Validators.PieceValid(pce), String.Format(
                "Invalid Pce represented by integer {0} On Sq {0}", pce, Io.SqToString(from)));
            
            int colour = (int) Data.PIECE_COLOURS[pce];
            int t_pceNum = -1;
            
            MakeMove.HashPiece(board, pce, from);
            // Removes the piece from the from square.
            board[from] = (int) Piece.EMPTY;

            MakeMove.HashPiece(board, pce, to);
            // Adds the piece to the to square.
            
            board[to] = pce;
            
            if (!Data.IS_PIECE_BIG[pce]) { // If piece is pawn.
                int sq64From = Conversion.getSq120ToSq64(from);
                int sq64To = Conversion.getSq120ToSq64(to);
                // Clear from sq
                BitBoard.ClearBit(ref board.Pawns[colour], sq64From);
                BitBoard.ClearBit(ref board.Pawns[(int) Colour.BOTH], sq64From);
                        
                // Sets to sq
                BitBoard.SetBit(ref board.Pawns[colour], sq64To);
                BitBoard.SetBit(ref board.Pawns[(int) Colour.BOTH], sq64To);
            }
            
            // Now we move the piece in the piece list:
            for (int i = 0; i < board.PceNum[pce]; ++i) {
                // Find the correct piece by comparing squares.
                if (board.PList[pce, i] == from) {
                    board.PList[pce, i] = to;
                    t_pceNum = i;
                    break;
                }
            }
            
            // If t_pceNum is unchanged.
            Debug.Assert(t_pceNum != -1, String.Format("Piece {0} at square {1} wasn't found in the Board's PList", (Piece) pce, Io.SqToString(from)));
        }

        // Possibly not static 
        // Returns true if legal move, or false otherwise
        /// <summary>
        /// Makes the input move on the board, if the move is legal.
        /// If illegal, nothing happens and the method returns false.
        /// </summary>
        /// <param name="board"> The board to make the move on </param>
        /// <param name="move"> The move to perform </param>
        public static bool Make_Move(Board.Board board, int move) {
            Debug.Assert(BoardOperations.CheckBoard(board), "The CheckBoard method returned false.");
            
            int from = MoveOperations.FromSq(move);
            int to = MoveOperations.ToSq(move);
            int side = board.Side;

            Debug.Assert(Validators.SqOnBoard(from), String.Format("Invalid From Square {0}", Io.SqToString(from)));
            Debug.Assert(Validators.SqOnBoard(to), String.Format("Invalid To Square {0}", Io.SqToString(to)));
            Debug.Assert(Validators.SideValid(side), String.Format("Invalid Side with integer value {0}", side));
            Debug.Assert(Validators.PieceValid(board[from]), String.Format("Invalid From Square {0}", Io.SqToString(from)));

            board.History[board.HistoryPly].PosKey = board.PosKey;
            
            // Now we check for special moves.
            if ((move & MoveOperations.MoveFlagEnPas) != 0) {
                if (side == (int) Colour.WHITE) { // If EnPas move we need to kill the pawn
                    ClearPiece(to-10, board); 
                } else { // Else black
                    ClearPiece(to+10, board);
                }
            } else if ((move & MoveOperations.MoveFlagCastle) != 0) {
                // If castle we need to move the rook.
                switch (to) {
                    case (int) Square.C1:
                        MovePiece((int) Square.A1, (int) Square.D1, board); break;
                    case (int) Square.C8:
                        MovePiece((int) Square.A8, (int) Square.D8, board); break;
                    case (int) Square.G1: 
                        MovePiece((int) Square.H1, (int) Square.F1, board); break;
                    case (int) Square.G8:
                        MovePiece((int) Square.H8, (int) Square.F8, board); break;
                    default:
                        throw new Exception(
                            "The move is a castling move, but the to square doesn't match any eligible moves");
                }
            }

            if (board.EnPas != (int) Square.NO_SQ) {
                MakeMove.HashEnPas(board);
            }
            
            MakeMove.HashCastle(board); // POSSIBLE ERROR, MAYBE IT SHOULD BE IN IF STATEMENT ABOVE.
            
            // Store information in history array.
            board.History[board.HistoryPly].Move = move;
            board.History[board.HistoryPly].EnPas = board.EnPas;
            board.History[board.HistoryPly].CastlePerm = board.CastlePerm;
            board.History[board.HistoryPly].FiftyMoves = board.FiftyMoves;
            
            // Adjust castle permissions if rook or king has moved.
            board.CastlePerm &= MakeMove.castlePerm[from];
            board.CastlePerm &= MakeMove.castlePerm[to];
            board.EnPas = (int) Square.NO_SQ;

            HashCastle(board);

            int captured = MoveOperations.Captured(move);
            board.FiftyMoves++;
            
            // If there is a piece captured
            if (captured != (int) Piece.EMPTY) {
                // Test if the piece is valid.
                Debug.Assert(Validators.PieceValid(captured), String.Format("Invalid piece ID integer {0}", captured));
                
                ClearPiece(to, board);
                board.FiftyMoves = 0; // reset 50moves, cuz a piece has been captured.
            }

            board.HistoryPly++;
            board.Ply++;
            
            
            if (Data.IsPiecePawn(board[from])) {
                board.FiftyMoves = 0; // Reset 50moves, cuz a pawn has moved.
                if ((move & MoveOperations.MoveFlagPawnStart) != 0) {
                    if (side == (int) Colour.WHITE) {
                        board.EnPas = from + 10;                        
                        Debug.Assert(Conversion.getRanksBrd(board.EnPas) == (int) Rank.RANK_3, String.Format("Invalid board state: the enPas square {0} is wrong", Io.SqToString(board.EnPas)));
                    } else {
                        board.EnPas = from - 10;
                        Debug.Assert(Conversion.getRanksBrd(board.EnPas) == (int) Rank.RANK_6, String.Format("Invalid board state: the enPas square {0} is wrong", Io.SqToString(board.EnPas)));
                    }

                    HashEnPas(board);
                }    
            }

            MovePiece(from, to, board);

            int promotedPce = MoveOperations.Promoted(move);
            if (promotedPce != (int) Piece.EMPTY) { // If there is a promoted piece
                // A piece cannot be promoted to a pawn.
                Debug.Assert(Validators.PieceValid(promotedPce) && !Data.IsPiecePawn(promotedPce), String.Format("Promoted piece {0} is invalid", (Piece) promotedPce));
                ClearPiece(to, board);
                AddPiece(to, board, promotedPce);
            }

            if (Data.IsPieceKing(board[to])) {
                // If piece is king set the KingSQ
                board.KingSq[side] = to;
            }
            
            // Exclusive or, changes the side from white to black or black to white.
            board.Side ^= 1;
            HashSide(board);

            Debug.Assert(BoardOperations.CheckBoard(board));
            // Make sure that the new king square isn't attacked.
            if (Attack.IsSqAttacked(board.KingSq[side], board.Side, board)) {
                TakeMove(board); 
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Evaluates whether the input move, exist from the current board position.
        /// </summary>
        /// <param name="board"> The board to evaluate </param>
        /// <param name="move"> The move to test </param>
        public static bool MoveExists(Board.Board board, int move) {
            MoveList list = new MoveList();
            MoveGen.GenerateAllMoves(board, list, false);

            for (int i = 0; i < list.Count; ++i) {
                // If illegal move.
                if (!Make_Move(board, list.Moves[i].Move)) {
                    continue;
                }

                TakeMove(board);

                if (list.Moves[i].Move == move) {
                    return true;
                }
            }

            return false;
        }
        
        public static void TakeMove(Board.Board board) {
            Debug.Assert(BoardOperations.CheckBoard(board));
            
            // Decrement the plys.
            board.HistoryPly--;
            board.Ply--;

            int move = board.History[board.HistoryPly].Move;
            int from = MoveOperations.FromSq(move);
            int to = MoveOperations.ToSq(move);
            
            Debug.Assert(Validators.SqOnBoard(from), String.Format("Invalid From Square {0}", Io.SqToString(from)));
            Debug.Assert(Validators.SqOnBoard(to), String.Format("Invalid To Square {0}", Io.SqToString(to)));
            
            // If there is an EnPas square, hash it out.
            if (board.EnPas != (int) Square.NO_SQ) {
                HashEnPas(board);
            }

            HashCastle(board);
            
            // Reverse relevant board structures.
            board.CastlePerm = board.History[board.HistoryPly].CastlePerm;
            board.FiftyMoves = board.History[board.HistoryPly].FiftyMoves;
            board.EnPas = board.History[board.HistoryPly].EnPas;
            
            // If the previous move have an EnPas square set, we need to hash it back in
            if (board.EnPas != (int) Square.NO_SQ) {
                HashEnPas(board);
            }
            // Hash the castling back incase it's changed.
            HashCastle(board);
            // Change the side to move back.
            board.Side ^= 1;
            HashSide(board);
            
            if ((move & MoveOperations.MoveFlagEnPas) != 0) {
                if (board.Side == (int) Colour.WHITE) { // If EnPas move we need to add the pawn
                    AddPiece(to-10, board, (int) Piece.bP); 
                } else { // Else black
                    AddPiece(to+10, board, (int) Piece.wP);
                }
            } else if ((move & MoveOperations.MoveFlagCastle) != 0) {
                // If castle we need to move the rook.
                switch (to) {
                case (int) Square.C1:
                    MovePiece((int) Square.D1, (int) Square.A1, board); break;
                case (int) Square.C8:
                    MovePiece((int) Square.D8, (int) Square.A8, board); break;
                case (int) Square.G1: 
                    MovePiece((int) Square.F1, (int) Square.H1, board); break;
                case (int) Square.G8:
                    MovePiece((int) Square.F8, (int) Square.H8, board); break;
                default:
                    throw new Exception(
                        "The move is a castling move, but the to square doesn't match any eligible moves");
                }
            }
            
            // Move the piece back to the original square.
            MovePiece(to, from, board);

            if (Data.IsPieceKing(board[from])) {
                board.KingSq[board.Side] = from;
            }

            int captured = MoveOperations.Captured(move);
            if (captured != (int) Piece.EMPTY) {
                Debug.Assert(Validators.PieceValid(captured), "Invalid piece captured");
                
                AddPiece(to, board, captured);
            }

            int promotedPce = MoveOperations.Promoted(move);
            if (promotedPce != (int) Piece.EMPTY) { // If there is a promoted piece
                // A piece cannot be promoted to a pawn.
                Debug.Assert((Validators.PieceValid(promotedPce) || !Data.IsPiecePawn(promotedPce)), String.Format("Promoted piece {0} is invalid. PieceValid returned {1}, and IsPiecePawn returned {2}", (Piece) promotedPce, Validators.PieceValid(promotedPce), Data.IsPiecePawn(promotedPce)));
                ClearPiece(from, board);
                AddPiece(from, board, ((Data.PIECE_COLOURS[promotedPce] == Colour.WHITE) ? (int) Piece.wP : (int) Piece.bP));
            }

            Debug.Assert(BoardOperations.CheckBoard(board));
        }
    }
}