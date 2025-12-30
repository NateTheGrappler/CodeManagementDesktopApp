using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AngleSharp.Text;
using Microsoft.Win32;
//Add in the pdf nu-get package that is needed:
using PdfiumViewer;
using System.Speech.Synthesis;
using WindowsInput;
using WindowsInput.Native;
using static System.Net.Mime.MediaTypeNames;


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
        public InputSimulator inputSimulator = new InputSimulator();
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
        private BooksManager      bookMange             = new BooksManager();
        private bookJsonManager   jsonBookManager       = new bookJsonManager();
        private string            pdfWords              = "Empty"; 
        private int               volumeSave            = 50; 
        private WindowState       originalWindowState;
        private WindowStyle       originalWindowStyle;
        private SpeechSynthesizer speechSynthesizer;
        


        public bookManager()
        {
            InitializeComponent();
            InitializeListBox();     //Load all the data for viewing
            InitializeSpeechSynth(); //Set up the speech synth and add all of the functions
            loadAvailableVoices();   //Get all of the voices from windows
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

            //Also init the web view for loading the pdfs
            await pdfWebView.EnsureCoreWebView2Async(null);
            await textToSpeechWebView.EnsureCoreWebView2Async(null);

            //Then set the source for the list box as the books that were gotten
            BookListBox.ItemsSource = bookMange.books;
        }
        private async void InitializeSpeechSynth()                                           //Set up the speech sysnthesis
        {
            speechSynthesizer = new SpeechSynthesizer();

            //speechSynthesizer.SpeakStarted   += Synthesizer_SpeakStarted;
            //speechSynthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;
            //speechSynthesizer.StateChanged   += Synthesizer_StateChanged;
        }
        private void loadAvailableVoices()                                                   //Load all of the variable voices from windows
        {
            var voices = speechSynthesizer.GetInstalledVoices();
            VoiceComboBox.ItemsSource = voices;
            VoiceComboBox.DisplayMemberPath = "VoiceInfo.Name";

            if (voices.Count > 0)
            {
                VoiceComboBox.SelectedIndex = 0;
            }
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
    
        //------------------------------Book Menu GUI Functions-------------------------------------------

        private void openBookMenu(object sender, RoutedEventArgs e)                        //Do The Animations for the book menu
        {
            if(sender is Button button)
            {
                if(button.Name == "OpenBookMenu" || button.Name == "closeTextToSpeech")
                {
                    SetUpData(sender, e); //Load all the book metaData to book menu
                    Storyboard storyboard = new Storyboard();

                    //Set the transform origin on the Border itself
                    bookMenuBorder.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 10);
                    Panel.SetZIndex(bookMenuBorder, 11);

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

                    Storyboard.SetTarget(scaleYUP, bookMenuBorder);
                    Storyboard.SetTargetProperty(scaleYUP, new PropertyPath("RenderTransform.ScaleY"));

                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    storyboard.Children.Add(scaleYUP);
                    storyboard.Children.Add(opacityAdd);
                    storyboard.Begin();
                }
                else if (button.Name == "CloseBookMenu" || button.Name == "doTextToSpeech" )
                {
                    Storyboard storyboard = new Storyboard();

                    //Set the transform origin on the Border itself
                    bookMenuBorder.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

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

                    Storyboard.SetTarget(scaleYDOWN, bookMenuBorder);
                    Storyboard.SetTargetProperty(scaleYDOWN, new PropertyPath("RenderTransform.ScaleY"));

                    Storyboard.SetTarget(opacityTake, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityTake, new PropertyPath("Opacity"));

                    storyboard.Children.Add(scaleYDOWN);
                    storyboard.Children.Add(opacityTake);
                    storyboard.Begin();

                    Panel.SetZIndex(TranslucentBox, -20);

                    //After Closing the window, also save all of the changed content to the item
                    saveDataToBook();
                }
            }
        }
    
        private void SetUpData(object sender, RoutedEventArgs e)                           //Loads the meta data from book into book menu
        {
            //Check to see if youre a button in the list, and if so, get that list, and select the item that you pressed the button from
            if (sender is Button button)
            {
                //Find the parent ListBoxItem
                DependencyObject parent = VisualTreeHelper.GetParent(button);

                //Get the parent of the button object
                while (parent != null && !(parent is ListBoxItem))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is ListBoxItem listBoxItem)
                {
                    //Select the ListBoxItem
                    listBoxItem.IsSelected = true;
                    ListBox listBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem) as ListBox;

                    //Make the item the selected one in the list box
                    if (listBox != null)
                    {
                        var selectedItem = listBox.SelectedItem;
                    }
                }
            }

            if (BookListBox.SelectedItem != null)
            {
                var book = BookListBox.SelectedItem as Book;
                BookMenuCover.Source = book.cover;
                bookTitle.Text       = book.title;
                bookAuthor.Text      = book.author;
                bookSubject.Text     = book.subject;
                bookPageCount.Text   = book.pageCount;
                bookKeywords.Text    = book.keyWords;
            }            
        }
        
        private void saveDataToBook()                                                      //Save all of the data from the book menu gui to the book on close
        {
            
            if (BookListBox.SelectedItem != null)
            {
                var book = BookListBox.SelectedItem as Book;
                if(!string.IsNullOrEmpty(bookTitle.Text))     { book.title     = bookTitle.Text;    }
                if(!string.IsNullOrEmpty(bookAuthor.Text))    { book.author    = bookAuthor.Text;   }
                if(!string.IsNullOrEmpty(bookSubject.Text))   { book.subject   = bookSubject.Text;  }
                if(!string.IsNullOrEmpty(bookPageCount.Text)) { book.pageCount = bookPageCount.Text;}
                if(!string.IsNullOrEmpty(bookKeywords.Text))  { book.keyWords  = bookKeywords.Text; }
            }   
        }

        private void readBookButton_Click(object sender, RoutedEventArgs e)                //Redirect to the animation function
        {
            openAndCloseAnimation(sender, e);
        }

        //-------------------------------------PDF Control Functions---------------------------------------

        private void openAndCloseAnimation(object sender, RoutedEventArgs e)               //Animate the opening of the pdf
        {
            if (sender is Button button)
            {
                if (button.Name == "readBookButton" || button.Name == "closeTextToSpeech")
                {
                    openPDF();
                    Storyboard storyboard = new Storyboard();

                    Panel.SetZIndex(readBookBorder, 15);

                    DoubleAnimation moveYDOWN = new DoubleAnimation
                    {
                        From = -1000,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };

                    Storyboard.SetTarget(moveYDOWN, readBookBorder);
                    Storyboard.SetTargetProperty(moveYDOWN, new PropertyPath("RenderTransform.Y"));

                    storyboard.Children.Add(moveYDOWN);
                    storyboard.Begin();
                }
                else if (button.Name == "ClosePdfView" || button.Name == "doTextToSpeech")
                {

                    //Add in an extra check to see if the user is full screened or not and close them out if they are
                    if (Window.GetWindow(this).WindowState == WindowState.Maximized)
                    {
                        Window.GetWindow(this).WindowState = originalWindowState;
                        Window.GetWindow(this).WindowStyle = WindowStyle.SingleBorderWindow; //set default value lowkey
                    }


                    Storyboard storyboard = new Storyboard();

                    DoubleAnimation moveYUP = new DoubleAnimation
                    {
                        From = 0,
                        To = -1000,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };

                    Storyboard.SetTarget(moveYUP, readBookBorder);
                    Storyboard.SetTargetProperty(moveYUP, new PropertyPath("RenderTransform.Y"));

                    storyboard.Children.Add(moveYUP);
                    storyboard.Begin();

                    //Also reload the pdf view to fix rendering glitch
                    reloadPDF(sender, e);

                }
            }
        }
    
        private void openPDF()                                                             //Set the web2view to the open book pdf 
        {
            if(BookListBox.SelectedItem != null)
            {
                var book = BookListBox.SelectedItem as Book;
                pdfWebView.CoreWebView2.Navigate(book.filePath); //Then get the open view to display the book
            }
        }

        private async void nextPage(object sender, RoutedEventArgs e)                      //Manually press right arrow to change pages
        {
            //Focus element so key press works
            pdfWebView.Focus();

            //Press the right arrow key once to simulate skipping to the next page
            bookMange.inputSimulator.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
            bookMange.inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT); ;
        }
        private void previousPage(object sender, RoutedEventArgs e)                        //Manually press left arrow to change pages
        {
            //Focus element so key press works
            pdfWebView.Focus();

            //Press the right arrow key once to simulate skipping to the next page
            bookMange.inputSimulator.Keyboard.KeyDown(VirtualKeyCode.LEFT);
            bookMange.inputSimulator.Keyboard.KeyUp(VirtualKeyCode.LEFT); ;
        }
        private void reloadPDF(object sender, RoutedEventArgs e)                           //Manually reload the pdf as a refresh
        {
            pdfWebView.Reload();
        }
        private void zoomInPDF(object sender, RoutedEventArgs e)                           //Set the zoom to 10% more
        {
            //zoom in by 10%
            pdfWebView.ZoomFactor += 0.1;
        }
        private void zoomOutPDF(object sender, RoutedEventArgs e)                          //Set the zoom to 10% less
        {
            //zoom out by 10%
            pdfWebView.ZoomFactor -= 0.1;
        }
        private void resetZoom(object sender, RoutedEventArgs e)                           //Set the zoom to normal
        {
            //reset zoom factor to original
            pdfWebView.ZoomFactor = 1.0f;
        }
        private void scrollDown(object sender, RoutedEventArgs e)                          //Scroll down by a fraction
        {
            //Focus element so key press works
            pdfWebView.Focus();

            //Press the right arrow key once to simulate skipping to the next page
            bookMange.inputSimulator.Keyboard.KeyDown(VirtualKeyCode.DOWN);
            bookMange.inputSimulator.Keyboard.KeyUp(VirtualKeyCode.DOWN); ;
        }
        private void scrollUp(object sender, RoutedEventArgs e)                            //Scroll up by a fraction
        {
            //Focus element so key press works
            pdfWebView.Focus();

            //Press the right arrow key once to simulate skipping to the next page
            bookMange.inputSimulator.Keyboard.KeyDown(VirtualKeyCode.UP);
        }
        private void fullscreenPDF(object sendder, RoutedEventArgs e)                      //FullScreen The Pdf for viewing
        {

            if(Window.GetWindow(this).WindowState != WindowState.Maximized)
            {
                //Save the window state in the class, and then also change it to be maximized
                originalWindowState = Window.GetWindow(this).WindowState;
                originalWindowStyle = Window.GetWindow(this).WindowStyle;
                Window.GetWindow(this).WindowState = WindowState.Maximized;
                Window.GetWindow(this).WindowStyle = WindowStyle.None;

                //Change the image of the fullscreen button to indicate you're fullscreened
                FullScreenImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/fullScreenExit.png"));
            }
            else if (Window.GetWindow(this).WindowState == WindowState.Maximized)
            {
                //Set the window state back to the saved state in the class
                Window.GetWindow(this).WindowState = originalWindowState;
                Window.GetWindow(this).WindowStyle = originalWindowStyle;

                //Change the image of the fullscreen button to indicate you're not fullscreened
                FullScreenImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/fullScreenArrows.png"));
            }

        }
    
        
        //------------------------------------For The Text To Speech part------------------------------------------
        private void doTextToSpeechAnimation(object sender, RoutedEventArgs e)             //The animation that handles closing all other windows and opening tts window
        {
            if (sender is Button button)
            {
                if (button.Name == "doTextToSpeech" )
                {
                    //Get the text from the pdf and the title as well
                    initTextToSpeechView(sender, e);

                    //Also close all of the other things that have been opened
                    openAndCloseAnimation(sender, e);
                    openBookMenu(sender, e);

                    //Reset the progress bar
                    textToSpeechProgress.Value = 0;

                    Storyboard storyboard = new Storyboard();

                    Panel.SetZIndex(textToSpeechBorder, 20);
                    Panel.SetZIndex(TranslucentBox, 10);

                    DoubleAnimation moveDOWN = new DoubleAnimation
                    {
                        From = -1000,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };
                    DoubleAnimation opacityAdd = new DoubleAnimation
                    {
                        From = 0,
                        To = 0.7,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    Storyboard.SetTarget(moveDOWN, textToSpeechBorder);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.Y"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    storyboard.Children.Add(moveDOWN);
                    storyboard.Children.Add(opacityAdd);
                    storyboard.Begin();
                }
                else if (button.Name == "closeTextToSpeech")
                {
                    disposeSpeechView();
                    Storyboard storyboard = new Storyboard();
                    openAndCloseAnimation(sender, e); 
                    openBookMenu(sender, e);

                    Panel.SetZIndex(TranslucentBox, -5);


                    DoubleAnimation moveYUP = new DoubleAnimation
                    {
                        From = 0,
                        To = -1000,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };
                    DoubleAnimation opacityTake = new DoubleAnimation
                    {
                        From = 0,
                        To = 0.7,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    Storyboard.SetTarget(moveYUP, textToSpeechBorder);
                    Storyboard.SetTargetProperty(moveYUP, new PropertyPath("RenderTransform.Y"));
                    Storyboard.SetTarget(opacityTake, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityTake, new PropertyPath("Opacity"));

                    storyboard.Children.Add(moveYUP);
                    storyboard.Children.Add(opacityTake);
                    storyboard.Begin();

                }
            }
        }
        private void initTextToSpeechView(object sender, RoutedEventArgs e)                //Get the text from pdf and set up proper view
        {
            if (BookListBox.SelectedItem != null)
            {
                //Get the book as a variable
                var book = BookListBox.SelectedItem as Book;

                //Get all of the text from the book and set in class var
                pdfWords = getTextFromCurrentPDF(sender, e);

                //Set the text box to the book text
                TextToSpeechTitle.Text = book.title;

                //init the webview so book renders
                textToSpeechWebView.CoreWebView2.Navigate(book.filePath); //Then get the open view to display the book
            }
        }
        private string getTextFromCurrentPDF(object sender, RoutedEventArgs e)             //A helper function to get the text from each page
        {
            //Check to see if book is not empty, and then get the book
            if(BookListBox.SelectedItem != null)
            {
                Book book = BookListBox.SelectedItem as Book;

                //init an empty string to append to
                string allTextInPdf = "";

                using (var pdf = PdfDocument.Load(book.filePath))
                {
                    //Get the text from each page, and then add it to the empty string
                    int pageCount;
                    textToSpeechProgress.Minimum = 0;
                    if(int.TryParse(book.pageCount, out pageCount))
                    {
                        textToSpeechProgress.Maximum = pageCount;
                        for (int i = 0; i < pageCount; i++)
                        {
                            allTextInPdf +=  pdf.GetPdfText(i);
                            Debug.WriteLine($"Got text from Page: {i}");
                            textToSpeechProgress.Value = i;
                        }
                        //Return Full string
                        return allTextInPdf;
                    }
                }
            }
            //Back up incase of failure
            return "Unable To Get Text From PDF";
        }
        private void disposeSpeechView()                                                   //Clear up page view and reset sliders
        {
            //Clear the text in the box and the title as well for faster load times
            pdfWords = "Empty";
            TextToSpeechTitle.Text = "";

            //Reset the slider values as well
            SpeedSlider.Value  = 1;
            VolumeSlider.Value = 50;
        }
        private void muteAndUnmute(object sender, RoutedEventArgs e)                       //Either set the volume to zero or reset it
        {
            //if it is not zero, then save it and set it to zero
            if(VolumeSlider.Value != 0)
            {
                volumeSave = (int)VolumeSlider.Value;
                VolumeSlider.Value = 0;
                VolumeImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/volumeOff.png"));
            }
            //If the volume is zero, change it to saved value
            else if (VolumeSlider.Value == 0)
            {
                VolumeSlider.Value = volumeSave;
                VolumeImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/volumeUp.png"));
            }
        }
        private void resetSpeed(object sender, RoutedEventArgs e)                          //Reset the speed that exists in the rate slider
        {
            //Just straight up set it to 1
            SpeedSlider.Value = 1;
        }

    }           
}               
                