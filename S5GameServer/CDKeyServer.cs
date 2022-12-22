using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace S5GameServices
{
    public static class CDKeyServer
    {
        public static void Run(int port = 44000)
        {
            Task.Run(() =>
            {
                var ipep = new IPEndPoint(IPAddress.Any, port);
                var newsock = new UdpClient(ipep);
                var sender = new IPEndPoint(IPAddress.Any, port);

                var getTokenResponse = new CDKeyMessage(new MessageDataList()
                {
                    new MessageDataString(99),
                    new MessageDataString(1),
                    new MessageDataString(1),
                    new MessageDataList()
                    {
                        new MessageDataString(38),
                        new MessageDataList()
                        {
                            new MessageDataBinary(new byte[5])
                        }
                    }
                });

                var authorizeResponse = new CDKeyMessage(new MessageDataList()
                {
                    new MessageDataString(99),
                    new MessageDataString(2),
                    new MessageDataString(2),
                    new MessageDataList()
                    {
                        new MessageDataString((byte)MessageCode.GSSUCCESS),
                        new MessageDataList()
                        {
                            new MessageDataBinary(new byte[16]),
                            new MessageDataBinary(new byte[16])
                        }
                    }
                });

                var login1Response = new CDKeyMessage(new MessageDataList()
                {
                    new MessageDataString(99),
                    new MessageDataString(3),
                    new MessageDataString(1),
                    new MessageDataList()
                    {
                        new MessageDataString((byte)MessageCode.GSSUCCESS),
                        new MessageDataList()
                        {
                            new MessageDataBinary(new byte[20])
                        }
                    }
                });

                var login2Response = new CDKeyMessage(new MessageDataList()
                {
                    new MessageDataString(99),
                    new MessageDataString(4),
                    new MessageDataString(2),
                    new MessageDataList()
                    {
                        new MessageDataString((byte)MessageCode.GSSUCCESS),
                        new MessageDataList()
                        {
                            new MessageDataString(2),
                            new MessageDataBinary(new byte[16])
                        }
                    }
                });

                var logoutResponse = new CDKeyMessage(new MessageDataList()
                {
                    new MessageDataString(99),
                    new MessageDataString(6),
                    new MessageDataString(1),
                    new MessageDataList()
                    {
                        new MessageDataString((byte)MessageCode.GSSUCCESS),
                        new MessageDataList()
                    }
                });

                while (true)
                {
                    var reqData = newsock.Receive(ref sender);
                    var req = new CDKeyMessage(reqData);
                    Console.WriteLine("IN:\t" + req.Data.ToString());

                    CDKeyMessage response;
                    switch (req.Data[1].AsInt)
                    {
                        case 1: response = getTokenResponse; break;
                        case 2: response = authorizeResponse; break;
                        case 3: response = login1Response; break;
                        case 4: response = login2Response; break;
                        case 6: response = logoutResponse; break;
                        case 7: continue; //heartbeat
                        default: throw new Exception("Unknown CDKey request: " + req.Data.ToString());
                    }

                    var udpConnId = req.Data[0].AsString;
                    response.Data[0].AsString = udpConnId;
                    Console.WriteLine("OUT:\t" + response.Data.ToString());

                    var respData = response.Serialize();
                    newsock.Send(respData, respData.Length, sender);
                }
            });
        }

        public static void Testing()
        {
            var sha = new SHA1Managed();

            var token1 = new byte[] { 0x2F, 0xD4, 0x92, 0xF1, 0xBE };
            var cdkey = Encoding.ASCII.GetBytes("SHK-CH3E-VMYA-61UH-NHQ0");
            var shok = Encoding.ASCII.GetBytes("SETTLERSHOK");

            var plain1 = Concat(token1, cdkey);
            Console.WriteLine(BitConverter.ToString(sha.ComputeHash(plain1)));

            var plain2 = Concat(shok, cdkey);
            Console.WriteLine(BitConverter.ToString(sha.ComputeHash(plain2)));

            var token2a = new byte[] { 0x33, 0x85, 0x14, 0x3F, 0x7E, 0xBB, 0x42, 0xC7, 0x8F, 0x4A, 0x11, 0x04, 0xB9, 0xB2, 0x1C, 0x23 };
            var token2 = new byte[] { 0xE7, 0xD0, 0xCB, 0xA7, 0xCE };

            var plain3 = Concat(token2, cdkey);
            Console.WriteLine(BitConverter.ToString(sha.ComputeHash(plain3)));

            var plain4 = Concat(shok, cdkey);
            Console.WriteLine(BitConverter.ToString(sha.ComputeHash(plain4)));

            for (int i = 1; i < 18; i++)
            {
                var cdkm = new CDKeyMessage(File.ReadAllBytes("D:/reproj/shok/cdkey/seq" + i + ".bin"));
                Console.WriteLine(cdkm.Data.ToString());

                File.WriteAllBytes("D:/reproj/shok/cdkey/seq" + i + "_d.bin", cdkm.BinData);
            }
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            var o = new byte[a.Length + b.Length];
            a.CopyTo(o, 0);
            b.CopyTo(o, a.Length);
            return o;
        }
    }
}