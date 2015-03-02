using System;

namespace FPBooru
{
	public struct Image
	{
		public int id;
		public string[] imagepaths;
		public int[] tagids;
		public DateTime created;
		public DateTime edited;
	}
}

