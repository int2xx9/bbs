<!DOCTYPE html>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageThread.aspx.cs" Inherits="BBS.ManageThread" %>
<html>
<head>
<meta charset="utf-8">
<title>管理画面</title>
<meta http-equiv="X-UA-Compatible" content="IE=edge">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<link rel="stylesheet" href="./css/bootstrap.min.css">
<link rel="stylesheet" href="./css/bootstrap.cerulean.min.css">
<link rel="stylesheet" href="./css/bbs.common.css">
</head>
<body data-spy="scroll">
<form id="form2" runat="server" defaultfocus="newTitleTextBox">
<bbs:HeaderNavigation runat="server" LoginedAsAdmin="<%# IsLoginedAsAdmin() %>" ID="headnav"></bbs:HeaderNavigation>
<div class="container">
<% if (!ManageThreadAlerts.HasNoAlerts()) { %>
    <div class="row">
        <div class="col-md-12">
            <bbs:AlertList ID="ManageThreadAlerts" runat="server" />
        </div>
    </div>
   </div>
<% } else if (!IsLoginedAsAdmin()) { %>
    <div class="row">
        <div class="col-md-12">
            <bbs:Alert Type="Danger" runat="server">エラー: 管理者としてログインしてください</bbs:Alert>
        </div>
    </div>
<% } else if (ThreadInfo == null) { %>
    <div class="row">
        <div class="col-md-12">
            <bbs:Alert Type="Danger" runat="server">エラー: そんなスレッドは存在しません</bbs:Alert>
        </div>
    </div>
<% } else { %>
	<div class="row">
		<div class="col-md-6 col-md-offset-3">
            <h3>スレッド管理</h3>
            <p>対象スレッド: <a href="/Thread.aspx?thread_id=<%= ThreadInfo.Id %>"><%: ThreadInfo.Title %></a></p>
			<div class="form-horizontal" role="form">
			    <div class="form-group">
				    <label for="newTitleTextBox" class="col-sm-2 control-label">タイトル</label>
				    <div class="col-sm-10"><asp:TextBox ID="newTitleTextBox" runat="server" class="form-control" placeholder="タイトル" submitButton="newTitleButton"></asp:TextBox></div>
			    </div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
                        <asp:Button ID="newTitleButton" runat="server" Text="タイトル変更" class="btn btn-primary"
                            onclick="newTitleButton_Click" />
                        <asp:Button ID="deleteButton" runat="server" Text="削除" class="btn btn-danger"
                            OnClientClick='if(!confirm("スレッドは削除されます。本当によろしいですか？"))return false;'
                            onclick="deleteButton_Click" />
					</div>
				</div>
            </div>
        </div>
    </div>
<% } %>
</div>
</form>
<script src="./js/jquery-1.11.1.min.js"></script>
<script src="./js/bootstrap.min.js"></script>
<script src="./js/bbs.common.js"></script>
</body>
</html>

