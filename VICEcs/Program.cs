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
            UCI uci = new UCI();
            uci.MainLoop();
        }
    }
}