using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace MobilePro.Models
{    

    public class BrandsModel
    {
        public string BrandCode { get; set; }
        
        public string BrandName { get; set; }
        
        public string BrandDesc { get; set; }
        
        public string Status { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public string UpdatedDate { get; set; }
        
    }


    public class ColorsModel
    {
        public string ColorCode { get; set; }

        public string ColorName { get; set; }

        public string Status { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public string UpdatedDate { get; set; }

    }

    
}