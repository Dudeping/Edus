using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using XZJ_BS.Models;
using PagedList;
using System.IO;
using System.Text.RegularExpressions;

namespace XZJ_BS.Controllers
{
    [Authorize]
    public class TeachersController : Controller
    {
        private XZJ_BSContext db = new XZJ_BSContext();

        // 教师信息首页
        // GET: Teachers
        public ActionResult Index(string searchStr, int page = 1)
        {
            //每页的数据量
            int listNum = 10;
            var model = from x in db.Teachers select x;

            //筛选搜索内容
            if(!String.IsNullOrEmpty(searchStr))
            {
                model = model.Where(p => p.TNo.Contains(searchStr) || p.TName.Contains(searchStr));
            }

            var teacherList = model.OrderByDescending(p => p.Id).ToPagedList(page, listNum);

            //当前页面和每页显示的数据量
            ViewBag.hidPage = page;
            ViewBag.ListNum = listNum;

            //检测是否为异步请求
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PartialTeacher", teacherList);
            }
            return View(teacherList);
        }

        // 新建教师
        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TNo,TName,Sex,TTitle,Phone,Email,ComeTime")] Teacher teacher)
        {
            //校验
            if (ModelState.IsValid)
            {
                //是否重复
                if (db.Teachers.Where(p => p.TNo == teacher.TNo).Count() > 0)
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该教师，请勿重复添加！" }.ToJson());
                }

                try
                {
                    //保存
                    db.Teachers.Add(teacher);
                    db.SaveChanges();
                    return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
                }
                catch (Exception ex)
                {
                    //添加失败
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
                }
            }

            //检验失败并返回相关信息
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

        // 批量导入教师
        // POST: Teachers/Create
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
            string teacherData = fileStreamReader.ReadToEnd().Trim();

            //关闭I/O流
            fileStream.Close();
            fileStreamReader.Close();
            fileStream = null;
            fileStreamReader = null;

            //错误列表
            List<TeacherError> errorList = new List<TeacherError>();
            //成功的信息
            string successRel = "";

            foreach (var teacherItem in teacherData.Split(new string[] { "-----"}, StringSplitOptions.RemoveEmptyEntries))
            {
                //当前循环错误信息
                TeacherError errorItem = new TeacherError();
                //当前循环是否有错
                bool isError = false;
                try
                {
                    //提取教师信息
                    var teacher = teacherItem.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    errorItem.Name = teacher[0].Trim() + "[" + teacher[1].Trim() + "]";
                    //校验教师工号
                    string teacherNo = teacher[0].Trim();
                    if(teacherNo.Length <8 || teacherNo.Length >10)
                    {
                        errorItem.Error += "工号必须在8-10个字符之间, ";
                        isError = true;
                    }
                    if(db.Teachers.Where(p =>p.TNo == teacherNo).Count() > 0)
                    {
                        errorItem.Error += "已有该教师, ";
                        isError = true;
                    }
                    Teacher model = new Teacher();
                    model.TNo = teacherNo;
                    //校验姓名
                    model.TName = teacher[1].Trim();
                    if(model.TName.Length > 4||model.TName.Length <2)
                    {
                        errorItem.Error += "姓名必须在1-4个字符之间, ";
                        isError = true;
                    }
                    //校验性别
                    model.Sex = teacher[2].Trim();
                    if(model.Sex != "男" && model.Sex != "女")
                    {
                        errorItem.Error += "性别必须为男或女";
                        isError = true;
                    }
                    //校验职称
                    model.TTitle = teacher[3].Trim();
                    if(model.TTitle.Length >5 || model.TTitle.Length < 1)
                    {
                        errorItem.Error += "职称在1-5个字符之间";
                        isError = true;
                    }
                    //校验电话
                    model.Phone = teacher[4].Trim();
                    if(model.Phone.Length>11 ||model.Phone.Length <0)
                    {
                        errorItem.Error += "电话号码格式错误";
                        isError = true;
                    }
                    //校验邮箱
                    model.Email = teacher[5].Trim();
                    if(!(Regex.IsMatch(model.Email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")))
                    {
                        errorItem.Error += "邮箱格式错误！";
                        isError = true;
                    }
                    //校验来校时间
                    model.ComeTime = DateTime.Parse(teacher[6].Trim());
                    if(model.ComeTime == null)
                    {
                        errorItem.Error = "来校日期格式错误！";
                        isError = true;
                    }
                    //持久化到数据库
                    if(!isError)
                    {
                        db.Teachers.Add(model);
                        db.SaveChanges();
                        successRel += errorItem.Name + ", ";
                    }
                }
                catch (Exception ex)
                {
                    //处理异常
                    errorItem.Error += "发生异常：" + ex.Message;
                    isError = true;
                }

                if (isError)
                {
                    //处理错误信息
                    errorList.Add(errorItem);
                }
            }
            
            //返回处理结果
            if(errorList.Count == 0)
            {
                //全部成功
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            else if(errorList.Count != 0 && successRel != "")
            {
                //部分成功
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "添加成功有：" + successRel.TrimEnd(new char[] {',', ' ' }) + "添加失败及其详细信息：" + GetTeacherError(errorList) }.ToJson());
            }
            else
            {
                //全部失败
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = GetTeacherError(errorList) }.ToJson());
            }
        }

        // 编辑教师
        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TNo,TName,Sex,TTitle,Phone,Email,ComeTime")] Teacher teacher)
        {
            //校验
            if (ModelState.IsValid)
            {
                //检测违规操作引发的错误
                if (db.Teachers.Find(teacher.Id) == null || db.Teachers.Find(teacher.Id).TNo != teacher.TNo)
                {
                    return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
                }
                //已有必须阻止该操作
                if(db.Teachers.Where(p => p.TNo == teacher.TNo && p.Id != teacher.Id).Count() > 0)
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该教师，请勿重复添加！" }.ToJson());
                }
                try
                {
                    //修改
                    var model = db.Teachers.Find(teacher.Id);
                    model.TName = teacher.TName;
                    model.Sex = teacher.Sex;
                    model.TTitle = teacher.TTitle;
                    model.Phone = teacher.Phone;
                    model.Email = teacher.Email;
                    model.ComeTime = teacher.ComeTime;
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

            //校验失败并返回相关信息
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

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            //id为空
            if(id == null)
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
            }
            Teacher teacher = db.Teachers.Find(id);
            //找不到
            if(teacher == null)
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            try
            {
                //删除
                db.Teachers.Remove(teacher);
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                //删除失败
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        //获取教师错误信息列表
        private string GetTeacherError(List<TeacherError> list)
        {
            string str = "";
            foreach (var item in list)
            {
                str += (item.Error != null ? (item.Name + "(" + item.Error + "), ") : (""));
            }
            return str.TrimEnd(new char[] { ',', ' ' });
        }

        //释放数据库上下文
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    //导入教师信息错误列表
    class TeacherError
    {
        public string Name { get; set; }

        public string Error { get; set; }
    }
}
