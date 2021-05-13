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

namespace CGHomework3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDrawMode = true;
        private bool isEditMode = false;
        private bool isDeleteMode = false;

        private bool isLineMode = false;
        private bool isPolygonMode = false;
        private bool isCircleMode = false;

        private bool wasClicked = false;
        private Point previousPoint = new Point(-1,-1);

        private List<Circle> circles = new List<Circle>();
        private List<Polygon> polygons = new List<Polygon>();
        private List<Line> lines = new List<Line>();

        private (int, bool, Point) lineChangeData;
        private (int, Point) circleChangeData;

       
        private Polygon temporaryPolygon= new Polygon();
        private (int,bool,int) polygonChangeData;

        
        public MainWindow()
        {
            InitializeComponent();
            
        }
        public BitmapSource InitializeImage(int width, int height)
        {
          
            // Calculate stride of source
            int stride = (width * PixelFormats.Bgra32.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * height;
            byte[] data = new byte[length];

          

            // Loop for inversion(alpha is skipped)
            for (int i = 0; i < length; i += 4)
            {
                data[i] = 0;
                data[i + 1] = 0;
                data[i + 2] = 0;
                data[i + 3] = 255;

            }

            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(
                width, height,
                96, 96, PixelFormats.Bgra32,
                null, data, stride);
        }

        private void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (mainImage.Source == null)
            {
                mainImage.Source = InitializeImage(Convert.ToInt32(imageGrid.ActualWidth), Convert.ToInt32(imageGrid.ActualHeight));
                //mainImage.Source = InitializeImage(200, 200);
            }

        }
        private bool wasLineVertexClicked(Point clicked, Point pointLine)
        {
            if (pointLine.X >= clicked.X -2.5 && pointLine.X <= clicked.X + 2.5 && pointLine.Y >= clicked.Y - 2.5 && pointLine.Y <= clicked.Y + 2.5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private (int, Point) findMyCircle(Point clicked)
        {
            var tmpPoint = new Point(-1, -1);
            int index = circles.FindIndex(a => a.Radius - 2.5 <=Rasterization.EuclideanDistance(clicked.X, clicked.Y,a.Center.X, a.Center.Y) && a.Radius + 2.5 >= Rasterization.EuclideanDistance(clicked.X, clicked.Y, a.Center.X, a.Center.Y));
            if (index != -1)
            {
                tmpPoint.X = clicked.X;
                tmpPoint.Y = clicked.Y;

            }



            return (index, tmpPoint);
        }
        private (int,bool,Point) findMyLine(Point clicked)
        {
            int index = -1;
            bool left = false;
           
            int leftIndex = lines.FindIndex(a => a.leftPoint.X >= clicked.X - 2.5 && a.leftPoint.X <= clicked.X + 2.5 && a.leftPoint.Y >= clicked.Y - 2.5 && a.leftPoint.Y <= clicked.Y + 2.5);
            int rightIndex = lines.FindIndex(a => a.rightPoint.X >= clicked.X - 2.5 && a.rightPoint.X <= clicked.X + 2.5 && a.rightPoint.Y >= clicked.Y - 2.5 && a.rightPoint.Y <= clicked.Y + 2.5);
            var tmpArray = lines.ToArray();
            var tmpPoint = new Point(-1,-1);
            if (leftIndex == -1 && rightIndex !=-1)
            {
                index = rightIndex;
                left = false;
                tmpPoint.X = tmpArray[index].rightPoint.X;
                tmpPoint.Y = tmpArray[index].rightPoint.Y;

            }
            else if(leftIndex != -1 && rightIndex == -1)
            {
                index = leftIndex;
                left = true;
                tmpPoint.X = tmpArray[index].leftPoint.X;
                tmpPoint.Y = tmpArray[index].leftPoint.Y;
            }

            return (index, left, tmpPoint);
        }

        private (int, bool, Point) findMyLine(Point clicked, List<Line> myLines)
        {
            int index = -1;
            bool left = false;

            int leftIndex = myLines.FindIndex(a => a.leftPoint.X >= clicked.X - 2.5 && a.leftPoint.X <= clicked.X + 2.5 && a.leftPoint.Y >= clicked.Y - 2.5 && a.leftPoint.Y <= clicked.Y + 2.5);
            int rightIndex = myLines.FindIndex(a => a.rightPoint.X >= clicked.X - 2.5 && a.rightPoint.X <= clicked.X + 2.5 && a.rightPoint.Y >= clicked.Y - 2.5 && a.rightPoint.Y <= clicked.Y + 2.5);
            
            var tmpArray = myLines.ToArray();
            var tmpPoint = new Point(-1, -1);
            if (rightIndex < leftIndex && rightIndex !=-1)
            {
                index = rightIndex;
                left = false;
                tmpPoint.X = tmpArray[index].rightPoint.X;
                tmpPoint.Y = tmpArray[index].rightPoint.Y;

            }
            else if (leftIndex < rightIndex && rightIndex != -1)
            {
                index = leftIndex;
                left = true;
                tmpPoint.X = tmpArray[index].leftPoint.X;
                tmpPoint.Y = tmpArray[index].leftPoint.Y;
            }

            return (index, left, tmpPoint);
        }
        private int findMyLineToMove(Point clicked, List<Line> myLines)
        {
            
            bool left = false;
            
            int index = myLines.FindIndex(a => Rasterization.EuclideanDistance(a.leftPoint.X, a.leftPoint.Y,clicked.X,clicked.Y)  >= Rasterization.EuclideanDistance(a.rightPoint.X, a.rightPoint.Y, clicked.X, clicked.Y)-2.5 &&
            Rasterization.EuclideanDistance(a.leftPoint.X, a.leftPoint.Y, clicked.X, clicked.Y) <= Rasterization.EuclideanDistance(a.rightPoint.X, a.rightPoint.Y, clicked.X, clicked.Y) + 2.5);


            return index;
        }
        private (int,bool,int) findMyPolygon(Point clicked)
        {
            int indexLines = -1;
            int indexPolygones = 0;
            foreach(var polygon in polygons)
            {
                
                var tmpLine = findMyLine(clicked, polygon.Lines);
                indexLines = tmpLine.Item1;
                    if (indexLines != -1)
                    {
                        return (indexLines, tmpLine.Item2, indexPolygones);
                    }
                indexPolygones++;
            }
            return (indexLines, false, indexPolygones);
        }
        private (int, int) findMyPolygonToMove(Point clicked)
        {
            int indexLines = -1;
            int indexPolygones = 0;
            foreach (var polygon in polygons)
            {

                var tmpLine = findMyLineToMove(clicked, polygon.Lines);
                indexLines = tmpLine;
                if (indexLines != -1)
                {
                    return (indexLines, indexPolygones);
                }
                indexPolygones++;
            }
            return (indexLines, indexPolygones);
        }
        private bool isNearFirst(Point clicked)
        {


            var tmpPoint = new Point(-1, -1);
            var tmpLine = temporaryPolygon.Lines.First();
            if(tmpLine.leftPoint.X >= clicked.X - 2.5 && tmpLine.leftPoint.X <= clicked.X + 2.5 && tmpLine.leftPoint.Y >= clicked.Y - 2.5 && tmpLine.leftPoint.Y <= clicked.Y + 2.5)
            {
                return true;
            }
            
            return false;
        }
        private Point findVertexClicked(Point clicked, Point pointLine)
        {
            if (pointLine.X >= clicked.X - 3 && pointLine.X <= clicked.X + 3 && pointLine.Y >= clicked.Y - 3 && pointLine.Y <= clicked.Y + 3)
            {
                return new Point(pointLine.X, pointLine.Y);
            }
            else
            {
                return new Point(-1, -1);
            }
        }
       
        private void redrawAllLines(Line tmpLine)
        {
            var newLine = new Line() { leftPoint = tmpLine.leftPoint, rightPoint = tmpLine.rightPoint, Color = tmpLine.Color, Thickness = tmpLine.Thickness };
            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)newLine.leftPoint.X, (int)newLine.leftPoint.Y, 
                (int)newLine.rightPoint.X, (int)newLine.rightPoint.Y, newLine.Color.R, newLine.Color.G, newLine.Color.B, newLine.Thickness);
            lines.Add(newLine);
        }

        private void plainPolygonsRedraw(Polygon tmpPolygon)
        {
            foreach(var line in tmpPolygon.Lines)
            {
                plainLinesRedraw(line);
            }
            
        }
        private void plainLinesRedraw(Line tmpLine)
        {
            var newLine = new Line() { leftPoint = tmpLine.leftPoint, rightPoint = tmpLine.rightPoint, Color = tmpLine.Color, Thickness = tmpLine.Thickness };
            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)newLine.leftPoint.X, (int)newLine.leftPoint.Y,
                (int)newLine.rightPoint.X, (int)newLine.rightPoint.Y, newLine.Color.R, newLine.Color.G, newLine.Color.B, newLine.Thickness);
            
        }
        private void whiteLinesDraw(Line tmpLine, Color color)
        {
            var newLine = new Line() { leftPoint = tmpLine.leftPoint, rightPoint = tmpLine.rightPoint, Color = color, Thickness = tmpLine.Thickness };
            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)newLine.leftPoint.X, (int)newLine.leftPoint.Y,
                (int)newLine.rightPoint.X, (int)newLine.rightPoint.Y, newLine.Color.R, newLine.Color.G, newLine.Color.B, newLine.Thickness);

        }
        private void redrawAllCircles(Circle tmpCircle)
        {
            var newCircle = new Circle() { Center = new Point(tmpCircle.Center.X, tmpCircle.Center.Y), Radius = tmpCircle.Radius, Color =tmpCircle.Color, Thickness = tmpCircle.Thickness };
            mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)tmpCircle.Center.X, (int)tmpCircle.Center.Y, tmpCircle.Radius,tmpCircle.Color.R, tmpCircle.Color.G, tmpCircle.Color.B, tmpCircle.Thickness);
            circles.Add(newCircle);
        }
        private void plainCirclesRedraw(Circle tmpCircle)
        {
            var newCircle = new Circle() { Center = new Point(tmpCircle.Center.X, tmpCircle.Center.Y), Radius = tmpCircle.Radius, Color = tmpCircle.Color, Thickness = tmpCircle.Thickness };
            mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)tmpCircle.Center.X, (int)tmpCircle.Center.Y, tmpCircle.Radius, tmpCircle.Color.R, tmpCircle.Color.G, tmpCircle.Color.B, tmpCircle.Thickness);
            
        }
        private void redrawLine(int index, bool left, Point clickedPixel,  byte red, byte green, byte blue, int thickness)
        {
            var tmpArray = lines.ToArray();
            var tmpLine = tmpArray[index];
            Point tmpPoint = new Point();
            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLine.leftPoint.X, (int)tmpLine.leftPoint.Y, (int)tmpLine.rightPoint.X, (int)tmpLine.rightPoint.Y, 0,0,0, thickness);
           
            if (!left)
            {
                tmpPoint.X = tmpLine.leftPoint.X;
                tmpPoint.Y= tmpLine.leftPoint.Y;
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpPoint.X, (int)tmpPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, red,green, blue, thickness);
            }
            else
            {
                tmpPoint.X = tmpLine.rightPoint.X;
                tmpPoint.Y = tmpLine.rightPoint.Y;
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source,(int)clickedPixel.X, (int)clickedPixel.Y, (int)tmpPoint.X, (int)tmpPoint.Y, red, green, blue, thickness);
            }
          
            
            var newLine = new Line() { leftPoint = new Point(tmpPoint.X, tmpPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };
            lines.RemoveAt(index);
            tmpArray = lines.ToArray();
            lines.Clear();
            lines.Add(newLine);
            for (int i =0; i< tmpArray.Length;i++ )
            {
                redrawAllLines(tmpArray[i]);
            }
           
          

            
            
        }
        private void redrawLine(int index, bool left, Point clickedPixel)
        {
            var tmpArray = lines.ToArray();
            var tmpLine = tmpArray[index];
            Point tmpPoint = new Point();
            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLine.leftPoint.X, (int)tmpLine.leftPoint.Y, (int)tmpLine.rightPoint.X, (int)tmpLine.rightPoint.Y, 0, 0, 0, tmpLine.Thickness);

            if (!left)
            {
                tmpPoint.X = tmpLine.leftPoint.X;
                tmpPoint.Y = tmpLine.leftPoint.Y;
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpPoint.X, (int)tmpPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, tmpLine.Color.R, tmpLine.Color.G, tmpLine.Color.B, tmpLine.Thickness);
            }
            else
            {
                tmpPoint.X = tmpLine.rightPoint.X;
                tmpPoint.Y = tmpLine.rightPoint.Y;
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)clickedPixel.X, (int)clickedPixel.Y, (int)tmpPoint.X, (int)tmpPoint.Y, tmpLine.Color.R, tmpLine.Color.G, tmpLine.Color.B, tmpLine.Thickness);
            }


            var newLine = new Line() { leftPoint = new Point(tmpPoint.X, tmpPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color =tmpLine.Color, Thickness =tmpLine.Thickness };
            lines.RemoveAt(index);
            tmpArray = lines.ToArray();
            lines.Clear();
            lines.Add(newLine);
            for (int i = 0; i < tmpArray.Length; i++)
            {
                redrawAllLines(tmpArray[i]);
            }





        }
        private void redrawLine(int index, bool left, Point clickedPixel, byte red, byte green, byte blue, int thickness, List<Line> mylines)
        {
            var tmpArray = mylines.ToArray();
            var tmpLine = tmpArray[index];
            Point tmpPoint = new Point();
            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLine.leftPoint.X, (int)tmpLine.leftPoint.Y, (int)tmpLine.rightPoint.X, (int)tmpLine.rightPoint.Y, 0, 0, 0, thickness);

            if (!left)
            {
                tmpPoint.X = tmpLine.leftPoint.X;
                tmpPoint.Y = tmpLine.leftPoint.Y;
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpPoint.X, (int)tmpPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, red, green, blue, thickness);
            }
            else
            {
                tmpPoint.X = tmpLine.rightPoint.X;
                tmpPoint.Y = tmpLine.rightPoint.Y;
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)clickedPixel.X, (int)clickedPixel.Y, (int)tmpPoint.X, (int)tmpPoint.Y, red, green, blue, thickness);
            }


            var newLine = new Line() { leftPoint = new Point(tmpPoint.X, tmpPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };
            mylines.RemoveAt(index);
            tmpArray = mylines.ToArray();
            mylines.Clear();
            mylines.Add(newLine);
            for (int i = 0; i < tmpArray.Length; i++)
            {
                redrawAllLines(tmpArray[i]);
            }





        }
        private void redrawCircle(int index, Point clickedPixel, byte red, byte green, byte blue, int thickness)
        {
            var tmpArray = circles.ToArray();
            var tmpCircle = tmpArray[index];
           

            var diffX = clickedPixel.X - circleChangeData.Item2.X;
            var diffY = clickedPixel.Y - circleChangeData.Item2.Y;

            mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)(tmpCircle.Center.X), (int)(tmpCircle.Center.Y), tmpCircle.Radius, 0, 0, 0, tmpCircle.Thickness);

            mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)(tmpCircle.Center.X+diffX),  (int)(tmpCircle.Center.Y+diffY), tmpCircle.Radius, tmpCircle.Color.R, tmpCircle.Color.G, tmpCircle.Color.B, tmpCircle.Thickness);


            var newCircle = new Circle() { Center = new Point(tmpCircle.Center.X + diffX, tmpCircle.Center.Y + diffY), Radius = tmpCircle.Radius, Color = tmpCircle.Color, Thickness = tmpCircle.Thickness };
            circles.RemoveAt(index);
            tmpArray = circles.ToArray();
            circles.Clear();
            circles.Add(newCircle);
            for (int i = 0; i < tmpArray.Length; i++)
            {
               redrawAllCircles(tmpArray[i]);
            }

        }
        private void movePolygon(int index, Point clickedPixel)
        {


            var tmpArray = polygons.ToArray();
            var mylines = tmpArray[index].Lines.ToArray();

            var diffX = clickedPixel.X - previousPoint.X;
            var diffY = clickedPixel.Y - previousPoint.Y;
            var white = Color.FromRgb(0, 0, 0);
            foreach (var line in mylines)
            {
                whiteLinesDraw(line, white);
            }
            for (int i = 0; i< mylines.Length;i++)
            {
                var newLeftLine = new Line()
                {
                    leftPoint = new Point(mylines[i].leftPoint.X + diffX, mylines[i].leftPoint.Y + diffY),
                    rightPoint = new Point(mylines[i].rightPoint.X + diffX, mylines[i].rightPoint.Y + diffY),
                    Color = mylines[i].Color,
                    Thickness = mylines[i].Thickness
                };
                mylines[i] = newLeftLine;
            }

            tmpArray[index].Lines.Clear();
            for(int i =0; i< mylines.Length;i++)
            {
                tmpArray[index].Lines.Add(mylines[i]);
            }
            polygons.Clear();
            foreach (var polygon in tmpArray)
            {
                plainPolygonsRedraw(polygon);
                polygons.Add(polygon);
            }

        }
        private void redrawEdge(int lineIndex, bool left, int polygonIndex, Point clickedPixel, byte red, byte green, byte blue, int thickness)
        {


            var tmpArray = polygons.ToArray();
            var mylines = tmpArray[polygonIndex].Lines.ToArray();

            var diffX = clickedPixel.X - previousPoint.X;
            var diffY = clickedPixel.Y - previousPoint.Y;
            var newLeftLine = new Line() { leftPoint = new Point(mylines[lineIndex].leftPoint.X +diffX, mylines[lineIndex].leftPoint.Y+diffY), 
                rightPoint = new Point(mylines[lineIndex].rightPoint.X+diffX, mylines[lineIndex].rightPoint.Y+diffY), 
                Color = Color.FromRgb(red, green, blue), Thickness = thickness };


            
            mylines[lineIndex] = newLeftLine;
            tmpArray[polygonIndex].Lines.Clear();

            for (int i = 0; i < mylines.Length; i++)
            {
                tmpArray[polygonIndex].Lines.Add(mylines[i]);
            }

            polygons.Clear();
            foreach (var polygon in tmpArray)
            {
                plainPolygonsRedraw(polygon);
                polygons.Add(polygon);
            }

        }
        private void circleWithDriffrentRadius(int index, Point clickedPixel, byte red, byte green, byte blue, int thickness)
        {
            var tmpArray = circles.ToArray();
            var tmpCircle = tmpArray[index];
            
            var diff = (int)Rasterization.EuclideanDistance(clickedPixel.X, clickedPixel.Y, circleChangeData.Item2.X, circleChangeData.Item2.Y);
            if (tmpCircle.Center.X - tmpCircle.Radius <= clickedPixel.X && tmpCircle.Center.X + tmpCircle.Radius >= clickedPixel.X &&
                tmpCircle.Center.Y - tmpCircle.Radius <= clickedPixel.Y && tmpCircle.Center.Y + tmpCircle.Radius >= clickedPixel.Y)
                diff = -diff;
            

            mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)(tmpCircle.Center.X), (int)(tmpCircle.Center.Y), tmpCircle.Radius, 0, 0, 0, tmpCircle.Thickness);

            mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)(tmpCircle.Center.X), (int)(tmpCircle.Center.Y), tmpCircle.Radius+diff, tmpCircle.Color.R, tmpCircle.Color.G, tmpCircle.Color.B, tmpCircle.Thickness);


            var newCircle = new Circle() { Center = new Point(tmpCircle.Center.X , tmpCircle.Center.Y ), Radius = tmpCircle.Radius+diff, Color = tmpCircle.Color, Thickness = tmpCircle.Thickness };
            circles.RemoveAt(index);
            tmpArray = circles.ToArray();
            circles.Clear();
            circles.Add(newCircle);
            for (int i = 0; i < tmpArray.Length; i++)
            {
                redrawAllCircles(tmpArray[i]);
            }

        }
        private void whiteCircleRedraw(int index,Color color)
        {
            var tmpArray = circles.ToArray();
            var tmpCircle = tmpArray[index];

            mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)(tmpCircle.Center.X), (int)(tmpCircle.Center.Y), tmpCircle.Radius, color.R, color.G, color.B, tmpCircle.Thickness);

           
          
           

        }

       
        private void redrawPolygone(int lineIndex, bool left, int polygonIndex,Point clickedPixel, byte red, byte green, byte blue, int thickness)
        {

            var tmpArray = polygons.ToArray();
            var mylines = tmpArray[polygonIndex].Lines.ToArray();
            var secondLineIndex = lineIndex;
            if (left)
            {
                secondLineIndex -= 1;
                if(secondLineIndex < 0)
                {
                    secondLineIndex = mylines.Length - 1;
                }
                var tmpLeftLine = mylines[secondLineIndex];
                var tmpRightLine = mylines[lineIndex];
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)tmpLeftLine.rightPoint.X, (int)tmpLeftLine.rightPoint.Y, 0, 0, 0, thickness);
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpRightLine.leftPoint.X, (int)tmpRightLine.leftPoint.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 0, 0, 0, thickness);

               // mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, 255, 255, 255, 5);
               // mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)clickedPixel.X, (int)clickedPixel.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 255, 255, 255, 5);


              
                var newLeftLine = new Line() { leftPoint = new Point(tmpLeftLine.leftPoint.X, tmpLeftLine.leftPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color=Color.FromRgb(red,green,blue), Thickness = thickness };
                var newRightLine = new Line() { leftPoint = new Point(clickedPixel.X, clickedPixel.Y), rightPoint = new Point(tmpRightLine.rightPoint.X, tmpRightLine.rightPoint.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };

                mylines[secondLineIndex] = newLeftLine;
                mylines[lineIndex] = newRightLine;

                tmpArray[polygonIndex].Lines.Clear();

                foreach (var tmp in mylines)
                {
                    tmpArray[polygonIndex].Lines.Add(tmp);
                }
                polygons.Clear();
                foreach (var polygon in tmpArray)
                {
                    plainPolygonsRedraw(polygon);
                    polygons.Add(polygon);
                }


            }
            else
            {
                secondLineIndex += 1;
                if (secondLineIndex == mylines.Length)
                {
                    secondLineIndex = 0;
                }
                var tmpRightLine = mylines[secondLineIndex];
                var tmpLeftLine = mylines[lineIndex];
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)tmpLeftLine.rightPoint.X, (int)tmpLeftLine.rightPoint.Y, 0, 0, 0, thickness);
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpRightLine.leftPoint.X, (int)tmpRightLine.leftPoint.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 0, 0, 0, thickness);
               

                //mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, 255, 255, 255, 5);
               // mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)clickedPixel.X, (int)clickedPixel.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 255, 255, 255, 5);
                
                var newLeftLine = new Line() { leftPoint = new Point(tmpLeftLine.leftPoint.X, tmpLeftLine.leftPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };
                var newRightLine = new Line() { leftPoint = new Point(clickedPixel.X, clickedPixel.Y), rightPoint = new Point(tmpRightLine.rightPoint.X, tmpRightLine.rightPoint.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };

                mylines[lineIndex] = newLeftLine;
                mylines[secondLineIndex] = newRightLine;

                tmpArray[polygonIndex].Lines.Clear();

                foreach (var tmp in mylines)
                {
                    tmpArray[polygonIndex].Lines.Add(tmp);
                }
                polygons.Clear();
                foreach (var polygon in tmpArray)
                {
                    plainPolygonsRedraw(polygon);
                    polygons.Add(polygon);
                }
            }


        }

        private void reshapePolygone(int lineIndex, bool left, int polygonIndex, Point clickedPixel, byte red, byte green, byte blue, int thickness)
        {

            var tmpArray = polygons.ToArray();
            var mylines = tmpArray[polygonIndex].Lines.ToArray();
            var secondLineIndex = lineIndex;
            if (left)
            {
                secondLineIndex -= 1;
                if (secondLineIndex < 0)
                {
                    secondLineIndex = mylines.Length - 1;
                }
                var tmpLeftLine = mylines[secondLineIndex];
                var tmpRightLine = mylines[lineIndex];
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)tmpLeftLine.rightPoint.X, (int)tmpLeftLine.rightPoint.Y, 0, 0, 0, thickness);
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpRightLine.leftPoint.X, (int)tmpRightLine.leftPoint.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 0, 0, 0, thickness);

                // mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, 255, 255, 255, 5);
                // mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)clickedPixel.X, (int)clickedPixel.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 255, 255, 255, 5);



                var newLeftLine = new Line() { leftPoint = new Point(tmpLeftLine.leftPoint.X, tmpLeftLine.leftPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };
                var newRightLine = new Line() { leftPoint = new Point(clickedPixel.X, clickedPixel.Y), rightPoint = new Point(tmpRightLine.rightPoint.X, tmpRightLine.rightPoint.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };

                mylines[secondLineIndex] = newLeftLine;
                mylines[lineIndex] = newRightLine;

                tmpArray[polygonIndex].Lines.Clear();

                foreach (var tmp in mylines)
                {
                    tmpArray[polygonIndex].Lines.Add(tmp);
                }
                polygons.Clear();
                foreach (var polygon in tmpArray)
                {
                    plainPolygonsRedraw(polygon);
                    polygons.Add(polygon);
                }


            }
            else
            {
                secondLineIndex += 1;
                if (secondLineIndex == mylines.Length)
                {
                    secondLineIndex = 0;
                }
                var tmpRightLine = mylines[secondLineIndex];
                var tmpLeftLine = mylines[lineIndex];
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)tmpLeftLine.rightPoint.X, (int)tmpLeftLine.rightPoint.Y, 0, 0, 0, thickness);
                mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpRightLine.leftPoint.X, (int)tmpRightLine.leftPoint.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 0, 0, 0, thickness);


                //mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLeftLine.leftPoint.X, (int)tmpLeftLine.leftPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, 255, 255, 255, 5);
                // mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)clickedPixel.X, (int)clickedPixel.Y, (int)tmpRightLine.rightPoint.X, (int)tmpRightLine.rightPoint.Y, 255, 255, 255, 5);

                var newLeftLine = new Line() { leftPoint = new Point(tmpLeftLine.leftPoint.X, tmpLeftLine.leftPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color = tmpRightLine.Color, Thickness = tmpRightLine.Thickness };
                var newRightLine = new Line() { leftPoint = new Point(clickedPixel.X, clickedPixel.Y), rightPoint = new Point(tmpRightLine.rightPoint.X, tmpRightLine.rightPoint.Y), Color = tmpRightLine.Color, Thickness = tmpRightLine.Thickness };

                mylines[lineIndex] = newLeftLine;
                mylines[secondLineIndex] = newRightLine;

                tmpArray[polygonIndex].Lines.Clear();

                foreach (var tmp in mylines)
                {
                    tmpArray[polygonIndex].Lines.Add(tmp);
                }
                polygons.Clear();
                foreach (var polygon in tmpArray)
                {
                    plainPolygonsRedraw(polygon);
                    polygons.Add(polygon);
                }
            }


        }
      
        private bool wasLineEdgeClicked(Point clicked, Point pointLine)
        {
            if (pointLine.X >= clicked.X - 2.5 && pointLine.X <= clicked.X + 2.5 && pointLine.Y >= clicked.Y - 2.5 && pointLine.Y <= clicked.Y + 2.5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void mainImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (btnDraw.IsChecked==true)
            {
                if (previousPoint==new Point(-1,-1))
                {
                    wasClicked = true;
                    previousPoint = new Point(e.GetPosition(mainImage).X, e.GetPosition(mainImage).Y);
                }
                else
                {
                    var clickedPixel = e.GetPosition(mainImage);
                    byte red = Convert.ToByte(txtRed.Text);
                    byte green = Convert.ToByte(txtGreen.Text);
                    byte blue = Convert.ToByte(txtBlue.Text);
                    int thickness = Convert.ToByte(txtBrush.Text);
                    if (isLineMode)
                    {
                        mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)previousPoint.X, (int)previousPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, red,green, blue,thickness);
                        var tmpLine = new Line() { leftPoint=new Point(previousPoint.X,previousPoint.Y), rightPoint= new Point(clickedPixel.X,clickedPixel.Y), Color=Color.FromRgb(red,green, blue),Thickness=thickness};
                        if(btnYesAntiAliasing.IsChecked==true)
                        {
                            mainImage.Source = Rasterization.AntiAliasinDecider((BitmapSource)mainImage.Source, (int)previousPoint.X, (int)previousPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, red, green, blue, thickness);
                        }
                        lines.Add(tmpLine);
                        wasClicked = false;
                        previousPoint = new Point(-1, -1);

                    }
                    else if (isCircleMode)
                    {
                        var radius = (int)Rasterization.EuclideanDistance(previousPoint.X, previousPoint.Y, clickedPixel.X, clickedPixel.Y);
                        mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)previousPoint.X, (int)previousPoint.Y, radius,red, green, blue,thickness);
                        var tmpCircle = new Circle() {Center=new Point(previousPoint.X, previousPoint.Y), Radius=radius, Color=Color.FromRgb(red, green, blue), Thickness = thickness};
                        circles.Add(tmpCircle);
                        wasClicked = false;
                        previousPoint = new Point(-1, -1);

                    }
                    else if (isPolygonMode)
                    {

                        if(temporaryPolygon.Lines == null)
                        {
                            var tmpLines = new List<Line>();
                            var tmpLine = new Line() { leftPoint = new Point(previousPoint.X, previousPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };
                            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)previousPoint.X, (int)previousPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, red, green, blue, thickness);
                            tmpLines.Add(tmpLine);
                            previousPoint = new Point(clickedPixel.X, clickedPixel.Y);
                            temporaryPolygon.Lines = tmpLines;
                            temporaryPolygon.Color = Color.FromRgb(red, green, blue);
                            temporaryPolygon.Thickness = thickness;
                        }
                        else if(!isNearFirst(clickedPixel))
                        {
                            var tmpLine = new Line() { leftPoint = new Point(previousPoint.X, previousPoint.Y), rightPoint = new Point(clickedPixel.X, clickedPixel.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };
                            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)previousPoint.X, (int)previousPoint.Y, (int)clickedPixel.X, (int)clickedPixel.Y, red, green, blue, thickness);

                            temporaryPolygon.Lines.Add(tmpLine);
                            previousPoint = new Point(clickedPixel.X, clickedPixel.Y);
                            temporaryPolygon.Color = Color.FromRgb(red, green, blue);
                            temporaryPolygon.Thickness = thickness;
                        }
                        else
                        {
                            var tmpNode = temporaryPolygon.Lines.First();
                            var tmpLine = new Line() { leftPoint = new Point(previousPoint.X, previousPoint.Y), rightPoint = new Point(tmpNode.leftPoint.X, tmpNode.leftPoint.Y), Color = Color.FromRgb(red, green, blue), Thickness = thickness };
                            mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)previousPoint.X, (int)previousPoint.Y, (int)tmpNode.leftPoint.X, (int)tmpNode.leftPoint.Y, red, green, blue, thickness);

                            temporaryPolygon.Lines.Add(tmpLine);
                           
                            temporaryPolygon.Color = Color.FromRgb(red, green, blue);
                            temporaryPolygon.Thickness = thickness;

                            polygons.Add(temporaryPolygon);
                            temporaryPolygon = new Polygon();
                            previousPoint = new Point(-1, -1);
                            wasClicked = false;
                        }
                        


                    }
                   
                    

                }
            }
            else if(btnEdit.IsChecked==true)
            {
                if (isLineMode)
                {
                    if(!wasClicked)
                    {
                        if (e.ChangedButton != MouseButton.Right)
                        {
                            var position = e.GetPosition(mainImage);
                           
                            

                            (int, bool, Point) tmpPoint = findMyLine(position);
                            if (tmpPoint.Item1!=-1)
                            {
                                wasClicked = true;
                                lineChangeData.Item1 = tmpPoint.Item1;
                                lineChangeData.Item2 = tmpPoint.Item2;
                                lineChangeData.Item3 = tmpPoint.Item3;
                            }
                           
                        }
                        else
                        {
                            var position = e.GetPosition(mainImage);
                            (int, bool, Point) tmpPoint = findMyLine(position);
                            if (tmpPoint.Item1 != -1)
                            {
                                var tmpArray = lines.ToArray();
                                byte red = Convert.ToByte(txtRed.Text);
                                byte green = Convert.ToByte(txtGreen.Text);
                                byte blue = Convert.ToByte(txtBlue.Text);
                                int thickness = Convert.ToByte(txtBrush.Text);
                               
                                
                                lines.Clear();
                                var white = Color.FromRgb(0, 0, 0);
                                whiteLinesDraw(tmpArray[tmpPoint.Item1], white);
                                tmpArray[tmpPoint.Item1].Color = Color.FromRgb(red, green, blue);
                                tmpArray[tmpPoint.Item1].Thickness = thickness;
                                foreach (var polygon in polygons)
                                {
                                    plainPolygonsRedraw(polygon);
                                }
                                foreach (var circle in circles)
                                {
                                    plainCirclesRedraw(circle);
                                }
                                foreach (var line in tmpArray)
                                {
                                    lines.Add(line);
                                    plainLinesRedraw(line);
                                }

                            }
                           
                        }
                        
                    }
                    else
                    {

                        var position = e.GetPosition(mainImage);
                       
                        redrawLine(lineChangeData.Item1, lineChangeData.Item2, position);
                        foreach (var polygon in polygons)
                        {
                            plainPolygonsRedraw(polygon);
                        }
                        foreach (var circle in circles)
                        {
                            plainCirclesRedraw(circle);
                        }
                        wasClicked = false;
                    }
                   
                }
                else if (isCircleMode)
                {
                    if (!wasClicked)
                    {
                        if (e.ChangedButton != MouseButton.Right)
                        {
                            var position = e.GetPosition(mainImage);
                           
                            
                            var tmpIndex = findMyCircle(position);
                            if (tmpIndex.Item1 != -1)
                            {
                                wasClicked = true;
                                circleChangeData.Item1 = tmpIndex.Item1;
                                circleChangeData.Item2 = tmpIndex.Item2;
                            }
                        }
                       
                        else
                        {
                            var position = e.GetPosition(mainImage);
                            var resultCircle = findMyCircle(position);
                            int index = resultCircle.Item1;
                            if (index != -1)
                            {
                                var tmpArray = circles.ToArray();
                                var tmpCircle = tmpArray[index];
                                
                                byte red = Convert.ToByte(txtRed.Text);
                                byte green = Convert.ToByte(txtGreen.Text);
                                byte blue = Convert.ToByte(txtBlue.Text);
                                int thickness = Convert.ToByte(txtBrush.Text);
                                mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)(tmpCircle.Center.X), (int)(tmpCircle.Center.Y), tmpCircle.Radius, 0, 0, 0, tmpCircle.Thickness);

                                tmpCircle.Color = Color.FromRgb(red, green, blue);
                                tmpCircle.Thickness = thickness;

                                foreach (var circle in circles)
                                {
                                    plainCirclesRedraw(circle);
                                }
                                foreach (var line in lines)
                                {
                                    plainLinesRedraw(line);

                                }
                                foreach (var polygon in polygons)
                                {
                                    plainPolygonsRedraw(polygon);
                                }
                            }


                        }


                    }
                    else
                    {
                        var position = e.GetPosition(mainImage);
                        byte red = Convert.ToByte(txtRed.Text);
                        byte green = Convert.ToByte(txtGreen.Text);
                        byte blue = Convert.ToByte(txtBlue.Text);
                        int thickness = Convert.ToByte(txtBrush.Text);
                        
                        redrawCircle(circleChangeData.Item1, position, red, green, blue, thickness);
                            
                        
                        foreach (var line in lines)
                        {
                            plainLinesRedraw(line);
                        }
                        foreach (var polygon in polygons)
                        {
                            plainPolygonsRedraw(polygon);
                        }
                        wasClicked = false;

                    }
                }
                else if (isPolygonMode)
                {
                    if (!wasClicked)
                    {
                        if (e.ChangedButton != MouseButton.Right)
                        {
                            var position = e.GetPosition(mainImage);

                            

                            var tmpPoint = findMyPolygon(position);
                            if (tmpPoint.Item1 != -1)
                            {
                                wasClicked = true;
                                polygonChangeData.Item1 = tmpPoint.Item1;
                                polygonChangeData.Item2 = tmpPoint.Item2;
                                polygonChangeData.Item3 = tmpPoint.Item3;
                            }

                        }
                        else
                        {
                            var position = e.GetPosition(mainImage);
                            var resultCircle = findMyPolygon(position);
                            int index = resultCircle.Item1;
                            if (index != -1)
                            {
                                var tmpArray = polygons[resultCircle.Item3].Lines;
                                byte red = Convert.ToByte(txtRed.Text);
                                byte green = Convert.ToByte(txtGreen.Text);
                                byte blue = Convert.ToByte(txtBlue.Text);
                                int thickness = Convert.ToByte(txtBrush.Text);
                                var white = Color.FromRgb(0, 0, 0);
                                var color = Color.FromRgb(red, green, blue);
                                foreach (var line in tmpArray)
                                {
                                    whiteLinesDraw(line, white);
                                    line.Color = color;
                                    line.Thickness = thickness;
                                }

                                foreach (var circle in circles)
                                {
                                    plainCirclesRedraw(circle);
                                }
                                foreach (var line in lines)
                                {
                                    plainLinesRedraw(line);

                                }
                                foreach (var polygon in polygons)
                                {
                                    plainPolygonsRedraw(polygon);
                                }
                            }
                            wasClicked = false;
                        }

                    }
                    else
                    {

                        var position = e.GetPosition(mainImage);
                        byte red = Convert.ToByte(txtRed.Text);
                        byte green = Convert.ToByte(txtGreen.Text);
                        byte blue = Convert.ToByte(txtBlue.Text);
                        int thickness = Convert.ToByte(txtBrush.Text);
                        var lineIndex = polygonChangeData.Item1;
                        var polygonIndex = polygonChangeData.Item3;
                        var left = polygonChangeData.Item2;
                       
                        
                        Point tmpPoint = new Point();
                        reshapePolygone(lineIndex, left, polygonIndex, position, red, green, blue, thickness);
                        foreach(var line in lines)
                        {
                            plainLinesRedraw(line);
                        }
                        foreach (var circle in circles)
                        {
                            plainCirclesRedraw(circle);
                        }
                        wasClicked = false;
                    }
                }

            }
            else if(btnDelete.IsChecked == true)
            {
                var position = e.GetPosition(mainImage);
                if (isLineMode)
                {
                    var resultLine = findMyLine(position);
                    int index = resultLine.Item1;
                    if (index != -1)
                    {
                        var tmpArray = lines.ToArray();
                        var tmpLine = tmpArray[resultLine.Item1];
                        mainImage.Source = Rasterization.LineDecider((BitmapSource)mainImage.Source, (int)tmpLine.leftPoint.X, (int)tmpLine.leftPoint.Y,
                         (int)tmpLine.rightPoint.X, (int)tmpLine.rightPoint.Y, 0, 0, 0, tmpLine.Thickness);
                        lines.RemoveAt(index);
                        tmpArray = lines.ToArray();
                        lines.Clear();

                        for (int i = 0; i < tmpArray.Length; i++)
                        {
                            redrawAllLines(tmpArray[i]);
                        }
                        foreach (var circle in circles)
                        {
                            plainCirclesRedraw(circle);
                        }
                        foreach(var polygon in polygons)
                        {
                            plainPolygonsRedraw(polygon);
                        }

                    }
                    
                   
                }
                else if(isCircleMode)
                {
                    var resultCircle = findMyCircle(position);
                    int index = resultCircle.Item1;
                    if(index!=-1)
                    {
                        var tmpArray = circles.ToArray();
                        var tmpCircle = tmpArray[index];
                        mainImage.Source = Rasterization.MidpointCircle((BitmapSource)mainImage.Source, (int)(tmpCircle.Center.X), (int)(tmpCircle.Center.Y), tmpCircle.Radius, 0, 0, 0, tmpCircle.Thickness);
                        circles.RemoveAt(index);

                        foreach (var circle in circles)
                        {
                            plainCirclesRedraw(circle);
                        }
                        foreach (var line in lines)
                        {
                            plainLinesRedraw(line);

                        }
                        foreach (var polygon in polygons)
                        {
                            plainPolygonsRedraw(polygon);
                        }
                    }
                   
                }
                else if(isPolygonMode)
                {
                    var resultCircle = findMyPolygon(position);
                    int index = resultCircle.Item1;
                    if (index != -1)
                    {
                        var tmpArray = polygons[resultCircle.Item3].Lines;
                        polygons.RemoveAt(resultCircle.Item3);
                        var white = Color.FromRgb(0, 0, 0);
                        foreach(var line in tmpArray)
                        {
                            whiteLinesDraw(line,white);
                        }
                        foreach (var circle in circles)
                        {
                            plainCirclesRedraw(circle);
                        }
                        foreach (var line in lines)
                        {
                            plainLinesRedraw(line);

                        }
                        foreach (var polygon in polygons)
                        {
                            plainPolygonsRedraw(polygon);
                        }
                    }

                }
                

                wasClicked = false;
                previousPoint = new Point(-1, -1);
            }
            else if(btnMove.IsChecked == true)
            {
                if (isPolygonMode)
                {
                    if (!wasClicked)
                    {
                        if (e.ChangedButton != MouseButton.Right)
                        {
                            var position = e.GetPosition(mainImage);


                            previousPoint = new Point(position.X, position.Y);
                            var tmpPoint = findMyPolygon(position);
                            if (tmpPoint.Item1 != -1)
                            {
                                wasClicked = true;
                                polygonChangeData.Item1 = tmpPoint.Item1;
                                polygonChangeData.Item2 = false;
                                polygonChangeData.Item3 = tmpPoint.Item3;
                            }

                        }

                    }
                    else
                    {

                        var position = e.GetPosition(mainImage);
                       
                        var lineIndex = polygonChangeData.Item1;
                        var polygonIndex = polygonChangeData.Item3;
                        var left = polygonChangeData.Item2;


                        Point tmpPoint = new Point();
                        movePolygon(polygonIndex, position);
                        foreach (var line in lines)
                        {
                            plainLinesRedraw(line);
                        }
                        foreach (var circle in circles)
                        {
                            plainCirclesRedraw(circle);
                        }
                        wasClicked = false;
                        previousPoint = new Point(-1, -1);
                    }
                }
            }
            else if(btnRadius.IsChecked == true)
            {
                if (isCircleMode)
                {
                    if (!wasClicked)
                    {
                        if (e.ChangedButton != MouseButton.Right)
                        {
                            var position = e.GetPosition(mainImage);


                            var tmpIndex = findMyCircle(position);
                            if (tmpIndex.Item1 != -1)
                            {
                                wasClicked = true;
                                circleChangeData.Item1 = tmpIndex.Item1;
                                circleChangeData.Item2 = tmpIndex.Item2;
                            }
                        }

                       
                    }
                    else
                    {
                        var position = e.GetPosition(mainImage);
                        byte red = Convert.ToByte(txtRed.Text);
                        byte green = Convert.ToByte(txtGreen.Text);
                        byte blue = Convert.ToByte(txtBlue.Text);
                        int thickness = Convert.ToByte(txtBrush.Text);

                        circleWithDriffrentRadius(circleChangeData.Item1, position, red, green, blue, thickness);



                        foreach (var line in lines)
                        {
                            plainLinesRedraw(line);
                        }
                        foreach (var polygon in polygons)
                        {
                            plainPolygonsRedraw(polygon);
                        }
                        wasClicked = false;

                    }
                }
            }
          
        }

       
        private void btnLine_Click(object sender, RoutedEventArgs e)
        {
            wasClicked = false;
            previousPoint = new Point(-1, -1);
            isLineMode = true;
            isPolygonMode = false;
            isCircleMode = false;
        }

        private void btnCircle_Click(object sender, RoutedEventArgs e)
        {
            wasClicked = false;
            previousPoint = new Point(-1, -1);
            isLineMode = false;
            isPolygonMode = false;
            isCircleMode = true;
        }

        private void btnPolygon_Click(object sender, RoutedEventArgs e)
        {
            wasClicked = false;
            previousPoint = new Point(-1, -1);
            isLineMode = false;
            isPolygonMode = true;
            isCircleMode = false;
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mainImage_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void mainImage_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            polygons.Clear();
            circles.Clear();
            lines.Clear();
            mainImage.Source = InitializeImage(Convert.ToInt32(imageGrid.ActualWidth), Convert.ToInt32(imageGrid.ActualHeight));
        }
    }
}
