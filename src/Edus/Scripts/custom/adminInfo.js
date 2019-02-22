//异步加载页面片段
function LoadAdminPartialPage(_type) {
    if (_type == "add")
        document.getElementById("addAdmin").reset();
    var options = {
        url: "/Account",
        data: {},
        type: "get"
    };
    $.ajax(options).done(function (data) {
        $("#AdminList").replaceWith(data);
    });
    return true;
}
/*删除*/
function DeleteAdmin(id, index) {
    swal({
        title: "您确定要删除该管理员吗",
        text: "删除后将无法恢复，请谨慎操作！",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "是的，我要删除！",
        cancelButtonText: "让我再考虑一下…",
        closeOnConfirm: false,
        closeOnCancel: false
    },
    function (isConfirm) {
        if (isConfirm) {
            $.ajax({
                url: "/Account/Delete",
                data: { Id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
                type: "post",
                dataType: "json",
                success: function (data) {
                    if (data.state == "success") {
                        swal({ title: "成功", text: "删除管理员成功！", type: "success" }, function () {
                            LoadAdminPartialPage();
                        });
                    }
                    else if (data.state == "warning") {
                        location.href = data.message;//Id为空
                    }
                    else if (data.state == "info") {
                        location.href = data.message;//没找到
                    }
                    else {
                        swal("错误", data.message, "error");
                    }
                }
            });
        } else {
            swal("已取消", "您取消了删除操作！", "error");
        }
    });
}

/*添加*/
function AddAdmin() {
    //判断邮箱
    var $AEmail = $.trim($("#AEmail").val());
    var $regEmailStr = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
    if (!$regEmailStr.test($AEmail)) {
        swal({ title: "提示", text: "邮箱格式不正确！", type: "warning" }, function () { $('#AEmail').focus(); });
        return;
    }
    //判断密码
    var $APassword = $.trim($('#APassword').val());
    if ($APassword.length < 6) {
        swal({ title: "提示", text: "密码不能小于6个字符！", type: "warning" }, function () { $('#APassword').focus(); });
        return;
    }
    //获取防跨站攻击请求的值
    var $verifi = $("input[name='__RequestVerificationToken']").last().val();
    //获得分管模块
    var $AType = $("input:checkbox[name='AType']:checked").map(function (index, elem) {
        return $(elem).val();
    }).get().join(',');

    //关闭模态框准备提交
    $('#close').focus().click();
    $.ajax({
        url: "/Account/Register",
        data: { Email: $AEmail, Password: $APassword, __RequestVerificationToken: $verifi, AdminType: $AType },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "添加管理员成功！", type: "success" }, function () {
                    LoadAdminPartialPage("add");
                });
            }
            else if (data.state == "warning") {
                swal({ title: "失败", text: data.message, type: "warning" }, function () {
                    $('#AddAdmin').focus().click();
                });
            }
            else {
                swal({ title: "错误", text: data.message, type: "error" }, function () {
                    $('#AddAdmin').focus().click();//添加失败重新打开模态框
                });
            }
        }
    });
}