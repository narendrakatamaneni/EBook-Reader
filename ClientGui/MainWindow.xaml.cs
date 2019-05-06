/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client GUI                                 //
// ver 1.2                                                         //
// Narendra Katamaneni CSE687 - Object Oriented Design             //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines one class MainWindow that provides tabs:
 * - Find Libs: Navigate through local directory to find files for testing
 *   - Shows local directories and files
 *   - Can navigate by double clicking directories
 * 
 * Required Files:
 * ---------------
 * MainWindow.xaml, MainWindow.xaml.cs
 * SelectionWindow.xaml, SelectionWindow.xaml.cs
 * Translater.h
 * 
 * Maintenance History:
 * -------------------
 * ver 1.0 : 28 Mar 2019
 * - first release
 * 
 */
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
using System.IO;
using Path = System.IO.Path;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ClientGui
{
    ///////////////////////////////////////////////////////////////////
    // MainWindow class
    // - Provides navigation and selection to find libraries to test.
    // - Creates a popup window to handle selections.

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string WDirectory { get; set; }
        //----< function dispatched by child thread to main thread >-------

        private void clearDirs()
        {
            lstFiles.Items.Clear();
        }
        //----< function dispatched by child thread to main thread >-------

        private void addDir(string dir)
        {
            lstFiles.Items.Add(dir);
        }
        //----< function dispatched by child thread to main thread >-------

        private void insertParent()
        {
            lstFiles.Items.Insert(0, "..");
        }

        private void addFile(string file)
        {
            lstFiles.Items.Add(file);
        }
        //----< add client processing for message with key >---------------


       
        //----< load getFiles processing into dispatcher dictionary >------
        public MainWindow()
        {
            InitializeComponent();
        }

        //<--Function called for loading main GUI window-->
        private  async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();

                        HttpResponseMessage resp = await client.GetAsync("http://localhost:52464/api/files");
                        var files = new List<string>();
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            JArray jArr = (JArray)JsonConvert.DeserializeObject(json);
                            foreach (var item in jArr)
                            lstFiles.Items.Add(item.ToString());
                            
                        }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception occured while loading main window");
            }

        }
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //WDirectory = dialog.SelectedPath;
                // Launch OpenFileDialog by calling ShowDialog method
                //Nullable<bool> result = dialog.ShowDialog();
                // Get the selected file name and display in a TextBox.
                // Load content of file in a TextBlock
                //if (result == true)
                //{
                /*txtPath.Text = dialog.FileName;
                FileStream fs = File.OpenRead(txtPath.Text);
                try
                {
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                    fs.Close();
                    MultipartFormDataContent multiContent = new MultipartFormDataContent();
                    string fileName = txtPath.Text;
                    multiContent.Add(bytes, "files", fileName);
                    HttpResponseMessage message = await client.PostAsync("http://localhost:52464/api/files/", multiContent);
                }
                catch (Exception)
                {

                }*/

                    
            }
            
        }
        private async void LstFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();
                string selectedFile = (string)lstFiles.SelectedItem;
                await client.GetAsync("http://localhost:52464/api/files" + "/" + selectedFile);
            }
            catch (Exception)
            {
                Console.WriteLine("Exception occured : you have clicked on the blank space");
            }
        }
    }
}
