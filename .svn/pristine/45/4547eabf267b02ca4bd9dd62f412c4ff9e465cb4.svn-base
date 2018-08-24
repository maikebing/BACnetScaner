using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UDPUtil
{
	public class AsyncTCPServer : AsyncServerBase
	{
		private string address;

		private int port;

		private ILogger logger;

		private TcpListener listener;

		private ManualResetEvent listenDone = new ManualResetEvent(false);

		private int bufferSize = 4096;

		private Dictionary<string, Socket> socketDic = new Dictionary<string, Socket>();

		private Dictionary<string, byte[]> bufferDic = new Dictionary<string, byte[]>();

		private bool separate;

		private bool separateBytes;

		private byte prefix;

		private byte suffix;

		private byte[] prefixBytes;

		private byte[] suffixBytes;

		private volatile bool needStop;

		private Dictionary<string, TCPStream> streamMap = new Dictionary<string, TCPStream>();

		public AsyncTCPServer(string address, int port, ILogger logger) : base(1000)
		{
			this.address = address;
			this.port = port;
			this.logger = logger;
		}

		public AsyncTCPServer(string address, int port, ILogger logger, int packetSize, int bufferSize) : base(packetSize)
		{
			this.address = address;
			this.port = port;
			this.bufferSize = bufferSize;
			this.logger = logger;
		}

		public AsyncTCPServer(string address, int port, ILogger logger, int packetSize, int bufferSize, byte prefix, byte suffix) : base(packetSize)
		{
			this.address = address;
			this.port = port;
			this.bufferSize = bufferSize;
			this.logger = logger;
			this.separate = true;
			this.separateBytes = false;
			this.prefix = prefix;
			this.suffix = suffix;
		}

		public AsyncTCPServer(string address, int port, ILogger logger, int packetSize, int bufferSize, byte[] prefixBytes, byte[] suffixBytes) : base(packetSize)
		{
			this.address = address;
			this.port = port;
			this.bufferSize = bufferSize;
			this.logger = logger;
			this.separate = true;
			this.separateBytes = true;
			this.prefixBytes = prefixBytes;
			this.suffixBytes = suffixBytes;
		}

		public void RequestStop()
		{
			this.needStop = true;
		}

		public void Start()
		{
			this.listener = new TcpListener(new IPEndPoint(IPAddress.Parse(this.address), this.port));
			this.listener.Start();
			this.logger.Info(string.Concat(new object[]
			{
				"Started ",
				this.address,
				":",
				this.port
			}));
			Thread thread = new Thread(new ThreadStart(this.ListenLoop));
			thread.Start();
			Thread thread2 = new Thread(new ThreadStart(this.ProcessLoop));
			thread2.Start();
		}

		public void Stop()
		{
			if (this.listener != null)
			{
				this.listener.Stop();
			}
		}

		private void ListenLoop()
		{
			while (!this.needStop)
			{
				this.listener.BeginAcceptSocket(new AsyncCallback(this.ClientConnected), this.listener);
				this.listenDone.WaitOne();
				this.listenDone.Reset();
				Thread.Sleep(1);
			}
		}

		private void ClientConnected(IAsyncResult ar)
		{
			try
			{
				TcpListener tcpListener = (TcpListener)ar.AsyncState;
				Socket socket = tcpListener.EndAcceptSocket(ar);
				string text = socket.RemoteEndPoint.ToString();
				this.socketDic.Remove(text);
				this.bufferDic.Remove(text);
				this.bufferDic.Add(text, new byte[this.bufferSize]);
				this.socketDic.Add(text, socket);
				socket.BeginReceive(this.bufferDic[text], 0, this.bufferSize, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), socket);
				this.logger.Error("clientConnect:" + text);
			}
			catch (Exception ex)
			{
				this.logger.Error("clientConnect Error:" + ex.Message);
			}
			this.listenDone.Set();
		}

		private void ProcessLoop()
		{
			while (true)
			{
				AsyncServerBag asyncServerBag = base.PopSend();
				if (asyncServerBag != null)
				{
					string text = asyncServerBag.remoteEP.ToString();
					try
					{
						Socket socket = this.socketDic[text];
						byte[] bytes = Encoding.UTF8.GetBytes(asyncServerBag.bag);
						byte[] array = bytes;
						if (this.separate)
						{
							if (this.separateBytes)
							{
								array = new byte[this.prefixBytes.Length + bytes.Length + this.suffixBytes.Length];
								Array.Copy(this.prefixBytes, 0, array, 0, this.prefixBytes.Length);
								Array.Copy(bytes, 0, array, this.prefixBytes.Length, bytes.Length);
								Array.Copy(this.suffixBytes, 0, array, this.prefixBytes.Length + bytes.Length, this.suffixBytes.Length);
							}
							else
							{
								array = new byte[1 + bytes.Length + 1];
								array[0] = this.prefix;
								Array.Copy(bytes, 0, array, 1, bytes.Length);
								array[1 + bytes.Length] = this.suffix;
							}
						}
						socket.BeginSend(array, 0, array.Length, SocketFlags.None, new AsyncCallback(this.SendCallback), asyncServerBag);
					}
					catch (Exception ex)
					{
						this.socketDic.Remove(text);
						this.bufferDic.Remove(text);
						this.logger.Error("BeginReceive Error:" + text + "\t" + ex.Message);
					}
				}
				Thread.Sleep(1);
			}
		}

		private void SendCallback(IAsyncResult iar)
		{
			try
			{
				AsyncServerBag asyncServerBag = iar.AsyncState as AsyncServerBag;
				this.logger.Info("send:" + asyncServerBag.remoteEP.ToString() + " " + asyncServerBag.bag);
			}
			catch (Exception ex)
			{
				this.logger.Error("SendCallback Error:" + ex.Message);
			}
		}

		private void ReceiveCallback(IAsyncResult iar)
		{
			Socket socket = (Socket)iar.AsyncState;
			EndPoint remoteEndPoint = socket.RemoteEndPoint;
			string key = remoteEndPoint.ToString();
			try
			{
				int num = socket.EndReceive(iar);
				if (num > 0)
				{
					IPEndPoint iPEndPoint = (IPEndPoint)remoteEndPoint;
					string key2 = iPEndPoint.ToString();
					if (!this.streamMap.ContainsKey(key2))
					{
						TCPStream value;
						if (this.separateBytes)
						{
							value = new TCPStream(this.prefixBytes, this.suffixBytes);
						}
						else
						{
							value = new TCPStream(this.prefix, this.suffix);
						}
						this.streamMap.Add(key2, value);
					}
					TCPStream tCPStream = this.streamMap[key2];
					List<byte[]> list = tCPStream.Process(this.bufferDic[key], num);
					for (int i = 0; i < list.Count; i++)
					{
						byte[] bytes = list[i];
						string @string = Encoding.UTF8.GetString(bytes);
						AsyncServerBag asyncServerBag = new AsyncServerBag();
						asyncServerBag.remoteEP = (IPEndPoint)remoteEndPoint;
						asyncServerBag.bag = @string;
						base.PushReceive(asyncServerBag);
						this.logger.Info("rece:" + asyncServerBag.remoteEP.ToString() + " " + asyncServerBag.bag);
					}
				}
				socket.BeginReceive(this.bufferDic[key], 0, this.bufferSize, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), socket);
			}
			catch (Exception ex)
			{
				this.socketDic.Remove(key);
				this.bufferDic.Remove(key);
				this.logger.Error("ReceiveCallback Error:" + ex.Message);
			}
		}
	}
}
