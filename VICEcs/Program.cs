﻿using System;
 using System.Collections;
 using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Move;
using VICEcs.ChessObjects.Search;
 using VICEcs.GUI;
 using VICEcs.Testing;
using VICEcs.Tools;

namespace VICEcs {
    internal class Program {
        public static void Main(string[] args) {
            Initializer.AllInit();
            /*
            UCI uci = new UCI();
            uci.MainLoop();
            
            */
            
            Board board = new Board();
            BoardOperations.ParseFen("rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR b KQkq - 0 1", board);
            BoardOperations.PrintBoard(board);
            Console.WriteLine("Position evaluated to {0}", Evaluate.Position(board));
            BoardOperations.MirrorBoard(board);
            BoardOperations.PrintBoard(board);
            Console.WriteLine("Position evaluated to {0}", Evaluate.Position(board));
            /*
            Console.WriteLine("Square H4 int {0}", Conversion.getSq120ToSq64((int) Square.H4));
            for (int sq = 0; sq < 64; ++sq) {
                BitBoard.SetBit(ref number, sq);
                Console.WriteLine("I just set sq {0}",
                    Io.SqToString(Conversion.getSq64ToSq120((int) sq)));
                BitBoard.PrintBitBoard(number);
            }
            
            BitBoard.PrintBitBoard(number);
            */
        }
    }
}