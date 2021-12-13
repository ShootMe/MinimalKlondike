using System.Runtime.InteropServices;
namespace Klondike.Entities {
    [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 1)]
    public struct Card {
        public static readonly Card EMTPY = new Card() { ID = 52, ID2 = 0, Suit = CardSuit.None, Rank = CardRank.None, IsEven = 1, IsRed = 2, RedEven = 2, Order = 0 };
        private static readonly string[] Cards = { "AC", "2C", "3C", "4C", "5C", "6C", "7C", "8C", "9C", "TC", "JC", "QC", "KC",
                                                   "AD", "2D", "3D", "4D", "5D", "6D", "7D", "8D", "9D", "TD", "JD", "QD", "KD",
                                                   "AS", "2S", "3S", "4S", "5S", "6S", "7S", "8S", "9S", "TS", "JS", "QS", "KS",
                                                   "AH", "2H", "3H", "4H", "5H", "6H", "7H", "8H", "9H", "TH", "JH", "QH", "KH", "  "};
        public byte ID;
        public byte ID2;
        public CardSuit Suit;
        public CardRank Rank;
        public byte IsRed;
        public byte IsEven;
        public byte RedEven;
        public byte Order;
        public Card(int id) {
            ID = (byte)id;
            Rank = (CardRank)(id % 13);
            Suit = (CardSuit)(id / 13);
            ID2 = (byte)(((int)Rank << 2) | (int)Suit);
            IsRed = (byte)((int)Suit & 1);
            IsEven = (byte)((int)Rank & 1);
            RedEven = (byte)(IsRed ^ IsEven);
            Order = (byte)((int)Suit >> 1);
        }
        public override string ToString() {
            return Cards[ID];
        }
    }
}