using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {

        Dictionary<double, List<UIElement>> neuralNetwork = new Dictionary<double, List<UIElement>>();
        List<double> xPositions = new List<double>();
        Button _animatedButton;
        bool _loaded;

        public Page1()
        {
            InitializeComponent();
            this.Loaded += isLoaded;
            this.Unloaded += isUnloaded;

            //Only run when the application has loaded up already
            this.Loaded += (s, e) =>
            {
                mainGrid.UpdateLayout();
                var main = Window.GetWindow(this) as MainWindow;
                //Helps with not drawing the Neural Network Lines multiple times
                if (main != null && main.drawNN)
                {
                    initNeuralNetwork();
                    CreateNNLines();
                    addFunctions();
                }

            };
        }
    

        private void buttonClick(object sender, EventArgs e)
        {
            // Reset the button state immediately before navigation
            foreach (UIElement element in mainGrid.Children)
            {
                if (element is System.Windows.Controls.Button button)
                {
                    resetButtonImmediately(button);
                }
            }

            this.NavigationService.Navigate(new Page2());
        }

        private void CheckListManagerButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("You clicked the check Manager List button");
            foreach (UIElement element in mainGrid.Children)
            {
                if (element is System.Windows.Controls.Button button)
                {
                    resetButtonImmediately(button);
                }
            }

            this.NavigationService.Navigate(new CheckListManagerPage());
        }

        private void CreateNNLines()
        {
            var main = Window.GetWindow(this) as MainWindow;
            main.drawNN = false;

            Debug.WriteLine($"Total layers: {xPositions.Count}");
            for (int i = 0; i < xPositions.Count; i++)
            {
                Debug.WriteLine($"Layer {i}: X={xPositions[i]}, Nodes={neuralNetwork[xPositions[i]].Count}");
            }

            for (int i = 0; i < xPositions.Count - 1; i++)
            {
                Debug.WriteLine($"Connecting layer {i} to layer {i + 1}");

                List<UIElement> layerOne = neuralNetwork[xPositions[i]];
                List<UIElement> layerTwo = neuralNetwork[xPositions[i + 1]];

                foreach (UIElement element1 in layerOne)
                {
                    var button1 = element1 as FrameworkElement;
                    Point center1 = GetElementCenter(button1);

                    foreach (UIElement element2 in layerTwo)
                    {
                        //SolidColorBrush lineBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215));

                        var button2 = element2 as FrameworkElement;
                        Point center2 = GetElementCenter(button2);

                        Line redLine = new Line();
                        redLine.X1 = center1.X;
                        redLine.Y1 = center1.Y;
                        redLine.X2 = center2.X;
                        redLine.Y2 = center2.Y;
                        redLine.Stroke = Brushes.Black;
                        redLine.StrokeThickness = 2; // Reduced for better visibility

                        // Ensure lines are visible by setting ZIndex
                        Panel.SetZIndex(redLine, -1);

                        mainGrid.Children.Add(redLine);
                    }
                }
            }
        }

        private void addFunctions()
        {
            foreach (KeyValuePair<double, List<UIElement>> keyValuePair in neuralNetwork)
            {
                foreach (UIElement element in keyValuePair.Value)
                {
                    if (element is System.Windows.Controls.Button button)
                    {
                        button.MouseEnter += animatedButton_Hover;
                        button.MouseLeave += resetButton;
                        Debug.WriteLine("Made it before lastTwoDigits");
                        string lastTwoDigits = button.Name.Substring(button.Name.Length - 2);
                        Debug.WriteLine("Last Two Digits are: " + lastTwoDigits);
                        if (lastTwoDigits == "PN")      { button.Click += buttonClick; }
                        else if (lastTwoDigits == "CM") { button.Click += CheckListManagerButton_Click; }
                        else { button.Click += buttonClick; }
                    }
                }
            }
        }

        private void animatedButton_Hover(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                AnimateButton(button);
            }
        }

        private void AnimateButton(Button targetButton)
        {
            _animatedButton = targetButton;

            // Create a Storyboard to hold the animations
            Storyboard storyboard = new Storyboard();

            // Set the transform origin to center (0.5, 0.5)
            targetButton.RenderTransformOrigin = new Point(0.5, 0.5);

            // Ensure the button has a ScaleTransform
            if (targetButton.RenderTransform is not ScaleTransform)
            {
                targetButton.RenderTransform = new ScaleTransform(1, 1);
            }

            translucentBox.Visibility = Visibility.Visible;
            Panel.SetZIndex(translucentBox, 20);
            translucentBox.Opacity = 0;
            Panel.SetZIndex(targetButton, 20);

            // Create scale animations instead of width/height animations
            DoubleAnimation scaleXAnimation = new DoubleAnimation
            {
                From = 1.0, // Start from original scale
                To = 1.5, // Scale to 1.5 times the size
                Duration = TimeSpan.FromSeconds(0.2),
                AutoReverse = false,
            };

            DoubleAnimation scaleYAnimation = new DoubleAnimation
            {
                From = 1.0, // Start from original scale
                To = 1.5, // Scale to 1.5 times the size
                Duration = TimeSpan.FromSeconds(0.2),
                AutoReverse = false,
            };

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0, // Start from original scale
                To = 0.75, // Scale to 1.5 times the size
                Duration = TimeSpan.FromSeconds(0.2),
                AutoReverse = false,
            };


            // Set the target object and property for the animations
            Storyboard.SetTarget(scaleXAnimation, targetButton);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

            Storyboard.SetTarget(scaleYAnimation, targetButton);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

            Storyboard.SetTarget(opacityAnimation, translucentBox);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            // Add the scale animations to the storyboard
            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);
            storyboard.Children.Add(opacityAnimation);


            // Begin the storyboard
            storyboard.Begin();
        }

        private void resetButton(object sender, EventArgs e)
        {
            if (_animatedButton != null)
            {

                // Create reset storyboard
                Storyboard resetStoryboard = new Storyboard();

                // Reset scale animations
                DoubleAnimation scaleXAnimation = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                DoubleAnimation scaleYAnimation = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = 0.75, // Start from original scale
                    To = 0, // Scale to 1.5 times the size
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false,
                };

                _animatedButton.Background = Brushes.White;
                _animatedButton.BorderBrush = Brushes.Black;

                // Set targets and properties
                Storyboard.SetTarget(scaleXAnimation, _animatedButton);
                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

                Storyboard.SetTarget(scaleYAnimation, _animatedButton);
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

                Storyboard.SetTarget(opacityAnimation, translucentBox);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

                resetStoryboard.Children.Add(scaleXAnimation);
                resetStoryboard.Children.Add(scaleYAnimation);
                resetStoryboard.Children.Add(opacityAnimation);

                Panel.SetZIndex(translucentBox, -1);
                Panel.SetZIndex(_animatedButton, 5);

                resetStoryboard.Begin();

            }
        }

        private void resetButtonImmediately(Button button)
        {
            // Stop any ongoing animations
            button.BeginAnimation(Button.RenderTransformProperty, null);

            // Reset scale immediately
            if (button.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.ScaleX = 1.0;
                scaleTransform.ScaleY = 1.0;
            }

            // Reset Z-index and other properties
            Panel.SetZIndex(button, 0);
            button.Background = Brushes.White;
            button.BorderBrush = Brushes.Black;

            // Reset translucent box
            translucentBox.Visibility = Visibility.Collapsed;
            translucentBox.Opacity = 0;
            Panel.SetZIndex(translucentBox, -1);
        }
        private void initNeuralNetwork()
        {

            neuralNetwork.Clear();
            xPositions.Clear();

            foreach (UIElement element in mainGrid.Children)
            {
                if (element is System.Windows.Controls.Button button)
                {
                    Point relativePosition = button.TransformToAncestor(mainGrid).Transform(new Point(0, 0));

                    // Include the actual rendered position
                    double actualX = relativePosition.X + button.Margin.Left;
                    double actualY = relativePosition.Y + button.Margin.Top;

                    if (!neuralNetwork.ContainsKey(actualX))
                    {
                        List<UIElement> buttonList = new List<UIElement>();
                        neuralNetwork.Add(actualX, buttonList);
                    }
                    neuralNetwork[actualX].Add(element);
                }
            }

            xPositions.AddRange(neuralNetwork.Keys);
            xPositions.Sort();
        }

        private Point GetElementCenter(FrameworkElement element)
        {
            Point relativePosition = element.TransformToAncestor(mainGrid).Transform(new Point(0, 0));
            return new Point(
                relativePosition.X + element.ActualWidth / 2,
                relativePosition.Y + element.ActualHeight / 2
            );
        }

        private void isLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Have ran the LOADED function");
            Debug.WriteLine("Value of bool is: " + _loaded);
            Debug.WriteLine("Count of xPositions: " + xPositions.Count + " " + "Count of Neural Network: " + neuralNetwork.Count);
            //Aparently this is oppisite of what it intuitvely is
            _loaded = false;
        }

        private void isUnloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Have ran the UNLOADED function");
            Debug.WriteLine("Value of bool is: " + _loaded);
            Debug.WriteLine("Count of xPositions: " + xPositions.Count + " " + "Count of Neural Network: " + neuralNetwork.Count);
            //Aparently this is oppisite of what it intuitvely is
            _loaded = true;
        }


    }
}
