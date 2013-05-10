using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyntaxDiff;

namespace Tests
{
    [TestClass]
    public class TreeMatching
    {
        [TestMethod]
        [TestCategory("TreeMatching")]
        public void TestTreeMatching()
        {
            var b = new Tree<int?>(4, 3, 7, new Tree<int?>(2, 1, 0));
            var l = new Tree<int?>(4, 3, new Tree<int?>(10, 1, 0), 9);

            Func<int?, string> getLabel = x => x.ToString();
            Func<TreeDiff<int?>.MergeTreeNode, string> getLabelMt = x => getLabel(x.value);

            var mTree = JavaMatching<int?>.getMappingTree(l, b, getLabel);

        }

        [TestMethod]
        [TestCategory("TreeMatching")]
        public void TestTreeMatching2()
        {
            var b = new Tree<int?>(4, 3, new Tree<int?>(7, 15, 16), new Tree<int?>(2, 1, 0));
            var l = new Tree<int?>(4, 3, new Tree<int?>(10, 1, 0), new Tree<int?>(9, 5, 6));

            Func<int?, string> getLabel = x => x.ToString();

            var mTree = JavaMatching<int?>.getMappingTree(l, b, getLabel);

        }

        [TestMethod]
        [TestCategory("TreeMatching")]
        public void TestTreeMatching3()
        {
            var b = new Tree<int?>(4, new Tree<int?>(7, new Tree<int?>(2, new Tree<int?>(3), new Tree<int?>(2, new Tree<int?>(3)))));
            var l = new Tree<int?>(4, new Tree<int?>(2, new Tree<int?>(3)));

            Func<int?, string> getLabel = x => x.ToString();

            var mTree = JavaMatching<int?>.getMappingTree(l, b, getLabel);

        }



        [TestMethod]
        [TestCategory("TreeMatching")]
        public void TestTreeMatching4()
        {
            var b = new Tree<string>("Invocation",
                        new Tree<string>("MemberAccess",
                            new Tree<string>("Console"),
                            new Tree<string>("Writeline")),
                        new Tree<string>("ArgumentList",
                                new Tree<string>("Hello")
                                ));

            var l = new Tree<string>("Variable",
                        new Tree<String>("VarId"),
                        new Tree<string>("VariableD",
                            new Tree<string>("equals", new Tree<string>("2"))));

            Func<string, string> getLabel = x => x;

            var mTree = Tree<string>.Match(b, l, getLabel);

        }
    }
}
