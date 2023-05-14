using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageUploadDemo
{
    public static class ZipHelper
    {
        public static async Task<MemoryStream> GetImagesFromAzureToZip(CloudBlobContainer container, List<string> imageUrls)
        {
            var outputMemStream = new MemoryStream();
            var zipOutputStream = new ZipOutputStream(outputMemStream);

            zipOutputStream.IsStreamOwner = false;
            zipOutputStream.SetLevel(5); 
            //zipOutputStream.UseZip64 = UseZip64.Off;

            try
            {
                foreach (var imageUrl in imageUrls)
                {
                    var filename = GetFilename(imageUrl);


                    var entry = new ZipEntry(filename);
                    zipOutputStream.PutNextEntry(entry);
                    var blob = container.GetBlockBlobReference(filename);
                    await blob.DownloadToStreamAsync(zipOutputStream);
                }
            } 
            finally
            {
                zipOutputStream.Finish();
                zipOutputStream.Close();
                //zipOutputStream.CloseEntry();

                outputMemStream.Position = 0;
            }
            
            return outputMemStream;
        }

        private static string GetFilename(string imageUrl)
        {
            return imageUrl.Split(new string[] { "/" }, StringSplitOptions.None).Last();
        }
    }
}
