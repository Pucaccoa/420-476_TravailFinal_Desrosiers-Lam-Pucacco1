using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _420_476_ProjetFinal_Desrosiers_Pucacco_Lam;
using System.IO;

namespace _420_476_ProjetFinal_Desrosiers_Pucacco_Lam.Controllers
{
    public class RequestsController : Controller
    {
        private BDProjetEntities db = new BDProjetEntities();

        // GET: Requests
        public ActionResult Index(int? page)
        {

            if (page == null)
            {
                page = 1;
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "id", "categoryName");
            var requests = db.Requests.Include(r => r.Category).Include(r => r.User).Include(r => r.User1).OrderBy(o => o.dateCreated).Skip((int)(page - 1) * 5).Take(5);
            var requestsList = requests.ToList();
            var Requests1 = db.Requests;
            var total = Requests1.ToList().Count();
            var numOfRequest = total;
            
            ViewBag.nbRequest = numOfRequest;
            return View(requestsList);
        }

        [HttpPost]
        public ActionResult Index(int categoryId, string offerTitle, int? page)
        {
            if (page == null)
            {
                page = 1;
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "id", "categoryName");

            if (offerTitle != "")
            {
                ViewBag.OfferTitle = offerTitle;
                var requests = db.Requests.Where(r => r.title.Contains(offerTitle) && r.categoryId == categoryId).Include(r => r.Category).Include(r => r.User).Include(r => r.User1).OrderBy(o => o.dateCreated).Skip((int)(page - 1) * 5).Take(5);
                var requestsList = requests.ToList();
                var Requests1 = db.Requests;
                var total = Requests1.ToList().Count();
                var numOfRequest = total;
                ViewBag.nbRequest = numOfRequest;
                return View(requestsList);
            }
            else
            {
                var requests = db.Requests.Where(r => r.categoryId == categoryId).Include(r => r.Category).Include(r => r.User).Include(r => r.User1).OrderBy(o => o.dateCreated).Skip((int)(page - 1) * 5).Take(5);
                var requestsList = requests.ToList();
                var Requests1 = db.Requests;
                var total = Requests1.ToList().Count();
                var numOfRequest = total;
                ViewBag.nbRequest = numOfRequest;
                return View(requestsList);
            }
        }




        // GET: Requests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Request request = db.Requests.Find(id);
            if (request == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatorId = request.User.id;
            ViewBag.Title = request.title;
            ViewBag.ItemId = request.id;
            MicrowaveCookiesRequest(id);
            return View(request);
        }

        public ActionResult Create()
        {
            if (Session["ConnectedUserID"] != null)
            {
                ViewBag.categoryId = new SelectList(db.Categories, "id", "categoryName");
                ViewBag.creatorId = new SelectList(db.Users, "id", "firstName");
                ViewBag.matchedUserID = new SelectList(db.Users, "id", "firstName");
                return View();
            }
            else
            {
                return RedirectToAction("Account", "Login");
            }
        }

        // POST: Requests/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,text,title,dateCreated,image,creatorId,matchedUserID,categoryId")] Request request)
        {
            if (Session["ConnectedUserID"] != null)
            {
                if (ModelState.IsValid)
                {


                    if (Request.Files.Count > 0)
                    {
                        var file = Request.Files[0];
                        //Vérifier le fichier soumis est non vide
                        if (file != null && file.ContentLength > 0)
                        {
                            if (MimeMapping.GetMimeMapping(file.FileName) == "image/jpg" || MimeMapping.GetMimeMapping(file.FileName) == "image/png" || MimeMapping.GetMimeMapping(file.FileName) == "image/jpeg")
                            {
                                //Récupérer le nom du fichier soumis
                                var fileName = Path.GetFileName(file.FileName);
                                //Récupérer l'extension du fichier soumis
                                var fileExtension = Path.GetExtension(file.FileName);
                                //Créer le chemin relatif à partir du dossier du projet pour la sauvegarde du fichier téléversé
                                var path = Path.Combine(Server.MapPath("~/Content/"), fileName);
                                file.SaveAs(path);
                                request.image = fileName;
                            }
                        }
                    }
                    var creatorid = (int)Session["ConnectedUserID"];
                    var requestid = db.Requests.Count() + 1;
                    request.id = requestid;
                    request.dateCreated = DateTime.Now;
                    request.creatorId = creatorid;
                    request.matchedUserID = null;
                    db.Requests.Add(request);
                    db.SaveChanges();
                    return RedirectToAction("MyOffersAndRequests", "Account");
                }

                ViewBag.categoryId = new SelectList(db.Categories, "id", "categoryName", request.categoryId);
                ViewBag.creatorId = new SelectList(db.Users, "id", "firstName", request.creatorId);
                ViewBag.matchedUserID = new SelectList(db.Users, "id", "firstName", request.matchedUserID);
                return View(request);
            }
            else
            {
                return RedirectToAction("Account", "Login");
            }
        }

        // GET: Requests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["ConnectedUserID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Request request = db.Requests.Find(id);
                if (request == null)
                {
                    return HttpNotFound();
                }
                ViewBag.categoryId = new SelectList(db.Categories, "id", "categoryName", request.categoryId);
                ViewBag.creatorId = new SelectList(db.Users, "id", "firstName", request.creatorId);
                ViewBag.matchedUserID = new SelectList(db.Users, "id", "firstName", request.matchedUserID);
                return View(request);
            }
            else
            {
                return RedirectToAction("Account", "Login");
            }
        }

        // POST: Requests/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id, creatorID, matchedUserId,text,title,image,categoryId")] Request request)
        {
            if (Session["ConnectedUserID"] != null)
            {
                if (ModelState.IsValid)
                {
                    if (Request.Files.Count > 0)
                    {
                        var file = Request.Files[0];
                        //Vérifier le fichier soumis est non vide
                        if (file != null && file.ContentLength > 0)
                        {
                            if (MimeMapping.GetMimeMapping(file.FileName) == "image/jpg" || MimeMapping.GetMimeMapping(file.FileName) == "image/png" || MimeMapping.GetMimeMapping(file.FileName) == "image/jpeg")
                            {
                                //Récupérer le nom du fichier soumis
                                var fileName = Path.GetFileName(file.FileName);
                                //Récupérer l'extension du fichier soumis
                                var fileExtension = Path.GetExtension(file.FileName);
                                //Créer le chemin relatif à partir du dossier du projet pour la sauvegarde du fichier téléversé
                                var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                                file.SaveAs(path);
                                request.image = fileName;
                            }
                        }
                    }
                    var creatorid = (int)Session["ConnectedUserID"];
                    request.creatorId = creatorid;
                    request.dateCreated = DateTime.Now;
                    db.Entry(request).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MyOffersAndRequests", "Account");
                }
                ViewBag.categoryId = new SelectList(db.Categories, "id", "categoryName", request.categoryId);
                ViewBag.creatorId = new SelectList(db.Users, "id", "firstName", request.creatorId);
                ViewBag.matchedUserID = new SelectList(db.Users, "id", "firstName", request.matchedUserID);
                return View(request);
            }
            else
            {
                return RedirectToAction("Account", "Login");
            }
        }

        // GET: Requests/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["ConnectedUserID"] != null || Session["AdminConnected"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Request request = db.Requests.Find(id);
                if (request == null)
                {
                    return HttpNotFound();
                }
                return View(request);
            }
            else
            {
                return RedirectToAction("Account", "Login");
            }
        }
        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["ConnectedUserID"] != null || Session["AdminConnected"] != null)
            {
                Request request = db.Requests.Find(id);
                db.Requests.Remove(request);
                db.SaveChanges();
                return RedirectToAction("MyOffersAndRequests", "Account");
            }
            else
            {
                return RedirectToAction("Account", "Login");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public void MicrowaveCookiesRequest(int? id)
        {
            var request = db.Requests.Where(p => p.id == id).FirstOrDefault();
            if (Request.Cookies["MyRequests"] != null)
            {
                HttpCookie myCookie = Request.Cookies["MyRequests"];

                int numberBefore = myCookie.Values.Count;
                for (int i = myCookie.Values.Count; i <= numberBefore; i++)
                {
                    myCookie[i.ToString()] = request.id.ToString();
                    Response.Cookies.Add(myCookie);
                }
            }
            else
            {
                HttpCookie myCookie = new HttpCookie("MyRequests");
                myCookie.Expires = DateTime.Now.AddDays(1);
                myCookie["0"] = request.id.ToString();
                Response.Cookies.Add(myCookie);
            }
        }
        public Request getRequestById(int id)
        {
            Request req = null;
            req = db.Requests.Where(r => r.id == id).FirstOrDefault();
            return req;
        }
        public ActionResult getRequestList()
        {
            Dictionary<int, int> list = null;
            List<int> finalList = null;
            if (Request.Cookies["MyRequests"] != null)
            {
                list = new Dictionary<int, int>();
                for (int i = 0; i <= Request.Cookies["MyRequests"].Values.Count - 1; i++)
                {
                    Request o = getRequestById(int.Parse(Request.Cookies["MyRequests"][i.ToString()]));
                    if (!(list.ContainsKey(o.id)))
                    {
                        int hello;
                        list.Add(o.id, 1);
                        list.TryGetValue(o.id, out hello);
                    }
                    else
                    {
                        int value;
                        if (list.TryGetValue(o.id, out value))
                        {
                            list[o.id] = value + 1;
                        }
                    }
                }
                var list2 = list.ToList();
                list2.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                finalList = new List<int>();
                foreach (KeyValuePair<int, int> i in list2)
                {
                    finalList.Add(i.Key);
                }
            }
            else {
                finalList = new List<int>();
            }

            string transformedString = "";
            int cpt = 0;
            foreach (int i in finalList)
            {
                if (cpt >= 3)
                {
                    break;
                }
                Request r = getRequestById(i);
                var image = "/AppPhoto/DefaultPhoto.jpg";
                if(r.image!=null)
                {
                    image = "Images/" + r.image;
                }
                transformedString = string.Concat(transformedString, " <a href=\"/Requests/Details/" + r.id + "\" style=\"text - decoration:none; color: black\"><div class=\"row\"><h3 id = \"Title\">" + r.title + "</h3></div><div class=\"row\"><img style=\"width:100%;height:100%;\"id = \"image\" src=\"/Content/" +image + "\"/><p id=\"text\">" + r.text + "</p></div></a>");
                cpt++;
            }
            return Json(transformedString, JsonRequestBehavior.AllowGet);

        }
        public JsonResult RequestAutoComplete(string term)
        {
            var result = (from r in db.Requests
                          where r.title.ToLower().Contains(term.ToLower())
                          select new { r.title }).Distinct();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}
