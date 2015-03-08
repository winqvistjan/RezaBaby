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
            _db.FirstThing.Remove(itemToRemove);
            _db.SaveChanges();

            WebCache.Remove(cacheIndexKeyFirstThings);
            WebCache.Remove(cacheIndexKeyFirstThingDetails);

            return RedirectToAction("Index");
        }
    }
}
