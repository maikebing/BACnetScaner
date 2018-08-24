using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UDPUtil
{
	public class AsyncUdpSever : AsyncServerBase
	{
		private IPEndPoint ipEndPoint;

		private bool isAsync;

		private ILogger logger;

		private UdpClient udpReceive;

		private UdpClient udpSend;

		private ManualResetEvent sendDone = new ManualResetEvent(false);

		private ManualResetEvent receiveDone = new ManualResetEvent(false);

		public AsyncUdpSever(IPEndPoint ipEndPoint, bool isAsync, ILogger logger) : base(1000)
		{
			this.ipEndPoint = ipEndPoint;
			this.isAsync = isAsync;
			this.logger = logger;
		}

		public AsyncUdpSever(IPEndPoint ipEndPoint, bool isAsync, ILogger logger, int packetSize) : base(packetSize)
		{
			this.ipEndPoint = ipEndPoint;
			this.isAsync = isAsync;
			this.logger = logger;
		}

		public void Start()
		{
			this.udpReceive = new UdpClient(this.ipEndPoint);
			this.udpSend = new UdpClient();
			this.logger.Info("Started " + this.ipEndPoint.ToString());
			Thread thread = new Thread(new ThreadStart(this.ReceiveLoop));
			thread.Start();
			Thread thread2 = new Thread(new ThreadStart(this.SendLoop));
			thread2.Start();
		}

		public void Stop()
		{
			if (this.udpReceive != null)
			{
				this.udpReceive.Close();
			}
			if (this.udpSend != null)
			{
				this.udpSend.Close();
			}
		}

		private void ReceiveLoop()
		{
			while (true)
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
		}

		private void SendLoop()
		{
			while (true)
			{
				AsyncServerBag asyncServerBag = base.PopSend();
				if (asyncServerBag != null)
				{
					if (this.isAsync)
					{
						this.Send(asyncServerBag);
					}
					else
					{
						this.SendSync(asyncServerBag);
					}
				}
				Thread.Sleep(1);
			}
		}

		private void ReceiveSync()
		{
			try
			{
				IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
				byte[] bytes = this.udpReceive.Receive(ref remoteEP);
				AsyncServerBag asyncServerBag = new AsyncServerBag();
				asyncServerBag.remoteEP = remoteEP;
				asyncServerBag.bag = Encoding.UTF8.GetString(bytes);
				base.PushReceive(asyncServerBag);
				this.logger.Info("rece:" + asyncServerBag.remoteEP.ToString() + " " + asyncServerBag.bag);
			}
			catch (Exception ex)
			{
				this.logger.Error(ex.Message);
			}
		}

		private void SendSync(AsyncServerBag bag)
		{
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(bag.bag);
				this.udpReceive.Send(bytes, bytes.Length, bag.remoteEP);
				this.logger.Info("send:" + bag.remoteEP.ToString() + " " + bag.bag);
			}
			catch (Exception ex)
			{
				this.logger.Error(ex.Message);
			}
		}

		private void Receive()
		{
			this.udpReceive.BeginReceive(new AsyncCallback(this.ReceiveCallback), this.udpReceive);
			this.receiveDone.WaitOne();
			this.receiveDone.Reset();
		}

		private void ReceiveCallback(IAsyncResult iar)
		{
			UdpClient udpClient = iar.AsyncState as UdpClient;
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			byte[] bytes = udpClient.EndReceive(iar, ref remoteEP);
			this.receiveDone.Set();
			AsyncServerBag asyncServerBag = new AsyncServerBag();
			asyncServerBag.remoteEP = remoteEP;
			asyncServerBag.bag = Encoding.UTF8.GetString(bytes);
			base.PushReceive(asyncServerBag);
			this.logger.Info("rece:" + asyncServerBag.remoteEP.ToString() + " " + asyncServerBag.bag);
		}

		private void Send(AsyncServerBag bag)
		{
			this.udpSend.Connect(bag.remoteEP);
			byte[] bytes = Encoding.UTF8.GetBytes(bag.bag);
			this.udpSend.BeginSend(bytes, bytes.Length, new AsyncCallback(this.SendCallback), this.udpSend);
			this.sendDone.WaitOne();
			this.sendDone.Reset();
			this.logger.Info("send:" + bag.remoteEP.ToString() + " " + bag.bag);
		}

		private void SendCallback(IAsyncResult iar)
		{
			UdpClient udpClient = iar.AsyncState as UdpClient;
			udpClient.EndSend(iar);
			this.sendDone.Set();
		}
	}
}
