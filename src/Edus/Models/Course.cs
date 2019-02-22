using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace XZJ_BS.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("课号")]
        [StringLength(20, ErrorMessage = "{0} 必须在 {2} 到{1} 个字符之间！", MinimumLength = 5)]
        public string CNo { get; set; }

        [DisplayName("课名")]
        [StringLength(50, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 2)]
        public string CName { get; set; }

        [DisplayName("学分")]
        [Range(0, 10, ErrorMessage = "学分应该在0-10分之间！")]
        public float Score { get; set; }

        [DisplayName("任课教师")]
        [StringLength(35, ErrorMessage = "{0} 必须在 1-3 位！", MinimumLength = 8)]
        public string TNo { get; set; }

        [DisplayName("上课地点")]
        [StringLength(20, ErrorMessage = "{0} 必须在 {2} 到 {1} 个字符之间！", MinimumLength = 2)]
        public string Location { get; set; }

        [DisplayName("计划人数")]
        [Range(0, 1000, ErrorMessage = "计划人数应该在 0-1000 个之间！")]
        public int PlanNum { get; set; }

        [DisplayName("已选人数")]
        [NotMapped] //通过计算, 不存数据库
        public int SelectNum { get; set; }

        [DisplayName("选课学生")]
        [MaxLength(11000, ErrorMessage = "选课学生不能超过 1000 人！")]
        public string SNo { get; set; }
    }
}