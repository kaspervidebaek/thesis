using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class Tree<T>
    {
        public T value;
        public List<Tree<T>> children;

        public Tree(T v, params Tree<T>[] children)
        {
            value = v;
            this.children = children.ToList();
        }

        public Tree(T v, params T[] children)
        {
            value = v;
            this.children = children.Select(x => new Tree<T>(x)).ToList();
        }

        public Tree(T v, params Object[] children)
        {
            value = v;
            this.children = children.Select(x =>
            {
                if (x is T)
                    return new Tree<T>((T)x);
                else if (x is Tree<T>)
                    return (Tree<T>)x;
                throw new Exception();
            }
                ).ToList();
        }


        public override string ToString()
        {
            return value.ToString();
        }

        public Tree<Y> Convert<Y>(Func<T, Y> c)
        {
            var newchildren = this.children.Select(x => x.Convert(c));
            return new Tree<Y>(c(value), newchildren.ToArray());
        }

        public List<T> PostOrderEnumeration()
        {
            var rv = new List<T>();

            foreach (var child in children.Reverse<Tree<T>>())
            {
                rv.AddRange(child.PostOrderEnumeration());
            }

            rv.Add(this.value);
            return rv;
        }

        /*
        public static implicit operator Tree<T>(T i)
        {
            return new Tree<T>(i);
        }*/
    }

    public static class TreeExtensions
    {
        public static List<Tree<T>> getChildren<T>(this Tree<T> c)
        {
            if (c != null)
                return c.children;
            else return new List<Tree<T>>();
        }
    }
}
