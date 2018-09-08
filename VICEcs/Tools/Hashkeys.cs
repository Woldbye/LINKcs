using System;
using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects;
using VICEcs.ChessObjects.Board;

namespace VICEcs.Tools {
    public static class Hashkeys {
        public static ulong[] CastleKeys = new ulong[16];
        public static ulong[,] PieceKeys = new ulong[13, 120];
        public static ulong SideKey;
        private static Random rand = new Random();

        private static ulong RAND_64() {
            return (ulong) Hashkeys.rand.Next() | 
                   ((ulong) Hashkeys.rand.Next() << 15) |
                   ((ulong) Hashkeys.rand.Next() << 30) |
                   ((ulong) Hashkeys.rand.Next() << 45) |
                   (((ulong) Hashkeys.rand.Next() & 0xf) << 60);
        }

        public static void InitHashKeys() {
            Hashkeys.SideKey = Hashkeys.RAND_64();

            for (var i = 0; i < 13; ++i) {
                for (var j = 0; j < 120; ++j) {
                    Hashkeys.PieceKeys[i, j] = Hashkeys.RAND_64();
                }
            }

            for (var i = 0; i < 16; i++) {
                Hashkeys.CastleKeys[i] = Hashkeys.RAND_64();
            }
        }

        public static ulong GeneratePosKey(Board posBoard) {
            var sq = 0;
            ulong finalKey = 0;
            var piece = (int) Piece.EMPTY;

            // pieces
            for (sq = 0; sq < Variables.BRD_SQ_NUM; ++sq) {
                piece = posBoard[sq];
                if (piece != (int) Piece.EMPTY && Validators.IsSq(sq)) {
                    Debug.Assert(piece >= (int) Piece.wP && piece <= (int) Piece.bK,
                        "error during GeneratePosKey Pieces, the piece u're accessing is not a valid piece.");
                    finalKey ^= Hashkeys.PieceKeys[piece, sq];
                }
            }

            // Color ID: 
            if (posBoard.Side == (int) Colour.WHITE) {
                finalKey ^= Hashkeys.SideKey;
            }

            // enpassange
            if (posBoard.EnPas != (int) Square.NO_SQ) {
                Debug.Assert(posBoard.EnPas >= 0 && posBoard.EnPas < Variables.BRD_SQ_NUM,
                    "error during GeneratePosKey EnPas.");
                finalKey ^= Hashkeys.PieceKeys[(int) Piece.EMPTY, posBoard.EnPas];
            }

            // Castle permission.
            Debug.Assert(posBoard.CastlePerm >= 0 && posBoard.CastlePerm <= 15,
                "error during GeneratePosKey CastlePerm");
            finalKey ^= Hashkeys.CastleKeys[posBoard.CastlePerm];

            return finalKey;
        }
    }
}