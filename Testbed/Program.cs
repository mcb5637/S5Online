using System;

namespace Testbed
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.SetWindowSize(200, 30);
            Console.ReadLine();

            //RSAParameters p = new RSAParameters();
            //p.Exponent = new byte[] { 3 };
            //p.Modulus = File.ReadAllBytes("D:/reproj/shok/crypto/test/n");
            //p.D = File.ReadAllBytes("D:/reproj/shok/crypto/test/d");
            //p.P = File.ReadAllBytes("D:/reproj/shok/crypto/test/p");
            //p.Q = File.ReadAllBytes("D:/reproj/shok/crypto/test/q");
            //p.DP = File.ReadAllBytes("D:/reproj/shok/crypto/test/dp");
            //p.DQ = File.ReadAllBytes("D:/reproj/shok/crypto/test/dq");
            //p.InverseQ = File.ReadAllBytes("D:/reproj/shok/crypto/test/c");

            //var data = File.ReadAllBytes("D:/reproj/shok/crypto/test/resp");
            //var fishkey = Decryption(data, p, false);

            //var rk = new RsaKeyExchange();

            //var bf = new Blowfish("foo");// rk.Key);

            ////RSAParameters key = new RSAParameters();
            ////key.Exponent = new byte[] { 3 };
            ////key.Modulus = File.ReadAllBytes("D:/reproj/shok/crypto/test/n");

            ////var serverResp = Encryption(fishkey, key, false);

            //var mm = Message.ParseIncoming(File.ReadAllBytes("D:/reproj/shok/crypto/seq1_G2S.bin"), bf).ToArray();
            //var body = mm[0].Body;
            //Console.WriteLine(mm[0].Data.ToString());
            //File.WriteAllBytes("D:/reproj/shok/crypto/seq3_dec.bin", body);
            //mm = Message.ParseIncoming(File.ReadAllBytes("D:/reproj/shok/crypto/seq2_S2G.bin"), bf).ToArray();
            //Console.WriteLine(mm[0].Data.ToString());

            //var mm2 = Message.ParseIncoming(File.ReadAllBytes("D:/reproj/shok/crypto/respM2.bin"), bf).ToArray();
            //File.WriteAllBytes("D:/reproj/shok/crypto/seq3_dec.bin", mm2[2].Body);
            //File.WriteAllBytes("D:/reproj/shok/crypto/seq3_dec.bin", mm2[2].Serialize());

            //foreach (var m in mm2)
            //    Console.WriteLine(m.Data.ToString());

            Console.ReadKey();
        }
    }
}