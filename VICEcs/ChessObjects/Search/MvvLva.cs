using System;
using VICEcs.ChessDefinitions;

namespace VICEcs.ChessObjects.Search {
    /// <summary>
    /// Small class that helps implement a MvvLva (Most valueable victim, least valuable attacker)
    /// search implementation.
    /// </summary>
    public static class MvvLva {
        /// <summary>
        /// holds the scores for capturing the indexed piece.
        /// </summary>
        private static readonly int[] VictimScores =
            {0, 100, 200, 300, 400, 500, 600, 100, 200, 300, 400, 500, 600};

        /// <summary>
        /// Indexed by MvvLvaScores[Victim, Attacker]. The higher the score,
        /// the more valuable the capture is.
        /// </summary>
        public static readonly int[,] Scores = new int[13, 13];

        /// <summary>
        /// Initializes the MvvLva array.
        /// </summary>
        public static void InitMvvLva() {
            for (int att /* attacker */ = (int) Piece.wP; att <= (int) Piece.bK; ++att) {
                for (int victim = (int) Piece.wP; victim <= (int) Piece.bK; ++victim) {
                    MvvLva.Scores[victim, att] =
                        MvvLva.VictimScores[victim] + 6 - (MvvLva.VictimScores[att] / 100);
                }
            }
        }

    }
}