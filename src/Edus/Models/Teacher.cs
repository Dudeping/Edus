using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Edus.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("工号")]
        [StringLength(10, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 8)]
        public string TNo { get; set; }

        [DisplayName("姓名")]
        [StringLength(4, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 1)]
        public string TName { get; set; }

        [DisplayName("性别")]
        [StringLength(1, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 1)]
        public string Sex { get; set; }

        [DisplayName("职称")]
        [StringLength(5, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 1)]
        public string TTitle { get; set; }

        [DisplayName("电话")]
        [Phone]
        public string Phone { get; set; }

        [DisplayName("邮箱")]
        [EmailAddress]
        public string Email { get; set; }

        [DisplayName("来校时间")]
        [DataType(DataType.Time, ErrorMessage ="时间格式不正确！")]
        public DateTime ComeTime { get; set; }
    }
}