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
            Hashkeys.InitHashKeys();
            Variables.Watch.Start();
            MvvLva.InitMvvLva();
        }        
    }
}