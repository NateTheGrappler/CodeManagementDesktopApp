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
        public string id               { get; set; } = "No MetaData Found";
        public string ownerName        { get; set; } = "No MetaData Found";
        public string repoName         { get; set; } = "No MetaData Found";
        public string repoFullName     { get; set; } = "No MetaData Found";
        public string Language         { get; set; } = "No MetaData Found";
        public string Description      { get; set; } = "No MetaData Found";
        public string cloneURL         { get; set; } = "No MetaData Found";
        public string readMeContent    { get; set; } = "No MetaData Found";
        public string defaultBranch    { get; set; } = "No MetaData Found";
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
        public string status           { get; set; } = "NonStarted";
        public bool isStarred          { get; set; } = false;
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
                if (readme != null)
                {
                    metaData.readMeContent = readme.Content;
                }
                else
                {
                    metaData.readMeContent = "No README found";
                }
            }
            catch (Octokit.RateLimitExceededException rlex)
            {
                MessageBox.Show(
                    "GitHub API rate limit exceeded. Please try again later.",
                    "Rate Limit Exceeded",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch(Exception ex)
            {
                //Let User Know that there was an error in getting some metadata
                    MessageBox.Show(
                    "Unable to get Readme due to repo's settings",
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
        private ObservableCollection<githubRepository> githubRepositories     = new ObservableCollection<githubRepository>();
        private ObservableCollection<githubRepository> collectionRepositories = new ObservableCollection<githubRepository>();
        private ObservableCollection<githubRepository> statusRepositories     = new ObservableCollection<githubRepository>();
        private githubJsonManagement githubJsonManagement                     = new githubJsonManagement();
        private List<string> tagsToAdd                                        = new List<string>();
        private List<string> collectionList                                   = new List<string>();
        
        public githubManager()
        {
            InitializeComponent();
            loadInContent();        //Set Up repo collection
            loadTags();             //for add new repo view
        }

        private async void loadInContent()                                                      //Set up the collection for the listview
        {
            //load in any saved data from json
            githubRepositories = await githubJsonManagement.loadJsonData<githubRepository>();

            //load through the repos to get all of the collections
            foreach(githubRepository repo in githubRepositories)
            {
                //check if condition is in list, if not, add it
                if(!collectionList.Contains(repo.collection))
                {
                    collectionList.Add(repo.collection);
                }
            }
            //set the collections list box to have items
            CollectionsComboBox.ItemsSource = collectionList;
            AddCB.ItemsSource = collectionList;


            //also set the itemsource for the main viewbox
            githubListView.ItemsSource = githubRepositories;
        }
        private async void BackButton_Click(object sender, RoutedEventArgs e)                   //Naviagte back to the main page
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
                    if (name == "Edit")                { item.Click += doSeeInfoForContext; }
                    else if (name == "Save")           { item.Click += saveGivenRepo     ; }
                    else if (name == "Delete")         { item.Click += deleteSelectedRepo; }
                    else if (name == "View Full Info") { item.Click += doSeeInfoForContext; }

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
                        Margin = new Thickness(2, 3, 2, 3),
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
            if (sender is Rectangle rec)
            {
                //get the data context as the custom class
                githubRepository gitRepo = rec.DataContext as githubRepository;

                //only if the repo is favorited, you should change it
                if (gitRepo.isStarred)
                {
                    if (rec.OpacityMask is ImageBrush imageBrush)
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
            if (sender is Button button)
            {
                //get the custom class from button
                githubRepository gitRepo = button.DataContext as githubRepository;


                if (button.Content is StackPanel panel)
                {
                    //get all of the inner button content
                    foreach (var child in panel.Children)
                    {
                        //locate rectange
                        if (child is Rectangle rectangle && rectangle.Name == "favoriteRectangle")
                        {
                            //get the image brush
                            if (rectangle.OpacityMask is ImageBrush imageBrush)
                            {
                                if (gitRepo.isStarred)
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
            foreach (githubRepository repo in githubRepositories)
            {
                if (repo.isStarred)
                {
                    tempList.Add(repo);
                }
            }

            //then add in all of the non stared repos
            foreach (githubRepository repo in githubRepositories)
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
        private async void deleteSelectedRepo(object sender, RoutedEventArgs e)                 //Perma Delete a repo 
        {
            //check to see if an item is selected
            if (githubListView.SelectedItem != null)
            {
                //make sure to ask user if they really want to delete a given item
                var result = MessageBox.Show(
                    "Are you sure you want to delete this repo? Deletion is permanent.",
                    "Delete Repo?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                    );


                if (result == MessageBoxResult.Yes)
                {
                    //if they respond yes, then pop the repo and save
                    githubRepositories.Remove(githubListView.SelectedItem as githubRepository);
                    await githubJsonManagement.saveToJson(githubRepositories);
                }
                else
                {
                    //do nothing
                    return;
                }
            }
        }
        private async void saveGivenRepo(object sender, RoutedEventArgs e)                      //context menu button save
        {
            //save when context menu button click
            await githubJsonManagement.saveToJson(githubRepositories);
        }
        private void ViewFullInfoClick(object sender, RoutedEventArgs e)                        //Open when clicked in side menu
        {
            doSeeInfoAnimation(sender, e);
        }
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
            foreach (string tag in tags)
            {
                //create a new button with all of the different styles it needs to have
                Button button = new Button
                {
                    Content = tag,
                    Margin = new Thickness(5),
                    Style = (Style)this.FindResource("EvenLessRoundedButton"),
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush,
                    BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(10, 5, 10, 5),
                    BorderThickness = new Thickness(2),
                    MinHeight = 40,
                    MinWidth = 40

                };

                //append the click handle function and then add the button to the wrap panel
                button.Click += tagButtonClick;
                TagsPanel.Children.Add(button);
            }

            //also then do the same for the extra info GUI panel
            foreach (string tag in tags)
            {
                //create a new button with all of the different styles it needs to have
                Button button = new Button
                {
                    Content = tag,
                    Margin = new Thickness(5),
                    Style = (Style)this.FindResource("EvenLessRoundedButton"),
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush,
                    BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(10, 5, 10, 5),
                    BorderThickness = new Thickness(2),
                    MinHeight = 40,
                    MinWidth = 40

                };

                //append the click handle function and then add the button to the wrap panel
                button.Click += changeTagButton;
                SItagsPanel.Children.Add(button);
            }
        }
        private void tagButtonClick(object sender, RoutedEventArgs e)                           //Add the tag to the current button
        {
            //check if button
            if (sender is Button button)
            {
                //get button foreground as brush
                if (button.Foreground is System.Windows.Media.SolidColorBrush solidColorBrush)
                {
                    //check to see if button is gray
                    if (solidColorBrush.Color == System.Windows.Media.Colors.DarkGray)
                    {
                        //if so, then make it blue again
                        button.Foreground = new SolidColorBrush(Colors.White);
                        button.BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush;
                        button.Background = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush;

                        //also remove the tag from the current tag list
                        tagsToAdd.Remove(button.Content.ToString());
                    }
                    else if (solidColorBrush.Color == System.Windows.Media.Colors.White)
                    {

                        //if not, then make it gray because it's blue
                        button.Foreground = new SolidColorBrush(Colors.DarkGray);
                        button.Background = new SolidColorBrush(Colors.LightGray);
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
            ANLink.Text = string.Empty;
            ANName.Text = string.Empty;

            //Reset the selection for the radio buttons
            YesRB.IsChecked = true;

            //clear all of the possible selections for the tags
            foreach (UIElement element in TagsPanel.Children)
            {
                if (element is Button button)
                {
                    //change back to enabled color scheme
                    button.Foreground = new SolidColorBrush(Colors.White);
                    button.BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush;
                    button.Background = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush;
                }
            }
        }
        private async void addInNewRepo(object sender, RoutedEventArgs e)                       //Get new repo content and save it
        {
            //check to see if all required fields are picked out
            if (!string.IsNullOrEmpty(ANLink.Text) || !string.IsNullOrEmpty(ANName.Text))
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
                    gitRepo.name = ANName.Text;
                    gitRepo.status = "NonStarted";

                    //check to see if user wants to get the metaData from the repo
                    if (YesRB.IsChecked == true)
                    {
                        //call the get metadata function
                        await gitRepo.getMetaData(gitRepo.metaData.ownerName, gitRepo.metaData.repoName);
                    }
                    //see if user added in collection
                    if (!string.IsNullOrEmpty(ANCollection.Text))
                    {
                        gitRepo.collection = ANCollection.Text;
                        if(!collectionList.Contains(ANCollection.Text))
                        {
                            //if not in list, add and refresh combo boxes
                            collectionList.Add(ANCollection.Text);
                            AddCB.ItemsSource = collectionList;
                            CollectionsComboBox.ItemsSource = collectionList;
                        }
                    }
                    //update the tags of the github repo and then clear tagsToAdd
                    foreach (string tag in tagsToAdd)
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
        private void CollectionAdd(object sender, SelectionChangedEventArgs e)                  //Update collection visual
        {
           //update visual to select collection
            ANCollection.Text = AddCB.SelectedItem.ToString();
        }

        //-----------------------The Extra Information GUI------------------------------
        private void doSeeInfoAnimation(object sender, RoutedEventArgs e)                        //handle opening and closing the new repo info
        {
            if (sender is Button button)
            {
                Storyboard sb = new Storyboard();

                if(button.Name != "SISave" && button.Name != "closeSeeInfoGUI")
                {
                    if (githubListView.SelectedItem == null)
                    {
                        //tell user they have to select an item
                        MessageBox.Show(
                            "Please select a github repository before trying to see extra information",
                            "Please select a repo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation
                            );

                        return; //return so it doesnt open
                    }
                    else
                    {
                        //run the fill out information function
                        populateInformation(sender, e);
                    }
                }

                //The open sequence
                if (button.Name == "InfoRepoButton")
                {
                    //Set the transform origin on the Border itself
                    SeeInfoGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    Panel.SetZIndex(TranslucentBox, 10);
                    Panel.SetZIndex(SeeInfoGUI, 11);

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

                    Storyboard.SetTarget(moveDOWN, SeeInfoGUI);
                    Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                    Storyboard.SetTarget(opacityAdd, TranslucentBox);
                    Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                    sb.Children.Add(moveDOWN);
                    sb.Children.Add(opacityAdd);
                    sb.Begin();
                }
                //the close sequence
                else if (button.Name == "closeSeeInfoGUI" || button.Name == "SISave")
                {

                    //disable all items
                    enableAll(sender, e);

                    //Set the transform origin on the Border itself
                    SeeInfoGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

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

                    Storyboard.SetTarget(moveDOWN, SeeInfoGUI);
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
        private void doSeeInfoForContext(object sender, RoutedEventArgs e)                       //opening info gui but for context menu
        {
            if (githubListView.SelectedItem == null)
            {
                //tell user they have to select an item
                MessageBox.Show(
                    "Please select a github repository before trying to see extra information",
                    "Please select a repo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                    );

                return; //return so it doesnt open
            }
            else
            {
                //run the fill out information function
                populateInformation(sender, e);
            }

            if(sender is MenuItem menuItem)
            {
                if(menuItem.Name == "EditItem")
                {
                    enableAll(sender, e);
                }
                Storyboard sb = new Storyboard();

                //Set the transform origin on the Border itself
                SeeInfoGUI.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                Panel.SetZIndex(TranslucentBox, 10);
                Panel.SetZIndex(SeeInfoGUI, 11);

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

                Storyboard.SetTarget(moveDOWN, SeeInfoGUI);
                Storyboard.SetTargetProperty(moveDOWN, new PropertyPath("RenderTransform.ScaleY"));
                Storyboard.SetTarget(opacityAdd, TranslucentBox);
                Storyboard.SetTargetProperty(opacityAdd, new PropertyPath("Opacity"));

                sb.Children.Add(moveDOWN);
                sb.Children.Add(opacityAdd);
                sb.Begin();
            }

        }
        private void populateInformation(object sender, RoutedEventArgs e)                       //fill in all of the info
        {
            //get the repo as an item
            githubRepository repo = githubListView.SelectedItem as githubRepository;

            //set all of the given texts outright
            SIMainTitle.Text    = $"Showing information for: {repo.name}";
            SIPersonalName.Text = repo.name;
            SIUrl.Text          = repo.url;
            SICollection.Text   = repo.collection;

            //set up the metaData
            githubMetaData data = repo.metaData;
            SIid.Text           = data.id;
            SIOwner.Text        = data.ownerName;
            SIRepoName.Text     = data.repoName;
            SIFullRepoName.Text = data.repoName;
            SIDescription.Text  = data.Description;
            SICloneUrl.Text     = data.cloneURL;
            SIReadme.Text       = data.readMeContent;
            SIBranch.Text       = data.defaultBranch;

            //set up the options for the status
            if      (repo.status == "NonStarted") { NonStartedRB.IsChecked = true; }
            else if (repo.status == "Started")    { NonStartedRB.IsChecked = true; }
            else if (repo.status == "Completed")  { NonStartedRB.IsChecked = true; }

            //go through each of the tags and highlight the ones that are selected
            foreach (UIElement child in SItagsPanel.Children)
            {
                //get button
                if (child is Button button)
                {
                    //see if button content is in repo's tags
                    if (repo.tags.Contains(button.Content.ToString()) && button.Foreground is System.Windows.Media.SolidColorBrush solidColorBrush)
                    {
                        button.Foreground = new SolidColorBrush(Colors.DarkGray);
                        button.Background = new SolidColorBrush(Colors.LightGray);
                        button.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                    }
                }
            }

        }
        private void changeTagButton(object sender, RoutedEventArgs e)                           //Just change the color for initial
        {
            //check if button
            if (sender is Button button)
            {
                //get button foreground as brush
                if (button.Foreground is System.Windows.Media.SolidColorBrush solidColorBrush)
                {
                    //check to see if button is gray
                    if (solidColorBrush.Color == System.Windows.Media.Colors.DarkGray)
                    {
                        //if so, then make it blue again
                        button.Foreground = new SolidColorBrush(Colors.White);
                        button.BorderBrush = System.Windows.Application.Current.Resources["DarkBorderBrushKey"] as Brush;
                        button.Background = System.Windows.Application.Current.Resources["MainBorderBrushKey"] as Brush;

                        //Remove the tag from the current repo's list
                        githubRepository repo = githubListView.SelectedItem as githubRepository;
                        repo.tags.Remove(button.Content.ToString());
                    }
                    else if (solidColorBrush.Color == System.Windows.Media.Colors.White)
                    {

                        //if not, then make it gray because it's blue
                        button.Foreground = new SolidColorBrush(Colors.DarkGray);
                        button.Background = new SolidColorBrush(Colors.LightGray);
                        button.BorderBrush = new SolidColorBrush(Colors.DarkGray);

                        //add a given tag to a given tag list
                        githubRepository repo = githubListView.SelectedItem as githubRepository;
                        repo.tags.Add(button.Content.ToString());
                    }
                }
            }
        }
        private async void saveChangesMade(object sender, RoutedEventArgs e)                     //The save git repo function
        {
            if(githubListView.SelectedItem != null)
            {
                //get the repo
                githubRepository repo = githubListView.SelectedItem as githubRepository;
                githubMetaData data = repo.metaData;

                //save all of the text into the repo
                SIMainTitle.Text    = $"Showing information for: {repo.name}";
                repo.name           = SIPersonalName.Text;
                repo.url            = SIUrl.Text;
                repo.collection     = SICollection.Text;
                data.id             = SIid.Text;
                data.ownerName      = SIOwner.Text;
                data.repoName       = SIRepoName.Text;
                data.repoName       = SIFullRepoName.Text;
                data.Description    = SIDescription.Text;
                data.cloneURL       = SICloneUrl.Text;
                data.readMeContent  = SIReadme.Text;
                data.defaultBranch  = SIBranch.Text;

                //Also save any changes in the radio buttons
                if      (NonStartedRB.IsChecked == true) { repo.status = "NonStarted";}
                else if (NonStartedRB.IsChecked == true) { repo.status = "Started";   }
                else if (NonStartedRB.IsChecked == true) { repo.status = "Completed"; }

                //the changing for the tags is already done

                //save to json
                await githubJsonManagement.saveToJson(githubRepositories);

                //refresh the main list view to reflect any changes
                githubListView.ItemsSource = null;
                githubListView.ItemsSource = githubRepositories;

                //then close the window
                doSeeInfoAnimation(sender, e);
            }
        }
        private void undoAllChanges(object sender, RoutedEventArgs e)                            //reload the info window
        {
            //ask the user if they are sure they want to reload the repo
            var result = MessageBox.Show(
                "Any changes made will be undone and not saved.",
                "Are you sure",
                MessageBoxButton.YesNo,
                MessageBoxImage.Hand
                );

            if(result == MessageBoxResult.Yes)
            {
                //lowkey just reload the information
                populateInformation(sender, e);
            }
            else
            {
                //do nothing
                return;
            }
        }
        private void enableAll(object sender, RoutedEventArgs e)                                 //Make all fields editable
        {
            if (SIPersonalName.IsEnabled == false)
            { 
                //set all text boxes as enabled, including radio buttons and wrap panel
                SIMainTitle.IsEnabled     = true;
                SIPersonalName.IsEnabled  = true;
                SIUrl.IsEnabled           = true;
                SICollection.IsEnabled    = true;
                SIid.IsEnabled            = true;
                SIOwner.IsEnabled         = true;
                SIRepoName.IsEnabled      = true;
                SIFullRepoName.IsEnabled  = true;
                SIDescription.IsEnabled   = true;
                SICloneUrl.IsEnabled      = true;
                SIReadme.IsEnabled        = true;
                SIBranch.IsEnabled        = true;
                SItagsPanel.IsEnabled     = true;
                StartedRB.IsEnabled       = true;
                NonStartedRB.IsEnabled    = true;
                CompletedRB.IsEnabled     = true;

            }
            else if (SIPersonalName.IsEnabled == true)
            {
                //set all text boxes as disabled, including radio buttons and wrap panel
                SIMainTitle.IsEnabled     = false;
                SIPersonalName.IsEnabled  = false;
                SIUrl.IsEnabled           = false;
                SICollection.IsEnabled    = false;
                SIid.IsEnabled            = false;
                SIOwner.IsEnabled         = false;
                SIRepoName.IsEnabled      = false;
                SIFullRepoName.IsEnabled  = false;
                SIDescription.IsEnabled   = false;
                SICloneUrl.IsEnabled      = false;
                SIReadme.IsEnabled        = false;
                SIBranch.IsEnabled        = false;
                SItagsPanel.IsEnabled     = false;
                StartedRB.IsEnabled       = false;
                NonStartedRB.IsEnabled    = false;
                CompletedRB.IsEnabled     = false;
            }
        }

        //-----------------------Some Search Button Stuff--------------------------------
        private void CollectionSelectionChanged(object sender, SelectionChangedEventArgs e)      //Search by collection
        {
            if(CollectionsComboBox.SelectedItem != null)
            {
                //change the text for the search bar to reflect the selected item
                var selectedItem = CollectionsComboBox.SelectedItem.ToString();
                SearchTB.Text = selectedItem;

                //clear the actual combo box & collections list
                collectionRepositories.Clear();

                //loop over repos to see if it is the right collection
                foreach (var repo in githubRepositories)
                {
                    //if it is right then add it to collection
                    if (repo.collection == SearchTB.Text.ToString())
                    {
                        collectionRepositories.Add(repo);
                    }
                }

                //refresh to showcase the collection repos
                githubListView.ItemsSource = null;
                githubListView.ItemsSource = collectionRepositories;
            }
        }
        private void searchByStatusClick(object sender, RoutedEventArgs e)                       //change repos based on their status
        {

            //clear the status repositories and search
            statusRepositories.Clear();
            CollectionsComboBox.SelectedItem = null;
            SearchTB.Text = string.Empty;

            if(sender is Button Button)
            {
                //cycle through to change the names
                if(StatusTextBlock.Text == "Search By Status") { StatusTextBlock.Text = "NonStarted"; }
                else if(StatusTextBlock.Text == "NonStarted")  { StatusTextBlock.Text = "Started";    }
                else if(StatusTextBlock.Text == "Started")     { StatusTextBlock.Text = "Completed";  }
                else if(StatusTextBlock.Text == "Completed")   { StatusTextBlock.Text = "Search By Status";  }
            }

            //check to see reset happens
            if(StatusTextBlock.Text == "Search By Status")
            {
                //set main repos as given
                githubListView.ItemsSource = null;
                githubListView.ItemsSource = githubRepositories;
            }
            else
            {
                foreach(var repo in githubRepositories)
                {
                    //check if the name is the same then add
                    if(StatusTextBlock.Text == repo.status)
                    {
                        statusRepositories.Add(repo);
                    }
                }

                //set as visible list view
                githubListView.ItemsSource = null;
                githubListView.ItemsSource = statusRepositories;
            }
        }

    }
}
