using System.Drawing;
using System.Windows.Forms;
using System.IO;
using GTA.Math;

namespace CircleDataCollection
{
    public class GTAVUtils
    {
        public static Bitmap GetScreenshot()
        {
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(screenshot);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.AllScreens[0].Bounds.Size);
            g.Dispose();
            return screenshot;
        }

        public static GTAVData DataPreprocess(Bitmap screenshot, ImageInfo imageInfo)
        {
            float cutBorderWidth = .1f;

            // cutImage
            int cutWidth = (int)(screenshot.Width * cutBorderWidth);
            int cutHeight = (int)(screenshot.Height * cutBorderWidth);
            Rectangle rect = new Rectangle(cutWidth, cutHeight, screenshot.Width - 2 * cutWidth, screenshot.Height - 2 * cutHeight);
            Bitmap cutedScreenshot = screenshot.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);
            ImageInfo cutedImageInfo = new ImageInfo(imageInfo)
            {
                Width = cutedScreenshot.Width,
                Height = cutedScreenshot.Height
            };


            return new GTAVData(cutedScreenshot, cutedImageInfo);
        }

    }

    public class ImageInfo
    {
        public ImageInfo(int width, int height, Vector3 camPos, Vector3 camRot)
        {
            Width = width;
            Height = height;
            CamPos = camPos;
            CamRot = camRot;
        }

        public ImageInfo(ImageInfo preImageInfo)
        {
            Width = preImageInfo.Width;
            Height = preImageInfo.Height;
            CamPos = preImageInfo.CamPos;
            CamRot = preImageInfo.CamRot;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public Vector3 CamPos { get; }

        public Vector3 CamRot { get; }
    }

    public class GTAVData
    {
        public GTAVData(Bitmap image, ImageInfo imageInfo)
        {
            Version = "v0.0.1";
            Image = image;
            ImageInfo = imageInfo;
        }

        public string Version { get; set; }

        public Bitmap Image { get; set; }

        public int ImageWidth
        {
            get
            {
                return Image.Width;
            }
        }

        public int ImageHight
        {
            get
            {
                return Image.Height;
            }
        }

        public ImageInfo ImageInfo;

        public void Save(string imageName)
        {
            SaveImage(imageName, Image);
        }

        public static void SaveImage(string fileName, Bitmap image)
        {
            CreateFileDirectoryIfNotExist(fileName);
            image.Save(fileName);
        }

        public static void CreateFileDirectoryIfNotExist(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
        }

    }
}