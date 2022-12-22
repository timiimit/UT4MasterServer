using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public struct CommonID
	{
		public string ID { get; private set; }

		public bool IsInvalid
		{
			get
			{
				return ID == GetInvalid().ID;
			}
		}

		public CommonID(string id)
		{
			// TODO: verify that id is hex string
			if (id.Length != 32)
				throw new ArgumentException("id needs to be 32 hexadecimal characters long");
			ID = id;
		}

		public static CommonID GenerateNew()
		{
			Random r = new Random();
			byte[] bytes = new byte[16];
			r.NextBytes(bytes);
			string? id = Convert.ToHexString(bytes);

			return new CommonID(id);
		}

		public static CommonID GetInvalid()
		{
			return new CommonID("00000000000000000000000000000000");
		}

		public static bool operator ==(CommonID lhs, CommonID rhs)
		{
			return lhs.Equals(rhs);
		}
		public static bool operator !=(CommonID lhs, CommonID rhs)
		{
			return !lhs.Equals(rhs);
		}

		public override bool Equals(object? obj)
		{
			if (obj == null)
				return false;

			if (obj is not UT4MasterServer.CommonID)
				return false;

			var objUserID = (CommonID)obj;

			return string.Equals(ID, objUserID.ID);
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}

		public override string ToString()
		{
			return ID;
		}
	}
}
