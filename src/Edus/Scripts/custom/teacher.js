//判断邮箱的正则表达式
var $regEmailStr = /^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/;
//加载页面片段
function LoadTeacherPatialPage(_page, type_) {
    if (type_ == "add") {
        document.getElementById("addTeacher").reset();
    }
    $.ajax({
        url: "/Teachers/Index",
        data: { page: _page, searchStr: $("input[name='searchStr']").first().val() },
        type: "get"
    }).done(function (data) {
        $("#TeacherList").replaceWith(data);
    });
}
/*删除*/
function DeleteTeacher(id, page, index) {
    swal({
        title: "您确定要删除该教师吗",
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
                url: "/Teachers/Delete",
                data: { Id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
                type: "post",
                dataType: "json",
                success: function (data) {
                    if (data.state == "success") {
                        swal({ title: "成功", text: "删除该教师成功！", type: "success" }, function () { LoadTeacherPatialPage(page); });
                    }
                    else if (data.state == "warning") {
                        location.href = data.message;
                    }
                    else if (data.state == "info") {
                        location.href = data.message;
                    }
                    else {
                        swal("失败", "删除该教师失败", "error");
                    }
                }
            });
        } else {
            swal("已取消", "您取消了删除操作！", "error");
        }
    });
}
/*添加*/
function AddTeacher() {
    //判断工号
    var $tno = $.trim($('#AddTheNo').val());
    if ($tno.length < 8 || $tno.length > 10) {
        swal({ title: "提示", text: "教师工号应该在8到10个字符之间！", type: "warning" }, function () { $('#addTNo').focus(); });
        return;
    }
    //判断教师姓名
    var $name = $.trim($('#AddTheName').val());
    if ($name.length < 1 || $name.length > 4) {
        swal({ title: "提示", text: "教师姓名应该在1到4个字符之间！", type: "warning" }, function () { $('#AddTheName').focus(); });
        return;
    }
    //判断性别
    var $sex = $.trim($('#AddTheSex').val());
    if ($sex.length != 1) {
        swal({ title: "提示", text: "教师性别应该为一个字符！", type: "warning" }, function () { $('#AddTheSex').focus(); });
        return;
    }
    //判断职称
    var $title = $.trim($('#AddTheTitle').val());
    if ($title.length < 1 || $title.length > 5) {
        swal({ title: "提示", text: "教师职称应该在1到5个字符之间！", type: "warning" }, function () { $('#AddTheTitle').focus(); });
        return;
    }
    //判断电话
    var $phone = $.trim($('#AddThePhone').val());
    if ($phone.length < 7 || $phone.length > 11) {
        swal({ title: "提示", text: "教师电话应该在7到11个字符之间！", type: "warning" }, function () { $('#AddThePhone').focus(); });
        return;
    }
    //判断邮箱
    var $email = $.trim($('#AddTheEmail').val());
    if (!$regEmailStr.test($email)) {
        swal({ title: "提示", text: "教师邮箱格式不正确！", type: "warning" }, function () { $('#AddTheEmail').focus(); });
        return;
    }
    //判断来校时间
    var $comeTime = $.trim($('#AddTheComeTime').val());
    if ($comeTime == '') {
        swal({ title: "提示", text: "教师来校时间不能为空！", type: "warning" }, function () { $('#AddTheComeTime').focus(); });
        return;
    }
    //关闭模态框准备提交
    $("#AddTeacherClose").focus().click();
    /*提交*/
    $.ajax({
        url: "/Teachers/Create",
        data: { TNo: $tno, TName: $name, Sex: $sex, TTitle: $title, Phone: $phone, Email: $email, ComeTime: $comeTime, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(1).val() },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "添加教师成功！", type: "success" }, function () { LoadTeacherPatialPage(1, "add"); });
            }
            else {
                swal({ title: "失败", text: data.message, type: "error" }, function () {
                    $("#AddTeacherBtn").focus().click();
                });
            }
        }
    });
}
/*编辑*/
function EditTeacher(id, page, index) {
    //判断工号
    var $tno = $.trim($('#EditTheNo_'+id).val());
    if ($tno.length < 8 || $tno.length > 10) {
        swal({ title: "提示", text: "教师工号应该在8到10个字符之间！", type: "warning" }, function () { $('#EditTheNo_' + id).focus(); });
        return;
    }
    //判断教师姓名
    var $name = $.trim($('#EditTheName_' + id).val());
    if ($name.length < 1 || $name.length > 4) {
        swal({ title: "提示", text: "教师姓名应该在1到4个字符之间！", type: "warning" }, function () { $('#EditTheName_' + id).focus(); });
        return;
    }
    //判断性别
    var $sex = $.trim($('#EditTheSex_' + id).val());
    if ($sex.length != 1) {
        swal({ title: "提示", text: "教师性别应该为一个字符！", type: "warning" }, function () { $('#EditTheSex_' + id).focus(); });
        return;
    }
    //判断职称
    var $title = $.trim($('#EditTheTitle_' + id).val());
    if ($title.length < 1 || $title.length > 5) {
        swal({ title: "提示", text: "教师职称应该在1到5个字符之间！", type: "warning" }, function () { $('#EditTheTitle_' + id).focus(); });
        return;
    }
    //判断电话
    var $phone = $.trim($('#EditThePhone_' + id).val());
    if ($phone.length < 7 || $phone.length > 11) {
        swal({ title: "提示", text: "教师电话应该在7到11个字符之间！", type: "warning" }, function () { $('#EditThePhone_' + id).focus(); });
        return;
    }
    //判断邮箱
    var $email = $.trim($('#EditTheEmail_' + id).val());
    if (!$regEmailStr.test($email)) {
        swal({ title: "提示", text: "教师邮箱格式不正确！", type: "warning" }, function () { $('#EditTheEmail_' + id).focus(); });
        return;
    }
    //判断来校时间
    var $comeTime = $.trim($('#EditTheComeTime_' + id).val());
    if ($comeTime == '') {
        swal({ title: "提示", text: "教师来校时间不能为空！", type: "warning" }, function () { $('#EditTheComeTime_' + id).focus(); });
        return;
    }

    //关闭模态框准备提交
    $("#EditTeacherClose_" + id).focus().click();
    /*提交数据*/
    $.ajax({
        url: "/Teachers/Edit",
        data: { Id: id, TNo: $tno, TName: $name, Sex: $sex, TTitle: $title, Phone: $phone, Email: $email, ComeTime: $comeTime, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "修改教师成功！", type: "success" }, function () { LoadTeacherPatialPage(page); });
            }
            else if (data.state == "warning") {
                location.href = data.message;
            }
            else {
                swal({ title: "失败", text: data.message, type: "error" }, function () {
                    $("#EditTeacher_" + id).focus().click();
                });
            }
        }
    });
}
$(function () {
    //搜索
    $("form[data-teacher-ajax='true']").submit(function () {
        $form = $(this);
        $.ajax({
            url: $form.attr("action"),
            data: $form.serialize(),
            type: $form.attr("method")
        }).done(function (data) {
            $($form.attr("data-teacher-target")).replaceWith(data);
        });
        return false;
    });
    //分页
    $(".body-content").on("click", ".pagedList a", function () {
        $a = $(this);
        $.ajax({
            url: $a.attr("href"),
            data: { searchStr: $("input[name='searchStr']").first().val() },
            type: "get"
        }).done(function (data) {
            $($a.parents("div.pagedList").attr("data-teacher-target")).replaceWith(data);
        });
    });
    return false;
});
//上传文件
function TFupFile() {
    //构造表单
    var formData = new FormData();
    formData.append('studentUpFile', $('#teacherUpFile')[0].files[0]);
    formData.append('__RequestVerificationToken', $("input[name='__RequestVerificationToken']").eq(2).val());
    $("#TAddTeacherClose").focus().click();
    //提交
    $.ajax({
        url: "/Teachers/TCreate",
        data: formData,
        type: "post",
        dataType: "json",
        cache: false,
        processData: false,
        contentType: false,
        success: function (data) {
            //展示处理结果
            if (data.state == "success") {
                swal({ title: "成功", text: "批量导入教师成功！", type: "success" }, function () {
                    document.getElementById("TaddTeacher").reset();
                    LoadTeacherPatialPage(1);
                });
            }
            else if (data.state == "info") {
                swal({ title: "部分导入成功！", text: data.message }, function () {
                    document.getElementById("TaddTeacher").reset();
                    LoadTeacherPatialPage(1);
                });
            }
            else if (data.state == "warning") {
                swal({ title: "导入失败", text: data.message }, function () {
                    $("#TAddTeacherBtn").focus().click();
                });
            }
            else {
                //文件错误
                swal({ title: "错误", text: data.message }, function () {
                    $("#TAddTeacherBtn").focus().click();
                });
            }
        }
    });
}

//校验并上传文件
function TAddTeacher() {
    //设置限制的文件大小和相应的提示信息
    var tipMsg = "此浏览器暂不支持计算上传文件的大小,仍要上传？！";

    //校验浏览器
    var browserCfg = {};
    var ua = window.navigator.userAgent;
    if (ua.indexOf("MSIE") >= 1) {
        browserCfg.ie = true;
    } else if (ua.indexOf("Firefox") >= 1) {
        browserCfg.firefox = true;
    } else if (ua.indexOf("Chrome") >= 1) {
        browserCfg.chrome = true;
    }

    //校验文件格式
    try {
        var obj_file = document.getElementById("teacherUpFile");
        if (obj_file.value == "") {
            swal("提示", "请先选择上传文件", "warning");
            return;
        }
        var index = obj_file.value.lastIndexOf(".");
        if (index == -1) {
            swal("提示", "上传文件类型不对!", "warning");
            return;
        }
        //判断文件类型
        var type = obj_file.value.substring(index + 1);
        if (type != "txt") {
            swal("提示", "文件格式不正确!", "warning");
            return;
        }

        //校验文件大小
        var filesize = 0;
        if (browserCfg.firefox || browserCfg.chrome) {
            filesize = obj_file.files[0].size;
        } else if (browserCfg.ie) {
            var obj_img = document.getElementById('tempimg');
            obj_img.dynsrc = obj_file.value;
            filesize = obj_img.fileSize;
        } else {
            swal({
                title: "提示",
                text: tipMsg,
                showCancelButton: true,
                confirmButtonText: "确定上传",
                cancelButtonText: "取消上传",
                closeOnConfirm: false,
                closeOnCancel: false
            }, function (isConfirm) {
                if (isConfirm) {
                    //确认上传
                    TFupFile();
                } else {
                    swal("已取消", "您取消了上传操作！", "error");
                }
            });
            return;
        }

        if (filesize == -1) {
            swal({
                title: "提示",
                text: tipMsg,
                showCancelButton: true,
                confirmButtonText: "确定上传",
                cancelButtonText: "取消上传",
                closeOnConfirm: false,
                closeOnCancel: false
            }, function (isConfirm) {
                if (isConfirm) {
                    //确认上传
                    TFupFile();
                } else {
                    swal("已取消", "您取消了上传操作！", "error");
                }
            });
            return;
        } else if (filesize > 2 * 1024 * 1024) {
            swal("提示", "上传文件不能超过2M！", "warning");
            return;
        } else {
            TFupFile();
            return;
        }
    }
    catch (e) {
        swal("提示", e, "warning");
    }
}