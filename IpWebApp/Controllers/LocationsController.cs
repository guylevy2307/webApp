﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IpWebApp.Models;

namespace IpWebApp.Controllers
{
    public class LocationsController : Controller
    {
        private IpDbContext db = new IpDbContext();

        // GET: Locations
        [Authorize]
        public ActionResult Index()
        {
            var locations1 = db.Locations.Include(l => l.Client);
            return View(locations1.ToList());
        }

        // GET: Locations/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location location = db.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        // GET: Locations/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.LocationId = new SelectList(db.Client, "ClientId", "Name");
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "LocationId,PriorityCountry,Latitude,Longitude,creatorId")] Location location)
        {
            if (ModelState.IsValid)
            {
                Client client = db.Client.Find( location.LocationId);
                Location temp = db.Locations.Find(location.LocationId);
                if (client != null)
                {
                    //need to update the location in client table

                    db.Entry(client).State = EntityState.Modified;
                    client.Location = location;
                 
                }
               
                else if (temp!=null)

                {
                    //need to update the location in location table
                    db.Entry(temp).State = EntityState.Modified;

                }
                //no need to update nothing just create new
                db.Locations.Add(location);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LocationId = new SelectList(db.Client, "ClientId", "Name", location.LocationId);
            return View(location);
        }

        // GET: Locations/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location location = db.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            //checking permissions
            if (!(location.creatorId.Equals(User.Identity.Name)) || User.IsInRole("Admin"))
            {
                return RedirectToAction("NoPremission", "Home");
            }
            ViewBag.LocationId = new SelectList(db.Client, "ClientId", "Name", location.LocationId);
            return View(location);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "LocationId,PriorityCountry,Latitude,Longitude")] Location location)
        {
            if (ModelState.IsValid)
            {
                db.Entry(location).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LocationId = new SelectList(db.Client, "ClientId", "Name", location.LocationId);
            return View(location);
        }

        // GET: Locations/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Location location = db.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Location location = db.Locations.Find(id);
            db.Locations.Remove(location);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //cheking how many records there are in each country
        public ActionResult GroupByCountry()
        {
            List<Record> records = db.Record.ToList();
            List<Location> locations = db.Locations.ToList();
            var result = from record in records
                         join location in locations
                         on record.Country
                         equals location.PriorityCountry
                         select new { record.Country };
            var groupedResult = from r in result
                                group r by r.Country into grp
                                select new RecoredLocation { key = grp.Key, cnt = grp.Count() };
                ViewBag.data = groupedResult.ToList();
            return View();
        }

        public class RecoredLocation
        {
            public string key { get; set; }
            public int cnt { get; set; }
        }
    }

    
}
