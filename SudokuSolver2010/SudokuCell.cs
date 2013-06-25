using System;
using System.Linq;

namespace SudokuSolver2010
{
    public class SudokuCell
    {
        
        public static readonly ulong[] BitMasks = MakeBitMasks();
        private static ulong[] DefaultBitMaskCache = new ulong[65];
        private static ulong[] MakeBitMasks()
        {
            ulong[] ret = new ulong[65];
            ulong current = 1;

            // start this loop at 1 so that BitMasks[1] == 1 0x0001
            //                              BitMasks[2] == 2 0x0010
            //                              BitMasks[3] == 4 0x0100
            //                              ...
            for (ulong i = 1; i < 65; i++)
            {
                ret[i] = current;
                current = current * 2;
            }
            return ret;
        }

        public int Value;
        internal ulong Possibilities;
        internal int NumberOfPossibilities;
        int Width;

        public SudokuCell(int width)
        {
            Width = width;
            NumberOfPossibilities = width;  // if this is a 9 X 9 puzzle, each cell has 9 possibilities
            Value = 0;
            ulong cachedValue = DefaultBitMaskCache[width];

            if (cachedValue == 0)
            {
                Possibilities = 0xFFFFFFFFFFFFFFFF;
                for (int i = width + 1; i < BitMasks.Length; i++) Possibilities = Possibilities ^ BitMasks[i];
                DefaultBitMaskCache[width] = Possibilities;

                // At this point, for a 9 X 9 puzzle the Possibilities should be:
                // 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0001 1111 1111
                // For a 16 X 16 puzzle the Possibilities should be:
                // 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 1111 1111 1111 1111
            }
            else
            {
                Possibilities = cachedValue;
            }
        }

        public SudokuCell(int width, int value)
        {
            Value = value;
        }

        internal void ForceRemovePossibility(int i)
        {
            Possibilities = Possibilities ^ BitMasks[i];
            NumberOfPossibilities--;

            if (NumberOfPossibilities == 1)
            {
                int alternate = 0;
                while ((Possibilities >> alternate) % 2 == 0) alternate++;
                Value = alternate + 1;
            }
        }

        internal void TryToRemovePossibility(int i, ref int workCounter)
        {
            if ((Possibilities & BitMasks[i]) != BitMasks[i]) return;
            Possibilities = Possibilities ^ BitMasks[i];
            NumberOfPossibilities--;
            workCounter++;
            if (NumberOfPossibilities == 1)
            {
                int alternate = 0;
                while( (Possibilities >> alternate) %2 == 0)    alternate++;
                Value = alternate+1;
            }
        }

        public void GetTwoRemainingValues(int width, ref int a, ref int b)
        {
            bool foundOne = false;
            for (int i = 1; i <= width; i++)
            {
                if ((Possibilities & BitMasks[i]) == BitMasks[i])
                {
                    if (foundOne)
                    {
                        b = i;
                        return;
                    }
                    else
                    {
                        a = i;
                        foundOne = true;
                    }
                }
            }

            throw new Exception("Bug - there are not 2 remaining values");
        }

        internal int[] GetAllRemainingValues(int width)
        {
            int[] ret = new int[NumberOfPossibilities];
            int index = 0;

            ulong mask;
            for (int i = 1; i <= width; i++)
            {
                mask = BitMasks[i];
                if ((Possibilities & mask) == mask)
                {
                    ret[index++] = i;
                    if (index == NumberOfPossibilities) return ret;
                }
            }

            throw new Exception("OOPS");
        }

        internal void GetThreeRemainingValues(int width, ref int a, ref int b, ref int c)
        {
            ulong mask;
            bool foundOne = false;
            bool foundTwo = false;
            for (int i = 1; i <= width; i++)
            {
                mask = BitMasks[i];
                if ((Possibilities & mask) == mask)
                {
                    if (foundOne && foundTwo)
                    {
                        c = i;
                        return;
                    }
                    else if (foundOne)
                    {
                        b = i;
                        foundTwo = true;
                    }
                    else
                    {
                        a = i;
                        foundOne = true;
                    }
                }
            }
        }

        public int GetFirstPossibility()
        {
            int alternate = 0;
            while ((Possibilities >> alternate) % 2 == 0) alternate++;
            return alternate + 1;
        }

        public string ULongToString()
        {
            string ret = string.Empty;
            byte[] bytes = BitConverter.GetBytes(Possibilities);

            foreach (byte b in bytes.Reverse())
            {
                string s = Convert.ToString(b, 2);
                while (s.Length < 8) s = "0" + s;
                ret += s + " ";
            }
            return ret;
        }

        public override string ToString()
        {
            if (Value != 0) return Value + "";
            else return ULongToString();
        }

        internal void GetFourRemainingValues(int width, ref int a, ref int b, ref int c, ref int d)
        {
            ulong mask;
            bool foundOne = false;
            bool foundTwo = false;
            bool foundThree = false;
            for (int i = 1; i <= width; i++)
            {
                mask = BitMasks[i];
                if ((Possibilities & mask) == mask)
                {
                    if (!foundOne)
                    {
                        a = i;
                        foundOne = true;
                    }
                    else if (!foundTwo)
                    {
                        b = i;
                        foundTwo = true;
                    }
                    else if (!foundThree)
                    {
                        c = i;
                        foundThree = true;
                    }
                    else
                    {
                        d = i;
                        return;
                    }
                }
            }
        }
    }
}
