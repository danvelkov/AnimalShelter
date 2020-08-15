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
    /// Interaction logic for Adoptions.xaml
    /// </summary>
    public partial class Adoptions
    {
        bool nameSort = false;
        bool typeSort = false;
        bool adopterSort = false;
        bool pinSort = false;
        bool addressSort = false;
        bool phoneSort = false;
        bool dateSort = false;

        public Adoptions()
        {
            InitializeComponent();
            LoadAdoptionsByDate();
        }

        private void searchByDate_Click(object sender, RoutedEventArgs e)
        {
            LoadAdoptionsByDate();
        }

        private void LoadAdoptionsByDate()
        {
            if ((startDate.SelectedDate == null && endDate.SelectedDate == null) || ((startDate.SelectedDate != null && endDate.SelectedDate != null)))
            {
                try
                {
                    var db = DBConfig.Connection;
                    MySqlCommand cmd = new MySqlCommand("GetAllAdoptionsProc", db);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("p_start_date", MySqlDbType.Date));
                    cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                    cmd.Parameters.Add(new MySqlParameter("p_end_date", MySqlDbType.Date));
                    cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;

                    cmd.Parameters[0].Value = startDate.SelectedDate;
                    cmd.Parameters[1].Value = endDate.SelectedDate;
                
                    db.Open();
                    MySqlDataAdapter dab = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    dab.Fill(dt);
                    adoptionListView.ItemsSource = dt.DefaultView;
                    db.Close();
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        private void showAnimal_Click(object sender, RoutedEventArgs e)
        {
            if (adoptionListView.SelectedItems.Count > 0)
            {
                int passportId = (int)adoptionListView.SelectedValue;
                AnimalInfo animalInfo = new AnimalInfo(passportId);
                animalInfo.Show();
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
                MySqlCommand cmd = new MySqlCommand("GetAllAdoptionsProc", db);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("p_start_date", MySqlDbType.Date));
                cmd.Parameters[0].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters.Add(new MySqlParameter("p_end_date", MySqlDbType.Date));
                cmd.Parameters[1].Direction = System.Data.ParameterDirection.Input;

                cmd.Parameters[0].Value = startDate.SelectedDate;
                cmd.Parameters[1].Value = endDate.SelectedDate;

                db.Open();

                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(cmd);

                dataAdapter.Fill(table);

                ClosedXML.Excel.XLWorkbook wbook = new ClosedXML.Excel.XLWorkbook();
                var ws = wbook.Worksheets.Add(table, "Осиновявания");
                ws.Row(1).InsertRowsAbove(1);
                if (startDate.ToString() == null && endDate.ToString() == null)
                {
                    ws.Cell(1, 1).Value = "Период:";
                    ws.Cell(1, 2).Value = startDate.ToString();
                    ws.Cell(1, 3).Value = endDate.ToString();
                }
                else
                {
                    ws.Cell(1, 1).Value = "Всички осиновявания";
                }

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

        private void listViewSort(object sender, RoutedEventArgs e)
        {
            var tag = ((GridViewColumnHeader)sender).Tag;
            switch (tag.ToString())
            {
                case "1": sort(ref nameSort, adoptionListView, "animal_name"); break;
                case "2": sort(ref typeSort, adoptionListView, "animal_type_name"); break;
                case "3": sort(ref adopterSort, adoptionListView, "adopter_name"); break;
                case "4": sort(ref pinSort, adoptionListView, "adopter_pin"); break;
                case "5": sort(ref addressSort, adoptionListView, "adopter_address"); break;
                case "6": sort(ref phoneSort, adoptionListView, "adopter_phone"); break;
                case "7": sort(ref dateSort, adoptionListView, "adoption_date"); break;
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
