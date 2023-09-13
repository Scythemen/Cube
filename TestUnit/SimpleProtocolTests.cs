using System.Buffers;
using Cube.SimpleProtocol;
using Cube.Utility;

namespace TestUnit
{
    public class SimpleProtocolTests
    {
        [SetUp]
        public void Setup()
        {
        }


        static bool IsEqualsFrame(SimpleFrame a, SimpleFrame b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            return a.ToString() == b.ToString();
        }


        [Test]
        [TestCase("24", "24080102030405060708  0E 0F 10 11 12 13", "0102030405060708")]
        [TestCase("5374617274", "5374617274 080102030405060708  0E ", "0102030405060708")]
        [TestCase("24", "EFEFEF  24080102030405060708  0E 0F 10 11 ", "0102030405060708")]
        [TestCase("EF", "112233  EF080102030405060708  0E 0F 10 11", "0102030405060708")]
        public void Test_delimiter_payload(string delimiter, string rawHexString, string payloadHex)
        {
            // delimiter + payload
            var options = new SimpleOptions()
            {
                Delimiter = delimiter,
                HeadBytes = 0, // no head
                LengthFieldBytes = 1,
                TailBytes = 0 // no tail
            };

            payloadHex = payloadHex.Replace(" ", "");
            rawHexString = rawHexString.Replace(" ", "");

            var proto = new SimpleCodec(options);

            var sequence = new ReadOnlySequence<byte>(rawHexString.HexToBytes());

            proto.Decode(ref sequence, out var outputFrame);

            Assert.IsTrue(payloadHex == outputFrame.Payload.ToHex());

            var frame = new SimpleFrame(null, payloadHex.HexToBytes(), null);
            proto.Encode(frame, out byte[] output);

            Assert.IsTrue(rawHexString.ToUpper().Contains(output.ToHex()));
        }


        [Test]
        [TestCase("24", "24 aabbcc  080102030405060708  0E 0F 10 11 12 13", "aabbcc", "0102030405060708")]
        [TestCase("5374617274", "5374617274 eeffcc 080102030405060708  0E ", "eeffcc", "0102030405060708")]
        [TestCase("2424", "EFEFEF 24 24 998877  080102030405060708  0E 0F 10 11 ", "998877", "0102030405060708")]
        public void Test_delimiter_head_payload(string delimiter, string rawHexString, string head, string payloadHex)
        {
            // delimiter + payload
            var options = new SimpleOptions()
            {
                Delimiter = delimiter,
                HeadBytes = 3,
                LengthFieldBytes = 1,
                TailBytes = 0 // no tail
            };

            payloadHex = payloadHex.Replace(" ", "").ToUpper();
            rawHexString = rawHexString.Replace(" ", "").ToUpper();
            head = head.Replace(" ", "").ToUpper();

            var proto = new SimpleCodec(options);

            var sequence = new ReadOnlySequence<byte>(rawHexString.HexToBytes());

            proto.Decode(ref sequence, out var outputFrame);

            Assert.IsTrue(payloadHex == outputFrame.Payload.ToHex());
            Assert.IsTrue(head == outputFrame.Head.ToHex());

            var frame = new SimpleFrame(head.HexToBytes(), payloadHex.HexToBytes(), null);
            proto.Encode(frame, out byte[] output);

            Assert.IsTrue(rawHexString.ToUpper().Contains(output.ToHex()));
        }


        [Test]
        [TestCase("24", "24 aabbcc  080102030405060708 0c0b0a00 0E 0F 10 11 12 13", "aabbcc", "0102030405060708", "0c0b0a00")]
        [TestCase("5374617274", "5374617274 eeffcc 080102030405060708 1a1b1c10  0E ", "eeffcc", "0102030405060708", "1a1b1c10")]
        [TestCase("2424", "EFEFEF 24 24 998877  080102030405060708 09080706 0E 0F 10 11 ", "998877", "0102030405060708", "09080706")]
        public void Test_delimiter_head_payload_tail(string delimiter, string rawHexString, string head, string payloadHex, string tail)
        {
            // delimiter + payload
            var options = new SimpleOptions()
            {
                Delimiter = delimiter,
                HeadBytes = 3,
                LengthFieldBytes = 1,
                TailBytes = 4
            };

            payloadHex = payloadHex.Replace(" ", "").ToUpper();
            rawHexString = rawHexString.Replace(" ", "").ToUpper();
            head = head.Replace(" ", "").ToUpper();
            tail = tail.Replace(" ", "").ToUpper();

            var proto = new SimpleCodec(options);

            var sequence = new ReadOnlySequence<byte>(rawHexString.HexToBytes());

            proto.Decode(ref sequence, out var outputFrame);

            Assert.IsTrue(payloadHex == outputFrame.Payload.ToHex());
            Assert.IsTrue(head == outputFrame.Head.ToHex());
            Assert.IsTrue(tail == outputFrame.Tail.ToHex());


            var frame = new SimpleFrame(head.HexToBytes(), payloadHex.HexToBytes(), tail.HexToBytes());
            proto.Encode(frame, out byte[] output);

            Assert.IsTrue(rawHexString.ToUpper().Contains(output.ToHex()));
        }


        [Test]
        [TestCase(true, 1, "08 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "0102030405060708")]
        [TestCase(true, 1, "00 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "")]
        [TestCase(true, 1, "01 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "01")]
        [TestCase(true, 1, "03 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "010203")]
        [TestCase(true, 2, "0003 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "010203")]
        [TestCase(true, 2, "0007 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "01020304050607")]
        [TestCase(true, 2,
            "0100 01020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708 EEEEEEEEEEEEEEEE",
            "01020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708")]
        [TestCase(true, 3, "000003 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "010203")]
        [TestCase(true, 4, "00000003 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "010203")]
        [TestCase(false, 1, "08 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "0102030405060708")]
        [TestCase(false, 2, "0700 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "01020304050607")]
        [TestCase(false, 2,
            "0001 01020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708 EEEEEEEEEEEEEEEE",
            "01020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708010203040506070801020304050607080102030405060708")]
        [TestCase(false, 3, "030000 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "010203")]
        [TestCase(false, 4, "03000000 0102030405060708 0c0b0a00 0E 0F 10 11 12 13", "010203")]
        public void Test_length_based(bool bigEndian, int lengthFieldBytes, string rawHexString, string payloadHex)
        {
            var options = new SimpleOptions()
            {
                Delimiter = null,
                HeadBytes = 0,
                LengthFieldBytes = lengthFieldBytes,
                LengthFieldBigEndian = bigEndian,
                TailBytes = 0
            };

            payloadHex = payloadHex.Replace(" ", "").ToUpper();
            rawHexString = rawHexString.Replace(" ", "").ToUpper();

            var proto = new SimpleCodec(options);

            var sequence = new ReadOnlySequence<byte>(rawHexString.HexToBytes());

            proto.Decode(ref sequence, out var outputFrame);

            Assert.IsTrue(payloadHex == outputFrame.Payload.ToHex());


            var frame = new SimpleFrame(null, payloadHex.HexToBytes(), null);
            proto.Encode(frame, out byte[] output);

            Assert.IsTrue(rawHexString.ToUpper().Contains(output.ToHex()));
        }


        [Test]
        [TestCase("00 01020304050607", "", 7, "01020304050607", "")]
        [TestCase("01 01020304050607", "01", 0, "", "020304050607")]
        [TestCase("00 01020304050607", "", 1, "01", "020304050607")]
        [TestCase("00 01020304050607", "", 3, "010203", "04050607")]
        [TestCase("03  010203 04050607", "010203", 4, "04050607", "")]
        public void Test_length_tail(string rawHexString, string payload, int tailBytes, string tail, string remain)
        {
            var options = new SimpleOptions()
            {
                LengthFieldBytes = 1,
                TailBytes = tailBytes
            };

            rawHexString = rawHexString.Replace(" ", "").ToUpper();
            payload = payload.Replace(" ", "").ToUpper();
            tail = tail.Replace(" ", "").ToUpper();
            remain = remain.Replace(" ", "").ToUpper();


            var proto = new SimpleCodec(options);

            var sequence = new ReadOnlySequence<byte>(rawHexString.HexToBytes());
            proto.Decode(ref sequence, out var frame);

            Assert.IsTrue(frame.Tail.ToHex() == tail);
            Assert.IsTrue(sequence.ToHex() == remain);

            proto.Encode(new SimpleFrame(null, payload.HexToBytes(), tail.HexToBytes()), out var output);

            Assert.IsTrue(rawHexString.Contains(output.ToHex()));
        }
    }
}