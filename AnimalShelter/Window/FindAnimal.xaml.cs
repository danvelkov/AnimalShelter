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
using Microsoft.Win32;
using System.ComponentModel;

namespace AnimalShelter.Window
{
    /// <summary>
    /// Interaction logic for FindAnimal.xaml
    /// </summary>
    public partial class FindAnimal
    {
        List<Animal> animalList = new List<Animal>();
        int passportId = 0;
        bool nameSort = false;
        bool typeSort = false;
        bool ageSort = false;
        bool colorSort = false;

        public FindAnimal()
        {
            InitializeComponent();
            LoadAnimalList();

            animalListView.ItemsSource = animalList;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(animalListView.ItemsSource);
            view.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as Animal).animalName.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item as Animal).animalColor.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item as Animal).animalType.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(animalListView.ItemsSource).Refresh();
        }

        private void LoadAnimalList()
        {
            try
            {
                using (var db = DBConfig.Connection)
                {
                    db.Open();
                    MySqlDataReader reader = null;
                    string selectCmd = "SELECT passport_id, animal_name, animal_age, animal_color, a.animal_type_name FROM passport JOIN animal_type a using(animal_type_id) JOIN animal b using(passport_id) WHERE b.animal_status = 0";

                    MySqlCommand command = new MySqlCommand(selectCmd, db);
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int passportId = (int)reader["passport_id"];
                        string animalName = (string)reader["animal_name"];
                        string animalAge = (string)reader["animal_age"];
                        string animalColor = (string)reader["animal_color"];
                        string animalType = (string)reader["animal_type_name"];

                        Animal buff = new Animal();
                        buff.passportId = passportId;
                        buff.animalName = animalName;
                        buff.animalAge = Int32.Parse(animalAge);
                        buff.animalColor = animalColor;
                        buff.animalType = animalType;

                        animalList.Add(buff);
                    }
                    db.Close();
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public class Animal
        {
            public int passportId { get; set; }

            public string animalName { get; set; }

            public int animalAge { get; set; }

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

        private void ChooseAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (animalListView.SelectedItems.Count > 0)
            {
                Animal selectedAnimal = (Animal)animalListView.SelectedItems[0];
                this.passportId = selectedAnimal.passportId;
                this.Close();
            }
        }


        internal int ChooseAnimal()
        {
            return this.passportId;
        }

        private void listViewSort(object sender, RoutedEventArgs e)
        {
            var tag = ((GridViewColumnHeader)sender).Tag;
            switch (tag.ToString())
            {
                case "1": sort(ref nameSort, animalListView, "animalName"); break;
                case "2": sort(ref typeSort, animalListView, "animalType"); break;
                case "3": sort(ref ageSort, animalListView, "animalAge"); break;
                case "4": sort(ref colorSort, animalListView, "animalColor"); break;
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

        private void exportSheet_Click(object sender, RoutedEventArgs e)
        {
            ExportDataFromServer();
        }

        protected void ExportDataFromServer()
        {
            DataTable table = new DataTable();

            using (var db = DBConfig.Connection)
            {
                db.Open();
                MySqlDataReader reader = null;
                string selectCmd = "Select p.animal_name, p.animal_age, p.animal_color, c.animal_type_name, b.compartment_name from passport p join animal а using (passport_id) join compartment b using (compartment_id) join animal_type c using (animal_type_id)";

                MySqlCommand command = new MySqlCommand(selectCmd, db);
                reader = command.ExecuteReader();

                table.Load(reader);

                ClosedXML.Excel.XLWorkbook wbook = new ClosedXML.Excel.XLWorkbook();
                var ws = wbook.Worksheets.Add(table, "Животни");
                ws.Row(1).InsertRowsAbove(1);
                ws.Cell(1, 1).Value = "Всички животни";

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files|*.xlsx",
                    Title = "Save an Excel File"
                };

                saveFileDialog.ShowDialog();

                if (!String.IsNullOrWhiteSpace(saveFileDialog.FileName))
                    wbook.SaveAs(saveFileDialog.FileName);

                wbook.Dispose();
            }
        }
    }
}
