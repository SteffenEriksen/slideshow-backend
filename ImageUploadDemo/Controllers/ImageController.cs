using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using ImageUploadDemo.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageUploadDemo.Controllers
{

    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private ConfigHelper _config;
        private readonly IHubContext<ImageHub> _hubContext;
        //private NotifyService _notifyService;

        public ImageController(ConfigHelper config, IHubContext<ImageHub> hubContext)
        {
            _config = config;
            _hubContext = hubContext;
            //_notifyService = notifyService;
        }


        public class FileUploadAPI
        {
            public IFormFile files { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            if (files.Count == 0) return BadRequest("No files in request. Aborted.");

            try
            {
                var result = new List<BlobUploadModel>();

                //var maxNumber = await GetMaxNumberFromStorage();
                //var nextNumber = maxNumber + 1;

                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var fileName = $"{formFile.FileName}___{Guid.NewGuid()}";

                        // Retrieve reference to a blob
                        var blobContainer = await BlobHelper.GetBlobContainer(_config);
                        var blob = blobContainer.GetBlockBlobReference(fileName);

                        // Set the blob content type
                        blob.Properties.ContentType = formFile.ContentType;


                        var filePath = Path.GetTempFileName();
                        {
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await formFile.CopyToAsync(stream);
                            }
                        }


                        var newFilePath = Path.GetTempFileName();
                        try
                        {
                            using (var fs = new FileStream(filePath, FileMode.Open))
                            {
                                var img = new MagickImage(fs);
                                img.AutoOrient();   // Fix orientation
                                img.Strip();        // remove all EXIF information
                                img.Write(newFilePath);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }


                        // Upload file into blob storage, basically copying it from local disk into Azure
                        using (var fs = new FileStream(newFilePath, FileMode.Open))
                        {
                            await blob.UploadFromStreamAsync(fs);
                        }

                        // Delete local file from disk
                        //File.Delete(fileData.LocalFileName);

                        // Create blob upload model with properties from blob info
                        var blobUpload = new BlobUploadModel
                        {
                            FileName = blob.Name,
                            FileUrl = blob.Uri.AbsoluteUri,
                            FileSizeInBytes = blob.Properties.Length
                        };

                        // Add uploaded blob to the list
                        result.Add(blobUpload);
                    }
                }

                //await _notifyService.SendNotificationAsync("file(s) uploaded");
                await _hubContext.Clients.All.SendAsync("UploadedImage", result.Select(e => e.FileUrl).ToList());
                //await _notifyService.SendNotificationAsync("file(s) uploaded");


                // process uploaded files
                // Don't rely on or trust the FileName property without validation.

                //return Ok(new { count = files.Count, size, filePath});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
            

            
            return Ok(true);
        }

        //[HttpPost]
        //public async Task<IActionResult> Post(List<IFormFile> files)
        //{
        //    if (files.Count == 0) return BadRequest("No files in request. Aborted.");

        //    try
        //    {
        //        var result = new List<BlobUploadModel>();
        //        long size = files.Sum(f => f.Length);
        //        //return Ok($"{files.Count} <=> {size}");

        //        // full path to file in temp location
        //        //var filePath = Path.GetTempFileName();
        //        //foreach (var formFile in files)
        //        //{
        //        //    if (formFile.Length > 0)
        //        //    {
        //        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        //        {
        //        //            await formFile.CopyToAsync(stream);
        //        //        }
        //        //    }
        //        //}

        //        var maxNumber = await GetMaxNumberFromStorage();
        //        var nextNumber = maxNumber + 1;

        //        foreach (var formFile in files)
        //        {
        //            if (formFile.Length > 0)
        //            {
        //                var fileName = $"{formFile.FileName}___{nextNumber}";
        //                nextNumber += 1;

        //                // Retrieve reference to a blob
        //                var blobContainer = await BlobHelper.GetBlobContainer(_config);
        //                var blob = blobContainer.GetBlockBlobReference(fileName);

        //                // Set the blob content type
        //                blob.Properties.ContentType = formFile.ContentType;

        //                //var temp = formFile.ContentDisposition;


        //                var filePath = Path.GetTempFileName();
        //                {
        //                    using (var stream = new FileStream(filePath, FileMode.Create))
        //                    {
        //                        await formFile.CopyToAsync(stream);
        //                    }
        //                }


        //                var newFilePath = Path.GetTempFileName();
        //                try
        //                {
        //                    using (var fs = new FileStream(filePath, FileMode.Open))
        //                    {
        //                        var img = new MagickImage(fs);
        //                        img.AutoOrient();   // Fix orientation
        //                        img.Strip();        // remove all EXIF information
        //                        img.Write(newFilePath);
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine(e);
        //                    throw;
        //                }


        //                // Upload file into blob storage, basically copying it from local disk into Azure
        //                using (var fs = new FileStream(newFilePath, FileMode.Open))
        //                {
        //                    await blob.UploadFromStreamAsync(fs);
        //                }

        //                // Delete local file from disk
        //                //File.Delete(fileData.LocalFileName);

        //                // Create blob upload model with properties from blob info
        //                var blobUpload = new BlobUploadModel
        //                {
        //                    FileName = blob.Name,
        //                    FileUrl = blob.Uri.AbsoluteUri,
        //                    FileSizeInBytes = blob.Properties.Length
        //                };

        //                // Add uploaded blob to the list
        //                result.Add(blobUpload);
        //            }
        //        }

        //        //await _notifyService.SendNotificationAsync("file(s) uploaded");
        //        await _hubContext.Clients.All.SendAsync("UploadedImage", result.Select(e => e.FileUrl).ToList());
        //        //await _notifyService.SendNotificationAsync("file(s) uploaded");


        //        // process uploaded files
        //        // Don't rely on or trust the FileName property without validation.

        //        //return Ok(new { count = files.Count, size, filePath});
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return BadRequest(e.Message);
        //    }



        //    return Ok(true);
        //}


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var container = await BlobHelper.GetBlobContainer(_config);
            await container.CreateIfNotExistsAsync();

            var list = container.ListBlobsSegmentedAsync(new BlobContinuationToken()).Result.Results;

            return Ok(list.Select(e => e.Uri).ToList());
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string imageUrl)
        {
            try
            {
                var container = await BlobHelper.GetBlobContainer(_config);
                await container.CreateIfNotExistsAsync();

                var list = container.ListBlobsSegmentedAsync(new BlobContinuationToken()).Result.Results;

                //imageUrl = imageUrl.Replace(" ", "%20");

                var listOfBlobUrls = list.Select(e => e.Uri.AbsoluteUri.ToString()).ToList();


                //var listOfHtmlBlobUrls = list.Select(e => e.Uri.AbsoluteUri.ToString()).ToList();
                //var listOfBlobUrls = new List<string>();
                //foreach (var name in listOfHtmlBlobUrls)
                //{
                //    var fixedBlobName = name.Replace("%20", " ");
                //    listOfBlobUrls.Add(fixedBlobName);
                //}
                var htmlImageUrl = imageUrl.Replace(" ", "%20");

                var imageUrlToDelete = listOfBlobUrls.FirstOrDefault(e => e.Equals(htmlImageUrl));
                //var temp = list.Where(e => e.Uri.AbsoluteUri.Equals(imageUrl));

                if (string.IsNullOrEmpty(imageUrlToDelete)) return BadRequest("Image not found");


                imageUrlToDelete = imageUrlToDelete.Replace("%20", " ");
                var blobName = imageUrlToDelete.Split(@"/")[4];
                //await BlobHelper.DeleteBlob2(container, blobName);
                await BlobHelper.DeleteBlob(blobName, _config);

                var filtered = listOfBlobUrls.Where(e => e != imageUrlToDelete).ToList();

                //return Ok(filtered);
                return Ok(filtered.Select(e => new Uri(e)).ToList());

                //var filtered = list.Where(e => e != imageUrlToDelete).ToList();

                //return Ok(filtered.Select(e => e.Uri).ToList());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }


        [HttpGet]
        [Route("GetImage")]
        public async Task<IActionResult> GetImage(int number)
        {
            // DEPRECATED
            try
            {
                return Ok(await GetNextImage(number));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("GetMaxNumber")]
        public async Task<IActionResult> GetMaxNumber()
        {
            try
            {
                return Ok(await GetMaxNumberFromStorage());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private async Task<NextNum> GetNextImage(int getNumber)
        {
            // "https://slidestorage.blob.core.windows.net/slidepictures/banana.png_03b1e75e-be12-463b-8e1c-f61bb8434aa9"

            if (getNumber < 1) getNumber = 1;

            var images = await GetImagesFromBlob();
            if (images.Count == 0) return new NextNum(1, "");

            var max = images.Keys.Max();
            var loopFailsafeCount = 1000;
            var count = 0;

            while (true)
            {
                if (getNumber <= max)
                {
                    count++;
                    if (count > loopFailsafeCount) throw new Exception("Loop failsafe over 1000");

                    if (images.ContainsKey(getNumber))
                    {
                        var nextNum = getNumber + 1;
                        return new NextNum(nextNum, images[getNumber]);
                    }

                    getNumber += 1;
                }
                else getNumber = 1;
            }
        }



        private async Task<int> GetMaxNumberFromStorage()
        {
            var images = await GetImagesFromBlob();

            return images.Count > 0 ? images.Keys.Max() : 0;
        }

        private async Task<Dictionary<int, string>> GetImagesFromBlob()
        {

            var container = await BlobHelper.GetBlobContainer(_config);
            await container.CreateIfNotExistsAsync();

            var list = container.ListBlobsSegmentedAsync(new BlobContinuationToken()).Result.Results;
            if (list.ToList().Count == 0) return new Dictionary<int, string>();

            var listSplitted = list.Where(e => e.Uri.AbsoluteUri.Contains("___"))
                .ToDictionary(e => int.Parse(e.Uri.AbsoluteUri.Split(new string[] { "___" }, StringSplitOptions.None)[1]), e => e.Uri.AbsoluteUri);

            return listSplitted;
        }


        class NextNum
        {
            public string ImageUrl { get; set; }
            public int NextNumber { get; set; }

            public NextNum(int num, string url)
            {
                ImageUrl = url;
                NextNumber = num;
            }
        }
    }
}
