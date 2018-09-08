using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using VICEcs;
using VICEcs.Testing;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Move;
using File = System.IO.File;

namespace VICEcs_Test {
    /// <summary>
    /// This class test possible move combinations,
    /// by performing a recursive Perft test.
    /// </summary>
    [TestFixture]
    public class PerftTests {
        [SetUp]
        public void SetUp() {
            Initializer.AllInit();
            var dir = new DirectoryInfo(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location));
            while (dir.Name != "bin") {
                dir = dir.Parent;
            }

            dir = dir.Parent;

            var path = Path.Combine(dir.FullName, "perftsuite.epd");
            
            var file = "";
            try {
                file = File.ReadAllText(path);
            } catch (Exception e) {
                throw new FileNotFoundException("Invalid file path");
            }

            perftSuite = file.Split(
                new[] {Environment.NewLine},
                StringSplitOptions.None
            );
            
            perft = new Perft();
            board = new Board();
            moveList = new MoveList();
        }

        private string[] perftSuite;
        private Perft perft;
        private Board board;
        private MoveList moveList;
        
        /// <summary>
        /// Tests up to the input depth. 
        /// </summary>
        /// <param name="k"> k restricted --> < 6 </param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void DepthK(int k) {
            for (int i = 0; i < perftSuite.Length - 1; ++i) {
                string line = perftSuite[i];
                string fen = "";
                int index = 0;

                // Locate the FEN
                for (int j = 0; j < line.Length; j++) {
                    if (line[j] == ';') {
                        fen = line.Substring(0, j - 1);
                        index = j;
                        break;
                    }
                }

                for (int j = 1; j < k; j++) {
                    FindNextSemiColon(ref index, line);
                    index++;
                }
                
                FindNextSemiColon(ref index, line);

                index += 4;
                int depthInt = FindExpectedDepthVal(index, line);

                BoardOperations.ParseFen(fen, board);
                MoveGen.GenerateAllMoves(board, moveList, false);

                perft.Perft_Test(k, board);
                Assert.AreEqual(perft.LeafNodes, depthInt);
            }
        }
        
        private int FindExpectedDepthVal(int index, string line) {
            StringBuilder sb = new StringBuilder();
            var t_index = index;
            while (true) {
                if (line[t_index+1] == ';') {
                    break;
                }

                sb.Append(line[t_index]);
                t_index++;
            }
            
            return Int32.Parse(sb.ToString());
        }

        private void FindNextSemiColon(ref int index, string line) {
            while (true) {
                if (line[index] == ';') {
                    break;
                }

                index++;
            }
        }
    }
}