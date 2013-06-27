using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{

    public class Triplet<T, U>
    {
        public T bas, even;
        public U other;
    }

    [DebuggerNonUserCode]
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

        public Triplet<Y, Z> Cast<Y, Z>()
            where Y : T
            where Z : T
        {
            if (A is Y && O is Y && B is Z)
                return new Triplet<Y, Z> { bas = (Y)O, even = (Y)A, other = (Z)B };
            if (A is Z && O is Y && B is Y)
                return new Triplet<Y, Z> { bas = (Y)O, even = (Y)B, other = (Z)A };
            throw new Exception();
        }

        public bool Is<Y>()
        {
            return A is Y && O is Y && B is Y;
        }

        public bool Is<Y, Z>()
        {
            return  (A is Y && O is Y && B is Z) ||
                    (A is Z && O is Y && B is Y);
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

    public class Doublet<T>
    {
        public T X, Y;

        public static Doublet<U> Create<U>(U X, U Y)
        {
            return new Doublet<U> { X = (U)X, Y = (U)Y };
        }
        public Doublet<Y> Cast<Y>()
            where Y : T
        {
            return new Doublet<Y> { X = (Y)X, Y = (Y)this.Y };
        }

        public bool Is<Y>()
        {
            return X is Y && this.Y is Y;
        }
        public bool Is<X, Y>()
        {
            return this.X is X && this.Y is Y;
        }

        public Doublet<Y> Select<Y>(Func<T, Y> f)
        {
            return new Doublet<Y> { X = f(X), Y = f(this.Y)};
        }

        public Y Apply<Y>(Func<Doublet<T>, Y> f)
        {
            return f(this);
        }

        public Y Apply<Y>(Func<T, T, Y> f)
        {
            return f(X, this.Y);
        }

        public Y ApplyIfExists<Y>(Func<T, T, Y> f)
        {
            if (X != null && this.Y != null)
                return f(X, this.Y);
            return default(Y);
        }
    }


}
