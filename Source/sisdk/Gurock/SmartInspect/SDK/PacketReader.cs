using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Gurock.SmartInspect.SDK
{
    internal abstract class PacketReader
    {
        private const int DAY_OFFSET = 693593;
        private const long TICKS_PER_DAY = 864000000000L;

        protected static void ThrowException()
        {
            throw new SmartInspectException("Invalid packet format detected");
        }

        protected static string ReadString(BinaryReader reader, int size)
        {
            var buffer = new byte[size];

            if (reader.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                ThrowException();
            }

            return Encoding.UTF8.GetString(buffer);
        }

        protected static Color ReadColor(BinaryReader reader)
        {
            var color = reader.ReadInt32();
            var r = color & 0xff;
            var g = (color >> 8) & 0xff;
            var b = (color >> 16) & 0xff;
            var a = (color >> 24) & 0xff;
            return Color.FromArgb(a, r, g, b);
        }

        protected static DateTime ReadTimestamp(BinaryReader reader)
        {
            var timestamp = reader.ReadDouble();
            timestamp += DAY_OFFSET; /* For year 0001 */
            var ticks = (long) (TICKS_PER_DAY*timestamp);
            return new DateTime(ticks);
        }

        protected static void ReadData(BinaryReader from, Stream to,
            int size)
        {
            var buffer = new byte[0x2000];

            while (size > 0)
            {
                int toRead;

                if (buffer.Length > size)
                {
                    toRead = size;
                }
                else
                {
                    toRead = buffer.Length;
                }

                var n = from.Read(buffer, 0, toRead);

                if (n <= 0)
                {
                    break;
                }
                to.Write(buffer, 0, n);
                size -= n;
            }

            if (size > 0)
            {
                ThrowException();
            }
        }

        public abstract Packet Read(BinaryReader reader);
    }

    internal class ControlCommandReader : PacketReader
    {
        private static bool IsValidControlCommand(int type)
        {
            return Enum.IsDefined(typeof(ControlCommandType), type);
        }

        public override Packet Read(BinaryReader reader)
        {
            var controlCommand = new ControlCommand();

            var type = reader.ReadInt32();
            var dataSize = reader.ReadInt32();

            if (!IsValidControlCommand(type))
            {
                ThrowException();
            }

            controlCommand.Level = Level.Control;
            controlCommand.ControlCommandType =
                (ControlCommandType) type;

            if (dataSize > 0)
            {
                Stream data = new MemoryStream();
                ReadData(reader, data, dataSize);
                controlCommand.Data = data;
                controlCommand.Data.Position = 0;
            }

            Logging.SDK.LogObject(Level.Debug, "controlCommand",
                controlCommand);
            return controlCommand;
        }
    }

    internal class LogEntryReader : PacketReader
    {
        private static bool IsValidLogEntry(int type)
        {
            return Enum.IsDefined(typeof(LogEntryType), type);
        }

        private static bool IsValidViewer(int id)
        {
            return Enum.IsDefined(typeof(ViewerId), id);
        }

        public override Packet Read(BinaryReader reader)
        {
            var logEntry = new LogEntry();

            var type = reader.ReadInt32();
            var id = reader.ReadInt32();

            if (!IsValidLogEntry(type) || !IsValidViewer(id))
            {
                ThrowException();
            }

            logEntry.LogEntryType = (LogEntryType) type;
            logEntry.ViewerId = (ViewerId) id;

            var appNameSize = reader.ReadInt32();
            var sessionNameSize = reader.ReadInt32();
            var titleSize = reader.ReadInt32();
            var hostNameSize = reader.ReadInt32();
            var dataSize = reader.ReadInt32();

            if (appNameSize < 0 || sessionNameSize < 0 ||
                titleSize < 0 || hostNameSize < 0 || dataSize < 0)
            {
                ThrowException();
            }

            logEntry.ProcessId = reader.ReadInt32();
            logEntry.ThreadId = reader.ReadInt32();
            logEntry.Timestamp = ReadTimestamp(reader);
            logEntry.Color = ReadColor(reader);
            logEntry.AppName = ReadString(reader, appNameSize);
            logEntry.SessionName = ReadString(reader, sessionNameSize);
            logEntry.Title = ReadString(reader, titleSize);
            logEntry.HostName = ReadString(reader, hostNameSize);

            if (dataSize > 0)
            {
                Stream data = new MemoryStream();
                ReadData(reader, data, dataSize);
                logEntry.Data = data;
                logEntry.Data.Position = 0;
            }

            Logging.SDK.LogObject(Level.Debug, "logEntry", logEntry);
            return logEntry;
        }
    }

    internal class ProcessFlowReader : PacketReader
    {
        private static bool IsValidProcessFlow(int type)
        {
            return Enum.IsDefined(typeof(ProcessFlowType), type);
        }

        public override Packet Read(BinaryReader reader)
        {
            var processFlow = new ProcessFlow();

            var type = reader.ReadInt32();

            if (!IsValidProcessFlow(type))
            {
                ThrowException();
            }

            var titleSize = reader.ReadInt32();
            var hostNameSize = reader.ReadInt32();

            if (titleSize < 0 || hostNameSize < 0)
            {
                ThrowException();
            }

            processFlow.ProcessFlowType = (ProcessFlowType) type;
            processFlow.ProcessId = reader.ReadInt32();
            processFlow.ThreadId = reader.ReadInt32();
            processFlow.Timestamp = ReadTimestamp(reader);
            processFlow.Title = ReadString(reader, titleSize);
            processFlow.HostName = ReadString(reader, hostNameSize);

            Logging.SDK.LogObject(Level.Debug, "processFlow",
                processFlow);
            return processFlow;
        }
    }

    internal class WatchReader : PacketReader
    {
        private static bool IsValidWatch(int type)
        {
            return Enum.IsDefined(typeof(WatchType), type);
        }

        public override Packet Read(BinaryReader reader)
        {
            var watch = new Watch();

            var nameSize = reader.ReadInt32();
            var valueSize = reader.ReadInt32();

            if (nameSize < 0 || valueSize < 0)
            {
                ThrowException();
            }

            var type = reader.ReadInt32();

            if (!IsValidWatch(type))
            {
                ThrowException();
            }

            watch.WatchType = (WatchType) type;
            watch.Timestamp = ReadTimestamp(reader);
            watch.Name = ReadString(reader, nameSize);
            watch.Value = ReadString(reader, valueSize);

            Logging.SDK.LogObject(Level.Debug, "watch", watch);
            return watch;
        }
    }
}