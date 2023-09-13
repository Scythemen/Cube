using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using Cube.Utility;
using Microsoft.Extensions.Logging;
using TestCommon;

namespace TestUnit
{
    internal class TestBytesHelper
    {
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = LoggerInitializer.InitLogger<TestBytesHelper>();
        }


        [Test]
        [TestCase("0001020304", new byte[] { 00, 01, 02, 03, 04 })]
        [TestCase("00010203", new byte[] { 00, 01, 02, 03 })]
        [TestCase("", new byte[] { })]
        [TestCase(null, new byte[] { })]
        [TestCase("0", new byte[] { 0 })]
        [TestCase("11", new byte[] { 0x11 })]
        [TestCase("123", new byte[] { 0x01, 0x23 })]
        public void Test_HexToBytes(string hex, byte[] expect)
        {
            var result = hex.HexToBytes();

            Assert.IsTrue(result.SequenceEqual(expect));
        }


        [Test]
        [TestCase("0001020304", "00 01 02 03 04")]
        [TestCase("00010203", "00 01 02 03")]
        [TestCase("0001020", "00 01 02 0")]
        [TestCase("", "")]
        [TestCase(null, null)]
        [TestCase("0", "0")]
        [TestCase("11", "11")]
        [TestCase("123", "12 3")]
        public void Test_InsertSpace(string hex, string expect)
        {
            var result = hex.InsertSpace();
            //  _logger.LogTrace("insert space result: {}", result);

            Assert.IsTrue(result == expect);
        }


        // [Test]
        // [TestCase(1, 0, new byte[] { 0x89 })]
        // [TestCase(1, 1, new byte[] { 0x97 })]
        // [TestCase(2, 1, new byte[] { 0x38, 0x97 })]
        // [TestCase(1, 2, new byte[] { 0x74 })]
        // [TestCase(3, 0, new byte[] { 0x01, 0x23, 0x89 })]
        // [TestCase(6, 7, new byte[] { 0x12, 0x38, 0x97, 0x46, 0x58, 0x00 })] // 12389.7465800
        // [TestCase(5, 3, new byte[] { 0x00, 0x12, 0x38, 0x97, 0x46 })] //  0012389.746
        // [TestCase(7, 7, new byte[] { 0x00, 0x12, 0x38, 0x97, 0x46, 0x58, 0x00 })]  // 0012389.7465800
        // public void Test_bcd2bytes(int bLength, int dPlace, byte[] expected)
        // {
        //     var val = "12389.74658";
        //     var res = Utils.BCD2bytes(val, bLength, dPlace);
        //
        //     Debug.WriteLine("result: " + res.ToHex());
        //     Debug.WriteLine("expected: " + expected.ToHex() + "\r\n");
        //
        //     Assert.AreEqual(expected, res);
        //
        // }
        //
        // [TestCase(2, 7, new byte[] { 0x00, 0x12, 0x38, 0x97, 0x46, 0x58, 0x00 })]  // 0012389.7465800
        // public void Test_bcd2bytes_index(int bLength, int dPlace, byte[] expected)
        // {
        //     var val = "12389.74658";
        //     var res = Utils.BCD2bytes(val, bLength, dPlace);
        //
        //     Debug.WriteLine("result: " + res.ToHex());
        //     Debug.WriteLine("expected: " + expected.ToHex() + "\r\n");
        //
        //     Assert.AreEqual(expected, res);
        //
        // }
    }
}