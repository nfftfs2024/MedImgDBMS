using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MedImgDBMS.Models;
using System.ComponentModel.DataAnnotations;

namespace MedImgDBMS.ViewModels
{
    public class UserAcctViewModels
    {
        public user User { get; set; }

        public long AcctID { get; set; }
        [Required(ErrorMessage = "Please enter account name")]
        [Display(Name = "Account Name")]
        public string AcctLName { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string AcctPasswd { get; set; }
        [Required(ErrorMessage = "Please enter confirm password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("AcctPasswd", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfPasswd { get; set; }
    }

    public class UserAcctEditViewModels
    {
        public user User { get; set; }
        public account Account { get; set; }
    }
}