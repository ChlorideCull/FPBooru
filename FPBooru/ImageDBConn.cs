using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace FPBooru
{
	public class ImageDBConn
	{
		MySqlCommand addImageCmd;
		MySqlCommand getImageCmd;
		MySqlCommand getImageCountCmd;
		MySqlCommand getImagesCmd;
		MySqlCommand getImageByTagsCmd;
		MySqlCommand addTagCmd;
		MySqlCommand resolveTagCmd;
		MySqlCommand resolveTagIDCmd;

		public ImageDBConn(MySqlConnection conn) {
			addImageCmd = new MySqlCommand("INSERT INTO fpbooru.images (thumbnailimg, imagepath_csv, tagids_csv, time_created, time_updated) VALUES (@thumbnailimg, @images, @tagids, UTC_TIMESTAMP(), UTC_TIMESTAMP());", conn);
			getImageCmd = new MySqlCommand("SELECT id, thumbnailimg, imagepath_csv, tagids_csv FROM fpbooru.images WHERE id = @id;", conn);
			getImageCountCmd = new MySqlCommand("SELECT COUNT(*) FROM fpbooru.images;", conn);
			getImagesCmd = new MySqlCommand("SELECT id, thumbnailimg, imagepath_csv, tagids_csv FROM fpbooru.images ORDER BY id DESC LIMIT @itemmin, @itemmax;", conn);
			addTagCmd = new MySqlCommand("INSERT INTO fpbooru.tags (imageids_csv, name) VALUES ('', @name);", conn);
			resolveTagCmd = new MySqlCommand("SELECT id FROM fpbooru.tags WHERE name=@nom;", conn);
			resolveTagIDCmd = new MySqlCommand("SELECT name FROM fpbooru.tags WHERE id=@theid;", conn);
			getImageByTagsCmd = new MySqlCommand("SELECT id, thumbnailimg, imagepath_csv, tagids_csv FROM fpbooru.images WHERE tagids_csv REGEXP @regex ORDER BY id DESC LIMIT @itemmin, @itemmax;", conn);

			addImageCmd.Prepare();
			getImageCmd.Prepare();
			getImageCountCmd.Prepare();
			getImagesCmd.Prepare();
			addTagCmd.Prepare();
			resolveTagCmd.Prepare();
			resolveTagIDCmd.Prepare();
			getImageByTagsCmd.Prepare();
		}

		public long AddImage(Image img) {
			addImageCmd.Parameters.Clear();
			addImageCmd.Parameters.AddWithValue("@thumbnailimg", img.thumbnailname);

			string imagepathcsv = "";
			foreach (string str in img.imagenames) {
				imagepathcsv += str + ",";
			}
			addImageCmd.Parameters.AddWithValue("@images", imagepathcsv);

			string tagscsv = "";
			foreach (int str in img.tagids) {
				tagscsv += str + ",";
			}
			addImageCmd.Parameters.AddWithValue("@tagids", tagscsv);
			addImageCmd.ExecuteNonQuery();
			return addImageCmd.LastInsertedId;
		}

		public long AddTag(string tagname) {
			addTagCmd.Parameters.Clear();
			addTagCmd.Parameters.AddWithValue("@name", tagname);
			addTagCmd.ExecuteNonQuery();
			return addTagCmd.LastInsertedId;
		}

		public Image GetImage(int id) {
			getImageCmd.Parameters.Clear();
			getImageCmd.Parameters.AddWithValue("@id", id);
			MySqlDataReader red = getImageCmd.ExecuteReader();
			return IterateImageReader(red)[0];
		}

		public Image[] GetImages(long page) {
			getImagesCmd.Parameters.Clear();
			getImagesCmd.Parameters.AddWithValue("@itemmin", 16 * page);
			getImagesCmd.Parameters.AddWithValue("@itemmax", 16 * (page + 1));
			MySqlDataReader red = getImagesCmd.ExecuteReader();
			return IterateImageReader(red);
		}

		public long GetImages() {
			using (MySqlDataReader red = getImageCountCmd.ExecuteReader()) {
				if (red.Read()) {
					return red.GetInt64(0);
				} else {
					return -1;
				}
			}
		}

		public Image[] GetImages(long page, long[] tags) {
			//REGEX Basic: The CSV field always end with a comma. Regex would be tag1,|tag2,|tag3, etc.
			string regex = "";
			foreach (int tag in tags)
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

		public long ResolveTag(string tagname, bool createifnotfound) {
			resolveTagCmd.Parameters.Clear();
			resolveTagCmd.Parameters.AddWithValue("@nom", tagname);
			using (MySqlDataReader red = resolveTagCmd.ExecuteReader()) {
				if (red.Read()) {
					return red.GetUInt32(red.GetOrdinal("id"));
				} else if (createifnotfound) {
					red.Close();
					return AddTag(tagname);
				} else {
					return -1;
				}
			}
		}

		public string ResolveTag(long id) {
			resolveTagIDCmd.Parameters.Clear();
			resolveTagIDCmd.Parameters.AddWithValue("@theid", id);
			using (MySqlDataReader red = resolveTagIDCmd.ExecuteReader()) {
				if (red.Read()) {
					return red.GetString(red.GetOrdinal("name"));
				} else {
					return "";
				}
			}
		}

		private static Image[] IterateImageReader(MySqlDataReader red) {
			List<Image> output = new List<Image>();
			while (red.Read())
			{
				Image tmp = new Image();
				tmp.id = red.GetInt64(red.GetOrdinal("id"));
				tmp.thumbnailname = red.GetString(red.GetOrdinal("thumbnailimg"));
				tmp.imagenames = red.GetString(red.GetOrdinal("imagepath_csv")).Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
				List<long> tmplist = new List<long>();
				foreach (string i in red.GetString(red.GetOrdinal("tagids_csv")).Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
				{
					tmplist.Add(Convert.ToInt64(i));
				}
				tmp.tagids = tmplist.ToArray();
				output.Add(tmp);
			}
			red.Close();
			return output.ToArray();
		}
	}
}

