using System;
using System.Globalization;
using System.Threading;

namespace Cube.Utility
{
    public class SnowFlakeUid
    {
        // 40-5-5-13
        // 40-6-4-13
        // 40-8-4-11
        //const int _timeBits = 40; // 34 years
        //const int _generatorBits = 8;
        //const int _idTypeBits = 4;
        //const int _sequenceBits = 63 - _timeBits - _generatorBits - _idTypeBits;    // keep the first bit

        //40-13-10, 40bit~~34.8years,10bit~~100w/1s
        //41-13-9, 41bit~=69years
        const int _timeBits = 40; // 34 years
        const int _generatorBits = 14;
        const int _sequenceBits = 63 - _timeBits - _generatorBits;

        public readonly int MaxGenerator;
        //public readonly int MaxIdType;
        public readonly int MaxSequence;
        //public string BitString { get { return $"{_timeBits}-{_generatorBits}-{_idTypeBits}-{_sequenceBits}"; } }
        public string BitString { get { return $"{_timeBits}-{_generatorBits}-{_sequenceBits}"; } }

        readonly DateTime _start;
        readonly long _generatorID = 0;
        //readonly long _idType = 0;

        int _sequence;
        long _previousTime;

        static readonly object _lockObject = new object();

        //public SnowFlakeGuid(DateTime start, int generatorId, int idType)
        public SnowFlakeUid(DateTime start, int generatorId)
        {
            MaxGenerator = -1 ^ (-1 << _generatorBits);
            //MaxIdType = -1 ^ (-1 << _idTypeBits);
            MaxSequence = -1 ^ (-1 << _sequenceBits);

            if (generatorId < 0 || generatorId >= MaxGenerator)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "generator id must be between 0 (inclusive) and {0} (exclusive).", MaxGenerator);
                throw new ArgumentException(msg, "generatorId");
            }
            //if (idType < 0 || idType >= MaxIdType)
            //{
            //    var msg = string.Format(CultureInfo.InvariantCulture, "id type must be between 0 (inclusive) and {0} (exclusive).", MaxIdType);
            //    throw new ArgumentException(msg, "idType");
            //}
            if (start == DateTime.MinValue || start.Kind != DateTimeKind.Utc || start > DateTime.UtcNow)
            {
                throw new ArgumentException("start date must not be UTC time in the future.", "start");
            }
            //_idType = idType;
            _generatorID = generatorId;
            _start = start;
        }

        public long Next()
        {
            lock (_lockObject)
            {
                SpinToNextSequence();
                // keep the first bit
                //long id = (_previousTime << (_generatorBits + _idTypeBits + _sequenceBits)) | (_generatorID << (_idTypeBits + _sequenceBits)) | (_idType << _sequenceBits) | (long)_sequence;
                long id = (_previousTime << (_generatorBits + _sequenceBits)) | (_generatorID << _sequenceBits) | (long)_sequence;
                return id;
            }
        }

        void SpinToNextSequence()
        {
            var time = (long)(DateTime.UtcNow - _start).TotalMilliseconds;
            while (time == _previousTime && _sequence >= MaxSequence)
            {
                Thread.Sleep(0);
                time = (long)(DateTime.UtcNow - _start).TotalMilliseconds;
            }
            _sequence = time == _previousTime ? (_sequence + 1) : 0;
            _previousTime = time;
        }

        public static IdParts ParseID(long id)
        {
            return new IdParts(id);
        }

        public struct IdParts
        {
            public readonly long Millisecond;
            public readonly int GeneratorId;
            //public readonly int IdType;
            public readonly int Sequence;

            //public IdParts(long id)
            //{
            //    id = id & 0x7FFFFFFFFFFFFFFF;
            //    Millisecond = id >> (_generatorBits + _idTypeBits + _sequenceBits);
            //    GeneratorId = (int)((id >> (_idTypeBits + _sequenceBits)) & (-1 ^ (-1 << _generatorBits)));
            //    IdType = (int)((id >> _sequenceBits) & (-1 ^ (-1 << _idTypeBits)));
            //    Sequence = (int)(id & (-1 ^ (-1 << _sequenceBits)));
            //}
            public IdParts(long id)
            {
                id = id & 0x7FFFFFFFFFFFFFFF;
                Millisecond = id >> (_generatorBits + _sequenceBits);
                GeneratorId = (int)((id >> (_sequenceBits)) & (-1 ^ (-1 << _generatorBits)));
                Sequence = (int)(id & (-1 ^ (-1 << _sequenceBits)));
            }
        }

    }
}
