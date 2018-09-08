using System;
using System.Collections;
using System.Reflection.Emit;
using VICEcs.ChessDefinitions;
using VICEcs.Tools;

namespace VICEcs.ChessObjects.Search {
    /// <summary>
    ///     Small class for holding and initializing pawn evaluation bitmasks,
    ///     used to locate "past pawns" and "isolated pawns".
    /// </summary>
    public static class EvalBitMask {
        private static ulong[] filesMask = new ulong[8];
        private static ulong[] ranksMask = new ulong[8];
        private static ulong[] blackPassedMask = new ulong[64];
        private static ulong[] whitePassedMask = new ulong[64];
        private static ulong[] isolatedMask = new ulong[64];
        
        public static void InitBitMask() {
            var index = 0;

            for (index = 0; index < 8; ++index) {
                EvalBitMask.filesMask[index] = 0UL;
                EvalBitMask.ranksMask[index] = 0UL;
            }
            
            // Fills out filesMask and ranksMask.
            for (index = 0; index < 64; ++index) {
                var sq120 = Conversion.getSq64ToSq120(index);
                var file = Conversion.getFilesBrd(sq120);
                var rank = Conversion.getRanksBrd(sq120);
                BitBoard.SetBit(ref EvalBitMask.filesMask[file], index);
                BitBoard.SetBit(ref EvalBitMask.ranksMask[rank], index);
                
                // Also initialize blackPassed, whitePassed and isolated.
                EvalBitMask.blackPassedMask[index] = 0UL;
                EvalBitMask.whitePassedMask[index] = 0UL;
                EvalBitMask.isolatedMask[index] = 0UL;
            }

            for (index = 0; index < 64; ++index) {
                var sq120 = Conversion.getSq64ToSq120(index);
                var file = Conversion.getFilesBrd(sq120);
                var rank = Conversion.getRanksBrd(sq120);

                // Fills out passed masks
                for (var j = 0; j < 64; ++j) {
                    var t_sq120 = Conversion.getSq64ToSq120(j);
                    var t_file = Conversion.getFilesBrd(t_sq120);
                    var t_rank = Conversion.getRanksBrd(t_sq120);
                    // True if the file of the sq is in range of 1 of original file.
                    var fileClose = Math.Abs(t_file - file) <= 1;
                    if (t_rank > rank && fileClose) {
                        BitBoard.SetBit(ref EvalBitMask.whitePassedMask[index], j);
                    }

                    if (t_rank < rank && fileClose) {
                        BitBoard.SetBit(ref EvalBitMask.blackPassedMask[index], j);
                    }
                }
                

                // Fills out isolatedMask.
                if (file == (int) File.FILE_A) {
                    EvalBitMask.isolatedMask[index] |= EvalBitMask.filesMask[file + 1];
                } else if (file == (int) File.FILE_H) {
                    EvalBitMask.isolatedMask[index] |= EvalBitMask.filesMask[file - 1];
                } else {
                    EvalBitMask.isolatedMask[index] |= EvalBitMask.filesMask[file + 1];
                    EvalBitMask.isolatedMask[index] |= EvalBitMask.filesMask[file - 1];
                }
            }
        }

        public static ulong GetWhitePassedMask(int index) {
            return EvalBitMask.whitePassedMask[index];
        }

        public static ulong GetBlackPassedMask(int index) {
            return EvalBitMask.blackPassedMask[index];
        }

        public static ulong GetIsolatedMask(int index) {
            return EvalBitMask.isolatedMask[index];
        }

        /// <summary>
        ///     Getter for the filesMask array.
        ///     Indexed such that GetFilesMask(0)
        ///     will get the mask for file A.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ulong GetFilesMask(int index) {
            return EvalBitMask.filesMask[index];
        }


        /// <summary>
        ///     Getter for the filesMask array.
        ///     Indexed such that GetRanksMask(0)
        ///     will get the mask for rank 1.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ulong GetRanksMask(int index) {
            return EvalBitMask.ranksMask[index];
        }
    }
}