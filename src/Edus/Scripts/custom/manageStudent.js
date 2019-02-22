//加载页面片段
function LoadManageStudentPartialPage(_page, couid, type_)
{
    if(type_ == "add")
    {
        document.getElementById("addStuToCourse").reset();
    }
    $.ajax({
        url: "/Courses/ManageStudent",
        data: { Id: couid, page: _page, searchStr: $("input[name='searchStr']").first().val() },
        type: "get"
    }).done(function (data) {
        $("#ManageStudentList").replaceWith(data);
    });
}
/*删除*/
function DeleteStuForCou(_couId, _stuNo, _page, index) {
    swal({
        title: "您确定要该同学从该课程中删除吗",
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
                url: "/Courses/DeleteStudent",
                data: { stuNo: _stuNo, couId: _couId, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
                type: "post",
                dataType: "json",
                success: function (data) {
                    if (data.state == "success") {
                        swal({ title: "成功", text: "从课程中删除学生成功！", type: "success" }, function () {
                            LoadManageStudentPartialPage(_page, _couId);
                        });
                    }
                    else if (data.state == "error") {
                        swal({ title: "添加失败", text: data.message, type: "error" });
                    }
                    else {
                        location.href = data.message;
                    }
                }
            });
        } else {
            swal("已取消", "您取消了删除操作！", "error");
        }
    });
}
/*添加*/
function AddStuToCourse(couId) {
    //判断学号
    var $sno = $.trim($("#AddStuToCouNo").val());
    if ($sno.length < 8 || $sno.length > 11) {
        swal({ title: "提示", text: "学号不合法！", type: "warning" }, function () { $("#AddStuToCouNo").focus(); });
        return;
    }
    //关闭模态框
    $("#AddStuToCourseClose").focus().click();
    //查找学号对应的学生数据
    $.ajax({
        url: "/Courses/FindStudent",
        data: { stuNo: $sno },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({
                    title: "确认是否为该学生",
                    text: data.message,
                    showCancelButton: true,
                    confirmButtonText: "确认添加",
                    cancelButtonText: "取消添加",
                    closeOnConfirm: false,
                    closeOnCancel: false
                }, function (isConfirm) {
                    if (isConfirm) {
                        //确认添加
                        $.ajax({
                            url: "/Courses/CreateStudent",
                            data: { stuNo: $sno, couId: couId, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(1).val() },
                            type: "post",
                            dataType: "json",
                            success: function (data) {
                                if (data.state == "success") {
                                    swal({ title: "成功", text: "添加学生到课程成功！", type: "success" }, function () {
                                        LoadManageStudentPartialPage(1, couId, "add");
                                    });
                                }
                                else if (data.state == "error") {
                                    swal({ title: "添加失败", text: data.message, type: "error" }, function () {
                                        $("#AddStuToCourseBtn").focus().click();
                                    });
                                }
                                else {
                                    location.href = data.message;
                                }
                            }
                        });
                    } else {
                        swal("已取消", "您取消了添加操作！", "error");
                    }
                });
            }
            else if (data.state == "warning") {
                location.href = data.message;//没找到
            }
            else {
                swal({ title: "错误", text: data.message, type: "error" }, function () {
                    $("#AddStuToCourseBtn").focus().click();
                });
            }
        }
    });
}

$(function () {
    //搜索
    $("form[data-manageStudent-ajax='true']").submit(function () {
        $form = $(this);
        $.ajax({
            url: $form.attr("action"),
            type: $form.attr("mothed"),
            data: $form.serialize()
        }).done(function (data) {
            $($form.attr("data-manageStudent-target")).replaceWith(data);
        });
        return false;
    });
    //分页
    $(".body-content").on("click", ".pagedList a", function () {
        $a = $(this);
        $.ajax({
            url: $a.attr("href"),
            data: { searchStr: $("input[name='searchStr']").first().val(), Id:$("input[name='couId']").first().val() },
            type: "get"
        }).done(function (data) {
            $($a.parents("div.pagedList").attr("data-ManageStudent-target")).replaceWith(data);
        });
    });
});