using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
[Serializable]
public class ImageCollection
{
    public List<byte[]> Images { get; set; }

    public ImageCollection()
    {
        Images = new List<byte[]>();
    }

    // Add a new image
    public void AddImage(Bitmap image)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            image.Save(ms, ImageFormat.Png);
            Images.Add(ms.ToArray());
        }
    }

    // Convert entire collection to a single byte array
    public byte[] ToByteArray()
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, this);
            return ms.ToArray();
        }
    }

    // Create collection from byte array
    public static ImageCollection FromByteArray(byte[] data)
    {
        try
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (ImageCollection)formatter.Deserialize(ms);
            }
        }
        catch
        {
            // Handle case of old format (single image)
            ImageCollection collection = new ImageCollection();
            collection.Images.Add(data);
            return collection;
        }
    }
}