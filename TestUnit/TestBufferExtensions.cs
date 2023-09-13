using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cube.Utility;

namespace TestUnit
{
    internal class BufferUtilTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase(-1, new byte[] { })]
        [TestCase(-1, new byte[] { 0xf })]
        [TestCase(-1, new byte[] { 0x1c })]
        [TestCase(-1, new byte[] { 0x0d })]
        [TestCase(-1, new byte[] { 0x0d, 0x1c })]
        [TestCase(0, new byte[] { 0x1c, 0x0d })]
        [TestCase(1, new byte[] { 0x1, 0x1c, 0x0d })]
        [TestCase(3, new byte[] { 0x1, 0x1c, 0x2, 0x1c, 0x0d })]
        public void Test_FirstOf(int expected, byte[] sequence)
        {
            var delimiter = new byte[] { 0x1c, 0x0d };
            var seq = new ReadOnlySequence<byte>(sequence);
            var p = seq.FirstOf(delimiter);

            var res = p == null ? -1 : p.Value.GetInteger();

            Assert.AreEqual(expected, res);
        }

        [Test]
        [TestCase(new byte[] { }, "")]
        [TestCase(new byte[] { 0xf }, "0F")]
        [TestCase(new byte[] { 0xff }, "FF")]
        [TestCase(new byte[] { 1, 2 }, "0102")]
        [TestCase(new byte[] { 1, 2, 3 }, "010203")]
        public void Test_ToHex(byte[] bytes, string expect)
        {
            var sequence = new ReadOnlySequence<byte>(bytes);
            var result = sequence.ToHex();

            Assert.AreEqual(expect, result);
        }
        
        [Test]
        [TestCase(new byte[] { }, "")]
        [TestCase(new byte[] { 0xf }, "0F")]
        [TestCase(new byte[] { 0xff }, "FF")]
        [TestCase(new byte[] { 1, 2 }, "0102")]
        [TestCase(new byte[] { 1, 2, 3 }, "010203")]
        public void Test_ToHex2(byte[] bytes, string expect)
        {
            var sequence = new MemorySequence<byte>(bytes);
            var result = sequence.ToHex();

            Assert.AreEqual(expect, result);
        }
        
    }
}