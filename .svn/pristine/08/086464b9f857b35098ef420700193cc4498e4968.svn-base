using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UDPUtil;
using System.Net;
using System.Net.Sockets;

namespace UDPUtilTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestServer();
            //TestClient();
            TestTCPServer();
            //TestTCPClient();
            //TestTCPClient2();
        }

        public static void TestTCPClient2()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("192.168.161.1", 8080);
            NetworkStream stream = tcpClient.GetStream();
            while (true)
            {
                string sendstring = "(abc)";
                byte[] sendbuffer = Encoding.UTF8.GetBytes(sendstring);
                stream.Write(sendbuffer, 0, sendbuffer.Length);
                LoggerImpl.Instance().Warn("send:" + sendstring);

                byte[] recebuffer = new byte[1024];
                int receLength = stream.Read(recebuffer, 0, recebuffer.Length);
                string receString = Encoding.UTF8.GetString(recebuffer, 0, receLength);
                LoggerImpl.Instance().Warn("rece:" + receString);
            }

        }

        public static void TestServer()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.6"), 33789);
            AsyncUdpSever aus = new AsyncUdpSever(ipEndPoint, false, LoggerImpl.Instance());
            aus.Start();
            while (true)
            {
                AsyncServerBag recemsg = aus.PopReceive();
                if (recemsg != null)
                {
                    aus.PushSend(recemsg);
                }
                Thread.Sleep(1);
            }
        }

        public static void TestClient()
        {
            // 本机节点
            IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("192.168.100.44"), 8081);
            // 远程节点
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.100.44"), 8080);

            AsyncUdpClient auc = new AsyncUdpClient(localEP, remoteEP, false, LoggerImpl.Instance());
            auc.Start();
            auc.PushSend("test");
            while (true)
            {
                string recemsg = auc.PopReceive();
                if (recemsg != null)
                {
                    auc.PushSend(recemsg);
                }
                Thread.Sleep(1);
            }
        }

        public static void TestTCPServer()
        {
            byte[] prefixBytes = { (byte)'(', (byte)'(' };
            byte[] suffixBytes = { (byte)')', (byte)')' };
            AsyncTCPServer aus = new AsyncTCPServer("192.168.161.1", 8080, LoggerImpl.Instance(), 1000, 4096, (byte)'(', (byte)')');
            aus.Start();
            while (true)
            {
                AsyncServerBag recemsg = aus.PopReceive();
                if (recemsg != null)
                {
                    aus.PushSend(recemsg);
                }
                Thread.Sleep(1);
            }
        }

        public static void TestTCPClient()
        {
            byte[] prefixBytes = { (byte)'(', (byte)'(' };
            byte[] suffixBytes = { (byte)')', (byte)')' };
            AsyncTCPClient auc = new AsyncTCPClient("192.168.161.1", 8080, LoggerImpl.Instance(), 1000, 4096, (byte)'(', (byte)')');
            auc.Start();
            while (true)
            {
                if (!auc.IsConnected())
                {
                    auc.Start();
                }
                auc.PushSend("cpu;20160308000000;");
                try
                {
                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}