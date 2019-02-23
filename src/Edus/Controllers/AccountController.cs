using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Edus.Models;
using System.Collections.Generic;

namespace Edus.Controllers
{
    [Authorize]
    //登录类
    public class AccountController : Controller
    {
        //这里用于控制账号相关的页面的JS版本
        //因为有些相应的处理逻辑布不能让攻击者知道
        #region JS版本控制
#if DEBUG
        private int layoutIndex = 0;
#else
        private int layoutIndex = 1;
#endif
        #endregion
        //应用程序登录管理器的私有字段
        private ApplicationSignInManager _signInManager;
        //应用程序用户管理器的私有字段
        private ApplicationUserManager _userManager;
        //应用程序角色管理器的私有字段
        private ApplicationRoleManager _roleManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AccountController()
        {
        }

        /// <summary>
        /// 构造函数
        /// 主要是提供一个可以初始化私有字段的构造函数
        /// </summary>
        /// <param name="userManager">应用程序用户管理器</param>
        /// <param name="signInManager">应用程序登录管理器</param>
        /// <param name="roleManager">应用程序角色管理器</param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            //初始化私有字段
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        //应用程序登录管理器属性
        public ApplicationSignInManager SignInManager
        {
            //如果已经有登录管理器的实例了, 就直接返回
            //如果没有, 就从OWIN环境字典中获取SignInManager对象
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        //应用程序用户管理器属性(同应用程序登录管理器)
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //应用程序角色管理器属性(同应用程序登录管理器)
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        // 登录
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LayoutIndex = layoutIndex;
            return View();
        }

        // 登录
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] //记得提交的时候添加字段__RequestVerificationToken
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            //验证失败则返回
            if (!ModelState.IsValid)
            {
                string _rel = "";
                foreach (var key in ModelState.Keys.ToList())
                {
                    var errors = ModelState[key].Errors.ToList();

                    foreach (var error in errors) { _rel += error.ErrorMessage; }
                }
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = _rel }.ToJson());
            }

            // 开启连续多次密码输错锁定账户 shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                //成功
                case SignInStatus.Success:
                    return Content(new AjaxResult { state = ResultType.success.ToString(), message = RedirectToLocal(returnUrl) }.ToJson());
                //锁定
                case SignInStatus.LockedOut:
                    return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Account/Lockout" }.ToJson());
                //失败
                case SignInStatus.Failure:
                default:
                    //这里不把用户名或密码错误还有账户不存在分别返回，使网站更加安全
                    //对于连续输错密码5次的用户，会锁住账户的
                    //对于账户不存在，是不会锁住账户的
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "无效的登录尝试, 连续五次无效的登录将导致您的账号被锁30分钟，请谨慎操作！" }.ToJson());
            }
        }

        // 管理员信息首页
        // GET: /Account/Index
        [Authorize(Roles ="Administrator")]
        public ActionResult Index()
        {
            List<AdminModel> model = new List<AdminModel>();
            foreach (var item in UserManager.Users.OrderByDescending(p => p.Id).ToList())
            {
                AdminModel admin = new AdminModel();
                //筛选掉超级管理员
                if (item.Roles.FirstOrDefault().RoleId == RoleManager.FindByName("Administrator").Id)
                    continue;
                admin.Id = item.Id;
                admin.Email = item.Email;
                foreach (var role in item.Roles)
                {
                    admin.AdminType += RoleManager.FindById(role.RoleId).Name + ", ";
                }
                admin.AdminType = admin.AdminType.TrimEnd(new char[] { ',', ' ' });
                model.Add(admin);
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PartialAdmin", model);
            }
            return View(model);
        }

        // 添加管理员
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken] //记得提交的时候添加字段__RequestVerificationToken
        public async Task<ActionResult> Register(AdminModel model)
        {
            if (ModelState.IsValid)
            {
                //添加管理员
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //加入角色
                    foreach (var item in model.AdminType.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        IdentityResult _result = await UserManager.AddToRoleAsync(user.Id, item);
                        if (!_result.Succeeded)
                        {
                            UserManager.Delete(user);
                            return Content(new AjaxResult { state = ResultType.error.ToString(), message = "加入角色'" + item + "'失败！详细信息：" + AddErrors(_result) }.ToJson());
                        }
                    }

                    return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
                }
                //添加不成功显示相应提示
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = AddErrors(result) }.ToJson()) ;
            }

            // 没有验证通过, 返回相应的错误信息
            string _rel = "";
            foreach (var item in ModelState.Keys.ToList())
            {
                var error = ModelState[item].Errors.ToList();
                foreach (var _item in error)
                {
                    _rel += _item.ToString() +", ";
                }
            }
            return Content(new AjaxResult { state = ResultType.warning.ToString(), message = _rel.TrimEnd(new char[] { ','}) + "! " }.ToJson());
        }

        // 删除管理员
        // POST: /Account/Delete/5
        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message="/Home/Error" }.ToJson());
            }
            if(UserManager.FindById(id) == null)
            {
                return Content(new AjaxResult { state = ResultType.info.ToString(), message = "/Home/NotFound" }.ToJson());
            }
            var reslut = UserManager.Delete(UserManager.FindById(id));
            if (reslut.Succeeded)
            {
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            return Content(new AjaxResult { state = ResultType.error.ToString(), message = AddErrors(reslut) }.ToJson());
        }

        // 忘记密码
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ViewBag.LayoutIndex = layoutIndex;
            return View();
        }

        // 忘记密码
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] //记得提交的时候添加字段__RequestVerificationToken
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if(user == null)
                {
                    // 这里不显示该用户不存在或者未经确认
                    // 使网站更加安全
                    return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Account/ForgotPasswordConfirmation" }.ToJson());
                }

                // 配置确认邮件信息
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var result = await UserManager.ConfirmEmailAsync(user.Id, code);

                if (result.Succeeded)
                {
                    //发送含修改密码链接的邮件，
                    //因为就这一处需要发送邮件所以就直接把邮件内容写在代码里，没有新建文件
                    code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    UserManager.SendEmail(user.Id, "重置密码", "<h2>请通过单击 <a href=\"" + callbackUrl + "\">此处</a>来重置你的密码。</h2><br/>如果不能跳转，请将以下链接复制到浏览器地址栏进行访问！<br/>" + callbackUrl + "<br/><br/>请勿回复此邮件，谢谢！<br/>四川农业大学教务管理系统");
                    return Content(new AjaxResult { state = ResultType.success.ToString(), message = "/Account/ForgotPasswordConfirmation" }.ToJson());
                }
                else
                {
                    //配置确认邮件信息失败
                    //返回邮件发送失败
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "邮件发送失败, 请重试！" }.ToJson());
                }
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            // 错误有可能是电子邮件放松出现错误
            string _rel = "";
            foreach (var key in ModelState.Keys.ToList())
            {
                var errors = ModelState[key].Errors.ToList();

                foreach (var error in errors) { _rel += error.ErrorMessage; }
            }
            return Content(new AjaxResult { state = ResultType.error.ToString(), message = _rel }.ToJson()); 
        }

        // 提示去邮箱
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // 重置密码
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            //判断是否能找到用户
            ViewBag.Code = code;
            ViewBag.LayoutIndex = layoutIndex;
            return UserManager.FindById(userId) == null ? View("Error") : View();
        }

        //重置密码
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] //记得提交的时候添加字段__RequestVerificationToken
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                string _rel = "";
                foreach (var item in ModelState.Keys.ToList())
                {
                    var error = ModelState[item].Errors.ToList();
                    foreach (var _item in error)
                    {
                        _rel += _item.ToString() + ", ";
                    }
                }
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = _rel.TrimEnd(new char[] { ',' }) + "! " }.ToJson());
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // 不显示该用户不存在
                // 让网站更加安全
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = "/Account/ResetPasswordConfirmation" }.ToJson());
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return Content(new AjaxResult { state = ResultType.success.ToString(), message = "/Account/ResetPasswordConfirmation" }.ToJson());
            }

            return Content(new AjaxResult { state = ResultType.error.ToString(), message = AddErrors(result) }.ToJson());
        }

        // 确认已修改，提示登录
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // 更改密码
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            ViewBag.LayoutIndex = layoutIndex;
            return View();
        }

        // 更改密码
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                string _rel = "";
                foreach (var item in ModelState.Keys.ToList())
                {
                    var error = ModelState[item].Errors.ToList();
                    foreach (var _item in error)
                    {
                        _rel += _item.ToString() + ", ";
                    }
                }
                return Content(new AjaxResult { state = ResultType.warning.ToString(), message = _rel.TrimEnd(new char[] { ',' }) + "! " }.ToJson());
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            return Content(new AjaxResult { state = ResultType.error.ToString(), message = AddErrors(result) }.ToJson());
        }

        // 注销
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken] //记得提交的时候添加字段__RequestVerificationToken
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DefaultAdministrator()
        {
            //判断是否已初始化
            if (RoleManager.FindByName("Administrator") != null && RoleManager.FindByName("Administrator").Users.Count > 0)
            {
                return Content("<script>alert('已初始化！'); location.href='/';</script>");
            }
            ViewBag.LayoutIndex = layoutIndex;
            return View();
        }

        // 初始化超级管理员
        // GET /Account/DufultAdministrator
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DefaultAdministrator(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                string email = model.Email;
                string password = model.Password;
                string role = "Administrator";
                //判断是否已经初始化
                if (UserManager.FindByEmail(email) != null && UserManager.FindByEmail(email).Roles.FirstOrDefault() != null && RoleManager.FindByName(role) != null)
                    if (RoleManager.FindByName(role).Id == UserManager.FindByEmail(email).Roles.FirstOrDefault().RoleId)
                        return Content(new AjaxResult { state = ResultType.info.ToString(), message = "已初始化！" }.ToJson());
                //添加管理员
                if (UserManager.FindByEmail(email) == null)
                {
                    IdentityResult result = UserManager.Create(new ApplicationUser { Email = email, UserName = email }, password);
                    if (!result.Succeeded)
                    {
                        return Content(new AjaxResult { state = ResultType.error.ToString(), message = "添加管理员失败！详细信息：" + AddErrors(result) }.ToJson());
                    }
                }
                //添加角色
                if (RoleManager.FindByName(role) == null)
                {
                    IdentityResult result = RoleManager.Create(new ApplicationRole { Name = role });
                    if (!result.Succeeded)
                    {
                        return Content(new AjaxResult { state = ResultType.error.ToString(), message = "添加角色'Administrator'失败！详细信息：" + AddErrors(result) }.ToJson());
                    }
                }

                IdentityResult _result;
                #region 添加角色
                if (RoleManager.FindByName("招生信息管理") == null)
                {
                    _result = RoleManager.Create(new ApplicationRole("招生信息管理"));
                    if (!_result.Succeeded)
                        return Content(new AjaxResult { state = ResultType.error.ToString(), message = "添加角色'招生信息管理'失败！，相信信息：" + AddErrors(_result) }.ToJson());
                }
                if (RoleManager.FindByName("教务信息管理") == null)
                {
                    _result = RoleManager.Create(new ApplicationRole("教务信息管理"));
                    if (!_result.Succeeded)
                        return Content(new AjaxResult { state = ResultType.error.ToString(), message = "添加角色'教务信息管理'失败！，相信信息：" + AddErrors(_result) }.ToJson());
                }
                if (RoleManager.FindByName("学生信息管理") == null)
                {
                    _result = RoleManager.Create(new ApplicationRole("学生信息管理"));
                    if (!_result.Succeeded)
                        return Content(new AjaxResult { state = ResultType.error.ToString(), message = "添加角色'学生信息管理'失败！，相信信息：" + AddErrors(_result) }.ToJson());
                }
                if (RoleManager.FindByName("教师信息管理") == null)
                {
                    _result = RoleManager.Create(new ApplicationRole("教师信息管理"));
                    if (!_result.Succeeded)
                        return Content(new AjaxResult { state = ResultType.error.ToString(), message = "添加角色'教师信息管理'失败！，相信信息：" + AddErrors(_result) }.ToJson());
                }
                if (RoleManager.FindByName("课程信息管理") == null)
                {
                    _result = RoleManager.Create(new ApplicationRole("课程信息管理"));
                    if (!_result.Succeeded)
                        return Content(new AjaxResult { state = ResultType.error.ToString(), message = "添加角色'课程信息管理'失败！，相信信息：" + AddErrors(_result) }.ToJson());
                }

                #endregion
                //加入角色
                IdentityResult result_ = UserManager.AddToRole(UserManager.FindByEmail(email).Id, role);
                if (!result_.Succeeded)
                {
                    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "加入角色'Administrator'失败！详细信息：" + AddErrors(result_) }.ToJson());
                }

                await SignInManager.SignInAsync(UserManager.FindByEmail(model.Email), isPersistent: false, rememberBrowser: false);
                return Content(new AjaxResult { state = ResultType.success.ToString() }.ToJson());
            }
            string _rel = "";
            foreach (var item in ModelState.Keys.ToList())
            {
                var error = ModelState[item].Errors.ToList();
                foreach (var _item in error)
                {
                    _rel += _item.ToString() + ", ";
                }
            }
            return Content(new AjaxResult { state = ResultType.warning.ToString(), message = _rel.TrimEnd(new char[] { ',' }) + "! " }.ToJson());
        }

        // 锁定
        // GET: /Account/Lockout
        [AllowAnonymous]
        public ActionResult Lockout()
        {
            return View("Lockout");
        }

        // 错误
        // GET: /Account/Error
        [AllowAnonymous]
        public ActionResult Error()
        {
            return View("Error");
        }

#region 帮助程序
        //释放链接
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        //解析IdentityResult的错误信息并通过字符串返回到页面
        private string AddErrors(IdentityResult result)
        {
            string _rel = "";
            foreach (var error in result.Errors)
            {
                _rel += error;
            }
            return _rel;
        }

        //这里为了防止跨站攻击，所以确认了一下是否为本网站的URL
        private string RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }
            
            return Url.Action("Index", "Home");
        }
#endregion
    }
}