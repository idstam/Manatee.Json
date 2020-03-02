﻿using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Manatee.Json.Tests
{
	[SetUpFixture]
	public class OptionsConfigurator
	{
		public class ConsoleLog : ILog
		{
			private static readonly Stopwatch _stopwatch;

			static ConsoleLog()
			{
				_stopwatch = new Stopwatch();
				_stopwatch.Start();
			}

			public void Verbose(string message, LogCategory category)
			{
				if (JsonOptions.LogCategory.HasFlag(category))
					Console.WriteLine($@"[{_stopwatch.Elapsed:mm\:ss\.ffffff}]: {message}");
			}
		}

		[OneTimeSetUp]
		public void Setup()
		{
			if (string.Equals(Environment.GetEnvironmentVariable("LOCAL_TEST"), "true", StringComparison.InvariantCultureIgnoreCase))
				JsonOptions.Log = new ConsoleLog();
		}
	}
}	