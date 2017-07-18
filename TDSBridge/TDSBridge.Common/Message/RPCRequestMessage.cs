using TDSBridge.Common.Packet;

namespace TDSBridge.Common.Message
{
    public class RPCRequestMessage : TDSMessage
    {
        public RPCRequestMessage()
        {
        }

        public RPCRequestMessage(TDSPacket firtsPacket)
            : base(firtsPacket)
        {
        }
    }
}