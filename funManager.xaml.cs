using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// <summary>
    /// Interaction logic for funManager.xaml
    /// </summary>
    public partial class funManager : Page
    {

        private int             turnCount  = 0;
        private string          typeOfGame = "";
        private string[]        board = { "", "", "", "", "", "", "", "", "" };

        public funManager()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        //----------------------------------The Side Button Functions------------------------------

        //--------------------------------The Functions for the Board------------------------------

        private void boardButtonClick(object sender, RoutedEventArgs e)             //Handles the base clicks for the user
        {

            if (sender is Button button && typeOfGame=="PvP")
            {

                //Determine whos turn it is
                string symbolType = "O";
                if (turnCount % 2 == 0)
                {
                    symbolType = "X";
                    TurnText.Text = "---X's Turn---";
                }
                else
                {
                    symbolType = "O";
                    TurnText.Text = "---O's Turn---";

                }

                //The Top Three Buttons
                if (button.Name == "TL" && board[0] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if(symbolType == "X")
                    {
                        //set the image and it's color
                        topLeftRec.Fill = new SolidColorBrush(Colors.Red);
                        topLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[0] = "X";
                    }
                    else if(symbolType == "O")
                    {
                        //set the image and it's color
                        topLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        topLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[0] = "O";
                    }
                    //update the turn count to switch turns
                    turnCount += 1;
                }
                else if (button.Name == "TM" && board[1] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if(symbolType == "X")
                    {
                        //set the image and color
                        topMidRec.Fill = new SolidColorBrush(Colors.Red);
                        topMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[1] = "X";
                    }
                    else if(symbolType == "O")
                    {
                        //set the image and color
                        topMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        topMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[1] = "O";
                    }
                    //update the turn count 
                    turnCount += 1;

                }
                else if (button.Name == "TR" && board[2] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X")
                    {
                        //set the image and color
                        topRightRec.Fill = new SolidColorBrush(Colors.Red);
                        topRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[2] = "X";
                    }
                    else if(symbolType == "O")
                    {
                        //set the image and color
                        topRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        topRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[2] = "O";
                    }
                    //update the turn count 
                    turnCount += 1;
                }

                //The Middle Three Buttons
                else if (button.Name == "ML" && board[3] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X")
                    {
                        //set the image and color
                        midLeftRec.Fill = new SolidColorBrush(Colors.Red);
                        midLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[3] = "X";
                    }
                    else if(symbolType == "O")
                    {
                        //set the image and color
                        midLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        midLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[3] = "O";
                    }

                    //update turn count
                    turnCount += 1;

                }
                else if (button.Name == "MM" && board[4] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X")
                    {
                        //set the image and color
                        midMidRec.Fill = new SolidColorBrush(Colors.Red);
                        midMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[4] = "X";
                    }
                    else if(symbolType == "O")
                    {
                        //set the image and color
                        midMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        midMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[4] = "O";
                    }

                    //update turn count
                    turnCount += 1;
                }
                else if (button.Name == "MR" && board[5] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X")
                    {
                        //set the image and color
                        midRightRec.Fill = new SolidColorBrush(Colors.Red);
                        midRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[5] = "O";
                    }
                    else if(symbolType == "O")
                    {
                        //set the image and color
                        midRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        midRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[5] = "O";
                    }
                    //update turn count
                    turnCount += 1;
                }
                
                //The bottom three buttons
                else if (button.Name == "BL" && board[6] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X")
                    {
                        //set the image and color
                        bottomLeftRec.Fill = new SolidColorBrush(Colors.Red);
                        bottomLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[6] = "O";
                    }
                    else if(symbolType == "O") 
                    {
                        //set the image and color
                        bottomLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        bottomLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[6] = "O";
                    }
                    //update turn count
                    turnCount += 1;
                }
                else if (button.Name == "BM" && board[7] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X") 
                    {
                        //set the image and color
                        bottomMidRec.Fill = new SolidColorBrush(Colors.Red);
                        bottomMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[7] = "O";
                    }
                    else if (symbolType == "O") 
                    {
                        //set the image and color
                        bottomMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        bottomMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[7] = "O";
                    }
                    //update turn count
                    turnCount += 1;
                }
                else if (button.Name == "BR" && board[8] == "") 
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X")
                    {
                        //set the image and color
                        bottomRightRec.Fill = new SolidColorBrush(Colors.Red);
                        bottomRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[8] = "O";
                    }
                    else if(symbolType == "O")
                    {
                        //set the image and color
                        bottomRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        bottomRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[8] = "O";
                    }
                    //update turn count
                    turnCount += 1;

                }
            }
        }

        private void resetBoard(object sender, RoutedEventArgs e)                   //Clear all of the images off of the board
        {
            //Set the images of all the buttons to empty
            topLeftImage.ImageSource     = null;
            topMidImage.ImageSource      = null;
            topRightImage.ImageSource    = null;
            midLeftImage.ImageSource     = null;
            midMidImage.ImageSource      = null;
            midRightImage.ImageSource    = null;
            bottomLeftImage.ImageSource  = null;
            bottomMidImage.ImageSource   = null;
            bottomRightImage.ImageSource = null;

            //also reset the board:
            for(int i = 0; i<board.Length; i++)
            {
                board[i] = "";
            }

            //reset the indicatory texts
            TurnText.Text = "Nobody's Turn";
            playTypeText.Text = "Please Pick a Play Option";

            //reset the values for the class
            turnCount = 0;
            typeOfGame = "";

        }

        private void pvpButtonClick(object sender, RoutedEventArgs e)               //Indicates this is a match between players
        {
            typeOfGame = "PvP";
            PlayVsFriendButton.IsEnabled = false;
            PlayVsRobotButton.IsEnabled = false;
            playTypeText.Text = "Match Type: Player vs Player";
        }

        private void pvrButtonClick(object sender, RoutedEventArgs e)              //Indicates this is a match between a robot and player
        {
            typeOfGame = "PvR";
            PlayVsFriendButton.IsEnabled = false;
            PlayVsRobotButton.IsEnabled = false;
            playTypeText.Text = "Match Type: Player vs Robot";
        }

    }
}
