using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RezaBaby.Controllers
{
    public class GrowthController : Controller
    {
        // GET: Growth
        public ActionResult Index()
        {
            return View();
        }

        // GET: Growth/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Growth/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Growth/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Growth/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Growth/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Growth/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Growth/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
