using System;

namespace TDSBridge.Common.Header
{
    public class TDSHeader
    {
        public const int HEADER_SIZE = 8;

        protected byte[] _Buffer = new byte[HEADER_SIZE];

        public TDSHeader(byte[] bPacket)
        {
            Array.Copy(bPacket, 0, _Buffer, 0, HEADER_SIZE);
        }

        public TDSHeader()
        {
        }

        public byte[] Data
        {
            get => _Buffer;
            set => _Buffer = value;
        }

        public HeaderType Type
        {
            get => (HeaderType) _Buffer[0];
            set => _Buffer[0] = (byte) value;
        }

        public byte StatusBitMask
        {
            get => _Buffer[1];
            set => _Buffer[1] = value;
        }

        public int LengthIncludingHeader
        {
            get => _Buffer[2] * 0x100 + _Buffer[3];
            set
            {
                _Buffer[3] = (byte) (value % 0x100);
                _Buffer[2] = (byte) (value / 0x100);
            }
        }


        public int PayloadSize
        {
            get => LengthIncludingHeader - HEADER_SIZE;
            set => LengthIncludingHeader = value + HEADER_SIZE;
        }

        public byte this[int idx]
        {
            get => _Buffer[idx];
            set => _Buffer[idx] = value;
        }

        public override string ToString()
        {
            return GetType().FullName +
                   "[Type=" + Type +
                   ";StatusBitMask=" + StatusBitMask +
                   ";LengthIncludingHeader=" + LengthIncludingHeader +
                   ";PayloadSize=" + PayloadSize +
                   "]";
        }
    }
}