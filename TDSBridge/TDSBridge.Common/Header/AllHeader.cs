namespace TDSBridge.Common.Header
{
    public class AllHeader
    {
        protected byte[] _bPayload;

        public AllHeader(byte[] bPayload)
        {
            _bPayload = bPayload;
        }

        public uint Length => (uint) _bPayload[3] * 0x01000000 +
                              (uint) _bPayload[2] * 0x00010000 +
                              (uint) _bPayload[1] * 0x00000100 +
                              (uint) _bPayload[0] * 0x00000001;
    }
}