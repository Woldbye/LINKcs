using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Move;
using VICEcs.Tools;

namespace VICEcs.Testing {
    public class Perft {
        public long LeafNodes { get; private set; }

        /// <summary>
        ///     Counts the amount of leafNode at the parameter input depth,
        ///     by an inorder recursive tree traversion algorithm. 
        ///     It checks all legal moves with the parameter input depth,
        ///     and increments the leafNode counter when the "last" move
        ///     (the leaf) has been located.
        /// </summary>
        /// <param name="depth"> The depth to traverse </param>
        /// <param name="board"> The chess board operate on </param>
        /// <exception cref="Exception"></exception>
        public void _Perft(int depth, Board board) { // For counting the TOTAL available moves
            Debug.Assert(BoardOperations.CheckBoard(board));
            // Increment leafNode and return.
            if (depth == 0) {
                LeafNodes++;
                return;
            }
            
            // Generate moves for rootposition
            MoveList list = new MoveList();
            MoveGen.GenerateAllMoves(board, list, false);
            
            for (int i = 0; i < list.Count; ++i) {
                if (!MakeMove.Make_Move(board, list.Moves[i].Move)) {
                    continue;
                }

                _Perft(depth-1, board);
                MakeMove.TakeMove(board);
            }
        }

        public void Perft_Test(int depth, Board board) { // Also prints information regarding the moves
            Debug.Assert(BoardOperations.CheckBoard(board));
            var startTime = Variables.Watch.ElapsedMilliseconds;
            
            Console.Write("\nStarting Perft Test to Depth {0}", depth);
            BoardOperations.PrintBoard(board);

            LeafNodes = 0;
            
            MoveList list = new MoveList();
            MoveGen.GenerateAllMoves(board, list, false);


            for (int i = 0; i < list.Count; ++i) {
                int move = list.Moves[i].Move;

                if (!MakeMove.Make_Move(board, list.Moves[i].Move)) {
                    continue;
                }

                long cumulativeNodes = LeafNodes;
                _Perft(depth - 1, board);
                MakeMove.TakeMove(board);
                long oldNodes = LeafNodes - cumulativeNodes;
                Console.Write("\nmove {0} : {1} : {2}", i + 1, Io.MoveToString(move), oldNodes);
            }
            
            Console.Write("\nTest Complete: {0} nodes visited in {1} miliseconds\n", LeafNodes, Variables.Watch.ElapsedMilliseconds - startTime);
        }
    }
}