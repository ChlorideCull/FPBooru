﻿using System;

namespace FPBooru
{
	public struct Image
	{
		public long id;
		public string[] imagenames;
		public string thumbnailname;
		public long[] tagids;
		public DateTime created;
		public DateTime edited;
	}
}

