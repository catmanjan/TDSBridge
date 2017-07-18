using System;
using System.Data;
using System.Net.Sockets;
using System.Text;
using TDSBridge.Common.Header;
using TDSBridge.Common.Message;

namespace TDSBridge.Common
{
    public static class SQLModifier
    {
        public static string SourceText { get; set; }
        public static string TargetText { get; set; }

        public static void ChangeSQL(string source, BridgedConnection conn)
        {
            if (source.Contains("showmethemoney"))
            {
                var data = "810100000000002000A703000904D0003403620061007200D10300666F6FFD1000C1000100000000000000";
                var msg = new TDSMessage();
                msg.BuildMessage(HeaderType.TabularResult, StringToByteArray(data));

                foreach (var packet in msg.Packets)
                {
                    conn.SocketCouple.ClientBridgeSocket.Send(packet.Buffer, packet.Buffer.Length, SocketFlags.None);
                    //conn.SocketCouple.ClientBridgeSocket.Send(packet.Header.Data, packet.Header.Data.Length, SocketFlags.None);
                    //conn.SocketCouple.ClientBridgeSocket.Send(packet.Payload, packet.Header.PayloadSize, SocketFlags.None);
                    Console.WriteLine("RESPONSE");
                    Console.WriteLine(Encoding.Unicode.GetString(packet.Buffer));
                    Console.WriteLine(BitConverter.ToString(packet.Buffer));
                }
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            var NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];
            for (var i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}