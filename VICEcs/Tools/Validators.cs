﻿using VICEcs.ChessDefinitions;

namespace VICEcs.Tools {
    public static class Validators {
        public static bool IsSq (int square) {
            switch (square) {
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                case 38:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                case 46:
                case 47:
                case 48:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                case 58:
                case 61:
                case 62:
                case 63:
                case 64:
                case 65:
                case 66:
                case 67:
                case 68:
                case 71:
                case 72:
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 78:
                case 81:
                case 82:
                case 83:
                case 84:
                case 85:
                case 86:
                case 87:
                case 88:
                case 91:
                case 92:
                case 93:
                case 94:
                case 95:
                case 96:
                case 97:
                case 98:
                    return true;
                default:
                    return false; 
            }
        }
        
        /// <summary>
        /// Validates whether the input sq is actually a valid 120 based square.
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static bool SqOnBoard(int sq) {
            return Validators.IsSq(sq);
        }

        /// <summary>
        /// Validates whether the input side, is actually a valid chess side (black or white).
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static bool SideValid(int side) {
            return (side == (int) Colour.BLACK || side == (int) Colour.WHITE);
        }

        /// <summary>
        /// Validates whether the input file or rank is valid.
        /// </summary>
        /// <param name="fileOrRank"></param>
        /// <returns></returns>
        public static bool FileOrRankValid(int fileOrRank) {
            return (fileOrRank >= 0 && fileOrRank <= 7);
        }

        /// <summary>
        /// Validates whether the input piece is a valid piece or empty.
        /// </summary>
        /// <param name="pce"></param>
        /// <returns></returns>
        public static bool PieceValidEmpty(int pce) {
            return (pce >= (int) Piece.EMPTY && pce <= (int) Piece.bK);
        }

        /// <summary>
        /// Validates whether the input piece is a valid piece.
        /// </summary>
        /// <param name="pce"></param>
        /// <returns></returns>
        public static bool PieceValid(int pce) {
            return (pce > (int) Piece.EMPTY && pce <= (int) Piece.bK);
        }
    }
}