using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppSQLTermekek
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string kapcsolatLeiro = "datasource=127.0.0.1;port=3306;username=root;password=;database=hardver;";
        List<termek> termekek = new List<termek>();

        MySqlConnection SQLkapcsolat;
        public MainWindow()
        {
            InitializeComponent();

            AdatbazisMegnyitas();

            TermekBetoltese();
            KategoriakBetoltese();
            GyartokBetoltese();

            AdatbazisLezarasa();
        }

        private void AdatbazisLezarasa()
        {
            SQLkapcsolat.Close();
            SQLkapcsolat.Dispose();
        }

        private void TermekBetoltese()
        {
            string sqlOsszesTermek = "Select * From termékek";
            MySqlCommand SQLparancs = new MySqlCommand(sqlOsszesTermek, SQLkapcsolat);
            MySqlDataReader eredmenyOlvaso = SQLparancs.ExecuteReader();

            while (eredmenyOlvaso.Read())
            {
                termek uj = new termek(eredmenyOlvaso.GetString("Kategória"),
                    eredmenyOlvaso.GetString("Gyártó"),
                    eredmenyOlvaso.GetString("Név"),
                    eredmenyOlvaso.GetInt32("Ár"),
                    eredmenyOlvaso.GetInt32("Garidő"));
            }
            eredmenyOlvaso.Close();

            dgTermekek.ItemsSource = termekek;
        }
        private void KategoriakBetoltese()
        {
            string sqlKategoriaTermek = "Select Distinct kategória from termékek GROUP BY kategória";
            MySqlCommand SQLparancs = new MySqlCommand(sqlKategoriaTermek, SQLkapcsolat);
            MySqlDataReader kategoriaOlvaso = SQLparancs.ExecuteReader();

            cbKategoria.Items.Add("- nincs megadva -");
            cbKategoria.SelectedIndex = 0;
            while (kategoriaOlvaso.Read())
            {
                cbKategoria.Items.Add(kategoriaOlvaso.GetString("Kategória"));
            }
            kategoriaOlvaso.Close();
        }

        private void GyartokBetoltese()
        {
            string sqlGyartok = "Select DISTINCT gyártó from termékek group by gyártó";
            MySqlCommand SQLparancs = new MySqlCommand(sqlGyartok, SQLkapcsolat);
            MySqlDataReader gyartoOlvaso = SQLparancs.ExecuteReader();

            cbGyarto.Items.Add(" - nincs megadva - ");
            cbGyarto.SelectedIndex = 0;
            while (gyartoOlvaso.Read())
            {
                cbGyarto.Items.Add(gyartoOlvaso.GetString("Gyártó"));
            }
            gyartoOlvaso.Close();
        }

        private void AdatbazisMegnyitas()
        {
            try
            {
                SQLkapcsolat = new MySqlConnection(kapcsolatLeiro);
                SQLkapcsolat.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Nem tud kapcsolódni az adatbázishoz!");
                this.Close();
                throw;
            }
        }

        private void btnSzukit_Click(object sender, RoutedEventArgs e)
        {
            termekek.Clear();

            AdatbazisMegnyitas();

            string SQLSzukitettLista = SzukitoLekerdezesEloallitasa();


            MySqlCommand SQLparancs = new MySqlCommand(SQLSzukitettLista, SQLkapcsolat);
            MySqlDataReader eredmenyOlvaso = SQLparancs.ExecuteReader();

            while (eredmenyOlvaso.Read())
            {
                termek uj = new termek(eredmenyOlvaso.GetString("Kategória"),
                    eredmenyOlvaso.GetString("Gyártó"),
                    eredmenyOlvaso.GetString("Név"),
                    eredmenyOlvaso.GetInt32("Ár"),
                    eredmenyOlvaso.GetInt32("Garidő"));
                termekek.Add(uj);
            }
            eredmenyOlvaso.Close();
            dgTermekek.Items.Refresh();
            if (dgTermekek.Items.Count == 0)
            {
                MessageBox.Show("Nincs a keresési feltételeknek megfelelő termék!");
            }
            AdatbazisLezarasa();

        }

        private string SzukitoLekerdezesEloallitasa()
        {
            string SQLSzukitettLista = "Select * From termékek ";

            if (cbGyarto.SelectedIndex > 0 || cbKategoria.SelectedIndex > 0 || txtTermek.Text != "")
            {
                SQLSzukitettLista += "Where ";
            }

            if (cbGyarto.SelectedIndex > 0)
            {
                SQLSzukitettLista += $"gyártó='{cbGyarto.SelectedItem}'";
            }

            if (cbKategoria.SelectedIndex > 0)
            {
                if (SQLSzukitettLista[SQLSzukitettLista.Length - 1] == '\'')
                {
                    SQLSzukitettLista += " AND ";
                }
                SQLSzukitettLista += $"kategória='{cbKategoria.SelectedItem}'";
            }

            if (txtTermek.Text != "")
            {
                if (SQLSzukitettLista[SQLSzukitettLista.Length - 1] == '\'')
                {
                    SQLSzukitettLista += " AND ";
                }
                SQLSzukitettLista += $"név LIKE '%{txtTermek.Text}%'";
            }
            return SQLSzukitettLista;
        }

        private void btnMentes_Click(object sender, RoutedEventArgs e)
        {
            /*
            termekek.Clear();

            AdatbazisMegnyitas();

            string SQLSzukitettLista = SzukitoLekerdezesEloallitasa();

            MySqlCommand SQLparancs = new MySqlCommand(SQLSzukitettLista, SQLkapcsolat);
            MySqlDataReader eredmenyOlvaso = SQLparancs.ExecuteReader();

            StreamWriter sw = new StreamWriter("szukitett.csv");
            
            while (eredmenyOlvaso.Read())
            {
                termek uj = new termek(eredmenyOlvaso.GetString("Kategória"),
                    eredmenyOlvaso.GetString("Gyártó"),
                    eredmenyOlvaso.GetString("Név"),
                    eredmenyOlvaso.GetInt32("Ár"),
                    eredmenyOlvaso.GetInt32("Garidő"));
                termekek.Add(uj); 
                sw.WriteLine($"{uj.Kategoria};{uj.Gyarto};{uj.Nev};{uj.Ar};{uj.Garido}");
            }
            sw.Close();

            eredmenyOlvaso.Close();
            dgTermekek.Items.Refresh();  */

            StreamWriter sw = new StreamWriter("szukitett.csv");

            foreach (var item in termekek)
            {
                sw.WriteLine($"{item.Kategoria};{item.Gyarto};{item.Nev};{item.Ar};{item.Garido}");

            }
            sw.Close();
            MessageBox.Show("A szűkített lista mentése sikeresen megtörtént!");
        }

    }
}
