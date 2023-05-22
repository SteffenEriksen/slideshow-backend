using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ImageUploadDemo
{
    public static class ZipHelper
    {
        public readonly static string ImageZipFilename = "images.zip";
        private const string ImagePath = "images";


        public static async Task<List<string>> DownloadImagesFromAzure(CloudBlobContainer container, List<string> imageUrls)
        {
            var filenames = GetFilenames(imageUrls);

            await DownloadImagesToServerFromAzureBlob(container, filenames);

            return filenames;
        }

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



        public static async Task<MemoryStream> DownloadImagesFromAzureWithTempStorage(CloudBlobContainer container, List<string> imageUrls)
        {
            var filenames = GetFilenames(imageUrls);

            await DownloadImagesToServerFromAzureBlob(container, filenames);

            //return CreateZipFromFilesWithStream(filenames);
            return CreateZipFromFilesWithStream2(filenames);
        }

        public static async Task DownloadImagesFromAzureWithTempStorage2(CloudBlobContainer container, List<string> imageUrls)
        {
            var filenames = GetFilenames(imageUrls);

            await DownloadImagesToServerFromAzureBlob(container, filenames);

            CreateZipFromFolder();

            DeleteTempDownloadFolder(); // Cleanup after zip is created
        }

        public static async Task<byte[]> DownloadImagesFromAzureWithTempStorage3(CloudBlobContainer container, List<string> imageUrls)
        {
            var filenames = GetFilenames(imageUrls);

            await DownloadImagesToServerFromAzureBlob(container, filenames);

            return GetZipFile(filenames);
        }

        private static async Task DownloadImagesToServerFromAzureBlob(CloudBlobContainer container, List<string> filenames)
        {
            if (!Directory.Exists(ImagePath))
                Directory.CreateDirectory(ImagePath);

            foreach (var filename in filenames)
            {
                var blob = container.GetBlobReference(filename);
                using (var fileStream = System.IO.File.OpenWrite($"{ImagePath}/{filename}"))
                {
                    await blob.DownloadToStreamAsync(fileStream);
                }
            }
        }
        
        private static byte[] GetZipFile(List<string> filenames)
        {
            //location of the file you want to compress
            //string filePath = @"C:\myfolder\myfile.ext";

            //name of the zip file you will be creating

            if (File.Exists(ImageZipFilename))
                File.Delete(ImageZipFilename);

            byte[] result;

            var filenameOne = filenames[0];


            using (MemoryStream zipArchiveMemoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(zipArchiveMemoryStream, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry zipEntry = zipArchive.CreateEntry(ImageZipFilename);
                    using (Stream entryStream = zipEntry.Open())
                    {
                        using (var tmpMemory = new MemoryStream(System.IO.File.ReadAllBytes($"{ImagePath}/{filenameOne}")))
                        {
                            tmpMemory.CopyTo(entryStream);
                        };
                    }
                    zipArchive.Dispose();
                }

                zipArchiveMemoryStream.Seek(0, SeekOrigin.Begin);
                result = zipArchiveMemoryStream.ToArray();
            }

            return result;
            //return File(result, "application/zip", ImageZipFilename);
        }

        private static void CreateZipFromFolder()
        {
            if (File.Exists(ImageZipFilename)) 
                File.Delete(ImageZipFilename);

            System.IO.Compression.ZipFile.CreateFromDirectory(ImagePath, ImageZipFilename);
        }

        private static void DeleteTempDownloadFolder()
        {
            Directory.Delete(ImagePath, true);
        }


        public static MemoryStream CreateZipFromFilesWithStream(List<string> filenames)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filename in filenames)
                    {
                        var file = $"{ImagePath}/{filename}";
                        var path = Path.GetFileName(file);
                        zipArchive.CreateEntryFromFile(file, path);
                    }
                }

                memoryStream.Position = 0;
                return memoryStream;
                //return File(memoryStream, "application/zip", "my.zip");
            }
        }

        public static MemoryStream CreateZipFromFilesWithStream2(List<string> filenames)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filename in filenames)
                    {
                        var fileInArchive = zipArchive.CreateEntry(ImageZipFilename, CompressionLevel.Optimal);
                        var fileBytes = System.IO.File.ReadAllBytes($"{ImagePath}/{filename}");

                        using (var entryStream = fileInArchive.Open())
                        {
                            using (var fileToCompressStream = new MemoryStream(fileBytes))
                            {
                                fileToCompressStream.CopyTo(entryStream);
                            }
                        }
                    }
                }

                using (var fileStream = new FileStream(ImageZipFilename, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }

                return memoryStream;
            }
        }

        private static List<string> GetFilenames(List<string> imageUrls)
        {
            return imageUrls.Select(GetFilename).ToList();
        }

        private static string GetFilename(string imageUrl)
        {
            return imageUrl.Split(new string[] { "/" }, StringSplitOptions.None).Last();
        }
    }
}
