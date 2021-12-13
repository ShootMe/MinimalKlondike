using Klondike.Entities;
using System;
using System.Diagnostics;
namespace Klondike {
    public class Program {
        public static void Main(string[] args) {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //113 Moves ~8 sec
            //SolveGame(123);

            //115 Moves ~26sec
            //SolveGame("081054022072134033082024052064053012061013042093084124092122062031083121113023043074051114091014103044131063041102101133011111071073034123104112021132032094");

            //106 Moves ~3sec - Playing a few moves manually before solving ~0.2sec
            SolveGame(888, 1, "IC @@AL KL @@AK @@@@@AE LJ AK LK");

            sw.Stop();
            Console.WriteLine($"Done {sw.Elapsed}");
        }
        private static SolveResult SolveGame(int deal, int drawCount = 1, string movesMade = null, bool allowFoundationMoves = false) {
            Board board = new Board(drawCount);
            board.Shuffle(deal);
            if (!string.IsNullOrEmpty(movesMade)) {
                board.PlayMoves(movesMade);
            }
            board.AllowFoundationToTableau = allowFoundationMoves;

            return SolveGame(board);
        }
        private static SolveResult SolveGame(string deal, int drawCount = 1, string movesMade = null, bool allowFoundationMoves = false) {
            Board board = new Board(drawCount);
            board.SetDeal(deal);
            if (!string.IsNullOrEmpty(movesMade)) {
                board.PlayMoves(movesMade);
            }
            board.AllowFoundationToTableau = allowFoundationMoves;

            return SolveGame(board);
        }
        private static SolveResult SolveGame(Board board) {
            Console.WriteLine($"Deal: {board.GetDeal()}");
            Console.WriteLine();
            Console.WriteLine(board);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            SolveDetail result = board.Solve(200, 10, 100000000);

            sw.Stop();

            Console.WriteLine($"Moves: {board.MovesMadeOutput}");
            Console.WriteLine();
            Console.WriteLine($"(Deal State: {result.Result} Foundation: {board.CardsInFoundation} Moves: {board.MovesMade} Rounds: {board.TimesThroughDeck} Took: {sw.Elapsed})");

            return result.Result;
        }
    }
}
