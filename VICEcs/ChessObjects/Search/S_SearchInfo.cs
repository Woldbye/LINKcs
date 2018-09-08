namespace VICEcs.ChessObjects.Search {
    public struct S_SearchInfo {
        public long StartTime;
        public long StopTime;
        public int Depth;
        public int DepthSet;
        public int MovesToGo;

        public bool Quit;
        public bool isTimeSet;
        public bool Stopped;
        public bool Infinite;
        
        public long Nodes;

        public float Fh; // fail high
        public float Fhf; // fail high first.
    }
}