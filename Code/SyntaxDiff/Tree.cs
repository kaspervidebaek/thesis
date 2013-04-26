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

        public override string ToString()
        {
            return value.ToString();
        }

        public static implicit operator Tree<T>(T i)
        {
            return new Tree<T>(i);
        }
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
