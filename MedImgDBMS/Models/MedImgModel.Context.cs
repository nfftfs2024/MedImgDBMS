﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class pjmedimgdbEntities : DbContext
    {
        public pjmedimgdbEntities()
            : base("name=pjmedimgdbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<account> accounts { get; set; }
        public virtual DbSet<comment> comments { get; set; }
        public virtual DbSet<image> images { get; set; }
        public virtual DbSet<imagestatu> imagestatus { get; set; }
        public virtual DbSet<patient> patients { get; set; }
        public virtual DbSet<report> reports { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<user> users { get; set; }
    }
}
