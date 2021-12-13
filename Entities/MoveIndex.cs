using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Klondike.Entities {
    [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 4)]
    public struct MoveIndex : IComparable<MoveIndex> {
        public int Index;
        public short Priority;
        public Estimate Estimate;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(MoveIndex other) {
            return Priority.CompareTo(other.Priority);
        }
        public override string ToString() {
            return $"({Priority}, {Estimate})";
        }
    }
}