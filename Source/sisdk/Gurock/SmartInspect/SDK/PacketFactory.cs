using System;
using System.IO;
using Gurock.SmartInspect;

namespace Gurock.SmartInspect.SDK
{
	internal static class PacketFactory
	{
		private const int PACKET_HEADER = 6;

		private static bool IsValidPacket(short type)
		{
			return Enum.IsDefined(typeof(PacketType), (int) type);
		}

		public static Packet GetPacket(BinaryReader reader, short type,
			int size)
		{
			if (!IsValidPacket(type))
			{
				throw new SmartInspectException("Unknown packet type");
			}

			Packet packet = null;

			switch ((PacketType) type)
			{
				case PacketType.ControlCommand:
					packet = ReadControlCommand(reader);
					break;

				case PacketType.LogEntry:
					packet = ReadLogEntry(reader);
					break;

				case PacketType.ProcessFlow:
					packet = ReadProcessFlow(reader);
					break;

				case PacketType.Watch:
					packet = ReadWatch(reader);
					break;
			}
			
			packet.Bytes = PACKET_HEADER + size;
			return packet;
		}

		private static Packet ReadControlCommand(BinaryReader reader)
		{
			PacketReader controlCommand = new ControlCommandReader();
			return controlCommand.Read(reader);
		}

		private static Packet ReadLogEntry(BinaryReader reader)
		{
			PacketReader logEntry = new LogEntryReader();
			return logEntry.Read(reader);
		}

		private static Packet ReadProcessFlow(BinaryReader reader)
		{
			PacketReader processFlow = new ProcessFlowReader();
			return processFlow.Read(reader);
		}

		private static Packet ReadWatch(BinaryReader reader)
		{
			PacketReader watch = new WatchReader();
			return watch.Read(reader);
		}
	}
}