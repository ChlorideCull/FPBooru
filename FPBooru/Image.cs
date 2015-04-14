using System;

namespace FPBooru
{
	public struct Image
	{
		public long id;
		public string[] imagepaths;
		public long[] tagids;
		public DateTime created;
		public DateTime edited;
	}
}

