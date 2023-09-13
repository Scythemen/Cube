using System;

namespace Cube.Utility.ButterflyUid
{
    // https://www.yuque.com/simonalong/butterfly/mn26oy
    // https://segmentfault.com/a/1190000021339201
    // https://www.percona.com/blog/2019/11/22/uuids-are-popular-but-bad-for-performance-lets-discuss/
    public class ButterflyUid
    {
        public int MaxGenerator { get; }
        public int MaxSequence { get; }
        public DateTime BaseTime { get; } = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static readonly object lockObject = new object();

        //41-13-9    //40-13-10 
        const int TimeBits = 41; // 40bit~=34 years, 41bit~=69years
        const int GeneratorBits = 13;
        const int SeqBits = 63 - TimeBits - GeneratorBits; //10bit~~100w/1s

        readonly int generatorId = 0;

        int sequence;
        long milliseconds;


        public ButterflyUid(int generatorId, long? lastUid = null)
        {
            MaxGenerator = -1 ^ -1 << GeneratorBits;
            MaxSequence = -1 ^ -1 << SeqBits;

            if (generatorId < 0 || generatorId >= MaxGenerator)
            {
                throw new ArgumentException($"The Generator value should be [0,{MaxGenerator})");
            }

            this.generatorId = generatorId;

            if (lastUid.HasValue)
            {
                var uid = Parse(lastUid.Value);
                InitPreviousSequence(uid.Milliseconds, uid.Sequence);
            }
            else
            {
                milliseconds = (long)(DateTime.UtcNow - BaseTime).TotalMilliseconds;
                sequence = 0;
            }

        }

        public long Next()
        {
            lock (lockObject)
            {
                SpinToNextSequence();

                // 符号，时间戳，高位序列号，机器号，最末位序列号
                // 1bit, 41bits, 8bits, 13bits, 1bit
                long id = milliseconds << GeneratorBits + SeqBits;
                id |= (long)(sequence >> 1 << GeneratorBits + 1);
                id |= (long)generatorId << 1;
                id |= (long)(sequence & 0x01);

                return id;
            }
        }

        void SpinToNextSequence()
        {
            var ticking = (long)(DateTime.UtcNow - BaseTime).TotalMilliseconds;
            if (ticking > milliseconds)
            {
                milliseconds = ticking;
                sequence = 0;
            }
            else
            {
                // 当前毫秒值相等，或者时钟回拨，都在上一次基础上累加
                sequence++;

                if (sequence > MaxSequence)
                {
                    sequence = 0;
                    milliseconds++;
                }
            }
        }

        void InitPreviousSequence(long ms, int seq)
        {
            var ticking = (long)(DateTime.UtcNow - BaseTime).TotalMilliseconds;
            if (ticking < ms)
            {// 时钟回拨
                if (ms - ticking >= 20 * 60 * 60 * 1000) // 20 hours
                {
                    throw new ArgumentOutOfRangeException();
                }

                milliseconds = ms;
                sequence = seq;
                if (seq >= MaxSequence)
                {
                    milliseconds++;
                    sequence = 0;
                }
            }
            else
            {
                milliseconds++;
                sequence = 0;
            }

        }

        public static Uid Parse(long uid)
        {
            uid = uid & 0x7FFFFFFFFFFFFFFF;
            var ms = uid >> GeneratorBits + SeqBits;
            var gtor = (int)(uid >> 1 & (-1 ^ -1 << GeneratorBits));
            var seq = (int)(uid >> GeneratorBits + 1 & (-1 ^ -1 << SeqBits - 1));//高位
            seq = seq << 1 | (int)(uid & 01);// 补上末位
            return new Uid(ms, gtor, seq);
        }

        public struct Uid
        {
            public long Milliseconds { get; }
            public int Generator { get; }
            public int Sequence { get; }

            public Uid(long milliseconds, int generator, int sequece)
            {
                Milliseconds = milliseconds;
                Generator = generator;
                Sequence = sequece;
            }
        }

    }
}
