using System;
using System.Collections.Generic;

namespace UDPUtil
{
	public class TCPStream
	{
		private bool separateBytes;

		private byte prefix;

		private byte suffix;

		private byte[] prefixBytes;

		private byte[] suffixBytes;

		private List<byte> contents = new List<byte>();

		private bool content_valid;

		private List<byte> buffer = new List<byte>();

		public TCPStream(byte prefix, byte suffix)
		{
			this.separateBytes = false;
			this.prefix = prefix;
			this.suffix = suffix;
		}

		public TCPStream(byte[] prefixBytes, byte[] suffixBytes)
		{
			this.separateBytes = true;
			this.prefixBytes = prefixBytes;
			this.suffixBytes = suffixBytes;
		}

		public List<byte[]> Process(byte[] bytes, int length)
		{
			List<byte[]> list = new List<byte[]>();
			if (this.separateBytes)
			{
				for (int i = 0; i < length; i++)
				{
					this.buffer.Add(bytes[i]);
				}
				while (true)
				{
					int num = this.FindIndexForward(this.buffer, this.buffer.Count, this.suffixBytes);
					if (num == -1)
					{
						break;
					}
					int num2 = this.FindIndexBackward(this.buffer, num, this.prefixBytes);
					if (num2 != -1)
					{
						int num3 = num - num2 - this.prefixBytes.Length;
						if (num3 > 0)
						{
							byte[] array = new byte[num3];
							for (int j = 0; j < num3; j++)
							{
								array[j] = this.buffer[num2 + this.prefixBytes.Length + j];
							}
							list.Add(array);
						}
					}
					for (int k = 0; k < num + this.suffixBytes.Length; k++)
					{
						this.buffer.RemoveAt(0);
					}
				}
			}
			else
			{
				for (int l = 0; l < length; l++)
				{
					byte b = bytes[l];
					if (b == this.suffix)
					{
						if (this.content_valid && this.contents.Count > 0)
						{
							byte[] array2 = new byte[this.contents.Count];
							for (int m = 0; m < this.contents.Count; m++)
							{
								array2[m] = this.contents[m];
							}
							list.Add(array2);
						}
						this.contents.Clear();
						this.content_valid = false;
					}
					else if (b == this.prefix)
					{
						this.contents.Clear();
						this.content_valid = true;
					}
					else if (this.content_valid)
					{
						this.contents.Add(b);
					}
				}
			}
			return list;
		}

		private int FindIndexForward(List<byte> byteList, int length, byte[] bytes)
		{
			int result = -1;
			for (int i = 0; i <= length - bytes.Length; i++)
			{
				if (this.Match(byteList, i, bytes))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private int FindIndexBackward(List<byte> byteList, int length, byte[] bytes)
		{
			int result = -1;
			for (int i = length - bytes.Length; i >= 0; i--)
			{
				if (this.Match(byteList, i, bytes))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private bool Match(List<byte> byteList, int index, byte[] bytes)
		{
			bool result = true;
			for (int i = 0; i < bytes.Length; i++)
			{
				if (byteList[index + i] != bytes[i])
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}
}
