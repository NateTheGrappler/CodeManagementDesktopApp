using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using AngleSharp.Dom;
using Microsoft.Win32;
//The extra imports needed for this:
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using Xabe.FFmpeg;
using FFMpegCore;
using FFMpegCore.Pipes;

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
        public RegularVideo FindVideo(int id)
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
        public PlayList FindPlaylist(int id)
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

        //All of the Data to store from the video itself
        public int      id               { get; set; }
        public string   title            { get; set; } = string.Empty;
        public string   category         { get; set; } = string.Empty;
        public string   notes            { get; set; } = string.Empty;
        public string   url              { get; set; } = string.Empty;
        public string   urlToDownload    { get; set; } = string.Empty;
        public string   thumbNailUrl     { get; set; } = string.Empty;
        public TimeSpan duration         { get; set; }
        public string   author           { get; set; } = string.Empty;
        public DateTime addedDate        { get; set; }
        public string   description      { get; set; } = string.Empty;
        public string   durationFormatted => duration.ToString(@"hh\:mm\:ss");
        public string   addedDateFormatted => addedDate.ToString("MMM dd, yyyy");

        public YoutubeClient youtube = new YoutubeClient();


        //------------------------Constructors--------------------------------
        public RegularVideo(string URL, string Category, string Notes)
        {
            //Set up all of the needed information in the constructor
            this.url = URL;
            this.category = Category;
            this.notes = Notes;
        }
        public RegularVideo() { }


        //-------------------------Functions for playing and also storing youtube videos--------------------
        public void playVideo(string urlInput)
        {

        }
        public async Task downloadVideo(string urlInput)
        {
            //get the stream manifest so we can grab audio n video
            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(urlInput);

            //Grab the infor of the audio and video
            //MuxedStreamInfo streamVideo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality(); DONT WORK

            //Get the Video and Audo Stream
            IVideoStreamInfo videoComponent = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
            IAudioStreamInfo audioComponent = (IAudioStreamInfo)streamManifest.GetAudioStreams().GetWithHighestBitrate();

            string outputPath = _dataPathVideos + this.title + ".mp4";

            //Create raw video and audio pipes
            var videoPipe = new StreamPipeSource(await youtube.Videos.Streams.GetAsync(videoComponent));
            var audioPipe = new StreamPipeSource(await youtube.Videos.Streams.GetAsync(audioComponent));

            //Combine using FFMpeg having input pipe, another pipe, then output at the specified location in Local Roaming Folder
            await FFMpegArguments.FromPipeInput(videoPipe).AddPipeInput(audioPipe).OutputToFile(outputPath,overwrite: true,options => options .WithVideoCodec("copy").WithAudioCodec("aac").WithFastStart()).ProcessAsynchronously();
        }

        public async Task updateVideoItems(string urlInput)
        {
            //Get Video data type
            Video video = await youtube.Videos.GetAsync(urlInput);

            //Set up all the data so that way it could be saved via the given link
            this.title = video.Title;
            this.description = video.Description;
            this.duration = (TimeSpan)video.Duration;
            this.author = video.Author.ChannelTitle;
            this.addedDate = DateTime.Now;
            this.url = video.Url;
            //TODO: Add in a way to make it so you see the thumbnails
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
        public DateTime addedDate       { get; set; }
        public string description       { get; set; } = string.Empty;
        public string durationFormatted => duration.ToString(@"hh\:mm\:ss");
        public string addedDateFormatted => addedDate.ToString("MMM dd, yyyy");
    }

    //Holds all of the information of a playlist, like category, #num of vids, idk stuff like that
    public class PlayList
    {
        public int id                  { get; set; }
        public string title            { get; set; } = string.Empty;
        public string url              { get; set; } = string.Empty;
        public string thumbNailUrl     { get; set; } = string.Empty;
        public int numOfVideos         { get; set; }
        public string platform         { get; set; } = string.Empty;
        public DateTime addedDate      { get; set; }
        public string description      { get; set; } = string.Empty;
        public string addedDateFormatted => addedDate.ToString("MMM dd, yyyy");
    }

    //Holds as many miscellianous pieces of information as possible
    public class OtherVideo
    {
        public int id { get; set; }
    }




    public partial class videoManager : Page
    {
        //Data Variables to store temporary information input by user
        public           string title       = "";
        public           string url         = "";
        public           string description = "";
        public           string category    = "";


        //Initialize all other needed objects
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
            else if(button.Name == "NewContentBack")
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

            //get tab that is selected and it's name
            if (e.Source is TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    tabName = selectedTab.Name; //assign name of tab for later use
                    this.createTabName = tabName;
                }
            }

            Debug.WriteLine(tabName);

            //Go through all of the checks to see which of the tabs to display
            if      (tabName == "CreateVideoTab")
            {
                Panel.SetZIndex(VideoInformationInput,     20);
                Panel.SetZIndex(PlaylistsInformationInput, 0);
                Panel.SetZIndex(ShortsInformationInput,    0);
                Panel.SetZIndex(OtherInformationInput,     0);

            }
            else if (tabName == "CreatePlaylistTab")
            {
                Panel.SetZIndex(VideoInformationInput,     0);
                Panel.SetZIndex(PlaylistsInformationInput, 20);
                Panel.SetZIndex(ShortsInformationInput,    0);
                Panel.SetZIndex(OtherInformationInput,     0);
            }
            else if (tabName == "CreateShortsTab")
            {
                Panel.SetZIndex(VideoInformationInput,     0);
                Panel.SetZIndex(PlaylistsInformationInput, 0);
                Panel.SetZIndex(ShortsInformationInput,    20);
                Panel.SetZIndex(OtherInformationInput,     0);
            }
            else if (tabName == "CreateOtherTab")
            {
                Panel.SetZIndex(VideoInformationInput,     0);
                Panel.SetZIndex(PlaylistsInformationInput, 0);
                Panel.SetZIndex(ShortsInformationInput,    0);
                Panel.SetZIndex(OtherInformationInput,     20);
            }
            else
            {
                Debug.WriteLine("ERROR");
            }
        }

        private async void addNewContent(object sender, RoutedEventArgs e)
        {

            if(createTabName == "CreateVideoTab")
            {
                RegularVideo video = new RegularVideo(LongVideoURL.Text.ToString(), "Whatever", "Dont Care");
                Debug.WriteLine(video.url);
                await video.downloadVideo(video.url);
                Debug.WriteLine("Download Successful?");

            }
        }
    }
}
