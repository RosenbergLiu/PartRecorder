using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static SQLite.SQLite3;
using CsvHelper;
using System.IO;
using System.Text.Unicode;
using System.Globalization;
using System.Security.Claims;
using System.Media;

namespace PartRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private readonly RecordsContext recordsContext = new RecordsContext();
        private readonly PartsContext partsContext = new PartsContext();
        private CollectionViewSource recordViewSource;
        public MainWindow()
        {
            InitializeComponent();
            recordViewSource = (CollectionViewSource)FindResource(nameof(recordViewSource));
            SetInput(false);
            PartList.Items.Refresh();
            SetInput(true);

        }

        public void SetInput(bool status)
        {
            BarCode.IsEnabled = status;
            btnAdd.IsEnabled = status;
            if (status)
            {
                BarCode.Focus();
            }

        }

        public async Task<string?> GetNumber(string Barcode)
        {
            var n = await partsContext.Parts.FindAsync(Barcode);
            if (n == null)
            {
                return null;
            }
            else
            {
                return n.PartNumber;
            }
        }

        public async void InsertNumber(string Barcode, string Number)
        {
            Part part = new Part()
            {
                Id = Barcode,
                PartNumber = Number,
            };
            await partsContext.AddAsync(part);
            await partsContext.SaveChangesAsync();
        }


        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string BarCodeInput = BarCode.Text;
            if (BarCodeInput != "")
            {
                BarCode.Clear();
                var r = await recordsContext.Records.FindAsync(BarCodeInput);

                if (BarCodeInput.Length < 7)
                {
                    if (r == null)
                    {
                        Record record = new Record()
                        {
                            Id = BarCodeInput,
                            PartNumber = BarCodeInput,
                            Quantity = 1
                        };
                        await recordsContext.AddAsync(record);
                        await recordsContext.SaveChangesAsync();
                    }
                    else
                    {
                        r.Quantity++;

                        recordsContext.Update(r);
                        await recordsContext.SaveChangesAsync();
                    }
                }
                else
                {
                    if (r != null)
                    {
                        r.Quantity++;
                        recordsContext.Update(r);
                        await recordsContext.SaveChangesAsync();
                    }
                    else
                    {
                        string? Number = await GetNumber(BarCodeInput);
                        if (Number == null)
                        {
                            InputDialog inputDialog = new InputDialog();
                            if (inputDialog.ShowDialog() == true)
                            {
                                Number = inputDialog.Answer;
                                InsertNumber(BarCodeInput, Number);
                                Record record = new Record()
                                {
                                    Id = BarCodeInput,
                                    PartNumber = Number,
                                    Quantity = 1
                                };
                                await recordsContext.AddAsync(record);
                                await recordsContext.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            Record record = new Record()
                            {
                                Id = BarCodeInput,
                                PartNumber = Number,
                                Quantity = 1
                            };
                            await recordsContext.AddAsync(record);
                            await recordsContext.SaveChangesAsync();
                        }
                    }
                }
                PartList.Items.Refresh();
                BarCode.Focus();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            recordsContext.Database.EnsureCreated();
            //partsContext.Database.EnsureCreated();
            recordsContext.Records.Load();
            recordViewSource.Source = recordsContext.Records.Local.ToObservableCollection();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Record record = (Record)PartList.SelectedItem;
            recordsContext.Remove(record);
            await recordsContext.SaveChangesAsync();
            PartList.Items.Refresh();
            btnDelete.IsEnabled = false;
        }

        private void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Record? record = (Record)PartList.SelectedItem;
            if(record != null)
            {
                btnDelete.IsEnabled = true;
            }
            
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure want to clear the list?",
                    "Clear",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                recordsContext.Records.RemoveRange(recordsContext.Records);
                recordsContext.SaveChanges();
                PartList.Items.Refresh();
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{date}.csv");
            var textWriter = File.CreateText(path);
            CultureInfo myCIintl = new CultureInfo("en-us", false);
            var csv = new CsvWriter(textWriter, myCIintl);
            foreach (var r in recordsContext.Records)
            {
                csv.WriteField(r.Id);
                csv.WriteField(r.Quantity);
                csv.NextRecord();
            }
            textWriter.Close();
            MessageBox.Show($"List exported to \n{path}","Export success",MessageBoxButton.OK,MessageBoxImage.Information);
        }
    }
}
