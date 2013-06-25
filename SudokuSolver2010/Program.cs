using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SudokuSolver2010
{
    static class ArrayExtensions
    {
        public static SudokuPuzzle[] CopyPuzzles(this SudokuPuzzle[] puzzles)
        {
            return (from p in puzzles select new SudokuPuzzle(p)).ToArray();
        }
    }

    class Program
    {
        public static int NumberOfGuesses = 0;
        public static int RuleOfKCount = 0;
        public static int RuleOfKSuccess;
        public static int HiddenValueCount = 0;
        public static int HiddenValueSuccess = 0;


        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            string key = args.Length == 0 ? "Hard2365" : args[0];
            SudokuPuzzle[] puzzles = TestPuzzles.GetPuzzles(key);
            SudokuPuzzle[] solutions = new SudokuPuzzle[puzzles.Length];
            Console.WriteLine("solving...");

            SudokuPuzzle[] toSolve = puzzles.CopyPuzzles();
            sw.Start();
            SudokuSolver.DefaultSolver.Solve(puzzles, solutions);
            sw.Stop();

            Console.WriteLine("Validating...");
            SudokuValidator.ValidateSolutions(puzzles, solutions);

            double per = (sw.ElapsedMilliseconds / (double)puzzles.Length);
            per = Math.Round(per, 2);
            
            Log("Done.  Solving "+puzzles.Length+" "+key + " took " + sw.ElapsedMilliseconds + " ms ("+per+" ms per puzzle)");

            File.WriteAllText(@"C:\logs\answer.txt", solutions[0].ToString());

            string dir = args.Length < 2 ? @"C:\logs" : args[1];
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);

            string file = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss-tt ") +key+" "+ puzzles.Length+"("+Math.Round(sw.Elapsed.TotalSeconds,3)+" s).txt";
            string path = Path.Combine(dir, file);
            File.WriteAllLines(path, log.ToArray());

#if DEBUG

            Console.WriteLine("Total Guesses: " + Program.NumberOfGuesses);
            Console.WriteLine("Rule of K Success: " + RuleOfKSuccess + " / " + RuleOfKCount + " (" + Math.Round(100.0 * RuleOfKSuccess / RuleOfKCount, 2)+" %)");
            Console.WriteLine("Hidden Value Success: " + HiddenValueSuccess + " / " + HiddenValueCount + " (" + Math.Round(100.0 * HiddenValueSuccess / HiddenValueCount, 2) + " %)");
#endif
            Console.WriteLine(solutions[0]);
            if (Debugger.IsAttached) Console.ReadKey();
        }

        static List<string> log = new List<string>();
        private static void Log(object entry)
        {
            log.Add(entry.ToString());
            Console.WriteLine(entry);
        }

        public static void CountRuleOfK(bool success)
        {
            if(success) RuleOfKSuccess++;
            RuleOfKCount++;
        }

        public static void CountHiddenValueAttempt(bool success)
        {
            if (success) HiddenValueSuccess++;
            HiddenValueCount++;
        }
    }
}
