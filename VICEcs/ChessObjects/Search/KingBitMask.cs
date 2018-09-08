using System;
using VICEcs.ChessDefinitions;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Search {
    public static class KingBitMask {
        // 0 is white king side castle
        // 1 is white queen side castle
        // 2 is black king side castle
        // 3 is black queen side castle
        private static ulong[] protectionMask = new ulong[4];

        public static void Init() {
            /*
             * White king side
             */
            BitBoard.SetBit(ref KingBitMask.protectionMask[0], Conversion.getSq120ToSq64((int) Square.H2));
            BitBoard.SetBit(ref KingBitMask.protectionMask[0], Conversion.getSq120ToSq64((int) Square.H3));
            BitBoard.SetBit(ref KingBitMask.protectionMask[0], Conversion.getSq120ToSq64((int) Square.G2));
            BitBoard.SetBit(ref KingBitMask.protectionMask[0], Conversion.getSq120ToSq64((int) Square.G3));
            BitBoard.SetBit(ref KingBitMask.protectionMask[0], Conversion.getSq120ToSq64((int) Square.F2));
            BitBoard.SetBit(ref KingBitMask.protectionMask[0], Conversion.getSq120ToSq64((int) Square.F3));
            
            Console.WriteLine("White king side:");
            BitBoard.PrintBitBoard(KingBitMask.protectionMask[0]);
            /*
             * White queen side
             */
            BitBoard.SetBit(ref KingBitMask.protectionMask[1], Conversion.getSq120ToSq64((int) Square.B2));
            BitBoard.SetBit(ref KingBitMask.protectionMask[1], Conversion.getSq120ToSq64((int) Square.B3));
            BitBoard.SetBit(ref KingBitMask.protectionMask[1], Conversion.getSq120ToSq64((int) Square.C2));
            BitBoard.SetBit(ref KingBitMask.protectionMask[1], Conversion.getSq120ToSq64((int) Square.C3));
            BitBoard.SetBit(ref KingBitMask.protectionMask[1], Conversion.getSq120ToSq64((int) Square.D2));
            BitBoard.SetBit(ref KingBitMask.protectionMask[1], Conversion.getSq120ToSq64((int) Square.D3));

            Console.WriteLine("White queen side:");
            BitBoard.PrintBitBoard(KingBitMask.protectionMask[1]);
            /*
             * Black king side
             */
            BitBoard.SetBit(ref KingBitMask.protectionMask[2], Conversion.getSq120ToSq64((int) Square.H6));
            BitBoard.SetBit(ref KingBitMask.protectionMask[2], Conversion.getSq120ToSq64((int) Square.H7));
            BitBoard.SetBit(ref KingBitMask.protectionMask[2], Conversion.getSq120ToSq64((int) Square.G6));
            BitBoard.SetBit(ref KingBitMask.protectionMask[2], Conversion.getSq120ToSq64((int) Square.G7));
            BitBoard.SetBit(ref KingBitMask.protectionMask[2], Conversion.getSq120ToSq64((int) Square.F6));
            BitBoard.SetBit(ref KingBitMask.protectionMask[2], Conversion.getSq120ToSq64((int) Square.F7));
            
            Console.WriteLine("Black king side:");
            BitBoard.PrintBitBoard(KingBitMask.protectionMask[2]);
            /*
             * Black queen side
             */
            BitBoard.SetBit(ref KingBitMask.protectionMask[3], Conversion.getSq120ToSq64((int) Square.B6));
            BitBoard.SetBit(ref KingBitMask.protectionMask[3], Conversion.getSq120ToSq64((int) Square.B7));
            BitBoard.SetBit(ref KingBitMask.protectionMask[3], Conversion.getSq120ToSq64((int) Square.C6));
            BitBoard.SetBit(ref KingBitMask.protectionMask[3], Conversion.getSq120ToSq64((int) Square.C7));
            BitBoard.SetBit(ref KingBitMask.protectionMask[3], Conversion.getSq120ToSq64((int) Square.D6));
            BitBoard.SetBit(ref KingBitMask.protectionMask[3], Conversion.getSq120ToSq64((int) Square.D7));
            Console.WriteLine("Black queen side:");
            BitBoard.PrintBitBoard(KingBitMask.protectionMask[3]);
        }
        
        /// <summary>
        /// Counts the amount of bits set in an area in relation to the king castling.
        /// Returns -1 if the area is invalid.
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public static int CountBitsSetInArea(ulong number, int square) {
            var count = 0;
            
            switch (square) {
            case (int) Square.G1:
                for (Square sq = Square.F2; sq <= Square.H2; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.F3; sq <= Square.H3; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            case (int) Square.C1:
                for (Square sq = Square.B2; sq <= Square.D2; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.B3; sq <= Square.D3; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            case (int) Square.G8:
                for (Square sq = Square.F6; sq <= Square.H6; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.F7; sq <= Square.H7; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            case (int) Square.C8:
                for (Square sq = Square.B6; sq <= Square.D6; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.B7; sq <= Square.D7; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((number & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            default:
                return -1;
            }
        }
        
        /// <summary>
        /// Receieves one of the four squares corresponding with the square of the king after
        /// castling. It then returns the mask to check whether that king is protected. 
        /// </summary>
        /// <param name="square"> One of the four squares corresponding with the king position after castling </param>
        /// <returns> A mask to check whether the king is protected </returns>
        public static ulong Get(int square) {
            switch (square) {
                case (int) Square.G1:
                    return KingBitMask.protectionMask[0];
                case (int) Square.C1:
                    return KingBitMask.protectionMask[1];
                case (int) Square.G8:
                    return KingBitMask.protectionMask[2];
                case (int) Square.C8:
                    return KingBitMask.protectionMask[3];
                default:
                    return 0UL;
            }
        }
    }
}