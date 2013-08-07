using System;
using System.Collections.Generic;

namespace SyntaxDiff
{
    public class NeedlemanWunsch<T>
    {
        public static List<Tuple<T, T>> Allignment(List<T> A, List<T> B, Func<T, T, bool> equal)
        {
            return NeedlemanWunsch<T, T>.Allignment(A, B, equal);
        }
    }
    public class NeedlemanWunsch<T, U> 
    {

        static Func<int, int, int> Sg(List<T> A, List<U> B, Func<T, U, bool> equal)
        {
            return  (i, j) => {
                if (equal(A[i-1], B[j-1]))
                    return -1;
                return 1;
            };
        }

        static int d = 0;
        public static int[,] AllignmentMatrix(List<T> A, List<U> B, Func<T, U, bool> equal)
        {
            var S = Sg(A, B, equal);

            int[,] F = new int[A.Count+1, B.Count+1];
            for (int i = 0; i < A.Count; i++)
            {
                F[i, 0] = d * i;
            }
            for (int j = 0; j < B.Count; j++)
            {
                F[0, j] = d * j;
            }

            for (int i = 1; i <= A.Count; i++)
            {
                for (int j = 1; j <= B.Count; j++)
                {
                    var match = F[i - 1, j - 1] + S(i, j);
                    var delete = F[i - 1, j] + d;
                    var insert = F[i, j - 1] + d;
                    F[i, j] = Math.Min(Math.Min(match, insert), delete);
                }
            }
            return F;
        }

        public static List<Tuple<T, U>> Allignment(List<T> A, List<U> B, Func<T, U, bool> equal)
        {
            var S = Sg(A, B, equal);
            var F = AllignmentMatrix(A, B, equal);

            var Allignment = new List<Tuple<T, U>>();
            
            int i = A.Count;
            int j = B.Count;
            
            while (i > 0 || j > 0)
            {
                if (i > 0 && j > 0 && F[i, j] == (F[i - 1, j - 1] + S(i, j)))
                {
                    Allignment.Add(Tuple.Create(A[i-1], B[j-1]));

                    i--;
                    j--;
                }
                else if (i > 0 && F[i, j] == F[i - 1, j] + d)
                {
                    Allignment.Add(Tuple.Create(A[i - 1], default(U)));
                    i--;
                }
                else //if (j > 0 && F[i, j] == F[i, j - 1] + d)
                {
                    Allignment.Add(Tuple.Create(default(T), B[j - 1]));
                    j--;
                }
            }
            Allignment.Reverse();
            return Allignment;
        }


    }
}