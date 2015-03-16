<!DOCTYPE html>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BBS.WebForm1" %>
<html>
<head>
<meta charset="utf-8">
<title>掲示板</title>
<meta http-equiv="X-UA-Compatible" content="IE=edge">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<link rel="stylesheet" href="./css/bootstrap.min.css">
<link rel="stylesheet" href="./css/bootstrap.cerulean.min.css">
<link rel="stylesheet" href="./css/bbs.common.css">
</head>
<body data-spy="scroll">
<form id="Form1" runat="server">
<bbs:HeaderNavigation runat="server" LoginedAsAdmin="<%# IsLoginedAsAdmin() %>" ID="headnav"></bbs:HeaderNavigation>
<div class="container">
	<div class="row">
		<div class="col-md-12">
			<h3 id="threadList" class="anchor-target">スレッド一覧</h3>
			<table class="table table-striped table-hover">
				<thead>
					<tr>
						<th>タイトル</th>
						<th>作成日</th>
						<th>更新日</th>
					</tr>
				</thead>
				<tbody>
                    <% foreach (ThreadWithDetail thread in PagedThreads) { %>
                    <tr>
                        <td>
                            <a href="/Thread.aspx?thread_id=<%= thread.Id %>">
                                <%: thread.Title %></a>(<%= thread.PostCount %>)
                                <% if (IsLoginedAsAdmin()) { %> <a href="/ManageThread.aspx?thread_id=<%= thread.Id %>"><span class="glyphicon glyphicon-wrench"></span></span></a> <% } %>
                        </td>
                        <td><%= thread.CreatedAt.HasValue ? thread.CreatedAt.ToString() : "---" %></td>
                        <td><%= thread.LastPostAt.HasValue ? thread.LastPostAt.ToString() : "---"%></td>
                    </tr>
                    <% } %>
				</tbody>
			</table>
            <bbs:Paging runat="server" LastPage="<%# LastPageNumber %>" CurrentPage="<%# PageNumber %>" BaseUrl='<%# Request.Url.AbsolutePath %>'></bbs:Paging>
		</div>
	</div>
	<div class="row">
		<div class="col-md-12">
			<h3 id="createThread" class="anchor-target">スレッド作成</h3>
            <bbs:AlertList runat="server" ID="ThreadAlerts" />
			<div class="form-horizontal" role="form">
				<div class="form-group">
					<label for="title" class="col-sm-2 control-label">タイトル</label>
					<div class="col-sm-10"><asp:TextBox ID="title" runat="server" class="form-control" placeholder="タイトル"></asp:TextBox></div>
				</div>
				<div class="form-group">
					<label for="name" class="col-sm-2 control-label">名前</label>
					<div class="col-sm-10"><asp:TextBox ID="name" runat="server" class="form-control" placeholder="名前"></asp:TextBox></div>
				</div>
				<div class="form-group">
					<label for="email" class="col-sm-2 control-label">メールアドレス</label>
					<div class="col-sm-10"><asp:TextBox ID="email" runat="server" class="form-control" placeholder="メールアドレス"></asp:TextBox></div>
				</div>
				<div class="form-group">
					<label for="password" class="col-sm-2 control-label">管理パスワード</label>
					<div class="col-sm-10"><asp:TextBox ID="password" runat="server" TextMode="Password" class="form-control" placeholder="管理パスワード"></asp:TextBox></div>
				</div>
				<div class="form-group">
					<label for="text" class="col-sm-2 control-label">本文</label>
					<div class="col-sm-10"><asp:TextBox ID="text" runat="server" TextMode="MultiLine" class="form-control" placeholder="本文" rows="5"></asp:TextBox></div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-2 col-sm-10"><asp:Button ID="CreateThreadButton" runat="server" Text="作成" PostBackUrl="/Default.aspx#createThread" class="btn btn-primary" /></div>
				</div>
			</div>
		</div>
	</div>
</div>
</form>
<script src="./js/jquery-1.11.1.min.js"></script>
<script src="./js/bootstrap.min.js"></script>
<script src="./js/bbs.common.js"></script>
</body>
</html>
