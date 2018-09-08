using System;
using System.Collections;
using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Board {
    /// <summary>
    /// Class for maintaining and updating the gameboard.
    /// </summary>
    public static class BoardOperations {
        /// <summary>
        /// Resets all board data structures.
        /// </summary>
        /// <param name="board"></param>
        public static void ResetBoard(Board board) {
            int index = 0;
            
            // sets all squares on the board to empty.
            for (index = 0; index < Variables.BRD_SQ_NUM; ++index) {
                board[index] = (int) Square.NO_SQ;
            }

            for (index = 0; index < 64; ++index) {
                board[Conversion.getSq64ToSq120(index)] = (int) Piece.EMPTY;
            }

            for (index = 0; index < 3; ++index) {
                if (index < 2) {
                    board.BigPieces[index] = 0;
                    board.MajPieces[index] = 0;
                    board.MinPieces[index] = 0;
                    board.Material[index] = 0;
                }
                board.Pawns[index] = 0UL;
            }

            for (index = 0; index < 13; ++index) {
                board.PceNum[index] = 0;
            }

            board.KingSq[(int) Colour.WHITE] = (int) Square.NO_SQ;
            board.KingSq[(int) Colour.BLACK] = (int) Square.NO_SQ;

            board.Side = (int) Colour.BOTH;
            board.EnPas = (int) Square.NO_SQ;
            board.FiftyMoves = 0;

            board.Ply = 0;
            board.HistoryPly = 0;
            for (Colour id = Colour.WHITE; id <= Colour.BLACK; ++id) {
                board.Material[(int) id] = 0;
            }
            board.CastlePerm = 0;
            board.PosKey = 0UL;
        }
        
        /// <summary>
        /// Parses the input FEN string onto the board.
        /// </summary>
        /// <param name="fen"> The FEN-string to parse. </param>
        /// <param name="board"> The board to update. </param>
        /// <returns></returns>
        public static int ParseFen(string fen, Board board) {
            int rank = (int) Rank.RANK_8;
            int file = (int) File.FILE_A;

            int fenIndex = 0;
            int i = 0;
            char fenChar = ' ';
            
            BoardOperations.ResetBoard(board);
            
            while (rank >= (int) Rank.RANK_1 && fenIndex <= fen.Length) {
                fenChar = fen[fenIndex];
                var count = 1;
                var piece = 0;
                switch (fenChar) {
                    case 'p' : piece = (int) Piece.bP;
                        break;
                    case 'r' : piece = (int) Piece.bR;
                        break;
                    case 'n' : piece = (int) Piece.bN;
                        break;
                    case 'b' : piece = (int) Piece.bB;
                        break;
                    case 'k' : piece = (int) Piece.bK;
                        break;
                    case 'q' : piece = (int) Piece.bQ;
                        break;
                    case 'P' : piece = (int) Piece.wP;
                        break;
                    case 'R' : piece = (int) Piece.wR;
                        break;
                    case 'N' : piece = (int) Piece.wN;
                        break;
                    case 'B' : piece = (int) Piece.wB;
                        break;
                    case 'K' : piece = (int) Piece.wK;
                        break;
                    case 'Q' : piece = (int) Piece.wQ;
                        break;
                    
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                        piece = (int) Piece.EMPTY;
                        count = fenChar - '0'; // holds the number of empty squares.
                        break;
                    case '/' :
                    case ' ' :
                        rank--;
                        file = (int) File.FILE_A;
                        fenIndex++;
                        continue;
                    default:
                        Console.Write("FEN Error default \n");
                        return -1;
                }

                for (i = 0; i < count; i++) {
                    var sq64 = (int) rank * 8 + (int) file;
                    var sq120 = Conversion.getSq64ToSq120(sq64);
                    if (piece != (int) Piece.EMPTY) {
                        board[sq120] = piece;
                    }

                    file++;
                }

                fenIndex++;
            }
            
            fenChar = fen[fenIndex];
            
                board.Side = (fenChar == 'w') ? (int) Colour.WHITE : (int) Colour.BLACK;
                fenIndex += 2; // We're now at castling permissions.

                for (i = 0; i < 4; i++) {
                    fenChar = fen[fenIndex];
                    if (fenChar == ' ') {
                        break;
                    }

                    switch (fenChar) {
                        case 'K':
                            board.CastlePerm |= (int) Castling.WKCA;
                            break;
                        case 'Q':
                            board.CastlePerm |= (int) Castling.WQCA;
                            break;
                        case 'k':
                            board.CastlePerm |= (int) Castling.BKCA;
                            break;
                        case 'q':
                            board.CastlePerm |= (int) Castling.BQCA;
                            break;
                        default:
                            break;
                    }
                    fenIndex++;
                }

                fenIndex++;
                fenChar = fen[fenIndex]; // We're not at the ENPASSANT SQ.
            
               
                if (board.CastlePerm >= 0 && board.CastlePerm <= 15) {
                    if (fenChar != '-') { // if there is an enpassant sq.
                        file = fenChar - 'a';
                        rank = fen[fenIndex + 1] - '1';
                        board.EnPas = (int) Conversion.FR2SQ(file, rank);
                    }
                    
                    BoardOperations.UpdateListsMaterial(board); 

                    board.PosKey = Hashkeys.GeneratePosKey(board);

                    return 0;
                } else {
                    Console.WriteLine("FEN CASTLEPERM ERROR");
                    return -1;
                }
        }

        /// <summary>
        /// Prints the input board to console.
        /// </summary>
        /// <param name="board"> THe board to print. </param>
        public static void PrintBoard(Board board) {
            int sq, file, rank, piece;
            
            Console.Write("\nGame Board:\n\n");

            for (rank = (int) Rank.RANK_8; rank >= (int) Rank.RANK_1; rank--) {
                Console.Write("{0}   ", rank+1);
                for (file = (int) File.FILE_A; file <= (int) File.FILE_H; file++) {
                    sq = (int) Conversion.FR2SQ(file, rank);
                    piece = board[sq];
                    Console.Write(" {0} ", Data.PCE_CHAR[piece]);
                }

                Console.Write("\n");
            }

            Console.Write("\n     A  B  C  D  E  F  G  H");

            Console.Write("\n");
            Console.Write("side:{0}\n", Data.SIDE_CHAR[board.Side]);
            Console.Write("enPas:{0}\n", board.EnPas);
            Console.Write("castle:{0}{1}{2}{3}\n", 
                (board.CastlePerm & (int) Castling.WKCA) != 0 ? 'K' : '-',
                (board.CastlePerm & (int) Castling.WQCA) != 0 ? 'Q' : '-',
                (board.CastlePerm & (int) Castling.BKCA) != 0 ? 'k' : '-',
                (board.CastlePerm & (int) Castling.BQCA) != 0 ? 'q' : '-');
            Console.Write("posKey: {0:X}\n", board.PosKey);
        }
        
        /// <summary>
        /// Updates the remaining board data structures, in regard to the "main" board.
        /// To be called if board[sq] is updated, but the remaining structures are not.
        /// </summary>
        /// <param name="board"> The board to update </param>
        public static void UpdateListsMaterial(Board board) {
            for (int index = 0; index < 64; ++index) {
                int sq = Conversion.getSq64ToSq120(index);
                int piece = board[sq];
                if (piece != (int) Piece.EMPTY) {
                    int colour = (int) Data.PIECE_COLOURS[piece];

                    if (Data.IS_PIECE_BIG[piece]) {
                        board.BigPieces[colour]++;
                    }

                    if (Data.IS_PIECE_MIN[piece]) {
                        board.MinPieces[colour]++;
                    }

                    if (Data.IS_PIECE_MAJ[piece]) {
                        board.MajPieces[colour]++;
                    }

                    board.Material[colour] += Data.PIECE_VAL[piece];
                    board.PList[piece, board.PceNum[piece]] = sq;
                    board.PceNum[piece]++;

                    if (piece == (int) Piece.wK || piece == (int) Piece.bK) {
                        board.KingSq[colour] = sq;
                    }

                    if (piece == (int) Piece.wP || piece == (int) Piece.bP) {
                        BitBoard.SetBit(ref board.Pawns[colour], index);
                        BitBoard.SetBit(ref board.Pawns[(int) Colour.BOTH], index);
                    }
                }
            }
        }

        /// <summary>
        /// Validator for the board state. Ensures all board data structures are inorder.
        /// Will throw an exception if something is astray.
        /// </summary>
        /// <remarks>
        /// CheckBoard is a very time consuming method,
        /// why it's only called if compiled in DEBUG
        /// </remarks>
        /// <param name="board"> The board to validate </param>
        /// <returns> True, if the board is valid </returns>
        public static bool CheckBoard(Board board) {
            int[] t_pceNum = new int[13];
            int[] t_bigPce = new int[2];
            int[] t_majPce = new int[2];
            int[] t_minPce = new int[2];
            int[] t_material = new int[2];

            ulong[] t_pawns = {0UL, 0UL, 0UL};
            BitArray[] t_pawnsBitArrray = new BitArray[3];

            for (int i = 0; i < board.Pawns.Length; ++i) {
                t_pawns[i] = board.Pawns[i];
            } 
            
            // check piece lists:
            for (int t_piece = (int) Piece.wP; t_piece <= (int) Piece.bK; ++t_piece) {
                for (int t_pce_num = 0; t_pce_num < board.PceNum[t_piece]; ++t_pce_num) {
                    int sq120 = board.PList[t_piece, t_pce_num];
                    if (board[sq120] != t_piece) {
                        BoardOperations.PrintBoard(board);
                        throw new Exception(String.Format(
                            "Error in the piece lists. Expected piece {0} on square {1}, but found piece {2} on board",
                            (Piece) t_piece, Io.SqToString(sq120), (Piece) board[sq120]));
                    }
                }
            }
            
            // check piece counters.
            for (int sq64 = 0; sq64 < 64; ++sq64) {
                int sq120 = Conversion.getSq64ToSq120(sq64);
                int t_piece = board[sq120];
                t_pceNum[t_piece]++;
                Colour pceColour = Data.PIECE_COLOURS[t_piece];
                if (Data.IS_PIECE_BIG[t_piece]) {
                    t_bigPce[(int) pceColour]++;
                }

                if (Data.IS_PIECE_MIN[t_piece]) {
                    t_minPce[(int) pceColour]++;
                }

                if (Data.IS_PIECE_MAJ[t_piece]) {
                    t_majPce[(int) pceColour]++;
                }

                if (pceColour != Colour.BOTH) {
                    t_material[(int) pceColour] += Data.PIECE_VAL[t_piece];                    
                }
            }

            for (int t_piece = (int) Piece.wP; t_piece <= (int) Piece.wK; ++t_piece) {
                if (t_pceNum[t_piece] != board.PceNum[t_piece]) {
                    throw new Exception("PceNum does not match board PceNum");
                }
            }

            // For ulong
            for (int i = 0; i < (int) Colour.BOTH; ++i) {
                int pawn_count = BitBoard.CountBits(t_pawns[i]);
                if (i == (int) Colour.WHITE) {
                    if (pawn_count != board.PceNum[(int) Piece.wP]) {
                        throw new Exception(
                            "white pawn bit count does not match white PceNum count.");
                    }
                } else if (i == (int) Colour.BLACK) {
                    if (pawn_count != board.PceNum[(int) Piece.bP]) {
                        throw new Exception("Black pawn bit count does not match black PceNum count.");
                    }
                } else { // COLOUR.BOTH
                    if (pawn_count != (board.PceNum[(int) Piece.wP] + board.PceNum[(int) Piece.bP])) {
                        throw new Exception("BOTH pawn bit count does not match BOTH PceNum count.");
                    }
                }
            }
            
            for (int i = 0; i < (int) Colour.BOTH; ++i) {
                while (t_pawns[i] != 0) {
                    int pawn_sq64 = BitBoard.PopBit(ref t_pawns[i]);
                    int sq120 = Conversion.getSq64ToSq120(pawn_sq64);
                    int pawn = 0;
                    if (i == (int) Colour.WHITE) {
                        pawn = (int) Piece.wP;
                    } else if (i == (int) Colour.BLACK) {
                        pawn = (int) Piece.bP;
                    } else { // Colour.BOTH
                        if (board[sq120] != (int) Piece.wP &&
                            board[sq120] != (int) Piece.bP) {
                            throw new Exception(String.Format("Found {0} on {1} instead of a pawn", (Piece) board[sq120], Io.SqToString(sq120)));                                
                        }
                        continue;
                    }
                    
                    if (board[sq120] != pawn) {
                        throw new Exception(String.Format("Found {0} on {1} instead of a pawn", (Piece) board[sq120], Io.SqToString(sq120)));
                    }    
                }
            }
            
            if (t_material[(int) Colour.WHITE] != board.Material[(int) Colour.WHITE] ||
                t_material[(int) Colour.BLACK] != board.Material[(int) Colour.BLACK]) {
                Console.WriteLine("WHITE:\n\tt_material: {0}\n\tboard.material: {1}",
                    t_material[(int) Colour.WHITE], board.Material[(int) Colour.WHITE]);
                throw new Exception("Material doesnt match with tempoary material.");
            }
            
            if (t_bigPce[(int) Colour.WHITE] != board.BigPieces[(int) Colour.WHITE] ||
                t_bigPce[(int) Colour.BLACK] != board.BigPieces[(int) Colour.BLACK]) {
                throw new Exception("BigPces doesnt match with tempoary BigPces.");
            }
            
            if (t_minPce[(int) Colour.WHITE] != board.MinPieces[(int) Colour.WHITE]  ||
                t_minPce[(int) Colour.BLACK] != board.MinPieces[(int) Colour.BLACK]) {
                throw new Exception("minPce doesnt match with tempoary minPces.");
            }
            
            if (t_majPce[(int) Colour.WHITE] != board.MajPieces[(int) Colour.WHITE]  ||
                t_majPce[(int) Colour.BLACK] != board.MajPieces[(int) Colour.BLACK]) {
                throw new Exception("majPce doesnt match with tempoary majPces.");
            }

            if (board.Side != (int) Colour.WHITE && board.Side != (int) Colour.BLACK) {
                throw new Exception("Side to play on the board is neither black or white.");
            }

            if (board.PosKey != Hashkeys.GeneratePosKey(board)) {
                throw new Exception("Position key doesnt match the generated poskey.");
            }
            
            /// Check enPas sq.
            if (board.EnPas != (int) Square.NO_SQ) {
                if ((board.Side == (int) Colour.WHITE) && (Conversion.getRanksBrd(board.EnPas) != (int) Rank.RANK_6)) {
                    throw new Exception("The EnPas square is invalid");
                } else if ((board.Side == (int) Colour.BLACK) &&
                           (Conversion.getRanksBrd(board.EnPas) != (int) Rank.RANK_3)) {
                    throw new Exception("The EnPas square is invalid");
                }
            }
            
            
            // Check kingsquares
            
            /// white king
            if (board[board.KingSq[(int) Colour.WHITE]] != (int) Piece.wK) {
                throw new Exception("White king square is wrong.");
            }
            /// black king
            if (board[board.KingSq[(int) Colour.BLACK]] != (int) Piece.bK) {
                int blackKingSq = board.KingSq[(int) Colour.BLACK];
                throw new Exception(String.Format("Expected black king on square {0}, but found pce {1} on board", Io.SqToString(blackKingSq), (Piece) board[blackKingSq]));
            }
            
            return true;
        }
        
        public static void MirrorBoard(Board board) {
            int[] t_PiecesArray = new int[64];
            int t_side = board.Side ^ 1; // Changes side
            int t_castlePerm = 0;
            int t_enPas = (int) Square.NO_SQ;
            
            // First we reverse castlepermissions.
            if ((board.CastlePerm & (int) Castling.WKCA) != 0) {
                t_castlePerm |= (int) Castling.BKCA;
            }
                        
            if ((board.CastlePerm & (int) Castling.WQCA) != 0) {
                t_castlePerm |= (int) Castling.BQCA;
            }
            
            if ((board.CastlePerm & (int) Castling.BKCA) != 0) {
                t_castlePerm |= (int) Castling.WKCA;
            }
            
            if ((board.CastlePerm & (int) Castling.BQCA) != 0) {
                t_castlePerm |= (int) Castling.WQCA;
            }

            // En pas
            if (board.EnPas != (int) Square.NO_SQ) {
                t_enPas = Conversion.getSq64ToSq120(Data.mirror64Table[Conversion.getSq120ToSq64(board.EnPas)]);                
            }

            for (int i = 0; i < 64; i++) {
                var mirror120Sq = Conversion.getSq64ToSq120(Data.mirror64Table[i]);
                t_PiecesArray[i] = board[mirror120Sq];
            }
            
            // Have all info so now we reset and put information in.
            BoardOperations.ResetBoard(board);

            for (int i = 0; i < 64; i++) {
                int reversedPiece = (int) Data.SWAP_PIECE[t_PiecesArray[i]];
                board[Conversion.getSq64ToSq120(i)] = reversedPiece;
            }

            board.Side = t_side;
            board.EnPas = t_enPas;
            board.CastlePerm = t_castlePerm;
            board.PosKey = Hashkeys.GeneratePosKey(board);
            
            BoardOperations.UpdateListsMaterial(board);
            
            Debug.Assert(BoardOperations.CheckBoard(board));
        }
    }
}
