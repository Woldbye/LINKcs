using System;
using VICEcs.ChessDefinitions;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Search {
    public static class KingSafety {
        /// <summary>
        /// Counts the amount of bits set in an area in relation to the king castling.
        /// Returns -1 if the area is invalid.
        /// </summary>
        /// <returns></returns>
        public static int CountBitsSetInArea(ulong bitBoard, int square) {
            var count = 0;
            
            switch (square) {
            case (int) Square.G1:
                for (Square sq = Square.F2; sq <= Square.H2; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.F3; sq <= Square.H3; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            case (int) Square.C1:
                for (Square sq = Square.B2; sq <= Square.D2; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.B3; sq <= Square.D3; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            case (int) Square.G8:
                for (Square sq = Square.F6; sq <= Square.H6; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.F7; sq <= Square.H7; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            case (int) Square.C8:
                for (Square sq = Square.B6; sq <= Square.D6; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }
                
                for (Square sq = Square.B7; sq <= Square.D7; ++sq) {
                    int sq64 = Conversion.getSq120ToSq64((int) sq);
                    if ((bitBoard & (1UL << sq64)) != 0) {
                        count++;
                    }
                }

                return count;
            default:
                return -1;
            }
        }
    }
}