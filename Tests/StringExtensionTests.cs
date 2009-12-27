using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sprocket;

namespace Tests
{
    [TestClass]
    public class StringExtensionTests
    {
        public Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestContext { get; set; }

        [TestMethod]
        public void StringExtensionTest1()
        {
            Assert.IsFalse("Hello".IsNullOrEmpty());
        }
        [TestMethod]
        public void StringExtensionTest2()
        {
            Assert.IsTrue("".IsNullOrEmpty());
        }
        [TestMethod]
        public void StringExtensionTest3()
        {
            string s = null;
            Assert.IsTrue(s.IsNullOrEmpty());
        }

        [TestMethod]
        public void StringExtensionTest4()
        {
            string s = "Hello World, Mom, and Dad";
            string r = s.ReplaceFirst("World", "Universe");
            Assert.IsTrue(r == "Hello Universe, Mom, and Dad");
        }
        [TestMethod]
        public void StringExtensionTest5()
        {
            string s = "One Two Two Three";
            string r = s.ReplaceFirst("One", "Ninja");
            Assert.IsTrue(r == "Ninja Two Two Three");
        }
        [TestMethod]
        public void StringExtensionTest6()
        {
            string s = "One Two Two Three";
            string r = s.ReplaceFirst("Two", "Ninja");
            Assert.IsTrue(r == "One Ninja Two Three");
        }

        [TestMethod]
        public void StringExtensionTest7()
        {
            string s = "123454321";
            Assert.IsTrue(s.CountOf('1') == 2);
        }
        [TestMethod]
        public void StringExtensionTest8()
        {
            string s = "123454321";
            Assert.IsTrue(s.CountOf('6') == 0);
        }
        [TestMethod]
        public void StringExtensionTest9()
        {
            string s = "123454321";
            Assert.IsTrue(s.CountOf('5') == 1);
        }
    }
}
