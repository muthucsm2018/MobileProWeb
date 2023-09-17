using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

using MobilePro.Filters;
using MobilePro.Models;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using System.Xml.Linq;
using System.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.IO;

using System.Globalization;
using System.Data.Entity;
using System.Data.SqlClient;

namespace MobilePro.Controllers
{
    public class ReportsController : Controller
    {
        
        #region Bills

        public ActionResult Bills(string Success)
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

                        ViewBag.ModelList = context.ServiceBills.Select( //x => x.Remarks)
                             x => new SelectListItem
                             {
                                 Text = x.ModelNo,
                                 Value = x.ModelNo
                             })
                             .Where(x => !string.IsNullOrEmpty(x.Value))
                             .Distinct().ToList(); 

                        ViewBag.BrandList = (from c in context.sp_frm_get_parm_Brands(WebSecurity.CurrentUserId, "", null)
                                             select new SelectListItem
                                             {
                                                 Text = c.BrandName,
                                                 Value = Shared.ToString(c.BrandCode)
                                             }).ToList();
                        

                        ViewBag.CreatedByList = context.sp_frm_get_Parm_Users(WebSecurity.CurrentUserId, "").ToList().Select(c => new SelectListItem
                        {
                            Value = c.UserId.ToString(),
                            Text = c.UserName
                        }); 

                    }
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        private DataSet ReturnQueryBill(string CompanyCode, string BrandCode, string ModelNo, string StatusCode, DateTime? TransactionFrom, DateTime? TransactionTo, string CreatedBy)
        {
            DataSet dsQuery = new DataSet();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[10];
                sqlParams[0] = new SqlParameter("@CompanyID", SqlDbType.Int);
                if (Shared.ToString(CompanyCode).Length > 0)
                    sqlParams[0].Value = Shared.ToInt(CompanyCode);
                else
                    sqlParams[0].Value = DBNull.Value;
                sqlParams[1] = new SqlParameter("@ReceiptNo", SqlDbType.NVarChar, 50);
                sqlParams[1].Value = DBNull.Value;
                sqlParams[2] = new SqlParameter("@BrandCode", SqlDbType.Int);
                if (Shared.ToString(BrandCode).Length > 0)
                    sqlParams[2].Value = Shared.ToInt(BrandCode);
                else
                    sqlParams[2].Value = DBNull.Value;
                sqlParams[3] = new SqlParameter("@ModelNo", SqlDbType.NVarChar, 50);
                if (Shared.ToString(ModelNo).Length > 0)
                    sqlParams[3].Value = ModelNo;
                else
                    sqlParams[3].Value = DBNull.Value;
                sqlParams[4] = new SqlParameter("@StatusCode", SqlDbType.Int);
                if (Shared.ToString(StatusCode).Length > 0)
                    sqlParams[4].Value = StatusCode;
                else
                    sqlParams[4].Value = DBNull.Value;

                sqlParams[5] = new SqlParameter("@TransactionFrom", SqlDbType.DateTime);
                if (TransactionFrom != null)
                    sqlParams[5].Value = TransactionFrom;
                else
                    sqlParams[5].Value = DBNull.Value;
                sqlParams[6] = new SqlParameter("@TransactionTo", SqlDbType.DateTime);
                if (TransactionFrom != null)
                    sqlParams[6].Value = TransactionTo;
                else
                    sqlParams[6].Value = DBNull.Value;
                sqlParams[7] = new SqlParameter("@UserId", SqlDbType.Int);
                sqlParams[7].Value = WebSecurity.CurrentUserId;
                sqlParams[8] = new SqlParameter("@PageName", SqlDbType.VarChar, 50);
                sqlParams[8].Value = "Bills";
                sqlParams[9] = new SqlParameter("@CreatedBy", SqlDbType.Int);
                if (Shared.ToString(CreatedBy).Length > 0)
                    sqlParams[9].Value = Shared.ToInt(CreatedBy);
                else
                    sqlParams[9].Value = DBNull.Value;
               

                dsQuery = db_data_access.FetchRS(CommandType.StoredProcedure, "sp_frm_get_Bills_Report", sqlParams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);
            }

            return dsQuery;
        }

        public List<BillsListModel> GetListBill(string CompanyCode, string BrandCode, string ModelNo, string StatusCode, DateTime? TransactionFrom, DateTime? TransactionTo, string CreatedBy)
        {
            DataSet ds = ReturnQueryBill(CompanyCode, BrandCode, ModelNo, StatusCode, TransactionFrom, TransactionTo, CreatedBy);

            if (ds.Tables[0].Rows.Count > 0)
            {
                var trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new BillsListModel
                {
                    ServiceID = Shared.ToString(dataRow.Field<Int64>("ServiceID")),
                    ReceiptNo = dataRow.Field<string>("ReceiptNo"),
                    ServiceDate = Shared.ToString(dataRow.Field<DateTime>("ServiceDate").ToString("MMM dd yyyy h:mm tt")),
                    CustomerName = dataRow.Field<string>("CustomerName"),
                    ContactNo = dataRow.Field<string>("ContactNo"),
                    BrandName = dataRow.Field<string>("BrandName"),
                    ModelNo = dataRow.Field<string>("ModelNo"),                   
                    NatureFault = dataRow.Field<string>("NatureFault"),
                    StatusName = dataRow.Field<string>("StatusName"),                   
                    NetAmount = Shared.ToString(dataRow.Field<decimal>("NetAmount")) ,
                    CreatedBy = dataRow.Field<string>("CreatedBy"),
                    CreatedDate = dataRow.Field<string>("CreatedDate"),
                    UpdatedBy = dataRow.Field<string>("UpdatedBy"),
                    UpdatedDate = dataRow.Field<string>("UpdatedDate")
                });
                return trxnList.ToList();
            }
            else

                return new List<BillsListModel>();
        }

        public JsonResult QueryBillList(string CompanyCode, string BrandCode, string ModelNo, string StatusCode, string daterange, string CreatedBy)
        {
            try
            {
                //02/02/2016 - 02/02/2016
                DateTime? _startdate = null;
                DateTime? _enddate = null;
                CultureInfo provider = CultureInfo.InvariantCulture;

                if (Shared.ToString(daterange).Length > 0)
                {
                    _startdate = DateTime.ParseExact(daterange.Split('|')[0].Trim().ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    _enddate = DateTime.ParseExact(daterange.Split('|')[1].Trim().ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    
                }

                List<BillsListModel> query = GetListBill(CompanyCode, BrandCode, ModelNo, StatusCode, _startdate, _enddate, WebSecurity.CurrentUserId == 1 ? CreatedBy : Shared.ToString(WebSecurity.CurrentUserId));

                return Json(new
                {
                    aaData = query.Select(x => new[] { x.ServiceID, x.ReceiptNo, x.ServiceDate, x.CustomerName, x.ContactNo, x.BrandName, x.ModelNo, x.NatureFault, x.StatusName, x.NetAmount, x.CreatedBy })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


        #endregion                
              
        

    }
}
