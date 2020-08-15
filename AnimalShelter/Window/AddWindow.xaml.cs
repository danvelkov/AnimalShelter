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
using AnimalShelter.Window;
using AnimalShelter.Validation;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace AnimalShelter.Window
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    /// 
    public partial class AddWindow
    {
        private string photoPath;
        bool nameSort = false;
        bool durationSort = false;

        public AddWindow()
        {
            InitializeComponent();
            FillComboBox();
            LoadType();
            LoadVaccine();
        }

        private void PictureButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                uploadedPhoto.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                photoPath = openFileDialog.FileName;
            }  
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
                    animalType.DataContext = ds.Tables["animal_type"].DefaultView;
                    animalType.DisplayMemberPath = "animal_type_name";
                    animalType.SelectedValuePath = "animal_type_id";
                    db.Close();
                }

                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlDataAdapter dab = new MySqlDataAdapter("SELECT compartment_id, compartment_name FROM compartment", db);
                    DataSet ds = new DataSet();
                    dab.Fill(ds, "compartment");
                    compartment.DataContext = ds.Tables["compartment"].DefaultView;
                    compartment.DisplayMemberPath = "compartment_name";
                    compartment.SelectedValuePath = "compartment_id";
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void AddAnimal()
        {
           /* if (!string.IsNullOrWhiteSpace(animalName.Text) && animalType.SelectedIndex > -1 &&
                !string.IsNullOrWhiteSpace(animalAge.Text) && !string.IsNullOrWhiteSpace(animalColor.Text)
                && compartment.SelectedIndex > -1 && !string.IsNullOrWhiteSpace(photoPath))*/
            if(Validator.TextValidator(animalName) && animalType.SelectedIndex > -1 && Validator.NumberValidator(animalAge) &&
                Validator.TextValidator(animalColor) && compartment.SelectedIndex > -1 && !string.IsNullOrWhiteSpace(photoPath))
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("InsertAnimalProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_animal_name", MySqlDbType.VarChar));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_animal_type_id", MySqlDbType.VarChar));
                    cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_animal_age", MySqlDbType.VarChar));
                    cmd.Parameters[2].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_animal_color", MySqlDbType.VarChar));
                    cmd.Parameters[3].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_compartment_id", MySqlDbType.VarChar));
                    cmd.Parameters[4].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_photo_path", MySqlDbType.VarChar));
                    cmd.Parameters[5].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = animalName.Text;
                    cmd.Parameters[1].Value = animalType.SelectedValue;
                    cmd.Parameters[2].Value = animalAge.Text;
                    cmd.Parameters[3].Value = animalColor.Text;
                    cmd.Parameters[4].Value = compartment.SelectedValue;
                    cmd.Parameters[5].Value = photoPath;

                    db.Open();
                    var result = cmd.ExecuteNonQuery();
                    db.Close();

                    animalName.Clear();
                    animalType.SelectedIndex = -1;
                    animalAge.Clear();
                    animalColor.Clear();
                    compartment.SelectedIndex = -1;
                    uploadedPhoto.Source = null;
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

        private void LoadType()
        {
            try
            {
                using ( var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlCommand sql_cmd = db.CreateCommand();
                    sql_cmd.CommandText = "SELECT animal_type_id,animal_type_name FROM animal_type";
                    DataTable dt = new DataTable();
                    dt.Load(sql_cmd.ExecuteReader());
                    typeList.ItemsSource = dt.DefaultView;
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
                    MySqlCommand sql_cmd = db.CreateCommand();
                    sql_cmd.CommandText = "SELECT vaccine_name, vaccine_duration FROM vaccine";
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

        private void AddAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            AddAnimal();
        }

        private void AddTypeButton_Click(object sender, RoutedEventArgs e)
        {
            AddType();
        }

        private void AddType()
        {
            //if (!string.IsNullOrWhiteSpace(newType.Text))
            if(Validator.TextValidator(newType))
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("InsertTypeProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_animal_type_name", MySqlDbType.VarChar));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = newType.Text;

                    db.Open();
                    var result = cmd.ExecuteNonQuery();
                    db.Close();
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.ToString());
                }

                newType.Clear();
                LoadType();
                notCorrectFieldsType.Visibility = Visibility.Hidden;
            }
            else 
            {
                notCorrectFieldsType.Visibility = Visibility.Visible;
            }
        }

        private void AddVaccineButton_Click(object sender, RoutedEventArgs e)
        {
            AddVaccine();
        }

        private void AddVaccine()
        {
            //if (!string.IsNullOrWhiteSpace(newVaccine.Text) && !string.IsNullOrWhiteSpace(vaccineDuration.Text))
            if (Validator.UsernameAndPasswordValidator(newVaccine) && !string.IsNullOrWhiteSpace(vaccineDuration.Text))
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("InsertVaccineProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_vaccine_name", MySqlDbType.VarChar));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_vaccine_duration", MySqlDbType.VarChar));
                    cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = newVaccine.Text;
                    cmd.Parameters[1].Value = vaccineDuration.Text;

                    db.Open();
                    var result = cmd.ExecuteNonQuery();
                    db.Close();
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.ToString());
                }

                newVaccine.Clear();
                vaccineDuration.Clear();
                LoadVaccine();
                notCorrectFieldsVaccine.Visibility = Visibility.Hidden;
            }
            else
            {
                notCorrectFieldsVaccine.Visibility = Visibility.Visible;
            }
        }

        private void listViewSort(object sender, RoutedEventArgs e)
        {
            var tag = ((GridViewColumnHeader)sender).Tag;
            switch (tag.ToString())
            {
                case "1": sort(ref nameSort, vaccineList, "vaccine_name"); break;
                case "2": sort(ref durationSort, vaccineList, "vaccine_duration"); break;
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
