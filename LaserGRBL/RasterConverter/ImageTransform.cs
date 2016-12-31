﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace LaserGRBL.RasterConverter
{
	public class ImageTransform
	{

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			Rectangle destRect = new Rectangle(0, 0, width, height);
			Bitmap destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (Graphics g = Graphics.FromImage(destImage))
			{
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

				using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
				{
					wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
					g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}

		public static Bitmap KillAlfa(Image image)
		{
			Bitmap destImage = new Bitmap(image.Width, image.Height);
			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (Graphics g = Graphics.FromImage(destImage))
			{
				g.Clear(Color.White);
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
				g.DrawImage(image, 0, 0);
			}

			return destImage;
		}

		public static Bitmap Negative(Image img)
		{

			ColorMatrix cm = new ColorMatrix(new float[][] {
				new float[] {
					-1,
					0,
					0,
					0,
					0
				},
				new float[] {
					0,
					-1,
					0,
					0,
					0
				},
				new float[] {
					0,
					0,
					-1,
					0,
					0
				},
				new float[] {
					0,
					0,
					0,
					1,
					0
				},
				new float[] {
					1,
					1,
					1,
					0,
					1
				}
			});

			return draw_adjusted_image(img, cm);

		}

		public static Bitmap Brightness(Image img, float brightness)
		{
			ColorMatrix cm = new ColorMatrix(new float[][] {
				new float[] {
					1,
					0,
					0,
					0,
					0
				},
				new float[] {
					0,
					1,
					0,
					0,
					0
				},
				new float[] {
					0,
					0,
					1,
					0,
					0
				},
				new float[] {
					0,
					0,
					0,
					1,
					0
				},
				new float[] {
					brightness,
					brightness,
					brightness,
					0,
					1
				}
			});

			return draw_adjusted_image(img, cm);

		}

		public static Bitmap Contrast(Image img, float contrast)
		{
			ColorMatrix cm = new ColorMatrix(new float[][] {
				new float[] {
					contrast,
					0,
					0,
					0,
					0
				},
				new float[] {
					0,
					contrast,
					0,
					0,
					0
				},
				new float[] {
					0,
					0,
					contrast,
					0,
					0
				},
				new float[] {
					0,
					0,
					0,
					1,
					0
				},
				new float[] {
					0,
					0,
					0,
					0,
					1
				}
			});

			return draw_adjusted_image(img, cm);

		}

		public static Bitmap BrightnessContrast(Image img, float brightness, float contrast)
		{
			ColorMatrix cm = new ColorMatrix(new float[][] {
				new float[] {
					contrast,
					0,
					0,
					0,
					0
				},
				new float[] {
					0,
					contrast,
					0,
					0,
					0
				},
				new float[] {
					0,
					0,
					contrast,
					0,
					0
				},
				new float[] {
					0,
					0,
					0,
					1,
					0
				},
				new float[] {
					brightness,
					brightness,
					brightness,
					0,
					1
				}
			});

			return draw_adjusted_image(img, cm);

		}

		public static Bitmap Threshold(Image img, float threshold, bool apply)
		{
			Bitmap bmp = new Bitmap(img);

			using (Graphics g = Graphics.FromImage(bmp))
			{
				// Create an ImageAttributes object, and set its color threshold.
				ImageAttributes imageAttr = new ImageAttributes();
				imageAttr.SetThreshold(threshold);

				if (apply)
					g.DrawImage(img, new Rectangle(0,0, bmp.Width, bmp.Height), 0,0, bmp.Width, bmp.Height,	GraphicsUnit.Pixel, imageAttr);
				else
					g.DrawImage(img, 0,0);
			}
			return bmp;
		}

		private static Bitmap draw_adjusted_image(Image img, ColorMatrix cm)
		{

			try {
				Bitmap tmp = new Bitmap(img.Width, img.Height);
				// create a copy of the source image 
				using (Graphics g = Graphics.FromImage(tmp)) {
					g.Clear(Color.Transparent);

					ImageAttributes imgattr = new ImageAttributes();
					Rectangle rc = new Rectangle(0, 0, img.Width, img.Height);
					// associate the ColorMatrix object with an ImageAttributes object
					imgattr.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

					// draw the copy of the source image back over the original image, 
					//applying the ColorMatrix

					g.DrawImage(img, rc, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgattr);
				}
				return tmp;
			} catch {
				return null;
			}

		}




		//**************************

		public enum Formula
		{
			SimpleAverage = 0,
			WeightAverage = 1,
            OpticalCorrect = 2
		}


		public static Bitmap GrayScale(Image img, float R, float G, float B, Formula formula)
		{
			ColorMatrix cm = default(ColorMatrix);

			// Apply selected grayscale formula
						
			if (formula == Formula.SimpleAverage)
			{
					cm = new ColorMatrix(new float[][] {
						new float[] {
							0.333F * R,
							0.333F * R,
							0.333F * R,
							0F,
							0F
						},
						new float[] {
							0.333F * G,
							0.333F * G,
							0.333F * G,
							0F,
							0F
						},
						new float[] {
							0.333F * B,
							0.333F * B,
							0.333F * B,
							0F,
							0F
						},
						new float[] {
							0F,
							0F,
							0F,
							1F,
							0F
						},
						new float[] {
							0F,
							0F,
							0F,
							0F,
							1F
						}
					});
			}
			else if (formula == Formula.WeightAverage)
			{
					cm = new ColorMatrix(new float[][] {
						new float[] {
							0.333F * R,
							0.333F * R,
							0.333F * R,
							0F,
							0F
						},
						new float[] {
							0.444F * G,
							0.444F * G,
							0.444F * G,
							0F,
							0F
						},
						new float[] {
							0.222F * B,
							0.222F * B,
							0.222F * B,
							0F,
							0F
						},
						new float[] {
							0F,
							0F,
							0F,
							1F,
							0F
						},
						new float[] {
							0F,
							0F,
							0F,
							0F,
							1F
						}
					});
			}
            else if (formula == Formula.OpticalCorrect) // Reference: http://www.had2know.com/technology/rgb-to-gray-scale-converter.html
            {
                cm = new ColorMatrix(new float[][] { //x = 0.299r + 0.587g + 0.114b
                        new float[] {
                            0.299F * R,
                            0.299F * R,
                            0.299F * R,
                            0F,
                            0F
                        },
                        new float[] {
                            0.587F * G,
                            0.587F * G,
                            0.587F * G,
                            0F,
                            0F
                        },
                        new float[] {
                            0.114F * B,
                            0.114F * B,
                            0.114F * B,
                            0F,
                            0F
                        },
                        new float[] {
                            0F,
                            0F,
                            0F,
                            1F,
                            0F
                        },
                        new float[] {
                            0F,
                            0F,
                            0F,
                            0F,
                            1F
                        }
                    });
            }


            return draw_adjusted_image(img, cm);

		}

		public static Image To8bit(Image image)
		{
			using (var bitmap = new Bitmap(image))
			using (var stream = new System.IO.MemoryStream())
			{
				var parameters = new EncoderParameters(1);
				parameters.Param[0] = new EncoderParameter(Encoder.ColorDepth, 8L);

				var info = GetEncoderInfo("image/tiff");
				bitmap.Save(stream, info, parameters);

				return Image.FromStream(stream);
			}
		}

		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for (j = 0; j < encoders.Length; ++j)
			{
				if (encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}
	}

}