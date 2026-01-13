using System;
using System.Collections.Generic;
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

        }

    }
}
