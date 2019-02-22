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
using XZJ_BS.Models;

namespace XZJ_BS.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private XZJ_BSContext db = new XZJ_BSContext();

        // 学生首页
        // GET: Students
        public ActionResult Index(string searchStr, int page = 1)
        {
            var model = from x in db.Students select x;
            //搜索内容(可以搜索学号或姓名)
            if(!String.IsNullOrEmpty(searchStr))
            {
                model = model.Where(p => p.SNo.Contains(searchStr) || p.SName.Contains(searchStr));
            }

            int listNum = 10;//每页显示的条数

            //倒序排列并转化为IPagedList
            var studentList = model.OrderByDescending(p => p.Id).ToPagedList(page, listNum);

            ViewBag.ListNum = listNum;
            ViewBag.hidPage = page;
            
            //若为异步请求，则返回分布页
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PartialStudent", studentList);
            }
            return View(studentList);
        }

        // 添加学生
        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SNo,SName,Sex,SClass,College")] Student student)
        {
            //校验
            if (!ModelState.IsValid)
            {
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
            //重复
            if(db.Students.Where(p => p.SNo == student.SNo).Count() > 0)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已存在该同学，请勿重复添加！" }.ToJson());
            }
            try
            {
                //插入数据
                db.Students.Add(student);
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                //插入失败
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        // 批量导入学生
        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TCreate()
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
            string studentData = fileStreamReader.ReadToEnd().Trim();

            //关闭I/O流
            fileStream.Close();
            fileStreamReader.Close();
            fileStream = null;
            fileStreamReader = null;

            //错误列表
            List<StudentError> errorList = new List<StudentError>();
            //成功条目
            string successRel = "";
            foreach (var studentItem in studentData.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries))
            {
                //当前循环错误
                StudentError errorItem = new StudentError();
                //当前循环是否有错
                bool isError = false;
                try
                {
                    //提取学生信息
                    var student = studentItem.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    errorItem.Name = student[0].Trim() + "[" + student[1].Trim() + "]";
                    //校验学号
                    string studentNo = student[0].Trim();
                    if (db.Students.Where(p => p.SNo == studentNo).Count() > 0)
                    {
                        errorItem.Error += "已有该学生！";
                        isError = true;
                    }
                    if (studentNo.Length > 10 || studentNo.Length < 8)
                    {
                        errorItem.Error += "学号应该在8-10个字符之间, ";
                        isError = true;
                    }
                    Student model = new Student();
                    model.SNo = studentNo;
                    //校验姓名
                    model.SName = student[1].Trim();
                    if (model.SName.Length > 4 || model.SName.Length < 1)
                    {
                        errorItem.Error += "姓名应该在1-4个字符之间, ";
                        isError = true;
                    }
                    //校验性别
                    model.Sex = student[2].Trim();
                    if (model.Sex != "男" && model.Sex != "女")
                    {
                        errorItem.Error += "性别必须为男或女, ";
                        isError = true;
                    }
                    //校验学院
                    model.College = student[3].Trim();
                    if (model.College.Length < 2 || model.College.Length > 20)
                    {
                        errorItem.Error += "学院应该在2-20个字符之间, ";
                        isError = true;
                    }
                    //校验班级
                    model.SClass = student[4].Trim();
                    if (model.SClass.Length < 2 || model.SClass.Length > 20)
                    {
                        errorItem.Error += "班级应在2-20个字符之间, ";
                        isError = true;
                    }
                    //如果信息正确，持久化学生到数据库
                    if(!isError)
                    {
                        db.Students.Add(model);
                        db.SaveChanges();
                        successRel += errorItem.Name + ", ";
                    }
                }
                catch (Exception ex)
                {
                    errorItem.Error += "出现异常：" + ex.Message;
                }

                //记录错误
                if(errorItem.Error!=null)
                {
                    errorList.Add(errorItem);
                    isError = true;
                }
            }

            //返回处理结果
            if (errorList.Count == 0)
            {
                //全部成功
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            else if (errorList.Count != 0 && successRel != "")
            {
                //部分成功
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "添加成功有：" + successRel.TrimEnd(new char[] { ',', ' ' }) + "添加失败及其详细信息：" + GetStudentError(errorList) }.ToJson());
            }
            else
            {
                //全部出错
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = GetStudentError(errorList) }.ToJson());
            }
        }

        // 编辑学生
        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SNo,SName,Sex,SClass,College")] Student student)
        {
            //校验
            if (ModelState.IsValid)
            {
                //找不到就直接抛错(因为很可能是违规操作造成这些错误)
                if(db.Students.Find(student.Id) == null||db.Students.Find(student.Id).SNo != student.SNo)
                {
                    return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
                }
                //查重
                if(db.Students.Where(p => p.SNo == student.SNo && p.Id != student.Id).Count() > 0)
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该学生，请勿重复添加！" }.ToJson());
                }
                try
                {
                    //保存修改
                    var model = db.Students.Find(student.Id);
                    model.SName = student.SName;
                    model.Sex = student.Sex;
                    model.SClass = student.SClass;
                    model.College = student.College;
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
                }
                catch (Exception ex)
                {
                    //修改失败
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
                }
            }

            //校验失败并返回
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

        // 删除学生
        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                //错误
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
            }

            Student student = db.Students.Find(id);
            if (student == null)
            {
                //没找到
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            try
            {
                db.Students.Remove(student);
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                //删除失败
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "删除该学生失败！详细信息：" + ex.Message }.ToJson());
            }
        }

        private string GetStudentError(List<StudentError> list)
        {
            string str = "";
            foreach (var item in list)
            {
                str += (item.Error == null ? ("") : (item.Name + "(" + item.Error + "), "));
            }
            return str.TrimEnd(new char[] { ',', ' ' });
        }

        // 释放数据库上下文
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    //导入学生信息错误列表
    class StudentError
    {
        public string Name { get; set; }

        public string Error { get; set; }
    }
}
