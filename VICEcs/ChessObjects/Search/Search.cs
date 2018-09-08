using System;
using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Move;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Search {
    public class Search {
        private Board.Board board;
        private readonly int mate;
        private readonly int infinite;
        
        public Search(Board.Board board) {
            this.board = board;
            this.mate = 29000;
            this.infinite = 30000;
        }
        
        public void CheckUp(ref S_SearchInfo info) {
            // If the stoptime has been passed, then stop.
            if (info.isTimeSet && Variables.Watch.ElapsedMilliseconds > info.StopTime) {
                info.Stopped = true;
            }
            // .. Check if time us up, or interrupt from GUI.

            Misc.ReadInput(ref info);
        }
        
        /// <summary>
        /// Finds the best move in the moveList looping from the parameter index
        /// to the end of the MoveList. It then swaps the best move with the
        /// move at the index moveNum.
        /// </summary>
        /// <param name="moveNum"></param>
        /// <param name="list"></param>
        public void PickNextMove(int moveNum, MoveList list) {
            int bestScore = 0;
            int bestIndex = moveNum;
            
            // Find the best move.
            for (int i = moveNum; i < list.Count; ++i) {
                int tempScore = list.Moves[i].Score;
                if (tempScore > bestScore) {
                    bestIndex = i;
                    bestScore = tempScore;
                }
            }
            
            // Swaps the values in the list.
            S_Move tempMove = list.Moves[moveNum];
            list.Moves[moveNum].Move = list.Moves[bestIndex].Move;
            list.Moves[bestIndex] = tempMove;
        }
        
        /// <summary>
        /// Evaluates whether the board position is a repetition
        /// </summary>
        /// <returns> true if position is a repetition </returns>
        public bool IsRepetition() {
            // loop through all keys in our history.
            for (int i = board.HistoryPly - board.FiftyMoves; i < board.HistoryPly; ++i) {
                Debug.Assert((i >= 0) && (i < Variables.MAX_GAME_MOVES));
                
                if (board.PosKey == board.History[i].PosKey) {
                    return true;
                }    
            }
            
            return false;
        }

        /// <summary>
        //  Clears the search to prepare for new search.
        /// </summary>
        /// <param name="info"></param>
        public void Clear(ref S_SearchInfo info) {
            for (int i = 0; i < 13; ++i) {
                for (int j = 0; j < Variables.BRD_SQ_NUM; ++j) {
                    board.SearchHistory[i, j] = 0;
                }
            }

            for (int i = 0; i < 2; ++i) {
                for (int j = 0; j < Variables.MAX_DEPTH; ++j) {
                    board.SearchKillers[i, j] = 0;
                }
            }
            
            board.PvTable.ClearPvTable();
            board.Ply = 0;

            info.Stopped = false;
            info.Nodes = 0;
            info.Fh = 0;
            info.Fhf = 0;
        }

        public int AlphaBeta(int alpha, int beta, int depth, ref S_SearchInfo info, bool DoNull) {
            Debug.Assert(BoardOperations.CheckBoard(board));
            
            if (depth == 0) {
                return Quiescence(alpha, beta, ref info);
            }
            
            if ((info.Nodes & 2047) == 0) {
                CheckUp(ref info);
            }
            
            info.Nodes++;
            
            // If position is a draw.
            if ((IsRepetition() || board.FiftyMoves >= 100) && board.Ply != 0) {
                return 0; 
            }

            if (board.Ply > Variables.MAX_DEPTH - 1) {
                return Evaluate.Position(board);
            }

            bool kingInCheck = Attack.IsSqAttacked(board.KingSq[board.Side], board.Side ^ 1, board);

            // If king is in check, search deeper to get out of check.
            if (kingInCheck) {
                depth++;
                // The two following lines are possibly ERROR.
                long timeInc = (info.StopTime - info.StartTime) * (1/2);
                info.StopTime += timeInc;
            }
            
            MoveList list = new MoveList();
            MoveGen.GenerateAllMoves(board, list, false);
            int oldAlpha = alpha;
            int score = -infinite;
            int legal = 0; // Will increment when we find a legal move.
            int bestMove = Variables.NO_MOVE;
            int PvMove = PvTable.Probe(board);
            // Prioritize Principle Variation move if it's found.
            if (PvMove != Variables.NO_MOVE) {
                for (int i = 0; i < list.Count; ++i) {
                    var move = list.Moves[i].Move;
                    if (move == PvMove) {
                        list.Moves[i].Score = 2000000;
                        break;
                    }
                }
            }
            
            for (int i = 0; i < list.Count; ++i) {
                PickNextMove(i, list);
                
                var move = list.Moves[i].Move;
                if (!MakeMove.Make_Move(board, move)) {
                    continue;
                }

                legal++;
                score = -AlphaBeta(-beta, -alpha, depth - 1, ref info, true);
                MakeMove.TakeMove(board); // Take back the made move.
                
                if (info.Stopped) {
                    return 0; // Back up to the root if times up.
                }
                                
                // We have a new alpha or beta cutoff.
                if (score > alpha) {
                    bool isCaptureMove = (move & MoveOperations.MoveFlagCapture) != 0;
                    // beta cutoff?
                    if (score >= beta) {
                        if (legal == 1) {
                            info.Fhf++; // We searched the best move first.
                        }

                        info.Fh++;

                        // If beta cutoff, but no capture move.
                        if (!isCaptureMove) {
                            board.SearchKillers[1, board.Ply] = board.SearchKillers[0, board.Ply];
                            board.SearchKillers[0, board.Ply] = move;
                        }
                        return beta;
                    }
                    
                    // Alpha cutoff
                    alpha = score;
                    bestMove = move;

                    if (!isCaptureMove) {
                        int from = MoveOperations.FromSq(move);
                        int to = MoveOperations.ToSq(move);
                        board.SearchHistory[board[from], to] += depth; // Prioritizes move near the root of the tree.
                    }
                }
            }
            
            // If we haven't had any legal moves.
            if (legal == 0) {
                // If in check with no legal moves checkmate.
                if (kingInCheck) {
                    return -mate + board.Ply; // Return the amount of moves it takes to mate.
                    // Returning in this way, allows the method to "prefer" the fastest checkmate combination.
                } else {
                    return 0; // Stalemate.
                }
            }

            if (alpha != oldAlpha) {
                PvTable.StoreMove(board, bestMove);
            }
            
            return alpha;
        }
        
        /// <summary>
        /// Search all capture positions, to help avoid the Horizon effect.
        /// </summary>
        private int Quiescence(int alpha, int beta, ref S_SearchInfo info) {
            Debug.Assert(BoardOperations.CheckBoard(board));

            if ((info.Nodes & 2047) == 0) {
                CheckUp(ref info);
            }
            
            info.Nodes++;
            
            // If position is a draw.
            if ((IsRepetition() || board.FiftyMoves >= 100) && board.Ply != 0) {
                return 0; 
            }

            int score = Evaluate.Position(board); // Stand_pat.
            
            if (board.Ply > Variables.MAX_DEPTH - 1) {
                return score;
            }

            if (score >= beta) {
                return beta;
            }

            if (score > alpha) {
                alpha = score;
            }
            
            
            MoveList list = new MoveList();
            MoveGen.GenerateAllMoves(board, list, true); // Only capture moves

            int oldAlpha = alpha;
            score = -infinite;
            int legal = 0; // Will increment when we find a legal move.
            int bestMove = Variables.NO_MOVE;
            int PvMove = PvTable.Probe(board);
            
            for (int i = 0; i < list.Count; ++i) {
                PickNextMove(i, list);
                
                var move = list.Moves[i].Move;
                
                if (!MakeMove.Make_Move(board, move)) {
                    continue;
                }

                legal++;
                score = -Quiescence(-beta, -alpha, ref info);
                MakeMove.TakeMove(board); // Take back the made move.

                if (info.Stopped) {
                    return 0;
                }
                
                // We have a new alpha or beta cutoff.
                if (score > alpha) {
                    bool isCaptureMove = (move & MoveOperations.MoveFlagCapture) != 0;
                    // beta cutoff?
                    if (score >= beta) {
                        if (legal == 1) {
                            info.Fhf++; // We searched the best move first.
                        }

                        info.Fh++;
                        return beta;
                    }
                    
                    // Alpha cutoff
                    alpha = score;
                    bestMove = move;
                }
            }
            
            if (alpha != oldAlpha) {
                PvTable.StoreMove(board, bestMove);
            }
            
            return alpha;
        }
        
        public void SearchPosition(ref S_SearchInfo info) {
            // Iterative deepening
            int bestMove = Variables.NO_MOVE;
            int bestScore = -infinite;
            int currentDepth = 0;
            Clear(ref info);
            
            for (currentDepth = 1; currentDepth <= info.Depth; ++currentDepth) {
                bestScore = AlphaBeta(-infinite, infinite, currentDepth, ref info, true);
                
                if (info.Stopped) {
                    break;
                }
                
                var pvMoves = PvTable.GetLine(currentDepth, board);
                bestMove = board.PvArray[0]; // First move in the PvArray, is the best move.
                Console.Write("info score cp {0} depth {1} nodes {2} time {3} ", 
                    bestScore, currentDepth, info.Nodes, Variables.Watch.ElapsedMilliseconds - info.StartTime);
                pvMoves = PvTable.GetLine(currentDepth, board);
                Console.Write("pv:");
                
                for (int PvNum = 0; PvNum < pvMoves; ++PvNum) {
                    int move = board.PvArray[PvNum];
                    Console.Write(" {0}", Io.MoveToString(move));
                }
                
                Console.Write("\n");
                /*
                if (info.Fh != 0) { // Cannot divide by zero.
                    Console.Write("Ordering: {0}\n", info.Fhf / info.Fh);                    
                }
                */
            }
            // info score cp 13  depth 1 nodes 13 time 15 pv f1b5
            Console.Write("bestmove {0}\n", Io.MoveToString(bestMove));
        }
    }
}