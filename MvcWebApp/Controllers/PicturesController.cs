using AzureStorageLibrary;
using AzureStorageLibrary.Models;
using AzureStorageLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using MvcWebApp.Models;
using Newtonsoft.Json;
using System.Text;

namespace MvcWebApp.Controllers
{
    public class PicturesController : Controller
    {
        public string UserId = "User123";
        public string City { get; set; } = "istanbul";

        private readonly INoSqlStorage<UserPicture> _noSqlStorage;
        private readonly IBlobStorage _blobStorage;

        public PicturesController(INoSqlStorage<UserPicture> noSqlStorage, IBlobStorage blobStorage)
        {
            _noSqlStorage = noSqlStorage;
            _blobStorage = blobStorage;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserId = UserId;
            ViewBag.City = City;

            List<FileBlob> fileBlobs = new List<FileBlob>();

            var user = await _noSqlStorage.Get(UserId, City);

            ViewBag.blobUrl = $"{_blobStorage.BlobUrl}/{ContainerNameEnum.pictures}";

            if (user != null)
            {
                foreach (var item in user.Paths)
                {
                    fileBlobs.Add(new FileBlob { Name = item, Url = $"{_blobStorage.BlobUrl}/{ContainerNameEnum.pictures}/{item}" });
                }
            }
            ViewBag.fileBlobs = fileBlobs;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IEnumerable<IFormFile> pictures)
        {
            List<string> pictureList = new List<string>();

            foreach (var item in pictures)
            {
                var newPictureName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(item.FileName)}";

                await _blobStorage.UploadAsync(item.OpenReadStream(), newPictureName, ContainerNameEnum.pictures);

                pictureList.Add(newPictureName);
            }

            var isUser = await _noSqlStorage.Get(UserId, City);

            if (isUser != null)
            {
                pictureList.AddRange(isUser.Paths);
                isUser.Paths = pictureList;
            }
            else
            {
                isUser = new UserPicture();
                isUser.RowKey = UserId;
                isUser.PartitionKey = City;
                isUser.Paths = pictureList;
            }

            await _noSqlStorage.Add(isUser);

            return RedirectToAction("index");
        }

        public async Task<IActionResult> AddWatermark(PictureWatermarkQueue pictureWatermarkQueue)
        {
            var jsonString = JsonConvert.SerializeObject(pictureWatermarkQueue);

            string jsonStringBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

            AzQueue azQueue = new AzQueue("watermarkqueue");

            await azQueue.SendMessageAsync(jsonStringBase64);

            return Ok();
        }

        public async Task<IActionResult> ShowWatermark()
        {
            UserPicture userPicture = await _noSqlStorage.Get(UserId, City);

            List<FileBlob> fileBlobs = new List<FileBlob>();

            foreach (var item in userPicture.WatermarkPaths)
            {
                fileBlobs.Add(new FileBlob { Name = item, Url = $"{_blobStorage.BlobUrl}/{ContainerNameEnum.watermarkpictures.ToString()}/{item}" });
            }

            ViewBag.fileBlobs = fileBlobs;

            return View();
        }
    }
}
