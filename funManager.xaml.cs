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
using AngleSharp.Dom;

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
        private List<string>    pastMoves = new List<string>();
        private List<string>    redoMoves = new List<string>();


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
        private void resetBoard(object sender, RoutedEventArgs e)                   //Clear all of the images off of the board
        {
            //Set the images of all the buttons to empty
            topLeftImage.ImageSource = null;
            topMidImage.ImageSource = null;
            topRightImage.ImageSource = null;
            midLeftImage.ImageSource = null;
            midMidImage.ImageSource = null;
            midRightImage.ImageSource = null;
            bottomLeftImage.ImageSource = null;
            bottomMidImage.ImageSource = null;
            bottomRightImage.ImageSource = null;

            //also reset the board:
            for (int i = 0; i < board.Length; i++)
            {
                board[i] = "";
            }

            //reset the indicatory texts
            TurnText.Text = "Nobody's Turn";
            playTypeText.Text = "Please Pick a Play Option";

            //reset the values for the class
            turnCount = 0;
            typeOfGame = "";
            gameover = false;
            pastMoves.Clear();


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


            RedoMoveButton.IsEnabled = false;
            redoButtonText.Foreground = new SolidColorBrush(Colors.Gray);
            UndoMoveButton.IsEnabled = false;
            undoButtonText.Foreground = new SolidColorBrush(Colors.Gray);
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

        private void undoMove(object sender, RoutedEventArgs e)                     //check through the list to undo the move and then 
        {
            //clear out the redo list before you add to it again and decrement turn
            redoMoves.Clear();
            turnCount -= 1;

            //get the last index in the moves list
            string location = pastMoves[pastMoves.Count-1];
            pastMoves.RemoveAt(pastMoves.Count - 1);

            //loop through all of the buttons in the grid
            foreach (UIElement child in ticTacToeGrid.Children)
            {
                //check each child to see if their name matches, then clear that button
                if(child is Button button)
                {
                    object content = new object();
                    //get a refrence to the content of the button
                    if(button.Name == location)
                    {
                       if(button.Content is StackPanel panel)
                       {
                           if (panel.Children[0] is Viewbox viewBox)
                           {
                                if(viewBox.Child is System.Windows.Shapes.Rectangle rectangle)
                                {
                                    if(rectangle.OpacityMask is ImageBrush brush)
                                    {
                                        //clear the brush
                                        brush.ImageSource = null;
                                        break;
                                    }
                                }
                           }
                       }
                    }
                }
            }

            //add in the move that was unmade to the redo list and update UI
            redoMoves.Add(location);
            RedoMoveButton.IsEnabled = true;
            redoButtonText.Foreground = new SolidColorBrush(Colors.White);

            //Also update the board value to be clear
            for(int i = 0; i<9; i++)
            {
                //find the spot of the board and then reset it
                if (boardNames[i] == location)
                {
                    board[i] = "";
                    break;
                }
            }

            //If there is no more moves, indicate so
            if(pastMoves.Count == 0)
            {
                UndoMoveButton.IsEnabled = false;
                undoButtonText.Foreground = new SolidColorBrush(Colors.Gray);
            }
            
            //update the UI
            if (turnCount % 2 == 0) { TurnText.Text = "---O's Turn---"; }
            else                    { TurnText.Text = "---X's Turn---"; }

        }
        
        private void redoMove(object sender, RoutedEventArgs e)
        {
            string symbolType = "";
            //Makes it an X
            if (turnCount % 2 == 0)
            {
                symbolType = "X";
                TurnText.Text = "---O's Turn---";
            }
            //Makes it a O
            else if (turnCount % 2 == 1)
            {
                symbolType = "O";
                TurnText.Text = "---X's Turn---";

            }

            //loop over the children
            foreach(UIElement child in ticTacToeGrid.Children)
            {
                //dumbass if logic because of how wpf works
                if(child is Button button)
                {
                    if (button.Content is StackPanel panel)
                    {
                        if (panel.Children[0] is Viewbox viewBox)
                        {
                            if (viewBox.Child is System.Windows.Shapes.Rectangle rectangle)
                            {
                                if (rectangle.OpacityMask is ImageBrush brush)
                                {
                                    if (button.Name == redoMoves[0] && symbolType == "X")
                                    {
                                        //Change the opacity's fill and image
                                        rectangle.Fill = new SolidColorBrush(Colors.Red);
                                        brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/xTicTacToe.png"));
                                    }
                                    else if (button.Name == redoMoves[0] && symbolType == "O")
                                    {
                                        //Change the opacity's fill and image
                                        rectangle.Fill = new SolidColorBrush(Colors.DarkBlue);
                                        brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Also then empty out the list after you are done, and disable the button
            RedoMoveButton.IsEnabled = false;
            redoButtonText.Foreground = new SolidColorBrush(Colors.Gray);

            //add in the move back over to the undo button and reenable
            pastMoves.Add(redoMoves[0]);
            if(UndoMoveButton.IsEnabled == false)
            {
                UndoMoveButton.IsEnabled = true;
                undoButtonText.Foreground = new SolidColorBrush(Colors.White);
            }

            turnCount += 1;
        }
        
        //--------------------------------The Functions for the Board------------------------------

        private void boardButtonClick(object sender, RoutedEventArgs e)             //Handles the base clicks for the user
        {

            //if there is a move to be done, enable the undo button
            if(pastMoves.Count > 0)
            {
                UndoMoveButton.IsEnabled = true;
                undoButtonText.Foreground = new SolidColorBrush(Colors.White);
            }
            if(RedoMoveButton.IsEnabled == true)
            {
                RedoMoveButton.IsEnabled = false;
                redoButtonText.Foreground = new SolidColorBrush(Colors.Gray);
                redoMoves.Clear();
            }

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
                        pastMoves.Add("TL");
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and it's color
                        topLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        topLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[0] = "O";
                        pastMoves.Add("TL");
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
                        pastMoves.Add("TM");

                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        topMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        topMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[1] = "O";
                        pastMoves.Add("TM");
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
                        pastMoves.Add("TR");
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        topRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        topRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[2] = "O";
                        pastMoves.Add("TR");
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
                        pastMoves.Add("ML");
                        board[3] = "X";
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        midLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        midLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        pastMoves.Add("ML");
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
                        pastMoves.Add("MM");
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        midMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        midMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[4] = "O";
                        pastMoves.Add("MM");
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
                        pastMoves.Add("MR");
                        board[5] = "X";
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        midRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        midRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        pastMoves.Add("MR");
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
                        pastMoves.Add("BL");
                        board[6] = "X";
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        bottomLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        bottomLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[6] = "O";
                        pastMoves.Add("BL");
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
                        pastMoves.Add("BM");
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        bottomMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        bottomMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[7] = "O";
                        pastMoves.Add("BM");

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
                        pastMoves.Add("BR");
                    }
                    else if (symbolType == "O")
                    {
                        //set the image and color
                        bottomRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                        bottomRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                        board[8] = "O";
                        pastMoves.Add("BR");
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
                UndoMoveButton.IsEnabled = false;
                undoButtonText.Foreground = new SolidColorBrush(Colors.Gray);
            }
            if (winDetected)
            {
                if (turnCount % 2 != 0) { TurnText.Text = "Play X Wins!!!"; }
                else if (turnCount % 2 == 0) { TurnText.Text = "Play O Wins!!!"; }
            }
        }
    
        private async Task aiTurn()                                                 //randomly choose one of the empty boxes and place your piece there
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
                pastMoves.Add("TL");
            }
            else if (boardNames[index] == "TM")
            {
                //set the image and color
                topMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                topMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[1] = "O";
                turnCount++;
                pastMoves.Add("TM");
            }
            else if (boardNames[index] == "TR")
            {
                //set the image and color
                topRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                topRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[2] = "O";
                turnCount++;
                pastMoves.Add("TR");
            }
            else if (boardNames[index] == "ML")
            {
                //set the image and color
                midLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                midLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[3] = "O";
                turnCount++;
                pastMoves.Add("ML");
            }
            else if (boardNames[index] == "MM")
            {
                //set the image and color
                midMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                midMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[4] = "O";
                turnCount++;
                pastMoves.Add("MM");
            }
            else if (boardNames[index] == "MR")
            {
                //set the image and color
                midRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                midRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[5] = "O";
                turnCount++;
                pastMoves.Add("MR");
            }
            else if (boardNames[index] == "BL")
            {
                //set the image and color
                bottomLeftRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                bottomLeftImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[6] = "O";
                turnCount++;
                pastMoves.Add("BL");
            }
            else if (boardNames[index] == "BM")
            {
                //set the image and color
                bottomMidRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                bottomMidImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[7] = "O";
                turnCount++;
                pastMoves.Add("BM");
            }
            else if (boardNames[index] == "BR")
            {
                //set the image and color
                bottomRightRec.Fill = new SolidColorBrush(Colors.DarkBlue);
                bottomRightImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/oTicTacToe.png"));
                //update the board and turncount
                board[8] = "O";
                turnCount++;
                pastMoves.Add("BR");
            }

            //also enable all of the buttons
            if (!gameover)
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
    
        private bool checkDrawCondition()                                           //Check if a draw happened and if so end game
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
