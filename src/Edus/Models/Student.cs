using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace XZJ_BS.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("学号")]
        [StringLength(10, ErrorMessage ="{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 8)]
        public string SNo { get; set; }

        [DisplayName("姓名")]
        [StringLength(4, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 1)]
        public string SName { get; set; }

        [DisplayName("性别")]
        [StringLength(1, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 1)]
        public string Sex { get; set; }

        [DisplayName("学院")]
        [StringLength(20, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 2)]
        public string College { get; set; }

        [DisplayName("班级")]
        [StringLength(20, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 2)]
        public string SClass { get; set; }
    }
}