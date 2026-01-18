using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using CodeManagementSystem;

namespace CodeManagementSystem
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>

    public class NoteFile : INotifyPropertyChanged
    {
        private string _filePath;
        private string _fileName;
        private string _content;
        private string _oldFilePath; //neeeded in case a name change thingy occures, for clearing at least
        public NoteFile() { }

        public NoteFile(string name, string path)
        {
            fileName = name;
            filePath = path;
        }

        public NoteFile(string name, string path, string fileContent)
        {
            fileName = name;
            filePath = path;
            oldFilePath = path;
            content = fileContent;
        }

        public string filePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(filePath));
                }
            }
        }
        public string fileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(fileName));
                }
            }
        }
        public string content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(content));
                }
            }
        }
        public string oldFilePath
        {
            get { return _oldFilePath; }
            set
            {
                if (_oldFilePath != value)
                {
                    _oldFilePath = value;
                    OnPropertyChanged(nameof(oldFilePath));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class noteViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<NoteFile> _noteList = new ObservableCollection<NoteFile>();

        public ObservableCollection<NoteFile> noteList
        {
            get => _noteList;
            set
            {
                if (_noteList != value)
                {
                    _noteList = value;
                    OnPropertyChanged(nameof(noteList));
                }
            }
        }

        public noteViewModel()
        {

            if (_noteList == null)
                _noteList = new ObservableCollection<NoteFile>();
        }

        public void AddNewItem(string name, string path, string content)
        {
            noteList.Add(new NoteFile { fileName = name, filePath = path, content = content });
        }

        public void RemoveItem(NoteFile item)
        {
            noteList.Remove(item);
        }

        public NoteFile GetNoteFile(string name)
        {
            foreach (NoteFile note in noteList)
            {
                if (note.fileName == name)
                {
                    return note;
                }
            }
            return new NoteFile("Empty", "Empty");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


    public partial class Page2 : Page
    {
        private string searchHint = "Search...";
        private noteViewModel _noteViewModel;
        private NoteFile highlightedNote;
        private TextBox currentEditTextbox;
        private string ogName;
        private int numUntitled = 0;
        public Page2()
        {
            InitializeComponent();
            initAppFolder();
            initNoteViewModel();
            initListBox();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            saveAllFilesToFolder();
            this.NavigationService.GoBack();
        }
        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("Change has occured in the text box");
            TextRange textRange = new TextRange(richtextBox.Document.ContentStart, richtextBox.Document.ContentEnd);
            if(highlightedNote != null)
            {
                Debug.Write("This is the content of the highlightedNote: " + highlightedNote.content);
                highlightedNote.content = textRange.Text;
            }
        }
        private void searchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == searchHint)
            {
                searchTextBox.Text = string.Empty;
            }
        }
        private void searchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                searchTextBox.Text = searchHint;
            }
        }
        private void initNoteViewModel()
        {
            _noteViewModel = new noteViewModel();
            this.DataContext = _noteViewModel;
        }
        private void initListBox()
        {
            noteListBox.ItemsSource = _noteViewModel.noteList;
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderName = "CodeInformationManagingSystem\\NotesFiles";
            string folderPath = System.IO.Path.Combine(localAppDataPath, appFolderName);

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);

                foreach (string filePath in files)
                {

                    string fileName = System.IO.Path.GetFileName(filePath);
                    string content = File.ReadAllText(filePath);
                    Debug.WriteLine("Filepath: " + filePath);
                    if (string.IsNullOrEmpty(content))
                    {
                        Debug.WriteLine("Content was empty");
                    }
                    _noteViewModel.AddNewItem(fileName.Substring(0, fileName.Length - 4), filePath, content);

                }
            }
        }
        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Click On New Button");
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderName = "CodeInformationManagingSystem\\NotesFiles\\";
            string folderPath = System.IO.Path.Combine(localAppDataPath, appFolderName);

            foreach (NoteFile notefile in _noteViewModel.noteList)
            {
                if (notefile.fileName.Substring(0, 8) == "Untitled")
                {
                    numUntitled += 1;
                }
            }

            _noteViewModel.AddNewItem("Untilted" + numUntitled, folderPath + "Untilted" + numUntitled + ".txt", "Start a new note");
            numUntitled += 1;
            saveAllFilesToFolder();
        }
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var noteFile = button?.DataContext as NoteFile;
            if (noteFile != null)
            {
                _noteViewModel.RemoveItem(noteFile);
            }
            highlightedNote = null;
            saveAllFilesToFolder();
        }
        private void duplicateButton_Click(object sender, RoutedEventArgs e)
        {
            if (highlightedNote != null)
            {
                string name = highlightedNote.fileName;
                string filepath = highlightedNote.filePath.Substring(0, highlightedNote.filePath.Length - 4) + " - Copy.txt";
                _noteViewModel.AddNewItem(name + " - Copy", filepath, highlightedNote.content);
                highlightedNote = null;
                if (checkIfNameInList(name + " - Copy"))
                {
                    Debug.WriteLine("NAME IS IN THE LIST WITH THE COPY");
                }
            }
        }
        private void noteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NoteFile selectedFile = noteListBox.SelectedItem as NoteFile;
            if (selectedFile != null)
            {
                highlightedNote = selectedFile;
            }
        }
        private void renameButtonList_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Clicked on Rename in List");
            var button = sender as Button;

            var listViewItem = FindVisualParent<ListBoxItem>(button);
            if (listViewItem != null)
            {
                var textBox = FindVisualChild<TextBox>(listViewItem);
                if (textBox != null)
                {
                    Debug.Print("TextBox found");
                    currentEditTextbox = textBox;
                    ogName = textBox.Text;
                    textBox.IsEnabled = true;
                    textBox.SelectAll();
                    textBox.Focus();
                }
            }
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

        //Basically just handle the name changing within the listbox
        private void nameTextBoxList_LostFocus(object sender, RoutedEventArgs e)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderName = "CodeInformationManagingSystem\\NotesFiles\\";
            string folderPath = System.IO.Path.Combine(localAppDataPath, appFolderName);

            currentEditTextbox.IsEnabled = false;
            if (string.IsNullOrWhiteSpace(currentEditTextbox.Text))
            {
                currentEditTextbox.Text = ogName;
            }
            highlightedNote.fileName = currentEditTextbox.Text;
            highlightedNote.filePath = folderPath + highlightedNote.fileName + ".txt";
            currentEditTextbox = null;
        }

        //Open the actual text here
        private void noteListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            saveAllFilesToFolder();
            if (noteListBox.SelectedItem != null)
            {
                Debug.WriteLine("Ran here babyyy");

                var note = noteListBox.SelectedItem as NoteFile;
                titleTextBox.Text = note.fileName;

                // Temporarily disable TextChanged event to prevent interference
                richtextBox.TextChanged -= RichTextBox_TextChanged;

                // Properly clear the RichTextBox
                richtextBox.Document.Blocks.Clear();

                // Create and add new content
                Paragraph paragraph = new Paragraph();
                Run run = new Run(note.content);
                paragraph.Inlines.Add(run);
                richtextBox.Document.Blocks.Add(paragraph);

                Debug.WriteLine("Note content on double click: " + note.content);

                // Set the current note and re-enable TextChanged event
                highlightedNote = note;
                richtextBox.TextChanged += RichTextBox_TextChanged;
            }

        }

        //Focus on the title above rich text box to change name
        private void noteTitleChangeButton_Click(object sender, RoutedEventArgs e)
        {
            titleTextBox.IsEnabled = true;
            titleTextBox.Focus();
            titleTextBox.SelectAll();
            if (ogName != null) { ogName = titleTextBox.Text; };
        }

        //When the title box above the richtextbox is lost focus, disable it, and either rename the focused element in list or create new
        private void titleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            titleTextBox.IsEnabled = false;
            if (checkIfNameInList(titleTextBox.Text))
            {
                return;
            }
            if (string.IsNullOrEmpty(titleTextBox.Text))
            {
                titleTextBox.Text = ogName;
            }

            if (highlightedNote != null)
            {
                highlightedNote.fileName = titleTextBox.Text;
            }
            else
            {
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolderName = "CodeInformationManagingSystem\\NotesFiles";
                string folderPath = System.IO.Path.Combine(localAppDataPath, appFolderName);

                TextRange textRange = new TextRange(richtextBox.Document.ContentStart, richtextBox.Document.ContentEnd);

                Debug.WriteLine("This is the content of the richTextBox: " + textRange.Text);

                _noteViewModel.AddNewItem(titleTextBox.Text, folderPath + titleTextBox.Text + ".txt", textRange.Text);
            }
            highlightedNote = _noteViewModel.GetNoteFile(titleTextBox.Text);
        }

        //Cycle through the list of noteFile objects to see if one already exists with that name in the array
        private bool checkIfNameInList(string name)
        {
            foreach (NoteFile note in _noteViewModel.noteList)
            {
                if (note.fileName == name)
                {
                    return true;
                }
            }
            return false;
        }

        //Create the app folder where all of the text files will be stored at basically
        private void initAppFolder()
        {

            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string appFolderName = "CodeInformationManagingSystem";

            string appFolderPath = System.IO.Path.Combine(localAppDataPath, appFolderName);

            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }

            string subFolderName = "NotesFiles";
            string subFolderPath = System.IO.Path.Combine(appFolderPath, subFolderName);
            if (!Directory.Exists(subFolderPath))
            {
                Directory.CreateDirectory(subFolderPath);
            }
        }

        private void saveAllFilesToFolder()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderName = "CodeInformationManagingSystem\\NotesFiles\\";
            string folderPath = System.IO.Path.Combine(localAppDataPath, appFolderName);

            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                try
                {
                    File.Delete(filePath);
                    Debug.WriteLine($"Deleted file: {filePath}");
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"Error deleting file {filePath}: {ex.Message}");
                }
            }


            foreach (NoteFile note in _noteViewModel.noteList)
            {
                File.WriteAllText(note.filePath, note.content);
                Debug.WriteLine("Saved file and content to filepath: " + note.filePath);
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            saveAllFilesToFolder();
        }

        private void richtextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            saveAllFilesToFolder();
        }

        private void roamingAppsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderName = "CodeInformationManagingSystem\\NotesFiles";
            string folderPath = System.IO.Path.Combine(localAppDataPath, appFolderName);
            Process.Start("explorer.exe", folderPath);
        }
    }
}

