using System;
using System.IO;

namespace Gurock.SmartInspect.SDK
{
	public class LogFile: LogStream
	{
		private const string INVALID_HEADER = 
			"No valid SmartInspect log file header found";

		public LogFile(string fileName): base(File.OpenRead(fileName))
		{ 
		}

		protected override void Initialize(Stream stream)
		{
			bool valid = true;
			byte[] header = new byte[4];

			if (stream.Read(header, 0, header.Length) != header.Length)
			{
				valid = false;
			}
			else 
			{
				Logging.SDK.LogBinary("header", header);

				valid =
					header[0] == 'S' &&
					header[1] == 'I' &&
					header[2] == 'L' &&
					header[3] == 'F';
			}

			if (!valid)
			{
				throw new SmartInspectException(INVALID_HEADER);
			}
		}
	}
}
