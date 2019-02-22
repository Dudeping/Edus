//邮箱验证的全局变量
var $regEmailStr = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
//登录页面提交数据函数
function Login() {
    //判断邮箱
    var $Email = $.trim($('#Email').val());
    if ($regEmailStr.test($Email) == false) {
        swal({ title: "提示", text: "邮箱格式不正确！", type: "warning" }, function () { $('#Email').focus(); });
        return;
    }
    //判断密码
    var $passWord = $.trim($('#passWord').val());
    if ($passWord.length < 6) {
        swal({ title: "提示", text: "密码不能小于6个字符！", type: "warning" }, function () { $('#passWord').focus(); });
        return;
    }
    //获取防跨站攻击请求的值
    var $verifi = $("input[name='__RequestVerificationToken']").first().val();
    //获取是否记住我
    var $checkBox = $("input[type='checkbox']:checked").first().val();
    //取得跳转进来的页面
    var $resultUrl = $("input[name='resultUrl']").first().val();
    //设置记住我的值
    if ($checkBox != "true") {
        $checkBox = "false";
    }
    //修改登录按钮
    $("#login").attr('disabled', 'disabled').html("验证中，请稍后...");
    //提交表单
    $.ajax({
        url: "/Account/Login",
        data: { Email: $Email, Password: $passWord, __RequestVerificationToken: $verifi, RememberMe: $checkBox, returnUrl: $resultUrl },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                $("#login").html("登录成功，正在跳转...");
                setTimeout(function () {
                    location.href = data.message;
                }, 1000);
            }
            else if (data.state == "warning") {
                location.href = data.message;
            }
            else {
                swal("错误", data.message, "error");
                $("#login").removeAttr('disabled').html("登录");
            }
        }
    });
}

//更改密码页面提交数据函数
function ChangePassword() {
    //判断邮箱
    var $oldPassword = $.trim($('#OldPassword').val());
    if ($oldPassword.length < 6) {
        swal({ title: "提示", text: "旧密码不能小于6个字符！", type: "warning" }, function () { $('#OldPassword').focus(); });
        return;
    }
    //判断密码
    var $passWord = $.trim($('#passWord').val());
    if ($passWord.length < 6) {
        swal({ title: "提示", text: "新密码不能小于6个字符！", type: "warning" }, function () { $('#passWord').focus(); });
        return;
    }
    //判断重复密码
    var $rePassWord = $.trim($('#rePassword').val());
    if ($rePassWord != $passWord) {
        swal({ title: "提示", text: "两次新密码输入不一致！", type: "warning" }, function () { $('#rePassWord').focus(); });
        return;
    }
    //获取防跨站攻击请求的值
    var $verifi = $("input[name='__RequestVerificationToken']").first().val();
    //修改登录按钮
    $("#changePassword").attr('disabled', 'disabled').html("验证中，请稍后...");
    //提交表单
    $.ajax({
        url: "/Account/ChangePassword",
        data: { OldPassword: $oldPassword, NewPassword: $passWord, ConfirmPassword: $passWord, __RequestVerificationToken: $verifi },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                $("#changePassword").html("更改成功，正在跳转...");
                setTimeout(function () {
                    location.href = "/Home/Index";
                }, 1000);
            }
            else {
                swal("错误", data.message, "error");
                $("#changePassword").removeAttr('disabled').html("更改密码");
            }
        }
    });
}

//忘记密码页面提交数据函数
function ForgotPassword() {
    //判断邮箱
    var $Email = $.trim($('#Email').val());
    if ($regEmailStr.test($Email) == false) {
        swal({ title: "提示", text: "邮箱格式不正确！", type: "warning" }, function () { $('#Email').focus(); });
        return;
    }
    //获取防跨站攻击请求的值
    var $verifi = $("input[name='__RequestVerificationToken']").first().val();
    //设置按钮
    $("#forgotPassword").attr('disabled', 'disabled').html("验证中，请稍后...");
    //发送请求
    $.ajax({
        url: "/Account/ForgotPassword",
        data: { Email: $Email, __RequestVerificationToken: $verifi },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                $("#forgotPassword").html("验证成功，正在跳转...");
                setTimeout(function () {
                    location.href = data.message;
                }, 1000);
            }
            else if (data.state = "warning") {
                //没有该账户
                location.href = data.message;
            }
            else {
                //程序出错
                swal("错误", data.message, "error");
                $("#forgotPassword").removeAttr('disabled').html("找回密码");
            }
        }
    });
}

//重置密码页面提交数据函数
function ResetPassword() {
    //判断邮箱
    var $Email = $.trim($('#Email').val());
    var $reg = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
    if ($reg.test($Email) == false) {
        swal({ title: "提示", text: "邮箱格式不正确！", type: "warning" }, function () { $('#Email').focus(); });
        return;
    }
    //判断密码
    var $passWord = $.trim($('#passWord').val());
    if ($passWord.length < 6) {
        swal({ title: "提示", text: "密码不能小于6个字符！", type: "warning" }, function () { $('#passWord').focus(); });
        return;
    }
    //判断重复密码
    var $rePassWord = $.trim($('#rePassword').val());
    if ($rePassWord != $passWord) {
        swal({ title: "提示", text: "两次密码输入不一致！", type: "warning" }, function () { $('#rePassWord').focus(); });
        return;
    }
    //获取防跨站攻击请求的值
    var $verifi = $("input[name='__RequestVerificationToken']").first().val();
    //获得Code
    var $code = $("#Code").val();
    //修改登录按钮
    $("#resetPassword").attr('disabled', 'disabled').html("验证中，请稍后...");
    //提交表单
    $.ajax({
        url: "/Account/ResetPassword",
        data: { Email: $Email, Password: $passWord, ConfirmPassword: $passWord, __RequestVerificationToken: $verifi, Code: $code },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                $("#resetPassword").html("设置成功，正在跳转...");
                setTimeout(function () {
                    location.href = data.message;
                }, 1000);
            }
            else if (data.state == "warning") {
                location.href = data.message;//账户不存在
            }
            else {
                swal("错误", data.message, "error");
                $("#resetPassword").removeAttr('disabled').html("重置密码");
            }
        }
    });
}

//初始化账号页面提交数据函数
function DefaultAdministrator() {
    //判断邮箱
    var $Email = $.trim($('#Email').val());
    var $reg = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
    if ($reg.test($Email) == false) {
        swal({ title: "提示", text: "邮箱格式不正确！", type: "warning" }, function () { $('#Email').focus(); });
        return;
    }
    //判断密码
    var $passWord = $.trim($('#passWord').val());
    if ($passWord.length < 6) {
        swal({ title: "提示", text: "密码不能小于6个字符！", type: "warning" }, function () { $('#passWord').focus(); });
        return;
    }
    //判断重复密码
    var $rePassWord = $.trim($('#rePassword').val());
    if ($rePassWord != $passWord) {
        swal({ title: "提示", text: "两次密码输入不一致！", type: "warning" }, function () { $('#rePassWord').focus(); });
        return;
    }
    //获取防跨站攻击请求的值
    var $verifi = $("input[name='__RequestVerificationToken']").first().val();
    //修改登录按钮
    $("#defaultAdministrator").attr('disabled', 'disabled').html("正在初始化，请稍后...");
    //提交表单
    $.ajax({
        url: "/Account/DefaultAdministrator",
        data: { Email: $Email, Password: $passWord, ConfirmPassword: $passWord, __RequestVerificationToken: $verifi },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                $("#resetPassword").html("初始化成功，正在跳转...");
                setTimeout(function () {
                    location.href = "/";
                }, 1000);
            }
            else if (data.state == "info") {
                swal({ title: "提示", text: "已初始化！" }, function () {
                    location.href = "/";//已初始化
                });
            }
            else {
                swal("错误", data.message, "error");
                $("#defaultAdministrator").removeAttr('disabled').html("初始化系统");
            }
        }
    });
}