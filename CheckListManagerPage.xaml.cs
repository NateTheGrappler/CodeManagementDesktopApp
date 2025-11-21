using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Xml.Linq;
using System.Xml.Serialization;


namespace CodeManagementSystem
{
    /// <summary>
    /// Interaction logic for CheckListManagerPage.xaml
    /// </summary>
    /// 

    public class projectJsonSaver
    {
        private readonly string _dataPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\JsonFolder", "projectChecklists.json"
        );

        public async Task SaveProjectChecklistAsync(ObservableCollection<checkListProject> projects)
        {

            Debug.WriteLine($"Saving data to: {_dataPath}");

            string? directory = System.IO.Path.GetDirectoryName(_dataPath);
            if (directory == null)
            {
                throw new InvalidOperationException("Directory path could not be determined from _dataPath.");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create a serializable list to avoid ObservableCollection issues
            var serializableList = projects.ToList();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Ensure consistent naming
            };

            string json = JsonSerializer.Serialize(serializableList, options);
            await File.WriteAllTextAsync(_dataPath, json);
            Debug.WriteLine("Data saved successfully");
        }

        public async Task<ObservableCollection<checkListProject>> LoadProjectChecklistAsync()
        {

            Debug.WriteLine($"Loading data from: {_dataPath}");

            if (!File.Exists(_dataPath))
            {
                Debug.WriteLine("File does not exist, returning empty collection");
                return new ObservableCollection<checkListProject>();
            }

            string json = await File.ReadAllTextAsync(_dataPath);
            Debug.WriteLine($"JSON content length: {json.Length}");

            // Deserialize as List and convert to ObservableCollection
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var projectsList = JsonSerializer.Deserialize<List<checkListProject>>(json, options);
            var projects = projectsList != null
                ? new ObservableCollection<checkListProject>(projectsList)
                : new ObservableCollection<checkListProject>();

            Debug.WriteLine($"Loaded {projects.Count} projects");
            return projects;
        }
    }


    //This is the class thats going to go inside of the list of each one of the checklistPoject classes
    public class checkListItem
    {
        //The describiption is the name of the thing you want to check, and the bool is to see if its done or not, held in nested listview box
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("isChecked")]
        public bool isChecked { get; set; }

        [JsonPropertyName("id")]
        public int id { get; set; }

        //Constructor with no params
        public checkListItem() { }

        //Direct constructor
        public checkListItem(string description, bool isCompleted = false)
        {
            Description = description;
            isChecked = isCompleted;
        }
    }

    //This is the class project thats going to actually hold checklist things, as well as the percent completion, and is held in the main list view box
    public class checkListProject
    {
        //Has the project name, total completion for the whole project, and all the checkitems in the ObservableCollection
        [JsonPropertyName("projectName")]
        public string projectName { get; set; } = string.Empty;

        [JsonIgnore]
        public float percentCompleteion => calculatePercentage();

        [JsonPropertyName("items")]
        public ObservableCollection<checkListItem> items { get; set; } = new ObservableCollection<checkListItem>();

        //Add parameterless constructor
        public checkListProject() { }

        //Math to get the total completion percent
        public float calculatePercentage()
        {
            if (items.Count == 0) { return 0; }

            int completed = items.Count(item => item.isChecked);
            return (float)completed / items.Count * 100;
        }

        //This adds in a new checklist item
        public void addNewItem(string description)
        {
            items.Add(new checkListItem(description, false));
        }

        public void removeItem(checkListItem item)
        {
            items.Remove(item);
        }

        public checkListItem GetCheckListItem(string description)
        {
            foreach(checkListItem item in items)
            {
                if(item.Description == description)
                {
                    return item;
                }
            }
            return new checkListItem("Couldnt Find Item", false);
        }
    }



    public partial class CheckListManagerPage : Page
    {
        public ObservableCollection<checkListProject>  mainTextBoxList = new ObservableCollection<checkListProject>();
        private readonly projectJsonSaver              _service = new projectJsonSaver();
        private checkListProject?                       currentProject;
        private checkListProject?                       currentNewProject;
        private TextBox?                                currentEditTextbox;
        private string?                                 ogName;
        private string?                                 currentNewProjectOgName;
        private checkListProject                        currentOpenCheckListProject;

        public CheckListManagerPage()
        {
            InitializeComponent();
            LoadDataAsync().ConfigureAwait(false);
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveDataAsync();
           this.NavigationService.GoBack();
        }

        //public void initializeListBox()
        //{
        //    listBoxContainer.ItemsSource = mainTextBoxList;
        //}

        private void enterCheckListButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Pressed The Black Button");
            animateMainContentIN(sender, e);
        }

        private void animateMainContentIN(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            checkListProject? checkListProject = clickedButton?.DataContext as checkListProject;
            if (checkListProject != null)
            {
                currentOpenCheckListProject = checkListProject;
                titleText.Text = checkListProject.projectName;
                Debug.WriteLine(currentOpenCheckListProject.projectName);
            }

            var slidingRectangle = mainContentSlideIn;

            translucentBox.Visibility = Visibility.Visible;
            Panel.SetZIndex(translucentBox, 20);
            Panel.SetZIndex(slidingRectangle, 20);

            var storyBoard = new Storyboard();

            if (slidingRectangle.RenderTransform is not TranslateTransform)
            {
                Debug.Write("Ran the TranslateTransform");
                slidingRectangle.RenderTransform = new TranslateTransform(-550, -10);
            }

            DoubleAnimation translateXAnimation = new DoubleAnimation
            {
                From = -550,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 0.90,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };


            // Set the target object and property for the animations
            Storyboard.SetTarget(translateXAnimation, slidingRectangle);
            Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(RenderTransform).(TranslateTransform.X)"));

            Storyboard.SetTarget(opacityAnimation, translucentBox);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyBoard.Children.Add(translateXAnimation);
            storyBoard.Children.Add(opacityAnimation);

            storyBoard.Begin();
        }
        private async Task LoadDataAsync()
        {
            Debug.WriteLine("Loading data...");
            mainTextBoxList = await _service.LoadProjectChecklistAsync();
            Debug.WriteLine($"Loaded {mainTextBoxList.Count} projects");

            // Use Dispatcher to update UI on the correct thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                listBoxContainer.ItemsSource = mainTextBoxList;
                mainTextBoxList.CollectionChanged += changedItemCollection;
            });
        }

        private async void changedItemCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            await SaveDataAsync();
        }

        private async Task SaveDataAsync()
        {
            await _service.SaveProjectChecklistAsync(mainTextBoxList); // Fixed typo
        }

        private async void save_Click(object sender, RoutedEventArgs e)
        {
            await SaveDataAsync();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var project = button?.DataContext as checkListProject;
            if (project != null)
            {
                mainTextBoxList.Remove(project);
            }
            await SaveDataAsync();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Clicked on Rename checkListProject Button");
            var button = sender as Button;
            var project = button?.DataContext as checkListProject;
            currentProject = project;

            var listViewItem = FindVisualParent<ListBoxItem>(button);
            if (listViewItem != null)
            {
                Debug.WriteLine("Listview item is: " + listViewItem.Name);
                var textBox = FindVisualChild<TextBox>(listViewItem);
                Debug.WriteLine("Textbox item is: " + textBox.Name);
                if (textBox != null)
                {
                    Debug.Write("textbox is not null");
                    currentEditTextbox = textBox;
                    ogName = textBox.Text;
                    textBox.IsEnabled = true;
                    textBox.SelectAll();
                    textBox.Focus();
                }
            }
        }

        private void nameTextBoxList_LostFocus(object sender, RoutedEventArgs e)
        {
            currentEditTextbox.IsEnabled = false;
            if (string.IsNullOrWhiteSpace(currentEditTextbox.Text))
            {
                currentEditTextbox.Text = ogName;
            }
            currentProject.projectName = currentEditTextbox.Text;
            currentEditTextbox = null;
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            await SaveDataAsync();
        }

        private void New_Project_Button_Click(object sender, RoutedEventArgs e)
        {
            var project = new checkListProject { projectName = "Untitled" };;
            currentNewProject = project;
            newProjectListBox.ItemsSource = project.items;

            var slidingRectangle = SlidingRectangleMain;
            var translucentRectangle = translucentBox;

            translucentBox.Visibility = Visibility.Visible;
            Panel.SetZIndex(translucentBox, 20);
            Panel.SetZIndex(slidingRectangle, 20);

            Storyboard storyBoard = new Storyboard();

            //slidingRectangle.RenderTransformOrigin = new Point(0.5, 0.5);

            // Ensure the button has a TranslateTransform
            if (slidingRectangle.RenderTransform is not TranslateTransform)
            {
                Debug.Write("Ran the TranslateTransform");
                slidingRectangle.RenderTransform = new TranslateTransform(0, 300);
            }

            DoubleAnimation translateXAnimation = new DoubleAnimation
            {
                From = 300,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 0.90,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };


            // Set the target object and property for the animations
            Storyboard.SetTarget(translateXAnimation, slidingRectangle);
            Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(RenderTransform).(TranslateTransform.Y)"));

            Storyboard.SetTarget(opacityAnimation, translucentBox);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyBoard.Children.Add(translateXAnimation);
            storyBoard.Children.Add(opacityAnimation);

            storyBoard.Begin();
        }

        private void exitButtonNewNote_Click(object sender, RoutedEventArgs e)
        {
            currentNewProject = null;
            undoNewNoteAnimation();
        }

        private void undoNewNoteAnimation()
        {
            var slidingRectangle = SlidingRectangleMain;
            var translucentRectangle = translucentBox;

            Storyboard storyBoard = new Storyboard();

            //slidingRectangle.RenderTransformOrigin = new Point(0.5, 0.5);

            // Ensure the button has a TranslateTransform
            if (slidingRectangle.RenderTransform is not TranslateTransform)
            {
                Debug.Write("Ran the TranslateTransform");
                slidingRectangle.RenderTransform = new TranslateTransform(0, 300);
            }

            DoubleAnimation translateXAnimation = new DoubleAnimation
            {
                From = 0,
                To = 300,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0.90,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };


            // Set the target object and property for the animations
            Storyboard.SetTarget(translateXAnimation, slidingRectangle);
            Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(RenderTransform).(TranslateTransform.Y)"));

            Storyboard.SetTarget(opacityAnimation, translucentBox);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyBoard.Children.Add(translateXAnimation);
            storyBoard.Children.Add(opacityAnimation);

            storyBoard.Begin();

            //translucentBox.Visibility = Visibility.Hidden;
            Panel.SetZIndex(translucentBox, 0);
            Panel.SetZIndex(slidingRectangle, 0);
        }

        //Delete the checklist item that is currently highlighted in the new project
        private void deleteCheckListItemButton_Click(object sender, RoutedEventArgs e)
        {
            if(newProjectListBox.SelectedItem != null && currentNewProject != null)
            {
                Debug.WriteLine("Removing a new checkList item...");
                currentNewProject.removeItem(newProjectListBox.SelectedItem as checkListItem);
            }
        }

        private void newCheckListItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentNewProject != null)
            {
                Debug.WriteLine("Adding in a new checkList item...");
                currentNewProject.addNewItem("Write Descripition Here");
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkList = sender as CheckBox;
            checkListItem item = checkList.DataContext as checkListItem;
            if (item != null) 
            {
                item.isChecked = !item.isChecked;
            }
            //currentNewProject.GetCheckListItem
        }

        private void newCheckName_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if(textBox != null)
            {
                currentNewProjectOgName = textBox.Text;
                textBox.SelectAll();
                textBox.Focus();
                textBox.Text = string.Empty;
            }
        }

        private void newCheckName_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            checkListItem item = textBox.DataContext as checkListItem;
            if(item != null && !string.IsNullOrEmpty(textBox.Text))
            {
                item.Description = textBox.Text;
            }
            else if(string.IsNullOrEmpty(textBox.Text) && currentNewProjectOgName != null)
            {
                textBox.Text = currentNewProjectOgName;
            }
            currentNewProjectOgName = null;
        }

        private void createButtonNewNote_Click(object sender, RoutedEventArgs e)
        {
            if(currentNewProject != null && !string.IsNullOrEmpty(currentNewProject.projectName))
            {
                mainTextBoxList.Add(currentNewProject);
                currentNewProject = null;
                currentNewProjectOgName = null;
                currentNewProjectName.Text = "Untitled...";
                currentNewProjectName.FontWeight = FontWeights.Normal;
                currentNewProjectName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF808080"));
                undoNewNoteAnimation();
            }
        }

        private void clearButtonNewNote_Click(object sender, RoutedEventArgs e)
        {
            if(currentNewProject != null)
            {
                currentNewProject.items.Clear();
            }
        }

        private void renameButtonNewProject_Click(object sender, RoutedEventArgs e)
        {
            currentNewProjectName.Focus();
            currentNewProjectName.SelectAll();
            currentNewProjectName.FontWeight = FontWeights.Bold;
            currentNewProjectName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
        }

        private void currentNewProjectName_GotFocus(object sender, RoutedEventArgs e)
        {
            currentNewProjectName.Focus();
            currentNewProjectName.SelectAll();
            currentNewProjectOgName = currentNewProjectName.Text;
            currentNewProjectName.Text = string.Empty;
        }

        private void currentNewProjectName_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(currentNewProjectName.Text) && currentNewProject != null)
            {
                currentNewProjectName.FontWeight = FontWeights.Bold;
                currentNewProjectName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
                currentNewProject.projectName = currentNewProjectName.Text;
            }
            else if(string.IsNullOrEmpty(currentNewProjectName.Text) && currentNewProjectOgName != null)
            {
                currentNewProjectName.Text = currentNewProjectOgName;
                currentNewProjectName.FontWeight = FontWeights.Normal;
                currentNewProjectName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF808080"));
            }
            currentNewProjectOgName = null;

        }

        private void ListBox_MouseEnter(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Entered In ListBox");
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                listBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            }
        }

        private void exitButtonMainContent_Click(object sender, RoutedEventArgs e)
        {
            var slidingRectangle = mainContentSlideIn;
            var translucentRectangle = translucentBox;

            Storyboard storyBoard = new Storyboard();

            // Ensure the button has a TranslateTransform
            if (slidingRectangle.RenderTransform is not TranslateTransform)
            {
                Debug.Write("Ran the TranslateTransform");
                slidingRectangle.RenderTransform = new TranslateTransform(0, 0);
            }

            DoubleAnimation translateXAnimation = new DoubleAnimation
            {
                From = 0,
                To = -550,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0.90,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false,
            };


            // Set the target object and property for the animations
            Storyboard.SetTarget(translateXAnimation, slidingRectangle);
            Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(RenderTransform).(TranslateTransform.X)"));

            Storyboard.SetTarget(opacityAnimation, translucentBox);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyBoard.Children.Add(translateXAnimation);
            storyBoard.Children.Add(opacityAnimation);

            storyBoard.Begin();

            //translucentBox.Visibility = Visibility.Hidden;
            Panel.SetZIndex(translucentBox, 0);
            Panel.SetZIndex(slidingRectangle, 0);
        }
    }
}
