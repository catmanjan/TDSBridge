using System.Collections.Generic;
using System.Text;
using TDSBridge.Common.Header;
using TDSBridge.Common.Packet;

namespace TDSBridge.Common.Message
{
    public class SQLBatchMessage : TDSMessage
    {
        public SQLBatchMessage()
        {
        }

        public SQLBatchMessage(TDSPacket firstPacket)
            : base(firstPacket)
        {
        }

        // By Calvin
        public SQLBatchMessage(string batchText)
        {
            //List<byte> lPayLoad = new List<byte>(4096 * 4);
            var lPayLoad = new List<byte>();

            lPayLoad.AddRange(ConstructAllHeader());
            lPayLoad.AddRange(Encoding.Unicode.GetBytes(batchText));

            BuildMessage(HeaderType.SQLBatch, lPayLoad.ToArray());
        }

        // By Calvin
        private byte[] ConstructAllHeader()
        {
            byte[] totalLength = {0x16, 0x00, 0x00, 0x00};
            byte[] headerLength = {0x12, 0x00, 0x00, 0x00};
            byte[] headerType = {0x02, 0x00};
            byte[] transactionDescriptor = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
            byte[] outstandingRequestCount = {0x01, 0x00, 0x00, 0x00};

            var allHeader = new List<byte>();
            allHeader.AddRange(totalLength);
            allHeader.AddRange(headerLength);
            allHeader.AddRange(headerType);
            allHeader.AddRange(transactionDescriptor);
            allHeader.AddRange(outstandingRequestCount);

            return allHeader.ToArray();
        }

        public string GetBatchText()
        {
            var bPayload = AssemblePayload();
            var _allHeader = new AllHeader(bPayload);

            var iHeaderLength = (int) _allHeader.Length;

            return Encoding.Unicode.GetString(
                bPayload,
                iHeaderLength,
                bPayload.Length - iHeaderLength);
        }
    }
}