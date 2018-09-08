using System;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Board;
using VICEcs.ChessObjects.Move;
using VICEcs.ChessObjects.Search;
using VICEcs.Tools;

namespace VICEcs.GUI {
    /// <summary>
    /// Universal Chess Interface.
    /// This class enables communication with a GUI
    ///  based on the UCI-protocol http://wbec-ridderkerk.nl/html/UCIProtocol.html.
    /// </summary>
    public class UCI {
        /// <summary>
        /// Max input size.
        /// </summary>
        public const int INPUT_BUFFER = 400 * 6;
        
        public void MainLoop() {
            PrintStartUpInfo();

            Board board = new Board();
            S_SearchInfo info = new S_SearchInfo();

            // Infinite loop, will only break once user quits.
            while (true) {
                string
                    stdin = null; // https://daveaglick.com/posts/capturing-standard-input-in-csharp
                stdin = Console.ReadLine();

                if (stdin == null) {
                    continue;
                }
                
                // First check length to avoid OutOfRangeException.
                if (stdin.Length >= 7 && stdin.Substring(0, 7).Equals("isready")) {
                    Console.Write("readyok\n");
                    continue;
                } else if (stdin.Length >= 8 && stdin.Substring(0, 8).Equals("position")) {
                    ParsePosition(stdin, board);
                } else if (stdin.Length >= 10 && stdin.Substring(0, 10).Equals("ucinewgame")) {
                    ParsePosition("position startpos\n", board);
                } else if (stdin.Length >= 2 && stdin.Substring(0, 2).Equals("go")) {
                    ParseGo(stdin, board, ref info);
                } else if (stdin.Length >= 4 && stdin.Substring(0, 4).Equals("quit")) {
                    info.Quit = true;
                    break;
                } else if (stdin.Length >= 3 && stdin.Substring(0, 3).Equals("uci")) {
                    PrintStartUpInfo();
                } else if (stdin.Length >= 4 && stdin.Substring(0, 4).Equals("stop")) {
                    info.Stopped = true;
                }

                if (info.Quit) {
                    break;
                }
            }
        }
        
        // should read "position fen"
        //             "position startpos"
        // which could possibly be followed by  "... moves a2a3 a3a4" etc.
        public void ParsePosition(string lineIn, Board board) {
            int stringIndex = 9; // starts at 9 cause position is length 8
            int moveIndex = lineIn.IndexOf("moves"); // is there any moves to consider?
            
            if (lineIn.Length >= stringIndex + 8 &&
                lineIn.Substring(stringIndex, 8).Equals("startpos")) {
                BoardOperations.ParseFen(Fens.START_FEN, board);
            } else { // Else "position fen fenstring"
                if (lineIn.Length >= stringIndex + 3 &&
                    lineIn.Substring(stringIndex, 3).Equals("fen")) {
                    stringIndex += 4; // should be at start of fenstring now.
                    if (moveIndex == -1) {
                        BoardOperations.ParseFen(lineIn.Substring(stringIndex), board);                        
                    } else {
                        BoardOperations.ParseFen(
                            lineIn.Substring(stringIndex, moveIndex - stringIndex - 1), board);
                    }
                } else {
                    BoardOperations.ParseFen(Fens.START_FEN, board);
                }
            }
            
            if (moveIndex != -1) { // moves were found.
                stringIndex = moveIndex + 6; // We are now at start of command moves. moves |a2a3
                int move = Variables.NO_MOVE;
                
                while (stringIndex <= lineIn.Length - 1) {
                    stringIndex += 5;
                    
                    if (lineIn.Length > stringIndex && lineIn[stringIndex - 1] == ' ') {
                        move = Io.ParseMove(board, lineIn.Substring(stringIndex - 5, 4).ToCharArray()); // not a promotion move.
                    } else if (stringIndex == lineIn.Length + 1) { // if at last move
                        move = Io.ParseMove(board,
                            lineIn.Substring(stringIndex - 5, 4).ToCharArray());
                    } else if (stringIndex == lineIn.Length) { // promotion move at end of line
                        move = Io.ParseMove(board,
                            lineIn.Substring(stringIndex - 5, 5).ToCharArray());
                    } else if (lineIn.Length >= stringIndex && lineIn[stringIndex - 1] != ' ') {
                        move = Io.ParseMove(board,
                            lineIn.Substring(stringIndex - 5, 5).ToCharArray());
                        stringIndex++;
                    }

                    if (move == Variables.NO_MOVE) {
                        break;
                    }
                    
                    MakeMove.Make_Move(board, move);
                    board.Ply = 0;
                }
            }
            
            BoardOperations.PrintBoard(board);
        }

        // should read "go depth int wtime int btime int binc int winc int movetime int movestogo int
        // etc:
        // "go depth 8 wtime 10000 btime 8000 binc 1000 winc 1000 movetime 1000 movestogo 50"
        // Tactic: 
        // Find each variable by String.IndexOf method, and process them.
        public void ParseGo(string lineIn, Board board, ref S_SearchInfo info) {
            info.isTimeSet = false;

            int depth = -1, movesToGo = 30, moveTime = -1, time = -1, inc = 0, indexOf = -1;
            
            // If binc variable exists.
            if (((indexOf = lineIn.IndexOf("binc")) != -1) && board.Side == (int) Colour.BLACK) {
                indexOf += 5;
                var endIndex = EndOfInteger(lineIn, indexOf);
                inc = Int32.Parse(lineIn.Substring(indexOf, endIndex - indexOf + 1));
            }
            
            // If winc variable exists.
            if (((indexOf = lineIn.IndexOf("winc")) != -1) && board.Side == (int) Colour.WHITE) {
                indexOf += 5;
                var endIndex = EndOfInteger(lineIn, indexOf);
                inc = Int32.Parse(lineIn.Substring(indexOf, endIndex - indexOf + 1));
            }
            
            if (((indexOf = lineIn.IndexOf("btime")) != -1) && board.Side == (int) Colour.BLACK) {
                indexOf += 6;
                var endIndex = EndOfInteger(lineIn, indexOf);
                time = Int32.Parse(lineIn.Substring(indexOf, endIndex - indexOf + 1));
            }
            
            if (((indexOf = lineIn.IndexOf("wtime")) != -1) && board.Side == (int) Colour.WHITE) {
                indexOf += 6;
                var endIndex = EndOfInteger(lineIn, indexOf);
                time = Int32.Parse(lineIn.Substring(indexOf, endIndex - indexOf + 1));
            }
            
            if ((indexOf = lineIn.IndexOf("movestogo")) != -1) {
                indexOf += 10;
                var endIndex = EndOfInteger(lineIn, indexOf);
                movesToGo = Int32.Parse(lineIn.Substring(indexOf, endIndex - indexOf + 1));
            }
            
            if ((indexOf = lineIn.IndexOf("movetime")) != -1) {
                indexOf += 9;
                var endIndex = EndOfInteger(lineIn, indexOf);
                moveTime = Int32.Parse(lineIn.Substring(indexOf, endIndex - indexOf + 1));
            }
            
            // If depth variable exists.
            if ((indexOf = lineIn.IndexOf("depth")) != -1) {
                indexOf += 6;
                var endIndex = EndOfInteger(lineIn, indexOf);
                depth = Int32.Parse(lineIn.Substring(indexOf, endIndex - indexOf + 1));
            }

            if (moveTime != -1) { // if the movetime were set.
                time = moveTime;
                movesToGo = 1;
            }

            info.StartTime = Variables.Watch.ElapsedMilliseconds;
            info.Depth = depth;

            if (time != -1) { // if time or movetime was set.
                info.isTimeSet = true;
                time /= movesToGo; // time per move
                time -= 50; // decrement by 50 to avoid overrunning.
                info.StopTime = info.StartTime + time + inc;
            }

            if (depth == -1) {
                info.Depth = Variables.MAX_DEPTH; // if no depth was specificed, we set it to max depth.
            }
            
            Console.Write("time:{0} start:{1} stop:{2} depth:{3} timeset:{4}\n", time, info.StartTime,
                info.StopTime, info.Depth, info.isTimeSet);
            board.Searcher.SearchPosition(ref info);
        }

        /// <summary>
        /// Receives a string line and the index to the start of a integer number.
        /// Thereafter it locates the end index of the integer, and returns it.
        /// </summary>
        /// <param name="lineIn"> The line to analyze </param>
        /// <param name="index"> The start index of the number </param>
        /// <returns> The index of the last number of the integer </returns>
        /// <remarks> Programmed by contract. Assumes the parameter index is within the string and the start of the number </remarks>
        private int EndOfInteger(string lineIn, int index) {
            int endIndex = lineIn.Length;
            for (int i = index; i < lineIn.Length; i++) {
                if (!(lineIn[i] >= '0' && lineIn[i] <= '9')) {
                    endIndex = i;
                    break;
                }
            }

            return endIndex - 1;
        }
        
        private void PrintStartUpInfo() {
            Console.Write("id name {0}\n", Variables.PROGRAM_NAME);
            Console.Write("id author Romild\n");
            Console.Write("uciok\n");
        }
    }
}