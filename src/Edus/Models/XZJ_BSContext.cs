using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace XZJ_BS.Models
{
    public class XZJ_BSContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public XZJ_BSContext() : base("name=XZJ_BSContext")
        {
        }

        public System.Data.Entity.DbSet<XZJ_BS.Models.Student> Students { get; set; }

        public System.Data.Entity.DbSet<XZJ_BS.Models.Teacher> Teachers { get; set; }

        public System.Data.Entity.DbSet<XZJ_BS.Models.Course> Courses { get; set; }

        public System.Data.Entity.DbSet<XZJ_BS.Models.EduAndStuInfo> EduAndStuInfoes { get; set; }
    }
}
