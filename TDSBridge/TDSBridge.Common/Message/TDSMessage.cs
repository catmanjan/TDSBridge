using System;
using System.Collections.Generic;
using System.Text;
using TDSBridge.Common.Header;
using TDSBridge.Common.Packet;

namespace TDSBridge.Common.Message
{
    public class TDSMessage
    {
        protected List<TDSPacket> _lPackets = new List<TDSPacket>();

        public TDSMessage()
        {
        }

        public TDSMessage(TDSPacket firtsPacket)
        {
            Packets.Add(firtsPacket);
        }

        public List<TDSPacket> Packets => _lPackets;

        public bool IsComplete
        {
            get
            {
                if (Packets.Count == 0)
                    return false;
                return (Packets[Packets.Count - 1].Header.StatusBitMask & StatusBitMask.END_OF_MESSAGE) ==
                       StatusBitMask.END_OF_MESSAGE;
            }
        }

        public bool HasIgnoreBitSet
        {
            get
            {
                if (Packets.Count == 0)
                    return false;
                return (Packets[Packets.Count - 1].Header.StatusBitMask & StatusBitMask.IGNORE_EVENT) ==
                       StatusBitMask.IGNORE_EVENT;
            }
        }

        public void BuildMessage(HeaderType messageType, byte[] lPayLoad)
        {
            var packet_size = 4096 - 8;

            Packets.Clear();

            for (var lower = 0; lower < lPayLoad.Length; lower += packet_size)
            {
                var payloadSize = lPayLoad.Length - lower;
                if (payloadSize > packet_size)
                    payloadSize = packet_size;

                // Create new TDS header
                var header = new TDSHeader();
                header.Type = messageType;
                if (lPayLoad.Length - lower <= packet_size)
                    header.StatusBitMask = StatusBitMask.END_OF_MESSAGE;
                else
                    header.StatusBitMask = StatusBitMask.NORMAL;
                header.PayloadSize = payloadSize;

                var payload = new byte[payloadSize];
                Array.Copy(lPayLoad, lower, payload, 0, payloadSize);

                // Create new TDS packet with new TDS header and payload
                var newPacket = new TDSPacket(header.Data, payload, payloadSize);

                // Add packet into message
                Packets.Add(newPacket);
            }
        }


        public byte[] AssemblePayload()
        {
            var lPayLoad = new List<byte>(4096 * 4);

            for (var i = 0; i < Packets.Count; i++)
                lPayLoad.AddRange(Packets[i].Payload);

            return lPayLoad.ToArray();
        }

        public static TDSMessage CreateFromFirstPacket(TDSPacket firstPacket)
        {
            switch (firstPacket.Header.Type)
            {
                case HeaderType.SQLBatch:
                    return new SQLBatchMessage(firstPacket);
                case HeaderType.AttentionSignal:
                    return new AttentionMessage(firstPacket);
                case HeaderType.RPC:
                    return new RPCRequestMessage(firstPacket);
                default:
                    return new TDSMessage(firstPacket);
            }
        }

        public override string ToString()
        {
            if (IsComplete)
            {
                var sb = new StringBuilder(GetType().FullName);

                sb.Append("[#Packets=" + Packets.Count +
                          ";IsComplete=" + IsComplete +
                          ";HasIgnoreBitSet=" + HasIgnoreBitSet +
                          ";TotalPayloadSize=" + AssemblePayload().Length);

                for (var i = 0; i < Packets.Count; i++)
                {
                    sb.Append("\n\t[P" + i + "[");
                    sb.Append(Packets[i]);
                    sb.Append("]]");
                }

                sb.Append("]");

                return sb.ToString();
            }
            return GetType().FullName + "{Incomplete message}";
        }
    }
}