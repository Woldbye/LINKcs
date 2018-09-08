using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading;
using VICEcs.ChessObjects.Search;

namespace VICEcs.Tools {
    public static class Misc {
        public static void ReadInput(ref S_SearchInfo info) {
            string stdin = null;
            
            if (Console.In.Peek() != -1) {
                stdin = Console.ReadLine();

                if (stdin != null) {
                    if (stdin.Length >= 4 && stdin.Substring(0, 4).Equals("quit")) {
                        info.Quit = true;
                    }    
                }
            }
        }
    }
}