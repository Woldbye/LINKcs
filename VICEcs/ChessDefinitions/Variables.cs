using System.Collections;
using System.Diagnostics;

namespace VICEcs.ChessDefinitions {
    public static class Variables {
        public const string PROGRAM_NAME = "Link v1.0";

        public const int BRD_SQ_NUM = 120;

        // half-moves.
        public const int MAX_GAME_MOVES = 2048;
        
        /// <summary>
        /// Maximum number of moves expected for a given position.
        /// </summary>
        public const int MAX_POSITION_MOVES = 256;
        
        /// <summary>
        /// MAXIMUM depth the program will try to search to.
        /// </summary>
        public const int MAX_DEPTH = 64;
        
        /// <summary>
        /// Holds information regarding the build of the current solution.
        /// If in debugging build, variable would be set to true from the Initializer.
        /// </summary>
        public static bool IsDebug = false;
        
        public const int NO_MOVE = 0;

        /// <summary>
        /// Stopwatch to help keep track of time during the program.
        /// The watch gets started by the initializer at the start of the game.
        /// </summary>
        public static Stopwatch Watch = new Stopwatch();
    }
}