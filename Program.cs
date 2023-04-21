using Klondike.Entities;
using System;
using System.Diagnostics;
using System.Globalization;
namespace Klondike {
    public class Program {
        public static void Main(string[] args) {
            if (args != null && args.Length > 0 && ((args.Length - 1) & 1) == 1) {
                Console.WriteLine($"Invalid argument count.");
                args = null;
            }

            if (args == null || args.Length == 0) {
                Console.WriteLine(
@$"Minimal Klondike
Klondike.exe [Options] [CardSet]

DrawCount (Default=1)
-D #

Initial Moves
-M ""Moves To Play Initially""

Max States (Default=50,000,000) (About 1GB RAM Per 22 Million)
-S #

Solve Seed 123 from GreenFelt:
Klondike.exe 123

Solve Given CardSet With Initial Moves:
Klondike.exe -D 1 -M ""HE KE @@@@AD GD LJ @@AH @@AJ GJ @@@@AG @AB"" 081054022072134033082024052064053012061013042093084124092122062031083121113023043074051114091014103044131063041102101133011111071073034123104112021132032094");
                return;
            }

            string cardSet = args[^1].Replace("\"", "");
            int drawCount = 1;
            string moveSet = null;
            int maxStates = 50_000_000;

            for (int i = 0; i < args.Length - 1; i++) {
                if (args[i] == "-D" && i + 1 < args.Length) {
                    if (!int.TryParse(args[i + 1], out drawCount)) {
                        Console.WriteLine($"Invalid DrawCount argument {args[i + 1]}. Defaulting to 1.");
                        drawCount = 1;
                    }
                    i++;
                } else if (args[i] == "-S" && i + 1 < args.Length) {
                    if (!int.TryParse(args[i + 1], NumberStyles.AllowThousands, null, out maxStates)) {
                        Console.WriteLine($"Invalid MaxStates argument {args[i + 1]}. Defaulting to 50,000,000.");
                        maxStates = 50_000_000;
                    }
                    i++;
                } else if (args[i] == "-M" && i + 1 < args.Length) {
                    moveSet = args[i + 1];
                    i++;
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (cardSet.Length < 11) {
                uint.TryParse(cardSet, out uint seed);
                SolveGame(seed, drawCount, moveSet, maxStates);
            } else {
                SolveGame(cardSet, drawCount, moveSet, maxStates);
            }

            sw.Stop();
            Console.WriteLine($"Done {sw.Elapsed}");
        }
        private static SolveDetail SolveGame(uint deal, int drawCount = 1, string movesMade = null, int maxStates = 50_000_000) {
            Board board = new Board(drawCount);
            board.ShuffleGreenFelt(deal);
            if (!string.IsNullOrEmpty(movesMade)) {
                board.PlayMoves(movesMade);
            }
            board.AllowFoundationToTableau = true;

            return SolveGame(board, maxStates);
        }
        private static SolveDetail SolveGame(string deal, int drawCount = 1, string movesMade = null, int maxStates = 50_000_000) {
            Board board = new Board(drawCount);
            board.SetDeal(deal);
            if (!string.IsNullOrEmpty(movesMade)) {
                board.PlayMoves(movesMade);
            }
            board.AllowFoundationToTableau = true;

            return SolveGame(board, maxStates);
        }
        private static SolveDetail SolveGame(Board board, int maxStates) {
            Console.WriteLine($"Deal: {board.GetDeal()}");
            Console.WriteLine();
            Console.WriteLine(board);

            SolveDetail result = board.Solve(250, 15, maxStates);

            Console.WriteLine($"Moves: {board.MovesMadeOutput}");
            Console.WriteLine();
            Console.WriteLine($"(Deal Result: {result.Result} Foundation: {board.CardsInFoundation} Moves: {board.MovesMade} Rounds: {board.TimesThroughDeck} States: {result.States} Took: {result.Time})");

            return result;
        }
    }
}
