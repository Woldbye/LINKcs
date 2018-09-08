using VICEcs.ChessDefinitions;

namespace VICEcs.ChessObjects.Move {
    public class MoveList {
        public S_Move[] Moves = new S_Move[Variables.MAX_POSITION_MOVES];
        public int Count;
    }
}