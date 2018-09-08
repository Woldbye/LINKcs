using System.Runtime.InteropServices.ComTypes;

namespace VICEcs.ChessDefinitions {
    /// <summary>
    /// Auxiliary class for holding data regarding Piece validation.
    /// </summary>
    public static class Data {
        private static readonly bool[] IS_PIECE_KNIGHT = {
            false, false, true, false, false, false, false, false, true, false, false, false, false
        };

        private static readonly bool[] IS_PIECE_KING = {
            false, false, false, false, false, false, true, false, false, false, false, false, true
        };

        private static readonly bool[] IS_PIECE_ROOK_QUEEN = {
            false, false, false, false, true, true, false, false, false, false, true, true, false
        };

        private static readonly bool[] IS_PIECE_BISHOP_QUEEN = {
            false, false, false, true, false, true, false, false, false, true, false, true, false
        };

        private static readonly bool[] IS_PIECE_PAWN = {
            false, true, false, false, false, false, false, true, false, false, false, false, false
        };
        
        /// <summary>
        /// For black indexing
        /// </summary>
        public static readonly int[] mirror64Table = {
            56, 57, 58, 59, 60, 61, 62, 63,
            48, 49, 50, 51, 52, 53, 54, 55,
            40, 41, 42, 43, 44, 45, 46, 47,
            32, 33, 34, 35, 36, 37, 38, 39,
            24, 25, 26, 27, 28, 29, 30, 31,
            16, 17, 18, 19, 20, 21, 22, 23,
            8, 9, 10, 11, 12, 13, 14, 15,
            0, 1, 2, 3, 4, 5, 6, 7
        };
        
        /// <summary>
        /// Swaps the color of the indexed piece.
        /// swapPiece(Piece.wP) outputs Piece.bP.
        /// </summary>
        public static readonly Piece[] SWAP_PIECE = {
            Piece.EMPTY, Piece.bP, Piece.bN, Piece.bB, Piece.bR, Piece.bQ, Piece.bK,
            Piece.wP, Piece.wN, Piece.wB, Piece.wR, Piece.wQ, Piece.wK
        };

        /// <summary>
        /// PCE_CHAR holds the char representive of the indexed piece.
        /// </summary>
        /// <remarks> first index is '.', since (int) Piece.EMPTY = 0 </remarks>
        /// <example> PCE_CHAR[(int) Piece.wP] outputs 'P' </example>
        public static readonly char[] PCE_CHAR = {
            '.', 'P', 'N', 'B', 'R', 'Q', 'K', 'p', 'n',
            'b', 'r', 'q', 'k'
        };
        
        /// <summary>
        /// SIDE_CHAR holds the char representive of the indexed side.
        /// </summary>
        /// <example> PCE_CHAR[(int) Side.BLACK] outputs 'b' </example>
        public static readonly char[] SIDE_CHAR = {'w', 'b', '-'};
        
        /// <summary>
        /// RANK_CHAR holds the char represntive of the index rank.
        /// </summary>
        /// <example> RANK_CHAR[(int) Rank.RANK_1] outputs '1' </example>
        public static readonly char[] RANK_CHAR = {'1', '2', '3', '4', '5', '6', '7', '8'};

        /// <summary>
        /// FILE_CHAR holds the char represntive of the index File.
        /// </summary>
        public static readonly char[] FILE_CHAR = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'};

        /// <summary>
        /// Returns true if the index piece is a big piece.
        /// </summary>
        /// <remarks> Every piece except a pawn is considered a big piece </remarks>
        public static readonly bool[] IS_PIECE_BIG =
            {false, false, true, true, true, true, true, false, true, true, true, true, true};

        /// <summary>
        /// Returns true if the index piece is a major piece.
        /// </summary>
        /// <remarks> Rook, Queen and King is considered major pieces </remarks>
        public static readonly bool[] IS_PIECE_MAJ =
            {false, false, false, false, true, true, true, false, false, false, true, true, true};

        /// <summary>
        /// Returns true if the index piece is a minor piece.
        /// </summary>
        /// <remarks> Knights and bishops are considered minor pieces </remarks>
        public static readonly bool[] IS_PIECE_MIN =
            {false, false, true, true, false, false, false, false, true, true, false, false, false};

        /// <summary>
        /// Holds the integer value of each indexed piece.
        /// </summary>
        /// <example> PIECE_VAL[(int) Piece.wK] outputs 50000 </example>
        public static readonly int[] PIECE_VAL =
            {0, 100, 325, 325, 550, 1000, 50000, 100, 325, 325, 550, 1000, 50000};

        /// <summary>
        /// Returns the colour type of the indexed piece.
        /// </summary>
        public static readonly Colour[] PIECE_COLOURS = {
            Colour.BOTH, Colour.WHITE, Colour.WHITE, Colour.WHITE, Colour.WHITE, Colour.WHITE,
            Colour.WHITE, Colour.BLACK, Colour.BLACK, Colour.BLACK, Colour.BLACK, Colour.BLACK,
            Colour.BLACK
        };
        
        /// <summary>
        /// If the piece is a sliding piece (rook, queen etc)
        /// </summary>
        public static readonly bool[] PIECE_SLIDES = 
        { false, false, false, true, true, true, false, false, false, true, true, true, true };

        public static bool IsPiecePawn(int piece) {
            return Data.IS_PIECE_PAWN[piece];
        }
        
        public static bool IsPieceKnight(int piece) {
            return Data.IS_PIECE_KNIGHT[piece];
        }
        
        public static bool IsPieceKing(int piece) {
            return Data.IS_PIECE_KING[piece];
        }
        
        public static bool IsPieceRookQueen(int piece) {
            return Data.IS_PIECE_ROOK_QUEEN[piece];
        }
        
        public static bool IsPieceBishopQueen(int piece) {
            return Data.IS_PIECE_BISHOP_QUEEN[piece];
        }
    }
}