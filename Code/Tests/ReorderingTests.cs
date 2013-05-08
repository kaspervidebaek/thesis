using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using System.Linq;
using SyntaxDiff;
using System.Diagnostics;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class ReorderingTests
    {
        [TestMethod]
        [TestCategory("Reordering")]
        public void TestReorderReorder()
        {
            var a = "A";
            var b = "B";
            var c = "C";
            var d = "D";


            var A = new List<string> { a, c, b, d };
            var O = new List<string> { a, b, c, d };
            var B = new List<string> { d, b, c, a };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B);

            var result = new List<string> { d, c, b, a };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));

        }

        [TestMethod]
        [TestCategory("Reordering")]
        public void TestReorderInsert()
        {
            var a = "A";
            var b = "B";
            var c = "C";
            var z = "Z";


            var A = new List<string> { a, c, b };
            var O = new List<string> { a, b, c };
            var B = new List<string> { a, z, b, c };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B);

            var result = new List<string> { a, z, c, b };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));

        }

        [TestMethod]
        [TestCategory("Reordering")]
        public void TestInsertTwice()
        {
            var a = "A";
            var b = "B";
            var z = "Z";
            var y = "Y";


            var A = new List<string> { a, z, b };
            var O = new List<string> { a, b };
            var B = new List<string> { a, y, b };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B);

            var result = new List<string> { a, y, z, b };
            var result2 = new List<string> { a, z, y, b };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result)
                || Enumerable.SequenceEqual(newOrder, result2));

        }

        [TestMethod]
        [TestCategory("Reordering")]
        public void TestReorderDelete()
        {
            var a = "A";
            var b = "B";
            var c = "C";


            var A = new List<string> { c, b, a };
            var O = new List<string> { a, b, c };
            var B = new List<string> { a, c };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B);

            var result = new List<string> { c, a };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));
        }

        [TestMethod]
        [TestCategory("Reordering")]
        public void TestReorderDelete2()
        {
            var a = "A";
            var b = "B";
            var c = "C";


            var A = new List<string> { c, b, a };
            var O = new List<string> { a, b, c };
            var B = new List<string> { b, c };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B);

            var result = new List<string> { c, b };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));
        }

        [TestMethod]
        [TestCategory("Reordering")]
        public void TestDeleteTwice()
        {
            var a = "A";
            var b = "B";
            var c = "C";


            var A = new List<string> { a, c };
            var O = new List<string> { a, b, c };
            var B = new List<string> { a, b };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B);

            var result = new List<string> { a };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));
        }


        [TestMethod]
        [TestCategory("Reordering")]
        public void TestDeleteInsert()
        {
            var a = "A";
            var b = "B";
            var c = "C";


            var A = new List<string> { a };
            var O = new List<string> { a, c };
            var B = new List<string> { a, b };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B);

            var result = new List<string> { a, b };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));
        }

        [TestMethod]
        [TestCategory("Reordering")]
        public void TestReorderingSameItem()
        {
            List<Tuple<int, int>> conflicts;

            var a = "A";
            var b = "B";
            var c = "C";
            var d = "D";
            var e = "E";


            var A = new List<string> { b, a, e, c, d };
            var O = new List<string> { a, e, b, c, d };
            var B = new List<string> { a, e, c, d, b };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B, out conflicts);

            var result = new List<string> { b, a, e, c, d, b };
            
            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));
            Assert.IsTrue(conflicts[0].Item1 == 0 && conflicts[0].Item2 == 5);
        }


        [TestMethod]
        [TestCategory("Reordering")]
        public void TestReorderingExample()
        {
            List<Tuple<int, int>> conflicts;

            var a = "A";
            var b = "B";
            var c = "C";
            var d = "D";
            var z = "Z";
            var y = "Y";


            var A = new List<string> { b, y, a, c, d };
            var O = new List<string> { a, b, c, d };
            var B = new List<string> { a, z, b, c, d };

            var newOrder = SyntaxDiff.Reorder<String>.OrderLists(A, O, B, out conflicts);
            /*
            var result = new List<string> { b, a, e, c, d, b };

            Assert.IsTrue(Enumerable.SequenceEqual(newOrder, result));
            Assert.IsTrue(conflicts[0].Item1 == 0 && conflicts[0].Item2 == 5);*/
        }
    }
}
