using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MobilePro.Models
{

    public class BillsListModel
    {
        public string ServiceID { get; set; }

        public string ReceiptNo { get; set; }

        public string CustomerName { get; set; }

        public string ContactNo { get; set; }

        public string IMEINo { get; set; }

        public string ServiceDate { get; set; }

        public string NatureFault { get; set; }

        public string TechRemark { get; set; } 

        public string PasswordType { get; set; }

        public string Password { get; set; }

        public string BrandCode { get; set; }

        public string BrandName { get; set; }

        public string ModelNo { get; set; }

        public string ColorCode { get; set; }

        public string ColorName { get; set; }

        public string StatusCode { get; set; }

        public string StatusName { get; set; }
        
        public string NetAmount { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public string UpdatedDate { get; set; }

    }

    public class CreateBillModel
    {
        public Int64 ServiceID { get; set; }

        [Required]
        public string ReceiptNo { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string ContactNo { get; set; }
        [Required]
        public string ModelNo { get; set; }
        [Required]
        public string NatureFault { get; set; }

        public string TechRemark { get; set; } 

        public int? StatusCode { get; set; }

        public string StatusName { get; set; }

        public string PasswordType { get; set; }

        public string Password { get; set; }

        [Display(Name = "Net Amount"), DataType(DataType.Currency)]
        public decimal NetAmount { get; set; }

        [Required]
        public DateTime? ServiceDate { get; set; }      

        [Required]
        public int? BrandCode { get; set; }

        public int? ColorCode { get; set; }
       
        public string IMEINo { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public string UpdatedDate { get; set; }

    }

    
}