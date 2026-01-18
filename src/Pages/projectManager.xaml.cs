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
using System.Xml.Linq;

namespace CodeManagementSystem
{
    public static class IconManager                                                        //For setting/getting icons
    {
        public static ImageSource GetFolderIcon()
        {
            //Vector folder icon
            var geometry = Geometry.Parse("M10,4H4C2.89,4 2,4.89 2,6V18C2,19.11 2.89,20 4,20H20C21.11,20 22,19.11 22,18V8C22,6.89 21.11,6 20,6H12L10,4Z");

            var drawing = new GeometryDrawing(
                Brushes.Goldenrod,
                new Pen(Brushes.DarkGoldenrod, 1),
                geometry);

            var drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            return drawingImage;
        }

        public static ImageSource GetFileIcon(string extension)
        {
            //Vector file icon
            var geometry = Geometry.Parse("M13,9V3.5L18.5,9M6,2C4.89,2 4,2.89 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2H6Z");

            var drawing = new GeometryDrawing(
                Brushes.LightSteelBlue,
                new Pen(Brushes.SteelBlue, 1),
                geometry);

            var drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            return drawingImage;
        }
    }
    public class projectJsonManagement                                                     //For loading and saving data to json
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
    public class projectDirectory                                                          //Main class that stores all of the inner folders and files
    {
        //--------------------------Varaibles---------------------------
        public string      folderName            { get; set; }
        public string      folderPath            { get; set; }
        public DateTime    addedDate             { get; set; }
        public int         innerFileCount        { get; set; } = 0;
        public int         innerDirCount         { get; set; } = 0;
        public float       size                  { get; set; } = 0;
        public ObservableCollection<projectFile>      innerFiles       { get; set; } = new ObservableCollection<projectFile>();
        public ObservableCollection<projectDirectory> innerDirectories { get; set; } = new ObservableCollection<projectDirectory>();


        public bool IsExpanded { get; set; }
        public bool HasSubDirectories = false;

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
        public void loadAllFilesInDirectory(projectDirectory directory)  //load all of the files inside of a directory
        {
            string path = directory.folderPath;

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
                    newFile.addedDate   = fileInfo.LastWriteTime;
                    newFile.fileSize    = (float)fileInfo.Length;
                    newFile.fileType    = fileInfo.Extension;
                    //newFile.icon        = IconManager.GetFileIcon(fileInfo.Extension);
                    directory.innerFiles.Add(newFile);
                }
                
            }
            catch (Exception ex)
            {
                //error box whenever you are unable to load a file
                //MessageBox.Show(
                //    "Unable to load directory, please check if path is correct",
                //    "Unable to load project",
                //    MessageBoxButton.OK,
                //    MessageBoxImage.Error
                //    );
            }
        }
        public void countAllInnerFiles(projectDirectory directory)       //recursively count all the files
        {
            //only add the main files once
            if(innerFileCount <= 0)
            {
                innerFileCount += innerFiles.Count;
            }

            string[] dirPaths = Directory.GetDirectories(directory.folderPath);

            //if there is folders inside of the folder
            if(dirPaths.Length > 0)
            {
                try
                {
                    foreach (string dirPath in dirPaths)
                    {
                        //get the amount of files in that folder, and then also check if that folder has inner dirs
                        string[] filePaths = Directory.GetFiles(dirPath);
                        innerFileCount += filePaths.Length / 100000000;

                        //set up directory so that way it can be passed into the function as well
                        var dirInfo = new DirectoryInfo(dirPath);
                        var newDirectory = new projectDirectory
                        {
                            folderPath = dirPath,
                        };

                        //recall the function to check
                        countAllInnerFiles(newDirectory);
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }
        public void countAllInnerDirectories(projectDirectory directory) //Also add recusively counted directories
        {
            //only add the main dirs once
            if (innerDirCount <= 0)
            {
                innerDirCount += innerDirectories.Count;
            }

            string[] dirPaths = Directory.GetDirectories(directory.folderPath);

            //if there is folders inside of the folder
            if (dirPaths.Length > 0)
            {
                //try in case you dont have access to some files
                try
                {
                    foreach (string dirPath in dirPaths)
                    {
                        //get the amount of dirs in that folder, and then also check if that folder has inner dirs
                        string[] directPaths = Directory.GetDirectories(dirPath);
                        innerDirCount += directPaths.Length;

                        //set up directory so that way it can be passed into the function as well
                        var dirInfo = new DirectoryInfo(dirPath);
                        var newDirectory = new projectDirectory
                        {
                            folderPath = dirPath,
                        };

                        //recall the function to check
                        countAllInnerDirectories(newDirectory);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        public void calculateSize(projectDirectory directory)
        {
            //inital condition
            if(size <= 0)
            {
                foreach(projectFile file in innerFiles)
                {
                    size += file.fileSize;
                }
            }

            //Check for dirs and inner dirs recursively
            string[] dirPaths = Directory.GetDirectories(directory.folderPath);

            //if there is folders inside of the folder
            if (dirPaths.Length > 0)
            {
                try
                {
                    foreach (string dirPath in dirPaths)
                    {
                        //get the amount of files in that folder, and then also check if that folder has inner dirs
                        string[] filePaths = Directory.GetFiles(dirPath);

                        //loop over all of the files
                        foreach (string filepath in filePaths)
                        {
                            var fileInfo = new FileInfo(filepath);
                            size += fileInfo.Length;
                        }

                        //set up directory so that way it can be passed into the function as well
                        var dirInfo = new DirectoryInfo(dirPath);
                        var newDirectory = new projectDirectory
                        {
                            folderPath = dirPath,
                        };

                        //recall the function to check
                        countAllInnerFiles(newDirectory);
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }
    }
    public class projectFile                                                               //File Class for any kind of file in project
    {
        //--------------------------Varaibles---------------------------
        public string      fileName    { get; set; }
        public string      filePath    { get; set; }
        public string      fileType    { get; set; }
        public float       fileSize    { get; set; }
        public DateTime    addedDate   { get; set; }
        
        [JsonIgnore]
        public ImageSource icon => IconManager.GetFileIcon(fileType);

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
        private projectJsonManagement                   jsonManager      = new projectJsonManagement();
        private ObservableCollection<projectDirectory>  savedDirectories = new ObservableCollection<projectDirectory>();
        private projectDirectory                        currentDir       = new projectDirectory();
        private List<projectDirectory>                  previousDir      = new List<projectDirectory>();
        public projectManager()
        {
            InitializeComponent();
            InitializeProjectList();
        }

        private async Task InitializeProjectList()
        {
            //diasable back button to prevent crashes
            backButton.IsEnabled = false;

            //load any given data from a json file
            savedDirectories = await jsonManager.loadJsonData<projectDirectory>();

            //then renable it
            backButton.IsEnabled = true;

            //set the saved Directories as the item source for the tree view
            mainTreeView.ItemsSource = savedDirectories;
        }
        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //save data and go back to main page
            await jsonManager.saveToJson(savedDirectories);
            this.NavigationService.GoBack();
        }

        //-----------------------Bottom Buttons Functions--------------------------

        private async void deleteDirectory(object sender, RoutedEventArgs e)              //delete a main selected directory
        {
            //check if a directory is highligted
            if(mainTreeView.SelectedItem != null)
            {
                //get as directory and remove from list
                var directory = mainTreeView.SelectedItem as projectDirectory;

                //ask the user if they are sure they want to delete this item?
                var result = MessageBox.Show(
                    "Are you sure you want to delete this item? This is quasi-permanent.",
                    "Delete item?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question
                    );

                if (result == MessageBoxResult.OK)
                {
                    // save the deletion
                    savedDirectories.Remove(directory);
                    await jsonManager.saveToJson(savedDirectories);
                }
                else if(result == MessageBoxResult.Cancel)
                {
                    //do nothing
                    return;
                }
            }
        }
        private async void updateAllDirectories(object sender, RoutedEventArgs e)         //recursively update all files for any changes
        {
            //create a temp list of directories
            ObservableCollection<projectDirectory> updatedDirectories = new ObservableCollection<projectDirectory>();

            //loop over current directories and copy/update them
            foreach(projectDirectory project in savedDirectories)
            {
                var newDirectory = new projectDirectory
                {
                    folderName = project.folderName,
                    folderPath = project.folderPath,
                    addedDate = DateTime.Now,
                };
                //Load all of the files and directories that are visible in that directory and add it to the main list 
                newDirectory.loadAllFilesInDirectory(newDirectory);
                checkForInnerDirectories(newDirectory);
                updatedDirectories.Add(newDirectory);
            }

            //Refresh Set the new saved directories
            savedDirectories = null;
            savedDirectories = updatedDirectories;
            mainTreeView.ItemsSource = savedDirectories;

            //do the button animation if function call came from front UI button
            if(sender is Button button)
            {
                //front UI button
                if(button.Name == "updateButton")
                {
                    //Disable button, wait for effect, and then reenable
                    updateButton.IsEnabled = false;
                    UpdateButtonText.Foreground = new SolidColorBrush(Colors.Gray);
                    await Task.Delay(2000);
                    updateButton.IsEnabled = true;
                    UpdateButtonText.Foreground = new SolidColorBrush(Colors.White);

                    //also save to json cuz why not
                    await jsonManager.saveToJson(savedDirectories);
                }
            }

        }
       
        //------------------------Add New Project GUI------------------------------
        private void NewProjectAnimation(object sender, RoutedEventArgs e)                 //Opens and closes the gui window for adding a new project
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
                    clearNewProjectGUI(sender, e);
                    
                }
            }
        }
        private void clearNewProjectGUI(object sender, RoutedEventArgs e)                  //Clears all of the inputed data
        {
            //clear the textboxes that are in view
            projectNameTB.Text = string.Empty;
            projectPathTB.Text = string.Empty;
        }
        private async void addNewProject(object sender, RoutedEventArgs e)                 //Create a new project object revurisvely using 
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
            //Load all of the files and directories that are visible in that directory and add it to the main list 
            newDirectory.loadAllFilesInDirectory(newDirectory);
            checkForInnerDirectories(newDirectory);
            savedDirectories.Add(newDirectory);

            //save all of the data to json
            await jsonManager.saveToJson(savedDirectories);

            //do the closing animation when finished
            NewProjectAnimation(sender, e);

        }
        private bool checkForInnerDirectories(projectDirectory directory)                  //function to set the bool for the directoies that might be within of a directory
        {
            //try to get inner directories
            try
            {
                //use the built in tools to get all directory paths
                string[] directPaths = Directory.GetDirectories(directory.folderPath);

                //if there are paths return true, otherwise false
                if(directPaths.Length > 0)
                {
                    //create a new directory item, and then make sure to update the list properly
                    foreach(string path in directPaths)
                    {
                        // Create new project directory
                        DirectoryInfo info = new DirectoryInfo(path);
                        var newDirectory = new projectDirectory
                        {
                            folderName = info.Name,
                            folderPath = path,
                            addedDate = DateTime.Now,
                        };

                        //update all of the files in there
                        newDirectory.loadAllFilesInDirectory(newDirectory);

                        //also call this function recursively to get all other directories and files
                        checkForInnerDirectories(newDirectory);

                        //add this directory to the list of ones held in main directory
                        directory.innerDirectories.Add(newDirectory);
                    }


                    //update inner bool and return true cuz why not
                    directory.HasSubDirectories = true;
                    return true;
                }
                else
                {
                    //bool stays the same, just return false
                    return false;
                }
            }
            //exception in case the directory does not exist
            catch(Exception ex)
            {
                return false;
            }
        }
                                                                                           
        //---------------------Folders and Files GUI-------------------------------                                                                   
        private void filesAndFoldersAnimation(object sender, MouseButtonEventArgs e)       //The function that would load the opening animation of a project gui
        {
            Debug.WriteLine("RAN IN THE OTHER FILES AND FOLDERS ANIMATION");
            if (sender is TreeView view)
            {
                if(view.SelectedItem != null)
                {
                    initTreeAndListView();
                    Storyboard sb = new Storyboard();

                    //Set the transform origin on the Border itself
                    FilesAndFoldersGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 10);
                    Panel.SetZIndex(FilesAndFoldersGUI, 11);

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

                    Storyboard.SetTarget(moveDOWN, FilesAndFoldersGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    sb.Children.Add(moveDOWN);
                    sb.Children.Add(opacityAdd);
                    sb.Begin();
                }
            }
        }
        private void filesAndFoldersClose(object sender, RoutedEventArgs e)                //The close function for the above GUI
        {
            if(sender is Button button)
            {
                if(button.Name == "closeFilesAndFoldersGUI")
                {
                    Storyboard sb = new Storyboard();

                    //Set the transform origin on the Border itself
                    FilesAndFoldersGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

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

                    Storyboard.SetTarget(moveDOWN, FilesAndFoldersGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    sb.Children.Add(moveDOWN);
                    sb.Children.Add(opacityAdd);
                    sb.Begin();

                    Panel.SetZIndex(TranslucentBox, -5);
                    clearVisibleLists();
                }
            }
        }
        private void clearVisibleLists()                                                   //Clear the tree view and list view of the ff GUI
        {
            ffTreeView.ItemsSource = null;
            ffListView.ItemsSource = null;

            //clear the previous dir and disable button
            previousDir.Clear();
            currentDir  = new projectDirectory();
            ffBackDirectory.IsEnabled = false;
        }
        private void initTreeAndListView()                                                 //Loads in all information
        {
            //Check to see if an item is selected
            if(mainTreeView.SelectedItem != null)
            {
                //get the item as a projectDirectory Class
                var directory = mainTreeView.SelectedItem as projectDirectory;

                //Fill out proper values
                ffProjectName.Text     = directory.folderName;
                ffTreeView.ItemsSource = directory.innerDirectories;
                ffListView.ItemsSource = directory.innerFiles;
                currentDir             = directory;


            }
        }
        private void openFile(projectFile file)                                            //Opens a file at the given path it exists at
        {
            try
            {
                //check if the file exists
                if(File.Exists(file.filePath))
                {
                    //using the process class, open the file
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = file.filePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    //warn user that file isnt there and to update their view
                    MessageBox.Show(
                        "File no longer exists at that path. Please update and try again",
                        "Unable to find file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(
                    "Error opening file!",
                    "You done goofed up",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
        }
        private void openDirectory(projectDirectory directory)                             //Loads directory information into visible GUI
        {
            //change all of the visible content
            ffProjectName.Text = directory.folderName;
            ffTreeView.ItemsSource = directory.innerDirectories;
            ffListView.ItemsSource = directory.innerFiles;

            currentDir = directory;

            //check to see if previous dir is not empty
            if (previousDir.Count > 0)
            {
                //update the previous dir and enable the button
                ffBackDirectory.IsEnabled = true;
            }
            else
            {
                //disable button
                ffBackDirectory.IsEnabled = false;
            }
        } 
        private void ffListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)    //Opens a file on double click
        {
            //check to see if a file is selected and then open it
            if(ffListView.SelectedItem != null)
            {
                var file = ffListView.SelectedItem as projectFile;
                openFile(file);
            }
        }
        private void ffTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)    //loads directory content on double click
        {
            if(ffTreeView.SelectedItem != null)
            {
                var directory = ffTreeView.SelectedItem as projectDirectory;

                //save the old path
                previousDir.Add(currentDir);

                //get the directory and then run the open function
                openDirectory(directory);
            }
        }
        private void openFileExplorerAtPath(object sender, RoutedEventArgs e)              //open up file explorer for user
        {
            //check if dir is empty or not
            if(!string.IsNullOrEmpty(currentDir.folderPath))
            {
                //open with explorer at given path
                System.Diagnostics.Process.Start("explorer.exe", currentDir.folderPath);
            }
        }
        private void ffbackDirectory_click(object sender, RoutedEventArgs e)               //change directories when button is clicked
        {
            Debug.WriteLine("Before Popping");
            foreach (projectDirectory project in previousDir)
            {
                Debug.WriteLine(project.folderPath);
            }

            //check to see if the path is null
            if (!string.IsNullOrEmpty(previousDir[previousDir.Count-1].folderPath))
            {
                //open the dir and then pop it from the list
                openDirectory(previousDir[previousDir.Count-1]);
                previousDir.RemoveAt(previousDir.Count-1);
            }

            if(previousDir.Count <= 0 )
            {
                ffBackDirectory.IsEnabled = false;

            }

            Debug.WriteLine("After Popping");
            foreach(projectDirectory project in previousDir)
            {
                Debug.WriteLine(project.folderPath);
            }
        }
        private void deleteSelectedItem(object sender, RoutedEventArgs e)                  //Delete a selected Item In a directory
        {
            //Check to see if selected item is folder or file never both
            if(ffTreeView.SelectedItem != null)
            {
                projectDirectory dir = ffTreeView.SelectedItem as projectDirectory;

                //ask the user if they are sure they want to delete this item?
                var result = MessageBox.Show(
                    "Are you sure you want to delete this item? This is quasi-permanent.",
                    "Delete item?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question
                    );

                if (result == MessageBoxResult.OK)
                {
                    //remove directory and update fileexplorer as well
                    try
                    {
                        //remove from given list, and from actual file
                        currentDir.innerDirectories.Remove(dir);
                        Directory.Delete(dir.folderPath);
                    }
                    catch
                    {
                        MessageBox.Show(
                        "Unable to remove directory. Please make sure you have permissions to delete this item",
                        "Unable to Remove Directory",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                    }
                }
                else if(result == MessageBoxResult.Cancel)
                {
                    //simply do nothing
                    return;
                }

            }
            else if(ffListView.SelectedItem != null)
            {
                projectFile file = ffListView.SelectedItem as projectFile;
                //ask the user if they are sure they want to delete this item?
                var result = MessageBox.Show(
                    "Are you sure you want to delete this item? This is quasi-permanent.",
                    "Delete item?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question
                    );

                if (result == MessageBoxResult.OK)
                {
                    //remove directory and update fileexplorer as well
                    try
                    {
                        //remove from given list, and from actual file
                        currentDir.innerFiles.Remove(file);
                        File.Delete(file.filePath);
                    }
                    catch
                    {
                        MessageBox.Show(
                        "Unable to remove file. Please make sure you have permissions to delete this item",
                        "Unable to Remove File",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    //simply do nothing
                    return;
                }
            }
        }

        //---------------------Show Project Info-------------------------------                                                                   
        private void doProjectInfoAnimation(object sender, RoutedEventArgs e)              //opens and closes the project info GUI
        {
            //get button for button name
            if (sender is Button button)
            {
                Storyboard sb = new Storyboard();

                //The open sequence
                if (button.Name == "infoButton")
                {
                    //load in all of the needed information
                    loadDirectoryInfo();

                    //Set the transform origin on the Border itself
                    ProjectInfoGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 10);
                    Panel.SetZIndex(ProjectInfoGUI, 11);

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

                    Storyboard.SetTarget(moveDOWN, ProjectInfoGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    sb.Children.Add(moveDOWN);
                    sb.Children.Add(opacityAdd);
                    sb.Begin();
                }
                //the close sequence
                else if (button.Name == "closeProjectInfoGUI" || button.Name == "PISaveButton")
                {
                    //Set the transform origin on the Border itself
                    ProjectInfoGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

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

                    Storyboard.SetTarget(moveDOWN, ProjectInfoGUI);
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
        private void loadDirectoryInfo()                                                   //Sets up the textboxes for viewing
        {
            //check if an item is selected
            if(mainTreeView.SelectedItem != null)
            {
                //get the item as a directory
                var dir = mainTreeView.SelectedItem as projectDirectory;
                
                //Check to see if you have un inited counts
                if(dir.innerFileCount <= 0)
                {
                    dir.countAllInnerFiles(dir);
                }
                //Also run seperate checks for dirs
                if(dir.innerDirCount <= 0)
                {
                    dir.countAllInnerDirectories(dir);
                }
                //Get the physical size of the folder
                if(dir.size <= 0)
                {
                    dir.calculateSize(dir);
                }

                PITitle.Text = $"File Info For: {dir.folderName}";

                //update all visuals based on information
                PIprojectNameTB.Text       = dir.folderName;
                PIprojectPathTB.Text       = dir.folderPath;
                PIinnderFilesTB.Text       = dir.innerFileCount.ToString();
                PIinnderDirectoriesTB.Text = dir.innerDirCount.ToString();
                PIDateAddedTB.Text         = dir.addedDate.ToString();
                PITotalSizeTB.Text         = dir.size.ToString();

            }
        }
        private void clearDirectoryInfo(object sender, RoutedEventArgs e)                  //clear path and name of directory 
        {
            //ask user if they are sure they want to clear
            var result = MessageBox.Show(
                "Are you sure you want to clear all info? This cannot be undone",
                "Clear all Info?",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question
                );

            //if cancel then dont do anything
            if(result == MessageBoxResult.Cancel)
            {
                return;
            }
            //if yes then clear all fields
            else if(result == MessageBoxResult.OK)
            {
                //update all visuals based on information
                PIprojectNameTB.Text       = string.Empty;
                PIprojectPathTB.Text       = string.Empty;
            }
        }
        private async void saveDirectoryInfo(object sender, RoutedEventArgs e)             //save any changed dir info
        {
            //ask user if they are sure they want to save
            var result = MessageBox.Show(
                "Are you sure you want to save all info? This will change data about the file",
                "Save all Info?",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question
                );


            //if cancel then dont do anything
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }
            else if (result == MessageBoxResult.OK)
            {

                //if all fields are not empty, then udate the inner class project
                if(mainTreeView.SelectedItem != null && (!string.IsNullOrEmpty(PIprojectNameTB.Text) && !string.IsNullOrEmpty(PIprojectPathTB.Text)))
                {

                    var dir = mainTreeView.SelectedItem as projectDirectory;

                    //check if the file path exists at path and if so then update, if not then revert
                    if (Directory.Exists(PIprojectPathTB.Text))
                    {
                        dir.folderName      = PIprojectNameTB.Text;
                        dir.folderPath      = PIprojectPathTB.Text;
                        dir.size            = 0;
                        dir.innerDirCount   = 0;
                        dir.innerFileCount  = 0;
                        dir.innerDirectories.Clear();
                        dir.innerFiles.Clear();

                        //clear dir's content and reload it
                        dir.loadAllFilesInDirectory(dir);
                        checkForInnerDirectories(dir);

                        //also save to json
                        await jsonManager.saveToJson(savedDirectories);
                        doProjectInfoAnimation(sender, e);
                    }
                    else
                    {
                        MessageBox.Show(
                            "The Given Path Does not exist, please enter valid path and try again",
                            "Path Not Found!",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                            );

                        //Revert back to old values
                        PIprojectNameTB.Text = dir.folderName;
                    }   PIprojectPathTB.Text = dir.folderPath;

                    
                }
                else
                {
                    var dir = mainTreeView.SelectedItem as projectDirectory;

                    MessageBox.Show(
                        "Please fill out all fields",
                        "Incomlplete Form",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );

                    //Revert back to old values
                    PIprojectNameTB.Text = dir.folderName;
                    PIprojectPathTB.Text = dir.folderPath;
                }
            }

            //also just reload the view
            mainTreeView.ItemsSource = null;
            mainTreeView.ItemsSource = savedDirectories;

        }
    
        //--------------------Add new item GUI--------------------------------
        private void doNewItemAnimation(object sender, RoutedEventArgs e)                  //Open and close the Add new File or Folder GUI
        {
            //get button for button name
            if (sender is Button button)
            {
                Storyboard sb = new Storyboard();

                //The open sequence
                if (button.Name == "ffCreateButton")
                {

                    //Set the transform origin on the Border itself
                    AddNewGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 15);
                    Panel.SetZIndex(AddNewGUI, 16);

                    DoubleAnimation moveDOWN = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };

                    Storyboard.SetTarget(moveDOWN, AddNewGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));

                    sb.Children.Add(moveDOWN);
                    sb.Begin();
                }
                //the close sequence
                else if (button.Name == "closeAddNewGUI" || button.Name == "ANCreateButton")
                {
                    //Set the transform origin on the Border itself
                    AddNewGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    DoubleAnimation moveDOWN = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.3),
                    };

                    Storyboard.SetTarget(moveDOWN, AddNewGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));

                    sb.Children.Add(moveDOWN);
                    sb.Begin();

                    Panel.SetZIndex(TranslucentBox, 10);
                }
            }
        }
        private void clearAddNewGUI(object sender, RoutedEventArgs e)                      //Clear any of the changes made by user
        {
            ANNameTB.Text = string.Empty;
            ANTypeTB.Text = string.Empty;
            FileRB.IsChecked = true;
            ANComboBox.SelectedItem = null;
        }
        private void FolderRB_Checked(object sender, RoutedEventArgs e)                    //Just handles radio button
        {
            //Enable the type text box
            ANTypeTB.IsEnabled = true;
            ANComboBox.IsEnabled = true;
            ANComboBox.IsEnabled = true;
            ANComboBox.SelectedItem = null;
        }
        private void FileRB_Checked(object sender, RoutedEventArgs e)                      //Just handles radio button
        {
            //Clear the text box and also disable it
            ANTypeTB.Text = string.Empty;
            ANTypeTB.IsEnabled = false;
            ANComboBox.IsEnabled = false;
        }
        private void ANComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)//Update the text to reflect user's choice
        {
            //Update the Text in the tb to what was selected in the combo box
            ANTypeTB.Text = ANComboBox.SelectedItem.ToString().Remove(0, 38);
        }
        private void CreateFileOrFolder(object sender, RoutedEventArgs e)                  //Create the system file at that given directory
        {
            if(string.IsNullOrEmpty(ANNameTB.Text))
            {
                //let the person know at least a name is needed then return to not run rest of function
                MessageBox.Show(
                    "Please enter in a name for this item.",
                    "Empty name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop
                    );

                return;
            }

            //create file
            if(FolderRB.IsChecked == true)
            {
                //check if the file has a type
                if (string.IsNullOrEmpty(ANTypeTB.Text))
                {
                    MessageBox.Show(
                        "Please enter in a type for this item.",
                        "Empty name",
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop
                        );

                    return;
                }

                //combine the current directory path, the inputted name, and also the desired type
                string filePath = System.IO.Path.Combine(currentDir.folderPath, ANNameTB.Text+ANTypeTB.Text);

                //check to see if a dir like this already exists
                if (!File.Exists(filePath))
                {
                    //try and create a new file
                    try
                    {
                        //create the dir and close the window
                        File.Create(filePath);

                        //also create a new directory item and appened
                        projectFile file = new projectFile(ANNameTB.Text, filePath);
                        currentDir.innerFiles.Add(file);

                        //refresh the view
                        ffListView.ItemsSource = null;
                        ffListView.ItemsSource = currentDir.innerFiles;
                        doNewItemAnimation(sender, e);
                    }
                    catch
                    {
                        //say you cannot create a new file 
                        MessageBox.Show(
                        "Error Creating File. Please try again later.",
                        "Invalid File",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                    }
                }
                else
                {
                    //dont create the dir and let the user know
                    MessageBox.Show(
                    "File of that Name Already Exists! Please enter a new name.",
                    "Repeat File",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );

                    return;
                }

            }
            //create a folder
            else if(FileRB.IsChecked == true)
            {
                string folderPath = System.IO.Path.Combine(currentDir.folderPath, ANNameTB.Text);

                //check to see if a dir like this already exists
                if(!Directory.Exists(folderPath))
                {
                    //create the dir and close the window
                    Directory.CreateDirectory(folderPath);
                    doNewItemAnimation(sender, e);

                    //also create a new directory item and appened
                    projectDirectory dir = new projectDirectory(ANNameTB.Text, folderPath);
                    currentDir.innerDirectories.Add(dir);

                    //refresh the view
                    ffTreeView.ItemsSource = null;
                    ffTreeView.ItemsSource = currentDir.innerDirectories;
                }
                else
                {
                    //dont create the dir and let the user know
                    MessageBox.Show(
                    "Directory of that Name Already Exists! Please enter a new name.",
                    "Repeat Directory",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );

                    return;
                }
            }
        }
    }
}
