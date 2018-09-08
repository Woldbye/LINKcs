using System.Text.RegularExpressions;
using VICEcs.ChessDefinitions;

namespace VICEcs.ChessObjects.Move {
    /// <summary>
    /// Game MoveOperations explaination:
    /// Lowest sq nr: 21, highest sq nr: 98.
    /// Need 7 bits, because 7 bits is the lowest bit to represent 98, the from and to fill 7 bits each.
    /// Promoted Piece and capture is 4 bits, cause u need 4 bits to represent the Pieces enumerator.
    ///     represented as hexadecimal, because each block can be represented by one hexa digit.
    ///     1111 binary equals F, 1111 1110 equals FE etc.
    /// 
    /// 0000 0000 0000 0000 0000 0111 1111 -> From
    /// 0000 0000 0000 0011 1111 1000 0000 -> To: shift right by 7 bit: >> 7, 0x7F
    /// 0000 0000 0011 1100 0000 0000 0000 -> Capture >> 14, 0xF
    /// 0000 0000 0100 0000 0000 0000 0000 -> EnPassant? 0x40000
    /// 0000 0000 1000 0000 0000 0000 0000 -> Pawn Start 0x80000
    /// 0000 1111 0000 0000 0000 0000 0000 -> Promoted Piece >> 20, 0xF
    /// 0001 0000 0000 0000 0000 0000 0000 -> Castle 0x1000000
    /// 
    /// </summary>
    public static class MoveOperations {
        /// <summary>
        /// En Passant MoveFlag
        /// </summary>
        public static int MoveFlagEnPas = 0x40000;
        /// <summary>
        /// Pawn Start MoveFlag
        /// </summary>
        public static int MoveFlagPawnStart = 0x80000;
        /// <summary>
        /// Castling MoveFlag
        /// </summary>
        public static int MoveFlagCastle = 0x1000000;
        /// <summary>
        /// Capture moveflag
        /// </summary>
        public static int MoveFlagCapture = 0x7C000;
        /// <summary>
        /// Promoted moveflag.
        /// </summary>
        public static int MoveFlagPromoted = 0xF00000;
        
        public static int FromSq(int move) {
            return move & 0x7F;
        }

        public static int ToSq(int move) {
            return (move >> 7) & 0x7F;
        }

        public static int Captured(int move) {
            return (move >> 14) & 0xF;
        }

        public static int Promoted(int move) {
            return (move >> 20) & 0xF;
        }
        
        public static int CreateMove(int from, int to, int capt, int promoted, int fl) {
            return (from | (to << 7) | (capt << 14) | (promoted << 20) | fl);
        }
    }
}