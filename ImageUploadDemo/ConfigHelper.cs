using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ImageUploadDemo
{
    //    public class ConfigHelper
    //    {
    //        public string ConnectionString { get; set; }
    //        public string ContainerName { get; set; }
    //        public string AccessKey { get; set; }


    //        public ConfigHelper()
    //        {
    //            //ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
    //            //ContainerName = ConfigurationManager.AppSettings["ContainerName"];
    //            //AccessKey = ConfigurationManager.AppSettings["AccessKey"];

    //            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=slidestorage;AccountKey=NazMo0hm39xnWzPqA6oFFqO6UUTAmV6aRf9RrwfL9/Yh5WFl9dLaMHjv1/pmzEbiiH2ZfRrRp1o2VsBPdgeUuw==;EndpointSuffix=core.windows.net";
    //            ContainerName = "slidepictures";
    //            AccessKey = "";
    //        }
    //    }

    public class ConfigHelper
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string AccessKey { get; set; }


        public ConfigHelper(IConfiguration config)
        {
            var apiConfig = config.GetSection("BlobStorage");

            ConnectionString = apiConfig["ConnectionString"];
            ContainerName = apiConfig["ContainerName"];
            AccessKey = apiConfig["AccessKey"];
        }
    }
}
