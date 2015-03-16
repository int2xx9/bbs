<!DOCTYPE html>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="BBS.Edit" %>
<html>
<head>
<meta charset="utf-8">
<title>編集</title>
<meta http-equiv="X-UA-Compatible" content="IE=edge">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<link rel="stylesheet" href="./css/bootstrap.min.css">
<link rel="stylesheet" href="./css/bootstrap.cerulean.min.css">
<link rel="stylesheet" href="./css/bbs.common.css">
</head>
<body data-spy="scroll">
<form id="form2" runat="server" defaultfocus="adminPassword">
<bbs:HeaderNavigation runat="server" ID="headnav" SearchBoxEnabled="true"></bbs:HeaderNavigation>
<div class="container">
	<div class="row">
		<div class="col-md-12">
            <% if (IsInvalidPostId || IsInvalidThreadId) { %>
            <p class="alert alert-danger">エラー: そんなスレッド・書き込みは存在しません</p>
            <% } else if (!IsLoginedAsAdmin() && String.IsNullOrEmpty(Post.Password)) { %>
            <p class="alert alert-danger">エラー: パスワードが設定されていない書き込みのため編集できません</p>
            <% } else { %>
			<h3>編集</h3>
            <bbs:AlertList runat="server" ID="EditAlerts"></bbs:AlertList>
			<div class="form-horizontal" role="form">
                <input type="hidden" name="post_id" value="<%= PostId %>" />
                <input type="hidden" name="thread_id" value="<%= ThreadId %>" />
                <% if (PostId == 1) { %>
				<div class="form-group">
					<label for="title" class="col-sm-2 control-label">タイトル</label>
					<div class="col-sm-10"><asp:TextBox ID="title" runat="server" class="form-control" placeholder="タイトル"></asp:TextBox></div>
				</div>
                <% } %>
				<div class="form-group">
					<label for="name" class="col-sm-2 control-label">名前</label>
					<div class="col-sm-10"><asp:TextBox ID="name" runat="server" class="form-control" placeholder="名前"></asp:TextBox></div>
				</div>
				<div class="form-group">
					<label for="email" class="col-sm-2 control-label">メールアドレス</label>
					<div class="col-sm-10"><asp:TextBox ID="email" runat="server" class="form-control" placeholder="メールアドレス"></asp:TextBox></div>
				</div>
                <% if (!IsLoginedAsAdmin()) { %>
				<div class="form-group">
					<label for="password" class="col-sm-2 control-label">管理パスワード</label>
					<div class="col-sm-10"><asp:TextBox ID="password" runat="server" TextMode="Password" class="form-control" placeholder="管理パスワード"></asp:TextBox></div>
				</div>
                <% } %>
				<div class="form-group">
					<label for="text" class="col-sm-2 control-label">本文</label>
					<div class="col-sm-10"><asp:TextBox ID="text" runat="server" TextMode="MultiLine" class="form-control" placeholder="本文" rows="5"></asp:TextBox></div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10">
                        <asp:Button ID="EditButton" runat="server" Text="編集" PostBackUrl="/Edit.aspx" class="btn btn-primary" />
                        <% if (PostId == 1) { %>
                        <asp:Button ID="ThreadDeleteButton" runat="server" Text="削除" PostBackUrl="/Edit.aspx" class="btn btn-warning" OnClientClick="if (!confirm('この書き込みを削除するとスレッドの書き込み全てが削除されます。本当に削除しますか？')) return false;" />
                        <% } else { %>
                        <asp:Button ID="PostDeleteButton" runat="server" Text="削除" PostBackUrl="/Edit.aspx" class="btn btn-warning" OnClientClick="if (!confirm('本当に削除しますか？')) return false;" />
                        <% } %>
					</div>
				</div>
			</div>
            <% } %>
		</div>
	</div>
</div>
</form>
<script src="./js/jquery-1.11.1.min.js"></script>
<script src="./js/bootstrap.min.js"></script>
<script src="./js/bbs.common.js"></script>
</body>
</html>
