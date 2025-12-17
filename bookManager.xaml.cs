using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
//Add in the pdf nu-get package that is needed:
using PdfiumViewer;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Policy;
using System.Net.Http;
namespace CodeManagementSystem
{

    public class BookManager //The class that holds the book source list and has functions managing that list if needed
    {
        //---------------------------------The Variables------------------------------------------
        ObservableCollection<Book> books;
        private readonly string _driveDownloadPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\BookManagement\\PDF\\");




        //---------------------------------Saving Functions---------------------------------------
        //Save a pdf book by just moving it from whatever folder it is in to the roaming apps folder
        public void saveFromDrive()
        {

            //Create a new file Dialog for user, that filters through only pdf files
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".pdf";
            openFileDialog.Filter = "PDF Files|*.pdf";

            //Then open the actual dialog and pause application to let user choose pdf
            bool? result = openFileDialog.ShowDialog();

            if(result == true)
            {
                //After a pdf is successfully chosen, get it's file name
                string fileName = openFileDialog.FileName;
                Debug.WriteLine(fileName);

                //if the directory doesnt exist, create it
                if(!System.IO.Directory.Exists(_driveDownloadPath))
                {
                    System.IO.Directory.CreateDirectory(_driveDownloadPath);
                }

                //Move the File over from whatever location it was at, to then the roaming apps folder
                System.IO.File.Move
                (
                    fileName,
                    System.IO.Path.Combine(_driveDownloadPath, openFileDialog.SafeFileName)
                );
            }
        }
        //Save a book from an online url
        public async Task saveFromURL(string url, string pdfname)
        {
            try
            {
                if (!System.IO.Directory.Exists(_driveDownloadPath))
                {
                    System.IO.Directory.CreateDirectory(_driveDownloadPath);
                }

                using (HttpClient client = new HttpClient())
                {
                    //get the pdf information via the byte array, then set the path, and save the file
                    byte[] pdfBytes = await client.GetByteArrayAsync(url);
                    string path = System.IO.Path.Combine(_driveDownloadPath, pdfname);
                    await System.IO.File.WriteAllBytesAsync(path, pdfBytes);
                }
            }
            catch(Exception ex)
            {
                //A message box for the user incase of error
                MessageBox.Show(
                    "Unable to Download PDf. Please Check if URL is valid and try again.",
                    "Invalid URL.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
        }


        //--------------------------------Constructor--------------------------------------------
        public BookManager() { }
    }


    //The Class the holds the data that can be acutally found in each of the books
    public class Book
    {
        //----------------------Variables------------------
        public string title { get; set; } = "Unkown";
        public string author { get; set; } = "Unkown";
        public string pageCount { get; set; } = "Unkown";


        //----------------------Constructors---------------
        Book(string title, string author, string pageCount)
        {
            this.title = title;
            this.author = author;
            this.pageCount = pageCount;
        }
        Book() {}

    }

    public partial class bookManager : Page
    {
        private BookManager bookMange = new BookManager();

        public bookManager()
        {
            InitializeComponent();
        }


        //--------------------------------------Front UI------------------------------------------
        
        //This function is for main backbutton thats in the title to go back to the neural network
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        //TODO: Delete This Function
        private async void UrlButton_Click(object sender, RoutedEventArgs e)
        {
            await bookMange.saveFromURL(URLInput.Text, "randomPdfName.pdf");
        }
    }
}
