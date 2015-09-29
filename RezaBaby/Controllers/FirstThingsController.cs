using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace RezaBaby.Controllers
{
    public class FirstThingsController : Controller
    {
        RezaDBEntities _db = new RezaDBEntities();

        string cacheIndexKeyFirstThings = "IndexFirstThings";
        string cacheIndexKeyFirstThingDetails = "IndexFirstThingDetails";

        // GET: FirstThings
        public ActionResult Index()
        {
            var model = WebCache.Get(cacheIndexKeyFirstThings);

            if (model == null)
            {
                model = (from f in _db.FirstThing
                         orderby f.When ascending
                             select f);

                WebCache.Set(cacheIndexKeyFirstThings, model, 1200, false);
            }

            return View(model);
        }

        // GET: FirstThings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FirstThings/Create
        [HttpPost]
        [Authorize(Users = "reza.baby@yahoo.com")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,What,When,Where")] FirstThing firstThing)
        {
            if (ModelState.IsValid)
            {
                _db.FirstThing.Add(firstThing);
                _db.SaveChanges();

                WebCache.Remove(cacheIndexKeyFirstThings);
                WebCache.Remove(cacheIndexKeyFirstThingDetails);

                return RedirectToAction("Index");
            }
            
            return View(firstThing);
        }

        // GET: FirstThings/Edit/5
        public ActionResult Edit(int id)
        {
            FirstThing first = _db.FirstThing.Find(id);
            if (first == null)
            {
                return HttpNotFound();
            }
            
            return View(first);
        }

        // POST: FirstThings/Edit/5
        [HttpPost]
        [Authorize(Users = "reza.baby@yahoo.com")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,What,When,Where")] FirstThing firstThing)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(firstThing).State = EntityState.Modified;
                _db.SaveChanges();

                WebCache.Remove(cacheIndexKeyFirstThings);
                WebCache.Remove(cacheIndexKeyFirstThingDetails);

                return RedirectToAction("Index");
            }
            return View(firstThing);
        }

        // GET: FirstThings/Delete/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Delete(int id)
        {
            var itemToRemove = _db.FirstThing.SingleOrDefault(f => f.ID == id);

            GetFirstIdDeleteMedia(id);
            //_db.FirstThing.Remove(itemToRemove);
            //_db.SaveChanges();
            //// Remove from Database start
            _db.FirstThing.Remove(itemToRemove);
            _db.SaveChanges();
            // Remove from Database end
            WebCache.Remove(cacheIndexKeyFirstThings);
            WebCache.Remove(cacheIndexKeyFirstThingDetails);

            return RedirectToAction("Index");
        }

        [Authorize(Users = "reza.baby@yahoo.com")]
        public void GetFirstIdDeleteMedia(int id)
        {
            int firstID = 0;
            var itemToRemove = _db.FirstThing.SingleOrDefault(f => f.ID == id);
            firstID = (from f in itemToRemove.FirstThingDetails where f.FirstId == id select f.ID).FirstOrDefault();
            DeleteMedia(firstID);
        }

        // GET: FirstThingDetail/Delete/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public void DeleteMedia(int id)
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
            }
            catch (Exception ex)
            {

            }
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
