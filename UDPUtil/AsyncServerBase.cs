using System;
using System.Collections.Generic;

namespace UDPUtil
{
	public class AsyncServerBase
	{
		private List<AsyncServerBag> sendList = new List<AsyncServerBag>();

		private List<AsyncServerBag> receiveList = new List<AsyncServerBag>();

		private int packetSize = 1000;

		public AsyncServerBase(int packetSize)
		{
			this.packetSize = packetSize;
		}

		public void PushSend(AsyncServerBag bag)
		{
			lock (this.sendList)
			{
				this.sendList.Add(bag);
				while (this.sendList.Count > this.packetSize)
				{
					this.sendList.RemoveAt(0);
				}
			}
		}

		public AsyncServerBag PopSend()
		{
			AsyncServerBag result = null;
			lock (this.sendList)
			{
				if (this.sendList.Count > 0)
				{
					result = this.sendList[0];
					this.sendList.RemoveAt(0);
				}
			}
			return result;
		}

		public void PushReceive(AsyncServerBag bag)
		{
			lock (this.receiveList)
			{
				this.receiveList.Add(bag);
				while (this.receiveList.Count > this.packetSize)
				{
					this.receiveList.RemoveAt(0);
				}
			}
		}

		public AsyncServerBag PopReceive()
		{
			AsyncServerBag result = null;
			lock (this.receiveList)
			{
				if (this.receiveList.Count > 0)
				{
					result = this.receiveList[0];
					this.receiveList.RemoveAt(0);
				}
			}
			return result;
		}
	}
}
