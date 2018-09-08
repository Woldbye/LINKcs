using System;

namespace VICEcs.Testing {
    public static class MoveFormatBits {
        public static void PrintBin(int move) {
            int index = 0;
            Console.Write("As binary:\n");
            for (index = 27; index >= 0; index--) {
                Console.Write(((1 << index) & move) != 0 ? "1" : "0");

                if ((index != 28) && (index % 4 == 0)) {
                    Console.Write(" ");
                }
            }

            Console.Write("\n");
        }
    }
}