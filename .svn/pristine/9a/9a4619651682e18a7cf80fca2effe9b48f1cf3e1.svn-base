using System;
using System.Collections.Generic;

namespace UDPUtil
{
	public class AsyncClientBase
	{
		private List<string> sendList = new List<string>();

		private List<string> receiveList = new List<string>();

		private int packetSize = 1000;

		public AsyncClientBase(int packetSize)
		{
			this.packetSize = packetSize;
		}

		public virtual bool IsConnected()
		{
			return false;
		}

		public virtual void Start()
		{
		}

		public virtual void Stop()
		{
		}

		public void PushSend(string bag)
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

		public string PopSend()
		{
			string result = null;
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

		public void PushReceive(string bag)
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

		public string PopReceive()
		{
			string result = null;
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
