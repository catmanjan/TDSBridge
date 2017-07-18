using TDSBridge.Common.Packet;

namespace TDSBridge.Common.Message
{
    public class AttentionMessage : TDSMessage
    {
        public AttentionMessage()
        {
        }

        public AttentionMessage(TDSPacket firtsPacket)
            : base(firtsPacket)
        {
        }
    }
}