//加载页面片段
function LoadEduInfoParialPage(_page, type_) {
    if(type_ == "add")
    {
        document.getElementById("addEduInfo").reset();
    }
    var opations = {
        url: "/EduInfoes/Index",
        type: "get",
        data: { page: _page, searchStr: $("input[name='searchStr']").first().val() }
    };
    $.ajax(opations).done(function (data) {
        $("#EduInfoList").replaceWith(data);
    });
}
/*删除*/
function DeleteEduInfo(id, page, index) {
    swal({
        title: "您确定要删除该教务信息吗",
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
                url: "/EduInfoes/Delete",
                data: { Id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
                type: "post",
                dataType: "json",
                success: function (data) {
                    if (data.state == "success") {
                        swal({ title: "删除成功", text: "您已经永久删除了这条教务信息。", type: "success" }, function () { LoadEduInfoParialPage(page);});
                    }
                    else if (data.state == "warning") {
                        location.href = data.message;
                    }
                    else if (data.state == "info") {
                        location.href = data.message;
                    }
                    else {
                        swal("删除失败", data.message, "error");
                    }
                }
            });
        } else {
            swal("已取消", "您取消了删除操作！", "error");
        }
    });
}

/*编辑*/
function EditEduInfo(id, page, index) {
    //判断标题
    var $title = $.trim($('#EditEduInfoTitle_' + id).val());
    if ($title.length < 5 || $title.length > 20) {
        swal({ title: "提示", text: "标题应该在5到20个字符之间！", type: "warning" }, function () { $("#EditEduInfoTitle_" + id).focus(); });
        return false;
    }
    //判断内容
    var $text = $.trim($('#EditEduInfoText_' + id).val());
    if ($text.length < 20) {
        swal({ title: "提示", text: "内容不能小于20个字符！", type: "warning" }, function () { $("#EditEduInfoText_" + id).focus(); });
        return false;
    }
    if ($text.length > 100000) {
        swal({ title: "提示", text: "内容不能大于100000个字符！", type: "warning" }, function () { $("#EditEduInfoText_" + id).focus(); });
        return false;
    }
    //关闭模态框，准备提交
    $("#EditEduInfoClose_" + id).focus().click();
    /*AJAX提交*/
    $.ajax({
        url: "/EduInfoes/Edit",
        data: { Id: id, Title: $title, Content: $text, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "编辑该教务信息成功！", type: "success" }, function () { LoadEduInfoParialPage(page); });
            }
            else if (data.state == "warning") {
                location.href = data.message;
            }
            else {
                swal({ title: "失败", text: data.message, type: "error" }, function () {
                    $("#EditEduInfo_" + id).focus().click();
                });
            }
        }
    });
}

/*添加*/
function AddEduInfo() {
    var $title = $.trim($('#AddEduInfoTitle').val());
    //判断标题
    if ($title.length < 5 || $title.length > 20) {
        swal({ title: "提示", text: "标题应该在5到20个字符之间！", type: "warning" }, function () { $("#AddEduInfoTitle").focus(); });
        return false;
    }
    //判断内容
    var $text = $.trim($('#AddEduInfoText').val());
    if ($text.length < 20) {
        swal({ title: "提示", text: "内容不能小于20个字符！", type: "warning" }, function () { $("#AddEduInfoText").focus(); });
        return false;
    }
    if ($text.length > 100000) {
        swal({ title: "提示", text: "内容不能大于100000个字符！", type: "warning" }, function () { $("#AddEduInfoText").focus(); });
        return false;
    }
    //关闭模态框准备提交
    $("#AddEduInfoClose").focus().click();
    //提交
    $.ajax({
        url: "/EduInfoes/Create",
        data: { Title: $title, Content: $text, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(1).val() },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "添加教务信息成功！", type: "success" }, function () { LoadEduInfoParialPage(1, "add"); });
            }
            else {
                swal({ title: "失败", text: data.message, type: "error" }, function () {
                    $("#AddEduInfoBtn").focus().click();
                });
            }
        }
    });
}

$(function () {
    //异步搜索
    $("form[data-eduInfo-ajax='true']").submit(function (data) {
        $form = $(this);
        $.ajax({
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: $form.serialize()
        }).done(function (data) {
            $($form.attr("data-eduInfo-target")).replaceWith(data);
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
            $($a.parents("div.pagedList").attr("data-eduInfo-target")).replaceWith(data);
        });
        return false;
    });
});