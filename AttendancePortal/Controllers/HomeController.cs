using System;
using System.Web.Mvc;
using AttendancePortal.Repository;
using AttendancePortal.Common;

namespace AttendancePortal.Controllers
{
    public class HomeController : Controller
    {

        Authorized _authorized = null;

        /// <summary>
        /// Employee's windows user name.
        /// </summary>
        private string _userName = string.Empty;
        private string _applicationname = "AttendancePortal";
        private string _error = "Error";
        private string _debug = "Debug";
        private string ErrorUrl = String.Format("/Home/Error");

        /// <summary>
        /// Login Index view.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                ViewBag.ErrorMessage = "";
                _authorized = new Authorized();

                if (Request.Browser.IsMobileDevice)
                {
                    _authorized.SaveAttendancePortalError(_applicationname, "Request.Browser.IsMobileDevice", "Index", "Request.Browser.IsMobileDevice", "Request.Browser.IsMobileDevice", _debug, _userName);
                    
                    return Redirect(ErrorUrl);
                }

                ViewBag.ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _authorized.SaveAttendancePortalError(_applicationname, ex.Message, "Index-Post", ex.InnerException.ToString(), ex.StackTrace, _error, _userName);
            }
            finally
            {
                _authorized = null;
            }
            return View();
        }

        /// <summary>
        /// Index view
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string ID)
        {
            try
            {
                if (!Request.Browser.IsMobileDevice)
                {
                    ViewBag.ErrorMessage = "";

                    _authorized = new Authorized();

                    _userName = Request["UserName"].ToString();
                    //Added by Ajit for Encrypt Username
                    _userName = CommonClass.EncodeBase64(_userName.ToString());
                    _userName = CommonClass.DecodeBase64(_userName.ToString());

                    if (!String.IsNullOrWhiteSpace(_userName))
                    {
                        if (_authorized.IsAuthorized(_userName))
                        {
                            string _url = CommonClass.AuthorizeUser(_userName);
                            return Redirect(_url);
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Sorry, The user name is incorrect. Please enter the correct user name.";
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "The user name is required.";
                    }
                }
                else
                {
                    return Redirect(ErrorUrl);
                }
            }
            catch (Exception ex)
            {
                _authorized.SaveAttendancePortalError(_applicationname, ex.Message, "Index-Post", ex.InnerException.ToString(), ex.StackTrace, _error, _userName);
            }
            finally
            {
                _authorized = null;
            }

            return View();
        }

        /// <summary>
        /// Logged In view
        /// </summary>
        /// <returns></returns>
        public ActionResult LoggedIn()
        {
            try
            {
                if (!Request.Browser.IsMobileDevice)
                {
                    string Error = Request.QueryString["error"];

                    string Code = Request.QueryString["code"];
                    if (Error != null)
                    {

                    }
                    else if (Code != null)
                    {
                        string UserName = Request.QueryString["state"];
                      

                        string Id = Convert.ToString(UserName);

                        string AccessToken = string.Empty;
                        string RefreshToken = CommonClass.ExchangeAuthorizationCode(Id, Code, out AccessToken);

                        string EmailId = string.Empty;
                        if (AccessToken != null && AccessToken != "")
                            EmailId = CommonClass.FetchEmailId(AccessToken);

                        string Url = String.Format("/Home/MarkTime?uid={0}&id={1}", UserName, CommonClass.EncodeBase64(EmailId.ToString()));
                        return Redirect(Url);
                    }
                }
                else
                {
                    return Redirect(ErrorUrl);
                }
            }
            catch (Exception ex)
            {
                _authorized = new Authorized();
                _authorized.SaveAttendancePortalError(_applicationname, ex.Message, "Index-Post", ex.InnerException.ToString(), ex.StackTrace, _error, _userName);
            }
            finally
            {
                _authorized = null;
            }
            return View();
        }

        /// <summary>
        /// Mark time view.
        /// </summary>
        /// <returns></returns>
        public ActionResult MarkTime()
        {
            try
            {
                if (!Request.Browser.IsMobileDevice)
                {
                    _authorized = new Authorized();

                    _userName = string.Empty;
                    string _emailId = string.Empty;

                    _userName = CommonClass.DecodeBase64(Request.QueryString["uid"]);
                    _emailId = CommonClass.DecodeBase64(Request.QueryString["id"]);

                    if (_authorized.IsEmailIDValid(_userName, _emailId))
                    {
                        _authorized.MarkTimeinDB(_userName);

                        ViewBag.ErrorMessage = "Thanks, Your time has been marked.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Sorry, Your time has not been marked. Entered mail ID hasn't matched with your office mail ID.";
                    }
                }
                else
                {
                    return Redirect(ErrorUrl);
                }
            }
            catch (Exception ex)
            {
                _authorized.SaveAttendancePortalError(_applicationname, ex.Message, "MarkTime", ex.InnerException.ToString(), ex.StackTrace, _error, _userName);
            }
            finally
            {
                _authorized = null;
            }
            return View();
        }

        /// <summary>
        /// Mark time view.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MarkTime(string id)
        {
            try
            {
                if (!Request.Browser.IsMobileDevice)
                {
                    string Url = String.Format("/Home/Index");
                    return Redirect(Url);
                }
                else
                {
                    return Redirect(ErrorUrl);
                }
            }
            catch (Exception ex)
            {
                _authorized = new Authorized();
                _authorized.SaveAttendancePortalError(_applicationname, ex.Message, "MarkTime-Post", ex.InnerException.ToString(), ex.StackTrace, _error, _userName);
            }
            finally
            {
                _authorized = null;
            }
            return View();
        }

        /// <summary>
        /// error page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Error()
        {
            try
            {
                ViewBag.ErrorMessage = "The application does not support this device.";
            }
            catch (Exception ex)
            {
                _authorized = new Authorized();
                _authorized.SaveAttendancePortalError(_applicationname, ex.Message, "MarkTime-Post", ex.InnerException.ToString(), ex.StackTrace, _error, _userName);
            }
            finally
            {
                _authorized = null;
            }
            return View();
        }
    }
}