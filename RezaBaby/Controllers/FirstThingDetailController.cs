using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

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
                    return View(firstThings);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage");
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
                if (ModelState.IsValid)
                {
                    _db.FirstThingDetails.Add(firstThingDetail);
                    _db.SaveChanges();

                    WebCache.Remove(cacheIndexKeyFirstThingDetails);
                    // Insert media start
                    if (file != null)
                    {
                        // The maximum allowed file size is 200MB.
                        if (file.ContentLength > 100 * 1024 * 1024)
                        {
                            ModelState.AddModelError("", "Your file is too large, maximum allowed size is: 100 MB ");
                            return View(firstThingDetail);
                            //return RedirectToAction("UploadTooBig");
                        }
                        //specify the container name
                        string containerName = "mycontainer";

                        for (int fileNum = 0; fileNum < Request.Files.Count; fileNum++)
                        {
                            string fileName = Path.GetFileName(Request.Files[fileNum].FileName);
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
                        }

                        AsyncBlockBlobUpload blobUploadManager = new AsyncBlockBlobUpload();
                        AsyncBlockBlobUploadCaller caller = new AsyncBlockBlobUploadCaller(blobUploadManager.UploadBlockBlobsInParallel);
                        caller.BeginInvoke(blobFileMapperList, containerName, new AsyncCallback(OnUploadBlockBlobsInParallelCompleted), null);
                    }
                    // Insert media end

                    // OK
                    ModelState.Clear();
                    return RedirectToAction("Index", new { id = firstThingDetail.FirstId });
                }

                return View(firstThingDetail);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage");
            }
        }

        // GET: FirstThingDetail/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var model = _db.FirstThingDetails.Find(id);

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage");
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
                return RedirectToAction("ErrorPage");
            }
        }

        // GET: FirstThingDetail/Delete/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Delete(int id)
        {
            try
            {
                var itemToRemove = _db.FirstThingDetails.SingleOrDefault(f => f.ID == id);
                _db.FirstThingDetails.Remove(itemToRemove);
                _db.SaveChanges();

                WebCache.Remove(cacheIndexKeyFirstThingDetails);

                return RedirectToAction("Index", new { id = itemToRemove.FirstId });
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage");
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

        public ActionResult UploadTooBig()
        {
            return View();
        }

        public ActionResult ErrorPage()
        {
            return View();
        }
    }
}
