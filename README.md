# Minimal Klondike Solver
Finds minimal length solutions for the thoughtful version of Klondike (Patience) Solitaire.

### Piles
  - A = Waste Pile
  - B = Clubs Pile
  - C = Diamonds Pile
  - D = Spades Pile
  - E = Hearts Pile
  - F = Tableau 1
  - G = Tableau 2
  - H = Tableau 3
  - I = Tableau 4
  - J = Tableau 5
  - K = Tableau 6
  - L = Tableau 7

### Deck Format
I took the format from an old web site, it's not the best, but haven't got around to adding anything better.

Each card is 'RRS' where R=Rank and S=Suit.
Rank goes from 01 - 13 and Suit goes from 1 - 4 (Clubs,Diamonds,Hearts,Spades)

ie) 052 = 5 of diamonds

Position of cards in deck string:
```
 A        B  C  D  E

 F  G  H  I  J  K  L
01 02 03 04 05 06 07
   08 09 10 11 12 13
      14 15 16 17 18
         19 20 21 22
            23 24 25
               26 27
                  28

Draw pile 29-52
```

072103023042094134111092051034044074114052123011083122012131091082124064014093033112071104132053133102084041013073063031061043081054113062024021101022032121

Would equate to this (+ represents visible cards), the 7C is the first card to be turned over in the draw pile when drawing one at a time, then TS, KD, etc...:
```
  A        B  C  D  E

  F  G  H  I  J  K  L
+7D TH 2H 4D 9S KS JC
   +9D 5C 3S 4S 7S JS
      +5D QH AC 8H QD
         +AD KC 9C 8D
            +QS 6S AS
               +9H 3H
                  +JD

 7C TS KD 5H KH TD 8S
 4C AH 7H 6H 3C 6C 4H
 8C 5S JH 6D 2S 2C TC
 2D 3D QC
```

### Moves
Are in the format XY, where X is the character of the source pile, and Y is the character of the destination pile. '@' represents a draw

ie) For the above deal with a draw count of 1 running this sequnce of moves would result in the following state:
```
 "IC @@AL KL @@AK @@@@@AE LJ AK LK"

  A        B  C  D  E
 8S          AD    AH
  F  G  H  I  J  K  L
+7D TH 2H 4D 9S KS JC
   +9D 5C 3S 4S 7S JS
      +5D+QH AC 8H QD
             KC 9C 8D
            +QS+6S+AS
            +JD+5H
            +TS+4C
            +9H+3H

 7H 6H 3C 6C 4H 8C 5S
 JH 6D 2S 2C TC 2D 3D
 QC
+TD+KH+KD+7C
```