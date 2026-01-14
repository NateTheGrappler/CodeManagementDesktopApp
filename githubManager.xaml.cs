using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

    public class githubMetaData
    {

        //-----------------------------varaibles-----------------------
        public string id { get; set; }
        public string ownerName { get; set; }
        public string repoName { get; set; }
        public string Description { get; set; }
        public string cloneURL { get; set; }
        //see if possible to get things like languages, size, readme content

        //-----------------------------constructor-----------------------
        public githubMetaData() { }
    }

    public class githubRepository
    {
        //-----------------------------Varaibles-------------------------
        public string url { get; set; }
        public string name { get; set; }
        public string[] tags { get; set; }
        public string collection { get; set; }
        public string status { get; set; }
        public bool isStarred { get; set; } = false;
        public githubMetaData metaData { get; set; }


        //-----------------------------Functions-------------------------

        //-----------------------------constructor-----------------------
        public githubRepository(string url)
        {
        }
    }


    public partial class githubManager : Page
    {
        public githubManager()
        {
            InitializeComponent();
            loadTags();
        }

        //The Button to go back
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //return back to the main page
            this.NavigationService.GoBack();
        }

        //--------------------------The Side Button Functions-------------------------

        //-------------------------The Add New Repo GUI-------------------------------
        private void doAddNewAnimation(object sender, RoutedEventArgs e)                                    //handle opening and closing the new repo info
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
        private void loadTags()                                             //Set up visible tags in the wrappanel
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
                    Content = tag,
                    Margin = new Thickness(5),
                    Style = (Style)this.FindResource("EvenLessRoundedButton"),
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = Application.Current.Resources["MainBorderBrushKey"] as Brush,
                    BorderBrush = Application.Current.Resources["DarkBorderBrushKey"] as Brush,
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
        }
        private void tagButtonClick(object sender, RoutedEventArgs e)       //Add the tag to the current button
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
                        button.Foreground = new SolidColorBrush(Colors.White);
                        button.BorderBrush = Application.Current.Resources["DarkBorderBrushKey"] as Brush;
                        button.Background = Application.Current.Resources["MainBorderBrushKey"] as Brush;
                    }
                    else if (solidColorBrush.Color == System.Windows.Media.Colors.White)
                    {
                        Debug.WriteLine("Do i just so happen to be white?");

                        //if not, then make it gray because it's blue
                        button.Foreground = new SolidColorBrush(Colors.DarkGray);
                        button.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                        button.Background = new SolidColorBrush(Colors.LightGray);
                    }
                }
            }
        }
        private void clearAllFields(object sender, RoutedEventArgs e)
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
                    button.BorderBrush = Application.Current.Resources["DarkBorderBrushKey"] as Brush;
                    button.Background  = Application.Current.Resources["MainBorderBrushKey"] as Brush;
                }
            }
        }
    }
}
