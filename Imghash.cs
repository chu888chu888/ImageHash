using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class Imghash
{
	// Calculate a hash of an image based on visual characteristics.
	// Described at http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html
	// The exact value of the resulting hash depends on the scaling algorithms used by the runtime.
	public static ulong AverageHash(System.Drawing.Image theImage)
	{
		// Squeeze the image down to an 8x8 image.

		// Chant the ancient incantations to create the correct data structures.
		Bitmap squeezedImage = new Bitmap(8, 8, PixelFormat.Format32bppRgb);
		Graphics drawingArea = Graphics.FromImage(squeezedImage);
			drawingArea.CompositingQuality = CompositingQuality.HighQuality;
			drawingArea.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawingArea.SmoothingMode = SmoothingMode.HighQuality;
			drawingArea.DrawImage(theImage, 0, 0, 8, 8);

		byte[] grayScaleImage = new byte[64];

		uint averageValue = 0;
		ulong finalHash = 0;

		// Reduce to 8-bit grayscale and alculate the average pixel value.
		for(int y = 0; y < 8; y++)
		{
			for(int x = 0; x < 8; x++)
			{
				Color pixelColour = squeezedImage.GetPixel(x,y);
				uint grayTone = ((uint)((pixelColour.R * 0.3) + (pixelColour.G * 0.59) + (pixelColour.B * 0.11)));

				grayScaleImage[x + y*8] = (byte)grayTone;
				averageValue += grayTone;
			}
		}
		averageValue /= 64;

		// Return 1-bits when the tone is equal to or above the average,
		// and 0-bits when it's below the average.
		for(int k = 0; k < 64; k++)
		{
			//if(k % 8 == 0)
			//	Console.WriteLine();

			if(grayScaleImage[k] >= averageValue)
			{
				finalHash |= (1UL << (63-k));
			//	Console.Write(" ");
			}
			//else
			//	Console.Write("#");
		}
		Console.WriteLine();
		Console.WriteLine();

		return finalHash;
	}

	public static int Main(string[] argv)
	{
		if(argv.Length == 1)
		{
			Bitmap theImage;

			try
			{
				theImage = new Bitmap(argv[0]);
			}
			catch(Exception e)
			{
				Console.WriteLine("Couldn't open the image " + argv[0] + ".");
				return 1;
			}

			Console.WriteLine(AverageHash(theImage).ToString("x16") + "\t" + argv[0]);
		}
		else if(argv.Length == 2)
		{
			Bitmap theImage;
			Bitmap theOtherImage;

			try
			{
				theImage = new Bitmap(argv[0]);
			}
			catch(Exception e)
			{
				Console.WriteLine("Couldn't open the image " + argv[0] + ".");
				return 1;
			}
			try
			{
				theOtherImage = new Bitmap(argv[1]);
			}
			catch(Exception e)
			{
				Console.WriteLine("Couldn't open the image " + argv[1] + ".");
				return 1;
			}

			ulong hash1 = AverageHash(theImage);
			ulong hash2 = AverageHash(theOtherImage);

			Console.WriteLine(hash1.ToString("x16") + "\t" + argv[0]);
			Console.WriteLine(hash2.ToString("x16") + "\t" + argv[1]);
			Console.WriteLine("Similarity: " + ((64 - BitCount(hash1 ^ hash2))*100.0)/64.0 + "%");
		}
		else
		{
			Console.WriteLine("To get the hash of an image: Imghash.exe <image name>");
			Console.WriteLine("To compare two images: Imghash.exe <image 1> <image 2>");
			return 1;
		}
		
		return 0;
	}

	// Count the number of 1-bits in a number.

	// We use a precomputed table to hopefully speed it up.
	// Made in Python as follows:
	// a = list()
	// a.append(0)
	// while len(a) <= 128:
	//  a.extend([b+1 for b in a])
	static byte[] bitCounts = {
	0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,4,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,1,2,2,3,2,3,3,4,
	2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,
	2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,
	4,5,5,6,5,6,6,7,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,
	2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,2,3,3,4,3,4,4,5,
	3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,
	4,5,5,6,5,6,6,7,5,6,6,7,6,7,7,8 };

	static uint BitCount(ulong theNumber)
	{
		uint count=0;

		for(;theNumber > 0;theNumber >>= 8)
		{
			count+= bitCounts[(theNumber & 0xFF)];
		}

		return count;
	}
}
