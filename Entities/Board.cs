using Klondike.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
namespace Klondike.Entities {
    public unsafe sealed class Board {
        internal const int DeckSize = 52;
        internal const int FoundationSize = 4;
        internal const int TableauSize = 7;
        internal const int PileSize = FoundationSize + TableauSize + 2;
        internal const int TalonSize = 24;
        internal const int WastePile = 0;
        internal const int FoundationStart = WastePile + 1;
        internal const int FoundationEnd = FoundationStart + FoundationSize - 1;
        internal const int Foundation1 = FoundationStart;
        internal const int Foundation2 = FoundationStart + 1;
        internal const int Foundation3 = FoundationStart + 2;
        internal const int Foundation4 = FoundationStart + 3;
        internal const int TableauStart = FoundationEnd + 1;
        internal const int TableauEnd = TableauStart + TableauSize - 1;
        internal const int StockPile = TableauEnd + 1;

        public bool AllowFoundationToTableau { get; set; }
        private readonly Card[] state, initialState, deck;
        private readonly Pile[] piles, initialPiles;
        private readonly Move[] movesMade;
        private Random random;
        private readonly TalonHelper helper;
        private Move lastMove;
        private int foundationCount, foundationMinimumBlack, foundationMinimumRed;
        private int movesTotal, roundCount, drawCount;
        public int CardsInFoundation {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return foundationCount; }
        }
        public int TimesThroughDeck {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return roundCount; }
        }
        public int DrawCount {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return drawCount; }
        }
        public bool Solved {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return foundationCount == DeckSize; }
        }

        public Board(int drawAmount) {
            drawCount = drawAmount;
            random = new Random();
            helper = new TalonHelper(TalonSize);

            deck = new Card[DeckSize];
            movesMade = new Move[512];

            piles = new Pile[PileSize];
            initialPiles = new Pile[PileSize];

            int stateIndex = TalonSize * 2 + FoundationSize * 13 + TableauSize * 13 + TableauSize * (TableauSize - 1) / 2;
            state = new Card[stateIndex];
            initialState = new Card[stateIndex];

            stateIndex = 0;
            int pileIndex = 0;
            initialPiles[pileIndex++] = new Pile(state, stateIndex);
            stateIndex += TalonSize;

            for (int i = 0; i < FoundationSize; i++) {
                initialPiles[pileIndex++] = new Pile(state, stateIndex);
                stateIndex += 13;
            }

            for (int i = 0; i < TableauSize; i++) {
                int size = i + 13;
                initialPiles[pileIndex++] = new Pile(state, stateIndex);
                stateIndex += size;
            }

            initialPiles[pileIndex++] = new Pile(state, stateIndex);

            Shuffle(0);
        }
        public bool CanAutoPlay() {
            return piles[StockPile].Size == 0 && piles[WastePile].Size == 0 &&
                (piles[TableauStart].Size == 0 || piles[TableauStart].First == 0) &&
                (piles[TableauStart + 1].Size == 0 || piles[TableauStart + 1].First == 0) &&
                (piles[TableauStart + 2].Size == 0 || piles[TableauStart + 2].First == 0) &&
                (piles[TableauStart + 3].Size == 0 || piles[TableauStart + 3].First == 0) &&
                (piles[TableauStart + 4].Size == 0 || piles[TableauStart + 4].First == 0) &&
                (piles[TableauStart + 5].Size == 0 || piles[TableauStart + 5].First == 0) &&
                (piles[TableauStart + 6].Size == 0 || piles[TableauStart + 6].First == 0);
        }
        public void PlayMoves(string moves) {
            Reset();

            int draws = 0;
            List<Move> moveList = new List<Move>();
            int count = moves.Length;
            for (int i = 0; i < count; i++) {
                char c = moves[i];
                if (c == '@') {
                    draws++;
                    continue;
                } else if (char.IsWhiteSpace(c)) {
                    continue;
                }

                if (i + 1 >= count) { break; }

                int stockSize = piles[StockPile].Size;
                int totalCount = draws * drawCount;
                if (totalCount > stockSize) {
                    draws -= (stockSize + drawCount - 1) / drawCount;
                    totalCount = stockSize;
                    if (draws > 0) {
                        totalCount += draws * drawCount;
                    }
                }

                Move newMove = new Move(c, moves[++i], totalCount);
                moveList.Clear();
                GetAvailableMoves(moveList, true);
                bool foundMove = false;
                for (int k = 0; k < moveList.Count; k++) {
                    Move move = moveList[k];
                    if (move == newMove && (move.From != WastePile || move.Count == newMove.Count)) {
                        MakeMove(move);
                        foundMove = true;
                        break;
                    }
                }
                if (!foundMove) { break; }
                draws = 0;
            }
        }
        public SolveDetail SolveRandom(int randomGamesToTry = 40000, int maxMoves = 250, int maxRounds = 20) {
            List<Move> moves = new List<Move>(64);
            int bestCount = 0;
            int bestMoves = maxMoves + 1;
            Move[] solution = new Move[movesMade.Length];
            int solutionCount = 0;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            int solves = 0;
            for (int i = 0; i < randomGamesToTry; i++) {
                Reset();

                int movesMadeRnd = 0;
                do {
                    moves.Clear();
                    GetAvailableMoves(moves);
                    if (moves.Count == 0) { break; }

                    Move move = GetRandomMove(moves);
                    movesMadeRnd += MovesAdded(move);
                    MakeMove(move);
                } while (movesMadeRnd < bestMoves && roundCount <= maxRounds);

                if (foundationCount >= bestCount) {
                    if (Solved) {
                        if (movesMadeRnd < bestMoves) {
                            bestMoves = MovesMade;
                            solutionCount = movesTotal;
                            Array.Copy(movesMade, solution, movesTotal);
                        }
                        solves++;
                    } else if (foundationCount > bestCount) {
                        solutionCount = movesTotal;
                        Array.Copy(movesMade, solution, movesTotal);
                    }
                    bestCount = foundationCount;
                }
            }

            timer.Stop();

            Reset();
            for (int i = 0; i < solutionCount; i++) {
                MakeMove(solution[i]);
            }

            return new SolveDetail() {
                Result = bestCount == DeckSize ? SolveResult.Solved : SolveResult.Unknown,
                States = solves,
                Time = timer.Elapsed
            };
        }
        public SolveDetail Solve(int maxMoves = 250, int maxRounds = 20, int maxNodes = 10000000, bool terminateEarly = false) {
            Heap<MoveIndex> open = new Heap<MoveIndex>((int)(maxNodes));
            HashMap<State> closed = new HashMap<State>(FindPrime((int)(maxNodes * 1.1)));

            MoveNode[] nodeStorage = new MoveNode[maxNodes + 1];
            Array.Fill(nodeStorage, new MoveNode() { Parent = -1 });

            int nodeCount = 1;
            int maxFoundationCount = 0;
            List<Move> moves = new List<Move>(64);
            Move[] movesStorage = new Move[movesMade.Length];

            //Initialize previous state if there are moves already made
            {
                Array.Copy(movesMade, movesStorage, movesTotal);
                int movesToMake = movesTotal;

                Reset();
                State state = GameState();
                state.Moves = Estimate;
                closed.Add(state);
                for (int i = 0; i < movesToMake; i++) {
                    Move move = movesStorage[i];
                    MakeMove(move);
                    state = GameState();
                    state.Moves = Estimate;
                    nodeStorage[nodeCount] = new MoveNode() { Move = move, Parent = nodeCount - 1 };
                    nodeCount++;
                    closed.Add(state);
                }
            }

            //Add current state
            open.Enqueue(new MoveIndex() { Index = nodeCount - 1, Estimate = Estimate });

            int bestSolutionMoveCount = maxMoves + 1;
            int solutionIndex = -1;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (open.Count > 0 && nodeCount < maxNodes) {
                //Get next state to evaluate
                MoveIndex node = open.Dequeue();

                Estimate estimate = node.Estimate;
                if (estimate.Total >= bestSolutionMoveCount) { continue; }

                //Initialize game to the next state
                int movesToMake = nodeStorage[node.Index].Copy(movesStorage, nodeStorage);
                Reset();
                for (int i = movesToMake - 1; i >= 0; --i) {
                    MakeMove(movesStorage[i]);
                }

                //Get any available moves to check
                moves.Clear();
                GetAvailableMoves(moves);

                //Make available moves and add them to be evaulated
                int canAdd = moves.Count;
                for (int i = 0; i < canAdd; ++i) {
                    Move move = moves[i];
                    int movesAdded = MovesAdded(move);
                    MakeMove(move);

                    //Check estimated move count to be less than current best
                    int newCurrent = estimate.Current + movesAdded;
                    if (newCurrent > 255) { newCurrent = 255; }
                    Estimate newEstimate = new Estimate() { Current = (byte)newCurrent, Remaining = (byte)MinimumMovesRemaining(roundCount == maxRounds) };
                    if (newEstimate.Total < bestSolutionMoveCount && roundCount <= maxRounds) {
                        State key = GameState();
                        key.Moves = newEstimate;

                        //Check state doesn't exist or that it used more moves than current
                        int index = closed.Add(key);
                        if (index < 0 || closed[index].Moves.Total > newEstimate.Total) {
                            if (index >= 0) {
                                closed[index].Moves = newEstimate;
                            }
                            nodeStorage[nodeCount] = new MoveNode() { Move = move, Parent = node.Index };

                            //Check for best solution to foundations
                            if (foundationCount > maxFoundationCount || Solved) {
                                solutionIndex = nodeCount;
                                maxFoundationCount = foundationCount;

                                //Save solution
                                if (Solved) {
                                    bestSolutionMoveCount = newEstimate.Total;
                                    nodeCount++;
                                    if (terminateEarly) { open.Clear(); break; }
                                }
                            }

                            if (!Solved) {
                                short heuristic = (short)((newEstimate.Total << 1) + movesAdded + (DeckSize - foundationCount + (roundCount << 1)));
                                open.Enqueue(new MoveIndex() { Index = nodeCount++, Priority = heuristic, Estimate = newEstimate });
                                if (nodeCount >= maxNodes) { break; }
                            }
                        }
                    }

                    UndoMove();
                }
            }

            timer.Stop();

            //Reset state to best found solution
            if (solutionIndex >= 0) {
                int movesToMake = nodeStorage[solutionIndex].Copy(movesStorage, nodeStorage);
                Reset();
                for (int i = movesToMake - 1; i >= 0; --i) {
                    MakeMove(movesStorage[i]);
                }
            }

            SolveResult result = nodeCount < maxNodes ? maxFoundationCount == DeckSize ? !terminateEarly ? SolveResult.Minimal : SolveResult.Solved : SolveResult.Impossible : maxFoundationCount == DeckSize ? SolveResult.Solved : SolveResult.Unknown;
            return new SolveDetail() {
                Result = result,
                States = nodeCount,
                Time = timer.Elapsed,
                Moves = result == SolveResult.Solved || result == SolveResult.Minimal ? MovesMade : 0
            };
        }
        public SolveDetail SolveFast(int maxMoves = 250, int maxRounds = 20, int maxNodes = 2000000) {
            Heap<MoveIndex> open = new Heap<MoveIndex>(maxNodes);
            HashMap<StateFast> closed = new HashMap<StateFast>(FindPrime(maxNodes));

            MoveNode[] nodeStorage = new MoveNode[maxNodes + 1];
            Array.Fill(nodeStorage, new MoveNode() { Parent = -1 });

            int nodeCount = 1;
            int maxFoundationCount = 0;
            List<Move> moves = new List<Move>(64);
            Move[] movesStorage = new Move[movesMade.Length];

            //Initialize previous state if there are moves already made
            {
                Array.Copy(movesMade, movesStorage, movesTotal);
                int movesToMake = movesTotal;

                Reset();
                StateFast state = GameStateFast();
                state.Moves = Estimate;
                closed.Add(state);
                for (int i = 0; i < movesToMake; i++) {
                    Move move = movesStorage[i];
                    MakeMove(move);
                    state = GameStateFast();
                    state.Moves = Estimate;
                    nodeStorage[nodeCount] = new MoveNode() { Move = move, Parent = nodeCount - 1 };
                    nodeCount++;
                    closed.Add(state);
                }
            }

            //Add current state
            open.Enqueue(new MoveIndex() { Index = nodeCount - 1, Estimate = Estimate });

            int bestSolutionMoveCount = maxMoves + 1;
            int solutionIndex = -1;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (open.Count > 0 && nodeCount < maxNodes) {
                //Get next state to evaluate
                MoveIndex node = open.Dequeue();

                Estimate estimate = node.Estimate;
                if (estimate.Total >= bestSolutionMoveCount) { continue; }

                //Initialize game to the next state
                int movesToMake = nodeStorage[node.Index].Copy(movesStorage, nodeStorage);
                Reset();
                for (int i = movesToMake - 1; i >= 0; --i) {
                    MakeMove(movesStorage[i]);
                }

                //Get any available moves to check
                moves.Clear();
                GetAvailableMoves(moves);

                //Make available moves and add them to be evaulated
                int canAdd = moves.Count;
                for (int i = 0; i < canAdd; ++i) {
                    Move move = moves[i];
                    int movesAdded = MovesAdded(move);
                    MakeMove(move);

                    //Check estimated move count to be less than current best
                    int newCurrent = estimate.Current + movesAdded;
                    if (newCurrent > 255) { newCurrent = 255; }
                    Estimate newEstimate = new Estimate() { Current = (byte)newCurrent, Remaining = (byte)MinimumMovesRemaining(roundCount == maxRounds) };
                    if (newEstimate.Total < bestSolutionMoveCount && roundCount <= maxRounds) {
                        StateFast key = GameStateFast();
                        key.Moves = newEstimate;

                        //Check state doesn't exist or that it used more moves than current
                        int index = closed.Add(key);
                        if (index < 0 || closed[index].Moves.Total > newEstimate.Total) {
                            if (index >= 0) {
                                closed[index].Moves = newEstimate;
                            }
                            nodeStorage[nodeCount] = new MoveNode() { Move = move, Parent = node.Index };

                            //Check for best solution to foundations
                            if (foundationCount > maxFoundationCount || Solved) {
                                solutionIndex = nodeCount;
                                maxFoundationCount = foundationCount;

                                //Save solution
                                if (Solved) {
                                    bestSolutionMoveCount = newEstimate.Total;
                                    nodeCount++;
                                    open.Clear();
                                    break;
                                }
                            }

                            if (!Solved) {
                                short heuristic = (short)((newEstimate.Total << 2) + (DeckSize - foundationCount + (roundCount << 1)));
                                open.Enqueue(new MoveIndex() { Index = nodeCount++, Priority = heuristic, Estimate = newEstimate });
                                if (nodeCount >= maxNodes) { break; }
                            }
                        }
                    }

                    UndoMove();
                }
            }

            timer.Stop();

            //Reset state to best found solution
            if (solutionIndex >= 0) {
                int movesToMake = nodeStorage[solutionIndex].Copy(movesStorage, nodeStorage);
                Reset();
                for (int i = movesToMake - 1; i >= 0; --i) {
                    MakeMove(movesStorage[i]);
                }
            }

            return new SolveDetail() {
                Result = maxFoundationCount == DeckSize ? SolveResult.Solved : SolveResult.Unknown,
                States = nodeCount,
                Time = timer.Elapsed
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void MakeMove(Move move) {
            movesMade[movesTotal++] = move;
            lastMove = move;

            if (move.From == WastePile && move.Count != 0) {
                if (!move.Flip) {
                    piles[StockPile].RemoveFlip(ref piles[WastePile], move.Count);
                } else {
                    ++roundCount;
                    int stockSize = piles[StockPile].Size + piles[WastePile].Size - move.Count;
                    if (stockSize >= 1) {
                        piles[WastePile].RemoveFlip(ref piles[StockPile], stockSize);
                    } else {
                        piles[StockPile].RemoveFlip(ref piles[WastePile], -stockSize);
                    }
                }
            }

            if (move.From == WastePile || move.Count == 1) {
                piles[move.From].Remove(ref piles[move.To]);

                if (move.To <= FoundationEnd) {
                    ++foundationCount;
                } else if (move.From >= FoundationStart && move.From <= FoundationEnd) {
                    --foundationCount;
                }
            } else {
                piles[move.From].Remove(ref piles[move.To], move.Count);
            }

            if (move.Flip) {
                piles[move.From].Flip();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void UndoMove() {
            Move move = movesMade[--movesTotal];
            lastMove = movesTotal > 0 ? movesMade[movesTotal - 1] : default;

            if (move.From == WastePile || move.Count == 1) {
                piles[move.To].Remove(ref piles[move.From]);

                if (move.To <= FoundationEnd) {
                    --foundationCount;
                } else if (move.From >= FoundationStart && move.From <= FoundationEnd) {
                    ++foundationCount;
                }
            } else {
                piles[move.To].Remove(ref piles[move.From], move.Count);
            }

            if (move.Flip) {
                piles[move.From].Flip(move.Count);
            }

            if (move.From == WastePile && move.Count != 0) {
                if (!move.Flip) {
                    piles[WastePile].RemoveFlip(ref piles[StockPile], move.Count);
                } else {
                    --roundCount;
                    int wasteSize = piles[WastePile].Size + piles[StockPile].Size - move.Count;
                    if (wasteSize >= 1) {
                        piles[StockPile].RemoveFlip(ref piles[WastePile], wasteSize);
                    } else {
                        piles[WastePile].RemoveFlip(ref piles[StockPile], -wasteSize);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void GetAvailableMoves(List<Move> moves, bool allMoves = false) {
            SetFoundationMin();

            //Check if last move was to uncover card that could move to foundation
            if (!allMoves && lastMove.From >= TableauStart && lastMove.To >= TableauStart && !lastMove.Flip) {
                Pile pileFrom = piles[lastMove.From];
                int pileFromSize = pileFrom.Size;
                if (pileFromSize > 0) {
                    Card card = pileFrom.BottomNoCheck;
                    int foundationMinimum = 0;
                    byte cardFoundation = CanMoveToFoundation(card, ref foundationMinimum);
                    if (cardFoundation != 255) {
                        moves.Add(new Move(lastMove.From, cardFoundation, 1, pileFromSize > 1 && pileFrom.UpSize == 1));
                        return;
                    }
                }
            }

            if (CheckTableau(moves, allMoves)) { return; }
            if (CheckStockAndWaste(moves, allMoves)) { return; }
            if (AllowFoundationToTableau) { CheckFoundation(moves); }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void SetFoundationMin() {
            int min1 = piles[Foundation1].Size;
            int min2 = piles[Foundation3].Size;
            foundationMinimumBlack = (min1 <= min2 ? min1 : min2) + 1;
            min1 = piles[Foundation2].Size;
            min2 = piles[Foundation4].Size;
            foundationMinimumRed = (min1 <= min2 ? min1 : min2) + 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool CheckTableau(List<Move> moves, bool allMoves = false) {
            int emptyPiles = 0;
            for (byte i = TableauStart; i <= TableauEnd; ++i) {
                Pile pileFrom = piles[i];

                int pileFromSize = pileFrom.Size;
                if (pileFromSize == 0) { emptyPiles++; }
            }
            //Check tableau to foundation, Check tableau to tableau
            for (byte i = TableauStart; i <= TableauEnd; ++i) {
                Pile pileFrom = piles[i];

                int pileFromSize = pileFrom.Size;
                if (pileFromSize == 0) { continue; }

                Card fromBottom = pileFrom.BottomNoCheck;
                int foundationMinimum = 0;
                byte cardFoundation = CanMoveToFoundation(fromBottom, ref foundationMinimum);
                if (cardFoundation != 255) {
                    Move temp = new Move(i, cardFoundation, 1, pileFromSize > 1 && pileFrom.UpSize == 1);
                    //is this an auto move?
                    if (!allMoves && (int)fromBottom.Rank <= foundationMinimum) {
                        moves.Clear();
                        moves.Add(temp);
                        return true;
                    } else {
                        moves.Add(temp);
                    }
                }

                Card fromTop = pileFrom.TopNoCheck;
                int pileFromLength = fromTop.Rank - fromBottom.Rank + 1;
                bool kingMoved = fromTop.Rank != CardRank.King;

                for (byte j = TableauStart; j <= TableauEnd; ++j) {
                    if (i == j) { continue; }
                    Pile pileTo = piles[j];

                    if (pileTo.Size == 0) {
                        if (!kingMoved && pileFromSize != pileFromLength) {
                            moves.Add(new Move(i, j, (byte)pileFromLength, true));
                            //only create one move for a blank spot
                            kingMoved = !allMoves;
                        }
                        continue;
                    }

                    Card toBottom = pileTo.BottomNoCheck;
                    if ((int)toBottom.Rank - (int)fromTop.Rank > 1 || fromBottom.RedEven != toBottom.RedEven || fromBottom.Rank >= toBottom.Rank) {
                        continue;
                    }

                    int pileFromMoved = toBottom.Rank - fromBottom.Rank;
                    if (allMoves || (pileFromMoved == pileFromLength && (pileFromMoved != pileFromSize || emptyPiles == 0)) || (pileFromMoved < pileFromLength && CanMoveToFoundation(pileFrom.UpNoCheck(pileFromMoved), ref foundationMinimum) != 255)) {
                        //we are moving all face up cards
                        //or look to see if we are covering a card that can be moved to the foundation
                        moves.Add(new Move(i, j, (byte)pileFromMoved, pileFromSize > pileFromMoved && pileFromMoved == pileFromLength));
                    }
                }
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool CheckStockAndWaste(List<Move> moves, bool allMoves = false) {
            int talonCount = helper.Calculate(drawCount, piles[WastePile], piles[StockPile]);

            //Check talon cards
            for (byte j = 0; j < talonCount; ++j) {
                Card talonCard = helper.StockWaste[j];
                int cardsToDraw = helper.CardsDrawn[j];
                int foundationMinimum = 0;
                byte cardFoundation = CanMoveToFoundation(talonCard, ref foundationMinimum);
                bool flip = cardsToDraw < 0;
                if (flip) { cardsToDraw = -cardsToDraw; }

                if (cardFoundation != 255) {
                    moves.Add(new Move(WastePile, cardFoundation, (byte)cardsToDraw, flip));

                    if ((int)talonCard.Rank <= foundationMinimum) {
                        if (drawCount > 1 || allMoves) { continue; }

                        if (cardsToDraw == 0 || moves.Count == 1) {
                            return true;
                        }
                        break;
                    }
                }

                for (byte i = TableauStart; i <= TableauEnd; ++i) {
                    Card tableauCard = piles[i].Bottom;
                    if (tableauCard.Rank - talonCard.Rank == 1 && talonCard.IsRed != tableauCard.IsRed) {
                        moves.Add(new Move(WastePile, i, (byte)cardsToDraw, flip));

                        if (talonCard.Rank == CardRank.King && !allMoves) { break; }
                    }
                }
            }

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void CheckFoundation(List<Move> moves) {
            //Check foundation to tableau, very rarely needed to solve optimally
            for (byte i = FoundationStart; i <= FoundationEnd; ++i) {
                Pile foundPile = piles[i];
                int foundationSize = foundPile.Size;
                int foundationMinimum = foundationMinimumBlack < foundationMinimumRed ? foundationMinimumBlack : foundationMinimumRed;
                if (foundationSize <= foundationMinimum) { continue; }

                Card foundCard = foundPile.BottomNoCheck;
                for (byte j = TableauStart; j <= TableauEnd; ++j) {
                    Card cardTop = piles[j].Bottom;
                    if (cardTop.Rank - foundCard.Rank == 1 && foundCard.IsRed != cardTop.IsRed) {
                        moves.Add(new Move(i, j));
                        if (foundCard.Rank == CardRank.King) { break; }
                    }
                }
            }
        }
        public Estimate Estimate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Estimate() { Current = (byte)MovesMade, Remaining = (byte)MinimumMovesRemaining() }; }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MinimumMovesRemaining(bool lastRound = false) {
            Pile wastePile = piles[WastePile];
            int wasteSize = wastePile.Size;
            int moves = piles[StockPile].Size;
            moves += (moves + drawCount - 1) / drawCount + wasteSize;
            Span<byte> mins = stackalloc byte[4] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };

            if (drawCount == 1 || lastRound) {
                for (byte i = 0; i < wasteSize; ++i) {
                    Card card = wastePile[i];
                    if ((byte)card.Rank < mins[(byte)card.Suit]) {
                        mins[(byte)card.Suit] = (byte)card.Rank;
                    } else {
                        moves++;
                    }
                }
            }

            for (byte i = TableauStart; i <= TableauEnd; ++i) {
                mins.Fill(byte.MaxValue);
                Pile pile = piles[i];
                moves += pile.Size;

                for (byte j = 0; j < pile.Size; ++j) {
                    Card card = pile[j];
                    if ((byte)card.Rank < mins[(byte)card.Suit]) {
                        if (j < pile.First) {
                            mins[(byte)card.Suit] = (byte)card.Rank;
                        }
                    } else {
                        moves++;
                        if (j >= pile.First) {
                            break;
                        }
                    }
                }
            }

            return moves;
        }
        public StateFast GameStateFast() {
            StateFast key = new StateFast();
            int z = 0;
            Move lastLastMove = movesTotal > 2 ? movesMade[movesTotal - 2] : default;
            key[z++] = (byte)((piles[Foundation1].Size << 4) | piles[Foundation3].Size);
            key[z++] = (byte)((piles[Foundation2].Size << 4) | piles[Foundation4].Size);
            key[z++] = (byte)lastLastMove.Value1;
            key[z++] = (byte)lastMove.Value1;
            key[z++] = (byte)piles[lastMove.From].Bottom.ID;
            key[z++] = (byte)piles[StockPile].Size;
            key[z++] = (byte)piles[WastePile].Size;

            return key;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private State GameState() {
            Span<byte> order = stackalloc byte[] { TableauStart, TableauStart + 1, TableauStart + 2, TableauStart + 3, TableauStart + 4, TableauStart + 5, TableauStart + 6 };

            //sort the piles
            for (byte current = 1; current < TableauSize; ++current) {
                byte search = current;
                do {
                    Pile one = piles[order[search - 1]];
                    Pile two = piles[order[search]];
                    if (one.Top.ID2 > two.Top.ID2) { break; }

                    byte temp = order[--search];
                    order[search] = order[search + 1];
                    order[search + 1] = temp;
                } while (search > 0);
            }

            State key = new State();
            int z = 0;
            key[z++] = (byte)((piles[Foundation1].Size << 4) | piles[Foundation3].Size);
            key[z++] = (byte)((piles[Foundation2].Size << 4) | piles[Foundation4].Size);

            int bits = 5;
            int mask = (byte)piles[WastePile].Size;
            for (byte i = 0; i < TableauSize; ++i) {
                Pile pile = piles[order[i]];
                int upSize = pile.UpSize;

                int added = 10;
                mask <<= 6;
                if (upSize > 0) {
                    mask |= pile.TopNoCheck.ID;
                    added += upSize - 1;
                }

                bits += added;
                mask <<= 4;
                mask |= upSize--;
                for (int j = 0; j < upSize; ++j) {
                    mask <<= 1;
                    mask |= pile.UpNoCheck(j).Order;
                }

                do {
                    bits -= 8;
                    key[z++] = (byte)(mask >> bits);
                } while (bits >= 8);
            }
            if (bits > 0) {
                key[z] = (byte)(mask << (8 - bits));
            }
            return key;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int MovesAdded(Move move) {
            int movesAdded = 1;
            if (move.From == WastePile && move.Count != 0) {
                int stockSize = piles[StockPile].Size;
                if (!move.Flip) {
                    movesAdded += (move.Count + drawCount - 1) / drawCount;
                } else {
                    movesAdded += (stockSize + drawCount - 1) / drawCount;
                    movesAdded += (move.Count - stockSize + drawCount - 1) / drawCount;
                }
            }

            return movesAdded;
        }
        public Move GetRandomMove(List<Move> moves) {
            int drawHit = 0;
            do {
                int index = random.Next() % moves.Count;
                Move move = moves[index];
                if (move.From == WastePile && move.Count != 0) {
                    drawHit++;
                    if (drawHit >= (move.Count + drawCount - 1) / drawCount) {
                        return move;
                    }
                } else if (move.From >= FoundationStart && move.From <= FoundationEnd) {
                    drawHit++;
                    if (drawHit > 1) {
                        return move;
                    }
                } else {
                    return move;
                }
            } while (true);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte CanMoveToFoundation(Card card, ref int foundationMinimum) {
            int pile = FoundationStart + (int)card.Suit;
            foundationMinimum = foundationMinimumBlack < foundationMinimumRed ? foundationMinimumBlack : foundationMinimumRed;
            return piles[pile].Size == (int)card.Rank ? (byte)pile : (byte)255;
        }
        public bool SetDeal(string cardSet) {
            if (cardSet.Length < deck.Length * 3 - 1) { return false; }

            int decks = deck.Length / 52;
            int[] used = new int[52];

            if (cardSet[2] == ' ') {
                for (int i = 0; i < deck.Length; i++) {
                    char suit = char.ToUpper(cardSet[i * 3 + 1]);
                    switch (suit) {
                        case 'C': suit = (char)CardSuit.Clubs; break;
                        case 'D': suit = (char)CardSuit.Diamonds; break;
                        case 'S': suit = (char)CardSuit.Spades; break;
                        case 'H': suit = (char)CardSuit.Hearts; break;
                        default: return false;
                    }

                    char rank = char.ToUpper(cardSet[i * 3]);
                    switch (rank) {
                        case 'A': rank = (char)CardRank.Ace; break;
                        case '2': rank = (char)CardRank.Two; break;
                        case '3': rank = (char)CardRank.Three; break;
                        case '4': rank = (char)CardRank.Four; break;
                        case '5': rank = (char)CardRank.Five; break;
                        case '6': rank = (char)CardRank.Six; break;
                        case '7': rank = (char)CardRank.Seven; break;
                        case '8': rank = (char)CardRank.Eight; break;
                        case '9': rank = (char)CardRank.Nine; break;
                        case 'T': rank = (char)CardRank.Ten; break;
                        case 'J': rank = (char)CardRank.Jack; break;
                        case 'Q': rank = (char)CardRank.Queen; break;
                        case 'K': rank = (char)CardRank.King; break;
                        default: return false;
                    }

                    int id = suit * 13 + rank;
                    if (id < 0 || id >= 52) { return false; }
                    used[id]++;
                    deck[i] = new Card(id);
                }
            } else {
                int index = 0;
                for (int k = 1, m = 0; k <= TableauSize; k++) {
                    for (int i = k, j = m; i <= TableauSize; i++) {
                        int id = GetCard(cardSet, index++ * 3);
                        if (id < 0 || id >= 52) { return false; }
                        used[id]++;
                        deck[j] = new Card(id);
                        j += i;
                    }
                    m += k + 1;
                }

                int end = deck.Length - TalonSize;
                for (int i = deck.Length - 1; i >= end; i--) {
                    int id = GetCard(cardSet, index++ * 3);
                    if (id < 0 || id >= 52) { return false; }
                    used[id]++;
                    deck[i] = new Card(id);
                }
            }

            for (int i = 0; i < 52; i++) {
                if (used[i] != decks) { return false; }
            }

            SetupInitial();
            Reset();
            return true;
        }
        private int GetCard(string cardSet, int index) {
            int suit = (cardSet[index + 2] ^ 0x30) - 1;
            if (suit >= 2) {
                suit = (suit == 2) ? 3 : 2;
            }

            int rank = (cardSet[index] ^ 0x30) * 10 + (cardSet[index + 1] ^ 0x30);
            return suit * 13 + rank - 1;
        }
        public string GetDeal(bool numbers = true) {
            StringBuilder cardSet = new StringBuilder(deck.Length * 3);
            if (!numbers) {
                for (int i = 0; i < deck.Length; i++) {
                    cardSet.Append($"{deck[i]} ");
                }
            } else {
                for (int k = 1, m = 0; k <= TableauSize; k++) {
                    for (int i = k, j = m; i <= TableauSize; i++) {
                        AppendCard(cardSet, deck[j]);
                        j += i;
                    }
                    m += k + 1;
                }

                int end = deck.Length - TalonSize;
                for (int i = deck.Length - 1; i >= end; i--) {
                    AppendCard(cardSet, deck[i]);
                }
            }
            return cardSet.ToString();
        }
        private void AppendCard(StringBuilder cardSet, Card card) {
            int suit = (int)card.Suit;
            if (suit >= 2) {
                suit = (suit == 2) ? 3 : 2;
            }
            suit++;

            cardSet.Append($"{(int)card.Rank + 1:00}{suit}");
        }
        private struct GreenRandom {
            public uint Seed;
            public uint Next() {
                Seed = (uint)(((ulong)Seed * 16807) % 0x7fffffff);
                return Seed;
            }
        }
        public void ShuffleGreenFelt(uint seed) {
            GreenRandom rnd = new GreenRandom() { Seed = seed };
            for (int i = 0; i < 26; i++) {
                deck[i] = new Card(i);
            }
            for (int i = 0; i < 13; i++) {
                deck[i + 26] = new Card(i + 39);
            }
            for (int i = 0; i < 13; i++) {
                deck[i + 39] = new Card(i + 26);
            }
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 52; j++) {
                    int k = (int)(rnd.Next() % 52);
                    Card temp = deck[j];
                    deck[j] = deck[k];
                    deck[k] = temp;
                }
            }
            Card[] tmp = new Card[52];
            Array.Copy(deck, 0, tmp, 28, 24);
            Array.Copy(deck, 24, tmp, 0, 28);
            Array.Copy(tmp, deck, 52);

            int orig = 27;
            for (int i = 0; i < 7; i++) {
                int pos = (i + 1) * (i + 2) / 2 - 1;
                for (int j = 6 - i; j >= 0; j--) {
                    if (j >= i) {
                        Card temp = deck[pos];
                        deck[pos] = deck[orig];
                        deck[orig] = temp;
                    }
                    orig--;
                    pos += (6 - j + 1);
                }
            }

            SetupInitial();
            Reset();
        }
        public int Shuffle(int dealNumber = -1) {
            if (dealNumber != -1) {
                random = new Random(dealNumber);
            } else {
                dealNumber = random.Next();
                random = new Random(dealNumber);
            }

            for (int i = 0; i < deck.Length; i++) { deck[i] = new Card(i % 52); }

            int cardLength = deck.Length;
            for (int i = 0; i < 7; i++) {
                for (int x = cardLength - 1; x >= 0; x--) {
                    int k = random.Next() % cardLength;
                    Card temp = deck[x];
                    deck[x] = deck[k];
                    deck[k] = temp;
                }
            }

            SetupInitial();
            Reset();
            return dealNumber;
        }
        private void SetupInitial() {
            Array.Fill(state, Card.EMTPY);
            initialPiles[WastePile].Reset();
            for (int i = FoundationStart; i <= FoundationEnd; i++) {
                initialPiles[i].Reset();
            }

            int m = 0;
            for (int i = TableauStart, j = 1; i <= TableauEnd; i++, j++) {
                initialPiles[i].Reset();
                for (int k = 0; k < j; k++) {
                    initialPiles[i].Add(deck[m++]);
                }
                initialPiles[i].Flip();
            }

            initialPiles[StockPile].Reset();
            while (m < deck.Length) {
                initialPiles[StockPile].Add(deck[m++]);
            }
            Array.Copy(state, initialState, state.Length);
        }
        public void Reset() {
            foundationCount = 0;
            foundationMinimumBlack = 0;
            foundationMinimumRed = 0;
            movesTotal = 0;
            roundCount = 1;
            lastMove = default;

            Array.Copy(initialState, state, state.Length);
            Array.Copy(initialPiles, piles, piles.Length);
        }
        public bool VerifyGameState() {
            int count = deck.Length;
            for (int i = 0; i < piles.Length; i++) {
                int size = piles[i].Size;
                count -= size;
                if (size < 0) { return false; }
            }
            if (count != 0) { return false; }

            for (int i = TableauStart; i <= TableauEnd; i++) {
                Pile pile = piles[i];

                int upSize = pile.UpSize;
                if (upSize < 0) { return false; }
                if (upSize > 1) {
                    int suit = pile.BottomNoCheck.IsRed;
                    for (int j = 1; j < upSize; j++) {
                        int temp = pile.Up(j).IsRed;
                        if (suit == temp) {
                            return false;
                        }
                        suit = temp;
                    }
                }
            }
            return true;
        }
        public int MovesMade {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get {
                int stockSize = TalonSize;
                int wasteSize = 0;
                int moves = 0;
                for (int i = 0; i < movesTotal; i++) {
                    Move move = movesMade[i];
                    if (move.From == WastePile) {
                        if (!move.Flip) {
                            moves += (move.Count + drawCount - 1) / drawCount;
                            stockSize -= move.Count;
                            wasteSize += move.Count;
                        } else {
                            moves += (stockSize + drawCount - 1) / drawCount;
                            moves += (move.Count - stockSize + drawCount - 1) / drawCount;

                            int times = stockSize + wasteSize - move.Count;
                            wasteSize -= times;
                            stockSize += times;
                        }

                        wasteSize--;
                    }

                    moves++;
                }
                return moves;
            }
        }
        public string MovesMadeOutput {
            get {
                StringBuilder sb = new StringBuilder();
                int stockSize = TalonSize;
                int wasteSize = 0;
                for (int i = 0; i < movesTotal; i++) {
                    Move move = movesMade[i];
                    if (move.From == WastePile) {
                        if (!move.Flip) {
                            sb.Append('@', (move.Count + drawCount - 1) / drawCount);
                            stockSize -= move.Count;
                            wasteSize += move.Count;
                        } else {
                            int times = (stockSize + drawCount - 1) / drawCount;
                            sb.Append('@', times);
                            times = (move.Count - stockSize + drawCount - 1) / drawCount;
                            sb.Append('@', times);
                            times = stockSize + wasteSize - move.Count;
                            wasteSize -= times;
                            stockSize += times;
                        }

                        wasteSize--;
                    }

                    sb.Append($"{move.Display} ");
                }
                return sb.ToString();
            }
        }
        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            byte column = (byte)'A';

            sb.Append($"  {(char)column++}");
            for (int i = 0; i < TableauSize - FoundationSize - 1; i++) {
                sb.Append("   ");
            }
            for (int i = 0; i < FoundationSize; i++) {
                sb.Append($"  {(char)column++}");
            }
            sb.AppendLine();

            sb.Append($" {piles[WastePile].Bottom}");
            for (int i = 0; i < TableauSize - FoundationSize - 1; i++) {
                sb.Append("   ");
            }
            for (int i = FoundationStart; i <= FoundationEnd; i++) {
                sb.Append($" {piles[i].Bottom}");
            }
            sb.AppendLine();

            for (int i = 0; i < TableauSize; i++) {
                sb.Append($"  {(char)column++}");
            }
            sb.AppendLine();

            int maxHeight = TableauSize + 12;
            for (int j = 0; j < maxHeight; j++) {
                bool added = false;
                for (int i = TableauStart; i <= TableauEnd; i++) {
                    Pile pile = piles[i];
                    if (pile.Size > j) {
                        added = true;
                        if (j < pile.First) {
                            sb.Append($" {pile[j]}");
                        } else {
                            sb.Append($"+{pile[j]}");
                        }
                    } else {
                        sb.Append("   ");
                    }
                }
                sb.AppendLine();
                if (!added) { break; }
            }

            Pile stock = piles[StockPile];
            int stockSize = stock.Size;
            int count = 0;
            for (int i = stockSize - 1; i >= 0; i--) {
                sb.Append($" {stock[i]}");
                if (++count > TableauSize - 1) {
                    sb.AppendLine();
                    count = 0;
                }
            }
            if (count != 0) { sb.AppendLine(); }
            count = 0;
            Pile waste = piles[WastePile];
            stockSize = waste.Size - 1;
            for (int i = stockSize - 1; i >= 0; i--) {
                sb.Append('+').Append(waste[i].ToString());
                if (++count > TableauSize - 1) {
                    sb.AppendLine();
                    count = 0;
                }
            }
            if (count != 0) { sb.AppendLine(); }

            return sb.ToString();
        }
        private int FindPrime(int input) {
            int maxValue = input << 1;
            for (int i = input + (input & 1) - 1; i < maxValue; i += 2) {
                if (Miller(i, 20)) {
                    return i;
                }
            }
            return input;
        }
        private int Modulo(int a, int b, int c) {
            long x = 1, y = a;
            while (b > 0) {
                if ((b & 1) == 1) {
                    x = (x * y) % c;
                }
                y = (y * y) % c;
                b >>= 1;
            }
            return (int)(x % c);
        }
        private int Mulmod(int a, int b, int c) {
            long x = 0, y = a % c;
            while (b > 0) {
                if ((b & 1) == 1) {
                    x = (x + y) % c;
                }
                y = (y << 1) % c;
                b >>= 1;
            }
            return (int)(x % c);
        }
        private bool Miller(int p, int iteration) {
            if (p < 2) { return false; }
            if (p != 2 && (p & 1) == 0) { return false; }

            int s = p - 1;
            while ((s & 1) == 0) {
                s >>= 1;
            }
            for (int i = 0; i < iteration; i++) {
                int a = random.Next() % (p - 1) + 1, temp = s;
                int mod = Modulo(a, temp, p);
                while (temp != p - 1 && mod != 1 && mod != p - 1) {
                    mod = Mulmod(mod, mod, p);
                    temp <<= 1;
                }
                if (mod != p - 1 && (temp & 1) == 0) {
                    return false;
                }
            }
            return true;
        }
    }
}