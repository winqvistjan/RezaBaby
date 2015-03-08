using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace RezaBaby.Controllers
{
    public class FirstThingDetailController : Controller
    {
        RezaDBEntities _db = new RezaDBEntities();

        string cacheIndexKeyFirstThingDetails = "IndexFirstThingDetails";

        // GET: FirstThingDetail
        public ActionResult Index([Bind(Prefix = "ID")] int firstId)
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

            // Testi end
            //var firstThings1 = _db.FirstThing.Find(firstId);

            if (firstThings != null)
            {
                return View(firstThings);
            }
            else
            {
                return HttpNotFound();
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
        public ActionResult Create(FirstThingDetail firstThingDetail)
        {
            if (ModelState.IsValid)
            {
                _db.FirstThingDetails.Add(firstThingDetail);
                _db.SaveChanges();

                WebCache.Remove(cacheIndexKeyFirstThingDetails);

                return RedirectToAction("Index", new { id = firstThingDetail.FirstId });
            }

            return View(firstThingDetail);
        }

        // GET: FirstThingDetail/Edit/5
        public ActionResult Edit(int id)
        {
            var model = _db.FirstThingDetails.Find(id);

            return View(model);
        }

        // POST: FirstThingDetail/Edit/5
        [HttpPost]
        [Authorize(Users = "reza.baby@yahoo.com")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FirstThingDetail firstDetail)
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

        // GET: FirstThingDetail/Delete/5
        [Authorize(Users = "reza.baby@yahoo.com")]
        public ActionResult Delete(int id)
        {
            var itemToRemove = _db.FirstThingDetails.SingleOrDefault(f => f.ID == id);
            _db.FirstThingDetails.Remove(itemToRemove);
            _db.SaveChanges();

            WebCache.Remove(cacheIndexKeyFirstThingDetails);

            return RedirectToAction("Index", new { id = itemToRemove.FirstId });
        }
    }
}
