using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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

namespace CodeManagementSystem
{
    public class projectJsonManagement
    {
        private readonly string _JsonPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CodeInformationManagingSystem\\ProjectManagement\\JSON\\");
        private readonly string _jsonStoragePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CodeInformationManagingSystem\\ProjectManagement\\JSON\\");


        //save to json informaton
        public async Task saveToJson(ObservableCollection<projectDirectory> dataPaths)
        {
            //Create the directory if it doesn't exist
            if (!System.IO.Directory.Exists(_JsonPath))
            {
                System.IO.Directory.CreateDirectory(_JsonPath);
            }

            //Set up the data to be written to the json, and also define the json format
            var serializedList = dataPaths.ToList();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            //Actually write the file to the json
            string json = JsonSerializer.Serialize(serializedList, options);
            await File.WriteAllTextAsync(System.IO.Path.Combine(_JsonPath, "projects.json"), json);
        }

        //-------------------------------The loading functions-------------------------------------
        public async Task<ObservableCollection<T>> loadJsonData<T>()
        {
            // Check to see if path exists, if not return empty
            if (!File.Exists(System.IO.Path.Combine(_jsonStoragePath, "projects.json")))
            {
                return new ObservableCollection<T>();
            }


            //Await until all of the json data is read
            string json = await File.ReadAllTextAsync(System.IO.Path.Combine(_jsonStoragePath, "projects.json"));

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
        public projectJsonManagement()
        {

        }
    }


    public class projectDirectory
    {
        //--------------------------Varaibles---------------------------
        public string folderName  { get; set; }
        public string folderPath  { get; set; }
        public DateTime addedDate { get; set; }
        public ImageSource icon   { get; set; }
        public ObservableCollection<projectFile>      innerFiles       { get; set; } = new ObservableCollection<projectFile>();
        public ObservableCollection<projectDirectory> innerDirectories { get; set; } = new ObservableCollection<projectDirectory>();
        public bool IsExpanded { get; set; }
        public bool HasSubDirectories => innerDirectories.Count > 0;

        //-------------------------Constructors---------------------------
        public projectDirectory() { }
        public projectDirectory(string folderName, string folderPath)
        {
            this.folderName = folderName;
            this.folderPath = folderPath;
            this.addedDate = DateTime.Now;
        }

        //-------------------events for logic handling---------
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //----------------------Inner Working Functions----------------------
        public void loadAllFilesInDirectory(string path)  //load all of the files inside of a directory
        {
            //loading all of the files in the from a given directory path
            try
            {
                //all of the file names
                string[] filePaths = Directory.GetFiles(path);
                foreach (string filePath in filePaths)
                {
                    //set up a way to get all of the metadata on the file
                    FileInfo fileInfo = new FileInfo(filePath);

                    //create the new file object using the data that was gotten
                    projectFile newFile = new projectFile(fileInfo.Name, filePath);
                    newFile.addedDate = fileInfo.LastWriteTime;
                    newFile.fileSize = fileInfo.Length.ToString();
                    newFile.fileType = fileInfo.Extension;
                    innerFiles.Add(newFile);
                }
            }
            catch (Exception ex)
            {
                //error box whenever you are unable to load a file
                MessageBox.Show(
                    "Unable to load directory, please check if path is correct",
                    "Unable to load project",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
        }
    
    }


    public class projectFile
    {
        //--------------------------Varaibles---------------------------
        public string fileName    { get; set; }
        public string filePath    { get; set; }
        public string fileType    { get; set; }
        public string fileSize    { get; set; }
        public DateTime addedDate { get; set; }
        public ImageSource icon { get; set; }

        //-------------------------Constructors---------------------------
        public projectFile() { }
        public projectFile(string fileName, string filePath) 
        {
            this.filePath = filePath;
            this.fileName = fileName;
        }
    }


    public partial class projectManager : Page
    {
        private projectJsonManagement                   jsonManager     = new projectJsonManagement();
        private ObservableCollection<projectDirectory> savedDirectories = new ObservableCollection<projectDirectory>();
        public projectManager()
        {
            InitializeComponent();
            InitializeProjectList();
        }

        private async Task InitializeProjectList()
        {
            //load any given data from a json file
            savedDirectories = await jsonManager.loadJsonData<projectDirectory>();

            //set the saved Directories as the item source for the tree view
            mainTreeView.ItemsSource = savedDirectories;
        }
        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //save data and go back to main page
            await jsonManager.saveToJson(savedDirectories);
            this.NavigationService.GoBack();
        }

        //------------------------Add New Project GUI------------------------------
        private void NewProjectAnimation(object sender, RoutedEventArgs e)               //Opens and closes the gui window for adding a new project
        {
            //get button for button name
            if(sender is Button button)
            {
                Storyboard sb = new Storyboard();


                //The open sequence
                if(button.Name == "AddButton")
                {
                    //Set the transform origin on the Border itself
                    NewProjecyGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 10);
                    Panel.SetZIndex(NewProjecyGUI, 11);

                    DoubleAnimation moveDOWN = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };
                    DoubleAnimation opacityAdd = new DoubleAnimation
                    {
                        From = 0,
                        To = 0.7,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    Storyboard.SetTarget(moveDOWN, NewProjecyGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    sb.Children.Add(moveDOWN);
                    sb.Children.Add(opacityAdd);
                    sb.Begin();
                }
                //the close sequence
                else if(button.Name == "closeAddGUI" || button.Name =="AddNewProjectButton")
                {
                    //Set the transform origin on the Border itself
                    NewProjecyGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    DoubleAnimation moveDOWN = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };
                    DoubleAnimation opacityAdd = new DoubleAnimation
                    {
                        From = 0.7,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    Storyboard.SetTarget(moveDOWN, NewProjecyGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    sb.Children.Add(moveDOWN);
                    sb.Children.Add(opacityAdd);
                    sb.Begin();

                    Panel.SetZIndex(TranslucentBox, -5);
                    
                }
            }
        }
        private void clearNewProjectGUI()                                                //Clears all of the inputed data
        {
            //clear the textboxes that are in view
            projectNameTB.Text = string.Empty;
            projectPathTB.Text = string.Empty;
        }
    
        private async void addNewProject(object sender, RoutedEventArgs e)
        {
            //simplify names for easier use
            string path = projectPathTB.Text.Trim();
            string name = projectNameTB.Text.Trim();

            //check for empty input texts
            if (string.IsNullOrEmpty(projectPathTB.Text) || string.IsNullOrEmpty(projectNameTB.Text))
            {
                //show an error message if the fields are empty
                MessageBox.Show(
                    "Please fill out all fields before you continue",
                    "Incomplete Fields",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            if(!Directory.Exists(path))
            {
                //show an error message if the fields are empty
                MessageBox.Show(
                    "Unable To Find Project At That Directory. Please Try Again.",
                    "Project Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            // Create new project directory
            var newDirectory = new projectDirectory
            {
                folderName = name,
                folderPath = path,
                addedDate = DateTime.Now,
            };
            //Load all of the files that are visible in that directory and add it to the main list
            newDirectory.loadAllFilesInDirectory(newDirectory.folderPath);
            savedDirectories.Add(newDirectory);

            //save all of the data to json
            await jsonManager.saveToJson(savedDirectories);

            //do the closing animation when finished
            NewProjectAnimation(sender, e);

        }
    }
}
