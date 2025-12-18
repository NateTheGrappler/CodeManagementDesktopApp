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
using System.Windows.Threading;
using Microsoft.Win32;
//Add in the pdf nu-get package that is needed:
using PdfiumViewer;
namespace CodeManagementSystem
{
    //Purely for loading and saving the pdf metaData 
    public class bookJsonManager
    { 
        //---------------------------------The Variables-------------------------------------------
        private readonly string _JsonPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\BookManagement\\JSON\\");
        private readonly string _jsonStoragePath = System.IO.Path.Combine(
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

        //-------------------------------The loading functions-------------------------------------
        public async Task<ObservableCollection<T>> loadJsonData<T>()
        {
            // Check to see if path exists, if not return empty
            if (!File.Exists(System.IO.Path.Combine(_jsonStoragePath, "books.json")))
            {
                return new ObservableCollection<T>();
            }


            //Await until all of the json data is read
            string json = await File.ReadAllTextAsync(System.IO.Path.Combine(_jsonStoragePath, "books.json"));

            //Create the JsonSerializerOptions
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            //Use serializer in order to deserialize the data and return the type of data inputed
            var itemList = JsonSerializer.Deserialize<List<T>>(json, options);
            return itemList != null
                ? new ObservableCollection<T>(itemList)
                : new ObservableCollection<T>();
        }


        //------------------------------------Constructor------------------------------------------
        public bookJsonManager() { }
    }



    //The class that holds the book source list and has functions managing that list if needed
    public class BooksManager 
    {
        //---------------------------------The Variables------------------------------------------
        public ObservableCollection<Book> books = new ObservableCollection<Book>();
        private readonly string _driveDownloadPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\BookManagement\\PDF\\");

        //---------------------------------Saving Functions---------------------------------------
        //Save a pdf book by just moving it from whatever folder it is in to the roaming apps folder
        public string saveFromDrive()
        {

            //Create a new file Dialog for user, that filters through only pdf files
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".pdf";
            openFileDialog.Filter = "PDF Files|*.pdf";

            //Then open the actual dialog and pause application to let user choose pdf
            bool? result = openFileDialog.ShowDialog();
            try
            {
                if (result == true)
                {
                    //After a pdf is successfully chosen, get it's file name
                    string fileName = openFileDialog.FileName;
                    Debug.WriteLine(fileName);

                    //if the directory doesnt exist, create it
                    if (!System.IO.Directory.Exists(_driveDownloadPath))
                    {
                        System.IO.Directory.CreateDirectory(_driveDownloadPath);
                    }

                    //Move the File over from whatever location it was at, to then the roaming apps folder
                    System.IO.File.Move
                    (
                        fileName,
                        System.IO.Path.Combine(_driveDownloadPath, openFileDialog.SafeFileName)
                    );
                    return System.IO.Path.Combine(_driveDownloadPath, openFileDialog.SafeFileName);
                }
                return "";
            }
            catch(Exception ex)
            {
                //Return empty if error happens
                return "";
            }
        }
        //Save a book from an online url
        public async Task<string> saveFromURL(string url)
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
                    string uniqueFileName = $"{Guid.NewGuid()}.pdf";
                    string path = System.IO.Path.Combine(_driveDownloadPath, uniqueFileName);
                    await System.IO.File.WriteAllBytesAsync(path, pdfBytes);
                    return path;
                }
            }
            catch(Exception ex)
            {
                //A message box for the user incase of error
                MessageBox.Show(
                    "Unable to Download PDF. Please Check if URL is valid and try again.",
                    "Invalid URL.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                return "";
            }
        }

        //--------------------------------Constructor--------------------------------------------
        public BooksManager() { }
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
            try
            {
                //Set up a file stream to load all of the data to memory
                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    //set up the bitmap image and then cache the image from the imagepath
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    cover = bitmap;
                }
            }
            //Incase loading the image fails for some reason
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");
                cover = null;
            }
        }
    }





    public partial class bookManager : Page
    {
        private BooksManager     bookMange       = new BooksManager();
        private bookJsonManager  jsonBookManager = new bookJsonManager();

        public bookManager()
        {
            InitializeComponent();
            InitializeListBox(); //Load all the data for viewing
        }

        private async void InitializeListBox()                                               //loads all of the information into the listbox
        {
            //Load all of the meta data for the books from the json file
            bookMange.books = await jsonBookManager.loadJsonData<Book>();

            //Also Load all of the stored covers of the images as well
            foreach(Book book in bookMange.books )
            {
                book.loadImageFromDrive(book.imagePath);
            }

            //Then set the source for the list box as the books that were gotten
            BookListBox.ItemsSource = bookMange.books;
        }

        //--------------------------------------Front UI------------------------------------------

        private async void BackButton_Click(object sender, RoutedEventArgs e)                //Goes back to the front page
        {
            this.NavigationService.GoBack();
            //redundent save call bc why not
            await jsonBookManager.saveToJson(bookMange.books);
        }

        private async void playAddContentGUI(object sender, RoutedEventArgs e)               //Does the animation for add gui
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
                    ClearNewContentButton_Click(sender, e);

                }
                //if coming from the close button inside GUI, undo animation
                else if(button.Name == "CloseAddGUI" || button.Name == "AddNewContentButton")
                {
                    Storyboard storyboard = new Storyboard();

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

                    Panel.SetZIndex(TranslucentBox, -20);
                    ClearNewContentButton_Click(sender, e);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)              //Delete the selected book in listbox
        {
            if (BookListBox.SelectedItem == null)
            {
                //Tell user that they need to select a book to delete
                var result = MessageBox.Show(
                    $"Please select a book to delete.",
                    "No selection found.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
            else
            {
                //Grab the selected item as a book then ask if they really want to delete it
                var selectedItem = BookListBox.SelectedItem as Book;
                var result = MessageBox.Show(
                    $"Are you sure you want to delete book: {selectedItem.title}.",
                    "WARNING!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning

                    );

                //If they say yes proceed with deletion
                if (result == MessageBoxResult.Yes)
                {
                    //Remove the item from the list
                    bookMange.books.Remove(selectedItem);

                    //Delete the pdf and image at their given paths
                    File.Delete(selectedItem.imagePath);
                    File.Delete(selectedItem.filePath);

                    //Also update the json data to remove the deleted book
                    await jsonBookManager.saveToJson(bookMange.books);
                }
            }
        }
        
        //--------------------------------Pop Up GUI Functions--------------------------------------------
        private void ClearNewContentButton_Click(object sender, RoutedEventArgs e)          //Clears all the text in the add gui
        {
            //Clear all of the input fields, and change back the radio buttons
            URLTextBox.Text            = string.Empty;
            SubjectTextBox.Text        = string.Empty;
            TitleTextBox.Text          = string.Empty;
            DriveRadioButton.IsChecked = false;
            URLRadioButton.IsChecked   = true;
        }

        private async void AddNewContentButton_Click(object sender, RoutedEventArgs e)      //Calls the main backend save functions for covers, pdf, etc
        {
            //disable buttons
            AddNewContentButton.IsEnabled = false;
            ClearNewContentButton.IsEnabled = false;

            //Check which of the radio button options the user chose
            if (DriveRadioButton.IsChecked == true)
            {
                //After the person opens the right file, get the new path of the file moved by .saveFromDrive()
                string filePath = bookMange.saveFromDrive();

                //check to see if valid path was given
                if(filePath == "") { ClearNewContentButton_Click(sender, e); AddNewContentButton.IsEnabled = true; ClearNewContentButton.IsEnabled = true; return; }

                //Init a new book using the path gotten, and also load all of it's data
                Book newBook = new Book(filePath);
                newBook.loadImageFromDrive(newBook.imagePath);

                //Update any of the information if the user put it in
                if(!string.IsNullOrEmpty(TitleTextBox.Text))   { newBook.title   = TitleTextBox.Text; }
                if(!string.IsNullOrEmpty(SubjectTextBox.Text)) { newBook.subject = SubjectTextBox.Text; }

                //Add the book to the collection, then also save it's data in json
                bookMange.books.Add(newBook);
                await jsonBookManager.saveToJson(bookMange.books);

                //Close the Gui on save
                playAddContentGUI(sender, e);
            }
            else if(URLRadioButton.IsChecked == true)
            {
                if(!string.IsNullOrEmpty(URLTextBox.Text))
                {
                    try
                    {
                        //download the pdf from file, and also move it correctly and get the data
                        string filePath = await bookMange.saveFromURL(URLTextBox.Text);

                        //Create a new book with the filepath that was gotten from the saveFromURL Function
                        Book newBook = new Book(filePath);
                        newBook.loadImageFromDrive(newBook.imagePath);

                        //Update any of the information if the user put it in
                        if (!string.IsNullOrEmpty(TitleTextBox.Text)) { newBook.title = TitleTextBox.Text; }
                        if (!string.IsNullOrEmpty(SubjectTextBox.Text)) { newBook.subject = SubjectTextBox.Text; }

                        //Add the book to the collection, then also save it's data in json
                        bookMange.books.Add(newBook);
                        await jsonBookManager.saveToJson(bookMange.books);

                        //Close the Gui on save
                        playAddContentGUI(sender, e);
                    }
                    catch
                    {
                        //A message box for the user incase of error
                        MessageBox.Show(
                            "If Issue Persists, see if manual download for your PDF is avaliable.",
                            "Unable to Load PDF",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                            );
                        //Clear all of the incorrect data
                        ClearNewContentButton_Click(sender, e);
                    }
                }
            }

            //enable buttons
            AddNewContentButton.IsEnabled = true;
            ClearNewContentButton.IsEnabled = true;
        }

        private void changedRadioButtons(object sender, RoutedEventArgs e)                  //Purely decorational for user
        {
            if(sender is RadioButton button)
            {
                //If the Drive Radio button is pressed, disable URL Input
                if (button.Name == "DriveRadioButton")
                {
                    URLTextBox.Text = string.Empty;
                    URLTextBox.IsEnabled = false;
                    URLLabel.TextDecorations = TextDecorations.Strikethrough;
                    URLLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(155, 155, 155));
                }
                //Otherwise, just enable it back if need be
                else if (button.Name == "URLRadioButton")
                {
                    URLTextBox.Text = string.Empty;
                    URLTextBox.IsEnabled = true;
                    URLLabel.TextDecorations = null;
                    URLLabel.Foreground = (SolidColorBrush)this.FindResource("MainBorderBrushKey");
                }
            }
        }
    }
}
