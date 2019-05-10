using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace Selami
{
    public static class db
    {
       
        public static SQLiteCommand cmd;
        public static SQLiteDataReader dr;
        public static SQLiteConnection baglanti = new SQLiteConnection(@"Data Source=C:\Users\corx\source\repos\Selami\Selami\selami.sqlite;Version=3;");

       
        public static string sor(string sorgu)
        {
            bagAc();
            cmd = new SQLiteCommand("select \"karsilik\" from selami where \"ifade\"="+"\'"+sorgu+"\'", baglanti);
            dr = cmd.ExecuteReader();
            dr.Read();
            string s= dr[0].ToString();
            bagKapat();
            return s;
        }
        public static async void ogret(string ifade, string karsilik)
        { 
            cmd = new SQLiteCommand("INSERT INTO selami(ifade,karsilik) VALUES(\"" + ifade + "\", \"" + karsilik + "\")", baglanti);
            await cmd.ExecuteNonQueryAsync();
        }
        public static void bagAc()
        {
            if (baglanti.State==System.Data.ConnectionState.Closed)
            {
                baglanti.Open();
            }
        }
        public static void bagKapat()
        {
            if (baglanti.State == System.Data.ConnectionState.Open)
            {
                baglanti.Close();
            }
        }
        public static string komutSor(string komut)
        {
            bagAc();
            cmd = new SQLiteCommand("select \"islem\" from komutlar where \"komut\"=" + "\'" + komut + "\'", baglanti);
            dr = cmd.ExecuteReader();
            dr.Read();
            string s = dr[0].ToString();
            bagKapat();
            return s;
        }
   
       

    }
}
