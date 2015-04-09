//-------------------------------------------------------------------------------------
//  Copyright 2012 Rutger Spruyt
//
//  This file is part of C# Comicviewer.
//
//  csharp comicviewer is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  csharp comicviewer is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with csharp comicviewer.  If not, see <http://www.gnu.org/licenses/>.
//-------------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace CSharpComicLoader
{
	/// <summary>
	/// Utilities used on an image.
	/// </summary>
	public class ImageUtils
	{
		private Image _image;

        /// <summary>
        /// Initializes a new instance of the ImageUtils class.
        /// </summary>
        public ImageUtils()
        {
        }

        public void Dispose()
        {
            if (_image != null)
                _image.Dispose();
        }

		/// <summary>
        /// Initializes a new instance of the ImageUtils class.
		/// </summary>
		/// <param name="image">The image.</param>
		public ImageUtils(byte[] image)
		{
			ObjectValue = image;
		}

		/// <summary>
        /// Initializes a new instance of the ImageUtils class.
		/// </summary>
		/// <param name="image">The image.</param>
		public ImageUtils(Image image)
		{
			ObjectValue = image;
		}

		/// <summary>
		/// Sets the object value.
		/// </summary>
		/// <value>
		/// The object value.
		/// </value>
		public Object ObjectValue
		{
			set
			{
				if (value is Image)
				{
					_image = value as Image;
				}
				else if (value is byte[])
				{
					_image = ConvertByteArrayToImage(value as byte[]);
				}
			}
		}

		/// <summary>
		/// Gets the object value as bytes.
		/// </summary>
		public byte[] ObjectValueAsBytes
		{
			get
			{
				if (_image != null)
				{
					return ConvertImageToByteArray(_image);
				}
			    return null;
			}
		}

		/// <summary>
		/// Gets the object value as image.
		/// </summary>
		public Image ObjectValueAsImage
		{
			get
			{
			    return _image;
			}
		}

		/// <summary>
		/// Gets or sets the height of the screen.
		/// </summary>
		/// <value>
		/// The height of the screen.
		/// </value>
		public int ScreenHeight { get; set; }

		/// <summary>
		/// Gets or sets the width of the screen.
		/// </summary>
		/// <value>
		/// The width of the screen.
		/// </value>
		public int ScreenWidth { get; set; }

		/// <summary>
		/// Convert a byte array to an image.
		/// </summary>
		/// <param name="inImage">The image as byte array.</param>
		/// <returns></returns>
		private Image ConvertByteArrayToImage(byte[] inImage)
		{
			var ms = new MemoryStream(inImage);
			Image returnImage = Image.FromStream(ms);
            ms.Dispose();
			return returnImage;
		}

		/// <summary>
		/// Convert an image to a byte array.
		/// </summary>
		/// <param name="inImage">The image.</param>
		/// <returns></returns>
		private byte[] ConvertImageToByteArray(Image inImage)
		{
		    using (var ms = new MemoryStream())
		    {
                inImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
		        return ms.ToArray();
		    }
		}


		/// <summary>
		/// Gets the color of the background.
		/// </summary>
		/// <value>
		/// The color of the background.
		/// </value>
		public System.Windows.Media.Brush BackgroundColor
		{
			get
			{
			    if (_image == null) return null;

			    Bitmap objBitmap = new Bitmap(_image);

			    const int dividedBy = 100;
			    var colors = new Color[dividedBy*4];

			    //get the color of a pixels at the edge of image
			    int i = 0;

			    //left
			    for (int y = 0; y < dividedBy; y++)
			    {
			        colors[i++] = objBitmap.GetPixel(0, y*(objBitmap.Height/dividedBy));
			    }

			    //top
			    for (int x = 0; x < dividedBy; x++)
			    {
			        colors[i++] = objBitmap.GetPixel(x*(objBitmap.Width/dividedBy), 0);
			    }

			    //right
			    for (int y = 0; y < dividedBy; y++)
			    {
			        colors[i++] = objBitmap.GetPixel(objBitmap.Width - 1, y*(objBitmap.Height/dividedBy));
			    }

			    //bottom
			    for (int x = 0; x < dividedBy; x++)
			    {
			        colors[i++] = objBitmap.GetPixel(x*(objBitmap.Width/dividedBy), objBitmap.Height - 1);
			    }

                objBitmap.Dispose();

			    //get mode of colors
			    int color = GetModeOfColorArray(colors);
			    //set bgcolor

			    var backColor = colors[0];
			    if (color != -1)
			    {
			        backColor = colors[color];
			    }

			    var backColorWpf = System.Windows.Media.Color.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B);

			    return new SolidColorBrush(backColorWpf);
			}
		}

		/// <summary>
		/// Gets the mode of a Color[]
		/// </summary>
		/// <param name="colors">Array of Colors</param>
		/// <returns>Index of mode, -1 if non found</returns>
		private int GetModeOfColorArray(Color[] colors)
		{
			Color[] distinctcolors = colors.Distinct().ToArray();
			int[] countcolors = new int[distinctcolors.Length];
			int highest = 1;
			int highestindex = -1;
			var mode = false;

			//count how many time distinct values are in colors
			for (int i = 0; i < distinctcolors.Length; i++)
			{
				foreach (Color t in colors)
				{
				    if (t == distinctcolors[i])
				        countcolors[i]++;
				}
			}
			//check what the highest value is
			for (int i = 0; i < countcolors.Length; i++)
			{
				if (countcolors[i] > highest)
				{
					highest = countcolors[i];
					highestindex = i;
					mode = true;
				}
			}

			if (mode)
				return Array.IndexOf(colors, distinctcolors[highestindex]);
			return -1;
		}

        public void ResizeImage(Size size, bool overrideHeight, bool overrideWidth, InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic)
		{
		    if (_image == null) 
                return;

		    int sourceWidth = _image.Width;
		    int sourceHeight = _image.Height;

            int destWide = 0;
            int destHigh = 0;
            if (overrideHeight && overrideWidth)
            {
                destWide = size.Width; //ScreenWidth;
                destHigh = size.Height; //ScreenHeight;
            }
            else if (overrideWidth)
            {
                destWide = size.Width;
                destHigh = sourceHeight*destWide/sourceWidth;
            }
            else if (overrideHeight)
            {
                destHigh = size.Height;
                destWide = sourceWidth * destHigh / sourceHeight;
            }

            Bitmap b = new Bitmap(destWide, destHigh);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = interpolationMode;
                g.DrawImage(_image, 0, 0, destWide, destHigh);
            }

            _image = b;

            //float nPercent = 0;

            ////if (!overrideHeight && overrideWidth)
            ////{
            ////    size.Width = ScreenWidth;
            ////}

            //float nPercentW = (size.Width / (float)sourceWidth);
            //float nPercentH = (size.Height / (float)sourceHeight);

            //if (!overrideHeight && !overrideWidth)
            //{
            //    if (nPercentH < nPercentW)
            //        nPercent = nPercentH;
            //    else
            //        nPercent = nPercentW;
            //}
            //else if (overrideHeight && !overrideWidth)
            //    nPercent = nPercentH;
            //else if (!overrideHeight && overrideWidth)
            //{
            //    nPercent = nPercentW;
            //}

            //int destWidth = (int)(sourceWidth * nPercent);
            //int destHeight = (int)(sourceHeight * nPercent);

            //if (overrideHeight && overrideWidth)
            //{
            //    destWidth = size.Width; //ScreenWidth;
            //    destHeight = size.Height; //ScreenHeight;
            //}

            //Bitmap b = new Bitmap(destWidth, destHeight);
            //using (Graphics g = Graphics.FromImage(b))
            //{
            //    g.InterpolationMode = interpolationMode;
            //    g.DrawImage(_image, 0, 0, destWidth, destHeight);
            //}

            //_image = b;
		}

	    public void DrawDouble(byte[] image1, byte[] image2, Size size, bool overrideHeight, bool overrideWidth)
	    {
            Bitmap b = new Bitmap(size.Width, size.Height);
	        using (var g = Graphics.FromImage(b))
	        {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(ConvertByteArrayToImage(image1), 0f, 0f, size.Width / 2.0f, size.Height);

                // For odd # of pages, the second image may be null
                Image secondImage = image2 == null ? new Bitmap(1, 1) : ConvertByteArrayToImage(image2);
                g.DrawImage(secondImage, size.Width / 2.0f, 0f, size.Width / 2.0f, size.Height);
	        }
	        _image = b;
	    }
	}
}
