using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Klondike.Entities {
    [StructLayout(LayoutKind.Sequential, Size = 6, Pack = 2)]
    public struct MoveNode {
        public int Parent;
        public Move Move;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int Copy(Move[] destination, MoveNode[] moveList) {
            int index = 0;
            if (Move.IsNull) { return 0; }

            destination[index++] = Move;
            int parentIndex = Parent;
            while (parentIndex >= 0) {
                MoveNode parent = moveList[parentIndex];
                if (parent.Move.IsNull) { break; }

                destination[index++] = parent.Move;
                parentIndex = parent.Parent;
            }
            return index;
        }
        public override string ToString() {
            return $"{Move}";
        }
    }
}