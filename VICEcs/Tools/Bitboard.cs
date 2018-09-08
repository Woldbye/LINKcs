using System;
using System.Collections;
using VICEcs.ChessDefinitions;

namespace VICEcs.Tools {
    public static class BitBoard {
        // Magic number used to pop bit from ulong
        private const ulong MAGIC_NUMBER = 0x37E84A99DAE458F; 
        
        // De Brujin sequence used to pop bit from ulong.
        private static readonly int[] MAGIC_TABLE =
        {
            0, 1, 17, 2, 18, 50, 3, 57,
            47, 19, 22, 51, 29, 4, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 5, 43, 34, 59, 8,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 7,
            62, 55, 45, 31, 13, 39, 36, 6,
            61, 44, 12, 35, 60, 11, 10, 9,
        };
        
        /// <summary>
        /// Forward scans an ulong to locate the first set bit.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static int BitScanForward(ulong val) {
            return BitBoard.MAGIC_TABLE[((ulong) ((long) val & -(long) val)*BitBoard.MAGIC_NUMBER) >> 58];
        }
        
        /// <summary>
        /// Pops the least significant bit in the parameter ulong
        /// </summary>
        /// <param name="bitBoard"></param>
        /// <returns></returns>
        public static int PopBit(ref ulong bitBoard) {
            int highestSetIndex = BitScanForward(bitBoard);
            BitBoard.ClearBit(ref bitBoard, highestSetIndex);
            return highestSetIndex;
        }

        /// <summary>
        /// Sets the indexed bit.
        /// </summary>
        /// <param name="bitBoard"></param>
        /// <param name="index"></param>
        public static void SetBit(ref ulong bitBoard, int index) {
            ulong mask = (1UL << index);
            bitBoard |= mask;
        }

        /// <summary>
        /// Clears the indexed bit.
        /// </summary>
        /// <param name="bitBoard"></param>
        /// <param name="index"></param>
        public static void ClearBit(ref ulong bitBoard, int index) {
            ulong mask = (1UL << index);
            bitBoard &= ~mask;
        }
        
        /// <summary>
        /// Counts the amount of set bit in the parameter unsigned long.
        /// </summary>
        /// <param name="value"> An unsigned long </param>
        /// <returns> The amount of set bit </returns>
        public static int CountBits(ulong value) {
            var count = 0;
            while (value != 0) {
                count++;
                value &= value - 1;
            }

            return count;
        }

        public static int CountBitArray(BitArray arr) {
            int rt_counter = 0;

            foreach (bool bit in arr) {
                // If bit set
                if (bit) {
                    rt_counter++;
                }
            }

            return rt_counter;
        }
        

        public static void PrintBitBoard(ulong bitBoard) {
            var shiftMe = 1UL;
            var sq64 = 0;

            Console.Write("\n");
            Console.WriteLine("  A B C D E F G H");
            for (var rank = Rank.RANK_8; rank >= Rank.RANK_1; --rank) {
                Console.Write("{0} ", (int) rank + 1);
                for (var file = File.FILE_A; file <= File.FILE_H; ++file) {
                    var sq = Conversion.FR2SQ((int) file, (int) rank); // 120 based.
                    sq64 = Conversion.getSq120ToSq64((int) sq); // 64 based.

                    if (((shiftMe << sq64) & bitBoard) != 0) {
                        Console.Write("X ");
                    } else {
                        Console.Write("- ");
                    }
                }

                Console.Write("\n");
            }

            Console.Write("\n");
        }

        public static void PrintBitArray(BitArray board) {
            for (int i = 0; i < board.Count; ++i)
            {
                bool bit = board.Get(i);
                Console.Write(bit ? 1 : 0);
                if (((i+1) % 8) == 0) {
                    Console.WriteLine();    
                }
            }
            Console.WriteLine();
        }
    }
}