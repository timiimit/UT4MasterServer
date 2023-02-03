using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer.Services.Singleton;

public class RuntimeInfoService
{
	public DateTime StartupTime { get; set; }

	public RuntimeInfoService()
	{
		StartupTime = DateTime.UtcNow;
	}
}
