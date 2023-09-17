using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using MobilePro.Models;
using WebMatrix.WebData;

namespace MobilePro.Controllers
{
    public class MasterTablesController : Controller
    {
        #region CommonFunctions

        #endregion

        #region Brands     

        public ActionResult Brands()
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
                  
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        private DataSet ReturnQueryBrands(string BrandName)
        {
            DataSet dsQuery = new DataSet();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[4];
                sqlParams[0] = new SqlParameter("@UserId", SqlDbType.Int);
                sqlParams[0].Value = WebSecurity.CurrentUserId;
                sqlParams[1] = new SqlParameter("@PageName", SqlDbType.VarChar, 50);
                sqlParams[1].Value = "Brands";
                sqlParams[2] = new SqlParameter("@BrandName", SqlDbType.VarChar, 100);
                sqlParams[2].Value = BrandName;                
                sqlParams[3] = new SqlParameter("@CompanyID", SqlDbType.Int);
                sqlParams[3].Value = DBNull.Value;

                dsQuery = db_data_access.FetchRS(CommandType.StoredProcedure, "sp_frm_get_Brands", sqlParams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);
            }

            return dsQuery;
        }

        public List<BrandsModel> GetListBrands(string BrandName)
        {
            DataSet ds = ReturnQueryBrands(BrandName);

            if (ds.Tables[0].Rows.Count > 0)
            {
                var trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new BrandsModel
                     {
                         BrandCode = Shared.ToString(dataRow.Field<Int32>("BrandCode")),
                         BrandName = dataRow.Field<string>("BrandName"),
                         BrandDesc = Shared.ToString(dataRow.Field<string>("BrandDesc")),
                         Status = Shared.ToString(dataRow.Field<Int16>("Status")),
                         UpdatedBy = Shared.ToString(dataRow.Field<string>("UpdatedBy")),
                         UpdatedDate = dataRow.Field<string>("UpdatedDate"),
                     });
                return trxnList.ToList();
            }
            else

                return new List<BrandsModel>();
        }
       
        public JsonResult BrandsList(string BrandName)
        {
            try
            {
                List<BrandsModel> query = GetListBrands(BrandName);

                return Json(new
                {
                    aaData = query.Select(x => new[] { x.BrandCode, x.BrandCode, x.BrandName, x.BrandDesc, x.Status, x.UpdatedBy, x.UpdatedDate })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveBrand(string BrandCode, string BrandName, string BrandDesc, string Status, string mode)
        {
            int _affectedRows = 0;
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    _affectedRows = context.sp_frm_add_upd_Brand(
                               Shared.ToInt(BrandCode),
                                BrandName.ToUpper(),
                                BrandDesc,
                                Status == "True" ? true : false,
                                WebSecurity.CurrentUserId,
                                "Brands"
                                );

                    return Json(new { success = true, response = Shared.ToString(mode) == "Create" ? "Successfully Added!" : "Successfully Updated!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message });
                }
            }
        }

        [HttpPost]
        public ActionResult CheckExistsBrand(string BrandName)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    var _exists = context.Brands.Any(x => x.BrandName == BrandName);
                    if (_exists)
                    {
                        return Json(new { success = false, response = "Brand Name Already Exists." });
                    }
                    else
                    {
                        return Json(new { success = true });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message });
                }
            }
        }

        #endregion

        #region Colors

        public ActionResult Colors()
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

                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        private DataSet ReturnQueryColors(string ColorName)
        {
            DataSet dsQuery = new DataSet();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[4];
                sqlParams[0] = new SqlParameter("@UserId", SqlDbType.Int);
                sqlParams[0].Value = WebSecurity.CurrentUserId;
                sqlParams[1] = new SqlParameter("@PageName", SqlDbType.VarChar, 50);
                sqlParams[1].Value = "Colors";
                sqlParams[2] = new SqlParameter("@ColorName", SqlDbType.VarChar, 100);
                sqlParams[2].Value = ColorName;
                sqlParams[3] = new SqlParameter("@CompanyID", SqlDbType.Int);
                sqlParams[3].Value = DBNull.Value;

                dsQuery = db_data_access.FetchRS(CommandType.StoredProcedure, "sp_frm_get_Colors", sqlParams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message);
            }

            return dsQuery;
        }

        public List<ColorsModel> GetListColors(string ColorName)
        {
            DataSet ds = ReturnQueryColors(ColorName);

            if (ds.Tables[0].Rows.Count > 0)
            {
                var trxnList = ds.Tables[0].AsEnumerable().Select(dataRow => new ColorsModel
                {
                    ColorCode = Shared.ToString(dataRow.Field<Int32>("ColorCode")),
                    ColorName = dataRow.Field<string>("ColorName"),
                    Status = Shared.ToString(dataRow.Field<Int16>("Status")),
                    UpdatedBy = Shared.ToString(dataRow.Field<string>("UpdatedBy")),
                    UpdatedDate = dataRow.Field<string>("UpdatedDate"),
                });
                return trxnList.ToList();
            }
            else

                return new List<ColorsModel>();
        }

        public JsonResult ColorsList(string ColorName)
        {
            try
            {
                List<ColorsModel> query = GetListColors(ColorName);

                return Json(new
                {
                    aaData = query.Select(x => new[] { x.ColorCode, x.ColorCode, x.ColorName, x.Status, x.UpdatedBy, x.UpdatedDate })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveColor(string ColorCode, string ColorName, string Status, string mode)
        {
            int _affectedRows = 0;
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    _affectedRows = context.sp_frm_add_upd_Color(
                               Shared.ToInt(ColorCode),
                                ColorName.ToUpper(),
                                Status == "True" ? true : false,
                                WebSecurity.CurrentUserId,
                                "Colors"
                                );

                    return Json(new { success = true, response = Shared.ToString(mode) == "Create" ? "Successfully Added!" : "Successfully Updated!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, response = Shared.ToString(ex.InnerException).Length > 0 ? ex.InnerException.Message : ex.Message });
                }
            }
        }       

        [HttpPost]
        public ActionResult CheckExistsColor(string ColorName)
        {
            using (MobileProEntities context = new MobileProEntities())
            {
                try
                {
                    var _exists = context.Colors.Any(x => x.ColorName == ColorName);
                    if (_exists)
                    {
                        return Json(new { success = false, response = "Color Name Already Exists." });
                    }
                    else
                    {
                        return Json(new { success = true });
                    }
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
