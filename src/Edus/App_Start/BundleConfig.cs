using System.Web;
using System.Web.Optimization;

namespace XZJ_BS
{
    public class BundleConfig
    {
        // 有关绑定的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //首页CSS打包
            bundles.Add(new StyleBundle("~/CSS/Home").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/custom/style.css",
                "~/Content/font-awesome.css"
                ));

            //忘记密码和更改密码和登陆页面的CSS打包
            bundles.Add(new StyleBundle("~/CSS/Account").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/plugins/sweetalert/sweetalert.css",
                "~/Content/font-awesome.css"
                ));

            //忘记密码和更改密码和登陆页面的JS打包(调试使用)
            bundles.Add(new ScriptBundle("~/Scripts/DubugAccount").Include(
                "~/Scripts/jquery-1.12.4.js",
                "~/Scripts/plugins/sweetalert/sweetalert.min.js",
                "~/Scripts/custom/account.js"
                ));

            //忘记密码和更改密码和登陆页面的JS打包(发布使用)
            bundles.Add(new ScriptBundle("~/Scripts/Account").Include(
                "~/Scripts/jquery-1.12.4.js",
                "~/Scripts/plugins/sweetalert/sweetalert.min.js",
                "~/Scripts/custom/account.min.js"
                ));

            //招生信息管理和教务信息管理页面CSS
            bundles.Add(new StyleBundle("~/CSS/EduAddStuInfoes").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/custom/style.css",
                "~/Content/plugins/sweetalert/sweetalert.css",
                "~/Content/font-awesome.css",
                "~/Content/PagedList.css",
                "~/Content/plugins/simditor/simditor.css"
                ));

            //Mobilecheck.JS打包情况
            bundles.Add(new ScriptBundle("~/Scripts/Mobilecheck").Include(
                "~/Scripts/jquery-1.12.4.min.js",
                "~/Scripts/plugins/simditor/mobilecheck.js"
                ));

            //招生信息管理和教务信息管理页面JS
            bundles.Add(new ScriptBundle("~/Scripts/EduAddStuInfoes").Include(
                "~/Scripts/jquery-ui-1.12.1.min.js",
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/plugins/sweetalert/sweetalert.min.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/custom/stuInfoes.js",
                "~/Scripts/custom/eduInfoes.js",
                "~/Scripts/plugins/simditor/mobilecheck.js",
                "~/Scripts/plugins/simditor/module.min.js",
                "~/Scripts/plugins/simditor/hotkeys.min.js",
                "~/Scripts/plugins/simditor/simditor.min.js"
                ));

            //招生信息管理详情、教务信息详情和课程信息详情页面CSS打包
            bundles.Add(new StyleBundle("~/CSS/StuInfoEduInfoCouInfoDetail").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/custom/style.css",
                "~/Content/font-awesome.css"
                ));

            //课程信息管理和管理选课学生和管理员信息页面CSS打包
            bundles.Add(new StyleBundle("~/CSS/CouAdminManageStudent").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/plugins/fileUpload/jasny-bootstrap.min.css",
                "~/Content/custom/style.css",
                "~/Content/PagedList.css",
                "~/Content/plugins/sweetalert/sweetalert.css",
                "~/Content/font-awesome.css"
                ));

            //课程信息管理和管理选课学生和管理员信息页面JS打包
            bundles.Add(new ScriptBundle("~/Scripts/CouAdminManageStudent").Include(
                "~/Scripts/jquery-1.12.4.js",
                "~/Scripts/jquery-ui-1.12.1.min.js",
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/plugins/fileUpload/jasny-bootstrap.min.js",
                "~/Scripts/plugins/sweetalert/sweetalert.min.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/custom/course.js",
                "~/Scripts/custom/manageStudent.js",
                "~/Scripts/custom/adminInfo.js"
                ));

            //学生信息管理和教师信息管理页面CSS打包
            bundles.Add(new StyleBundle("~/CSS/TeacherAddStuInfoes").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/plugins/fileUpload/jasny-bootstrap.min.css",
                "~/Content/custom/style.css",
                "~/Content/plugins/sweetalert/sweetalert.css",
                "~/Content/font-awesome.css",
                "~/Content/PagedList.css",
                "~/Content/plugins/footable/footable.core.css"
                ));

            //学生信息管理和教师信息管理页面JS打包
            bundles.Add(new ScriptBundle("~/Scripts/TeacherAddStuInfoes").Include(
                "~/Scripts/jquery-1.12.4.js",
                "~/Scripts/jquery-ui-1.12.1.min.js",
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/plugins/fileUpload/jasny-bootstrap.min.js",
                "~/Scripts/plugins/sweetalert/sweetalert.min.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/plugins/footable/footable.all.min.js",
                "~/Scripts/custom/student.js",
                "~/Scripts/custom/teacher.js"
                ));
        }
    }
}
