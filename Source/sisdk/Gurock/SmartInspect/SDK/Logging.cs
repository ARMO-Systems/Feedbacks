using System;
using System.Drawing;
using Gurock.SmartInspect;

namespace Gurock.SmartInspect.SDK
{
	public static class Logging
	{
		private static Session m_SDK;

		static Logging()
		{
			m_SDK = SiAuto.Si.AddSession("SDK");
		}

		public static Session SDK
		{
			get { return m_SDK; }
		}
	}
}
