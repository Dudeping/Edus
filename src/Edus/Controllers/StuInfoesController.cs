using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using XZJ_BS.Models;

namespace XZJ_BS.Controllers
{
    [Authorize]
    public class StuInfoesController : Controller
    {
        private XZJ_BSContext db = new XZJ_BSContext();
        //一张页面加载的元素条数，为了统一，所以放在方法外
        private int pageEleNum = 10;
           
        // 首页
        // GET: StuInfoes
        public ActionResult Index(string searchStr, int page = 1)
        {
            var model = from x in db.EduAndStuInfoes
                          where x.IsEdu == false
                          select x;
            //搜索内容
            if (!string.IsNullOrEmpty(searchStr))
            {
                model = model.Where(p => p.Title.Contains(searchStr));
            }
            //倒序排列并转化为IPagedList
            var stuInfoList = model.OrderByDescending(p => p.Id).ToPagedList(page, pageEleNum);

            //当前的页面数
            ViewBag.hidPage = page;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PartialStuInfoes", stuInfoList);
            }
            return View(stuInfoList);
        }

        // 详情
        // GET: StuInfoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                //错误
                return View("Error");
            }
            var model = db.EduAndStuInfoes.Find(id);
            if (model == null)
            {
                //没找到
                return View("NotFound");
            }
            //上一篇
            var front = db.EduAndStuInfoes.Where(p => p.Id < id && p.IsEdu == false).OrderByDescending(p => p.Id).Take(1);
            ViewBag.front = front.Count() == 0 ? null : front.FirstOrDefault();
            //下一篇
            var behind = db.EduAndStuInfoes.Where(p => p.Id > id && p.IsEdu == false).Take(1);
            ViewBag.behind = behind.Count() == 0 ? null : behind.FirstOrDefault();
            //当前页
            int num = db.EduAndStuInfoes.Where(p => p.Id > id && p.IsEdu == false).Count();
            ViewBag.pageNum = num / pageEleNum + 1;
            return View(model);
        }

        // 创建
        // POST: StuInfoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]//取消跨站攻击的防御，因为这里用到了编辑器，所以要取消
        public ActionResult Create([Bind(Include = "Title,Content")] EduAndStuInfo eduAndStuInfo)
        {
            //校验不成功，返回错误信息
            if (!ModelState.IsValid)
            {
                string rel = "";
                foreach (var key in ModelState.Keys.ToList())
                {
                    var errors = ModelState[key].Errors.ToList();

                    foreach (var error in errors) { rel += error.ErrorMessage; }
                }
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = rel }.ToJson());
            }

            //如若同名，则不让其添加
            if(db.EduAndStuInfoes.Where(p => p.Title == eduAndStuInfo.Title && p.IsEdu == false).Count() > 0)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该信息，请勿重复添加！" }.ToJson());
            }
            // 由于某些操作的失误，
            // 插入数据的过程中有可能出错，所以这里最好加上try
            try
            {
                //修改信息
                eduAndStuInfo.Author = User.IsInRole("Administrator") ? "Administrator" : "管理员";
                eduAndStuInfo.CreateTime = DateTime.Now;
                eduAndStuInfo.IsEdu = false;
                //创建并保存
                db.EduAndStuInfoes.Add(eduAndStuInfo);
                db.SaveChanges();

                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                //新建失败返回
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        // 编辑
        // POST: StuInfoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]//取消跨站攻击的防御，因为这里用到了编辑器，所以要取消
        public ActionResult Edit([Bind(Include = "Id,Title,Content")] EduAndStuInfo eduAndStuInfo)
        {
            // 校验不成功，返回错误信息
            if (!ModelState.IsValid)
            {
                string rel = "";
                foreach (var key in ModelState.Keys.ToList())
                {
                    var errors = ModelState[key].Errors.ToList();

                    foreach (var error in errors) { rel += error.ErrorMessage; }
                }
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = rel }.ToJson());
            }
            //如若同名，则阻止该操作
            if (db.EduAndStuInfoes.Where(p => p.Title == eduAndStuInfo.Title && p.IsEdu == false && p.Id != eduAndStuInfo.Id).Count() > 0)
            {
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "已有该信息，请勿重复添加！" }.ToJson());
            }
            var model = db.EduAndStuInfoes.Find(eduAndStuInfo.Id);
            //找不到
            if (model == null)
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            try
            {
                //保存
                model.Title = eduAndStuInfo.Title;
                model.Content = eduAndStuInfo.Content;

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

        // 删除
        // POST: StuInfoes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                //错误
                //一般产生这种情况和下一种情况的都是在违规操作，所以不用客气，好不犹豫地抛错就是了
                //但是为了调试程序的时候方便发现错误，这里把不同的错误区分开来
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Home/Error" }.ToJson());
            }
            var model = db.EduAndStuInfoes.Find(id);
            if (model == null)
            {
                //没找到
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }

            //防止删除失败程序抛异常，所以还是try一下
            try
            {
                db.EduAndStuInfoes.Remove(model);
                db.SaveChanges();
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            catch (Exception ex)
            {
                //删除失败返回
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
