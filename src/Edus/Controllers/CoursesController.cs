using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Edus.Models;

namespace Edus.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private XZJ_BSContext db = new XZJ_BSContext();
        private int pageEleNum = 10;//没张页面显示的数据条数

        // 课程首页
        // GET: Courses
        public ActionResult Index(string searchStr, int page = 1)
        {
            //基本搜索
            var model = from x in db.Courses select x;

            //匹配搜索框内容
            if(!String.IsNullOrEmpty(searchStr))
            {
                model = model.Where(p => p.CNo.Contains(searchStr) || p.CName.Contains(searchStr));
            }
            //倒叙并转化为IPagedList
            var courseList = model.OrderByDescending(p => p.Id).ToPagedList(page, pageEleNum);
            ViewBag.hidPage = page;
            //判断是否为异步请求
            if(Request.IsAjaxRequest())
            {
                //异步请求只返回分布页
                return PartialView("_PartialCourse", courseList);
            }
            return View(courseList);
        }

        // 课程详情
        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            //判断id是否为空
            if (id == null)
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
            }
            //查询
            Course course = db.Courses.Find(id);
            //判断结果是否为空
            if (course == null)
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            //解析教师
            string techer = "";
            foreach (var item in course.TNo.Split(new char[] { '#'}, StringSplitOptions.RemoveEmptyEntries))
            {
                techer += db.Teachers.Where(p => p.TNo == item.Trim()).FirstOrDefault().TName + ", ";
            }
            course.TNo = techer.TrimEnd(new char[] { ',', ' '});
            course.TNo = String.Join(", ", course.TNo.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries));

            //计算选课人数
            course.SelectNum = course.SNo != null ? course.SNo.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries).Count() : 0;

            //计算所在页码
            int num = db.Courses.Where(p => p.Id > id).Count();
            ViewBag.pageNum = num / pageEleNum + 1;
            return View(course);
        }

        // 添加课程
        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CNo,CName,Score,TNo,Location,PlanNum")] Course course)
        {
            //校验
            if (ModelState.IsValid)
            {
                //已有课程
                if(db.Courses.Where(p => p.CNo == course.CNo).Count() > 0)
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该课程，请勿重复添加！" }.ToJson());
                }
                //检查教师
                string findTRel = CheckTeacher(course.TNo);
                if (findTRel != "")
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = findTRel }.ToJson());

                try
                {
                    //保存修改
                    course.TNo = string.Join("#", course.TNo.Split(new char[] { '#', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct());//去除重复教师
                    db.Courses.Add(course);
                    db.SaveChanges();
                    return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
                }
                catch (Exception ex)
                {   //错误
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
                }
            }
            //校验没通过
            string _str = "";
            foreach (var key in ModelState.Keys)
            {
                foreach (var item in ModelState[key].Errors)
                {
                    _str += item.ErrorMessage;
                }
            }
            return Content(new AjaxResult { state = ResultType.error.ToString(), message = _str }.ToJson());
        }

        //批量添加课程
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TCreateCourse()
        {
            //校验文件
            var file = Request.Files[0];
            string fileName = file.FileName;
            int fileLength = file.ContentLength;
            if (file == null)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "未收到文件！" }.ToJson());
            }
            if (Path.GetExtension(fileName) != ".txt")
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "文件类型错误！" }.ToJson());
            }
            if (fileLength > 2 * 1024 * 1024)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "文件大小超出限制！" }.ToJson());
            }

            //读取文件
            byte[] fileData = new byte[fileLength];
            Stream fileStream = file.InputStream;
            fileStream.Read(fileData, 0, fileLength);
            fileStream.Position = 0;
            StreamReader fileStreamReader = new StreamReader(fileStream, System.Text.Encoding.Default);
            string courseData = fileStreamReader.ReadToEnd().Trim();

            //关闭I/O流
            fileStream.Close();
            fileStreamReader.Close();
            fileStream = null;
            fileStreamReader = null;

            //建立错误列表
            List<CourseError> errorList = new List<CourseError>();
            //成功信息
            string successRel = "";
            //遍历添加所有课程
            foreach (var courseItem in courseData.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries))
            {
                CourseError errorItem = new CourseError();
                //判断是否有错
                bool isError = false;
                try
                {
                    //获得课程信息
                    var course = courseItem.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    //错误信息标题
                    errorItem.Name = course[0].Trim() + "[" + course[1].Trim() + "]";
                    //校验课号
                    string courseNo = course[0].Trim();
                    if(courseNo.Length > 20 || courseNo.Length < 5)
                    {
                        errorItem.NError += "课名长度应该在5-20个字符之间, ";
                        isError = true;
                    }
                    if (db.Courses.Where(p => p.CNo == courseNo).Count() > 0)
                    {
                        errorItem.NError += "已有该课程";
                        isError = true;
                    }
                    Course model = new Course();
                    model.CNo = courseNo;
                    //校验课名
                    model.CName = course[1].Trim();
                    if(model.CName.Length>50 ||model.CName.Length<2)
                    {
                        errorItem.NError += "课名应该在2-50个字符之间, ";
                        isError = true;
                    }
                    //校验课程学分
                    float score;
                    if (!float.TryParse(course[2].Trim(), out score))
                    {
                        errorItem.NError += "分数格式不正确, ";
                        isError = true;
                    }
                    model.Score = score;
                    //校验任课教师
                    foreach (var item in course[3].Split(new char[] { '#', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
                    {
                        if (db.Teachers.Where(p => p.TNo == item).Count() == 0)
                        {
                            //移除非本系统教师
                            errorItem.NoT += item + ", ";
                            course[3] = course[3].Replace(item, "").Replace("##", "#");
                        }
                    }
                    //去除重复
                    model.TNo = string.Join("#", course[3].Split(new char[] { '#', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct());
                    //校验上课地点
                    model.Location = course[4].Trim();
                    if(model.Location.Length > 20 || model.Location.Length < 2)
                    {
                        errorItem.NError += "上课地点应该在2-20个字符之间, ";
                        isError = true;
                    }
                    //校验计划人数
                    int planNum;
                    if (!Int32.TryParse(course[5].Trim(), out planNum))
                    {
                        errorItem.NError += "计划人数格式不正确, ";
                        isError = true;
                    }
                    model.PlanNum = planNum;
                    //校验选课学生
                    if (course[6] != "#")
                    {
                        foreach (var item in course[6].Split(new char[] { '#', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
                        {
                            if (db.Students.Where(p => p.SNo == item).Count() == 0)
                            {
                                //移除非本系统学生
                                errorItem.NoS += item + ", ";
                                course[6] = course[6].Replace(item, "").Replace("##", "#");
                            }
                        }
                        model.SNo = string.Join("#", course[6].Split(new char[] { '#', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct());
                    }
                    //持久化该条课程信息
                    if(!isError)
                    {
                        db.Courses.Add(model);
                        db.SaveChanges();
                        successRel += errorItem.Name + ", ";
                    }
                }
                catch (Exception ex)
                {
                    errorItem.NError += "出现异常：" + ex.Message;
                    isError = true;
                }

                //有错误就添加
                if (errorItem.NError != null || errorItem.NoS != null || errorItem.NoT != null)
                {
                    errorItem.NError = errorItem.NError == null ? errorItem.NError : errorItem.NError.Trim(new char[] { ',', ' ' });
                    errorItem.NoS = errorItem.NoS == null ? errorItem.NoS : errorItem.NoS.Trim(new char[] { ',', ' ' });
                    errorItem.NoT = errorItem.NoT == null ? errorItem.NoT : errorItem.NoT.Trim(new char[] { ',', ' ' });
                    errorList.Add(errorItem);
                }
            }

            //按照错误列表返回
            if (errorList.Count == 0)
            {
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            else if (errorList.Count != 0 && successRel != "")
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "添加成功有：" + successRel.TrimEnd(new char[] { ',', ' ' }) + "添加失败及其详细信息：" + GetCourseError(errorList) }.ToJson());
            }
            else
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = GetCourseError(errorList) }.ToJson());
            }
        }

        //编辑课程
        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CNo,CName,Score,TNo,Location,PlanNum")] Course course)
        {
            //校验
            if (ModelState.IsValid)
            {
               //非法数据,同Id，不同名
                if (db.Courses.Find(course.Id) == null || db.Courses.Find(course.Id).CNo != course.CNo)
                {
                    return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
                }
                //判断教师
                string findTRel = CheckTeacher(course.TNo);
                if (findTRel != "")
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = findTRel.TrimEnd(new char[] { ',', ' ' }) }.ToJson());

                try
                {
                    //保存修改
                    var model = db.Courses.Find(course.Id);
                    model.CName = course.CName;
                    model.Score = course.Score;
                    model.TNo = string.Join("#", course.TNo.Split(new char[] { '#', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct());
                    model.Location = course.Location;
                    model.PlanNum = course.PlanNum;

                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
                }
                catch (Exception ex)
                {
                    return Content(new AjaxResult { state = ResultType.success.ToString(), message = ex.Message }.ToJson());
                }
            }
            string _str = "";
            foreach (var key in ModelState.Keys)
            {
                foreach (var item in ModelState[key].Errors)
                {
                    _str += item.ErrorMessage;
                }
            }
            return Content(new AjaxResult { state = ResultType.error.ToString(), message = _str }.ToJson());
        }

        // 删除课程
        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            //判断id是否为空
            if (id == null)
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
            //查询元素
            Course course = db.Courses.Find(id);
            if(course == null)
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            try
            {
                //保存修改
                db.Courses.Remove(course);
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                //保存错误
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        // 管理选课学生首页
        // POST: Courses/ManageStudent/5
        public ActionResult ManageStudent(int? Id, string searchStr, int page = 1)
        {
            //判断id
            if(Id == null)
            {
                return RedirectToAction("Error", "Home");
            }
            var model = db.Courses.Find(Id);
            if (model == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            //回传courseId
            ViewBag.CourseId = Id;
            //判断有无选课学生
            if(model.SNo != null && model.SNo != "")
            {
                //解析保存在string里的选课信息
                List<Student> studentList = new List<Student>();
                foreach (var item in model.SNo.Split(new char[] { '#'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    //隔开分隔符
                    studentList.Add(db.Students.Where(p => p.SNo == item.Trim()).FirstOrDefault());
                }
                if (!string.IsNullOrEmpty(searchStr))
                {
                    //搜索条件
                    studentList.RemoveAll(p => !p.SNo.Contains(searchStr) && !p.SName.Contains(searchStr));
                }
                //排序及转化
                var student = studentList.OrderBy(p => p.SNo).ToPagedList(page, 10);
                //异步返回
                if (Request.IsAjaxRequest())
                {
                    //返回
                    return PartialView("_PartialManageStudent", student);
                }
                return View(student);
            }

            //如果该课没有学生，则返回空值。
            List<Student> _str = new List<Student>();
            var nullRel = _str.ToPagedList(page, 10);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PartialManageStudent", nullRel);
            }
            return View(nullRel);
        }

        // 添加选课学生
        // POST: Courses/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStudent(string stuNo, int? couId)
        {
            //判断id和学号是否都在
            if (stuNo == null || couId == null)
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());

            var stuModel = db.Students.Where(p => p.SNo == stuNo).FirstOrDefault();
            var couModel = db.Courses.Find(couId);
            //判断数据是否为空
            if(stuModel == null || couModel == null)
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            if(couModel.SNo != null && couModel.SNo != "")
            {
                //如果已添加，则阻止操作
                if (couModel.SNo.Contains(stuNo))
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已添加该同学，请勿重复添加！" }.ToJson());
                }
            }
            try
            {
                //保存修改
                couModel.SNo = couModel.SNo + "#" + stuNo;
                db.Entry(couModel).State = EntityState.Modified;
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        // 删除选课学生
        // POST: Courses/DeleteStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteStudent(string stuNo, int? couId)
        {
            //看学号或课程id是否为空
            if (stuNo == null || couId == null)
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());

            //查询
            var stuModel = db.Students.Where(p => p.SNo == stuNo).FirstOrDefault();
            var couModel = db.Courses.Find(couId);
            //没找到其中之一
            if (stuModel == null || couModel == null)
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            try
            {
                //删除记录并保存。
                couModel.SNo = couModel.SNo.Replace(stuNo, "").Replace("##", "#");
                db.Entry(couModel).State = EntityState.Modified;
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        // 查询学生
        // POST: /Courses/FindStudent
        [HttpPost]
        public ActionResult FindStudent(string stuNo)
        {
            //id是否为空
            if(stuNo == null)
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
            }
            //查询学生
            var model = db.Students.Where(p => p.SNo == stuNo).FirstOrDefault();
            if(model == null)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "没有找到该学生！" }.ToJson());
            }
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = "学号：" + model.SNo + ", 姓名：" + model.SName + ", 性别：" + model.Sex + ", 学院：" + model.College + ", 班级：" + model.SClass }.ToJson());
        }
        //释放上下文
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // 检查教师
        private string CheckTeacher(string tNo)
        {
            string _str = "";
            foreach (var item in tNo.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (db.Teachers.Where(p => p.TNo == item.Trim()).Count() == 0)
                {
                    _str += "没找到工号为 " + item.Trim() + " 的教师, ";
                }
            }
            return _str == "" ? _str : _str.TrimEnd(new char[] { ',', ' ' });
        }

        private string GetCourseError(List<CourseError> model)
        {
            string rel = "";
            foreach (var item in model)
            {
                rel += item.Name + "(" + (item.NError != null ? (item.NError + ", ") : ("")) + (item.NoS != null ? ("未找到学生: " + item.NoS + ", ") : ("")) + (item.NoT != null ? ("未找到教师: " + item.NoT + ", ") : ("")) + "), ";
            }
            return rel.TrimEnd(new char[] { ',', ' ' });
        }
    }

    //批量添加课程错误信息
    class CourseError
    {
        public string Name { get; set; }

        public string NError { get; set; }

        public string NoS { get; set; }

        public string NoT { get; set; }
    }
}
