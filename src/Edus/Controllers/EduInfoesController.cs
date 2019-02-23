using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Edus.Models;
using PagedList;

namespace Edus.Controllers
{
    [Authorize]
    public class EduInfoesController : Controller
    {
        private XZJ_BSContext db = new XZJ_BSContext();
        private int pageEleNum = 10;//页面加载的条数

        // 教务信息首页
        // GET: EduInfoes
        public ActionResult Index(string searchStr, int page = 1)
        {
            //检索
            var model = from x in db.EduAndStuInfoes
                        where x.IsEdu == true
                        select x;

            //筛选
            if(!string.IsNullOrEmpty(searchStr))
            {
                model = model.Where(p => p.Title.Contains(searchStr));
            }

            //排序并转化
            var eduInfoList = model.OrderByDescending(p => p.Id).ToPagedList(page, pageEleNum);
            
            //返回
            ViewBag.hidPage = page;
            if(Request.IsAjaxRequest())
            {
                return PartialView("_PartialEduInfo", eduInfoList);
            }
            return View(eduInfoList);
        }

        // 教务信息详情
        // GET: EduInfoes/Details/5
        public ActionResult Details(int? id)
        {
            //id是否为空
            if (id == null)
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
            }
            //查询内容
            EduAndStuInfo eduAndStuInfo = db.EduAndStuInfoes.Find(id);
            if (eduAndStuInfo == null)
            {
                //找不到
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            //上一篇
            var front = db.EduAndStuInfoes.Where(p => p.Id < id && p.IsEdu == true).OrderByDescending(p => p.Id).Take(1);
            ViewBag.front = front.Count() == 0 ? null : front.FirstOrDefault();
            //下一篇
            var behind = db.EduAndStuInfoes.Where(p => p.Id > id && p.IsEdu == true).Take(1);
            ViewBag.behind = behind.Count() == 0 ? null : behind.FirstOrDefault();
            //当前页数
            int num = db.EduAndStuInfoes.Where(p => p.Id > id && p.IsEdu == true).Count();
            ViewBag.pageNum = num / pageEleNum + 1;

            //返回
            return View(eduAndStuInfo);
        }
        
        // 新建教务信息
        // POST: EduInfoes/Create
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Content")] EduAndStuInfo eduAndStuInfo)
        {
            //校验
            if (ModelState.IsValid)
            {
                //查重
                if(db.EduAndStuInfoes.Where(p => p.Title == eduAndStuInfo.Title && p.IsEdu == true).Count() > 0)
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该信息，请勿重复添加！" }.ToJson());
                }
                //配置信息
                eduAndStuInfo.IsEdu = true;
                eduAndStuInfo.CreateTime = DateTime.Now;
                eduAndStuInfo.Author = User.IsInRole("Administrator") ? "Administrator" : "管理员";

                try
                {
                    //插入数据库
                    db.EduAndStuInfoes.Add(eduAndStuInfo);
                    db.SaveChanges();
                    return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
                }
                catch (Exception ex)
                {
                    //插入失败
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
                }
            }

            //校验失败返回
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

        // 编辑教务信息
        // POST: EduInfoes/Edit/5
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Content")] EduAndStuInfo eduAndStuInfo)
        {
            //校验
            if (ModelState.IsValid)
            {
                EduAndStuInfo model = db.EduAndStuInfoes.Find(eduAndStuInfo.Id);
                //找不到
                if (model == null)
                {
                    return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/NotFound" }.ToJson());
                }
                //已有
                if(db.EduAndStuInfoes.Where(p => p.Title == eduAndStuInfo.Title && p.IsEdu == true && p.Id != eduAndStuInfo.Id).Count() > 0)
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该信息，请勿重复添加！" }.ToJson());
                }
                //修改
                model.Title = eduAndStuInfo.Title;
                model.Content = eduAndStuInfo.Content;

                try
                {
                    //持久化修改
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
                }
                catch (Exception ex)
                {
                    //保存失败
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
                }
            }

            //校验失败返回
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

        // 删除教务信息
        // POST: EduInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            //id为空
            if (id == null)
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "Home/Error" }.ToJson());
            }
            //检索
            EduAndStuInfo eduAndStuInfo = db.EduAndStuInfoes.Find(id);
            //找不到
            if(eduAndStuInfo == null)
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "?Home/NotFound" }.ToJson());
            }
            try
            {
                //删除
                db.EduAndStuInfoes.Remove(eduAndStuInfo);
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                //删除失败
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
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
}
