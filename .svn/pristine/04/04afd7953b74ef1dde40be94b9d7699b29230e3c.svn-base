using System;

namespace UDPUtil
{
	public class LoggerImpl : ILogger
	{
		private static LoggerImpl instance;

		public static LoggerImpl Instance()
		{
			if (LoggerImpl.instance == null)
			{
				LoggerImpl.instance = new LoggerImpl();
			}
			return LoggerImpl.instance;
		}

		private LoggerImpl()
		{
		}

		public void Debug(string message)
		{
			Console.WriteLine("Debug\t" + message);
		}

		public void Info(string message)
		{
			Console.WriteLine("Info\t" + message);
		}

		public void Warn(string message)
		{
			Console.WriteLine("Warn\t" + message);
		}

		public void Error(string message)
		{
			Console.WriteLine("Error\t" + message);
		}

		public void Fatal(string message)
		{
			Console.WriteLine("Fatal\t" + message);
		}
	}
}
