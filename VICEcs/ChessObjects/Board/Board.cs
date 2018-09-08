﻿using System.Collections;
using VICEcs.ChessDefinitions;

namespace VICEcs.ChessObjects.Board {
    public class Board {
        // Holds the values for all the materials for black and white respectively.
        public int[] Material;
        
        /*
        big pieces is every piece except pawns, indexed by enum color.
        etc. bigPieces[1] holds the amount of black big pieces on the board.
        */
        public int[] BigPieces;

        public Search.Search Searcher;
        
        // castling permission.
        public int CastlePerm;

        // enPassange square
        public int EnPas;

        // Fifty move counter.
        public int FiftyMoves;

        // hisPLy, how many half moves into the game.
        public int HistoryPly;

        // stores information about each move during the game.
        public Undo[] History;

        // king squares.
        public int[] KingSq;

        /*
        maj pieces is rooks and queens, indexed by enum color.
        etc. majPieces[1] holds the amount of black major pieces on the board.
        */
        public int[] MajPieces;

        /*
        Minor pieces is bishops and knights indexed by enum color.
        etc. minPieces[1] holds the amount of black big pieces on the board.
        */
        public int[] MinPieces;

        /*
        00000000 01000000 00000000 00000000 00000000 00000000 00000000 00000000
        represents pawn on square B2.
        */
        public ulong[] Pawns;
        
        /*
        the amount of each pieces on the board indexed by enum Piece.
        etc. pceNum[2] holds the amount of white knights on the board.
        */
        public int[] PceNum;
        
        private int[] pieces;

        // holds the position of every piece on the board.
        // pList[(int) wN][0] = E1;
        // pList[(int) wN][1] = D4; ... ...
        // emptySquares initialized to NO_SQ.
        public int[,] PList;

        // How many half moves into the current search.
        public int Ply;

        public ulong PosKey;

        // current side to move.
        public int Side;

        public PvTable PvTable;

        public int[] PvArray;

        /// <summary>
        /// Implements a HISTORY-HEURISTIC for the search.
        ///  In game trees, the same branch, or move, will occur many times at different
        ///  nodes, or positions. A history is maintained of how successful each move is in leading to
        ///  the highest minimax score at an interior node. This information is maintained for every different
        ///  move, regardless of the originating position. At interior nodes of the tree, moves are examined in
        ///  order of their prior history of success. In this manner, previous search information is accumulated
        ///  and distributed throughout the tree.
        /// </summary>
        public int[,] SearchHistory;

        /// <summary>
        /// Implements a KILLER-HEURISTIC for the search by
        /// storing the last two moves, which aren't capture moves,
        /// that has most recently caused a BETA-cutoff in the search.
        /// See: https://en.wikipedia.org/wiki/Killer_heuristic
        /// </summary>
        public int[,] SearchKillers;
        
        // Constructor.
        public Board() {
            Searcher = new Search.Search(this);
            pieces = new int[Variables.BRD_SQ_NUM];
            Pawns = new ulong[3];
            KingSq = new int[2];
            EnPas = 99; // NO_SQ
            Side = -1;
            FiftyMoves = -1;
            Ply = 0;
            HistoryPly = 0;
            PceNum = new int[13];
            Material = new int[2];
            BigPieces = new int[2];
            MajPieces = new int[2];
            MinPieces = new int[2];
            CastlePerm = -1;
            PList = new int[13, 10];
            SearchHistory = new int[13, Variables.BRD_SQ_NUM];
            SearchKillers = new int[2, Variables.MAX_DEPTH];
            HistoryInit();
            PvTable = new PvTable(20000);
            PvArray = new int[Variables.MAX_DEPTH];
        }

        private void HistoryInit() {
            History = new Undo[Variables.MAX_GAME_MOVES];
            for (int i = 0; i < Variables.MAX_GAME_MOVES; ++i) {
                History[i] = new Undo();
            }
        }

        // getter and setter for a piece on the board.
        public int this[int index] {
            get => pieces[index];
            set => pieces[index] = value;
        }        
    }
    
}