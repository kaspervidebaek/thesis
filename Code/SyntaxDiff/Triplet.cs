using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{

        public class Triplet<T>
        {
            public T A, O, B;

            public static Triplet<Y> Create<Y>(Y A, Y O, Y B) 
            {
                return new Triplet<Y> { A = A, O = O, B = B };
            }

            public Triplet<Y> Cast<Y>() 
                where Y : T
            {
                return new Triplet<Y> { A = (Y)A, O = (Y)O, B = (Y)B };
            }

            public bool Is<Y>() 
            {
                return A is Y && O is Y && B is Y;
            }

            public Triplet<Y> Select<Y>(Func<T, Y> f)
            {
                return new Triplet<Y> { A = f(A), O = f(O), B = f(B) };
            }

            public Y Apply<Y>(Func<Triplet<T>, Y> f)
            {
                return f(this);
            }

            public Y Apply<Y>(Func<T, T, T, Y> f)
            {
                return f(A, O, B);
            }


            public Y ApplyIfExists<Y>(Func<T, T, T, Y> f)
            {
                if(A != null && O != null && B != null)
                    return f(A, O, B);
                return default(Y);
            }
        }



}
