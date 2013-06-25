using System;

namespace SudokuSolver2010
{
    public static class SudokuValidator
    {
        public static bool ValidateSolutions(SudokuPuzzle[] originalPuzzles, SudokuPuzzle[] solutions)
        {
            bool valid, complete;
            for (int i = 0; i < originalPuzzles.Length; i++)
            {
                SudokuPuzzle puzzle = originalPuzzles[i];
                SudokuPuzzle solution = solutions[i];
                Validate(solution, out valid, out complete);
                if (!complete)
                {
                    Console.WriteLine(solution);
                    return false;
                }

                if (!valid)
                {
                    Console.WriteLine(solution);
                    return false;
                }

                if (!EqualsIgnoreZeros(puzzle, solution))
                {
                    Console.WriteLine("This solution does not match the corresponding puzzle");
                    Console.WriteLine(solution);
                    Console.WriteLine(puzzle);
                    return false;
                }
            }
            return true;
        }

        public static void Validate(SudokuPuzzle puzzle, out bool isValid, out bool isComplete)
        {
            isValid = IsValid(puzzle);
            if (!isValid) isComplete = false;
            else isComplete = IsComplete(puzzle);
        }

        public static bool EqualsIgnoreZeros(SudokuPuzzle a, SudokuPuzzle b)
        {
            for (int i = 0; i < a.Width; i++)
            {
                for (int j = 0; j < a.Width; j++)
                {
                    if (a.Groups[i][j].Value == 0 || b.Groups[i][j].Value == 0) continue;
                    if (a.Groups[i][j].Value != a.Groups[i][j].Value) return false;
                }
            }
            return true;
        }

        public static bool IsValid(SudokuPuzzle puzzle)
        {
            ulong found;
            for (int i = 0; i < puzzle.Groups.Length; i++)
            {
                found = 0;
                for (int j = 0; j < puzzle.Width; j++)
                {
                    if (puzzle.Groups[i][j].Value == 0) continue;
                    if ((SudokuCell.BitMasks[puzzle.Groups[i][j].Value] & found) != 0) return false;
                    found = found | SudokuCell.BitMasks[puzzle.Groups[i][j].Value];
                }
            }
            return true;
        }

        public static bool IsComplete(SudokuPuzzle puzzle)
        {
            for (int i = 0; i < puzzle.Width; i++)
            {
                for (int j = 0; j < puzzle.Width; j++)
                {
                    if (puzzle.Groups[i][j].Value == 0) return false;
                }
            }

            return true;
        }
    }
}
