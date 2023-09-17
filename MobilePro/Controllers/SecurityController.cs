﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using MobilePro.Models;
using WebMatrix.WebData;
using System.Text.RegularExpressions;

namespace MobilePro.Controllers
{
    public class SecurityController : Controller
    {      
        #region CommonFunctions

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion         

        #region QueryUser

        public ActionResult QueryUser()
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                if (Request.IsAuthenticated)
                {
                    try
                    {
                        ViewBag.Message = "";
                    }
                    catch
                    {

                    }
                    var count = context.security_Roles.AsEnumerable().Count();
                    if (count > 0)
                    {
                        ViewBag.RoleList = (from p in context.security_Roles
                                            select new SelectListItem
                                            {
                                                Text = p.Description,
                                                Value = p.RoleName
                                            }).ToList();
                    }
                    else
                    {
                        ViewBag.RoleList = new SelectListItem
                        {
                            Text = "",
                            Value = ""
                        };
                    }

                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        private DataSet ReturnQueryUser(string FirstName, string Role, string Status)
        {
            DataSet dsQuery = new DataSet();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[5];
                
                sqlParams[0] = new SqlParameter("@FirstName", SqlDbType.VarChar, 256);
                if (Shared.ToString(FirstName).Length > 0)
                    sqlParams[0].Value = FirstName;
                else
                    sqlParams[0].Value = DBNull.Value;
                sqlParams[1] = new SqlParameter("@RoleName", SqlDbType.VarChar, 50);
                if (Shared.ToString(Role).Length > 0)
                    sqlParams[1].Value = Role;
                else
                    sqlParams[1].Value = DBNull.Value;
                sqlParams[2] = new SqlParameter("@IsActive", SqlDbType.SmallInt);
                if (Shared.ToString(Status).Length > 0)
                    sqlParams[2].Value = Shared.ToInt(Status);
                else
                    sqlParams[2].Value = DBNull.Value;
                sqlParams[3] = new SqlParameter("@UserId", SqlDbType.Int);
                sqlParams[3].Value = WebSecurity.CurrentUserId;
                sqlParams[4] = new SqlParameter("@PageName", SqlDbType.VarChar, 50);
                sqlParams[4].Value = "QueryUser";

                dsQuery = db_data_access.FetchRS(CommandType.StoredProcedure, "sp_frm_Security_get_Users", sqlParams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);
            }

            return dsQuery;
        }

        public List<QueryUserGridModel> GetListUsers(string FirstName, string Role, string Status)
        {
            DataSet ds = ReturnQueryUser(FirstName, Role, Status);

            if (ds.Tables[0].Rows.Count > 0)
            {
                var trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new QueryUserGridModel
                {
                    //UserID = Shared.ToString(dataRow.Field<Guid>("Id")),
                    Email = dataRow.Field<string>("Email"),
                    UserName = dataRow.Field<string>("UserName"),
                    FirstName = dataRow.Field<string>("FirstName"),
                    LastName = dataRow.Field<string>("LastName"),
                    ContactNo = dataRow.Field<string>("ContactNo"),
                    Roles = dataRow.Field<string>("Roles"),
                    UpdatedBy = dataRow.Field<string>("UpdatedBy"),
                    UpdatedDate = dataRow.Field<string>("UpdatedDate"),
                    IsApproved = Shared.ToString(dataRow.Field<Boolean>("IsConfirmed"))
                });
                return trxnList.ToList();
            }
            else

                return new List<QueryUserGridModel>();
        }

        public JsonResult QueryUsersList(string FirstName, string Role, string Status)
        {
            try
            {
                List<QueryUserGridModel> query = GetListUsers(FirstName, Role, Status);

                return Json(new
                {
                    aaData = query.Select(x => new[] { x.UserName, x.FirstName, x.LastName, x.Email, x.ContactNo, x.Roles, x.IsApproved, x.UpdatedBy, x.UpdatedDate }) 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        #endregion

        #region CreateEditUser
        public ActionResult CreateUser(string Success)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                CreateUserModel model = new CreateUserModel();

                model.RoleMaster = (from p in context.security_Roles
                                        select new UserRole
                                        {
                                           RoleID = p.RoleId,
                                           RoleName = p.RoleName,
                                           Selected = false
                                        }).ToList();               


                List<SelectListItem> StatusList = new List<SelectListItem>();
                StatusList.Add(new SelectListItem { Text = "Active", Value = "1" });
                StatusList.Add(new SelectListItem { Text = "Inactive", Value = "0" });
                ViewBag.StatusList = StatusList;

                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(CreateUserModel model, FormCollection frm)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    // Has permission to view the page?
                    if (!Request.IsAuthenticated)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    else
                    {
                        var selectedGenres = model.RoleMaster.Where(x => x.Selected).Select(x => x.RoleID).ToList();
                        if (selectedGenres.Count == 0) ModelState.AddModelError(string.Empty, "Role is Required.");

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { FirstName = model.FirstName, LastName = Shared.ToString(model.LastName), Email = model.Email, ContactNo = model.ContactNo, CreatedBy = WebSecurity.CurrentUserId, CreatedDate = DateTime.Now });

                                foreach(int role in selectedGenres)
                                {
                                    context.Database.ExecuteSqlCommand("insert into security_UsersInRoles(UserID,RoleID) values('" + WebSecurity.GetUserId(model.UserName) + "','" + role + "')");
                                    context.SaveChanges();
                                }

                                return RedirectToAction("EditUser", "Security", new { Success = "Successfully Added", UserID = WebSecurity.GetUserId(model.UserName) });

                            }
                            catch (MembershipCreateUserException e)
                            {
                                ModelState.AddModelError(string.Empty, ErrorCodeToString(e.StatusCode));
                            }

                        }
                        else
                        {
                            return View(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);

                    return View(model);
                }

                finally
                {
                    List<SelectListItem> StatusList = new List<SelectListItem>();
                    StatusList.Add(new SelectListItem { Text = "Active", Value = "True" });
                    StatusList.Add(new SelectListItem { Text = "Inactive", Value = "False" });
                    ViewBag.StatusList = StatusList;

                }
                return View(model);
            }
        }

        public ActionResult EditUser(string UserId, string Success)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                EditUserModel model = new EditUserModel();

                try
                {
                    ViewBag.Message = Success;
                    int? _userid = Shared.ToInt(UserId);

                    model = (from c in context.sp_frm_Security_get_Users(null, null, null, WebSecurity.CurrentUserId, "QueryUser")
                             select new EditUserModel
                             {
                                 UserID = c.UserId,
                                 UserName = Shared.ToString(c.UserName).Length > 0 ? Regex.Match(c.UserName, @"<a [^>]*>(.*?)</a>").Groups[1].Value : c.UserName,
                                 FirstName = Shared.ToString(c.FirstName),
                                 LastName = c.LastName,
                                 Email = Shared.ToString(c.Email),
                                 ContactNo = c.ContactNo,
                                 RoleMaster = (from p in context.security_Roles
                                               select new UserRole
                                               {
                                                   RoleID = p.RoleId,
                                                   RoleName = p.RoleName,
                                                   Selected = c.Roles.Contains(p.RoleName)
                                               }).ToList(),
                                 Status = c.IsConfirmed == true ? 1 : 0
                             }).FirstOrDefault(i => i.UserID == _userid);

                   
                }
                catch (Exception ex)
                {
                    return View(model);
                }
                finally
                {

                    List<SelectListItem> StatusList = new List<SelectListItem>();
                    StatusList.Add(new SelectListItem { Text = "Active", Value = "True" });
                    StatusList.Add(new SelectListItem { Text = "Inactive", Value = "False" });
                    ViewBag.StatusList = StatusList;

                }
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(EditUserModel model, FormCollection frm)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    // Has permission to view the page?
                    if (!Request.IsAuthenticated)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    else
                    {
                        var selectedGenres = model.RoleMaster.Where(x => x.Selected).Select(x => x.RoleID).ToList();
                        if (selectedGenres.Count == 0) ModelState.AddModelError(string.Empty, "Role is Required.");

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { FirstName = model.FirstName, LastName = model.LastName, Email = model.Email, ContactNo = model.ContactNo, CreatedBy = WebSecurity.CurrentUserId, CreatedDate = DateTime.Now });
                                context.Database.ExecuteSqlCommand("update UserProfile set [FirstName] = '" + model.FirstName + "', [LastName] = '" + Shared.ToString(model.LastName) + "', [Email] = '" + model.Email + "', [ContactNo] = '" + model.ContactNo + "', [UpdatedBy] = '" + WebSecurity.CurrentUserId + "', [UpdatedDate] = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where UserID = '" + WebSecurity.GetUserId(model.UserName) + "'");
                                context.SaveChanges();

                                context.Database.ExecuteSqlCommand("delete from security_UsersInRoles where UserID = '" + WebSecurity.GetUserId(model.UserName) + "'");
                                context.SaveChanges();

                                foreach (int role in selectedGenres)
                                {
                                    context.Database.ExecuteSqlCommand("insert into security_UsersInRoles(UserID,RoleID) values('" + WebSecurity.GetUserId(model.UserName) + "','" + role + "')");
                                    context.SaveChanges();
                                }

                                return RedirectToAction("EditUser", "Security", new { Success = "Successfully Updated", UserID = WebSecurity.GetUserId(model.UserName) });

                            }
                            catch (MembershipCreateUserException e)
                            {
                                ModelState.AddModelError(string.Empty, ErrorCodeToString(e.StatusCode));
                            }

                        }
                        else
                        {
                            return View(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);

                    return View(model);
                }

                finally
                {
                    List<SelectListItem> StatusList = new List<SelectListItem>();
                    StatusList.Add(new SelectListItem { Text = "Active", Value = "True" });
                    StatusList.Add(new SelectListItem { Text = "Inactive", Value = "False" });
                    ViewBag.StatusList = StatusList;

                }
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(string UserName, string Password)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    MembershipUser mu = Membership.GetUser(UserName);
                    mu.ChangePassword(mu.ResetPassword(), Password);

                    return Json(new { success = true, response = "Password Successfully Updated!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message });
                }
            }
        }
        #endregion
    }
}
