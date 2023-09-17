using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using MobilePro.Models;
using WebMatrix.WebData;
using System.Globalization;
using System.Data;

namespace MobilePro.Controllers
{
    public class BillingController : Controller
    {      
        #region CommonFunctions     

        public static string GenerateBillNo()
        {
            string _patientid = "";
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    var count = context.ServiceBills.AsEnumerable().Count();
                    if (count > 0)
                    {
                        var _existId = (from c in context.ServiceBills orderby c.CreatedDate descending where c.CreatedBy == WebSecurity.CurrentUserId select c.ReceiptNo).FirstOrDefault();
                        if (_existId != null)
                        {
                            _existId = Shared.ToString(_existId).Split('-')[1];
                            _patientid = "R" + "-" + ((Int32.Parse(_existId) + 1).ToString("D4").Trim());
                        }
                        else { _patientid = "R" + "-" + "0001"; }
                    }
                    else
                    {
                        _patientid = "R" + "-" + "0001";
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return _patientid;
            }
        }

        #endregion

        #region ListBills
      
        public ActionResult ListBills(string Success)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                if (Request.IsAuthenticated)
                {
                    try
                    {
                        ViewBag.Message = Success;
                    }
                    catch
                    {

                    }
                    finally
                    {
                        var count = context.Status.AsEnumerable().Count();
                        if (count > 0)
                        {
                            ViewBag.StatusList = context.Status.ToList().Select(c => new SelectListItem
                            {
                                Value = c.StatusCode.ToString(),
                                Text = c.StatusName
                            });
                        }
                        else
                        {
                            ViewBag.StatusList = new SelectListItem
                            {
                                Text = "",
                                Value = ""
                            };
                        }

                        ViewBag.BrandList = (from c in context.sp_frm_get_parm_Brands(WebSecurity.CurrentUserId, "", null)
                                             select new SelectListItem
                                             {
                                                 Text = c.BrandName,
                                                 Value = Shared.ToString(c.BrandCode)
                                             }).ToList();


                        ViewBag.StatusListModel = (from p in context.sp_frm_get_Parm_Status(WebSecurity.CurrentUserId, "")
                                              where p.StatusCode == 2 || p.StatusCode == 3 || p.StatusCode == 4
                                              select new SelectListItem
                                              {
                                                  Text = p.StatusName,
                                                  Value = p.StatusCode.ToString()
                                              }).ToList();
                    }

                    BillsListModel model = new BillsListModel();
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        private DataSet ReturnQueryBill(string ReceiptNo, string CustomerName, DateTime? ServiceDate, string BrandCode, string ModelNo, string StatusCode, string ParmType)
        {
            DataSet dsQuery = new DataSet();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[10];
                sqlParams[0] = new SqlParameter("@ServiceID", SqlDbType.Int);
                sqlParams[0].Value = DBNull.Value;
                sqlParams[1] = new SqlParameter("@ReceiptNo", SqlDbType.NVarChar, 50);
                if (Shared.ToString(ReceiptNo).Length > 0)
                    sqlParams[1].Value = ReceiptNo;
                else
                    sqlParams[1].Value = DBNull.Value;
                sqlParams[2] = new SqlParameter("@CustomerName", SqlDbType.NVarChar, 100);
                if (Shared.ToString(CustomerName).Length > 0)
                    sqlParams[2].Value = CustomerName;
                else
                    sqlParams[2].Value = DBNull.Value;
                sqlParams[3] = new SqlParameter("@ModelNo", SqlDbType.NVarChar, 50);
                if (Shared.ToString(ModelNo).Length > 0)
                    sqlParams[3].Value = ModelNo;
                else
                    sqlParams[3].Value = DBNull.Value;
                sqlParams[4] = new SqlParameter("@BrandCode", SqlDbType.Int);
                if (Shared.ToString(BrandCode).Length > 0)
                    sqlParams[4].Value = Shared.ToInt(BrandCode);
                else
                    sqlParams[4].Value = DBNull.Value;
                sqlParams[5] = new SqlParameter("@ServiceDate", SqlDbType.DateTime);
                if (ServiceDate != null)
                    sqlParams[5].Value = ServiceDate;
                else
                    sqlParams[5].Value = DBNull.Value;               
                sqlParams[6] = new SqlParameter("@StatusCode", SqlDbType.Int);
                if (Shared.ToString(StatusCode).Length > 0)
                    sqlParams[6].Value = StatusCode;
                else
                    sqlParams[6].Value = DBNull.Value;
               
                sqlParams[7] = new SqlParameter("@UserId", SqlDbType.Int);
                sqlParams[7].Value = WebSecurity.CurrentUserId;
                sqlParams[8] = new SqlParameter("@PageName", SqlDbType.VarChar, 50);
                sqlParams[8].Value = "Bills";
                sqlParams[9] = new SqlParameter("@ParmType", SqlDbType.Int);
                sqlParams[9].Value = Shared.ToInt(ParmType) == 0 ? 1 : 2 ;

                dsQuery = db_data_access.FetchRS(CommandType.StoredProcedure, "sp_frm_get_Bills", sqlParams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);
            }

            return dsQuery;
        }

        public List<BillsListModel> GetListBill(string ReceiptNo, string CustomerName, DateTime? ServiceDate, string BrandCode, string ModelNo, string StatusCode, string ParmType)
        {
            DataSet ds = ReturnQueryBill(ReceiptNo, CustomerName, ServiceDate, BrandCode, ModelNo, StatusCode, ParmType);

            if (ds.Tables[0].Rows.Count > 0)
            {
                var trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new BillsListModel
                {
                    ServiceID = Shared.ToString(dataRow.Field<Int64>("ServiceID")),
                    ReceiptNo = dataRow.Field<string>("ReceiptNo"),
                    ServiceDate = Shared.ToString(dataRow.Field<DateTime>("ServiceDate").ToString("MMM dd yyyy h:mm tt")),
                    CustomerName = dataRow.Field<string>("CustomerName"),
                    ContactNo = dataRow.Field<string>("ContactNo"),
                    IMEINo = dataRow.Field<string>("IMEINo"),
                    BrandName = dataRow.Field<string>("BrandName"),
                    ModelNo = dataRow.Field<string>("ModelNo"),
                    ColorName = dataRow.Field<string>("ColorName"),
                    NatureFault = dataRow.Field<string>("NatureFault"),
                    StatusName = dataRow.Field<string>("StatusName"),
                    NetAmount = Shared.ToString(dataRow.Field<decimal>("NetAmount"))
                    
                });
                return trxnList.ToList();
            }
            else

                return new List<BillsListModel>();
        }
        
        public JsonResult QueryBillList(string ReceiptNo, string CustomerName, string ServiceDate, string BrandCode, string ModelNo, string StatusCode, string ParmType)
        {
            try
            {
                //02/02/2016 - 02/02/2016
                DateTime? _servicedate = null;

                if (Shared.ToString(ServiceDate).Length > 0)
                {
                    _servicedate = DateTime.ParseExact(ServiceDate.Trim().ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (Shared.ToString(ReceiptNo).Length > 0)
                {
                    ReceiptNo = "R" + "-" + ((Int32.Parse(ReceiptNo)).ToString("D4").Trim());
                }

                List<BillsListModel> query = GetListBill(ReceiptNo, CustomerName, _servicedate, BrandCode, ModelNo, StatusCode,ParmType);

                return Json(new
                {
                    aaData = query.Select(x => new[] { x.ServiceID, x.ReceiptNo, x.ServiceDate, x.CustomerName, x.ContactNo, x.ModelNo, x.NatureFault, x.StatusName, x.NetAmount , x.ReceiptNo})
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        public ActionResult Invoice(string BillId, string Backto)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                var _ServiceDetails = (from c in context.sp_frm_get_Bills(null,Shared.ToString(BillId), null, null, null, null, null, null, WebSecurity.CurrentUserId, "ListBills", 2) select c).FirstOrDefault();
                ViewBag.ReceiptNo = Shared.ToString(_ServiceDetails.ReceiptNo);
                ViewBag.ApplicationDate = Shared.ToString(_ServiceDetails.ServiceDate);
                ViewBag.ApplicantName = Shared.ToString(_ServiceDetails.CustomerName);
                ViewBag.NatureFault = Shared.ToString(_ServiceDetails.NatureFault);
                ViewBag.BrandModel = Shared.ToString(_ServiceDetails.BrandName) + "-" + Shared.ToString(_ServiceDetails.ModelNo);
                ViewBag.ContactNo = Shared.ToString(_ServiceDetails.ContactNo);
                ViewBag.Amount = Shared.ToInt(_ServiceDetails.NetAmount);
                
               
                ViewBag.Backto = Shared.ToString(Backto).Length > 0 ? "Yes" : null;

                return View();
            }
        }

        [HttpPost]
        public ActionResult GetBillDetails(string ServiceID)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    BillsListModel model = new BillsListModel();

                    model = (from c in context.sp_frm_get_Bills(Shared.ToInt(ServiceID), null, null, null, null, null, null, null, WebSecurity.CurrentUserId, "ListBills", 2)
                             select new BillsListModel
                             {
                                 ReceiptNo = c.ReceiptNo,
                                 CustomerName = c.CustomerName,
                                 ServiceDate = c.ServiceDate.Value.ToString("MMM dd yyyy h:mm tt"),
                                 ContactNo = c.ContactNo,
                                 BrandName = c.BrandName,
                                 ColorName = c.ColorName,
                                 ModelNo = c.ModelNo,
                                 IMEINo = c.IMEINo,
                                 NatureFault = c.NatureFault,
                                 StatusName = c.StatusName,
                                 PasswordType = c.PasswordType,
                                 Password = c.Password,
                                 NetAmount = Shared.ToString(c.NetAmount),
                                 TechRemark = c.TechRemark,
                                 CreatedBy = c.CreatedBy,
                                 CreatedDate = c.CreatedDate
                             }).FirstOrDefault();

                    if (model != null)
                    {
                        return Json(new { success = true, response = model });
                    }
                    else
                        return Json(new { success = true, response = " " });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message });
                }
            }
        }

        [HttpPost]
        public ActionResult UpdateBillStatus(string ReceiptNo, string Status)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    if (Shared.ToString(ReceiptNo).Length > 0)
                    {
                        context.Database.ExecuteSqlCommand("update ServiceBill set [StatusCode] = '" + Shared.ToInt(Status) + "', [UpdatedBy] = '" + WebSecurity.CurrentUserId + "', [UpdatedDate] = '" + DateTime.Now.AddHours(15).ToString("yyyy-MM-dd HH:mm:ss") + "'  where ReceiptNo = '" + Shared.ToString(ReceiptNo) + "' and CreatedBy = '" + WebSecurity.CurrentUserId + "'");
                        context.SaveChanges();
                    }

                    return Json(new { success = true, response = "Successfully Added!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message });
                }
            }

        }

        [HttpPost]
        public ActionResult DeleteBillDetails(string ServiceID)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    if (Shared.ToInt(ServiceID) > 0)
                    {
                        context.Database.ExecuteSqlCommand("delete from ServiceBill where ServiceID = '" + Shared.ToInt(ServiceID) + "' ");
                        context.SaveChanges();
                    }

                    return Json(new { success = true, response = "Successfully Deleted!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message });
                }
            }

        }
        #endregion

        #region CreateEditBill

        public ActionResult CreateBill(string Success)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                CreateBillModel model = new CreateBillModel();

                ViewBag.StatusList = (from p in context.sp_frm_get_Parm_Status(WebSecurity.CurrentUserId, "")
                                      where p.StatusCode == 1 || p.StatusCode == 2 
                                      select new SelectListItem
                                      {
                                          Text = p.StatusName,
                                          Value = p.StatusCode.ToString()
                                      }).ToList();

                ViewBag.BrandList = (from c in context.sp_frm_get_parm_Brands(WebSecurity.CurrentUserId, "", null)
                                     select new SelectListItem
                                     {
                                         Text = c.BrandName,
                                         Value = Shared.ToString(c.BrandCode)
                                     }).ToList();

                ViewBag.ColorList = (from c in context.sp_frm_get_parm_Colors(WebSecurity.CurrentUserId, "", null)
                                     select new SelectListItem
                                     {
                                         Text = c.ColorName,
                                         Value = Shared.ToString(c.ColorCode)
                                     }).ToList();

                model.ServiceDate = DateTime.Now.AddHours(15);
                model.ReceiptNo = GenerateBillNo();

                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBill(CreateBillModel model, FormCollection frm)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                int _affectedRows = 0;
                try
                {
                    // Has permission to view the page?
                    if (!Request.IsAuthenticated)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    else
                    {
                        //if (Shared.ToInt(model.StatusCode) != 2 && Shared.ToInt(model.NetAmount) == 0)
                        //    ModelState.AddModelError(string.Empty, "Bill Amount Cannot be Zero");

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                _affectedRows = context.sp_frm_add_upd_Bill(
                                    null,
                                    GenerateBillNo(),
                                    model.CustomerName.ToUpper(),
                                    model.ContactNo,
                                    Shared.ToString(model.ModelNo.ToUpper()),
                                    model.NatureFault.ToUpper(),
                                    "",
                                    Shared.ToInt(model.StatusCode),
                                    model.TechRemark,
                                    model.PasswordType,
                                    model.Password,
                                    model.ServiceDate,
                                    Shared.ToInt(model.BrandCode),
                                    Shared.ToInt(model.ColorCode),
                                    model.IMEINo,
                                    Shared.ToDecimal(model.NetAmount),                                   
                                    WebSecurity.CurrentUserId,
                                    "CreateBill"
                                );

                                //return RedirectToAction("ListBills", "Billing", new { Success = "Successfully Added" });
                                return RedirectToAction("Invoice", "Billing", new { BillId = model.ReceiptNo, Backto = "Billing" });
                            }
                            catch (Exception e)
                            {
                                ModelState.AddModelError(string.Empty, e.Message);
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
                    ViewBag.StatusList = (from p in context.sp_frm_get_Parm_Status(WebSecurity.CurrentUserId, "")
                                          select new SelectListItem
                                          {
                                              Text = p.StatusName,
                                              Value = p.StatusCode.ToString()
                                          }).ToList();

                    ViewBag.BrandList = (from c in context.sp_frm_get_parm_Brands(WebSecurity.CurrentUserId, "", null)
                                         select new SelectListItem
                                         {
                                             Text = c.BrandName,
                                             Value = Shared.ToString(c.BrandCode)
                                         }).ToList();

                    ViewBag.ColorList = (from c in context.sp_frm_get_parm_Colors(WebSecurity.CurrentUserId, "", null)
                                         select new SelectListItem
                                         {
                                             Text = c.ColorName,
                                             Value = Shared.ToString(c.ColorCode)
                                         }).ToList();

                    model.ServiceDate = DateTime.Now;
                    model.ReceiptNo = GenerateBillNo();
                }
                return View(model);
            }
        }
       
        public ActionResult EditBill(string ServiceID, string Success)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                CreateBillModel model = new CreateBillModel();

                try
                {
                    ViewBag.Message = Success;

                    model = (from c in context.sp_frm_get_Bills(Shared.ToInt(ServiceID), null, null, null, null, null, null, null, WebSecurity.CurrentUserId, "ListBills", 2)
                             select new CreateBillModel
                             {
                                 ServiceID= Shared.ToInt(ServiceID),
                                 ReceiptNo = c.ReceiptNo,
                                 CustomerName = c.CustomerName,
                                 ServiceDate = c.ServiceDate,
                                 ContactNo = c.ContactNo,
                                 BrandCode = c.BrandCode,
                                 ModelNo = c.ModelNo,
                                 ColorCode = c.ColorCode,
                                 IMEINo = c.IMEINo,
                                 NatureFault = c.NatureFault,
                                 StatusCode = c.StatusCode,
                                 PasswordType = c.PasswordType,
                                 Password = c.Password,
                                 NetAmount = Shared.ToDecimal(c.NetAmount),
                                 TechRemark = c.TechRemark
                             }).FirstOrDefault();

                }
                catch (Exception ex)
                {
                    return View(model);
                }
                finally
                {
                    ViewBag.StatusList = (from p in context.sp_frm_get_Parm_Status(WebSecurity.CurrentUserId, "")
                                          select new SelectListItem
                                          {
                                              Text = p.StatusName,
                                              Value = p.StatusCode.ToString()
                                          }).ToList();

                    ViewBag.BrandList = (from c in context.sp_frm_get_parm_Brands(WebSecurity.CurrentUserId, "", null)
                                         select new SelectListItem
                                         {
                                             Text = c.BrandName,
                                             Value = Shared.ToString(c.BrandCode)
                                         }).ToList();

                    ViewBag.ColorList = (from c in context.sp_frm_get_parm_Colors(WebSecurity.CurrentUserId, "", null)
                                         select new SelectListItem
                                         {
                                             Text = c.ColorName,
                                             Value = Shared.ToString(c.ColorCode)
                                         }).ToList();


                }
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditBill(CreateBillModel model, FormCollection frm)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                int _affectedRows = 0;
                try
                {
                    // Has permission to view the page?
                    if (!Request.IsAuthenticated)
                    {
                        return RedirectToAction("LogOn", "Account");
                    }
                    else
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                _affectedRows = context.sp_frm_add_upd_Bill(
                                     model.ServiceID,
                                     model.ReceiptNo,
                                     model.CustomerName.ToUpper(),
                                     model.ContactNo,
                                     model.ModelNo,
                                     model.NatureFault.ToUpper(),
                                     "",
                                     Shared.ToInt(model.StatusCode),
                                     model.TechRemark,
                                     model.PasswordType,
                                     model.Password,
                                     model.ServiceDate,
                                     Shared.ToInt(model.BrandCode),
                                     Shared.ToInt(model.ColorCode),
                                     model.IMEINo,
                                     Shared.ToDecimal(model.NetAmount),
                                     WebSecurity.CurrentUserId,
                                     "CreateBill"
                                 );

                                return RedirectToAction("ListBills", "Billing", new { Success = "Successfully Updated" });
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);

                                return View(model);
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
                    ViewBag.StatusList = (from p in context.sp_frm_get_Parm_Status(WebSecurity.CurrentUserId, "")
                                          select new SelectListItem
                                          {
                                              Text = p.StatusName,
                                              Value = p.StatusCode.ToString()
                                          }).ToList();


                    ViewBag.BrandList = (from c in context.sp_frm_get_parm_Brands(WebSecurity.CurrentUserId, "", null)
                                         select new SelectListItem
                                         {
                                             Text = c.BrandName,
                                             Value = Shared.ToString(c.BrandCode)
                                         }).ToList();

                    ViewBag.ColorList = (from c in context.sp_frm_get_parm_Colors(WebSecurity.CurrentUserId, "", null)
                                         select new SelectListItem
                                         {
                                             Text = c.ColorName,
                                             Value = Shared.ToString(c.ColorCode)
                                         }).ToList();

                }
            }
        }

        #endregion           

    }
}
