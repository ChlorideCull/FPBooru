using System;
using MySql.Data.MySqlClient;

namespace FPBooru
{
    public static class ImageDBConn
    {
        public static string[][] GetImages(MySqlConnection conn, int page) {
            MySqlCommand cmd = new MySqlCommand("SELECT id, imagepath_csv FROM fpbooru.images ORDER BY id DESC LIMIT " + (16 * page) + "," + (16 * (page + 1)) + ";", conn);
            MySqlDataReader red = cmd.ExecuteReader();
            //TODO: Need the SQL before making the foreach
        }

        public static string[][] GetImages(MySqlConnection conn, int page, string[] tags) {
            //REGEX Basic: The CSV field always end with a comma. Regex would be tag1,|tag2,|tag3, etc.
            string regex;
            foreach (string tag in tags)
            {
                regex += tag + ",|";
            }
            regex = regex.Substring(0, regex.Length - 1);
            MySqlCommand cmd = new MySqlCommand("SELECT id, imagepath_csv FROM fpbooru.images WHERE tagids_csv REGEXP '" + regex + "' ORDER BY id DESC LIMIT " + (16 * page) + "," + (16 * (page + 1)) + ";", conn);
            MySqlDataReader red = cmd.ExecuteReader();
            //TODO: Need the SQL before making the foreach
        }
    }
}

