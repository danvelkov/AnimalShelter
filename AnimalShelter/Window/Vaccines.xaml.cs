using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using AnimalShelter.Window;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace AnimalShelter.Window
{
    /// <summary>
    /// Interaction logic for Vaccines.xaml
    /// </summary>
    public partial class Vaccines
    {
        public int passportId { get; set; }
        bool nameSort = false;
        bool shotSort = false;
        bool expirationSort = false;

        public Vaccines()
        {
            InitializeComponent();
            LoadComboBox();
        }

        private void AnimalInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            animalInfo.Clear();

            FindAnimal findAnimal = new FindAnimal();
            findAnimal.exportButton.Visibility = Visibility.Hidden;
            findAnimal.ShowDialog();
            this.passportId = findAnimal.ChooseAnimal();

            using (var db = DBConfig.Connection)
            {
                db.Open();
                MySqlDataReader reader = null;
                string selectCmd = "SELECT p.animal_name, b.animal_type_name FROM passport p JOIN animal_type b USING(animal_type_id) WHERE p.passport_id =" + this.passportId;

                MySqlCommand command = new MySqlCommand(selectCmd, db);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string animalName = (string)reader["animal_name"];
                    string animalType = (string)reader["animal_type_name"];

                    animalInfo.Text = animalName + " " + animalType;
                }
                db.Close();
            }

            LoadVaccines();
        }

        private void LoadVaccines()
        {
            try
            {
                using (var db = DBConfig.Connection)
                {
                    MySqlCommand sql_cmd = db.CreateCommand();
                    sql_cmd.CommandText = "SELECT date_shot, date_expiration, v.vaccine_name " +
                    "FROM vaccines_taken join vaccine v using (vaccine_id) where passport_id = " + this.passportId + " and date_expiration > curdate() ";
                    MySqlDataAdapter dab = new MySqlDataAdapter(sql_cmd.CommandText, db);
                    DataTable dt = new DataTable();
                    dab.Fill(dt);
                    vaccineList.ItemsSource = dt.DefaultView;
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void LoadComboBox()
        {
            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlDataAdapter dab = new MySqlDataAdapter("SELECT vaccine_id, vaccine_name FROM vaccine", db);
                    DataSet ds = new DataSet();
                    dab.Fill(ds, "vaccine");
                    vaccinesCombo.DataContext = ds.Tables["vaccine"].DefaultView;
                    vaccinesCombo.DisplayMemberPath = "vaccine_name";
                    vaccinesCombo.SelectedValuePath = "vaccine_id";
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void AddVaccineButton_Click(object sender, RoutedEventArgs e)
        {
            if(vaccinesCombo.SelectedIndex > -1 && passportId > 0)
                AddVaccine();
        }

        private void AddVaccine()
        { 
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("AdministerVaccineProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_passport_id", MySqlDbType.Int16));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(new MySqlParameter("p_vaccine_id", MySqlDbType.Int16));
                cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = this.passportId;
                cmd.Parameters[1].Value = vaccinesCombo.SelectedValue;

                db.Open();
                int result = cmd.ExecuteNonQuery();
                db.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }

            LoadVaccines();
        }

        private void listViewSort(object sender, RoutedEventArgs e)
        {
            var tag = ((GridViewColumnHeader)sender).Tag;
            switch (tag.ToString())
            {
                case "1": sort(ref nameSort, vaccineList, "vaccine_name"); break;                
                case "2": sort(ref shotSort, vaccineList, "date_shot"); break; 
                case "3": sort(ref expirationSort, vaccineList, "date_expiration"); break;           
                default: break;
            }
        }

        private void sort(ref bool flag, ListView tableName, string tableColumn)
        {
            if (flag == false)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(tableName.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(tableColumn, ListSortDirection.Ascending));
                flag = true;
            }
            else
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(tableName.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(tableColumn, ListSortDirection.Descending));
                flag = false;
            }
        }
    }
}
