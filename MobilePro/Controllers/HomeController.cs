using MobilePro.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace MobilePro.Controllers
{
    public class HomeController: Controller
    {
        public ActionResult RedirectPage()
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                if (Request.IsAuthenticated)
                {
                    try
                    {
                        // Get Home Page
                        string _homeURL = (from c in context.sp_frm_Security_get_UserHomePage(WebSecurity.CurrentUserId) select c).SingleOrDefault();
                        string _controller = Shared.ToString(_homeURL.Split('/')[1]);
                        string _pageName = Shared.ToString(_homeURL.Split('/')[2]);

                        return RedirectToAction(_pageName, _controller);
                    }
                    catch
                    {

                    }
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        private DataSet ReturnTodayTrxnDetails(DateTime? CreatedDate)
        {
            DataSet dsQuery = new DataSet();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[2];
                sqlParams[0] = new SqlParameter("@UserId", SqlDbType.Int);
                sqlParams[0].Value = WebSecurity.CurrentUserId;
                sqlParams[1] = new SqlParameter("@CreatedDate", SqlDbType.DateTime);
                if (CreatedDate != null)
                    sqlParams[1].Value = CreatedDate;
                else
                    sqlParams[1].Value = DBNull.Value;

                dsQuery = db_data_access.FetchRS(CommandType.StoredProcedure, "sp_frm_Dashboard", sqlParams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);
            }

            return dsQuery;
        }

        public ActionResult Index()
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                if (Request.IsAuthenticated)
                {
                    //try
                    //{
                    //    var _isAdmin = context.security_UsersInRoles.AsEnumerable().Count(p => p.UserID == WebSecurity.CurrentUserId && p.RoleID == 1);
                    //    if (_isAdmin == 0)
                    //        return Redirect(Request.UrlReferrer.AbsolutePath);                         
                        
                    //}
                    //catch { }

                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        #region Bills      

        public List<TodayReportModel> GetTodayList(DateTime? CreatedDate)
        {
            DataSet ds = ReturnTodayTrxnDetails(CreatedDate);

            if (ds.Tables[0].Rows.Count > 0)
            {
                var trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new TodayReportModel
                {
                    col1 = Shared.ToString(dataRow.Field<string>("col1")),
                    col2 = Shared.ToString(dataRow.Field<string>("col2")),
                    col3 = Shared.ToString(dataRow.Field<string>("col3")),
                    col4 = Shared.ToString(dataRow.Field<string>("col4")),
                    col5 = Shared.ToString(dataRow.Field<string>("col5")),
                });
                return trxnList.ToList();
            }
            else

                return new List<TodayReportModel>();
        }

        public JsonResult QueryTodayList(string CreatedDate)
        {
            try
            {
                DateTime? _servicedate = null;

                if (Shared.ToString(CreatedDate).Length > 0)
                {
                    _servicedate = DateTime.ParseExact(CreatedDate.Trim().ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                List<TodayReportModel> query = GetTodayList(_servicedate);

                return Json(new
                {
                    aaData = query.Select(x => new[] { x.col1, x.col2, x.col3, x.col4, x.col5 })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }       

        #endregion                 

        public ActionResult Contact()
        {
            return View();
        }

        #region Menu        

        public IList<ChildMenu> GetChildMenus(int? parentid)
        {
            IList<ChildMenu> result;

            using (MobileProEntities context = new MobileProEntities())
            {
                result = (from du in context.sp_frm_Security_TreeViewMenu(WebSecurity.CurrentUserId, "SUBMENU", parentid)
                          select new ChildMenu
                          {
                              PageID = du.PageID,
                              PageTitle = du.PageTitle,
                              NavigateUrl = du.PageURL
                          }).ToList();
            }

            return result;
        }

        public ActionResult Menu()
        {
            IEnumerable<ParentMenu> menu = null;

            using (MobileProEntities context = new MobileProEntities())
            {
                if (Request.IsAuthenticated)
                {
                    try
                    {
                       var  menu1 = (from app in context.sp_frm_Security_TreeViewMenu(WebSecurity.CurrentUserId, "MAIN", 0)
                                select new ParentMenu
                                {
                                    ID = app.PageID,
                                    Text = app.PageTitle,
                                    PageUrl = app.PageURL,
                                    childs = GetChildMenus(app.PageID)
                                }).ToList();
                        menu = menu1.AsEnumerable();
                    }
                    catch
                    {

                    }

                    var auth = context.security_UsersInRoles.AsEnumerable().Count(p => p.UserID == WebSecurity.CurrentUserId && (p.RoleID == 1 || p.RoleID == 9));
                    if (auth > 0)
                        ViewBag.UserAuth = "true";
                    else
                        ViewBag.UserAuth = "false";

                    return PartialView(menu);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        #endregion

        #region CommonFunctions

        #endregion    
    }
}