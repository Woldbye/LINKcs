using System;
using System.Diagnostics;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Move;

namespace VICEcs.ChessDefinitions {
    /// <summary>
    ///     SEE VIDEO 51 IF ISSUES.
    ///     The Principal variation (PV) is a sequence of moves that programs consider best and therefore expect to be played.
    ///     All the nodes included by the PV are PV-nodes. Inside an iterative deepening framework, it is the most important
    ///     move ordering consideration to play the PV collected during the current iteration, as the very first left moves of
    ///     the next iteration.
    /// </summary>
    public class PvTable {
        private readonly int numEntries;
        public S_PvEntry[] Entries; // needs initialization?

        public PvTable(int size) {
            Entries = new S_PvEntry[size];
            numEntries = size;
        }

        public static int GetLine(int depth, Board board) {
            Debug.Assert(depth < Variables.MAX_DEPTH);

            int move = PvTable.Probe(board);
            int count = 0;

            while (move != Variables.NO_MOVE && count < depth) {
                if (MakeMove.MoveExists(board, move)) {
                    MakeMove.Make_Move(board, move);
                    board.PvArray[count++] = move;
                } else {
                    break;
                }

                move = PvTable.Probe(board);
            }
            
            // Take back the performed moves, so board position stay unchanged.
            while (board.Ply > 0) {
                MakeMove.TakeMove(board); 
            }

            return count;
        }
        
        public void ClearPvTable() {
            for (var i = 0; i < Entries.Length; ++i) {
                Entries[i].Move = Variables.NO_MOVE;
                Entries[i].PosKey = 0UL;
            }
        }

        public static void StoreMove(Board board, int move) {
            var index = (int) (board.PosKey % (ulong) board.PvTable.numEntries);
            Debug.Assert(index >= 0 && index <= board.PvTable.numEntries - 1);

            board.PvTable.Entries[index].Move = move;
            board.PvTable.Entries[index].PosKey = board.PosKey;
        }

        /// <summary>
        /// Probes the parameter board for the PvMove is such exist. If
        /// no PvMove, it returns NO_MOVE.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public static int Probe(Board board) {
            var index = (int) (board.PosKey % (ulong) board.PvTable.numEntries);
            Debug.Assert(index >= 0 && index <= board.PvTable.numEntries - 1);

            if (board.PvTable.Entries[index].PosKey == board.PosKey) {
                return board.PvTable.Entries[index].Move;
            }

            return Variables.NO_MOVE;
        } 
    }
}