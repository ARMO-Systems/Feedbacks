using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Gurock.SmartInspect.SDK
{
	public abstract class LogStream: ILog
	{
		private Stream m_Stream;

		protected LogStream(Stream stream)
		{
			m_Stream = new BufferedStream(stream);
		}

		protected virtual void Initialize(Stream stream)
		{
		}

		public IEnumerator<Packet> GetEnumerator()
		{
			Initialize(m_Stream);
			BinaryReader reader = new BinaryReader(m_Stream);

			while (true)
			{
				Packet packet = null;

			    try
			    {
			        BeforePacket(m_Stream);
			        packet = ReadPacket(reader);
			    }
			    catch (EndOfStreamException)
			    {
			        Logging.SDK.LogWarning("EOF");
			    }
			    catch (SmartInspectException ex)
			    {
			        yield break;
			    }

				if (packet == null)
				{
					break;
				}
				else
				{
					AfterPacket(m_Stream);
					yield return packet;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected virtual void BeforePacket(Stream stream)
		{
		}

		private static Packet ReadPacket(BinaryReader reader)
		{
			short type = reader.ReadInt16();
			int size = reader.ReadInt32();
			return PacketFactory.GetPacket(reader, type, size);
		}

		protected virtual void AfterPacket(Stream stream)
		{
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_Stream.Close();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
