using AzureStorageLibrary;
using Microsoft.AspNetCore.Mvc;
using MvcWebApp.Models;

namespace MvcWebApp.Controllers
{
    public class BlobsController : Controller
    {
        private readonly IBlobStorage _blobStorage;

        public BlobsController(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<IActionResult> Index()
        {
            var names = _blobStorage.GetNames(ContainerNameEnum.pictures);
            var blobUrls = $"{_blobStorage.BlobUrl}/{ContainerNameEnum.pictures}";

            ViewBag.blobs = names.Select(x => new FileBlob { Name = x, Url = $"{blobUrls}/{x}" }).ToList();
            ViewBag.logs = await _blobStorage.GetLogAsync("controller.txt");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile picture)
        {
            await _blobStorage.SetLogAsync("Upload methodunuza giriş yapıldı", "controller.txt");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(picture.FileName);

            await _blobStorage.UploadAsync(picture.OpenReadStream(), fileName, ContainerNameEnum.pictures);

            await _blobStorage.SetLogAsync("Upload methodundan çıkış yapıldı", "controller.txt");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _blobStorage.DownloadAsync(fileName, ContainerNameEnum.pictures);

            return File(stream, "application/octet-stream", fileName);
        }

        public async Task<IActionResult> Delete(string fileName)
        {
            await _blobStorage.DeleteAsync(fileName, ContainerNameEnum.pictures);

            return RedirectToAction("Index");
        }
    }
}
