using System;
using System.Net;
using System.Security.Policy;
using VICEcs.ChessDefinitions;

namespace VICEcs.Tools {
    public static class Conversion {
        /// holds the equivalent index on a 64-index board.
        private static int[] Sq120ToSq64 = new int[Variables.BRD_SQ_NUM];

        /// holds the equivalent index on a 120-index board.
        private static int[] Sq64ToSq120 = new int[64];
        
        // Holds the associated file for a given 120-index sq.
        private static int[] FilesBrd = new int[Variables.BRD_SQ_NUM];
        
        // Holds the associated rank for a given 120-index sq.
        private static int[] RanksBrd = new int [Variables.BRD_SQ_NUM];
        
        public static int FR2SQ(int file, int rank) {
            return (21 + file + rank * 10);
        }

        public static int getRanksBrd(int index) {
            return Conversion.RanksBrd[index];
        }

        public static int getFilesBrd(int index) {
            return Conversion.FilesBrd[index];
        }
        
        public static int getSq120ToSq64(int index) {
            return Conversion.Sq120ToSq64[index];
        }

        public static int getSq64ToSq120(int index) {
            return Conversion.Sq64ToSq120[index];
        }

        /// initializes Sq120To64 and Sq64To120.
        public static void InitSq120To64() {
            var index = 0;
            var sq64 = 0;
            /// set all values in Sq120ToSq64 to 65.
            for (index = 0; index < Variables.BRD_SQ_NUM; ++index) {
                Conversion.Sq120ToSq64[index] = 65;
            }

            /// set all values in Sq64ToSq120 to 120.
            for (index = 0; index < 64; ++index) {
                Conversion.Sq64ToSq120[index] = 120;
            }

            for (var rank = Rank.RANK_1; rank <= Rank.RANK_8; ++rank) {
                for (var file = File.FILE_A; file <= File.FILE_H; ++file) {
                    var sq = Conversion.FR2SQ((int) file, (int) rank);
                    Conversion.Sq64ToSq120[sq64] = sq;
                    Conversion.Sq120ToSq64[sq] = sq64;
                    sq64++;
                }
            }
        }

        /// initializes FilesBrd and RanksBrd
        public static void InitFilesBrdAndRanksBrd() {
            for (int index = 0; index < Variables.BRD_SQ_NUM; ++index) {
                Conversion.FilesBrd[index] = (int) Square.NO_SQ;
                Conversion.RanksBrd[index] = (int) Square.NO_SQ;
            }

            for (int rank = (int) Rank.RANK_1; rank <= (int) Rank.RANK_8; ++rank) {
                for (int file = (int) File.FILE_A; file <= (int) File.FILE_H; ++file) {
                    var sq = (int) Conversion.FR2SQ(file, rank);
                    Conversion.FilesBrd[sq] = file;
                    Conversion.RanksBrd[sq] = rank;
                }
            }
        }
        
        public static string Number2String(int number, bool isCaps) {
            var c = (char) ((isCaps ? 65 : 97) + (number - 1));

            return c.ToString();
        }
    }
}