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
    /// Interaction logic for EditProfile.xaml
    /// </summary>
    public partial class EditProfile
    {
        public string usernameLogged { get; set; }

        public EditProfile(string username)
        {
            InitializeComponent();
            this.usernameLogged = username;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateProfile();
        }

        private void UpdateProfile()
        {
            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("UpdateProfileProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_profile_name", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.InputOutput;
                cmd.Parameters.Add(new MySqlParameter("p_profile_password", MySqlDbType.VarChar));
                cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(new MySqlParameter("p_person_name", MySqlDbType.VarChar));
                cmd.Parameters[2].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(new MySqlParameter("p_email", MySqlDbType.VarChar));
                cmd.Parameters[3].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = usernameLogged;
                cmd.Parameters[1].Value = newPassword.Password.ToString();
                cmd.Parameters[2].Value = newName.Text;
                cmd.Parameters[3].Value = newEmail.Text;

                db.Open();
                int result = cmd.ExecuteNonQuery();
                db.Close();

                this.Close();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
