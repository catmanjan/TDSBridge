using System;
using System.Collections.Generic;
using TDSBridge.Common.Header;

namespace TDSBridge.Common.Packet
{
    public class TDSPacket
    {
        //static int iCnt = 0;

        protected TDSHeader _header;
        protected byte[] _payload;

        public TDSPacket(byte[] bBuffer)
        {
            _header = new TDSHeader(bBuffer);

            _payload = new byte[_header.LengthIncludingHeader - TDSHeader.HEADER_SIZE];
            Array.Copy(bBuffer, TDSHeader.HEADER_SIZE, _payload, 0, _payload.Length);
        }

        public TDSPacket(byte[] bHeader, byte[] bPayload, int iPayloadSize)
        {
            _header = new TDSHeader(bHeader);

            _payload = new byte[iPayloadSize];
            Array.Copy(bPayload, 0, _payload, 0, iPayloadSize);
        }

        public byte[] Payload => _payload;
        public TDSHeader Header => _header;

        public byte[] Buffer
        {
            get
            {
                var buffer = new List<byte>();
                buffer.AddRange(Header.Data);
                buffer.AddRange(_payload);
                return buffer.ToArray();
            }
        }

        public override string ToString()
        {
            return base.ToString() + "[Header=" + Header + "]";
        }
    }
}