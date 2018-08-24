using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UDPUtil
{
	public class AsyncTCPClient : AsyncClientBase
	{
		private string address;

		private int port;

		private ILogger logger;

		private TcpClient tcpClient;

		private NetworkStream stream;

		private int bufferSize = 4096;

		private byte[] sendBuffer;

		private byte[] receiveBuffer;

		private TCPStream tcpstream;

		private bool separate;

		private bool separateBytes;

		private byte prefix;

		private byte suffix;

		private byte[] prefixBytes;

		private byte[] suffixBytes;

		private volatile bool started;

		private volatile bool connected;

		private volatile bool needStop;

		public AsyncTCPClient(string address, int port, ILogger logger) : base(1000)
		{
			this.address = address;
			this.port = port;
			this.logger = logger;
			this.sendBuffer = new byte[this.bufferSize];
			this.receiveBuffer = new byte[this.bufferSize];
		}

		public AsyncTCPClient(string address, int port, ILogger logger, int packetSize, int bufferSize) : base(packetSize)
		{
			this.address = address;
			this.port = port;
			this.bufferSize = bufferSize;
			this.logger = logger;
			this.sendBuffer = new byte[this.bufferSize];
			this.receiveBuffer = new byte[this.bufferSize];
		}

		public AsyncTCPClient(string address, int port, ILogger logger, int packetSize, int bufferSize, byte prefix, byte suffix) : base(packetSize)
		{
			this.address = address;
			this.port = port;
			this.bufferSize = bufferSize;
			this.logger = logger;
			this.sendBuffer = new byte[this.bufferSize];
			this.receiveBuffer = new byte[this.bufferSize];
			this.separate = true;
			this.separateBytes = false;
			this.prefix = prefix;
			this.suffix = suffix;
			this.tcpstream = new TCPStream(this.prefix, this.suffix);
		}

		public AsyncTCPClient(string address, int port, ILogger logger, int packetSize, int bufferSize, byte[] prefixBytes, byte[] suffixBytes) : base(packetSize)
		{
			this.address = address;
			this.port = port;
			this.bufferSize = bufferSize;
			this.logger = logger;
			this.sendBuffer = new byte[this.bufferSize];
			this.receiveBuffer = new byte[this.bufferSize];
			this.separate = true;
			this.separateBytes = true;
			this.prefixBytes = prefixBytes;
			this.suffixBytes = suffixBytes;
			this.tcpstream = new TCPStream(this.prefixBytes, this.suffixBytes);
		}

		public override bool IsConnected()
		{
			return this.connected;
		}

		public void RequestStop()
		{
			this.needStop = true;
		}

		public override void Start()
		{
			if (!this.started)
			{
				Thread thread = new Thread(new ThreadStart(this.ProcessLoop));
				thread.Start();
				this.started = true;
			}
		}

		public override void Stop()
		{
			this.needStop = true;
		}

		private void ProcessLoop()
		{
			while (!this.needStop)
			{
				if (!this.connected)
				{
					try
					{
						this.tcpClient = new TcpClient();
						this.tcpClient.Connect(this.address, this.port);
						this.connected = true;
						this.stream = this.tcpClient.GetStream();
						this.stream.BeginRead(this.receiveBuffer, 0, this.receiveBuffer.Length, new AsyncCallback(this.ReceiveCallback), this.stream);
						this.logger.Info(string.Concat(new object[]
						{
							"Started ",
							this.address,
							":",
							this.port
						}));
					}
					catch (Exception ex)
					{
						this.logger.Error("Connect Error:" + ex.Message);
						Thread.Sleep(1000);
					}
				}
				if (this.connected)
				{
					try
					{
						string text = base.PopSend();
						if (text != null)
						{
							byte[] bytes = Encoding.UTF8.GetBytes(text);
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
							this.stream.BeginWrite(array, 0, array.Length, new AsyncCallback(this.SendCallback), text);
							Thread.Sleep(10);
						}
					}
					catch (Exception ex2)
					{
						if (this.tcpClient != null)
						{
							try
							{
								this.tcpClient.Close();
							}
							catch (Exception)
							{
							}
						}
						if (this.stream != null)
						{
							try
							{
								this.stream.Close();
							}
							catch (Exception)
							{
							}
						}
						this.connected = false;
						this.logger.Error("Loop Error:" + ex2.Message);
					}
				}
				Thread.Sleep(1);
			}
			if (this.tcpClient != null)
			{
				try
				{
					this.tcpClient.Close();
				}
				catch (Exception)
				{
				}
			}
			if (this.stream != null)
			{
				try
				{
					this.stream.Close();
				}
				catch (Exception)
				{
				}
			}
			this.connected = false;
			this.started = false;
		}

		public void SendCallback(IAsyncResult iar)
		{
			try
			{
				string str = (string)iar.AsyncState;
				this.stream.EndWrite(iar);
				this.logger.Info("send:" + str);
			}
			catch (Exception ex)
			{
				this.connected = false;
				this.logger.Error("SendCallback Error:" + ex.Message);
			}
		}

		public void ReceiveCallback(IAsyncResult iar)
		{
			try
			{
				int num = this.stream.EndRead(iar);
				if (num > 0)
				{
					List<byte[]> list = this.tcpstream.Process(this.receiveBuffer, num);
					foreach (byte[] current in list)
					{
						string @string = Encoding.UTF8.GetString(current);
						base.PushReceive(@string);
						this.logger.Info("rece:" + @string);
					}
				}
				this.stream.BeginRead(this.receiveBuffer, 0, this.receiveBuffer.Length, new AsyncCallback(this.ReceiveCallback), this.stream);
			}
			catch (Exception ex)
			{
				this.connected = false;
				this.logger.Error("ReceiveCallback Error:" + ex.Message);
			}
		}
	}
}
