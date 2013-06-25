using System;
using System.Collections.Generic;
using SudokuSolver2010;

namespace Sudoku.Models
{
    public class SudokuSolutionModel
    {
        public int NumberOfPuzzles { get; set; }
        public double TotalSolveTimeInMilliseconds { get; set; }
        public string Error { get; set; }

        public SudokuSolution[] Solutions { get; set; }

        public SudokuSolutionModel()
        {
 
        }
    }

    public class SudokuSolution
    {
        public bool IsValid { get; set; }
        public bool IsComplete { get; set; }
        public string PuzzleText { get; set; }
        public string SolutionText { get; set; }
        public double SolveTimeInMilliseconds { get; set; }
        public string Error { get; set; }
        internal SudokuPuzzle Solution { get; set; }

        public int Width
        {
            get
            {
                return Solution.Width;
            }
        }

        public int WidthSqrt
        {
            get
            {
                return (int)Math.Sqrt(Solution.Width);
            }
        }
    }
}