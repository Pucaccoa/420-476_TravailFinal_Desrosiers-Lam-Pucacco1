using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _420_476_ProjetFinal_Desrosiers_Pucacco_Lam;
using SendGrid;
using System.Net.Mail;
using System.Web.Helpers;
using System.IO;

namespace _420_476_ProjetFinal_Desrosiers_Pucacco_Lam.Controllers
{
    public class AccountController : Controller
    {
        private BDProjetEntities db = new BDProjetEntities();

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string login, string password)
        {
            if (Session["ConnectedUserID"] == null)
            {
                if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                {
                    foreach (User u in db.Users)
                    {
                        if (u.login == login)
                        {
                            bool pass = CheckPassword(u.password, password);
                            if (pass == true)
                            {
                                Session["ConnectedUserID"] = u.id;
                                Session["ConnectedUserName"] = u.firstName + " " + u.lastName;
                                return RedirectToAction("Index", "Offers");
                            }
                        }
                    }
                    foreach (Admin a in db.Admins)
                    {
                        if (a.login == login)
                        {
                            bool pass = CheckPassword(a.password, password);
                            if (pass == true)
                            {
                                Session["ConnectedUserName"] = a.firstName + " " + a.lastName;
                                Session["AdminConnected"] = true;
                                return RedirectToAction("Index", "Offers");
                            }
                        }
                    }

                    ViewBag.LoginFail = "Login/Mot de passe invalid";
                    return View();
                }
                else {
                    ViewBag.LoginFail = "Login/Mot de passe invalid";
                    return View();
                }
            }
            else {
                return RedirectToAction("Index", "Offers");
            }
        }

        public ActionResult SignUp()
        {
            if (Session["ConnectedUserID"] != null || Session["AdminConnected"] != null)
            {
                return RedirectToAction("Index", "Offers");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult SignUp([Bind(Include = "firstName,lastName,email,login,password,description")] User user)
        {
            ViewBag.Login = user.login;
            ViewBag.FirstName = user.firstName;
            ViewBag.Email = user.email;
            ViewBag.LastName = user.lastName;
            ViewBag.Description = user.description;

            if (!string.IsNullOrEmpty(user.login) && !string.IsNullOrEmpty(user.password) && !string.IsNullOrEmpty(user.firstName) && !string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.lastName) && !string.IsNullOrEmpty(user.description))
            {

                if (checkAvailability(user.login))
                {
                    if (ModelState.IsValid)
                    {
                        string contactPass = user.password;
                        string pass = hashPassword(user.password);
                        user.password = pass;
                        user.id = getAutoId();
                        user.image = null;
                        db.Users.Add(user);
                        db.SaveChanges();
                        string fullName = user.firstName + " " + user.lastName;
                        sendRegistrationEmail(user.email, fullName, user.login, contactPass);
                        return RedirectToAction("Login", "Account");
                    }

                }
                else {
                    ViewBag.ErrorMessage = "Identifiant choisi est non disponible";
                }
            }
            else {
                ViewBag.ErrorMessage = "Un des champs n'est pas rempli, veuillez le remplir";
            }
            return View();
        }


        public ActionResult EditProfile()
        {
            if (Session["ConnectedUserID"] != null)
            {
                int userID = (int)Session["ConnectedUserID"];
                var user = db.Users.Where(u => u.id == userID);
                return View(user.ToList()[0]);
            }
            else {
                return RedirectToAction("Index", "Offers");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile([Bind(Include = "id,firstName,lastName,login,email,password,image,description")] User user)
        {
            if (Session["ConnectedUserID"] != null)
            {
                if (!string.IsNullOrEmpty(user.firstName) && !string.IsNullOrEmpty(user.lastName) && !string.IsNullOrEmpty(user.email) && !string.IsNullOrEmpty(user.description))
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
                                    user.image = fileName;
                                }
                            }
                        }
                        else
                        {
                            user.image = user.image;
                        }
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                        Session["ConnectedUserName"] = user.firstName + " " + user.lastName;
                        return RedirectToAction("Index", "Offers");
                    }
                }
                ViewBag.EditFail = "Veuillez vous assurer que les champs ne sont pas vide";

                return View(user);
            }
            else {
                return View(user);
            }
        }


        public int getAutoId()
        {
            int id = db.Users.Count();
            id++;
            return id;
        }

        public ActionResult LogOut()
        {
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        public bool checkAvailability(string login)
        {
            foreach (User u in db.Users)
            {
                if (u.login == login)
                {
                    return false;
                }
            }
            foreach (Admin a in db.Admins)
            {
                if (a.login == login)
                {
                    return false;
                }
            }
            return true;
        }

        public ActionResult MyOffersAndRequests()
        {
            if (Session["ConnectedUserID"] != null)
            {
                int id = (int)Session["ConnectedUserID"];
                ViewBag.CategoryID = new SelectList(db.Categories, "id", "categoryName");
                var requests = db.Requests.Where(r => r.creatorId == id).Include(r => r.Category).Include(r => r.User).Include(r => r.User1);
                var offers = db.Offers.Where(o => o.creatorID == id).Include(o => o.Category).Include(o => o.User).Include(o => o.User1);

                ViewBag.RequestsList = requests.ToList();
                ViewBag.OffersList = offers.ToList();
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Offers");
            }
        }

        public void sendRegistrationEmail(string email, string fullName, string login, string password)
        {
            var regMessage = new SendGridMessage();

            regMessage.From = new MailAddress("echange_service_noreply@exservice.com");
            regMessage.AddTo(email);

            regMessage.Subject = "Bienvenue à Échange-Service!";
            regMessage.Html = "<div><h2>Bienvenue " + fullName + " à Échange-Service</h2><p>Merci d'avoir créer un compte Échange-Service</p><p>Voici vos informations de connexion: <br> Identifiant:" + login + " <br> Mot de passe: " + password + "</p></div>";

            var transportWeb = new Web("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");
            transportWeb.DeliverAsync(regMessage);
        }

        public string hashPassword(string password)
        {
            string salt = Crypto.GenerateSalt();
            string combinedPassword = password;

            string hashedPassword = Crypto.HashPassword(combinedPassword);
            return hashedPassword;
        }

        public bool CheckPassword(string dbHashedPassword, string UserUnhashedPassword)
        {
            return Crypto.VerifyHashedPassword(dbHashedPassword, UserUnhashedPassword);
        }
    }
}