using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

using ImageResizer;
using System.Drawing;

using RezaBaby.Models;

namespace RezaBaby.Controllers
{
    public class FirstThingDetailController : Controller
    {
        List<FileBlobNameMapper> blobFileMapperList = new List<FileBlobNameMapper>();
        public delegate List<BlobOperationStatus> AsyncBlockBlobUploadCaller(List<FileBlobNameMapper> blobFileMapperList, string containerName);
        
        RezaDBEntities _db = new RezaDBEntities();

        string cacheIndexKeyFirstThingDetails = "IndexFirstThingDetails";

        // GET: FirstThingDetail
        public ActionResult Index([Bind(Prefix = "ID")] int firstId)
        {
            try
            {
                IEnumerable<dynamic> firstThingsAll = WebCache.Get(cacheIndexKeyFirstThingDetails);

                if (firstThingsAll == null)
                {
                    // Get from database
                    firstThingsAll = (from f in _db.FirstThing
                                      orderby f.When ascending
                                      select f);

                    WebCache.Set(cacheIndexKeyFirstThingDetails, firstThingsAll, 1200, false);
                }

                FirstThing firstThings = (from ft in firstThingsAll
                                          where ft.ID == firstId
                                          select ft).FirstOrDefault();

                if (firstThings != null)
                {
                    // Get media files start
                    ViewBag.Media = _db.FirstThingMedias;
                    // Get media files end
                    return View(firstThings);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception)
            {
                return View("Error");
            }
            
        }

        // GET: FirstThingDetail/Create
        [HttpGet]
        public ActionResult Create(int firstId)
        {
            return View();
        }

        // POST: FirstThingDetail/Create
        [HttpPost]
        [Authorize(Users = "reza.baby@yahoo.com")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FirstThingDetail firstThingDetail, HttpPostedFileBase file)
        {
            try
            {
                List<FirstThingMedia> firstThingMedia = new List<FirstThingMedia>();
                string mimeType = string.Empty;

                if (ModelState.IsValid)
                {
                    _db.FirstThingDetails.Add(firstThingDetail);

                    WebCache.Remove(cacheIndexKeyFirstThingDetails);
                    // Insert media start
                    if (file != null)
                    {
                        // The maximum allowed file size is 200MB.
                        if (file.ContentLength > 100 * 1024 * 1024)
                        {
                            ModelState.AddModelError("", "Your file is too large, maximum allowed size is: 100 MB ");
                            return View(firstThingDetail);
                        }
                        //specify the container name
                        string containerName = "mycontainer";
                        
                        //
                        for (int fileNum = 0; fileNum < Request.Files.Count; fileNum++)
                        {
                            // Create new filename
                            string fileName = String.Format("{0}{1}",
                                            Guid.NewGuid().ToString(),
                                            Path.GetExtension(Request.Files[fileNum].FileName));

                            string filePath = Path.GetFullPath(Request.Files[fileNum].FileName);

                            // Validate file Start
                            string[] AllowedFileExtensions = new string[] { ".jpg", ".JPG", ".png", ".PNG", ".gif", ".GIF", ".mp4", ".MP4", };
                            
                            if (!AllowedFileExtensions.Contains(fileName.Substring(fileName.LastIndexOf('.'))))
                            {
                                ModelState.AddModelError("", "You can upload only jpg, png, gif, and mp4 extension file");
                                return View(firstThingDetail);
                            }
                            // Validate file End
                            blobFileMapperList.Add(new FileBlobNameMapper(fileName, filePath));
                            // Check if video or picture start
                            if (fileName.Contains(".mp4") || fileName.Contains(".MP4"))
                            {
                                mimeType = "video/mp4";
                            }
                            else
                            {
                                mimeType = "image/jpeg";
                            }
                            // Check if video or picture end
                            _db.FirstThingMedias.Add(new FirstThingMedia
                            {
                                FileName = fileName,
                                URL = ConfigurationManager.AppSettings.Get("CloudStorageBlob") + 
                                ConfigurationManager.AppSettings.Get("CloudStorageContainerReference") + "/" + fileName,
                                MimeType = mimeType,
                                Orientation = "L"
                            });
                        }

                        AsyncBlockBlobUpload blobUploadManager = new AsyncBlockBlobUpload();
                        AsyncBlockBlobUploadCaller caller = new AsyncBlockBlobUploadCaller(blobUploadManager.UploadBlockBlobsInParallel);
                        caller.BeginInvoke(blobFileMapperList, containerName, new AsyncCallback(OnUploadBlockBlobsInParallelCompleted), null);
                    }
                    // Insert media end
                    _db.SaveChanges();
                    // OK
                    ModelState.Clear();
                    return RedirectToAction("Index", new { id = firstThingDetail.FirstId });
                }

                return View(firstThingDetail);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // GET: FirstThingDetail/Edit/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Edit(int id)
        {
            try
            {
                var model = _db.FirstThingDetails.Find(id);
                // Get media files start
                ViewBag.Media = _db.FirstThingMedias;
                // Get media files end
                return View(model);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // POST: FirstThingDetail/Edit/5
        [HttpPost]
        [Authorize(Users = "reza.baby@yahoo.com")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FirstThingDetail firstDetail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(firstDetail).State = EntityState.Modified;
                    _db.SaveChanges();

                    WebCache.Remove(cacheIndexKeyFirstThingDetails);

                    return RedirectToAction("Index", new { id = firstDetail.FirstId });
                }

                return View(firstDetail);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // GET: FirstThingDetail/Delete/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Delete(int id)
        {
            try
            {
                BlobMethods blobMethod = new BlobMethods("mycontainer");

                IEnumerable<dynamic> itemInMedia = (from fm in _db.FirstThingMedias where fm.FirstThingDetailId == id select fm);

                // Get media files start
                var itemToRemove = _db.FirstThingDetails.SingleOrDefault(f => f.ID == id);
                _db.FirstThingDetails.Remove(itemToRemove);

                WebCache.Remove(cacheIndexKeyFirstThingDetails);
                
                // Remove from blob start
                foreach (var item in itemInMedia)
                {
                    string result = blobMethod.DeleteBlob(item.FileName);
                }
                _db.SaveChanges();
                // Remove from blob end

                return RedirectToAction("Index", new { id = itemToRemove.FirstId });
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        /// <summary>
        /// Callback method for upload to azure blob operation
        /// </summary>
        /// <param name="result">async result</param>
        public static void OnUploadBlockBlobsInParallelCompleted(IAsyncResult result)
        {
            // Retrieve the delegate.
            AsyncResult asyncResult = (AsyncResult)result;
            AsyncBlockBlobUploadCaller caller = (AsyncBlockBlobUploadCaller)asyncResult.AsyncDelegate;

            //retrive the blob upload operation status list to take necessary action
            List<BlobOperationStatus> operationStausList = caller.EndInvoke(asyncResult);

            //print the status of upload operation for each blob
            foreach (BlobOperationStatus blobStatus in operationStausList)
            {
                Console.WriteLine("Blob name:" + blobStatus.Name + Environment.NewLine);
                Console.WriteLine("Blob operation status:" + blobStatus.OperationStatus + Environment.NewLine);
                if (blobStatus.ExceptionDetails != null)
                {
                    Console.WriteLine("Blob operation exception if any:" + blobStatus.ExceptionDetails.Message + Environment.NewLine);
                }

                //Note:This is where you can write the failed blob operation entry in table/ queue and again make worker role traverse th' to perform upload again.
            }

        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Add(FirstThingDetail firstThingDetail, HttpPostedFileBase file)
        {
            try
            {
                int firstThingID = Convert.ToInt32(Request.Form["firstThingID"]);
                int returnID = Convert.ToInt32(Request.Form["returnID"]);
                string mimeType = string.Empty;
                //
                // Insert media start
                if (file != null)
                {
                    // The maximum allowed file size is 200MB.
                    if (file.ContentLength > 100 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Your file is too large, maximum allowed size is: 100 MB ");
                        return View(firstThingDetail);
                    }
                    //specify the container name
                    string containerName = "mycontainer";

                    //
                    for (int fileNum = 0; fileNum < Request.Files.Count; fileNum++)
                    {
                        // Create new filename
                        string fileName = String.Format("{0}{1}",
                                        Guid.NewGuid().ToString(),
                                        Path.GetExtension(Request.Files[fileNum].FileName));

                        string filePath = Path.GetFullPath(Request.Files[fileNum].FileName);

                        // Validate file Start
                        string[] AllowedFileExtensions = new string[] { ".jpg", ".JPG", ".png", ".PNG", ".gif", ".GIF", ".mp4", ".MP4", };

                        if (!AllowedFileExtensions.Contains(fileName.Substring(fileName.LastIndexOf('.'))))
                        {
                            ModelState.AddModelError("", "You can upload only jpg, png, gif, and mp4 extension file");
                            return View(firstThingDetail);
                        }
                        // Validate file End
                        blobFileMapperList.Add(new FileBlobNameMapper(fileName, filePath));
                        // Check if video or picture start
                        if (fileName.Contains(".mp4") || fileName.Contains(".MP4"))
                        {
                            mimeType = "video/mp4";
                        }
                        else
                        {
                            mimeType = "image/jpeg";
                        }
                        // Check if video or picture end
                        _db.FirstThingMedias.Add(new FirstThingMedia
                        {
                            FirstThingDetailId = firstThingID,
                            FileName = fileName,
                            URL = ConfigurationManager.AppSettings.Get("CloudStorageBlob") +
                            ConfigurationManager.AppSettings.Get("CloudStorageContainerReference") + "/" + fileName,
                            MimeType = mimeType,
                            Orientation = "L"
                        });
                    }

                    AsyncBlockBlobUpload blobUploadManager = new AsyncBlockBlobUpload();
                    AsyncBlockBlobUploadCaller caller = new AsyncBlockBlobUploadCaller(blobUploadManager.UploadBlockBlobsInParallel);
                    caller.BeginInvoke(blobFileMapperList, containerName, new AsyncCallback(OnUploadBlockBlobsInParallelCompleted), null);
                }
                // Insert media end
                _db.SaveChanges();
                // OK
                ModelState.Clear();

                return RedirectToAction("Index", new { id = returnID });
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        /// <summary>
        /// Get URL from media saved
        /// </summary>
        /// <param name="result">async result</param>
        public List<string> GetURLFromAzureContainer(IAsyncResult result)
        {
            // Retrieve the delegate.
            AsyncResult asyncResult = (AsyncResult)result;
            AsyncBlockBlobUploadCaller caller = (AsyncBlockBlobUploadCaller)asyncResult.AsyncDelegate;

            //retrive the blob upload operation status list to take necessary action
            List<BlobOperationStatus> operationStausList = caller.EndInvoke(asyncResult);

            List<string> mediaURL = null;
            //print the status of upload operation for each blob
            foreach (BlobOperationStatus blobStatus in operationStausList)
            {
                if (blobStatus.ExceptionDetails != null)
                {
                    string myurl = blobStatus.BlobUri.ToString();
                    mediaURL.Add(blobStatus.BlobUri.ToString());
                }
            }
            return mediaURL;
        }

        private string DeletePicture(string pictureName)
        {
            BlobMethods blobMethod = new BlobMethods("mycontainer");
            string result = blobMethod.DeleteBlob(pictureName);

            var itemToRemove = _db.FirstThingMedias.SingleOrDefault(fm => fm.FileName == pictureName);
            _db.FirstThingMedias.Remove(itemToRemove);
            _db.SaveChanges();
            return result;
        }

        private string TransformPicture(string rotateL, string rotateR, string land, string port, string pictureName)
        {
            try
            {
                BlobMethods blobMethod = new BlobMethods("mycontainer");
                byte[] result = blobMethod.DownloadToStream(pictureName);
                byte[] newFileContent = null;
                WebImage photo = new WebImage(result);

                if (rotateL != null)
                {
                    if (photo != null)
                    {
                        photo.RotateLeft();
                    }
                }

                if (rotateR != null)
                {
                    if (photo != null)
                    {
                        photo.RotateRight();
                    }
                }

                if (land != null)
                {
                    if (photo != null)
                    {
                        // Landscape
                        photo.Resize(width: 260, height: 195, preserveAspectRatio: true, preventEnlarge: false);
                        // Update orientation start
                        var itemToUpdate = _db.FirstThingMedias.SingleOrDefault(fm => fm.FileName == pictureName);
                        
                        _db.FirstThingMedias.Add(new FirstThingMedia
                        {
                            FirstThingDetailId = itemToUpdate.FirstThingDetailId,
                            FileName = itemToUpdate.FileName,
                            URL = itemToUpdate.URL,
                            MimeType = itemToUpdate.MimeType,
                            Orientation = "L"
                        });

                        // Save new
                        _db.SaveChanges();
                        // Delete old
                        var itemToDelete = _db.FirstThingMedias.SingleOrDefault(fm => fm.ID == itemToUpdate.ID);
                        _db.FirstThingMedias.Remove(itemToDelete);
                        _db.SaveChanges();
                        // Update orientation end
                    }
                }

                if (port != null)
                {
                    if (photo != null)
                    {
                        // Portrait
                        photo.Resize(width: 260, height: 346, preserveAspectRatio: true, preventEnlarge: false);
                        // Update orientation start
                        var itemToUpdate = _db.FirstThingMedias.SingleOrDefault(fm => fm.FileName == pictureName);

                        _db.FirstThingMedias.Add(new FirstThingMedia
                        {
                            FirstThingDetailId = itemToUpdate.FirstThingDetailId,
                            FileName = itemToUpdate.FileName,
                            URL = itemToUpdate.URL,
                            MimeType = itemToUpdate.MimeType,
                            Orientation = "P"
                        });

                        // Save new
                        _db.SaveChanges();
                        // Delete old
                        var itemToDelete = _db.FirstThingMedias.SingleOrDefault(fm => fm.ID == itemToUpdate.ID);
                        _db.FirstThingMedias.Remove(itemToDelete);
                        _db.SaveChanges();
                        // Update orientation end
                    }
                }
                
                newFileContent = photo.GetBytes();
                if (newFileContent != null)
                {
                    return blobMethod.UploadFromByteArray(newFileContent, pictureName);
                }
                return "Error";
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Transform(FormCollection collection)
        {
            try
            {
                string deletePicNumber = (Request.Form["deletePicture"]);
                int picNumber = Convert.ToInt32(Request.Form["pictureNumber"]);
                var rotateL = (Request.Form["rotateLeft"]);
                var rotateR = (Request.Form["rotateRight"]);
                var land = (Request.Form["landscape"]);
                var port = (Request.Form["portrait"]);
                var imageName = (Request.Form["pictureName"]);
                string[] pictureName = imageName.Split(',');

                if (deletePicNumber != null)
                {
                    string result = DeletePicture(pictureName[Convert.ToInt32(deletePicNumber)]);
                }
                else
                {
                    string result = TransformPicture(rotateL, rotateR, land, port, pictureName[picNumber]);
                }

                WebCache.Remove(cacheIndexKeyFirstThingDetails);

                string stringFirstID = (Request.Form["stringFirstThingID"]);
                string[] firstID = stringFirstID.Split(',');

                return RedirectToAction("Index", new { id = Convert.ToInt32(firstID[picNumber]) });
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        //public ActionResult ErrorPage()
        //{
        //    return View();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
