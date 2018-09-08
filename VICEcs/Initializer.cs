using System.Diagnostics;
using VICEcs.ChessDefinitions;
using VICEcs.ChessObjects.Search;
using VICEcs.Tools;

namespace VICEcs {
    public static class Initializer {
        public static void AllInit() {
            Conversion.InitSq120To64();
            Conversion.InitFilesBrdAndRanksBrd();
            EvalBitMask.InitBitMask();
            KingBitMask.Init();
            Hashkeys.InitHashKeys();
            Initializer.IsDebugging();
            Variables.Watch.Start();
            MvvLva.InitMvvLva();
        }
        
        /// <summary>
        /// Sets the IsDebug variable to true, if compiled in DEBUG-mode.
        /// </summary>
        [ConditionalAttribute("DEBUG")]
        private static void IsDebugging() {
            Variables.IsDebug = true;
        }
    }
}