using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
//added dependecies
using Octokit;



namespace CodeManagementSystem
{
    public class githubJsonManagement                                                     //For loading and saving data to json
    {
        private readonly string _JsonPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CodeInformationManagingSystem\\GithubRepoManagement\\JSON\\");
        private readonly string _jsonStoragePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CodeInformationManagingSystem\\GithubRepoManagement\\JSON\\");


        //save to json informaton
        public async Task saveToJson(ObservableCollection<githubRepository> dataPaths)
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
            await File.WriteAllTextAsync(System.IO.Path.Combine(_JsonPath, "githubRepos.json"), json);
        }

        //-------------------------------The loading functions-------------------------------------
        public async Task<ObservableCollection<T>> loadJsonData<T>()
        {
            // Check to see if path exists, if not return empty
            if (!File.Exists(System.IO.Path.Combine(_jsonStoragePath, "githubRepos.json")))
            {
                return new ObservableCollection<T>();
            }


            //Await until all of the json data is read
            string json = await File.ReadAllTextAsync(System.IO.Path.Combine(_jsonStoragePath, "githubRepos.json"));

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
        public githubJsonManagement()
        {

        }
    }

    public class githubMetaData
    {

        //-----------------------------varaibles-----------------------
        public string id               { get; set; }
        public string ownerName        { get; set; }
        public string repoName         { get; set; }
        public string repoFullName     { get; set; }
        public string Language         { get; set; }
        public string Description      { get; set; }
        public string cloneURL         { get; set; }
        public string readMeContent    { get; set; }
        public string defaultBranch    { get; set; }
        //see if possible to get things like languages, size, readme content

        //-----------------------------constructor-----------------------
        public githubMetaData() { }
    }

    public class githubRepository
    {
        //-----------------------------Varaibles-------------------------
        public string url              { get; set; }
        public string name             { get; set; }
        public List<string> tags       { get; set; } = new List<string>();
        public string collection       { get; set; } = "None";
        public string status           { get; set; }
        public bool isStarred          { get; set; } = true;
        public githubMetaData metaData { get; set; } = new githubMetaData();

        [JsonIgnore]
        private GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("CodeMangementSystem"));


        //-----------------------------Functions-------------------------
        public void getOwnerAndName(string url)
        {
            //try split up the repo link
            var parts = url.Replace("https://github.com/", "").Split('/');
            if(parts.Length < 2)
            {
                //show message to user saying youre unable to get metadata then return
                MessageBox.Show(
                    "Unable to get metadata from Repository Link, regular repo will still be added",
                    "Unable to get metadata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                    );

                return;
            }

            //update metaData information to hold ownerName and repoName
            metaData.ownerName = parts[0];
            metaData.repoName = parts[1].Replace(".git", "");
        }
        public async Task getMetaData(string owner, string repoName)
        {
            try
            {
                //get the main repo object and get the data from there
                var repo = await gitHubClient.Repository.Get(owner, repoName);
                metaData.repoFullName  = repo.FullName;
                metaData.Language      = repo.Language;
                metaData.Description   = repo.Description;
                metaData.cloneURL      = repo.CloneUrl;
                metaData.defaultBranch = repo.DefaultBranch;

                //get the reade me as well (gotta be seperate)
                Octokit.Readme readme = await gitHubClient.Repository.Content.GetReadme(owner, repoName);
                metaData.readMeContent = readme.Content;
            }
            catch
            {
                //Let User Know that there was an error in getting some metadata
                    MessageBox.Show(
                    "There was an error in getting some part of the metadata",
                    "Unable to get full metadata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
        }
        //-----------------------------constructor-----------------------
        public githubRepository(string url)
        {
            this.url = url;
        }
    }


    public partial class githubManager : System.Windows.Controls.Page
    {
        private ObservableCollection<githubRepository> githubRepositories   = new ObservableCollection<githubRepository>();
        private githubJsonManagement                   githubJsonManagement = new githubJsonManagement();
        private List<string>                           tagsToAdd            = new List<string>();
        public githubManager()
        {
            InitializeComponent();
            loadInContent();        //Set Up repo collection
            loadTags();             //for add new repo view
        }
        
        private async void loadInContent()                                                       //Set up the collection for the listview
        {
            //load in any saved data from json
            githubRepositories = await githubJsonManagement.loadJsonData<githubRepository>();

            //also set the itemsource for the main viewbox
            githubListView.ItemsSource = githubRepositories;
        }
        private async void BackButton_Click(object sender, RoutedEventArgs e)                    //Naviagte back to the main page
        {
            //return back to the main page
            this.NavigationService.GoBack();
            await githubJsonManagement.saveToJson(githubRepositories);
        }

        //-------------------The Main List Box Button Functions-----------------------
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)       //Redirect to the given github repo
        {
            try
            {
                //Try to open the link that the user inputted
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                //If link is invalid or is unable to open, then let user know and abort the process so no crash happens
                MessageBox.Show(
                    $"Unable to open URL: {e.Uri}. Please enter valid URL and try again.",
                    "Invalid URL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                e.Handled = true; //handle the event so the .net system doesnt kill itself
            }

        }
        private void OpenContextMenu(object sender, RoutedEventArgs e)                          //get where you clicked, and then summon the little context menu to open there
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


            //Get the context menu from the key in App.cs
            ContextMenu menu = this.FindResource("VideoContextMenuKey") as ContextMenu; //lowkey just use the same context menu

            if (menu != null) //if menu is not null, then display it
            {
                //Open the Context Menu
                menu.IsOpen = true;

                //Add in click function based on header value

                foreach (MenuItem item in menu.Items)
                {
                    string name = item.Header.ToString();
                    //if (name == "Edit") { item.Click += openInfoGUI; }
                    //else if (name == "Save") { item.Click += saveVideoPage; }
                    //else if (name == "Delete") { item.Click += deleteSelectedItem; }
                    //else if (name == "View Full Info") { item.Click += openInfoGUI; }

                }

            }
        }
        private void LoadListViewTags(object sender, RoutedEventArgs e)                         //Load in the tags as visible buttons
        {
            //check if is wrapanel
            if (sender is WrapPanel wrapPanel)
            {
                //get the data context as the custom class
                githubRepository gitRepo = wrapPanel.DataContext as githubRepository;

                //add in a button for each of the tags in the class tags list
                foreach (string tag in gitRepo.tags)
                {

                    //create a new button with all of the different styles it needs to have
                    Button button = new Button
                    {
                        Content = tag,
                        Margin = new Thickness(2),
                        Style = (Style)this.FindResource("EvenLessRoundedButton"),
                        Foreground = new SolidColorBrush(Colors.White),
                        Background = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush,
                        BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(3, 1, 3, 1),
                        BorderThickness = new Thickness(2),
                        MinHeight = 10,
                        MinWidth = 10

                    };
                    wrapPanel.Children.Add(button);
                }
            }
        }
        private void LoadFavoritedStatus(object sender, RoutedEventArgs e)                      //Check to see if item is a favorite or not
        {
            //get the rectangle as the sender
            if(sender is Rectangle rec)
            {
                //get the data context as the custom class
                githubRepository gitRepo = rec.DataContext as githubRepository;

                //only if the repo is favorited, you should change it
                if(gitRepo.isStarred)
                {
                    if(rec.OpacityMask is ImageBrush imageBrush)
                    {
                        //update the color to be gold, and change the image to be full
                        rec.Fill = new SolidColorBrush(Colors.Gold);
                        imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CodeManagementSystem;component/Images/star.png"));
                    }
                }
            }
        }
        private async void favoriteStarButtonClick(object sender, RoutedEventArgs e)            //update favorited status as needed
        {
            //get the button
            if(sender is Button button)
            {
                //get the custom class from button
                githubRepository gitRepo = button.DataContext as githubRepository;

                
                if(button.Content is StackPanel panel)
                {
                    //get all of the inner button content
                    foreach(var child in panel.Children)
                    {
                        //locate rectange
                        if (child is Rectangle rectangle && rectangle.Name == "favoriteRectangle")
                        {
                            //get the image brush
                            if (rectangle.OpacityMask is ImageBrush imageBrush)
                            {
                                if(gitRepo.isStarred)
                                {
                                    //unstar the inner repo variable
                                    gitRepo.isStarred = !gitRepo.isStarred;

                                    //change the visual to reflect the change
                                    rectangle.Fill = new SolidColorBrush(Colors.DarkBlue);
                                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CodeManagementSystem;component/Images/star-outline.png"));

                                }
                                else
                                {
                                    //star the inner repo variable
                                    gitRepo.isStarred = !gitRepo.isStarred;

                                    //change the visual to reflect the change
                                    rectangle.Fill = new SolidColorBrush(Colors.Gold);
                                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/CodeManagementSystem;component/Images/star.png"));
                                }
                            }
                            break;
                        }
                    }
                }

                //reorder and save the new repos
                await reorderListBasedOnFavorite();


            }
        }
        private async Task reorderListBasedOnFavorite()                                         //updates list order
        {
            //create temp empty list
            ObservableCollection<githubRepository> tempList = new ObservableCollection<githubRepository>();
            
            //add in all of the starred repos first
            foreach(githubRepository repo in githubRepositories)
            {
                if(repo.isStarred)
                {
                    tempList.Add(repo);
                }
            }

            //then add in all of the non stared repos
            foreach(githubRepository repo in githubRepositories)
            {
                if (!repo.isStarred)
                {
                    tempList.Add(repo);
                }
            }

            //update the github repo list
            githubRepositories = tempList;

            //refresh the list box
            githubListView.ItemsSource = null;
            githubListView.ItemsSource = githubRepositories;

            //save the new order to the json
            await githubJsonManagement.saveToJson(githubRepositories);


        }
       
        //--------------------------The Side Button Functions-------------------------

        //-------------------------The Add New Repo GUI-------------------------------
        private void doAddNewAnimation(object sender, RoutedEventArgs e)                        //handle opening and closing the new repo info
        {
            if (sender is Button button)
            {
                Storyboard sb = new Storyboard();


                //The open sequence
                if (button.Name == "AddNewRepoButton")
                {
                    //Set the transform origin on the Border itself
                    NewRepoGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 10);
                    Panel.SetZIndex(NewRepoGUI, 11);

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

                    Storyboard.SetTarget(moveDOWN, NewRepoGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    sb.Children.Add(moveDOWN);
                    sb.Children.Add(opacityAdd);
                    sb.Begin();
                }
                //the close sequence
                else if (button.Name == "closeAddNewGUI" || button.Name == "ANCreateButton")
                {
                    //Set the transform origin on the Border itself
                    NewRepoGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

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

                    Storyboard.SetTarget(moveDOWN, NewRepoGUI);
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
        private void loadTags()                                                                 //Set up visible tags in the wrappanel
        {
            //List all of the avalible tags for a github repo
            string[] tags =
            {
                "C", "C#", "C++", "Python", "Java", "Javascript-TS", "Go", "Rust", "PHP", "Lua",
                "Frontend", "Backend", "Fullstack", "CSS-UI", "React native", "Electron",
                "WPF", "Flutter", "Swift", "Dev Tools", "CLI-Tools", "Automation",
                "Framework", "Library", "BolierPlate", "Demo", "Tutorial", "OS", "Learning",
                "CheatSheet", "Documentation", "Awesome-List", "Books", "Icons-Fonts", "Design",
                "Game-Dev", "Cyber Security", "Low-Level", "Education", "Science", "Engineering",
                "graphics", "3d" 
            };

            //add in a button for each of them in the tags panel
            foreach(string tag in tags)
            {
                //create a new button with all of the different styles it needs to have
                Button button = new Button
                {
                    Content         = tag,
                    Margin          = new Thickness(5),
                    Style           = (Style)this.FindResource("EvenLessRoundedButton"),
                    Foreground      = new SolidColorBrush(Colors.White),
                    Background      = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush,
                    BorderBrush     = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush,
                    FontWeight      = FontWeights.Bold,
                    Padding         = new Thickness(10, 5, 10, 5),
                    BorderThickness = new Thickness(2),
                    MinHeight = 40,
                    MinWidth = 40

                };

                //append the click handle function and then add the button to the wrap panel
                button.Click += tagButtonClick;
                TagsPanel.Children.Add(button);
            }
        }
        private void tagButtonClick(object sender, RoutedEventArgs e)                           //Add the tag to the current button
        {
            //check if button
            if(sender is Button button)
            {
                //get button foreground as brush
                if(button.Foreground is System.Windows.Media.SolidColorBrush solidColorBrush)
                {
                    //check to see if button is gray
                    if (solidColorBrush.Color == System.Windows.Media.Colors.DarkGray)
                    {
                        //if so, then make it blue again
                        button.Foreground  = new SolidColorBrush(Colors.White);
                        button.BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush;
                        button.Background  = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush;

                        //also remove the tag from the current tag list
                        tagsToAdd.Remove(button.Content.ToString());
                    }
                    else if (solidColorBrush.Color == System.Windows.Media.Colors.White)
                    {

                        //if not, then make it gray because it's blue
                        button.Foreground  = new SolidColorBrush(Colors.DarkGray);
                        button.Background  = new SolidColorBrush(Colors.LightGray);
                        button.BorderBrush = new SolidColorBrush(Colors.DarkGray);

                        //add a given tag to a given tag list
                        tagsToAdd.Add(button.Content.ToString());
                    }
                }
            }
        }
        private void clearAllFields(object sender, RoutedEventArgs e)                           //Clear the input fields and clear tags as well
        {
            //clear all of the textboxes first
            ANCollection.Text = string.Empty;
            ANLink.Text       = string.Empty;
            ANName.Text       = string.Empty;

            //Reset the selection for the radio buttons
            YesRB.IsChecked = true;

            //clear all of the possible selections for the tags
            foreach(UIElement element in TagsPanel.Children)
            {
                if(element is Button button)
                {
                    //change back to enabled color scheme
                    button.Foreground  = new SolidColorBrush(Colors.White);
                    button.BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush;
                    button.Background  = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush;
                }
            }
        }
        private async void addInNewRepo(object sender, RoutedEventArgs e)                       //Get new repo content and save it
        {
            //check to see if all required fields are picked out
            if(!string.IsNullOrEmpty(ANLink.Text) || !string.IsNullOrEmpty(ANName.Text))
            {
                //update saving button to reflect saving
                ANCreateButton.IsEnabled = false;
                ANCreateText.Text = "Creating...";
                closeAddNewGUI.IsEnabled = false;

                //create a new repository item 
                try
                {
                    githubRepository gitRepo = new githubRepository(ANLink.Text);
                    gitRepo.getOwnerAndName(gitRepo.url);
                    gitRepo.name   = ANName.Text;
                    gitRepo.status = "Unlooked";

                    //check to see if user wants to get the metaData from the repo
                    if(YesRB.IsChecked == true)
                    {
                        //call the get metadata function
                        await gitRepo.getMetaData(gitRepo.metaData.ownerName, gitRepo.metaData.repoName);
                    }
                    //see if user added in collection
                    if(!string.IsNullOrEmpty(ANCollection.Text))
                    {
                        gitRepo.collection = ANCollection.Text;
                    }
                    //update the tags of the github repo and then clear tagsToAdd
                    foreach(string tag in tagsToAdd)
                    {
                        gitRepo.tags.Add(tag);
                    }
                    tagsToAdd.Clear();

                    //add the repo to the list
                    githubRepositories.Add(gitRepo);
                }
                catch
                {
                    //show user their error of not filling out all fields
                    MessageBox.Show(
                        "Unable to get full metadata from repo",
                        "Error getting all metadata",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                }

                //save to the json file, clear fields, and then close the window
                await githubJsonManagement.saveToJson(githubRepositories);
                clearAllFields(sender, e);
                doAddNewAnimation(sender, e);

                //reable all buttons
                ANCreateButton.IsEnabled = true;
                ANCreateText.Text = "Add New Repo";
                closeAddNewGUI.IsEnabled = true;
            }
            else
            {
                //show user their error of not filling out all fields
                MessageBox.Show(
                    "Please Fill out all non-optional fields before creating a new Repository Item",
                    "Not all fields met",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }

        }
    }
}
