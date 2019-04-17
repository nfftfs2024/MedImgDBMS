using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MedImgDBMS.Models;

namespace MedImgDBMS.ViewModels
{
    public class ImgRepCmtViewModels
    {
        public image Image { get; set; }
        public report Report { get; set; }
        public comment Comment { get; set; }
    }
}