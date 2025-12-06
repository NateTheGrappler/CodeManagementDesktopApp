using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
using AngleSharp.Dom;
using FFMpegCore;
using Microsoft.Win32;
//The extra imports needed for this:
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;



namespace CodeManagementSystem
{
    /// <summary>
    /// Interaction logic for videoManager.xaml
    /// </summary>
    /// 

    //Json manager for this part of the project
    public class VideoJsonSaver
    {
        //-----------------------------------All of the filepaths---------------------------------
        //Set up all of the filepaths in the Roaming app folder for each of the json files
        private readonly string _dataPathVideos = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\JsonFolder", "RegularVideos.json"
        );
        private readonly string _dataPathPlaylists = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\JsonFolder", "Playlist.json"
        );
        private readonly string _dataPathShorts = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\JsonFolder", "Shorts.json"
        );
        private readonly string _dataPathOther = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\JsonFolder", "OtherVideos.json"
        );

        //--------------------------------------Save Methods--------------------------------------

        //regular video save method
        public async Task SaveRegularVideosAsync(ObservableCollection<RegularVideo> videos)
        {
            await SaveToFileAsync(_dataPathVideos, videos);
        }
        public async Task SavePlaylistsAsync(ObservableCollection<PlayList> playlists)
        {
            await SaveToFileAsync(_dataPathPlaylists, playlists);
        }
        public async Task SaveShortsAsync(ObservableCollection<ShortsVideo> shorts)
        {
            await SaveToFileAsync(_dataPathShorts, shorts);
        }
        public async Task SaveOtherVideosAsync(ObservableCollection<OtherVideo> otherVideos)
        {
            await SaveToFileAsync(_dataPathOther, otherVideos);
        }


        //------------------------------------Main Save Async Method----------------------
        private async Task SaveToFileAsync<T>(string filePath, ObservableCollection<T> collection)
        {
            Debug.WriteLine($"Saving data to: {filePath}");

            string? directory = System.IO.Path.GetDirectoryName(filePath);
            if (directory == null) //Make sure that the directory given is valid
            {
                throw new InvalidOperationException("Directory path could not be determined.");
            }

            if (!Directory.Exists(directory)) //If the folder doesnt exist, create it
            {
                Directory.CreateDirectory(directory);
            }

            //This creates the necessary classes to create the json files
            var serializableList = collection.ToList();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            //This writes all of the json text given the data
            string json = JsonSerializer.Serialize(serializableList, options);
            await File.WriteAllTextAsync(filePath, json);
            Debug.WriteLine("Data saved successfully");
        }
       

        //-------------------------All of the load values for each type of file-----------
        public async Task<ObservableCollection<RegularVideo>> LoadRegularVideosAsync()
        {
            return await LoadFromFileAsync<RegularVideo>(_dataPathVideos);
        }
        public async Task<ObservableCollection<PlayList>> LoadPlaylistsAsync()
        {
            return await LoadFromFileAsync<PlayList>(_dataPathPlaylists);
        }
        public async Task<ObservableCollection<ShortsVideo>> LoadShortsAsync()
        {
            return await LoadFromFileAsync<ShortsVideo>(_dataPathShorts);
        }
        public async Task<ObservableCollection<OtherVideo>> LoadOtherVideosAsync()
        {
            return await LoadFromFileAsync<OtherVideo>(_dataPathOther);
        }

       
        //---------This is the maind async load file that gets all of the data--------
        private async Task<ObservableCollection<T>> LoadFromFileAsync<T>(string filePath)
        {
            Debug.WriteLine($"Loading data from: {filePath}");

            if (!File.Exists(filePath)) //Check to see if path exists, if not return empty
            {
                Debug.WriteLine("File does not exist, returning empty collection");
                return new ObservableCollection<T>();
            }

            //Await until all of the json data is read
            string json = await File.ReadAllTextAsync(filePath);
            Debug.WriteLine($"JSON content length: {json.Length}");

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
    }

    //This class will hold all of the source lists, and be able to manage each of the lists (maybe save and load to json too idk yet)
    public class ContentManager
    {
        public ObservableCollection<ShortsVideo>  ShortsArray   = new ObservableCollection<ShortsVideo>();
        public ObservableCollection<RegularVideo> VideosArray   = new ObservableCollection<RegularVideo>();
        public ObservableCollection<PlayList>     PlaylistArray = new ObservableCollection<PlayList>();
        public ObservableCollection<OtherVideo>   OtherArray    = new ObservableCollection<OtherVideo>();

        //Clear Function
        public void clearAllCollections()
        {
            ShortsArray.Clear();
            VideosArray.Clear();
            PlaylistArray.Clear();
            OtherArray.Clear();
        }

        //-----------------------------------------------FIND FUNCTIONS-------------------------------------------------------
        //Functions iterate through each items id to find the right one, otherwise returns an new empty one
        public ShortsVideo FindShort(int id)
        {
            foreach (ShortsVideo video in ShortsArray)
            {
                if(video.id == id)
                {
                    return video;
                }
            }
            return new ShortsVideo();
        }
        //The regular youtube video finder
        public RegularVideo FindVideo(string id)
        {
            foreach (RegularVideo video in VideosArray)
            {
                if (video.id == id)
                {
                    return video;
                }
            }
            return new RegularVideo();
        }
        //The regular playlist finder
        public PlayList FindPlaylist(string id)
        {
            foreach (PlayList playlist in PlaylistArray)
            {
                if (playlist.id == id)
                {
                    return playlist;
                }
            }
            return new PlayList();
        }
        //The other video finder via the id
        public OtherVideo FindOtherVideo(int id)
        {
            foreach (OtherVideo video in OtherArray)
            {
                if (video.id == id)
                {
                    return video;
                }
            }
            return new OtherVideo();
        }

        //----------------------------------------------REMOVE FUNCTIONS-------------------------------------------------------
        //Remove a video function
        public void RemoveVideo(RegularVideo video)
        {
            VideosArray.Remove(video);
        }
        //Remove a playlist from list
        public void RemovePlaylist(PlayList playlist)
        {
            PlaylistArray.Remove(playlist);
        }
        //Remove a Short from list
        public void RemoveShort(ShortsVideo shorts)
        {
            ShortsArray.Remove(shorts);
        }
        //Remove an other type object
        public void RemoveOther(OtherVideo other)
        {
            OtherArray.Remove(other);
        }

        //------------------------------------------------CONSTRUCTOR-------------------------------------------------------


    }

    //Shorts Videos, hold link, maybe a topic, maybe a description, maybe a title
    public class RegularVideo
    {
        //The path to where downloaded Videos are stored
        private readonly string _dataPathVideos = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\SavedVideos\\");

        private readonly string _ffmpegFilePath = Path.Combine(GetProjectRootDirectory(AppContext.BaseDirectory), "ffmpeg", "ffmpeg.exe");

        //All of the Data to store from the video itself
        public string   id               { get; set; }
        public string   title            { get; set; } = string.Empty;
        public string   category         { get; set; } = string.Empty;
        public string   notes            { get; set; } = string.Empty;
        public string   url              { get; set; } = string.Empty;
        public string   platform    { get; set; } = string.Empty;
        public string   thumbNailUrl     { get; set; } = string.Empty;
        public TimeSpan duration         { get; set; }
        public string   author           { get; set; } = string.Empty;
        public DateTime addedDate        { get; set; }
        public string   description      { get; set; } = string.Empty;
        public string   durationFormatted  => duration.ToString(@"hh\:mm\:ss");
        public string   addedDateFormatted => addedDate.ToString("MMM dd, yyyy");

        public bool     isDownloaded       = false;

        public YoutubeClient youtube = new YoutubeClient();


        //------------------------Constructors--------------------------------
        public RegularVideo(string URL, string Category, string Notes, string platForm)
        {
            //Set up all of the needed information in the constructor
            this.url = URL;
            this.category = Category;
            this.notes = Notes;
            this.platform = platForm;
        }
        public RegularVideo() { }


        //-------------------------Functions for playing and also storing youtube videos--------------------
        public void playVideo(string urlInput)
        {

        } //TODO See if its possible to do this

        /**
        [Obsolete("This may violate youtube terms of service. Use at your own risk.")]
        public async Task downloadVideo(string urlInput)
        {
            //See if it is a yt video, if it is then download and store
            try
            {
                //Check To see if Ffmpeg exits
                if (!File.Exists(_ffmpegFilePath))
                {
                    Debug.WriteLine("DIRECTORY: " + _ffmpegFilePath);
                    MessageBox.Show(
                        "FFmpeg is not found. Please ensure ffmpeg.exe is in the 'ffmpeg' folder next to the application.",
                        "FFmpeg Missing",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var video = await youtube.Videos.GetAsync(urlInput);

                //Create the output path via the roaming apps, proper title, and the file type
                string outputPath = _dataPathVideos + string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars())) + ".mp4";

                if (!Directory.Exists(_dataPathVideos)) //Create directory if it doesnt exist yet
                {
                    Directory.CreateDirectory(_dataPathVideos);
                }

                //Download at the given location, combining both audio and video using the explicit ffmpeg filepath
                await youtube.Videos.DownloadAsync(urlInput, outputPath, o => o.SetContainer("mp4").SetFFmpegPath(_ffmpegFilePath).SetPreset(ConversionPreset.Fast));

                //Tell user that the video was downloaded successfully
                MessageBox.Show(
                    "Video Downloaded Succcessfully",
                    "Success!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
            catch (Exception ex) //If not yt video, store only link and dont download
            {
                Debug.WriteLine("Download failed");
                MessageBox.Show(
                    "Link is either not valid or not from Youtube. Please double check the URL. Save Current URL?.",
                    "Invalid URL",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Information);
            }
        }
        **/
        public async Task updateVideoItems(string urlInput)
        {
            //Try Catch for links that are not from youtube
            try
            {
                //Get Video data type
                Video video = await youtube.Videos.GetAsync(urlInput);

                //Set up all the data so that way it could be saved via the given link
                this.id = video.Id;
                this.title = video.Title;
                this.description = video.Description;
                this.duration = (TimeSpan)video.Duration;
                this.author = video.Author.ChannelTitle;
                this.addedDate = DateTime.Now;
                this.url = video.Url;
                //TODO: Add in a way to make it so you see the thumbnails
            }
            catch (ArgumentException)
            {
                MessageBoxResult result = MessageBox.Show(
                    "The URL entered is not from an known site; information will have to be entered manually.",
                    "Unknown URL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(
                    "The URL entered is Not Valid. Unable To Add New Content.",
                    "Invalid URL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }

        }

        //Helper function for naviagting to ffmpeg
        public static string GetProjectRootDirectory(string startingPath)
        {
            var directory = new DirectoryInfo(startingPath);

            while (directory != null &&
                   !directory.GetFiles("*.csproj").Any() &&
                   !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? startingPath;
        }
    }

    //The regular youtube videos, holds url, thumbnail, content, description, all the goods
    public class ShortsVideo
    {
        public int id                   { get; set; }
        public string title             { get; set; } = string.Empty;
        public string url               { get; set; } = string.Empty;
        public string thumbNailUrl      { get; set; } = string.Empty;
        public TimeSpan duration        { get; set; }
        public string platform          { get; set; } = string.Empty;
        public string notes             { get; set; } = string.Empty;
        public string category          { get; set; } = string.Empty;
        public DateTime addedDate       { get; set; }
        public string description       { get; set; } = string.Empty;
        public string durationFormatted => duration.ToString(@"hh\:mm\:ss");
        public string addedDateFormatted => addedDate.ToString("MMM dd, yyyy");

        public YoutubeClient youtube = new YoutubeClient();


        //------------------------Constructors--------------------------------
        public ShortsVideo(string URL, string Category, string Notes, string platForm)
        {
            //Set up all of the needed information in the constructor
            this.url = URL;
            this.category = Category;
            this.notes = Notes;
            this.platform = platForm;
        }
        public ShortsVideo() { }
    }

    //Holds all of the information of a playlist, like category, #num of vids, idk stuff like that
    public class PlayList
    {
        public string               id               { get; set; }
        public string               title            { get; set; } = string.Empty;
        public string               author           { get; set; } = string.Empty;
        public string               url              { get; set; } = string.Empty;
        public string               thumbNailUrl     { get; set; } = string.Empty;
        public int                  numOfVideos      { get; set; }
        public string               platform         { get; set; } = string.Empty;
        public string               notes            { get; set; } = string.Empty;
        public string               category         { get; set; } = string.Empty;
        public DateTime             addedDate        { get; set; }

        public List<RegularVideo> regularVideos      { get; set; } = new List<RegularVideo>();

        public string               addedDateFormatted => addedDate.ToString("MMM dd, yyyy");
        public YoutubeClient        youtube            = new YoutubeClient();

        //------------------------Constructors--------------------------------
        public PlayList(string URL, string Category, string Notes, string platForm)
        {
            //Set up all of the needed information in the constructor
            this.url = URL;
            this.category = Category;
            this.notes = Notes;
            this.platform = platForm;
        }
        public PlayList() { }

        //------------------------------------Information Manipulation Functions-----------------------
        public async Task updatePlaylistItems(string URL)
        {
            try
            {
                //get the actual playlist object via given URL
                var playlist = await youtube.Playlists.GetAsync(URL);
                var playlistVideos = await youtube.Playlists.GetVideosAsync(playlist.Id).CollectAsync();

                //For each of the videos in the list, get it's information and store it
                foreach (var video in playlistVideos)
                {
                    RegularVideo newVideo = new RegularVideo();
                    newVideo.id = video.Id.Value;
                    newVideo.title = video.Title;
                    newVideo.duration = video.Duration ?? TimeSpan.Zero;
                    newVideo.author = video.Author.ChannelTitle;
                    newVideo.addedDate = DateTime.Now;
                    newVideo.url = video.Url;
                    newVideo.platform = "YouTube";
                    regularVideos.Add(newVideo);
                }

                //Also get all of the infomration of the playlist itself
                this.id = playlist.Id.ToString();
                this.title = playlist.Title;
                this.addedDate = DateTime.Now;
                this.author = playlist.Author.ChannelTitle;
                this.url = URL;
                this.numOfVideos = playlistVideos.Count;
                //TODO Add in a way to see thumbnail
            }
            catch (ArgumentException)
            { //TODO Add in a way to cancel the operation
                MessageBoxResult result = MessageBox.Show(
                    "The URL entered is not from an known site; information will have to be entered manually.",
                    "Unknown URL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(
                    "The URL entered is Not Valid. Unable To Add New Content.",
                    "Invalid URL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
            }
        }
    }

    //Holds as many miscellianous pieces of information as possible
    public class OtherVideo
    {
        public int id            { get; set; }
        public string url        { get; set; } = string.Empty;
        public string platform   { get; set; } = string.Empty;
        public string notes      { get; set; } = string.Empty;
        public string category   { get; set; } = string.Empty;


        //------------------------Constructors--------------------------------
        public OtherVideo(string URL, string Category, string Notes, string platForm)
        {
            //Set up all of the needed information in the constructor
            this.url = URL;
            this.category = Category;
            this.notes = Notes;
            this.platform = platForm;
        }
        public OtherVideo() { }
    }




    public partial class videoManager : Page
    {

        //Initialize all needed objects
        private readonly VideoJsonSaver jsonManagement = new VideoJsonSaver();
        public           ContentManager contentManager = new ContentManager();
        public           String         tabName        = string.Empty;
        public           String         createTabName  = string.Empty;

        public videoManager()
        {
            InitializeComponent();
            InitializeAllLists().ConfigureAwait(false);
        }

        //----------------------------Miscelnious Functions for saving or loading----------------------------
        //This function loads all the data from the user json files into the app, and also sets the item sources for each listbox
        private async Task InitializeAllLists()
        {

            //Get all of the tasks from the json files 
            ObservableCollection<RegularVideo> videos    = await jsonManagement.LoadRegularVideosAsync();
            ObservableCollection<PlayList>     playlists = await jsonManagement.LoadPlaylistsAsync();
            ObservableCollection<ShortsVideo>  shorts    = await jsonManagement.LoadShortsAsync();
            ObservableCollection<OtherVideo>   others    = await jsonManagement.LoadOtherVideosAsync();

            //Clear all of the collections just incase
            contentManager.clearAllCollections();

            //Actually append of the gotten videos from the json into the main content Manager for use
            foreach(RegularVideo video in videos)
            {
                contentManager.VideosArray.Add(video);
            }
            foreach(PlayList playlist in playlists)
            {
                contentManager.PlaylistArray.Add(playlist);
            }
            foreach (ShortsVideo shortVideo in shorts)
            {
                contentManager.ShortsArray.Add(shortVideo);
            }
            foreach (OtherVideo other in others)
            {
                contentManager.OtherArray.Add(other);
            }

            //Set all of the viewsource/listBoxSources according to the manager
            VideoListBox.ItemsSource    = contentManager.VideosArray;
            PlaylistListBox.ItemsSource = contentManager.PlaylistArray;
            ShortsListBox.ItemsSource   = contentManager.ShortsArray;
            OtherListBox.ItemsSource    = contentManager.OtherArray;

            Debug.WriteLine("ALL DATA CREATED CORRECTLY");

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
                    this.tabName = tabName;
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

        
        //---------------------------------------All Left Sidebar Buttons------------------------------------
        //Open up a new popup that allows for the creation of a new Video, Short, Playlist, or Other
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            //Start Slide in animation for new creation GUI
            var button = sender as Button;
            Storyboard storyboard = new Storyboard();

            if (button.Name == "NewButton") //For Add content button
            {
                //Set up translucent box and all the proper Z panel stuff
                Panel.SetZIndex(NewContentGUI, 31);
                Panel.SetZIndex(translucentBox, 30);

                //Set up a doubleAnimation to move the x val
                DoubleAnimation translateXTo = new DoubleAnimation
                {
                    From = -1500,
                    To = 10,
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false,
                };
                //Opacity animaiton for black box
                DoubleAnimation opactiy = new DoubleAnimation
                {
                    From = 0,
                    To = 0.7,
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false,
                };

                Storyboard.SetTarget(opactiy, translucentBox);
                Storyboard.SetTargetProperty(opactiy, new PropertyPath("Opacity"));
                Storyboard.SetTarget(translateXTo, NewContentGUI);
                Storyboard.SetTargetProperty(translateXTo, new PropertyPath("(RenderTransform).(TranslateTransform.X)"));

                storyboard.Children.Add(opactiy);
                storyboard.Children.Add(translateXTo);
                storyboard.Begin();
            }
            else if(button.Name == "NewContentBack"|| button.Name == "SaveNewContentButton")
            {
                Panel.SetZIndex(NewContentGUI, 0);
                Panel.SetZIndex(translucentBox, -10);

                //Set up the animation for it to slide bacl
                DoubleAnimation translateXBack = new DoubleAnimation
                {
                    From = 10,
                    To = -1500,
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false,
                };
                DoubleAnimation opactiy = new DoubleAnimation
                {
                    From = 0.7,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false,
                };

                Storyboard.SetTarget(opactiy, translucentBox);
                Storyboard.SetTargetProperty(opactiy, new PropertyPath("Opacity"));
                Storyboard.SetTarget(translateXBack, NewContentGUI);
                Storyboard.SetTargetProperty(translateXBack, new PropertyPath("(RenderTransform).(TranslateTransform.X)"));

                storyboard.Children.Add(opactiy);
                storyboard.Children.Add(translateXBack);
                storyboard.Begin();

                clearCurrentGUI(sender, e);
            }
        }
        //Delete the currently selected Item in the ListSourceBox
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }
        //TODO: Implement the ability to search for videos by title or something
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

        }
        //Save all of the videos from all of the tabs to the json file
        private async void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);
            await jsonManagement.SavePlaylistsAsync(contentManager.PlaylistArray);
            await jsonManagement.SaveShortsAsync(contentManager.ShortsArray);
            await jsonManagement.SaveOtherVideosAsync(contentManager.OtherArray);
        }
        //Save only the current content in the open tab
        private async void SavePageButton_Click(object sender, RoutedEventArgs e)
        {
            //Null Check to return nothing to stop errors.
            if(string.IsNullOrEmpty(tabName))
            {
                return;
            }

            //Check to see which page is open, and then save that page
            if     (tabName == "VideoTab")
            {
                await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);
            }
            else if(tabName == "PlaylistTab")
            {
                await jsonManagement.SavePlaylistsAsync(contentManager.PlaylistArray);
            }
            else if(tabName == "ShortsTab")
            {
                await jsonManagement.SaveShortsAsync(contentManager.ShortsArray);
            }
            else if(tabName == "OtherTab")
            {
                await jsonManagement.SaveOtherVideosAsync(contentManager.OtherArray);
            }
        }


        //--------------------------------------Functions for the Creation GUI-----------------------------
        private void TabControlCreation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String tabName = "";

            //get tab that is selected and it's name, hold it in class for future 
            if (e.Source is TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    tabName = selectedTab.Name; //assign name of tab for later use
                    this.createTabName = tabName;
                }
            }

            //Check to see which tab has been clicked in order to see what the title should display
            if      (tabName == "CreateVideoTab")     { AddNewContentTitle.Text = "Add New Long Form Video"; }
            else if (tabName == "CreateShortsTab")    { AddNewContentTitle.Text = "Add New Shorts Video"; }
            else if (tabName == "CreatePlaylistTab")  { AddNewContentTitle.Text = "Add New Playlist"; }
            else if (tabName == "CreateOtherTab")     { AddNewContentTitle.Text = "Add Another Type Of Content"; }
        }

        private async void addNewContent(object sender, RoutedEventArgs e)
        {
         
            //Check To see if any content is null
            if(string.IsNullOrEmpty(URL.Text.ToString())) 
            {
                MessageBox.Show(
                    "A URL is required",
                    "Incomplete Submission",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            //Check to see the type of content created, then create/store it accordingly
            if      (createTabName == "CreateVideoTab")
            {
                //Create a new video object using all of the gathered data
                RegularVideo video = new RegularVideo(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );

                //Update all of it's internal variables
                await video.updateVideoItems(video.url);

                //Append it to the list of all videos
                contentManager.VideosArray.Add(video);

                //Then save the newly added content
                await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);

                //After Saving, clear the textFields, then close the popup
                clearCurrentGUI(sender, e);
                NewButton_Click(sender, e);
            }
            else if (createTabName == "CreatePlaylistTab")
            {
                PlayList playlist = new PlayList(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );

                //Get all of the proper meta data for the playlist
                await playlist.updatePlaylistItems(playlist.url);

                //Append it to the list of all playlists
                contentManager.PlaylistArray.Add(playlist);

                //Then save the newly added content
                await jsonManagement.SavePlaylistsAsync(contentManager.PlaylistArray);

                //After Saving, clear the textFields, then close the popup
                clearCurrentGUI(sender, e);
                NewButton_Click(sender, e);


            }
            else if (createTabName == "CreateShortsTab")
            {
                ShortsVideo shortsVideo = new ShortsVideo(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );

                //Append it to the list of all shorts
                contentManager.ShortsArray.Add(shortsVideo);

                //Then save the newly added content
                await jsonManagement.SaveShortsAsync(contentManager.ShortsArray);


                //After Saving, clear the textFields, then close the popup
                clearCurrentGUI(sender, e);
                NewButton_Click(sender, e);
            }
            else if (createTabName == "CreateOtherTab")
            {
                OtherVideo other = new OtherVideo(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );

                //Append it to the list of all other videos
                contentManager.OtherArray.Add(other);

                //Then save the newly added content
                await jsonManagement.SaveOtherVideosAsync(contentManager.OtherArray);


                //After Saving, clear the textFields, then close the popup
                clearCurrentGUI(sender, e);
                NewButton_Click(sender, e);
            }
        }

        //Empty out all of the fields that the user can put in
        public void clearCurrentGUI(object sender, RoutedEventArgs e)
        {
            URL.Text = string.Empty;
            Category.Text = string.Empty;
            Notes.Text = string.Empty;
            Platform.Text = string.Empty;
        }
    }
}
