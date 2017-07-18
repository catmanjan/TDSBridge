using System;
using System.Net.Sockets;
using System.Threading;
using TDSBridge.Common.Header;
using TDSBridge.Common.Message;
using TDSBridge.Common.Packet;

namespace TDSBridge.Common
{
    public enum ConnectionType
    {
        ClientBridge,
        BridgeSQL
    }

    public class BridgedConnection
    {
        public BridgedConnection(BridgeAcceptor BridgeAcceptor, SocketCouple SocketCouple)
        {
            this.BridgeAcceptor = BridgeAcceptor;
            this.SocketCouple = SocketCouple;
        }

        public BridgeAcceptor BridgeAcceptor { get; protected set; }
        public SocketCouple SocketCouple { get; protected set; }

        public void Start()
        {
            var tIn = new Thread(ClientBridgeThread);
            tIn.IsBackground = true;
            tIn.Start();

            var tOut = new Thread(BridgeSQLThread);
            tOut.IsBackground = true;
            tOut.Start();
        }

        protected virtual void ClientBridgeThread()
        {
            try
            {
                byte[] bBuffer = null;
                var bHeader = new byte[TDSHeader.HEADER_SIZE];
                var iReceived = 0;
                TDSMessage tdsMessage = null;

                while ((iReceived =
                           SocketCouple.ClientBridgeSocket.Receive(bHeader, TDSHeader.HEADER_SIZE, SocketFlags.None)) > 0)
                    //while ((iReceived = sc.InputSocket.Receive(bBuffer, SocketFlags.None)) > 0)
                {
                    var header = new TDSHeader(bHeader);

                    var iMinBufferSize = Math.Max(0x1000, header.LengthIncludingHeader + 1);
                    if (bBuffer == null || bBuffer.Length < iMinBufferSize)
                        bBuffer = new byte[iMinBufferSize];

                    //Console.WriteLine(header.Type);

                    if (header.Type == (HeaderType) 23)
                        iReceived = SocketCouple.ClientBridgeSocket.Receive(bBuffer, 0, 0x1000 - TDSHeader.HEADER_SIZE,
                            SocketFlags.None);
                    else if (header.PayloadSize > 0)
                        SocketCouple.ClientBridgeSocket.Receive(bBuffer, 0, header.PayloadSize, SocketFlags.None);
                    var tdsPacket = new TDSPacket(bHeader, bBuffer, header.PayloadSize);
                    OnTDSPacketReceived(tdsPacket);

                    if (tdsMessage == null)
                        tdsMessage = TDSMessage.CreateFromFirstPacket(tdsPacket);
                    else
                        tdsMessage.Packets.Add(tdsPacket);


                    if (!(tdsMessage is SQLBatchMessage)) // By Calvin: If message is not SQL command, do not intercept
                    {
                        SocketCouple.BridgeSQLSocket.Send(bHeader, bHeader.Length, SocketFlags.None);
                        if (header.Type == (HeaderType) 23)
                            SocketCouple.BridgeSQLSocket.Send(bBuffer, iReceived, SocketFlags.None);
                        else
                            SocketCouple.BridgeSQLSocket.Send(bBuffer, header.PayloadSize, SocketFlags.None);
                    }

                    if ((header.StatusBitMask & StatusBitMask.END_OF_MESSAGE) == StatusBitMask.END_OF_MESSAGE)
                    {
                        OnTDSMessageReceived(tdsMessage);

                        // By Calvin: If message is SQL command, intercept
                        if (tdsMessage is SQLBatchMessage)
                        {
                            //Console.WriteLine("REQUEST");
                            //Console.WriteLine(System.Text.Encoding.Unicode.GetString(tdsMessage.Packets[0].Payload));
                            //Console.WriteLine(BitConverter.ToString(tdsMessage.Packets[0].Payload));

                            // Modify it here
                            var b = (SQLBatchMessage) tdsMessage;
                            if (b.GetBatchText().Contains("showmethemoney"))
                                SQLModifier.ChangeSQL(b.GetBatchText(), this);
                            else
                                foreach (var packet in b.Packets)
                                {
                                    //Console.WriteLine(packet.Payload.Length + " | " +packet.Header.PayloadSize);
                                    SocketCouple.BridgeSQLSocket.Send(packet.Header.Data, packet.Header.Data.Length,
                                        SocketFlags.None);
                                    SocketCouple.BridgeSQLSocket.Send(packet.Payload, packet.Header.PayloadSize,
                                        SocketFlags.None);
                                }
                        }
                        tdsMessage = null;
                    }
                }
            }
            catch (Exception e)
            {
                OnBridgeException(ConnectionType.ClientBridge, e);
            }

            OnConnectionDisconnected(ConnectionType.ClientBridge);
            //Console.WriteLine("Closing InputThread");
        }

        protected virtual void BridgeSQLThread()
        {
            try
            {
                var bBuffer = new byte[4096];
                var iReceived = 0;

                while ((iReceived = SocketCouple.BridgeSQLSocket.Receive(bBuffer, SocketFlags.None)) > 0)
                {
                    var header = new TDSHeader(bBuffer);

                    //Console.WriteLine("[OUT][" + header.Type.ToString() + "]{" + iReceived + "}");

                    SocketCouple.ClientBridgeSocket.Send(bBuffer, iReceived, SocketFlags.None);
                    //Console.WriteLine("RESPONSE");
                    //Console.WriteLine(System.Text.Encoding.Unicode.GetString(bBuffer));
                    //Console.WriteLine(BitConverter.ToString(bBuffer).Replace("-", " "));
                }
            }
            catch (Exception e)
            {
                OnBridgeException(ConnectionType.BridgeSQL, e);
            }

            OnConnectionDisconnected(ConnectionType.BridgeSQL);
            //Console.WriteLine("Closing OutputThread");
        }


        #region Event handlers

        protected virtual void OnTDSMessageReceived(TDSMessage msg)
        {
            BridgeAcceptor.OnTDSMessageReceived(this, msg);
        }

        protected virtual void OnTDSPacketReceived(TDSPacket packet)
        {
            BridgeAcceptor.OnTDSMessageReceived(this, packet);
        }

        protected virtual void OnBridgeException(ConnectionType ct, Exception exce)
        {
            BridgeAcceptor.OnBridgeException(this, ct, exce);
        }

        protected virtual void OnConnectionDisconnected(ConnectionType ct)
        {
            BridgeAcceptor.OnConnectionDisconnected(this, ct);

            switch (ct)
            {
                case ConnectionType.ClientBridge:
                    if (SocketCouple.BridgeSQLSocket.Connected)
                        SocketCouple.BridgeSQLSocket.Disconnect(false);
                    break;
                case ConnectionType.BridgeSQL:
                    if (SocketCouple.ClientBridgeSocket.Connected)
                        SocketCouple.ClientBridgeSocket.Disconnect(false);
                    break;
            }
        }

        #endregion
    }
}