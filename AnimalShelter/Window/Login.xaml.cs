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
using AnimalShelter.Validation;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;


namespace AnimalShelter
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login
    {
        string usernameLogged;

        public Login()
        {
            InitializeComponent();
            password.BorderBrush = username.BorderBrush;
            username.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Validator.UsernameAndPasswordValidator(username) && Validator.UsernameAndPasswordValidator(password))
            {
                if (DBLogin())
                {
                    MainWindow mainWindow = new MainWindow(usernameLogged);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    wrongCombo.Content = "Грешна комбинация";
                    wrongCombo.Visibility = Visibility.Visible;
                }
            }
            else
            {
                wrongCombo.Content = "Въведете Валидни данни";
                wrongCombo.Visibility = Visibility.Visible;
            }
        }

        private bool DBLogin() {
            try
            {
                var db = DBConfig.Connection;    
                MySqlCommand cmd = new MySqlCommand("LoginProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_profile_name", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.InputOutput;
                cmd.Parameters.Add(new MySqlParameter("p_profile_password", MySqlDbType.VarChar));
                cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = username.Text;
                cmd.Parameters[1].Value = password.Password.ToString();

                db.Open();

                int result = cmd.ExecuteNonQuery();

                db.Close();

                if (result > 0)
                {
                    usernameLogged = (string)cmd.Parameters["p_profile_name"].Value;
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e) {
                MessageBox.Show(e.ToString());
                return false;
            }
        }
    }
}
