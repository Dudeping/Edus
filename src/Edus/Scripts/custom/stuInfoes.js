//异步加载页面片段
function LoadStuInfoPartialPage(_page, type_) {
    if (type_ == "add")//添加成功后要清零
        document.getElementById("addStuInfo").reset();
    var options = {
        url: "/StuInfoes",
        data: { page: _page, searchStr: $("input[name='searchStr']").first().val() },
        type: "get"
    };
    $.ajax(options).done(function (data) {
        var $target = $($("form[data-stuInfo-ajax='true']").attr("data-stuInfo-target"));
        $target.replaceWith(data);
    });
    return true;
}
//删除
function DeleteStuInfo(id, page, index) {
    swal({
        title: "您确定要删除这条信息吗",
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
                url: "/StuInfoes/Delete",
                data: { Id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },//last最后一个元素，first第一个元素，eq(),参数index，第几个元素
                type: "post",
                dataType: "json",
                success: function (data) {
                    if (data.state == "success") {
                        swal({ title: "成功", text: "删除招生信息成功！", type: "success" }, function () {
                            LoadStuInfoPartialPage(page);
                        });
                    }
                    else if (data.state == "warning") {
                        location.href = data.message;//Id为空
                    }
                    else if (data.state == "info") {
                        location.href = data.message;//没找到
                    }
                    else {
                        swal("删除招生信息失败", data.message, "error");
                    }
                }
            });
        } else {
            swal("已取消", "您取消了删除操作！", "error");
        }
    });
}

//编辑
function EditStuInfo(id, page, index) {
    //判断标题
    var $title = $.trim($('#EditStuInfoTitle_' + id).val());
    if ($title.length < 5 || $title.length > 20) {
        swal({ title: "提示", text: "标题必须在5到20个字符之间！", type: "warning" }, function () { $('#EditStuInfoTitle_' + id).focus(); });
        return;
    }
    //判断文本
    var $text = $.trim($('#EditStuInfoText_' + id).val());
    if ($text.length < 20) {
        swal({ title: "提示", text: "内容不能小于20个字符！", type: "warning" }, function () { $('#EditStuInfoText_' + id).focus(); });
        return;
    }
    if ($text.length > 100000) {
        swal({ title: "提示", text: "内容不能超过100000个字符！", type: "warning" }, function () { $('#EditStuInfoText_' + id).focus(); });
        return;
    }
    //关闭模态框准备提交
    $('#EditStuInfoClose_' + id).focus().click();
    //提交数据
    $.ajax({
        url: "/StuInfoes/Edit",
        data: { Id: id, Title: $title, Content: $text, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").eq(index).val() },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "编辑招生信息成功！", type: "success" }, function () {
                    LoadStuInfoPartialPage(page);
                });
            }
            else if (data.state == "warning") {
                location.href = data.message;//没找到
            }
            else {
                //编辑失败，打开模态窗口继续编辑
                swal({ title: "编辑招生信息失败", text: data.message, type: "error" }, function () { $("#EditStuInfo_" + id).focus().click(); });
            }
        }
    });
}
//添加
function AddStuInfo() {
    //判断标题
    var $title = $.trim($('#AddStuInfoTitle').val());
    if ($title.length < 5 || $title.length > 20) {
        swal({ title: "提示", text: "标题必须在5到20个字符之间！", type: "warning" }, function () { $('#AddStuInfoTitle').focus(); });
        return;
    }
    //判断文本
    var $text = $.trim($('#AddStuInfoText').val());
    if ($text.length < 20) {
        swal({ title: "提示", text: "内容不能小于20个字符！", type: "warning" }, function () { $('#AddStuInfoText').focus(); });
        return;
    }
    if ($text.length > 100000) {
        swal({ title: "提示", text: "内容不能超过100000个字符！", type: "warning" }, function () { $('#AddStuInfoText').focus(); });
        return;
    }
    //获取防跨站攻击请求的值
    var $verifi = $("input[name='__RequestVerificationToken']").eq(1).val();//这里在退出表单之后，当然是1了
    $('#AddStuInfoClose').focus().click();
    //AJAX提交
    $.ajax({
        url: "/StuInfoes/Create",
        data: { Title: $title, Content: $text, __RequestVerificationToken: $verifi },
        type: "post",
        dataType: "json",
        success: function (data) {
            if (data.state == "success") {
                swal({ title: "成功", text: "添加招生信息成功！", type: "success" }, function () {
                    LoadStuInfoPartialPage(1, "add");
                });
            } else {
                swal({ title: "添加教务信息失败", text: data.message, type: "error" }, function () { $("#AddStuInfoBtn").focus().click(); });
            }
        }
    });
}

$(function () {
    //异步搜索
    var ajaxFormSubmit = function () {
        var $form = $(this);
        var options = {
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: $form.serialize() //将该表单里的全部信息序列化
        };
        $.ajax(options).done(function (data) {
            var $target = $($form.attr("data-stuInfo-target"));
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
            var target = $a.parents("div.pagedList").attr("data-stuInfo-target");
            $(target).replaceWith(data);
        });
        return false;
    };
    //jQuery实现异步搜索
    $("form[data-stuInfo-ajax='true']").submit(ajaxFormSubmit);
    //异步分页
    $(".body-content").on("click", ".pagedList a", getPage);
});