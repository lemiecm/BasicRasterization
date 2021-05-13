using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CGHomework3
{
    public static class Rasterization
    {
        public static void drawLine(int x, int y, Color color, int stride, int width, int height, byte[] data, int thickness)
        {
            if (thickness > 1)
            {
                int matrixSize = thickness;
                
                int[,] kernelBrush = CreateBrushKernel(matrixSize);
                //int[,] kernelBrush = CreateBrushMatrixSize(matrixSize);
                int f_x = (thickness - 1) / 2;
                for (int j = -f_x; j <= f_x; ++j)
                    for (int k = -f_x; k <= f_x; ++k)
                        if(kernelBrush[j + f_x, k + f_x]==1)
                            putPixel(x + k, y + j, color, stride, width, height, data);
            }
            else
                putPixel(x, y, color, stride, width, height, data);


        }
        
        public static void putPixel(int x, int y, Color color, int stride, int width, int height, byte[] data)
        {
            if (isInPicture(x,  width) && isInPicture(y, height))
            {
                try
                {
                    data[4 * x + y * stride] = color.B;
                    data[4 * x + 1 + y * stride] = color.G;
                    data[4 * x + 2 + y * stride] = color.R;
                }
                catch (IndexOutOfRangeException e)  // CS0168
                {
                    
                }
               
            }
            

        }
        public static void drawCircle(int xc, int yc, int x, int y, Color color, int stride, int width, int height, byte[] data)
        {
            putPixel(xc + x, yc + y, color, stride, width,height, data);
            putPixel(xc - x, yc + y, color, stride, width, height, data);
            putPixel(xc + x, yc - y, color, stride, width, height, data);
            putPixel(xc - x, yc - y, color, stride, width, height, data);
            putPixel(xc + y, yc + x, color, stride, width, height, data);
            putPixel(xc - y, yc + x, color, stride, width, height, data);
            putPixel(xc + y, yc - x, color, stride, width, height, data);
            putPixel(xc - y, yc - x, color, stride, width, height, data);
        }
        public static void drawThickCircle(int xc, int yc, int x, int y, Color color, int stride, int width, int height, byte[] data, int thickness)
        {
            if (thickness > 1)
            {
                int matrixSize = thickness;

                //int[,] kernelBrush = CreateBrushKernel(matrixSize);
                
                int[,] kernelBrush = CreateBrushMatrixSize(matrixSize);
                int f_x = (thickness - 1) / 2;
                for (int j = -f_x; j <= f_x; ++j)
                    for (int k = -f_x; k <= f_x; ++k)
                        if (kernelBrush[j + f_x, k + f_x] == 1)
                            drawCircle(xc,yc,x + k, y + j, color, stride, width, height, data);
            }
            else
                drawCircle(xc, yc, x, y, color, stride, width, height, data);
        }
        public static bool isInPicture(int x, int width)
        {
            if (x >= 0 && x <= width)
                return true;
            else
                return false;
            
        }
        public static double EuclideanDistance(double x1, double y1, double x2, double y2)=> Math.Sqrt(((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)));
        public static BitmapSource LineDecider(BitmapSource source, int x1, int y1, int x2, int y2, byte red, byte green, byte blue, int thickness)
        {
            if (Math.Abs(y2-y1) < Math.Abs(x2-x1))
            {
                if (x1 > x2)
                {
                    return SymmetricLineLow(source, x2, y2, x1, y1, red, green, blue,thickness);
                   
                }
                else
                {
                   return SymmetricLineLow(source, x1, y1, x2, y2, red, green, blue, thickness);
                  
                   
                }

                
            }
            else
            {
                if(y1>y2)
                {
                    return SymmetricLineHigh(source, x2, y2, x1, y1, red, green, blue, thickness);
                   
                }     
                else
                {
                    return SymmetricLineHigh(source, x1, y1, x2, y2, red, green, blue, thickness);
                    
                }
            }

        }
        public static BitmapSource AntiAliasinDecider(BitmapSource source, int x1, int y1, int x2, int y2, byte red, byte green, byte blue, int thickness)
        {
            if (Math.Abs(y2 - y1) < Math.Abs(x2 - x1))
            {
                if (x1 > x2)
                {
                    
                    return AntiAliasingLow(source, x2, y2, x1, y1, red, green, blue, thickness);
                }
                else
                {
                   
                    return AntiAliasingLow(source, x1, y1, x2, y2, red, green, blue, thickness);

                }


            }
            else
            {
                if (y1 > y2)
                {
                   return AntiAliasingHigh(source, x2, y2, x1, y1, red, green, blue, thickness);
                }
                else
                {
                    return AntiAliasingHigh(source, x1, y1, x2, y2, red, green, blue, thickness);
                }
            }
        }
        public static BitmapSource AntiAliasingLow(BitmapSource source, int x1, int y1, int x2, int y2, byte red, byte green, byte blue, int thickness)
        {

            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
            byte[] data = new byte[length];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);
            ThickAntialiasedLine(x1, y1, x2, y2, thickness, data, Color.FromRgb(red, green, blue), source.PixelWidth, source.PixelHeight, stride);
            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(


                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, data, stride);
        }
        public static BitmapSource AntiAliasingHigh(BitmapSource source, int x1, int y1, int x2, int y2, byte red, byte green, byte blue, int thickness)
        {

            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
            byte[] data = new byte[length];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);
            ThickAntialiasedLineHigh(x1, y1, x2, y2, thickness, data, Color.FromRgb(red, green, blue), source.PixelWidth, source.PixelHeight, stride);
            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(


                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, data, stride);
        }
        public static BitmapSource SymmetricLineLow(BitmapSource source, int x1, int y1, int x2, int y2, byte red, byte green, byte blue,int thickness)
        {
            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
            byte[] data = new byte[length];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            int dx = x2 - x1;
            int dy = y2 - y1;
           
            int xf = x1, yf = y1;
            int xb = x2, yb = y2;

            int yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy *= yi;
            }
            int d = 2 * dy - dx;
            int dE = 2 * dy;
            int dNE = 2 * (dy - dx);

            drawLine(xf, yf, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data,thickness);
            drawLine(xb, yb, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data,thickness);


            while (xf < xb)
            {
                ++xf; --xb;
                if (d < 0)
                    d += dE;
                else
                {
                    d += dNE;
                    yf += yi;
                    yb -= yi;
                }
                drawLine(xf, yf, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data, thickness);
                drawLine(xb, yb, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data, thickness);
            }
            





            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, data, stride);
        }
        public static BitmapSource SymmetricLineHigh(BitmapSource source, int x1, int y1, int x2, int y2, byte red, byte green, byte blue,int thickness)
        {
            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
            byte[] data = new byte[length];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            int dx = x2 - x1;
            int dy = y2 - y1;
            
            int xf = x1, yf = y1;
            int xb = x2, yb = y2;

            int xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx *= xi;
            }
            int d = 2 * dx - dy;
            int dE = 2 * dx;
            int dNE = 2 * (dx - dy);
            drawLine(xf, yf, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data,thickness);
            drawLine(xb, yb, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data,thickness);

            while (yf < yb)
            {
                ++yf;
                --yb;
               
                if (d < 0)
                    d += dE;
                else
                {
                    d += dNE;
                    xf += xi;
                    xb -= xi;
                }
                drawLine(xf, yf, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data, thickness);
                drawLine(xb, yb, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data, thickness);
            }






            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, data, stride);
        }
        public static BitmapSource MidpointCircle(BitmapSource source, int xc, int yc, int R, byte red, byte green, byte blue, int thickness)
        {

            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
            byte[] data = new byte[length];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            int d = 1 - R;
            int x = 0;
            int y = R;

            drawThickCircle(xc, yc,x , y,  Color.FromRgb(red,green,blue),stride, source.PixelWidth, source.PixelHeight, data,thickness);
            while (y > x)
            {
                if (d < 0) //move to E
                    d += 2 * x + 3;
                else //move to SE
                {
                    d += 2 * x - 2 * y + 5;
                    --y;
                }
                ++x;
                drawThickCircle(xc, yc, x, y, Color.FromRgb(red, green, blue), stride, source.PixelWidth, source.PixelHeight, data,thickness);
            }


            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, data, stride);
        }

        public static int[,] CreateBrushKernel(int thickness)
        {
            int[,] kernel = new int[thickness, thickness];
            int f_x = (thickness - 1) / 2;
            for (int j = 0; j < thickness; ++j)
            {
                for (int k = 0; k < thickness; ++k)
                {
                  
                        kernel[j, k] = 0;
                }

            }
            for (int j = 0; j < thickness; ++j)
            {
                if(j <= f_x)
                {
                    int start = f_x - j - 1;
                    int end = f_x + j+1;
                    if(start < 0)
                    {
                        start = 0;
                    }
                    if(end >= thickness)
                    {
                        end = thickness - 1;
                    }
                    for (int k =start; k <= end; ++k)
                    {
                        kernel[j, k] = 1;
                    }
                }
                else
                {
                    int diff = j - f_x;
                    int end = thickness - diff +1;
                    int start = diff - 1;
                    if(start < 0)
                    {
                        start = 0;
                    }
                    if (end > thickness)
                    {
                        end = thickness;
                    }
                    for (int k = start; k < end; ++k)
                    {
                        kernel[j, k] = 1;
                    }

                }
               

            }
           
             
               
            return kernel;
        }
        public static int[,] CreateBrushMatrixSize(int thickness)
        {
            int[,] kernel = new int[thickness, thickness];
            int f_x = (thickness - 1) / 2;
            for (int j = 0; j < thickness; ++j)
            {
                for (int k = 0; k < thickness; ++k)
                {
                    if (j == 0 && k == 0 || (j == thickness - 1 && k == thickness - 1) || (j == 0 && k == thickness - 1) || (j == thickness - 1 && k == 0))
                        kernel[j, k] = 0;
                    else
                        kernel[j, k] = 1;

                }

            }
           



            return kernel;
        }
        public static float IntensifyPixel(int x, int y, int thickness, float distance, Color color,int stride,int width,int height,byte[]data)
        {
            float r = 0.5f;
            float cov = coverage(thickness, distance, r);
            if (cov > 0)
            {
                var newColor = LinearInterpolation(Color.FromRgb(0, 0, 0), color, cov);
                putPixel(x, y, newColor,stride,width,height,data);
            }
            return cov;
        }
        public static float lerp(float v0, float v1, float t)
        {
            return v0+(v1-v0)*t;
        }
        public static Color LinearInterpolation(Color background, Color myline, float cov)
        {
            var r = lerp(background.R, myline.R, cov);
            var g = lerp(background.G, myline.G, cov);
            var b = lerp(background.B, myline.B, cov);

            var red = Clamp(r);
            var green = Clamp(g);
            var blue = Clamp(b);
            return Color.FromRgb(red, green, blue);
        }
        public static byte Clamp(float val)
        {
            if (val > 255)
                return 255;
            else if (val < 0)
                return 0;
            else
                return (byte)val;
        }
        public static void ThickAntialiasedLine(int x1, int y1, int x2, int y2, int thickness, byte[]data,Color color, int width, int height, int stride)
        {
            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy *= yi;
            }
            int dE = 2 * dy, dNE = 2 * (dy - dx);
            int d = 2 * dy - dx;
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float) (1.0 / (2 * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2 * dx * invDenom; //precomputed constant
            int x = x1, y = y1;
            
            IntensifyPixel(x, y, thickness, 0, color, stride, width, height, data);
            for (int i = 1; IntensifyPixel(x, y + i, thickness, i*two_dx_invDenom, color, stride, width, height, data) == 1; i++) ;
            for (int i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom, color, stride, width, height, data) == 1; i++) ;

           

            while (x < x2)
            {
                ++x;
                if (d < 0) // move to E
                {
                    two_v_dx = d + dx;
                    d += dE;
                }
                else // move to NE
                {
                    
                    two_v_dx = d - dx;
                    d += dNE;
                    y+=yi;
                }
              
                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, color, stride, width, height, data);
                for (int i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom, color, stride, width, height, data) == 1; i++) ;
                for (int i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom, color, stride, width, height, data) == 1; i++) ;
             
            }
        }
        public static void ThickAntialiasedLineHigh(int x1, int y1, int x2, int y2, int thickness, byte[] data, Color color, int width, int height, int stride)
        {
            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx *= xi;
            }
            int d = 2 * dx - dy;
            int dE = 2 * dx;
            int dNE = 2 * (dx - dy);
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float)(1.0 / (2 * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2 * dx * invDenom; //precomputed constant
            int x = x1, y = y1;

            IntensifyPixel(x, y, thickness, 0, color, stride, width, height, data);
            for (int i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom, color, stride, width, height, data) == 1; i++) ;
            for (int i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom, color, stride, width, height, data) == 1; i++) ;

           

            while (y < y2)
            {
                ++y;
                if (d < 0) // move to E
                {
                    two_v_dx = d + dx;
                    d += dE;
                }
                else // move to NE
                {
                    two_v_dx = d - dx;
                    d += dNE;
                    x += xi;
                }

                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, color, stride, width, height, data);
                for (int i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom, color, stride, width, height, data) == 1; i++) ;
                for (int i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom, color, stride, width, height, data) == 1; i++) ;

            }
        }
        // d - distance between line and the center
        public static float coverage(int thickness, float d, float r)
        {
           var w = (float)thickness / 2;
            

            if(w>=r)
            {
                if (w < d)
                {
                    return cov(d - w, r);
                }
                else if (0 <= d && d<=w)
                {
                    return (1 - cov(w - d, r));
                }
                
            }
            return 0;
            //// necesarry?
            //else
            //{

            //    if (0 <= d && d <= w)
            //    {
            //        return 1 - cov(w - d, r) - cov(w + d, r);
            //    }
            //    else if (w < d && d <= r - w)
            //    {
            //        return cov(d - w, r) - cov(d + w, r);
            //    }
            //    else if (r - w < d && d <= r + w)
            //    {
            //        return cov(d - w, r);
            //    }
            //    else
            //        return -1;
            //}
           

        }
        public static float cov(float d, float r)
        {
            if (d > r)
                return 0;
            else
            {
                
                float tmp = (float)((1 / Math.PI) * Math.Acos(d / r) - (d / (Math.PI * Math.Pow(r,2)) * Math.Sqrt(Math.Pow(r, 2) - Math.Pow(d, 2))));
                return tmp;
            }
        }
        
    }
}
