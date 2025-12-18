using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
//Add in the pdf nu-get package that is needed:
using PdfiumViewer;
namespace CodeManagementSystem
{
    public class bookJsonManager
    { 
        //---------------------------------The Variables-------------------------------------------
        private readonly string _JsonPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\BookManagement\\JSON\\");

        //save to json informaton
        public async Task saveToJson(ObservableCollection<Book> books)
        {
            //Create the directory if it doesn't exist
            if(!System.IO.Directory.Exists(_JsonPath))
            {
                System.IO.Directory.CreateDirectory(_JsonPath);
            }

            //Set up the data to be written to the json, and also define the json format
            var serializedList = books.ToList();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            //Actually write the file to the json
            string json = JsonSerializer.Serialize(serializedList, options);
            await File.WriteAllTextAsync(System.IO.Path.Combine(_JsonPath, "books.json"), json);
        }

        public async Task loadFromJson()
        {

        }

        public bookJsonManager() { }
    }


    public class BookManager //The class that holds the book source list and has functions managing that list if needed
    {
        //---------------------------------The Variables------------------------------------------
        public ObservableCollection<Book> books = new ObservableCollection<Book>();
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

        //-------------------------------The loading functions-------------------------------------

        //--------------------------------Constructor--------------------------------------------
        public BookManager() { }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }


    //The Class the holds the data that can be acutally found in each of the books
    public class Book
    {
        //----------------------Variables------------------
        private readonly string _CoverFolderPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\BookManagement\\Covers\\");
        public string      title           { get; set; } = "Unkown";
        public string      author          { get; set; } = "Unkown";
        public string      keyWords        { get; set; } = "Unkown";
        public string      subject        { get; set; }  = "Unkown";
        public string      pageCount       { get; set; } = "Unkown";
        public string      filePath        { get; set; } = "Unkown";
        public string      imagePath       { get; set; } = "Unkown";
        
        [JsonIgnore] //cant store bitmapSource
        public BitmapSource cover          { get; set; } = null;


        //----------------------Constructors---------------
        public Book(string bookFilePath)
        {
            this.filePath = bookFilePath;
            loadData();
            saveImage();
        }
        public Book() {}

        //---------------------Functions--------------------
        //Only use this function the first time a book is saved, only so it can then be saved to a json
        public void loadData()
        {
            using (var pdf = PdfDocument.Load(filePath))
            {
                //Get the meta data of the pdf
                var info  = pdf.GetInformation();
                if (!string.IsNullOrEmpty(info.Title)) { title = info.Title; }
                pageCount = pdf.PageCount.ToString();
                author    = info.Author;
                keyWords  = info.Keywords;
                subject   = info.Subject;
            }
        }

        //This function is seperate due to the fact that loading images is expensive and cant be saved in json
        public void saveImage()
        {
            using (var pdf = PdfDocument.Load(filePath))
            {
                //After getting the cover, save it to a directory
                if(!Directory.Exists(_CoverFolderPath))
                {
                    Directory.CreateDirectory(_CoverFolderPath);
                }

                //Create a unique file name for the book
                string uniqueFileName = $"{Guid.NewGuid()}.png";
                string fullImagePath = System.IO.Path.Combine(_CoverFolderPath, uniqueFileName);

                //Save the image as a png at the unique path that was created
                var image = (Bitmap)pdf.Render(0, 96, 96, PdfRenderFlags.None);
                image.Save(fullImagePath, ImageFormat.Png);
                imagePath = fullImagePath;
            }
        }

        public void loadImageFromDrive(string imagePath)
        {
            Uri uri = new Uri(imagePath);
            cover = new BitmapImage(uri);
        }
    }

    public partial class bookManager : Page
    {
        private BookManager      bookMange       = new BookManager();
        private bookJsonManager  jsonBookManager = new bookJsonManager();

        public bookManager()
        {
            InitializeComponent();
            InitializeListBox(); //Load all the data for viewing
        }

        private async void InitializeListBox()
        {
            //Book testBook = new Book("C:\\Users\\guzin\\AppData\\Roaming\\CodeInformationManagingSystem\\BookManagement\\PDF\\PDFBook-2Designing-data-intensive-applications.pdf");
            //bookMange.books.Add(testBook);
            //testBook.loadImageFromDrive(testBook.imagePath);

            //Book testBook2 = new Book("C:\\Users\\guzin\\AppData\\Roaming\\CodeInformationManagingSystem\\BookManagement\\PDF\\randomPdfName.pdf");
            //bookMange.books.Add(testBook2);
            //testBook2.loadImageFromDrive(testBook2.imagePath);

            //Book testBook3 = new Book("C:\\Users\\guzin\\AppData\\Roaming\\CodeInformationManagingSystem\\BookManagement\\PDF\\a_complete_guide_to_standard_cpp_algorithms_v1_0_1.pdf");
            //bookMange.books.Add(testBook3);
            //testBook3.loadImageFromDrive(testBook3.imagePath);

            //foreach(Book book in bookMange.books)
            //{
            //    Debug.WriteLine("The imagePath: " + book.imagePath);
            //    Debug.WriteLine("The cover: "     + book.cover);
            //}

            //await jsonBookManager.saveToJson(bookMange.books);

            //BookListBox.ItemsSource = bookMange.books;
        }

        //--------------------------------------Front UI------------------------------------------

        //This function is for main backbutton thats in the title to go back to the neural network
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private async void playAddContentGUI(object sender, RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                //Add in a scale information for the Gui That Opens when a new Book is made
                if(button.Name == "NewButton")
                {
                    Storyboard storyboard = new Storyboard();

                    //Set the transform origin on the Border itself
                    AddBookBorder.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 10);
                    Panel.SetZIndex(AddBookBorder, 11);

                    DoubleAnimation scaleYUP = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    DoubleAnimation opacityAdd = new DoubleAnimation
                    {
                        From = 0,
                        To = 0.7,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    Storyboard.SetTarget(scaleYUP, AddBookBorder);
                    Storyboard.SetTargetProperty(scaleYUP, new PropertyPath("RenderTransform.ScaleY"));

                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    storyboard.Children.Add(scaleYUP);
                    storyboard.Children.Add(opacityAdd);
                    storyboard.Begin();

                }
                //if coming from the close button inside GUI, undo animation
                else if(button.Name == "CloseAddGUI")
                {
                    Storyboard storyboard = new Storyboard();

                    Panel.SetZIndex(TranslucentBox, -20);

                    //Set the transform origin on the Border itself
                    AddBookBorder.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    DoubleAnimation scaleYDOWN = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.2),
                    };
                    DoubleAnimation opacityTake = new DoubleAnimation
                    {
                        From = 0.7,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    Storyboard.SetTarget(scaleYDOWN, AddBookBorder);
                    Storyboard.SetTargetProperty(scaleYDOWN, new PropertyPath("RenderTransform.ScaleY"));

                    Storyboard.SetTarget(opacityTake, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityTake, new PropertyPath("Opacity"));

                    storyboard.Children.Add(scaleYDOWN);
                    storyboard.Children.Add(opacityTake);
                    storyboard.Begin();
                }
            }
        }


        //--------------------------------------Pop Up GUI Functions

    }
}
