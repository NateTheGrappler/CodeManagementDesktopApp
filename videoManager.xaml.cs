using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
using System.Net.Http;



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
        public YoutubeClient                      yt            = new YoutubeClient();


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
        public ShortsVideo FindShort(string id)
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
        public OtherVideo FindOtherVideo(string id)
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

        //------------------------------------------------Link Manager Functions-------------------------------------------------------
        public string CheckLinkType(string link)
        {
            if(link.Contains("youtube.com/playlist"))
            {
                return "playlist";
            }
            if(link.Contains("youtube.com/watch"))
            {
                return "video";
            }

            return "neither";
        }

    }

    //Shorts Videos, hold link, maybe a topic, maybe a description, maybe a title
    public class RegularVideo
    {
        //The path to where downloaded Videos are stored
        private readonly string _dataPathVideos = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodeInformationManagingSystem\\SavedVideos\\");

        //private readonly string _ffmpegFilePath = Path.Combine(GetProjectRootDirectory(AppContext.BaseDirectory), "ffmpeg", "ffmpeg.exe");

        //All of the Data to store from the video itself
        public string           id               { get; set; }
        public string           title            { get; set; } = string.Empty;
        public string           category         { get; set; } = string.Empty;
        public string           notes            { get; set; } = string.Empty;
        public string           url              { get; set; } = string.Empty;
        public string           platform         { get; set; } = string.Empty;
        public string           thumbNailUrl     { get; set; } = "https://i.ytimg.com/vi/9wafxM-vA0E/maxresdefault.jpg";
        public TimeSpan         duration         { get; set; } = TimeSpan.FromSeconds(1);
        public string           author           { get; set; } = string.Empty;
        public DateTime         addedDate        { get; set; }
        public string           description      { get; set; } = string.Empty;

        public string   durationFormatted  => duration.ToString(@"hh\:mm\:ss");
        public string   addedDateFormatted => addedDate.ToString("MMM dd, yyyy");

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
            //Get Video data type
            Video video = await youtube.Videos.GetAsync(urlInput);

            //Set up all the data so that way it could be saved via the given link
            if(video != null)
            {
                Debug.WriteLine("Saving data from youtube: title is " + this.title);
                this.id = video.Id;
                this.title = video.Title;
                this.description = video.Description;
                this.duration = (TimeSpan)video.Duration;
                this.author = video.Author.ChannelTitle;
                this.addedDate = DateTime.Now;
                this.url = video.Url;
                this.thumbNailUrl = GetBestThumbnailUrl(video);

            }
        }

        //---------------------------------Helper Functions-----------------------------------------------
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
        private string GetBestThumbnailUrl(Video video)
        {
            //Try to get the highest quality thumbnail available
            var thumbnails = video.Thumbnails;

            //Order by resolution (width * height) descending to get best quality
            var bestThumbnail = thumbnails
                .OrderByDescending(t => t.Resolution.Width * t.Resolution.Height)
                .FirstOrDefault();

            return bestThumbnail?.Url ?? string.Empty;
        }


    }

    //The regular youtube videos, holds url, thumbnail, content, description, all the goods
    public class ShortsVideo
    {
        public string id                { get; set; } = string.Empty;
        public string title             { get; set; } = string.Empty;
        public string url               { get; set; } = string.Empty;
        public string thumbNailUrl      { get; set; } = string.Empty;
        public string author            { get; set; } = string.Empty;
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
    }

    //Holds as many miscellianous pieces of information as possible
    public class OtherVideo
    {
        public string id           { get; set; } = string.Empty;
        public string title        { get; set; } = string.Empty;
        public string url          { get; set; } = string.Empty;
        public string thumbNailUrl { get; set; } = string.Empty;
        public string author       { get; set; } = string.Empty;
        public TimeSpan duration   { get; set; }
        public string platform     { get; set; } = string.Empty;
        public string notes        { get; set; } = string.Empty;
        public string category     { get; set; } = string.Empty;
        public DateTime addedDate  { get; set; }
        public string description  { get; set; } = string.Empty;
        public string durationFormatted => duration.ToString(@"hh\:mm\:ss");
        public string addedDateFormatted => addedDate.ToString("MMM dd, yyyy");


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
        //The Objects for when the user selects different types of content
        public           RegularVideo   selectedVideo;
        public           Playlist       selectedPlaylist;
        public           ShortsVideo    selectedShorts;
        public           OtherVideo     selectedOther;


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
                CreateTabControl.SelectedItem = CreateVideoTab;

            }
            else if(tabName == "PlaylistTab")
            {
                Debug.WriteLine("PlaylistTab");
                Panel.SetZIndex(VideoListBox,     0);
                Panel.SetZIndex(PlaylistListBox, 20);
                Panel.SetZIndex(ShortsListBox,    0);
                Panel.SetZIndex(OtherListBox,     0);
                CreateTabControl.SelectedItem = CreatePlaylistTab;

            }
            else if(tabName == "ShortsTab")
            {
                Debug.WriteLine("ShortsTab");
                Panel.SetZIndex(VideoListBox,    0);
                Panel.SetZIndex(PlaylistListBox, 0);
                Panel.SetZIndex(ShortsListBox,  20);
                Panel.SetZIndex(OtherListBox,    0);
                CreateTabControl.SelectedItem = CreateShortsTab;
            }
            else if(tabName == "OtherTab")
            {
                Debug.WriteLine("OtherTab");
                Panel.SetZIndex(VideoListBox,    0);
                Panel.SetZIndex(PlaylistListBox, 0);
                Panel.SetZIndex(ShortsListBox,   0);
                Panel.SetZIndex(OtherListBox,   20);
                CreateTabControl.SelectedItem = CreateOtherTab;
            }
            else
            {
                Debug.WriteLine("ERROR");
            }
        }



        
        //---------------------------------------All Left Sidebar Buttons------------------------------------
        //Open up a new popup that allows for the creation of a new Video, Short, Playlist, or Other
        private void NewButton_Click(object sender, RoutedEventArgs e) //Lowkey just all of the animations
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
                    Duration = TimeSpan.FromSeconds(0.5),
                    AutoReverse = false,
                };
                //Opacity animaiton for black box
                DoubleAnimation opactiy = new DoubleAnimation
                {
                    From = 0,
                    To = 0.7,
                    Duration = TimeSpan.FromSeconds(0.5),
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
                    Duration = TimeSpan.FromSeconds(0.5),
                    AutoReverse = false,
                };
                DoubleAnimation opactiy = new DoubleAnimation
                {
                    From = 0.7,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
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



        //-------------------------All of the functions for the actual main content listboxes-------------------------------
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        //Handle button that is attached to the three dots in the main listbox
        private void OpenContextMenu(object sender, RoutedEventArgs e)
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
            ContextMenu menu = this.FindResource("VideoContextMenuKey") as ContextMenu;
            
            if (menu != null) //if menu is not null, then display it
            {
                //Open the Context Menu
                menu.IsOpen = true;

                //Add in click function based on header value
                
                foreach (MenuItem item in menu.Items)
                {
                    string name = item.Header.ToString();
                    if     (name == "Edit")           { item.Click += openInfoGUI; }
                    else if(name == "Save")           { item.Click += saveVideoPage; }
                    else if(name == "Delete")         { item.Click += deleteSelectedItem; }
                    else if(name == "View Full Info") { item.Click += openInfoGUI; }

                }
                
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

        //THE FUNCTION THAT HANDLES ALL LOADING BACKEND FUNCTIONALITY
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


            //Have a safeguard for if a person is in the wrong tab when adding content, by passing the other stuff if its a link from yt
            if (contentManager.CheckLinkType(URL.Text.ToString()) == "video")
            {
                RegularVideo video = new RegularVideo(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );
                video.title = URL.Text.ToString(); //Set the title as the url before actual title is gotten so you have something to display

                Debug.WriteLine("New Video Created with title: " + video.title);

                //Change the add button to be a loading as an indicator
                SaveNewContentButton.IsEnabled = false;
                SaveNewContentButton.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                SaveNewContentButton.Content = "Loading...";

                //Update all of it's internal variables and then save it
                await video.updateVideoItems(video.url);
                contentManager.VideosArray.Add(video);
                await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);

                //Reset Add Button Color
                SaveNewContentButton.IsEnabled = true;
                SaveNewContentButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
                SaveNewContentButton.Content = "Add";

                //clear the textFields, then close the popup
                clearCurrentGUI(sender, e);
                NewButton_Click(sender, e);
                TabControl.SelectedItem = VideoTab;
                return;
            }
            else if (contentManager.CheckLinkType(URL.Text.ToString()) == "playlist")
            {

                PlayList playlist = new PlayList(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );

                //Change the add button to be a loading as an indicator
                SaveNewContentButton.IsEnabled = false;
                SaveNewContentButton.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));

                //Get all of the proper meta data for the playlist
                await playlist.updatePlaylistItems(playlist.url);
                contentManager.PlaylistArray.Add(playlist);
                await jsonManagement.SavePlaylistsAsync(contentManager.PlaylistArray);

                //Reset Add Button Color
                SaveNewContentButton.IsEnabled = true;
                SaveNewContentButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));

                //After Saving, clear the textFields, then close the popup
                clearCurrentGUI(sender, e);
                NewButton_Click(sender, e);
                TabControl.SelectedItem = PlaylistTab;
                return;
            }


            //Check to see the type of content created, then create/store it accordingly
            if (createTabName == "CreateVideoTab")
            {
                //Create a new video object using all of the gathered data
                RegularVideo video = new RegularVideo(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );
                video.title = URL.Text.ToString(); //Set the title as the url before actual title is gotten so you have something to display

                if (contentManager.CheckLinkType(URL.Text.ToString()) == "video") //If Is youtube video, save all of the data
                {
                    //Change the add button to be a loading as an indicator
                    SaveNewContentButton.IsEnabled = false;
                    SaveNewContentButton.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                    SaveNewContentButton.Content = "Loading...";

                    //Update all of it's internal variables and then save it
                    await video.updateVideoItems(video.url);
                    contentManager.VideosArray.Add(video);
                    await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);

                    //Reset Add Button Color
                    SaveNewContentButton.IsEnabled = true;
                    SaveNewContentButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
                    SaveNewContentButton.Content = "Add";

                    //clear the textFields, then close the popup
                    clearCurrentGUI(sender, e);
                    NewButton_Click(sender, e);
                }
                else //Otherwise give the option to proceed or not
                {
                    //Let them cancel or continue, if cancel clear, if not add the input only, no extra data
                    MessageBoxResult result = MessageBox.Show(
                        "The URL entered is Invalid or from an Unknown site; information will have to be entered manually.",
                        "Unknown URL",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning
                        );
                    if (result == MessageBoxResult.Cancel)
                    {
                        clearCurrentGUI(sender, e);
                    }
                    else
                    {
                        contentManager.VideosArray.Add(video);
                        await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);
                        clearCurrentGUI(sender, e);
                        NewButton_Click(sender, e);
                    }
                }

                TabControl.SelectedItem = VideoTab;
            }
            else if (createTabName == "CreatePlaylistTab")
            {
                PlayList playlist = new PlayList(
                    URL.Text.ToString(),
                    Category.Text.ToString(),
                    Notes.Text.ToString(),
                    Platform.Text.ToString()
                    );

                if (contentManager.CheckLinkType(URL.Text.ToString()) == "playlist")
                {
                    //Change the add button to be a loading as an indicator
                    SaveNewContentButton.IsEnabled = false;
                    SaveNewContentButton.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));

                    //Get all of the proper meta data for the playlist
                    await playlist.updatePlaylistItems(playlist.url);
                    contentManager.PlaylistArray.Add(playlist);
                    await jsonManagement.SavePlaylistsAsync(contentManager.PlaylistArray);

                    //Reset Add Button Color
                    SaveNewContentButton.IsEnabled = true;
                    SaveNewContentButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));

                    //After Saving, clear the textFields, then close the popup
                    clearCurrentGUI(sender, e);
                    NewButton_Click(sender, e);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(
                        "The URL entered is not from an known site; information will have to be entered manually.",
                        "Unknown URL",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning
                        );
                    if (result == MessageBoxResult.Cancel)
                    {
                        clearCurrentGUI(sender, e);
                    }
                    else
                    {
                        contentManager.PlaylistArray.Add(playlist);
                        await jsonManagement.SavePlaylistsAsync(contentManager.PlaylistArray);
                        clearCurrentGUI(sender, e);
                        NewButton_Click(sender, e);
                    }
                }

                TabControl.SelectedItem = PlaylistTab;

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
                TabControl.SelectedItem = ShortsTab;
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
                TabControl.SelectedItem = OtherTab;
            }
        }


        //------------------------------------------------Helper Functions-----------------------------------------------

        //Empty out all of the fields that the user can put in
        public void clearCurrentGUI(object sender, RoutedEventArgs e)
        {
            URL.Text = string.Empty;
            Category.Text = string.Empty;
            Notes.Text = string.Empty;
            Platform.Text = string.Empty;
        }


        //--------------------------------------------Functions for Context Menu Items-------------------------
        
        //The function to open the GUI to view a RegularVideos full details
        public void openInfoGUI(object sender, RoutedEventArgs e)
        {
            Storyboard storyboard = new Storyboard();

            //Check to see if its the X button, if so run the animation then return so rest of the code doesnt run
            if (sender is Button button)
            {
                //Update all of the neccessary information for the video the user is about to see
                clearAllInfo();
                Panel.SetZIndex(MoreInfoGUI,    -15);
                Panel.SetZIndex(translucentBox, -15);

                DoubleAnimation transXBack = new DoubleAnimation
                {
                    From = 1,
                    To = 1500,
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                DoubleAnimation opacityBack = new DoubleAnimation
                {
                    From = 0.7,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5)
                };


                Storyboard.SetTarget(opacityBack, translucentBox);
                Storyboard.SetTargetProperty(opacityBack, new PropertyPath("Opacity"));
                Storyboard.SetTarget(transXBack, MoreInfoGUI);
                Storyboard.SetTargetProperty(transXBack, new PropertyPath("(RenderTransform).(TranslateTransform.X)"));

                storyboard.Children.Add(opacityBack);
                storyboard.Children.Add(transXBack);
                storyboard.Begin();

                return; //The return at the end so the rest of the code doesnt run
            }
            else
            {
                //Update all of the neccessary information for the video the user is about to see
                updateAllInfo(sender, e);
                Panel.SetZIndex(MoreInfoGUI, 31);
                Panel.SetZIndex(translucentBox, 30);

                //Set the animation the same as the others, but just coming from the left this time
                DoubleAnimation transX = new DoubleAnimation
                {
                    From = 1500,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                DoubleAnimation opacity = new DoubleAnimation
                {
                    From = 0,
                    To = 0.7,
                    Duration = TimeSpan.FromSeconds(0.5)
                };


                Storyboard.SetTarget(opacity, translucentBox);
                Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));
                Storyboard.SetTarget(transX, MoreInfoGUI);
                Storyboard.SetTargetProperty(transX, new PropertyPath("(RenderTransform).(TranslateTransform.X)"));

                storyboard.Children.Add(opacity);
                storyboard.Children.Add(transX);
                storyboard.Begin();
            }

        }

        //Purely really just a cosmetic function
        public async void saveVideoPage(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = this.FindResource("VideoContextMenuKey") as ContextMenu;

            //Find the Save menu button, change it to indicate saving to user, save, then change it back.
            if (menu != null)
            {
                foreach (MenuItem item in menu.Items)
                {
                    if (item.Header.ToString() == "Save")
                    {
                        item.Header = "Saving...";
                        await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);
                        item.Header = "Save";
                    }
                }
            }
        }

        //The universal delete function for any given piece of content
        private async void deleteSelectedItem(object sender, RoutedEventArgs e)
        {

            //Check to see if for some reason no tab is selected
            if (string.IsNullOrEmpty(tabName))
            {
                return;
            }

            //Check open tab, and if said tab has a selected item, if so, delete it (For the main trash button), and save the page.
            if (tabName == "VideoTab" && VideoListBox.SelectedItem != null)
            {
                contentManager.VideosArray.Remove(VideoListBox.SelectedItem as RegularVideo);
                await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);
            }
            else if (tabName == "PlaylistTab" && PlaylistListBox.SelectedItem != null)
            {
                contentManager.PlaylistArray.Remove(PlaylistListBox.SelectedItem as PlayList);
                await jsonManagement.SavePlaylistsAsync(contentManager.PlaylistArray);
            }
            else if (tabName == "ShortsTab"   && ShortsListBox.SelectedItem != null)
            {
                contentManager.ShortsArray.Remove(ShortsListBox.SelectedItem as ShortsVideo);
                await jsonManagement.SaveShortsAsync(contentManager.ShortsArray);
            }
            else if (tabName == "OtherTab"    && OtherListBox.SelectedItem != null)
            {
                contentManager.OtherArray.Remove(OtherListBox.SelectedItem as OtherVideo);
                await jsonManagement.SaveOtherVideosAsync(contentManager.OtherArray);
            }

        }

        //---------------------------Functions For the More Info GUI------------------------------------------

        //Clear all of the displayed information about a topic in the info GUI //TODO: Add in visibibility and invisiblity of all of the videos in a playlist
        private void clearAllInfo()
        {
            //Iterate over each child in grid, if child is textbox, set it's content to empty
            foreach(UIElement item in infoGrid.Children)
            {
                if(item is System.Windows.Controls.TextBox tb)
                {
                    tb.Text = string.Empty;
                }
            }
        }
        
        //When view or edit is clicked, update all of the information based on the selected itme and tab
        private void updateAllInfo(object sender, RoutedEventArgs e)
        {
            if(tabName == "VideoTab")
            {
                RegularVideo video = VideoListBox.SelectedItem as RegularVideo;
                moreInfoTitle.Text = video.title;              
                titleTB.Text       = video.title;
                CategoryTB.Text    = video.category;
                NotesTB.Text       = video.notes;
                URLTB.Text         = video.url;
                PlatformTB.Text    = video.platform;
                ThumbnailTB.Text   = video.thumbNailUrl;
                DurationTB.Text    = video.durationFormatted;
                AuthorTB.Text      = video.author;
                IdTB.Text          = video.id;
                DescriptionTB.Text = video.description;
            }
            else if (tabName == "PlaylistTab")
            {

            }
        }

        //Save all of the data in the gui to the actual object
        private async void saveAllInfo(object sender, RoutedEventArgs e)
        {
            if(tabName == "VideoTab")
            {
                //Save all info fromt text to item
                RegularVideo video = VideoListBox.SelectedItem as RegularVideo;
                if (TimeSpan.TryParse(DurationTB.Text, out TimeSpan parsedDuration)) { video.duration = parsedDuration; }
                video.title        = titleTB.Text;
                video.category     = CategoryTB.Text;
                video.notes        = NotesTB.Text;
                video.url          = URLTB.Text;
                video.platform     = PlatformTB.Text;
                video.thumbNailUrl = ThumbnailTB.Text;
                video.author       = AuthorTB.Text;
                video.id           = IdTB.Text;
                video.description  = DescriptionTB.Text;

                //Save the content, and update UI to reflect the saving
                moreInfoSave.IsEnabled = false;
                moreInfoSave.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                moreInfoSave.Content = "Saving...";
                await jsonManagement.SaveRegularVideosAsync(contentManager.VideosArray);
                moreInfoSave.IsEnabled = true;
                moreInfoSave.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
                moreInfoSave.Content = "Save";
            }
            else if (tabName == "PlaylistTab")
            {

            }
            else if (tabName == "ShortsTab")
            {
                //Update the actual class item
                ShortsVideo video = ShortsListBox.SelectedItem as ShortsVideo;
                if (TimeSpan.TryParse(DurationTB.Text, out TimeSpan parsedDuration)) { video.duration = parsedDuration; }
                video.title        = titleTB.Text;
                video.category     = CategoryTB.Text;
                video.notes        = NotesTB.Text;
                video.url          = URLTB.Text;
                video.platform     = PlatformTB.Text;
                video.thumbNailUrl = ThumbnailTB.Text;
                video.author       = AuthorTB.Text;
                video.id           = IdTB.Text;
                video.description  = DescriptionTB.Text;

                //Save the content, and update UI to reflect the saving
                moreInfoSave.IsEnabled = false;
                moreInfoSave.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                moreInfoSave.Content = "Saving...";
                await jsonManagement.SaveShortsAsync(contentManager.ShortsArray);
                moreInfoSave.IsEnabled = true;
                moreInfoSave.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
            }
            else if (tabName == "OtherTab")
            {
                //Update the actual class item
                OtherVideo video = OtherListBox.SelectedItem as OtherVideo;
                if (TimeSpan.TryParse(DurationTB.Text, out TimeSpan parsedDuration)) { video.duration = parsedDuration; }
                video.title        = titleTB.Text;
                video.category     = CategoryTB.Text;
                video.notes        = NotesTB.Text;
                video.url          = URLTB.Text;
                video.platform     = PlatformTB.Text;
                video.thumbNailUrl = ThumbnailTB.Text;
                video.author       = AuthorTB.Text;
                video.id           = IdTB.Text;
                video.description  = DescriptionTB.Text;

                //Save the content, and update UI to reflect the saving
                moreInfoSave.IsEnabled = false;
                moreInfoSave.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                moreInfoSave.Content = "Saving...";
                await jsonManagement.SaveOtherVideosAsync(contentManager.OtherArray);
                moreInfoSave.IsEnabled = true;
                moreInfoSave.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7"));
            }
        }                               

    }
}
