using System.Buffers;
using System.Diagnostics;
using Cube.Utility;

namespace Cube.SimpleProtocol
{
    /// <summary>
    ///    frame: [ Delimiter ][ Head...][ Length-Field ][ Payload...][ Tail...]
    /// required: [   yes     ][  no    ][ yes 1~2 byte ][  no       ][ no     ]
    /// </summary>
    public class SimpleCodec
    {
        private readonly SimpleOptions _options;
        private readonly byte[] _delimiterBytes;

        public SimpleCodec(SimpleOptions options)
        {
            _options = options;

            if (_options == null)
            {
                throw new ArgumentNullException(nameof(SimpleOptions));
            }

            _delimiterBytes = string.IsNullOrEmpty(options.Delimiter) ? Array.Empty<byte>() : Convert.FromHexString(options.Delimiter);

            if (_options.LengthFieldBytes < 1 || _options.LengthFieldBytes > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(SimpleOptions.LengthFieldBytes), "the value should be in [1,4] ");
            }

            // var length = _delimiterBytes.Length + _options.HeadBytes + _options.LengthFieldBytes
            //              + _options.LengthFieldBytes + _options.TailBytes;
            //
            // if (length > _options.MaxLength)
            // {
            //     throw new ArgumentOutOfRangeException(nameof(SimpleProtocolOptions.MaxLength),
            //         $"Invalid message length, MaxLength={_options.MaxLength} ");
            // }
            //
            // if (_options.LengthFieldBytes < 1 || _options.LengthFieldBytes > 2)
            // {
            //     throw new ArgumentOutOfRangeException(nameof(SimpleProtocolOptions.LengthFieldBytes),
            //         "Invalid LengthFieldBytes, it's one or two bytes.");
            // }
        }

        public bool Decode(ref ReadOnlySequence<byte> input, out SimpleFrame? frame)
        {
            var reader = new SequenceReader<byte>(input);

            frame = default;
            SequencePosition? passFirstDelimiter = default;

            if (reader.Remaining < 1)
            {
                return false;
            }

            // looking for the delimiter
            if (_delimiterBytes.Length > 0)
            {
                passFirstDelimiter = reader.FirstOf(_delimiterBytes, true);
                if (passFirstDelimiter == null)
                {
                    return false;
                }
                else
                {
                    reader.Advance(passFirstDelimiter.Value.GetInteger());
                }
            }

            // parse head
            var head = ReadOnlySequence<byte>.Empty;
            if (_options.HeadBytes > 0)
            {
                if (_options.HeadBytes <= reader.Remaining)
                {
                    head = input.Slice(reader.Position, _options.HeadBytes);
                    reader.Advance(_options.HeadBytes);
                }
                else
                {
                    // no enough data 
                    return true;
                }
            }


            int lengthValue = 0;
            if (_options.LengthFieldBytes == 1)
            {
                reader.TryRead(out byte len);
                lengthValue = len;
            }
            else
            {
                // multiple bytes
                for (int i = 0; i < _options.LengthFieldBytes; i++)
                {
                    reader.TryRead(out byte b);
                    if (_options.LengthFieldBigEndian)
                    {
                        var mv = (_options.LengthFieldBytes - i - 1) << 3;
                        lengthValue = lengthValue | (b << (int)mv);
                    }
                    else
                    {
                        var mv = i << 3;
                        lengthValue = lengthValue + (b << (int)mv);
                    }
                }
            }

            // no enough bytes for payload
            if (reader.Remaining < lengthValue)
            {
                return true;
            }

            // parse payload
            var payload = ReadOnlySequence<byte>.Empty;

            payload = input.Slice(reader.Position, lengthValue);
            reader.Advance(lengthValue);

            // look for the Tail
            var tail = ReadOnlySequence<byte>.Empty;
            if (_options.TailBytes > 0)
            {
                if (_options.TailBytes <= reader.Remaining)
                {
                    tail = input.Slice(reader.Position, _options.TailBytes);
                    reader.Advance(_options.TailBytes);
                }
                else
                {
                    // no enough data
                    return true;
                }
            }

            input = input.Slice(reader.Position);
            frame = new SimpleFrame(head.ToArray(), payload.ToArray(), tail.ToArray());

            return true;
        }


        public bool Encode(SimpleFrame frame, out byte[]? output)
        {
            output = default;

            var length = _delimiterBytes.Length + +(frame.Head?.Length ?? 0) + _options.LengthFieldBytes
                         + (frame.Payload?.Length ?? 0) + (frame.Tail?.Length ?? 0);
            //
            // if (length > _options.MaxLength)
            // {
            //     throw new ArgumentOutOfRangeException(nameof(SimpleProtocolOptions.MaxLength), $"Too long, maxLength={_options.MaxLength}");
            // }

            output = new byte[length];
            var index = 0;

            // write delimiter
            if (_delimiterBytes.Length > 0)
            {
                for (int i = 0; i < _delimiterBytes.Length; i++)
                {
                    output[index++] = _delimiterBytes[i];
                }
            }

            // write head
            if (_options.HeadBytes > 0)
            {
                if (_options.HeadBytes != (frame.Head?.Length ?? 0))
                {
                    throw new ArgumentOutOfRangeException(nameof(SimpleFrame.Head),
                        $"The length of head not match, it's set to {_options.HeadBytes} bytes, but given {frame.Head?.Length} bytes");
                }

                for (int i = 0; i < frame.Head.Length; i++)
                {
                    output[index++] = frame.Head[i];
                }
            }

            // write length-field
            if (frame.Payload?.Length > Math.Pow(2, _options.LengthFieldBytes << 2))
            {
                throw new ArgumentOutOfRangeException(nameof(SimpleOptions.LengthFieldBytes),
                    $"The length field is tool small to store the the length of payload.");
            }

            if (_options.LengthFieldBytes == 1)
            {
                output[index++] = (byte)frame.Payload.Length;
            }
            else
            {
                if (_options.LengthFieldBigEndian)
                {
                    for (int i = 0; i < _options.LengthFieldBytes; i++)
                    {
                        var mv = (_options.LengthFieldBytes - i - 1) * 8;
                        output[index++] = (byte)(frame.Payload.Length >> mv);
                    }
                }
                else
                {
                    for (int i = 0; i < _options.LengthFieldBytes; i++)
                    {
                        var mv = i * 8;
                        output[index++] = (byte)((frame.Payload.Length >> mv) & 0xff);
                    }
                }
            }


            // write payload
            if (frame.Payload?.Length > 0)
            {
                for (int i = 0; i < frame.Payload.Length; i++)
                {
                    output[index++] = frame.Payload[i];
                }
            }


            // write tail
            if (_options.TailBytes > 0)
            {
                if (_options.TailBytes != (frame.Tail?.Length ?? 0))
                {
                    throw new ArgumentOutOfRangeException(nameof(SimpleFrame.Tail),
                        $"The length of tail not match, it's set to {_options.TailBytes} bytes, but given {frame.Tail?.Length} bytes");
                }

                for (int i = 0; i < frame.Tail.Length; i++)
                {
                    output[index++] = frame.Tail[i];
                }
            }

            Debug.Assert(index == length);

            return true;
        }
    }
}