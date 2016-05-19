using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _420_476_ProjetFinal_Desrosiers_Pucacco_Lam.Controllers
{
    public class MessagingController : Controller
    {

        private BDProjetEntities db = new BDProjetEntities();

        public ActionResult Message(string creatorId, string title, string itemId, string msgType)
        {
            if (Session["ConnectedUserID"] != null)
            {
                ViewBag.CreatorId = creatorId;
                ViewBag.Title = title;
                int newId = Convert.ToInt32(itemId);
                if (msgType.Equals("Offer"))
                {
                    Offer offer = db.Offers.Where(o => o.id == newId).FirstOrDefault();
                    ViewBag.msgTitle = "Communiqué pour " + "'" + offer.title + "'";
                }
                else
                {
                    Request request = db.Requests.Where(o => o.id == newId).FirstOrDefault();
                    ViewBag.msgTitle = "Communiqué pour " + "'" + request.title + "'";
                }
                ViewBag.returnType = msgType;
                ViewBag.returnItemId = itemId;
                return View();
            }
            else {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult Message([Bind(Include = "title,targetUserId,message1")] Message msg, string msgType, string itemId)
        {
            if (Session["ConnectedUserID"] != null)
            {
                if (!string.IsNullOrEmpty(msg.message1) && !string.IsNullOrEmpty(msg.title))
                {
                    if (ModelState.IsValid)
                    {
                        msg.sourceUserId = (int)Session["ConnectedUserID"];
                        db.Messages.Add(msg);
                        db.SaveChanges();
                        return RedirectToAction("ViewMessages", "Messaging");
                    }
                    else {
                        return RedirectToAction("ViewMessages", "Messaging");
                    }
                }

                ViewBag.SendFail = "Veuillez vous assurer que votre titre/message n'est pas vide";
                ViewBag.CreatorId = msg.sourceUserId;
                ViewBag.msgTitle = msg.title;
                ViewBag.returnType = msgType;
                ViewBag.returnItemId = itemId;
                return View();
            }
            else {
                return RedirectToAction("Login", "Account");
            }
        }


        //[HttpPost]
        //public ActionResult SendMessage([Bind(Include = "title,targetUserId,message1")] Message msg)
        //{
        //    if (Session["ConnectedUserID"] != null)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            msg.sourceUserId = (int)Session["ConnectedUserID"];
        //            db.Messages.Add(msg);
        //            db.SaveChanges();
        //            return RedirectToAction("ViewMessages", "Messaging");
        //        }
        //        else {
        //            return RedirectToAction("ViewMessages", "Messaging");
        //        }
        //    }
        //    else {
        //        return RedirectToAction("Login", "Account");
        //    }
        //}

        public ActionResult ReplyToMessage(string title, string targetUserId)
        {
            if (Session["ConnectedUserID"] != null)
            {
                ViewBag.TargetUserId = targetUserId;
                ViewBag.Title = title;
                return View();
            }
            else {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult ReplyToMessage([Bind(Include = "title,targetUserId,message1")] Message msg)
        {
            if (Session["ConnectedUserID"] != null)
            {
                if (!string.IsNullOrEmpty(msg.message1) && !string.IsNullOrEmpty(msg.title))
                {
                    if (ModelState.IsValid)
                    {
                        msg.sourceUserId = (int)Session["ConnectedUserID"];
                        db.Messages.Add(msg);
                        db.SaveChanges();
                        return RedirectToAction("ViewMessages", "Messaging");
                    }
                    else
                    {
                        return RedirectToAction("ViewMessages", "Messaging");
                    }

                }
                else {
                    ViewBag.TargetUserId = msg.targetUserId;
                    ViewBag.Title = msg.title;
                    ViewBag.SendFail = "Veuillez vous assurer que votre titre/message n'est pas vide";
                    return View();
                }
            }
            else {
                return RedirectToAction("Login", "Account");
            }
        }


        [HttpPost]
        public ActionResult Redirect(string itemId, string msgType)
        {
            if (Session["ConnectedUserID"] != null)
            {
                if (msgType == "Offer")
                {
                    return RedirectToAction("Details", "Offers", new { id = Convert.ToInt32(itemId) });
                }
                else {
                    return RedirectToAction("Details", "Requests", new { id = Convert.ToInt32(itemId) });
                }
            }
            else {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult ViewMessages()
        {
            if (Session["ConnectedUserID"] != null)
            {
                int currentUserID = (int)Session["ConnectedUserID"];
                var receivedMsgs = db.Messages.Where(m => m.targetUserId == currentUserID);
                var sentMsgs = db.Messages.Where(m => m.sourceUserId == currentUserID);

                ViewBag.Received = receivedMsgs.ToList();
                ViewBag.Sent = sentMsgs.ToList();

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
    }
}