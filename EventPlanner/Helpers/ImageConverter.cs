using System.IO;
using Avalonia.Media.Imaging;

namespace EventPlanner.Helpers
{
    public static class ImageConverter
    {
        public static byte[] ImageToByteArray(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static Bitmap ByteArrayToImage(byte[] byteArray)
        {
            using (var memoryStream = new MemoryStream(byteArray))
            {
                return new Bitmap(memoryStream);
            }
        }
    }
}
