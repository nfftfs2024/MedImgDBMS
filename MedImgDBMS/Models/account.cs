//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MedImgDBMS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class account
    {
        public long AcctID { get; set; }
        [Required(ErrorMessage = "Please enter account name")]
        [DisplayName("Account Name")]
        public string AcctLName { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string AcctPasswd { get; set; }
        [DisplayName("Account Status")]
        public long AcctStatus { get; set; }
        public System.DateTime AcctCreateTime { get; set; }

        public account()
        {
            AcctStatus = 1;
            AcctCreateTime = DateTime.UtcNow;
        }

        public virtual accountstatu accountstatu { get; set; }
        public virtual user user { get; set; }
    }
}
