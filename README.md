# LINKcs
Chess engine implemented in C# on basis of a long series of instructed videos by Bluefever Software.
From the start, the goal of the program have been to further develop my C# skills with a special emphasis
on coding with bit based data structures. 
I have made no effort to optimize, or group the program according to an object-oriented style. Due to this, 
most of the classes are static.

The chess engine is implemented with a alpha-beta search algorithm. 
This search algorithm have been further optimized by implementing a variety of heuristics.
Among these are the Killer-heuristic, History-heuristic, PV-Table, MvvLva (Most valueable victim, least valuable attacker),
and a couple others. 

Currently, the program can be operated by any GUI that implements the UCI-protocol. I have used Arena for testing,
but any GUI upholding the protocol should do.

As of 13/09/2018, the engine isn't entirely complete, but it compiles can be played.
Possible future improvements include but are not limited to, implementing a polyglot opening book and optimizing compiling.
