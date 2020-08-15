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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnimalShelter.Validation;
using AnimalShelter.Window;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace AnimalShelter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public string usernameLogged { get; set; }
        public string usernameLoggedName { get; set; }
        public string usernameLoggedEmail { get; set; }

        public int passportId { get; set; }


        public MainWindow(string username)
        {
            InitializeComponent();
            this.usernameLogged = username;
            LoadContent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditProfile editProfile = new EditProfile(this.usernameLogged);
            editProfile.ShowDialog();
            LoadContent();
        }

        private void Animal_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            addWindow.Show();
        }

        private void FindAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            FindAnimal findAnimal = new FindAnimal();
            findAnimal.chooseAnimal.Visibility = Visibility.Hidden;
            findAnimal.Show();
        }

        private void AdoptAnimal()
        {
            /* if (!string.IsNullOrWhiteSpace(adopterName.Text) && !string.IsNullOrWhiteSpace(adopterPin.Text)
                 && !string.IsNullOrWhiteSpace(adopterAddress.Text) && !string.IsNullOrWhiteSpace(adopterNumber.Text))*/
            if (Validator.TextValidator(adopterName) && Validator.PinValidator(adopterPin) && Validator.PhoneNumberValidator(adopterNumber) && Validator.AddressValidator(adopterAddress))
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("AdoptAnimalProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_passport_id", MySqlDbType.VarChar));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_adopter_name", MySqlDbType.VarChar));
                    cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_adopter_pin", MySqlDbType.VarChar));
                    cmd.Parameters[2].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_adopter_address", MySqlDbType.VarChar));
                    cmd.Parameters[3].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_adopter_phone", MySqlDbType.VarChar));
                    cmd.Parameters[4].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = this.passportId;
                    cmd.Parameters[1].Value = adopterName.Text;
                    cmd.Parameters[2].Value = adopterPin.Text;
                    cmd.Parameters[3].Value = adopterAddress.Text;
                    cmd.Parameters[4].Value = adopterNumber.Text;

                    db.Open();
                    int result = cmd.ExecuteNonQuery();
                    db.Close();

                    animalInfo.Clear();
                    adopterName.Clear();
                    adopterPin.Clear();
                    adopterAddress.Clear();
                    adopterNumber.Clear();

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

        private void LoadContent()
        {
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("GetProfileInfoProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_profile_name", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.InputOutput;
                cmd.Parameters.Add(new MySqlParameter("p_person_name", MySqlDbType.VarChar));
                cmd.Parameters[1].Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar));
                cmd.Parameters[2].Direction = System.Data.ParameterDirection.Output;

                cmd.Parameters[0].Value = usernameLogged;

                db.Open();

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    usernameLoggedName = (string)cmd.Parameters["p_person_name"].Value;
                    usernameLoggedEmail = (string)cmd.Parameters["p_email"].Value;
                }
                db.Close();

                username.Content = usernameLogged;
                personName.Content = usernameLoggedName;
                email.Content = usernameLoggedEmail;
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void AdoptButton_Click(object sender, RoutedEventArgs e)
        {
            AdoptAnimal();
        }

        private void AllAdoptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Adoptions adoptionsWindow = new Adoptions();
            adoptionsWindow.Show();
        }

        private void VaccineButton_Click(object sender, RoutedEventArgs e)
        {
            Vaccines vaccinesWindow = new Vaccines();
            vaccinesWindow.Show();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveWindow removeWindow = new RemoveWindow();
            removeWindow.Show();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (adminPassword.Password.ToString().Equals("admin"))
                ShowAdmin();

            adminPassword.Clear();
        }

        private void ShowAdmin()
        {
            LoadCompartment();
            admin.Visibility = Visibility.Visible;
        }

        private void LoadCompartment()
        {
            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlCommand sql_cmd = db.CreateCommand();
                    sql_cmd.CommandText = "SELECT compartment_id, compartment_name FROM compartment";
                    DataTable dt = new DataTable();
                    dt.Load(sql_cmd.ExecuteReader());
                    compartmentList.ItemsSource = dt.DefaultView;
                    compartmentList.SelectedValuePath = "compartment_id";
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }


        private void AddCompartmentButton_Click(object sender, RoutedEventArgs e)
        {
            AddCompartment();
        }

        private void AddCompartment()
        {
            if (Validator.TextValidator(compartmentName))
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("InsertCompartmentProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_compartment_name", MySqlDbType.VarChar));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = compartmentName.Text;

                    db.Open();
                    var result = cmd.ExecuteNonQuery();
                    db.Close();
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.ToString());
                }

                compartmentName.Clear();
                notCorrectFieldsCompartment.Visibility = Visibility.Hidden;
            }
            else
            {
                notCorrectFieldsCompartment.Visibility = Visibility.Visible;
            }

            compartmentName.Clear();
            LoadCompartment();
        }

        private void RemoveCompartmentButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveCompartment();
        }

        private void RemoveCompartment()
        {
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("RemoveCompartmentProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_compartment_id", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = compartmentList.SelectedValue;

                db.Open();
                var result = cmd.ExecuteNonQuery();
                db.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }

            LoadCompartment();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUser();
        }

        private void AllUserButton_Click(object sender, RoutedEventArgs e)
        {
            AllUsers allUsers = new AllUsers();
            allUsers.Show();
        }

        private void AddUser()
        {
           /* if (!string.IsNullOrWhiteSpace(newUsername.Text) && !string.IsNullOrWhiteSpace(newPassword.Password.ToString())
                && !string.IsNullOrWhiteSpace(newName.Text) && !string.IsNullOrWhiteSpace(newEmail.Text))*/
            if(Validator.UsernameAndPasswordValidator(newUsername) && Validator.UsernameAndPasswordValidator(newPassword) && Validator.TextValidator(newName) && Validator.EmailValidator(newEmail))
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("InsertProfileProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_profile_name", MySqlDbType.VarChar));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_profile_password", MySqlDbType.VarChar));
                    cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_person_name", MySqlDbType.VarChar));
                    cmd.Parameters[2].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar));
                    cmd.Parameters[3].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = newUsername.Text;
                    cmd.Parameters[1].Value = newPassword.Password.ToString();
                    cmd.Parameters[2].Value = newName.Text;
                    cmd.Parameters[3].Value = newEmail.Text;

                    db.Open();
                    var result = cmd.ExecuteNonQuery();
                    db.Close();
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.ToString());
                }

                newUsername.Clear();
                newPassword.Clear();
                newName.Clear();
                newEmail.Clear();
                notCorrectFieldsProfile.Visibility = Visibility.Hidden;
            }
            else
            {
                notCorrectFieldsProfile.Visibility = Visibility.Visible;
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("LogoutProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_profile_name", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = usernameLogged;

                db.Open();
                int result = cmd.ExecuteNonQuery();
                db.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
