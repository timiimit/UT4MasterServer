using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public class Router
	{
		public List<Router> subRouters;

		public Router()
		{
			subRouters = new List<Router>();
		}

		public void Passthrough()
		{
			subRouters
		}
	}
}
