using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Edus.Models
{
    public class EduAndStuInfo
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("标题")]
        [StringLength(20, ErrorMessage = "{0} 必须在 {2} 和 {1} 个字符之间！", MinimumLength = 5)]
        public string Title { get; set; }

        [DisplayName("内容")]
        [StringLength(100000, ErrorMessage = "{0} 必须在 {2} 和 {1} 个字符之间！", MinimumLength = 20)]
        public string Content { get; set; }

        [DisplayName("作者")]
        public string Author { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }

        [DisplayName("是否是教务新闻")]
        public bool IsEdu { get; set; }
    }
}