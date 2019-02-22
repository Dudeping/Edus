//异步加载页面片段
function LoadCoursePartialPage(_page, type_) {
    if (type_ == "add")
        document.getElementById("addCourse").reset();
    $.ajax({
        url: "/Courses/Index",
        data: { page: _page, searchStr: $("input[name='searchStr']").first().val() },
        type: "get"
    }).done(function (data) {
        $("#CourseList").replaceWith(data);
    });
}
/*删除*/
function DeleteCourse(id, page, index){
    swal({
        title: "您确定要删除该课程吗",
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
                url: "/Courses/Delete",
                data: { Id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
                type: "post",
                dataType: "json",
                success: function (data) {
                    if (data.state == "success") {
                        swal({ title: "成功", text: "删除课程成功！", type: "success" }, function () { LoadCoursePartialPage(page); });
                    }
                    else if (data.state == "warning") {
                        location.href = data.message;
                    }
                    else if (data.state == "info") {
                        location.href = data.message;
                    }
                    else {
                        swal("失败", "删除该课程失败！", "error");
                    }
                }
            });
        } else {
            swal("已取消", "您取消了删除操作！", "error");
        }
    });
}

/*编辑*/
function EditCourse(id, page, index) {
    //判断课号
    var $cno = $.trim($('#EditCouNo_' + id).val());
    if ($cno.length < 5 || $cno.length > 20) {
        swal({ title: "提示", text: "课号应该在5到20个字符之间！", type: "warning" }, function () { $("#EditCouNo_" + id).focus(); });
        return;
    }
    //判断课名
    var $cName = $.trim($('#EditCouName_' + id).val());
    if ($cName.length < 2 || $cName.length > 50) {
        swal({ title: "提示", text: "课名应该在2到50个字符之间！", type: "warning" }, function () { $("#EditCouName_" + id).focus(); });
        return;
    }
    //判断学分
    var $score = $.trim($('#EditCouScore_' + id).val());
    if ($score.length < 0 || (parseFloat($score) < 0 || parseFloat($score) > 10)) {
        swal({ title: "提示", text: "课程学分应该在0到10分之间！", type: "warning" }, function () { $("#EditCouScore_" + id).focus(); });
        return;
    }
    //判断任课教师
    var $teacher = $.trim($('#EditCouTeacher_' + id).val());
    if ($teacher.length > 35 || $teacher.length<8) {
        swal({ title: "提示", text: "任课教师应该在1-3位！", type: "warning" }, function () { $("#EditCouTeacher_" + id).focus(); });
        return;
    }
    //判断上课地点
    var $location = $.trim($('#EditCouLocation_' + id).val());
    if ($location.length < 2 || $location.length > 20) {
        swal({ title: "提示", text: "上课地点应该在2到10个字符之间！", type: "warning" }, function () { $("#EditCouLocation_" + id).focus(); });
        return;
    }
    //判断计划人数
    var $planMum = $.trim($('#EditCouPlanNum_' + id).val());
    if ($planMum.length < 0 (parseFloat($planMum) < 0 || parseFloat($planMum) > 1000)) {
        swal({ title: "提示", text: "计划人数应该在0到1000人之间！", type: "warning" }, function () { $("#EditCouPlanNum_" + id).focus(); });
        return;
    }
    //关闭模态框准备提交数据
    $("#EditCourseClose_" + id).focus().click();
    //提交
    $.ajax({
        url: "/Courses/Edit",
        data: { Id: id, CNo: $cno, CName: $cName, Score: $score, TNo: $teacher, Location: $location, PlanNum: $planMum, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "修改课程信息成功！", type: "success" }, function () { LoadCoursePartialPage(page); });
            }
            else if (data.state == "warning") {
                location.href = data.message;
            }
            else {
                swal({ title: "失败", text: data.message, type: "error" }, function () {
                    $("#EditCourse_" + id).focus().click();
                });
            }
        }
    });
}

/*添加*/
function AddCourse() {
    //判断课号
    var $cno = $.trim($('#AddCouNo').val());
    if ($cno.length < 5 || $cno.length > 20) {
        swal({ title: "提示", text: "课号应该在5到20个字符之间！", type: "warning" }, function () { $("#AddCouNo").focus(); });
        return;
    }
    //判断课名
    var $cName = $.trim($('#AddCouName').val());
    if ($cName.length < 2 || $cName.length > 50) {
        swal({ title: "提示", text: "课名应该在2到50个字符之间！", type: "warning" }, function () { $("#EditCouName_" + id).focus(); });
        return;
    }
    //判断学分
    var $score = $.trim($('#AddCouScore').val());
    if ($score.length < 0 || (parseFloat($score) < 0 || parseFloat($score) > 10)) {
        swal({ title: "提示", text: "课程学分应该在0到10分之间！", type: "warning" }, function () { $("#EditCouScore_" + id).focus(); });
        return;
    }
    //判断任课教师
    var $teacher = $.trim($('#AddCouTeacher').val());
    if ($teacher.length > 35) {
        swal({ title: "提示", text: "任课教师应该在三位以内！", type: "warning" }, function () { $("#EditCouTeacher_" + id).focus(); });
        return;
    }
    //判断上课地点
    var $location = $.trim($('#AddCouLocation').val());
    if ($location.length < 2 || $location.length > 20) {
        swal({ title: "提示", text: "上课地点应该在2到10个字符之间！", type: "warning" }, function () { $("#EditCouLocation_" + id).focus(); });
        return;
    }
    //判断计划人数
    var $planMum = $.trim($('#AddCouPlanNum').val());
    if ($planMum.length < 0 || (parseFloat($planMum) < 0 || parseFloat($planMum) > 1000)) {
        swal({ title: "提示", text: "计划人数应该在0到1000人之间！", type: "warning" }, function () { $("#EditCouPlanNum_" + id).focus(); });
        return;
    }
    //关闭模态框准备提交
    $("#AddCourseClose").focus().click();
    //提交数据
    $.ajax({
        url: "/Courses/Create",
        data: { CNo: $cno, CName: $cName, Score: $score, TNo: $teacher, Location: $location, PlanNum: $planMum, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(1).val() },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "修改课程信息成功！", type: "success" }, function () { LoadCoursePartialPage(1, "add"); });
            }
            else {
                swal({ title: "失败", text: data.message, type: "error" }, function () {
                    $("#AddCourseBtn").focus().click();
                });
            }
        }
    });
}

$(function () {
    //搜索
    $("form[data-course-ajax='true']").submit(function () {
        $form = $(this);
        $.ajax({
            url: $form.attr("action"),
            data: $form.serialize(),
            type: $form.attr("mothed")
        }).done(function (data) {
            $($form.attr("data-course-target")).replaceWith(data);
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
            $($a.parents("div.pagedList").attr("data-course-target")).replaceWith(data);
        });
        return false;
    });
});

//上传文件
function CFupFile() {
    var formData = new FormData();
    formData.append('courseUpFile', $('#courseUpFile')[0].files[0]);
    formData.append('__RequestVerificationToken', $("input[name='__RequestVerificationToken']").eq(2).val());
    $("#TAddCourseClose").focus().click();
    $.ajax({
        url: "/Courses/TCreateCourse",
        data: formData,
        type: "post",
        dataType: "json",
        cache: false,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "批量导入课程成功！", type: "success" }, function () {
                    document.getElementById("TaddCourse").reset();
                    LoadCoursePartialPage(1);
                });
            }
            else if (data.state == "info") {
                swal({ title: "部分导入成功！", text: data.message }, function () {
                    document.getElementById("TaddCourse").reset();
                    LoadCoursePartialPage(1);
                });
            }
            else if (data.state == "warning") {
                swal({ title: "导入失败", text: data.message }, function () {
                    $("#TAddCourseBtn").focus().click();
                });
            }
            else {
                swal({ title: "错误", text: data.message }, function () {
                    $("#TAddCourseBtn").focus().click();
                });
            }
        }
    });
}

//校验并上传文件
function TAddCourse() {
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
        var obj_file = document.getElementById("courseUpFile");
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
                    CFupFile();
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
                    CFupFile();
                } else {
                    swal("已取消", "您取消了上传操作！", "error");
                }
            });
            return;
        } else if (filesize > 2 * 1024 * 1024) {
            swal("提示", "上传文件不能超过2M！", "warning");
            return;
        } else {
            CFupFile();
            return;
        }
    }
    catch (e) {
        swal("提示", e, "warning");
    }
}