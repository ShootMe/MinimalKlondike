using System;
namespace Klondike.Entities {
    public enum SolveResult {
        Unknown,
        Impossible,
        Solved,
        Minimal
    }
    public struct SolveDetail {
        public SolveResult Result;
        public int States;
        public TimeSpan Time;

        public override string ToString() {
            return $"{Result} ({States})";
        }
    }
}