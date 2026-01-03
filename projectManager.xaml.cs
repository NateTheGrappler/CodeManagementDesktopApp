using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class projectDirectory
    {
        //--------------------------Varaibles---------------------------
        public string folderName  { get; set; }
        public string folderPath  { get; set; }
        public DateTime addedDate { get; set; }
        public ImageSource icon   { get; set; }
        ObservableCollection<projectFile> innerFiles            { get; set; } = new ObservableCollection<projectFile>();
        ObservableCollection<projectDirectory> innerDirectories { get; set; } = new ObservableCollection<projectDirectory>();


        //-------------------------Constructors---------------------------
        public projectDirectory() { }
    }


    public class projectFile
    {
        //--------------------------Varaibles---------------------------
        public string fileName    { get; set; }
        public string filePath    { get; set; }
        public string fileType    { get; set; }
        public string fileSize    { get; set; }
        public DateTime addedDate { get; set; }




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
        public projectManager()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
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
                else if(button.Name == "closeAddGUI")
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
    }
}
