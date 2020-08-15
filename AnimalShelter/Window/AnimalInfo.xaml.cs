using System;
using System.IO;
using Microsoft.Win32;
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
using AnimalShelter.Validation;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace AnimalShelter.Window
{
    /// <summary>
    /// Interaction logic for AnimalInfo.xaml
    /// </summary>
    public partial class AnimalInfo
    {
        bool nameSort = false;
        bool shotSort = false;
        bool expirationSort = false;

        bool isEdit = false;
        string photoPath = null;

        private int passportId { get; set; }

        public AnimalInfo(int passportId)
        {
            InitializeComponent();
            this.passportId = passportId;
            LoadContent();
            LoadVaccines();
        }

        private void LoadContent()
        {
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("GetAnimalInfoProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_passport_id", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(new MySqlParameter("p_animal_name", MySqlDbType.VarChar));
                cmd.Parameters[1].Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(new MySqlParameter("p_animal_age", MySqlDbType.VarChar));
                cmd.Parameters[2].Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(new MySqlParameter("p_animal_color", MySqlDbType.VarChar));
                cmd.Parameters[3].Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(new MySqlParameter("p_animal_type_name", MySqlDbType.VarChar));
                cmd.Parameters[4].Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(new MySqlParameter("p_compartment_name", MySqlDbType.VarChar));
                cmd.Parameters[5].Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(new MySqlParameter("p_photo_path", MySqlDbType.VarChar));
                cmd.Parameters[6].Direction = System.Data.ParameterDirection.Output;

                cmd.Parameters[0].Value = passportId;

                string animalName = "", animalAge = "", animalColor = "", animalTypeName = "", compartmentName = "", photoPath = "";

                db.Open();

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    animalName = (string)cmd.Parameters["p_animal_name"].Value;
                    animalAge = (string)cmd.Parameters["p_animal_age"].Value;
                    animalColor = (string)cmd.Parameters["p_animal_color"].Value;
                    animalTypeName = (string)cmd.Parameters["p_animal_type_name"].Value;
                    compartmentName = (string)cmd.Parameters["p_compartment_name"].Value;
                    photoPath = (string)cmd.Parameters["p_photo_path"].Value;
                }
                db.Close();

                animalNameLabel.Content = animalName;
                animalAgeLabel.Content = animalAge;
                animalColorLabel.Content = animalColor;
                animalTypeLabel.Content = animalTypeName;
                compartmentLabel.Content = compartmentName;
                try
                {
                    animalPhoto.Source = new BitmapImage(new Uri(photoPath));
                }
                catch (System.IO.FileNotFoundException c)
                {
                    animalPhoto.Source = null;
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void LoadVaccines()
        {
            try
            {
                using (var db = DBConfig.Connection)
                {
                    MySqlCommand sql_cmd = db.CreateCommand();
                    sql_cmd.CommandText = "SELECT date_shot, date_expiration, v.vaccine_name " +
                    "FROM vaccines_taken join vaccine v using (vaccine_id) where passport_id = " + this.passportId +" AND date_expiration > curdate()";
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

        private void EditAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            isEdit = true;
            FillComboBox();

            editAnimalName.Visibility = Visibility.Visible;
            editAnimalName.Text = animalNameLabel.Content.ToString();
            editAnimalAge.Visibility = Visibility.Visible;
            editAnimalAge.Text = animalAgeLabel.Content.ToString();
            editAnimalColor.Visibility = Visibility.Visible;
            editAnimalColor.Text = animalColorLabel.Content.ToString();
            editAnimalType.Visibility = Visibility.Visible;
            editCompartment.Visibility = Visibility.Visible;
            saveButton.Visibility = Visibility.Visible;
        }

        private void FillComboBox()
        {

            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlDataAdapter dab = new MySqlDataAdapter("SELECT animal_type_id, animal_type_name FROM animal_type", db);
                    DataSet ds = new DataSet();
                    dab.Fill(ds, "animal_type");
                    editAnimalType.DataContext = ds.Tables["animal_type"].DefaultView;
                    editAnimalType.DisplayMemberPath = "animal_type_name";
                    editAnimalType.SelectedValuePath = "animal_type_id";
                    db.Close();
                }

                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlDataAdapter dab = new MySqlDataAdapter("SELECT compartment_id, compartment_name FROM compartment", db);
                    DataSet ds = new DataSet();
                    dab.Fill(ds, "compartment");
                    editCompartment.DataContext = ds.Tables["compartment"].DefaultView;
                    editCompartment.DisplayMemberPath = "compartment_name";
                    editCompartment.SelectedValuePath = "compartment_id";
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void SaveAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (editAnimalType.SelectedIndex > -1 && editCompartment.SelectedIndex > -1)
            {
                isEdit = false;
                UpdateAnimal();

                editAnimalName.Visibility = Visibility.Hidden;
                editAnimalAge.Visibility = Visibility.Hidden;
                editAnimalColor.Visibility = Visibility.Hidden;
                editAnimalType.Visibility = Visibility.Hidden;
                editCompartment.Visibility = Visibility.Hidden;
                saveButton.Visibility = Visibility.Hidden;

                notCorrectFields.Visibility = Visibility.Hidden;
                LoadContent();
            } 
            else
            {
                notCorrectFields.Visibility = Visibility.Visible;
            }
        }

        private void ContentControl_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (isEdit)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    animalPhoto.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                    photoPath = openFileDialog.FileName;
                }
            }
        }

        private void UpdateAnimal()
        {
           /* if (!string.IsNullOrWhiteSpace(editAnimalName.Text) && editAnimalType.SelectedIndex > -1 &&
                !string.IsNullOrWhiteSpace(editAnimalAge.Text) && !string.IsNullOrWhiteSpace(editAnimalColor.Text)
                && editCompartment.SelectedIndex > -1)*/
            if(Validator.TextValidator(editAnimalName) && editAnimalType.SelectedIndex > -1 &&
                Validator.NumberValidator(editAnimalAge) && Validator.TextValidator(editAnimalColor) 
                && editCompartment.SelectedIndex > -1)
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("UpdateAnimalProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_passport_id", MySqlDbType.VarChar));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_animal_name", MySqlDbType.VarChar));
                    cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_animal_type_id", MySqlDbType.VarChar));
                    cmd.Parameters[2].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_animal_age", MySqlDbType.VarChar));
                    cmd.Parameters[3].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_animal_color", MySqlDbType.VarChar));
                    cmd.Parameters[4].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_compartment_id", MySqlDbType.VarChar));
                    cmd.Parameters[5].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_photo_path", MySqlDbType.VarChar));
                    cmd.Parameters[6].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = this.passportId;
                    cmd.Parameters[1].Value = editAnimalName.Text;
                    cmd.Parameters[2].Value = editAnimalType.SelectedValue;
                    cmd.Parameters[3].Value = editAnimalAge.Text;
                    cmd.Parameters[4].Value = editAnimalColor.Text;
                    cmd.Parameters[5].Value = editCompartment.SelectedValue;
                    cmd.Parameters[6].Value = photoPath;

                    db.Open();
                    var result = cmd.ExecuteNonQuery();
                    db.Close();
        
                    notCorrectFields.Visibility = Visibility.Hidden;
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
            else
            {
                notCorrectFields.Visibility = Visibility.Visible;
            }
        }
    }
}
