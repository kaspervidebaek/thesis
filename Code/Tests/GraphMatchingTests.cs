﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class GraphMatchingTests
    {
    
        [TestMethod]
        public void TestTotalMatch()
        {
            var xs = new List<int?>(new int?[] { 1, 2, 7 });
            var ys = new List<int?>(new int?[] { 1, 2, 5 });
            var match = SyntaxDiff.GraphMatching<int?, int?>.Match(xs, ys, (x, y) => Math.Abs(x.Value - y.Value));

            Assert.IsTrue(match.Count == 3);
            var e = match.GetEnumerator();
            e.MoveNext();

            Assert.IsTrue(e.Current.Item1 == 1 && e.Current.Item2 == 1); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == 2 && e.Current.Item2 == 2); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == 7 && e.Current.Item2 == 5);

        }

        [TestMethod]
        public void TestMissingYs()
        {
            var xs = new List<int?>(new int?[] { 1, 2, 3 });
            var ys = new List<int?>(new int?[] { 1, 2 });
            var match = SyntaxDiff.GraphMatching<int?, int?>.Match(xs, ys, (x, y) => Math.Abs(x.Value - y.Value));

            Assert.IsTrue(match.Count == 3);
            var e = match.GetEnumerator();
            e.MoveNext();

            Assert.IsTrue(e.Current.Item1 == 1 && e.Current.Item2 == 1); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == 2 && e.Current.Item2 == 2); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == 3 && e.Current.Item2 == null);

        }

        [TestMethod]
        public void TestMissingXs()
        {
            var xs = new List<int?>(new int?[] { 1, 2 });
            var ys = new List<int?>(new int?[] { 1, 2, 3 });
            var match = SyntaxDiff.GraphMatching<int?, int?>.Match(xs, ys, (x, y) => Math.Abs(x.Value - y.Value));

            Assert.IsTrue(match.Count == 3);
            var e = match.GetEnumerator();
            e.MoveNext();

            Assert.IsTrue(e.Current.Item1 == 1 && e.Current.Item2 == 1); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == 2 && e.Current.Item2 == 2); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == null && e.Current.Item2 == 3);
        }

        [TestMethod]
        public void TestNoLinks()
        {
            var xs = new List<int?>(new int?[] { 10, 11 });
            var ys = new List<int?>(new int?[] { 1, 2, 3 });
            var match = SyntaxDiff.GraphMatching<int?, int?>.Match(xs, ys, (x, y) => {
                var dist = Math.Abs(x.Value - y.Value);
                return dist < 8 ? dist : (int?)null;
            });

        }
        [TestMethod]
        public void TestNullMatch()
        {
            var xs = new List<string>(new string[] { "1", "2", "3" }); // Use string to force out nullpointer exceptions
            var ys = new List<string>(new string[] { "1", "2" });
            var match = SyntaxDiff.GraphMatching<string, string>.Match(xs, ys, (x, y) =>
            {
                return x != "3" ? 1 : (int?)null;
            });

            Assert.IsTrue(match.Count == 3);
            var e = match.GetEnumerator();
            e.MoveNext();

            Assert.IsTrue(e.Current.Item1 == "1" && e.Current.Item2 == "1"); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == "2" && e.Current.Item2 == "2"); e.MoveNext();
            Assert.IsTrue(e.Current.Item1 == "3" && e.Current.Item2 == null);
        }
    }
}