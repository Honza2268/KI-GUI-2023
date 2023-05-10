using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using QuickStart;
using Path = System.IO.Path;

namespace Fractals {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        DataContext dc = new DataContext();
        
        public MainWindow() {
            
            ReloadDLLs(@".\DLLs");

            InitializeComponent();
            DataContext = dc;

            dc.FractalOriginX = Canvas.Width / 2;
            dc.FractalOriginY = Canvas.Height / 2;
        }

        public void ReloadDLLs(string path) {
            foreach (var file in Directory.GetFiles(path).Where(path => path.EndsWith(".dll"))) {
                var fullPath = Path.GetFullPath(file);
                var a = Assembly.LoadFile(fullPath);
                LoadGeneratorsFromAssembly(a);
            }
        }

        public void LoadGeneratorsFromAssembly(Assembly assembly) {
            foreach (var fractalGenType in assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(FractalGenerator)) && !type.IsAbstract)) {
                FractalGenerator fg = Activator.CreateInstance(fractalGenType) as FractalGenerator;

                dc.FractalGenerators.Add(fg);
            }
        }
        
        private void SelectFractal_Click(object sender, RoutedEventArgs e) {
            var selected = FractalList.SelectedItem as FractalGenerator;
            if (selected == null) return;
            StatusMessage.Text = "Ready";
            dc.SelectedGenerator = selected;
        }

        private async void GenerateFractal_Click(object sender, RoutedEventArgs e) {
            if (dc.SelectedGenerator == null) {
                StatusMessage.Text = "No generator selected";
                return;
            }
            ControlsPanel.IsEnabled = false;

            Trace.WriteLine("Starting...");
            StatusMessage.Text = "Generating...";
            
            Canvas.Children.Clear();
            
            var shapeCount = 0;

            //await Task.Run(() => dc.SelectedGenerator.GenerateShapes(new Point(dc.FractalOriginX, dc.FractalOriginY)));

            float expShapeCount = (float)dc.SelectedGenerator.ShapeCount;

            await foreach (var shape in dc.SelectedGenerator.GenerateShapes(new Point(dc.FractalOriginX, dc.FractalOriginY))) {
                if (shape.Length < 4) continue; // missing coords

                SolidColorBrush brush = new();

                if (shape.Length >= 7) {
                    var r = (byte)shape[4];
                    var g = (byte)shape[5];
                    var b = (byte)shape[6];
                    brush.Color = Color.FromRgb(r, g, b);
                }
                else brush = Brushes.Black;


                await Dispatcher.InvokeAsync(() => {
                    var line = new Line() { X1 = shape[0], Y1 = shape[1], X2 = shape[2], Y2 = shape[3], Stroke = brush };
                    Canvas.Children.Add(line);
                }, System.Windows.Threading.DispatcherPriority.Input);
                //if (shapeCount % 100 == 0) await Task.Delay(1);

                shapeCount++;
                dc.ProgressValue = (int)(shapeCount / expShapeCount * 100);
            }

            StatusMessage.Text = "Done!";
            Trace.WriteLine("...Done!");
            
            ControlsPanel.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Canvas.Children.Clear();
        }

        private void GeneratorProperties_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            var s = sender as DataGrid;
            s.Dispatcher.BeginInvoke(new Action(s.Items.Refresh), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
