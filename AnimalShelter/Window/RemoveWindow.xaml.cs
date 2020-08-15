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

namespace AnimalShelter.Window
{
    /// <summary>
    /// Interaction logic for RemoveWindow.xaml
    /// </summary>
    public partial class RemoveWindow
    {
        List<Animal> animalList = new List<Animal>();

        public RemoveWindow()
        {
            InitializeComponent();
            LoadAnimalList();
            LoadType();
            LoadVaccine();
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as Animal).animalName.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item as Animal).animalType.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item as Animal).animalAge.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item as Animal).animalColor.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(animalListView.ItemsSource).Refresh();
        }

        private void LoadAnimalList()
        {
            animalList.Clear();

            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlDataReader reader = null;
                    string selectCmd = "SELECT passport_id, animal_name, a.animal_type_name, animal_age, animal_color FROM passport JOIN animal_type a USING(animal_type_id)";

                    MySqlCommand command = new MySqlCommand(selectCmd, db);
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int passportId = (int)reader["passport_id"];
                        string animalName = (string)reader["animal_name"];
                        string animalType = (string)reader["animal_type_name"];
                        string animalAge = (string)reader["animal_age"];
                        string animalColor = (string)reader["animal_color"];

                        Animal buff = new Animal();
                        buff.passportId = passportId;
                        buff.animalName = animalName;
                        buff.animalType = animalType;
                        buff.animalAge = animalAge;
                        buff.animalColor = animalColor;

                        animalList.Add(buff);
                    }
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }

            animalListView.ItemsSource = null;
            animalListView.ItemsSource = animalList;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(animalListView.ItemsSource);
            view.Filter = UserFilter;
        }

        public class Animal
        {
            public int passportId { get; set; }

            public string animalName { get; set; }

            public string animalAge { get; set; }

            public string animalColor { get; set; }

            public string animalType { get; set; }
        }

        private void ShowAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (animalListView.SelectedItems.Count > 0)
            {
                Animal selectedAnimal = (Animal)animalListView.SelectedItems[0];
                AnimalInfo animalInfo = new AnimalInfo(selectedAnimal.passportId);
                animalInfo.Show();
            }
        }

        private void RemoveAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            if(animalListView.SelectedItems.Count > 0)
                RemoveAnimal();
        }

        private void RemoveAnimal()
        {
            Animal selectedAnimal = (Animal)animalListView.SelectedItems[0];

            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("RemoveAnimalProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_passport_id", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = selectedAnimal.passportId;

                db.Open();
                int result = cmd.ExecuteNonQuery();
                db.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }

            LoadAnimalList();
        }

        private void LoadType()
        {
            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlCommand sql_cmd = db.CreateCommand();
                    sql_cmd.CommandText = "SELECT animal_type_id,animal_type_name FROM animal_type";
                    DataTable dt = new DataTable();
                    dt.Load(sql_cmd.ExecuteReader());
                    typeList.ItemsSource = dt.DefaultView;
                    typeList.SelectedValuePath = "animal_type_id";
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void LoadVaccine()
        {
            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlCommand sql_cmd = db.CreateCommand();
                    sql_cmd.CommandText = "SELECT vaccine_id, vaccine_name FROM vaccine";
                    DataTable dt = new DataTable();
                    dt.Load(sql_cmd.ExecuteReader());
                    vaccineList.ItemsSource = dt.DefaultView;
                    vaccineList.SelectedValuePath = "vaccine_id";
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void RemoveTypeButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveType();
        }

        private void RemoveType()
        {
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("RemoveTypeProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_animal_type_id", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = typeList.SelectedValue;

                db.Open();
                var result = cmd.ExecuteNonQuery();
                db.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }

            LoadType();
        }

        private void RemoveVaccineButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveVaccine();
        }

        private void RemoveVaccine()
        {
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("RemoveVaccineProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_vaccine_id", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = vaccineList.SelectedValue;

                db.Open();
                var result = cmd.ExecuteNonQuery();
                db.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }

            LoadVaccine();

        }
    }
}
