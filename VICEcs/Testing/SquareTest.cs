using System;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects;
using VICEcs.ChessObjects.Board;
using VICEcs.Tools;

namespace VICEcs.Testing {
    public static class SquareTest {
        /// <summary>
        /// Prints a board representing all the squares attacked by the input side.
        /// If a square is attacked, it's marked by a 'X', if not '-'.
        /// </summary>
        /// <param name="side"> The attacking side </param>
        /// <param name="board"> The chess board </param>
        public static void ShowSqAttackBySide(int side, Board board) {
            int rank = 0;
            int file = 0;
            int sq = 0;

            Console.Write("\nSquares Attacked by {0}\n", Data.SIDE_CHAR[side]);
            for (rank = (int) Rank.RANK_8; rank >= (int) Rank.RANK_1; --rank) {
                for (file = (int) File.FILE_A; file <= (int) File.FILE_H; ++file) {
                    sq = (int) Conversion.FR2SQ(file, rank);
                    if (Attack.IsSqAttacked(sq, side, board)) {
                        Console.Write("X");
                    } else {
                        Console.Write("-");
                    }
                }
                Console.Write("\n");
            }
            
            Console.Write("\n\n");
        }
    }
}