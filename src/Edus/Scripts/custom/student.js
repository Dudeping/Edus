//加载页面片段
function LoadStudentPartialPage(_page, type_) {
    if (type_ == "add")
        document.getElementById("addStudent").reset();
    var options = {
        url: "/Students/Index",
        type: "get",
        data: { page: _page, searchStr: $("input[name='searchStr']").first().val() }
    };
    $.ajax(options).done(function (data) {
        $("#StudentList").replaceWith(data);
    });
}
/*删除学生*/
function DeleteStudent(id, page, index){
    swal({
        title: "您确定要删除该学生吗",
        text: "删除后将无法恢复，请谨慎操作！",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "是的，我要删除！",
        cancelButtonText: "让我再考虑一下…",
        closeOnConfirm: false,
        closeOnCancel: false
    }, function (isConfirm) {
        if (isConfirm) {
            $.ajax({
                url: "/Students/Delete",
                data: { Id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
                type: "post",
                dataType: "json",
                success: function (data) {
                    if (data.state == "success") {
                        swal({ title: "成功", text: "删除该学生成功！", type: "success" }, function () { LoadStudentPartialPage(page); });
                    }
                    else if (data.state == "warning") {
                        location.href = data.message;
                    }
                    else if (data.state == "info") {
                        location.href = data.message;
                    }
                    else {
                        swal("错误", data.message, "error");
                    }
                },
                error: function (xhr) {
                    swal("错误", "详细信息：" + xhr.responseText + "一般可能是服务器连接不上或服务器端未响应，请重试！", "error");
                }
            });
        } else {
            swal("已取消", "您取消了删除操作！", "error");
        }
    });
}
/*添加学生*/
function AddStudent() {
    //判断学号
    var $num = $.trim($("#AddStuNo").val());
    if($num.length>10||$num.length<8){
        swal({ title: "提示", text: "学号应该在8到10个字符之间", type: "warning" }, function () { $("#AddStuNo").focus(); });
        return;
    }
    //判断班级
    var $name = $.trim($("#AddStuName").val());
    if ($name.length > 4 || $name.length < 1) {
        swal({ title: "提示", text: "姓名应该在1到4个字符之间", type: "warning" }, function () { $("#AddStuName").focus(); });
        return;
    }
    //判断性别
    var $sex = $.trim($("#AddStuSex").val());
    if ($sex.length != 1) {
        swal({ title: "提示", text: "姓别应该在1个字符", type: "warning" }, function () { $("#AddStuSex").focus(); });
        return;
    }
    //判断班级
    var $class_ = $.trim($("#AddStuClass").val());
    if ($class_.length > 20 || $class_.length < 2) {
        swal({ title: "提示", text: "班级应该在2到20个字符之间", type: "warning" }, function () { $("#AddStuClass").focus(); });
        return;
    }
    //判断学院
    var $college = $.trim($("#AddStuCollege").val());
    if ($college.length > 20 || $college.length < 2) {
        swal({ title: "提示", text: "学院应该在2到20个字符之间", type: "warning" }, function () { $("#AddStuCollege").focus(); });
        return;
    }
    //获得防伪标记
    var $ver = $("input[name='__RequestVerificationToken']").eq(1).val();
    //关闭表单准备提交
    $("#AddStudentClose").focus().click();
    /*AJAX提交*/
    $.ajax({
        url: "/Students/Create",
        data: { SNo: $num, SName: $name, Sex: $sex, SClass: $class_, College: $college, __RequestVerificationToken: $ver },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "添加该学生成功！", type: "success" }, function () {
                    LoadStudentPartialPage(1, "add");
                });
            }
            else {
                swal({ title: "错误", text: data.message, type: "error" }, function () {
                    $("#AddStudentBtn").focus().click();
                });
            }
        },
        error: function (xhr) {
            swal({ title: "添加该学生失败", text: "详细信息：" + xhr.responseText + "(一般可能是服务器连接不上或服务器端未响应，请重试！)", type: "error" }, function () {
                $("#AddStudentBtn").focus().click();
            });
        }
    });
}
/*编辑学生*/
function EidtStudent(id, page, index) {
    //判断学号
    var $num = $.trim($("#EditStuNo_" + id).val());
    if ($num.length > 10 || $num.length < 8) {
        swal({ title: "提示", text: "学号应该在8到10个字符之间", type: "warning" }, function () { $("#EditStuNo_" + id).focus(); });
        return;
    }
    //判断班级
    var $name = $.trim($("#EditStuName_" + id).val());
    if ($name.length > 4 || $name.length < 1) {
        swal({ title: "提示", text: "姓名应该在1到4个字符之间", type: "warning" }, function () { $("#EditStuName_" + id).focus(); });
        return;
    }
    //判断性别
    var $sex = $.trim($("#EditStuSex_" + id).val());
    if ($sex.length != 1) {
        swal({ title: "提示", text: "姓别应该在1个字符", type: "warning" }, function () { $("#EditStuSex_" + id).focus(); });
        return;
    }
    //判断班级
    var $class_ = $.trim($("#EditStuClass_" + id).val());
    if ($class_.length > 20 || $class_.length < 2) {
        swal({ title: "提示", text: "班级应该在2到20个字符之间", type: "warning" }, function () { $("#EditStuClass_" + id).focus(); });
        return;
    }
    //判断学院
    var $college = $.trim($("#EditStuCollege_" + id).val());
    if ($college.length > 20 || $college.length < 2) {
        swal({ title: "提示", text: "学院应该在2到20个字符之间", type: "warning" }, function () { $("#EditStuCollege_" + id).focus(); });
        return;
    }
    //获得防伪标记
    var $ver = $("input[name='__RequestVerificationToken']").eq(index).val();
    //关闭表单准备提交
    $("#EditStuClose_" + id).focus().click();
    /*AJAX提交*/
    $.ajax({
        url: "/Students/Edit",
        data: { Id:id, SNo: $num, SName: $name, Sex: $sex, SClass: $class_, College: $college, __RequestVerificationToken: $ver },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "修改该学生成功！", type: "success" }, function () {
                    LoadStudentPartialPage(page);
                });
            }
            else if (data.state == "warning") {
                location.href = data.message;
            }
            else {
                swal({ title: "错误", text: data.message, type: "error" }, function () {
                    $("#EditStu_" + id).focus().click();
                });
            }
        },
        error: function (xhr) {
            swal({ title: "添加该学生失败", text: "详细信息：" + xhr.responseText + "(一般可能是服务器连接不上或服务器端未响应，请重试！)", type: "error" }, function () {
                $("#AddStudentBtn").focus().click();
            });
        }
    });
}

$(function () {
    //异步搜索
    var ajaxFormSubmit = function (data) {
        var $form = $(this);
        var options = {
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: $form.serialize() //将该表单里的全部信息序列化
        };
        $.ajax(options).done(function (data) {
            var $target = $($form.attr("data-Student-target"));
            $target.replaceWith(data);
        });
        return false;
    };

    //异步分页
    var getPage = function () {
        var $a = $(this);
        var options = {
            url: $a.attr("href"),
            data: { searchStr: $("input[name='searchStr']").first().val() },
            type: "get"
        };
        $.ajax(options).done(function (data) {
            var target = $a.parents("div.pagedList").attr("data-Student-target");
            $(target).replaceWith(data);
        });
        return false;
    };
    //jQuery实现异步搜索
    $("form[data-Student-ajax='true']").submit(ajaxFormSubmit);
    //异步分页
    $(".body-content").on("click", ".pagedList a", getPage);
});

//上传文件
function SFupFile() {
    var formData = new FormData();
    formData.append('studentUpFile', $('#studentUpFile')[0].files[0]);
    formData.append('__RequestVerificationToken', $("input[name='__RequestVerificationToken']").eq(2).val());
    $("#TAddStudentClose").focus().click();
    $.ajax({
        url: "/Students/TCreate",
        data: formData,
        type: "post",
        dataType: "json",
        cache: false,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "批量导入学生成功！", type: "success" }, function () {
                    document.getElementById("TaddStudent").reset();
                    LoadStudentPartialPage(1);
                });
            }
            else if (data.state == "info") {
                swal({ title: "部分导入成功！", text: data.message }, function () {
                    document.getElementById("TaddStudent").reset();
                    LoadStudentPartialPage(1);
                });
            }
            else if (data.state == "warning") {
                swal({ title: "导入失败", text: data.message }, function () {
                    $("#TAddStudentBtn").focus().click();
                });
            }
            else {
                swal({ title: "错误", text: data.message }, function () {
                    $("#TAddStudentBtn").focus().click();
                });
            }
        }
    });
}

//校验并上传文件
function TAddStudent() {
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
        var obj_file = document.getElementById("studentUpFile");
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
                    SFupFile();
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
                    SFupFile();
                } else {
                    swal("已取消", "您取消了上传操作！", "error");
                }
            });
            return;
        } else if (filesize > 2 * 1024 * 1024) {
            swal("提示", "上传文件不能超过2M！", "warning");
            return;
        } else {
            SFupFile();
            return;
        }
}