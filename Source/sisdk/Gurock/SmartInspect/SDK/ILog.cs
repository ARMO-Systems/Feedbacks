using System;
using System.Collections.Generic;

namespace Gurock.SmartInspect.SDK
{
	public interface ILog: IEnumerable<Packet>, IDisposable
	{
	}
}
