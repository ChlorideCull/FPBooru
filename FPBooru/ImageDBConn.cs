using System;
using MySql.Data.MySqlClient;

namespace FPBooru
{
    public static class ImageDBConn
    {
        public string[][] GetImages(MySqlConnection conn, int page) {
            MySqlCommand cmd = new MySqlCommand("This SQL is written down somewhere", conn); //TODO Get SQL for paged, all images from notes
            MySqlDataReader red = cmd.ExecuteReader();
            //TODO: Need the SQL before making the foreach
        }

        public string[][] GetImages(MySqlConnection conn, int page, string[] tags) {
            MySqlCommand cmd = new MySqlCommand("This SQL is also written down somewhere", conn); //TODO Get SQL for images with tags in field from notes
            MySqlDataReader red = cmd.ExecuteReader();
            //TODO: Need the SQL before making the foreach
        }
    }
}

