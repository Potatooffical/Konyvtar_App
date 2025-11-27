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
            cb_mufaj.ItemsSource= mufajlista;
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
            if (cb_mufaj.SelectedIndex != 0 )
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
                        else if (int.TryParse(tbx_konyvkiadaseve.Text, out  kiadasev) && kiadasev>2025)
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
                            cmd.Parameters.AddWithValue("@elerheto",elerheto);
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
            cb_konyvmufaj.SelectedIndex=0;
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
            tbx_konyvkiadaseve.Text = sor["mufaj"].ToString();
            cb_konyvmufaj.SelectedItem = sor["ev"].ToString();
            //folytkov
        }
    }
}
