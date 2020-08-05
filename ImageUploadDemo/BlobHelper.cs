using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageUploadDemo
{
    public static class BlobHelper
    {
        public static async Task<CloudBlobContainer> GetBlobContainer(ConfigHelper config)
        {
            //var config = new ConfigHelper();
            // Pull these from config
            var blobStorageConnectionString = config.ConnectionString;
            var blobStorageContainerName = config.ContainerName;

            // Create blob client and return reference to the container
            var blobStorageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
            var blobClient = blobStorageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(blobStorageContainerName);
            BlobContainerPermissions permissions = await container.GetPermissionsAsync();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            await container.SetPermissionsAsync(permissions);

            return container;
        }

        public static async Task DeleteBlob(string blobName, ConfigHelper config)
        {
            var blob = GetBlobContainer(config).Result.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }
        public static async Task DeleteBlob2(CloudBlobContainer container, string blobName)
        {
            var blob = container.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }
    }
}
