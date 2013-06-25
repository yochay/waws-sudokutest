using System.Threading.Tasks;
using System;
namespace SudokuSolver2010
{
    public class SudokuSolver
    {
        public static SudokuSolver DefaultSolver = new SudokuSolver();

        protected SudokuSolver() { }

        public void Solve(SudokuPuzzle[] input, SudokuPuzzle[] solutions)
        {
            SudokuHeuristics h = new SudokuHeuristics(input[0]);
            for (int i = 0; i < input.Length; i++) solutions[i] = Solve(input[i], h);
        }

        public void SolveParallel(SudokuPuzzle[] input, SudokuPuzzle[] solutions)
        {
            Parallel.ForEach<SudokuPuzzle>(input, puzzle => { solutions[puzzle.OriginalIndex] = Solve(puzzle, new SudokuHeuristics(puzzle)); });
        }

        public SudokuPuzzle Solve(SudokuPuzzle input, SudokuHeuristics heuristics)
        {
            bool isComplete, isValid;
            
            // Cannot share the same SudokuHeuristics object across threads so we must create one each time solve is called.
            heuristics.ResolvePuzzle(input, out isValid, out isComplete);
            if (isComplete) return input;
            
            SudokuPuzzle current = input;
            SudokuPuzzle[] guessStack = new SudokuPuzzle[input.Width * input.Width];
            
            int topOfStack = 0;
            guessStack[topOfStack] = input;
            guessStack[++topOfStack] = current.Guess();

            while (true)
            {
                heuristics.ResolvePuzzle(current = guessStack[topOfStack], out isValid, out isComplete);
                if (isComplete) return current;
                
                if (isValid == false) guessStack[--topOfStack].RemoveGuessAsPossibility(current);
                else                  guessStack[++topOfStack] = current.Guess();
            }
        }
    }
}
