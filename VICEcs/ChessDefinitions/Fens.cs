namespace VICEcs.ChessDefinitions {
    /// <summary>
    /// Holds various important FEN position, used for testing or other operations.
    /// </summary>
    public static class Fens {
        // Start FEN position of the board.
        public const string START_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public const string MATERIAL_DRAW_FEN = "8/6R1/2k5/6P1/8/8/4nP2/6K1 w - - 1 41";
        
        // For testing the evaluation method.
        public const string EVALUATION_FEN = "2k1r2r/Bpq3pp/3b4/3Bp3/8/7b/PPP1QP2/R3R1K1 w - - 1 1";
        
        // For testing knights
        public const string KNIGHTS_TEST = "5k2/1n6/4n3/6N1/8/3N4/8/5K2 w - - 0 1";
        
        // For testing white pawns
        public const string WHITE_PAWNS_TEST =
            "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1";
        
        // For testing black pawns
        public const string BLACK_PAWNS_TEST =
            "rnbqkbnr/p1p1p3/3p3p/1p1p4/2P1Pp2/8/PP1P1PpP/RNBQKB1R b KQkq e3 0 1";
        
        // For testing rooks
        public const string ROOKS_TEST = "6k1/8/5r2/8/1nR5/5N2/8/6K1 b - - 0 1";
        
        // For testing queens
        public const string QUEENS_TEST = "6k1/8/4nq2/8/1nQ5/5N2/1N6/6K1 b - - 0 1 ";
        
        // For testing bishops
        public const string BISHOPS_TEST = "6k1/1b6/4n3/8/1n4B1/1B3N2/1N6/2b3K1 b - - 0 1 ";
        
        // For testing that the white castling correctly gets validated.
        public const string WHITE_CASTLING_TEST_1 = "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1";

        // For testing that the black castling correctly gets validated.
        public const string BLACK_CASTLING_TEST_1 = "r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1";
        
        // For testing that u cannot castle if fields are attacked etc.
        public const string WHITE_CASTLING_TEST_2 = "3rk2r/8/8/8/8/8/6p1/R3K2R w KQk - 0 1";
        
        public const string BLACK_CASTLING_TEST_2 = "3rk2r/8/8/8/8/8/6p1/R3K2R b KQk - 0 1";

        // Expect 48 legal moves.
        public const string MOVELIST_TEST =
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

        public const string MATE_IN_3 = "2rr3k/pp3pp1/1nnqbN1p/3pN3/2pP4/2P3Q1/PPB4P/R4RK1 w - -";
    }
}