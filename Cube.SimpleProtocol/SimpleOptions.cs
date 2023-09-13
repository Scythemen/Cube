namespace Cube.SimpleProtocol
{
    /// <summary>
    /// Indicate how to encode/decode the message. <br/>
    ///    frame: [ Delimiter ][ Head...][ Length-Field ][ Payload...][ Tail...]
    /// required: [   yes     ][  no    ][ yes 1~2 byte ][  no       ][ no     ]
    /// </summary>
    public class SimpleOptions
    {
        /// <summary>
        /// the delimiter/start of the message, default=empty no delimiter<br/>
        /// it's a hex-string of bytes, two chars represent one byte. <br/>
        /// e.g:<br/>
        /// if the delimiter is '$', then Delimiter=24. <br/>
        /// if the delimiter is 'Start', then Delimiter=5374617274
        /// </summary>
        public string Delimiter { get; set; } = string.Empty;

        /// <summary>
        /// total bytes of the head, it begins from the end of delimiter. if no head then set to zero.
        /// </summary>
        public int HeadBytes { get; set; } = 0;

        /// <summary>
        /// Required. total bytes of the length-field, to store the length of the payload.
        /// it begins from the end of head. 
        /// </summary>
        public int LengthFieldBytes { get; set; } = 1;

        /// <summary>
        /// default=true, if only the LengthFieldBytes is two bytes.
        /// the most significant byte in the left, the least significant byte int the right.
        /// e.g: big endian of 0x1234, -> byte[] {0x12,0x34}
        /// </summary>
        public bool LengthFieldBigEndian { get; set; } = true;

        /// <summary>
        /// total bytes of the tail, it begins from the end of the payload. if no tail then set to zero.
        /// </summary>
        public int TailBytes { get; set; } = 0;
    }
}