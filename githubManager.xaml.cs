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
        public string         url         { get; set; }
        public string         name        { get; set; }
        public string[]       tags        { get; set; }
        public string         collection  { get; set; }
        public string         status      { get; set; }
        public bool           isStarred   { get; set; } = false;
        public githubMetaData metaData    { get; set; }


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
        }

        //The Button to go back
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //return back to the main page
            this.NavigationService.GoBack();
        }

        //--------------------------The Side Button Functions-------------------------
    }
}
