namespace SudokuSolver2010
{
    public class SudokuHeuristics
    {
        int workDone;
        bool invalid;

        SudokuCell[] twinSet, tripleSet, quadSet;

        public SudokuHeuristics(SudokuPuzzle input)
        {
            twinSet = new SudokuCell[input.Width];
            tripleSet = new SudokuCell[input.Width];
            quadSet = new SudokuCell[input.Width];
        }

        public void ResolvePuzzle(SudokuPuzzle puzzle, out bool isValid, out bool isComplete)
        {
            workDone = 1;
            invalid = false;
            while (workDone > 0)
            {
                workDone = 0;

                for (int i = 0; i < puzzle.Groups.Length; i++)
                {
                    ProcessRowColumnOrRegion(puzzle, i);
                    if (!invalid && workDone == 0) TryToFindHiddenValues(puzzle, i);
                    //if (!invalid && workDone == 0) RuleOfK(puzzle, i);                   
                    if (invalid) { isValid = false; isComplete = false; return; };
                }
            }
            isValid = true;
            isComplete = SudokuValidator.IsComplete(puzzle);
        }

        private void TryToFindHiddenValues(SudokuPuzzle puzzle, int index)
        {
            SudokuCell[] group = puzzle.Groups[index];

            for (int currentValue = 1; currentValue <= group.Length; currentValue++)
            {
                SudokuCell currentValueIsPossible = null;

                for (int i = 0; i < group.Length; i++)
                {
                    // This is statement is an optimization for larger puzzles
                    if (group[i].Value == currentValue) break;

                    if (group[i].Value == 0)
                    {
                        if ((group[i].Possibilities & SudokuCell.BitMasks[currentValue]) == SudokuCell.BitMasks[currentValue])
                        {
                            if (currentValueIsPossible == null)
                            {
                                // If we get here then this is the only cell so far that has currentValue as a possibility.
                                currentValueIsPossible = group[i];
                            }
                            else
                            {
                                // If we got here then there are at least 2 cells that have currentValue as a possibility.
                                //  So we give up on this heuristic.
                                currentValueIsPossible = null;
                                break;
                            }
                        }
                    }
                }

                if (currentValueIsPossible != null)
                {
                    // If we get here then currentValueIsPossible is the only cell in the group that has currentValue as a possibility.
                    // So we can safely assume that this cell's value is currentValue.
                    currentValueIsPossible.Value = currentValue;
                    workDone++;

#if DEBUG
                    Program.CountHiddenValueAttempt(true);
#endif

                    return;
                }
            }

#if DEBUG
                    Program.CountHiddenValueAttempt(false);
#endif
        }

        private void ProcessRowColumnOrRegion(SudokuPuzzle puzzle, int index)
        {
            SudokuCell[] set = puzzle.Groups[index];
            int twoCount = 0, threeCount = 0, fourCount = 0, tripleLen = 0, twinLen = 0, quadLen = 0;
            ulong seen = 0, mask;

            for (int i = 0; i < set.Length; i++)
            {
                mask = SudokuCell.BitMasks[set[i].Value];
                if (set[i].Value != 0)
                {
                    if ((seen & mask) == mask) { invalid = true; return; }
                    seen = seen ^ mask;

                    // If this group has not seen this value yet
                    //    Remove this value as a possibility from all other cells in the group
                    if ((puzzle.GroupSeen[index] & mask) != mask)
                    {
                        // Set flag indicating that this group has now seen this value
                        // and that this value has been removed as a possibility from all
                        // other cells in the group
                        puzzle.GroupSeen[index] = puzzle.GroupSeen[index] ^ mask;
                        
                        for (int j = 0; j < set.Length; j++)
                        {
                            if (set[j].Value == 0)
                            {
                                set[j].TryToRemovePossibility(set[i].Value, ref workDone);
                            }
                        }
                    }
                }
                else
                {
                    if (set[i].NumberOfPossibilities == 2)
                    {
                        twinSet[twinLen++] = set[i];
                        twoCount++;
                    }
                    else if (set[i].NumberOfPossibilities == 3)
                    {
                        tripleSet[tripleLen++] = set[i];
                        threeCount++;
                    }
                    else if (set[i].NumberOfPossibilities == 4)
                    {
                        quadSet[quadLen++] = set[i];
                        fourCount++;
                    }
                }
            }

            if(twoCount > 1) PerformNakedTwinTest(set, twinSet, twinLen);
            if (threeCount > 2) PerformNakedTripletTest(set, tripleSet, tripleLen);
            if (fourCount > 3) PerformNakedQuadTest(set, quadSet, quadLen);
        }

        private void RuleOfK(SudokuPuzzle puzzle, int index)
        {
            SudokuCell[] group = puzzle.Groups[index];

            InsertionSortCellsByPossibilities(group, 0, group.Length);

            ulong last = ulong.MaxValue;
            int lastI = 0;
            int kCount = 0;
            int kCandidate = 0;
            for (int i = 0; i < group.Length; i++)
            {
                if (group[i].Value == 0)
                {
                    if (group[i].Possibilities == last)
                    {
                        kCandidate = group[i].NumberOfPossibilities;
                        kCount++;
                    }
                    else
                    {
                        if (kCandidate == kCount && kCount > 1)
                        {

                            int[] valuesToRemoveFromOtherCells = group[lastI].GetAllRemainingValues(puzzle.Width);
                            for (int k = 0; k < group.Length; k++)
                            {
                                if (group[k].Value == 0 && group[k].Possibilities != last)
                                {
                                    for (int j = 0; j < kCount; j++)
                                    {
                                        if ((group[k].Possibilities & SudokuCell.BitMasks[valuesToRemoveFromOtherCells[j]]) == SudokuCell.BitMasks[valuesToRemoveFromOtherCells[j]])
                                        {
                                            group[k].ForceRemovePossibility(valuesToRemoveFromOtherCells[j]);
                                            workDone++;
                                        }
                                    }
                                }
                            }

                            if (workDone > 0)
                            {
#if DEBUG
                                Program.CountRuleOfK(true);
#endif
                                return;
                            }
                        }

                        kCount = 1;
                    }
                    lastI = i;
                    last = group[i].Possibilities;
                }
            }
#if DEBUG
            Program.CountRuleOfK(false);
#endif
        }

        private void PerformNakedTwinTest(SudokuCell[] cells, SudokuCell[] twinSet, int numTwinCandidates)
        {
            InsertionSortCellsByPossibilities(twinSet, 0, numTwinCandidates);

            SudokuCell lastCell = twinSet[0];
            SudokuCell currentCell;

            for (int i = 1; i < numTwinCandidates; i++)
            {
                currentCell = twinSet[i];

                // if there are 2 possibilities &&
                //    they are the same 2 as the last cell &&
                //    this is the last cell || the next cell has different possibilities
                // then 
                //    this is a sudoku  twin

                if (currentCell.NumberOfPossibilities == 2 &&
                    currentCell.Possibilities == lastCell.Possibilities &&
                    (i == numTwinCandidates - 1 || currentCell.Possibilities != twinSet[i + 1].Possibilities))
                {
                    int possibleA = 0, possibleB = 0;

                    currentCell.GetTwoRemainingValues(cells.Length, ref possibleA, ref possibleB);

                    for (int j = 0; j < cells.Length; j++)
                    {
                        if (cells[j].Value == 0 && cells[j] != currentCell && cells[j] != lastCell)
                        {
                            cells[j].TryToRemovePossibility(possibleA, ref workDone);
                            cells[j].TryToRemovePossibility(possibleB, ref workDone);
                        }
                    }
                    return;
                }
                lastCell = currentCell;
            }
        }

        private void PerformNakedTripletTest(SudokuCell[] cells, SudokuCell[] tripleSet, int numTripletCandidates)
        {
            InsertionSortCellsByPossibilities(tripleSet, 0, numTripletCandidates);

            SudokuCell twoAgoCell = tripleSet[0], lastCell = tripleSet[1], currentCell;
            for (int i = 2; i < numTripletCandidates; i++)
            {
                currentCell = tripleSet[i];

                if (currentCell.NumberOfPossibilities == 3 &&
                    currentCell.Possibilities == lastCell.Possibilities && currentCell.Possibilities == twoAgoCell.Possibilities &&
                    (i == numTripletCandidates - 1 || currentCell.Possibilities != tripleSet[i + 1].Possibilities))
                {
                    int possibleA = 0, possibleB = 0, possibleC = 0;

                    currentCell.GetThreeRemainingValues(cells.Length, ref possibleA, ref possibleB, ref possibleC);

                    for (int j = 0; j < cells.Length; j++)
                    {
                        if (cells[j].Value == 0 && cells[j] != currentCell && cells[j] != lastCell && cells[j] != twoAgoCell)
                        {
                            cells[j].TryToRemovePossibility(possibleA, ref workDone);
                            cells[j].TryToRemovePossibility(possibleB, ref workDone);
                            cells[j].TryToRemovePossibility(possibleC, ref workDone);
                        }
                    }
                    return;
                }
                twoAgoCell = lastCell;
                lastCell = currentCell;
            }
        }


        private void PerformNakedQuadTest(SudokuCell[] cells, SudokuCell[] quadSet, int numQuadCandidates)
        {
            InsertionSortCellsByPossibilities(quadSet, 0, numQuadCandidates);

            SudokuCell threeAgoCell = quadSet[0], twoAgoCell = quadSet[1], lastCell = quadSet[2], currentCell;
            for (int i = 3; i < numQuadCandidates; i++)
            {
                currentCell = quadSet[i];

                if (currentCell.NumberOfPossibilities == 4 &&
                    currentCell.Possibilities == lastCell.Possibilities && currentCell.Possibilities == twoAgoCell.Possibilities && currentCell.Possibilities == threeAgoCell.Possibilities &&
                    (i == numQuadCandidates - 1 || currentCell.Possibilities != quadSet[i + 1].Possibilities))
                {
                    int possibleA = 0, possibleB = 0, possibleC = 0, possibleD = 0;

                    currentCell.GetFourRemainingValues(cells.Length, ref possibleA, ref possibleB, ref possibleC, ref possibleD);

                    for (int j = 0; j < cells.Length; j++)
                    {
                        if (cells[j].Value == 0 && cells[j] != currentCell && cells[j] != lastCell && cells[j] != twoAgoCell && cells[j] != threeAgoCell)
                        {
                            cells[j].TryToRemovePossibility(possibleA, ref workDone);
                            cells[j].TryToRemovePossibility(possibleB, ref workDone);
                            cells[j].TryToRemovePossibility(possibleC, ref workDone);
                            cells[j].TryToRemovePossibility(possibleD, ref workDone);
                        }
                    }
                    return;
                }

                threeAgoCell = twoAgoCell;
                twoAgoCell = lastCell;
                lastCell = currentCell;
            }
        }

        private static void InsertionSortCellsByPossibilities(SudokuCell[] a, int start, int end)
        {
            int i, j;
            SudokuCell index;

            for (i = 1; i < end; i++)
            {
                index = a[i];
                j = i;

                while ((j > 0) && (a[j - 1].Possibilities > index.Possibilities))
                {
                    a[j] = a[j - 1];
                    j = j - 1;
                }
                a[j] = index;
            }
        }
    }
}
