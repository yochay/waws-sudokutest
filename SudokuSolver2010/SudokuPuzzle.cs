using System;

namespace SudokuSolver2010
{
    public class SudokuPuzzle
    {
        public SudokuCell[][] Cells;
        public int Width, WidthSqrt;
        public SudokuCell[][] Groups;
        public ulong[] GroupSeen;
        public int OriginalIndex;

        private int GuessI, GuessJ, GuessValue;

        public SudokuPuzzle(int[][] initialValues, int originalIndex = 0)
        {
            OriginalIndex = originalIndex;
            Width = initialValues.Length;
            WidthSqrt = (int)Math.Sqrt(Width);
            Cells = new SudokuCell[Width][];
            for (int i = 0; i < Width; i++)
            {
                Cells[i] = new SudokuCell[Width];
                for (int j = 0; j < Width; j++)
                {
                    int value = initialValues[i][j];
                    Cells[i][j] = value == 0 ? new SudokuCell(Width) : new SudokuCell(Width, value);
                }
            }

            // 3 because each cell is a part of 3 groups (i.e row, 
            Groups = new SudokuCell[3 * Width][];
            GroupSeen = new ulong[3 * Width];

            AnalyzeAllRowsAndCols();
            AnalyzeAllRegions();
        }

        public SudokuPuzzle(SudokuPuzzle initialValues)
        {
            Width = initialValues.Width;
            WidthSqrt = initialValues.WidthSqrt;
            Cells = new SudokuCell[initialValues.Width][];
            for (int i = 0; i < Width; i++)
            {
                Cells[i] = new SudokuCell[Width];
                for (int j = 0; j < Width; j++)
                {
                    int value = initialValues.Cells[i][j].Value;
                    Cells[i][j] = value == 0 ? new SudokuCell(Width) : new SudokuCell(Width, value);
                }
            }

            Groups = new SudokuCell[3 * Width][];
            GroupSeen = new ulong[3 * Width];

            AnalyzeAllRowsAndCols();
            AnalyzeAllRegions();
        }

        private SudokuPuzzle(SudokuPuzzle initialValues, int guessI, int guessJ, int guessValue) : this(initialValues)
        {
            Width = initialValues.Width;
            WidthSqrt = initialValues.WidthSqrt;
            GuessI = guessI;
            GuessJ = guessJ;
            GuessValue = guessValue;
            Cells[guessI][guessJ].Value = guessValue;
        }

        private void AnalyzeAllRowsAndCols()
        {
            for (int i = 0; i < Width; i++)
            {
                Groups[i] = new SudokuCell[Width];
                Groups[Width + i] = new SudokuCell[Width];

                for (int j = 0; j < Width; j++)
                {
                    Groups[i][j] = Cells[i][j]; // rows
                    Groups[Width + i][j] = Cells[j][i]; // cols
                }
            }
        }

        private void AnalyzeAllRegions()
        {
            int regionI = 0;
            int regionJ = 0;
            int regionCellIndex = 0;
            int maxRegionI, maxRegionJ;
            int regionNumber = 2 * Width;
            for (int i = 0; i < WidthSqrt; i++)
            {
                regionJ = 0;
                for (int j = 0; j < WidthSqrt; j++)
                {
                    regionCellIndex = 0;
                    maxRegionI = regionI + WidthSqrt;

                    Groups[regionNumber] = new SudokuCell[Width];

                    for (int ri = regionI; ri < maxRegionI; ri++)
                    {
                        maxRegionJ = regionJ + WidthSqrt;
                        for (int rj = regionJ; rj < maxRegionJ; rj++)
                        {
                            Groups[regionNumber][regionCellIndex++] = Cells[ri][rj];
                        }
                    }
                    regionNumber++;
                    regionJ += WidthSqrt;
                }
                regionI += WidthSqrt;
            }
        }

        public SudokuPuzzle Guess()
        {
#if DEBUG
            Program.NumberOfGuesses++;
#endif
            int iWinner = 0, jWinner = 0, least = int.MaxValue, possible;
            SudokuCell guessCell;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    guessCell = Cells[i][j];
                    if (guessCell.Value == 0)
                    {
                        possible = guessCell.NumberOfPossibilities;

                        if (possible == 2)
                        {
                            return new SudokuPuzzle(this, i, j, guessCell.GetFirstPossibility());
                        }
                        else if (possible < least)
                        {
                            least = guessCell.NumberOfPossibilities;
                            iWinner = i;
                            jWinner = j;
                        }
                    }
                }
            }

            return new SudokuPuzzle(this, iWinner, jWinner, Cells[iWinner][jWinner].GetFirstPossibility());
        }

        public void RemoveGuessAsPossibility(SudokuPuzzle guess)
        {
            Cells[guess.GuessI][guess.GuessJ].ForceRemovePossibility(guess.GuessValue);
        }

        public override string ToString()
        {
            double sqrt =  Math.Sqrt(Width);
            string ret = Environment.NewLine;
            bool valid, complete;
            SudokuValidator.Validate(this, out valid, out complete);

            ret += valid ? "Valid Puzzle, " : "Invalid Puzzle, ";
            ret += complete ? "(completed)" : "(incomplete)";
            ret += Environment.NewLine + Environment.NewLine;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    string value = Width <= 9 ? Cells[i][j].Value.ToString() : TestPuzzles.Upto36ReverseIndex[Cells[i][j].Value].ToString();
                    ret += value + (j + 1 != 0 && (j + 1) % sqrt == 0 ? "  " : " ");
                }
                ret += Environment.NewLine;
                if (i+1 != 0 && (i+1) % sqrt == 0) ret += Environment.NewLine;
            }
            return ret;
        }
    }
}
