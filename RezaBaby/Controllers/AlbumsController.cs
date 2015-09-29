using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RezaBaby;
using System.IO;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Web.Helpers;

namespace RezaBaby.Controllers
{
    public class AlbumsController : Controller
    {
        List<FileBlobNameMapper> blobFileMapperList = new List<FileBlobNameMapper>();
        public delegate List<BlobOperationStatus> AsyncBlockBlobUploadCaller(List<FileBlobNameMapper> blobFileMapperList, string containerName);
        bool defaultPictureInserted = false;
        private RezaDBEntities _db = new RezaDBEntities();

        // GET: Albums
        public ActionResult Index()
        {
            return View(_db.Albums.ToList());
        }

        // GET: Albums/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = _db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            ViewBag.Media = _db.AlbumMedias;
            return View(album);
        }

        // GET: Albums/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Albums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Users = "reza.baby@yahoo.com")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Album album, HttpPostedFileBase file)
        {
            try
            {
                List<AlbumMedia> albumMedia = new List<AlbumMedia>();
                string mimeType = string.Empty;
                //throw new Exception("Roskaa");
                if (ModelState.IsValid)
                {
                    // Insert media start
                    if (file != null)
                    {
                        // The maximum allowed file size is 200MB.
                        if (file.ContentLength > 100 * 1024 * 1024)
                        {
                            ModelState.AddModelError("", "Your file is too large, maximum allowed size is: 100 MB ");
                            return View(album);
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
                                return View(album);
                            }
                            // Validate file End
                            blobFileMapperList.Add(new FileBlobNameMapper(fileName, filePath));

                            if (!fileName.Contains(".mp4") || !fileName.Contains(".MP4"))
                            {
                                if (!defaultPictureInserted)
                                {
                                    album.URL = ConfigurationManager.AppSettings.Get("CloudStorageBlob") +
                                    ConfigurationManager.AppSettings.Get("CloudStorageContainerReference") + "/" + fileName;
                                    _db.Albums.Add(album);
                                    defaultPictureInserted = true;
                                }   
                            }
                            
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
                            _db.AlbumMedias.Add(new AlbumMedia
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
                    //db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(album);
            }
            catch (Exception)
            {
                return View("Error");
                //return RedirectToAction("ErrorPage");
            }
        }

        // GET: Albums/Edit/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = _db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            // Get media files start
            ViewBag.Media = _db.AlbumMedias;
            // Get media files end
            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Users = "reza.baby@yahoo.com")]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,AlbumName,URL")] Album album)
        {
            if (ModelState.IsValid)
            {
                // Only update changed property start
                _db.Albums.Attach(album);
                foreach (var propertyName in _db.Entry(album).OriginalValues.PropertyNames)
                {
                    var original = _db.Entry(album).GetDatabaseValues().GetValue<object>(propertyName);
                    var current = _db.Entry(album).CurrentValues.GetValue<object>(propertyName);
                    // If current is null skip property
                    if (current != null)
                    {
                        if (!object.Equals(original, current))
                        {
                            _db.Entry(album).Property(propertyName).IsModified = true;
                        }
                    }
                }
                // Only update changed property end
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(album);
        }

        // GET: Albums/Delete/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = _db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Users = "reza.baby@yahoo.com")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Album album = _db.Albums.Find(id);

            GetAlbumIdDeleteMedia(id);

            _db.Albums.Remove(album);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void GetAlbumIdDeleteMedia(int id)
        {
            BlobMethods blobMethod = new BlobMethods("mycontainer");
            var mediaToRemove = (from am in _db.AlbumMedias where am.AlbumId == id select am);

            foreach (var item in mediaToRemove)
            {
                string result = blobMethod.DeleteBlob(item.FileName);
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

        private string DeletePicture(string pictureName)
        {
            BlobMethods blobMethod = new BlobMethods("mycontainer");
            string result = blobMethod.DeleteBlob(pictureName);

            var itemToRemove = _db.AlbumMedias.SingleOrDefault(fm => fm.FileName == pictureName);
            _db.AlbumMedias.Remove(itemToRemove);
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
                        var itemToUpdate = _db.AlbumMedias.SingleOrDefault(fm => fm.FileName == pictureName);

                        _db.AlbumMedias.Add(new AlbumMedia
                        {
                            AlbumId = itemToUpdate.AlbumId,
                            FileName = itemToUpdate.FileName,
                            URL = itemToUpdate.URL,
                            MimeType = itemToUpdate.MimeType,
                            Orientation = "L"
                        });

                        // Save new
                        _db.SaveChanges();
                        // Delete old
                        var itemToDelete = _db.AlbumMedias.SingleOrDefault(fm => fm.ID == itemToUpdate.ID);
                        _db.AlbumMedias.Remove(itemToDelete);
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
                        var itemToUpdate = _db.AlbumMedias.SingleOrDefault(fm => fm.FileName == pictureName);

                        _db.AlbumMedias.Add(new AlbumMedia
                        {
                            AlbumId = itemToUpdate.AlbumId,
                            FileName = itemToUpdate.FileName,
                            URL = itemToUpdate.URL,
                            MimeType = itemToUpdate.MimeType,
                            Orientation = "P"
                        });

                        // Save new
                        _db.SaveChanges();
                        // Delete old
                        var itemToDelete = _db.AlbumMedias.SingleOrDefault(fm => fm.ID == itemToUpdate.ID);
                        _db.AlbumMedias.Remove(itemToDelete);
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

                string stringAlbumID = (Request.Form["albumID"]);
                string[] albumID = stringAlbumID.Split(',');

                return RedirectToAction("Index", new { id = Convert.ToInt32(albumID[picNumber]) });
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Add(Album album, HttpPostedFileBase file)
        {
            try
            {
                int albumID = Convert.ToInt32(Request.Form["albumID"]);
                string mimeType = string.Empty;

                // Insert media start
                if (file != null)
                {
                    // The maximum allowed file size is 200MB.
                    if (file.ContentLength > 100 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Your file is too large, maximum allowed size is: 100 MB ");
                        return View(album);
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
                            return View(album);
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
                        _db.AlbumMedias.Add(new AlbumMedia
                        {
                            AlbumId = albumID,
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

                return RedirectToAction("Index", new { id = albumID });
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
