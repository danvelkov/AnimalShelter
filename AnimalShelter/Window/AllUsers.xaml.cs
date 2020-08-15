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
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace AnimalShelter.Window
{
    /// <summary>
    /// Interaction logic for AllUsers.xaml
    /// </summary>
    public partial class AllUsers
    {

        List<User> userList = new List<User>();
        bool nameSort = false;
        bool personSort = false;
        bool emailSort = false;

        public AllUsers()
        {
            InitializeComponent();
            LoadUsers();

            userListView.ItemsSource = userList;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(userListView.ItemsSource);
            view.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as User).profileName.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item as User).personName.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item as User).email.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(userListView.ItemsSource).Refresh();
        }

        public class User
        {
            public int profiletId { get; set; }

            public string profileName { get; set; }

            public string personName { get; set; }

            public string email { get; set; }
        }

        private void LoadUsers()
        {
            userList.Clear();

            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlDataReader reader = null;
                    string selectCmd = "SELECT profile_id, profile_name, person_name, email FROM profile";

                    MySqlCommand command = new MySqlCommand(selectCmd, db);
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int profileId = (int)reader["profile_id"];
                        string profileName = (string)reader["profile_name"];
                        string personName = (string)reader["person_name"];
                        string email = (string)reader["email"];

                        User buff = new User();
                        buff.profiletId = profileId;
                        buff.profileName = profileName;
                        buff.personName = personName;
                        buff.email = email;

                        userList.Add(buff);
                    }
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }

            userListView.ItemsSource = null;
            userListView.ItemsSource = userList;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(userListView.ItemsSource);
            view.Filter = UserFilter;
        }
            

        private void listViewSort(object sender, RoutedEventArgs e)
        {
            var tag = ((GridViewColumnHeader)sender).Tag;
            switch (tag.ToString())
            {
                case "1": sort(ref nameSort, userListView, "profileName"); break;
                case "2": sort(ref personSort, userListView, "personName"); break;
                case "3": sort(ref emailSort, userListView, "email"); break;
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

        private void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {
            if(userListView.SelectedItems.Count > 0)
                RemoveUser();
        }

        private void RemoveUser()
        {
            User selectedUser = (User)userListView.SelectedItems[0];
            int userId = selectedUser.profiletId;

            try
            {
                var db = DBConfig.Connection;
                MySqlCommand cmd = new MySqlCommand("RemoveProfileProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_profile_id", MySqlDbType.VarChar));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = userId;

                db.Open();
                int result = cmd.ExecuteNonQuery();
                db.Close();

                LoadUsers();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
