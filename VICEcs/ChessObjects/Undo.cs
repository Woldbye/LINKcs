namespace VICEcs.ChessObjects {
    public class Undo {
        public int Move;
        public int CastlePerm;
        public int FiftyMoves;
        public int EnPas;
        public ulong PosKey;
        
        public Undo() {
            // 99 = NO_SQ.
            PosKey = 0;
            EnPas = 99;
            Move = -1;
            CastlePerm = -1;
            FiftyMoves = -1;
        }
    }
}