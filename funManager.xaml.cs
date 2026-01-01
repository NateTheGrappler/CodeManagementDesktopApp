using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool            gameover = false;
        private string          typeOfGame = "";
        private string[]        board      = { "", "", "", "", "", "", "", "", "" };
        private string[]        boardNames = { "TL", "TM", "TR", "ML", "MM", "MR", "BL", "BM", "BR" };


        public funManager()
        {
            InitializeComponent();
            InitButtons();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void InitButtons()
        {
            //Also disable all of the buttons
            TL.IsEnabled = false;
            TM.IsEnabled = false;
            TR.IsEnabled = false;
            ML.IsEnabled = false;
            MM.IsEnabled = false;
            MR.IsEnabled = false;
            BL.IsEnabled = false;
            BM.IsEnabled = false;
            BR.IsEnabled = false;
        }

        //----------------------------------The Side Button Functions------------------------------

        //--------------------------------The Functions for the Board------------------------------

        private void boardButtonClick(object sender, RoutedEventArgs e)             //Handles the base clicks for the user
        {

            if (sender is Button button)
            {

                //Determine whos turn it is
                string symbolType = "O";
                //if game type is PvP then just let it rock out as normal
                if(typeOfGame == "PvP")
                {
                    if (turnCount % 2 == 0)
                    {
                        symbolType = "X";
                        TurnText.Text = "---O's Turn---";
                    }
                    else
                    {
                        symbolType = "O";
                        TurnText.Text = "---X's Turn---";

                    }
                }
                //if it is ai, then depending on if ai turn, return and dont let rest of the function pass
                else if(typeOfGame == "PvR")
                {
                    if (turnCount % 2 == 0)
                    {
                        symbolType = "X";
                        TurnText.Text = "---O's Turn---";
                        //also enable all of the buttons
                        TL.IsEnabled = false;
                        TM.IsEnabled = false;
                        TR.IsEnabled = false;
                        ML.IsEnabled = false;
                        MM.IsEnabled = false;
                        MR.IsEnabled = false;
                        BL.IsEnabled = false;
                        BM.IsEnabled = false;
                        BR.IsEnabled = false;

                    }
                }

                //The Top Three Buttons
                if (button.Name == "TL" && board[0] == "")
                {
                    //Check the symbol type, and then update the visual based on it
                    if (symbolType == "X")
                    {
                        //set the image and it's color
                        topLeftRec.Fill = new SolidColorBrush(Colors.Red);
                        topLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[0] = "X";
                    }
                    else if (symbolType == "O")
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
                    if (symbolType == "X")
                    {
                        //set the image and color
                        topMidRec.Fill = new SolidColorBrush(Colors.Red);
                        topMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                        board[1] = "X";
                    }
                    else if (symbolType == "O")
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
                    else if (symbolType == "O")
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
                    else if (symbolType == "O")
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
                    else if (symbolType == "O")
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
                        board[5] = "X";
                    }
                    else if (symbolType == "O")
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
                        board[6] = "X";
                    }
                    else if (symbolType == "O")
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
                        board[7] = "X";
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
                        board[8] = "X";
                    }
                    else if (symbolType == "O")
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

            //also run a win condition check no matter what
            checkWinCondition();

            //if is AI turn
            if (typeOfGame == "PvR" && !gameover)
            {
                TurnText.Text = "---X's Turn---";
                //Play the Ai turn and then check for win
                aiTurn();
                checkWinCondition();
                TurnText.Text = "---O's Turn---";
                return;
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

            //enable to choice to choose the game type again
            PlayVsFriendButton.IsEnabled = true;
            PlayVsRobotButton.IsEnabled = true;

            //reset any background colors
            TL.Background = new SolidColorBrush(Colors.Transparent);
            TM.Background = new SolidColorBrush(Colors.Transparent);
            TR.Background = new SolidColorBrush(Colors.Transparent);
            ML.Background = new SolidColorBrush(Colors.Transparent);
            MM.Background = new SolidColorBrush(Colors.Transparent);
            MR.Background = new SolidColorBrush(Colors.Transparent);
            BL.Background = new SolidColorBrush(Colors.Transparent);
            BM.Background = new SolidColorBrush(Colors.Transparent);
            BR.Background = new SolidColorBrush(Colors.Transparent);

            //Also renable all of the buttons
            TL.IsEnabled = false;
            TM.IsEnabled = false;
            TR.IsEnabled = false;
            ML.IsEnabled = false;
            MM.IsEnabled = false;
            MR.IsEnabled = false;
            BL.IsEnabled = false;
            BM.IsEnabled = false;
            BR.IsEnabled = false;

            gameover = false;

        }

        private void pvpButtonClick(object sender, RoutedEventArgs e)               //Indicates this is a match between players
        {
            typeOfGame = "PvP";
            PlayVsFriendButton.IsEnabled = false;
            PlayVsRobotButton.IsEnabled = false;
            playTypeText.Text = "Match Type: Player vs Player";
            //also enable all of the buttons
            TL.IsEnabled = true;
            TM.IsEnabled = true;
            TR.IsEnabled = true;
            ML.IsEnabled = true;
            MM.IsEnabled = true;
            MR.IsEnabled = true;
            BL.IsEnabled = true;
            BM.IsEnabled = true;
            BR.IsEnabled = true;
        }

        private void pvrButtonClick(object sender, RoutedEventArgs e)               //Indicates this is a match between a robot and player
        {
            typeOfGame = "PvR";
            PlayVsFriendButton.IsEnabled = false;
            PlayVsRobotButton.IsEnabled = false;
            playTypeText.Text = "Match Type: Player vs Robot";

            //also enable all of the buttons
            TL.IsEnabled = true;
            TM.IsEnabled = true;
            TR.IsEnabled = true;
            ML.IsEnabled = true;
            MM.IsEnabled = true;
            MR.IsEnabled = true;
            BL.IsEnabled = true;
            BM.IsEnabled = true;
            BR.IsEnabled = true;
        }

        private void checkWinCondition()                                            //Compare array values to see if a win has happened
        {
            bool winDetected = false;

            //The horizontal ways to win
            if (board[0] == board[1] && board[1] == board[2] && board[0] != "")
            {
                //set the color of the squares to match that a win has happened
                TL.Background = new SolidColorBrush(Colors.Gold);
                TM.Background = new SolidColorBrush(Colors.Gold);
                TR.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;

            }
            else if (board[3] == board[4] && board[4] == board[5]  && board[3] != "")
            {
                ML.Background = new SolidColorBrush(Colors.Gold);
                MM.Background = new SolidColorBrush(Colors.Gold);
                MR.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;
            }
            else if (board[6] == board[7] && board[7] == board[8] && board[6] != "")
            {
                BL.Background = new SolidColorBrush(Colors.Gold);
                BM.Background = new SolidColorBrush(Colors.Gold);
                BR.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;
            }   

            //The vertical ways to win
            if (board[0] == board[3] && board[3] == board[6] && board[0] != "")
            {
                TL.Background = new SolidColorBrush(Colors.Gold);
                ML.Background = new SolidColorBrush(Colors.Gold);
                BL.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;
            }
            else if (board[1] == board[4] && board[4] == board[7] && board[1] != "")
            {
                TM.Background = new SolidColorBrush(Colors.Gold);
                MM.Background = new SolidColorBrush(Colors.Gold);
                BM.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;
            }
            else if (board[2] == board[5] && board[5] == board[8] && board[2] != "")
            {
                TR.Background = new SolidColorBrush(Colors.Gold);
                MR.Background = new SolidColorBrush(Colors.Gold);
                BR.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;
            }

            //The diagonal ways to win
            if (board[0] == board[4] && board[4] == board[8] && board[0] != "")
            {
                TL.Background = new SolidColorBrush(Colors.Gold);
                MM.Background = new SolidColorBrush(Colors.Gold);
                BR.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;
            }
            else if (board[2] == board[4] && board[4] == board[6] && board[2] != "")
            {
                TR.Background = new SolidColorBrush(Colors.Gold);
                MM.Background = new SolidColorBrush(Colors.Gold);
                BL.Background = new SolidColorBrush(Colors.Gold);
                //update the win condition
                winDetected = true;
            }

            if(winDetected || checkDrawCondition())
            {
                gameover = true;

                //update text to display a winner
                if(checkDrawCondition())     { TurnText.Text = "It's A Draw!!!";}


                //Also disable all of the buttons
                TL.IsEnabled = false;
                TM.IsEnabled = false;
                TR.IsEnabled = false;
                ML.IsEnabled = false;
                MM.IsEnabled = false;
                MR.IsEnabled = false;
                BL.IsEnabled = false;
                BM.IsEnabled = false;
                BR.IsEnabled = false;
            }
            if (winDetected)
            {
                if (turnCount % 2 != 0) { TurnText.Text = "Play X Wins!!!"; }
                else if (turnCount % 2 == 0) { TurnText.Text = "Play O Wins!!!"; }
            }
        }
    
        private async Task aiTurn()                                                       //randomly choose one of the empty boxes and place your piece there
        {
            await Task.Delay(1000);

            //conditions to not run the turn
            if(checkDrawCondition() || gameover || typeOfGame != "PvR") { return; }

            //set up vars
            bool foundMove = false;
            int     index = 0;
            Random  rand = new Random();

            //Set up a while loop to repeat random vals till one is found
            while(!foundMove)
            {
                //generate random value between 1 and 9
                int randVal = rand.Next(9);
                if (board[randVal] == "")
                {
                    //save the index on the board and end out of loop
                    index = randVal;
                    foundMove = true;
                }
            }

            if (boardNames[index] == "TL")
            {
                //set the image and color
                topLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                topLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[0] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "TM")
            {
                //set the image and color
                topMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                topMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[1] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "TR")
            {
                //set the image and color
                topRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                topRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[2] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "ML")
            {
                //set the image and color
                midLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                midLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[3] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "MM")
            {
                //set the image and color
                midMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                midMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[4] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "MR")
            {
                //set the image and color
                midRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                midRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[5] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "BL")
            {
                //set the image and color
                bottomLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                bottomLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[6] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "BM")
            {
                //set the image and color
                bottomMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                bottomMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[7] = "O";
                turnCount++;
            }
            else if (boardNames[index] == "BR")
            {
                //set the image and color
                bottomRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                bottomRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[8] = "O";
                turnCount++;
            }

            //also enable all of the buttons
            if(!gameover)
            {
                TL.IsEnabled = true;
                TM.IsEnabled = true;
                TR.IsEnabled = true;
                ML.IsEnabled = true;
                MM.IsEnabled = true;
                MR.IsEnabled = true;
                BL.IsEnabled = true;
                BM.IsEnabled = true;
                BR.IsEnabled = true;
            }

            checkWinCondition();
        }
    
        private bool checkDrawCondition()
        {
            foreach(string val in board)
            {
                if(val == "")
                {
                    return false;
                }
            }

            TurnText.Text = "It's a Draw!!!";
            return true;
        }
    }
}
