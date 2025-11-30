using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using AngleSharp.Dom;

namespace CodeManagementSystem
{
    /// <summary>
    /// Interaction logic for videoManager.xaml
    /// </summary>
    public partial class videoManager : Page
    {
        public videoManager()
        {
            InitializeComponent();
        }

        //This function is for main backbutton thats in the title to go back to the neural network
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        //This function determines which of the listBoxViews to show based on the name of the button clicked
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String tabName = "";

            //get tab that is selected and it's name
            if (e.Source is TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    tabName = selectedTab.Name; //assign name of tab for later use
                }
            }

            //Go through all of the checks to see which of the tabs to display
            if (tabName == "VideoTab")
            {
                Debug.WriteLine("VideoTab");
                Panel.SetZIndex(VideoListBox,   20);
                Panel.SetZIndex(PlaylistListBox, 0);
                Panel.SetZIndex(ShortsListBox,   0);
                Panel.SetZIndex(OtherListBox,    0);

            }
            else if(tabName == "PlaylistTab")
            {
                Debug.WriteLine("PlaylistTab");
                Panel.SetZIndex(VideoListBox,     0);
                Panel.SetZIndex(PlaylistListBox, 20);
                Panel.SetZIndex(ShortsListBox,    0);
                Panel.SetZIndex(OtherListBox,     0);
            }
            else if(tabName == "ShortsTab")
            {
                Debug.WriteLine("ShortsTab");
                Panel.SetZIndex(VideoListBox,    0);
                Panel.SetZIndex(PlaylistListBox, 0);
                Panel.SetZIndex(ShortsListBox,  20);
                Panel.SetZIndex(OtherListBox,    0);
            }
            else if(tabName == "OtherTab")
            {
                Debug.WriteLine("OtherTab");
                Panel.SetZIndex(VideoListBox,    0);
                Panel.SetZIndex(PlaylistListBox, 0);
                Panel.SetZIndex(ShortsListBox,   0);
                Panel.SetZIndex(OtherListBox,   20);
            }
            else
            {
                Debug.WriteLine("ERROR");
            }
        }
    }
}
