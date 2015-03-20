using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace FPBooru
{
	public class ImageDBConn
	{
		MySqlCommand getImageCmd;
		MySqlCommand getImageByTagsCmd;

		public ImageDBConn(MySqlConnection conn) {
			getImageCmd = new MySqlCommand("SELECT id, imagepath_csv, tagids_csv FROM fpbooru.images ORDER BY id DESC LIMIT @itemmin, @itemmax;", conn);
			getImageByTagsCmd = new MySqlCommand("SELECT id, imagepath_csv, tagids_csv FROM fpbooru.images WHERE tagids_csv REGEXP '@regex' ORDER BY id DESC LIMIT @itemmin, @itemmax;", conn);
			getImageCmd.Prepare();
			getImageByTagsCmd.Prepare();
		}

		public Image[] GetImages(int page) {
			getImageCmd.Parameters.Clear();
			getImageCmd.Parameters.AddWithValue("@itemmin", 16 * page);
			getImageCmd.Parameters.AddWithValue("@itemmax", 16 * (page + 1));
			MySqlDataReader red = getImageCmd.ExecuteReader();
			return IterateImageReader(red);
		}

		public Image[] GetImages(int page, string[] tags) {
			//REGEX Basic: The CSV field always end with a comma. Regex would be tag1,|tag2,|tag3, etc.
			string regex = "";
			foreach (string tag in tags)
			{
				regex += tag + ",|";
			}
			regex = regex.Substring(0, regex.Length - 1);

			getImageByTagsCmd.Parameters.Clear();
			getImageByTagsCmd.Parameters.AddWithValue("@regex", regex);
			getImageByTagsCmd.Parameters.AddWithValue("@itemmin", 16 * page);
			getImageByTagsCmd.Parameters.AddWithValue("@itemmax", 16 * (page + 1));
			MySqlDataReader red = getImageByTagsCmd.ExecuteReader();
			return IterateImageReader(red);
		}

		private static Image[] IterateImageReader(MySqlDataReader red) {
			List<Image> output = new List<Image>();
			while (red.Read())
			{
				Image tmp = new Image();
				tmp.id = red.GetInt32(red.GetOrdinal("id"));
				tmp.imagepaths = red.GetString(red.GetOrdinal("imagepath_csv")).Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
				List<int> tmplist = new List<int>();
				foreach (string i in red.GetString(red.GetOrdinal("tagids_csv")).Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
				{
					tmplist.Add(Convert.ToInt32(i));
				}
				tmp.tagids = tmplist.ToArray();
				output.Add(tmp);
			}
			return output.ToArray();
		}
	}
}

