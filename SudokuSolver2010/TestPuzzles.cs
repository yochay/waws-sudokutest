using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace SudokuSolver2010
{
    class TestPuzzles
    {
        public static SudokuPuzzle[] GetPuzzles(string resourceName)
        {
            PropertyInfo prop = typeof(Resources).GetProperty(resourceName, BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null) throw new ArgumentException("No property called " + resourceName);
            string puzzleText = prop.GetValue(null, null) as string;

            if (puzzleText.Contains(" "))
            {
                return new SudokuPuzzle[] { ParsePuzzleWithNumbers(puzzleText) };
            }

            return ParsePuzzles(puzzleText);
        }

        private static SudokuPuzzle[] ParsePuzzles(string inputString)
        {
            string[] inputStrings = inputString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            SudokuPuzzle[] ret = new SudokuPuzzle[inputStrings.Length];

            for (int puz = 0; puz < inputStrings.Length; puz++)
            {
                string contents = inputStrings[puz];

                ret[puz] = ParsePuzzle(contents);
                ret[puz].OriginalIndex = puz;
            }
            return ret;
        }

        private static SudokuPuzzle ParsePuzzleWithNumbers(string inputString)
        {
            var numbers = (from c in inputString.Split(' ', '\n') where !string.IsNullOrWhiteSpace(c) select int.Parse(c)).ToArray();
            int w = (int)Math.Sqrt(numbers.Length);
            int[][] vals = new int[w][];
            int index = 0;
            for (int i = 0; i < w; i++)
            {
                vals[i] = new int[w];
                for (int j = 0; j < w; j++)
                {
                    vals[i][j] = numbers[index++];
                }
            }

            return new SudokuPuzzle(vals);
        }

        private static SudokuPuzzle ParsePuzzle(string inputString)
        {
            int width = (int)Math.Sqrt(inputString.Length);

            int[][] values = new int[width][];

            int index = 0;
            for (int i = 0; i < width; i++)
            {
                values[i] = new int[width];
                for (int j = 0; j < width; j++)
                {
                    values[i][j] = width <= 9 ? int.Parse(inputString[index] + "") : UpTo36Index[inputString[index]];

                    index++;
                }
            }

            return new SudokuPuzzle(values);
        }

        private static Dictionary<char, int> UpTo36Index;

        public static Dictionary<int, char> Upto36ReverseIndex;

        private static Dictionary<int, char> ReverseUpTo36Index()
        {
            Dictionary<int, char> ret = new Dictionary<int, char>();
            foreach (char key in UpTo36Index.Keys)
            {
                ret.Add(UpTo36Index[key], key);
            }

            return ret;
        }

        static TestPuzzles()
        {
            UpTo36Index = new Dictionary<char, int>
            {
                {'0',0},
                {'A',1},
                {'B',2},
                {'C',3},
                {'D',4},
                {'E',5},
                {'F',6},
                {'G',7},
                {'H',8},
                {'I',9},
                {'J',10},
                {'K',11},
                {'L',12},
                {'M',13},
                {'N',14},
                {'O',15},
                {'P',16},
                {'Q',17},
                {'R',18},
                {'S',19},
                {'T',20},
                {'U',21},
                {'V',22},
                {'W',23},
                {'X',24},
                {'Y',25},
                {'Z',26},
                {'+',27},
                {'1',28},
                {'2',29},
                {'3',30},
                {'4',31},
                {'5',32},
                {'6',33},
                {'7',34},
                {'8',35},
                {'9',36},
            };

            Upto36ReverseIndex = ReverseUpTo36Index();
        }
    }
}
     