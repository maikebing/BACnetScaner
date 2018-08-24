using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UDPUtil
{
	public class AsyncUdpClient : AsyncClientBase
	{
		private IPEndPoint localEP;

		private IPEndPoint remoteEP;

		private bool isAsync;

		private ILogger logger;

		private UdpClient udpClient;

		public bool shouldStop;

		public bool receiveStopped;

		public bool sendStopped;

		private bool WaitOne_Send;

		private bool WaitOne_Rece;

		public AsyncUdpClient(IPEndPoint localEP, IPEndPoint remoteEP, bool isAsync, ILogger logger) : base(1000)
		{
			this.localEP = localEP;
			this.remoteEP = remoteEP;
			this.isAsync = isAsync;
			this.logger = logger;
		}

		public AsyncUdpClient(IPEndPoint localEP, IPEndPoint remoteEP, bool isAsync, ILogger logger, int packetSize) : base(packetSize)
		{
			this.localEP = localEP;
			this.remoteEP = remoteEP;
			this.isAsync = isAsync;
			this.logger = logger;
		}

		public override bool IsConnected()
		{
			return false;
		}

		public override void Start()
		{
			this.udpClient = new UdpClient(this.localEP);
			this.udpClient.Connect(this.remoteEP);
			this.logger.Info("Started " + this.localEP.ToString() + "  " + this.remoteEP.ToString());
			Thread thread = new Thread(new ThreadStart(this.ReceiveLoop));
			thread.Start();
			Thread thread2 = new Thread(new ThreadStart(this.SendLoop));
			thread2.Start();
		}

		public override void Stop()
		{
			this.shouldStop = true;
			while (!this.sendStopped || !this.receiveStopped)
			{
				Thread.Sleep(1);
			}
			if (this.udpClient != null)
			{
				this.udpClient.Close();
			}
		}

		private void ReceiveLoop()
		{
			this.receiveStopped = false;
			while (!this.shouldStop)
			{
				if (this.isAsync)
				{
					this.Receive();
				}
				else
				{
					this.ReceiveSync();
				}
				Thread.Sleep(1);
			}
			this.receiveStopped = true;
		}

		private void SendLoop()
		{
			this.sendStopped = false;
			while (!this.shouldStop)
			{
				string text = base.PopSend();
				if (text != null)
				{
					if (this.isAsync)
					{
						this.Send(text);
					}
					else
					{
						this.SendSync(text);
					}
				}
				Thread.Sleep(1);
			}
			this.sendStopped = true;
		}

		private void ReceiveSync()
		{
			try
			{
				IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] bytes = this.udpClient.Receive(ref iPEndPoint);
				string str = Encoding.UTF8.GetString(bytes);
				base.PushReceive(str);
				this.logger.Info("rece:" + str);
			}
			catch (Exception ex)
			{
				this.logger.Error(ex.Message);
			}
		}

		private void SendSync(string bag)
		{
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(bag);
				this.udpClient.Send(bytes, bytes.Length);
				this.logger.Info("send:" + bag);
			}
			catch (Exception ex)
			{
				this.logger.Error(ex.Message);
			}
		}

		private void Send(string msg)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(msg);
			this.WaitOne_Send = false;
			this.udpClient.BeginSend(bytes, bytes.Length, new AsyncCallback(this.SendCallback), this.udpClient);
			while (!this.shouldStop)
			{
				if (this.WaitOne_Send)
				{
					this.logger.Info("send:" + msg);
					return;
				}
				Thread.Sleep(1);
			}
		}

		public void SendCallback(IAsyncResult iar)
		{
			UdpClient udpClient = iar.AsyncState as UdpClient;
			try
			{
				udpClient.EndSend(iar);
				this.WaitOne_Send = true;
			}
			catch (Exception)
			{
			}
		}

		public void Receive()
		{
			this.WaitOne_Rece = false;
			this.udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), this.udpClient);
			while (!this.shouldStop)
			{
				if (this.WaitOne_Rece)
				{
					return;
				}
				Thread.Sleep(1);
			}
		}

		public void ReceiveCallback(IAsyncResult iar)
		{
			UdpClient udpClient = iar.AsyncState as UdpClient;
			try
			{
				IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] bytes = udpClient.EndReceive(iar, ref iPEndPoint);
				this.WaitOne_Rece = true;
				string str = Encoding.UTF8.GetString(bytes);
				base.PushReceive(str);
				this.logger.Info("rece:" + str);
			}
			catch (Exception)
			{
			}
		}
	}
}
