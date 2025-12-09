using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySqlConnector;

namespace Konyvtar_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int Selecteditem;
        private readonly string connectionstring = "server=localhost;user=root;password=;database=konyvtar_db";
        List<string> mufajlista = new List<string>() { "kérem válasszon" };
        public MainWindow()
        {
            InitializeComponent();
            Adatbetolt();
            Mufajlistahozzaad();
            cb_konyvmufaj.ItemsSource = mufajlista;
            cb_mufaj.ItemsSource = mufajlista;
            cb_mufaj.SelectedIndex = 0;
            cb_konyvmufaj.SelectedIndex = 0;
        }

        private void Adatbetolt()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT id, cim, szerzo, ev, mufaj, elerheto FROM konyvek ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dg_tablazat.ItemsSource = dt.DefaultView;
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Adatbázis hiba vagy inaktiv a szerver \n" + ex.Message,
                                    "Hiba", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
        }
        private void Mufajlistahozzaad()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT mufaj FROM konyvek";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        mufajlista.Add(row["mufaj"].ToString());
                    }

                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Adatbázis hiba vagy inaktiv a szerver \n" + ex.Message,
                                    "Hiba", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
        }

        private void btn_hozzaadas_Click(object sender, RoutedEventArgs e)
        {
            if (cb_mufaj.SelectedIndex != 0)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionstring))
                    {
                        conn.Open();

                        string cim = tbx_konyvcim.Text;
                        string szerzo = tbx_konyvszerzo.Text;
                        int kiadasev;
                        string mufaj = cb_konyvmufaj.SelectedItem.ToString();
                        bool elerheto = false;
                        if (!int.TryParse(tbx_konyvkiadaseve.Text, out int hossz))
                        {
                            MessageBox.Show("Hossz csak szám lehet!");
                            return;
                        }
                        else if (int.TryParse(tbx_konyvkiadaseve.Text, out kiadasev) && kiadasev > 2025)
                        {
                            MessageBox.Show("Bro nincs delorean-nom");
                            return;
                        }
                        if (cbx_kolcsonhezheto.IsChecked == true)
                        {
                            elerheto = true;
                        }
                        else
                        {
                            elerheto = false;
                        }
                        string sql = "INSERT INTO konyvek (cim,szerzo,ev,mufaj,elerheto) VALUES (@cim, @szerzo, @ev, @mufaj,@elerheto)";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@cim", cim);
                            cmd.Parameters.AddWithValue("@szerzo", szerzo);
                            cmd.Parameters.AddWithValue("@ev", kiadasev);
                            cmd.Parameters.AddWithValue("@mufaj", mufaj);
                            cmd.Parameters.AddWithValue("@elerheto", elerheto);
                            int rows = cmd.ExecuteNonQuery();
                            MessageBox.Show(rows > 0 ? "Sikeres hozzáadás!" : "Nem sikerült hozzáadni.");
                        }
                        Adatbetolt();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Adatbázis hiba vagy inaktív a szerver \n" + ex.Message,
                                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Hiányos adatok!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_frissit_Click(object sender, RoutedEventArgs e)
        {
            tbx_konyvszerzo.Text = "";
            tbx_konyvkiadaseve.Text = "";
            tbx_konyvcim.Text = "";
            cb_konyvmufaj.Text = "";
            tbx_cim.Text = "";
            cb_mufaj.SelectedIndex = 0;
            cb_konyvmufaj.SelectedIndex = 0;
            cbx_kolcsonhezheto.IsChecked = false;
            Adatbetolt();
        }

        private void dg_tablazat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView sor = dg_tablazat.SelectedItem as DataRowView;
            if (sor == null) return;
            int.TryParse(sor["id"].ToString(), out Selecteditem);
            tbx_konyvcim.Text = sor["cim"].ToString();
            tbx_konyvszerzo.Text = sor["szerzo"].ToString();
            tbx_konyvkiadaseve.Text = sor["ev"].ToString();
            cb_konyvmufaj.SelectedItem = sor["mufaj"].ToString();
            //folytkov
        }

        private void btn_modositas_Click(object sender, RoutedEventArgs e)
        {
            if (cb_konyvmufaj.SelectedIndex != 0)
            {
                if (Selecteditem == -1)
                {
                    MessageBox.Show("Nincs kiválasztva műfaj :|");
                    return;
                }
                var eredmeny = MessageBox.Show("Biztosan akkarod módositani a sort?", "Modósitás", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (eredmeny != MessageBoxResult.OK)
                {
                    MessageBox.Show("nem történt változás");
                    return;
                }

                else
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionstring))
                    {
                        try
                        {
                            conn.Open();
                            string cim = tbx_konyvcim.Text;
                            string szerzo = tbx_konyvszerzo.Text;
                            if (int.TryParse(tbx_konyvkiadaseve.Text, out int kiadasev) && kiadasev <= 1000)
                            {
                                MessageBox.Show("Túl régi a könyv!");
                            }
                            string korhatar = tbx_konyvszerzo.Text;
                            string mufaj = cb_konyvmufaj.SelectedItem.ToString();
                            bool elerheto;
                            if (cbx_kolcsonhezheto.IsChecked == true)
                            {
                                elerheto = true;
                            }
                            else
                            {
                                elerheto = false;
                            }
                            //UPDATE `konyvek` SET `id`='[value-1]',`cim`='[value-2]',`szerzo`='[value-3]',`ev`='[value-4]',`mufaj`='[value-5]',`elerheto`='[value-6]' WHERE 1
                            string sql = "update konyvek set cim=@cim,szerzo=@szerzo,ev=@kiadasev,mufaj=@mufaj,elerheto=@elerheto where id=@id";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@cim", cim);
                                cmd.Parameters.AddWithValue("@szerzo", szerzo);
                                cmd.Parameters.AddWithValue("@mufaj", mufaj);
                                cmd.Parameters.AddWithValue("@kiadasev", kiadasev);
                                cmd.Parameters.AddWithValue("@id", Selecteditem);
                                cmd.Parameters.AddWithValue("@elerheto", elerheto);

                                int rows = cmd.ExecuteNonQuery();
                                MessageBox.Show(rows > 0 ? "Sikeres módositani!" : "Nem sikerült módositás.");
                            }
                            Adatbetolt();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Adatbázis hiba történt!\n" + ex.Message);
                        }
                    }
                }
            }
        }

        private void btn_torles_Click(object sender, RoutedEventArgs e)
        {
            if (Selecteditem == -1)
            {
                MessageBox.Show("Nincs kiválasztva adat :|");
                return;
            }
            var eredmeny = MessageBox.Show("Biztosan akkarod törölni a sort?", "Törlés", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            if (eredmeny != MessageBoxResult.OK)
            {
                MessageBox.Show("nem történt változás");
                return;
            }
            else
            {
                using (MySqlConnection conn = new MySqlConnection(connectionstring))
                {
                    try
                    {
                        conn.Open();

                        string sql = "DELETE FROM konyvek WHERE id=@id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", Selecteditem);

                            int rows = cmd.ExecuteNonQuery();
                            MessageBox.Show(rows > 0 ? "Sikeres törlés!" : "Nem sikerült törölni.");
                        }
                        Adatbetolt();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Adatbázis hiba történt!\n" + ex.Message);
                    }
                }
            }
        }

        private void tbx_cim_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionstring))
            {
                try
                {
                    string keresett = tbx_cim.Text;
                    if (tbx_cim.Text != "")
                    {
                        conn.Open();
                        keresett = tbx_cim.Text;
                        string sql = "SELECT id,cim,szerzo,ev,mufaj,elerheto FROM konyvek WHERE cim like @keresett";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@keresett", "%" + keresett + "%");
                        MySqlDataAdapter hozzaad = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        hozzaad.Fill(dt);
                        dg_tablazat.ItemsSource = dt.DefaultView;
                    }
                    else
                    {
                        Adatbetolt();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Adatbázis hiba történt!\n" + ex.Message);
                }
            }
        }
    }
}
