<!DOCTYPE html>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Thread.aspx.cs" Inherits="BBS.Thread" %>
<html>
<head runat="server">
<meta charset="utf-8">
<title>スレッド</title>
<meta http-equiv="X-UA-Compatible" content="IE=edge">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<link rel="stylesheet" href="./css/bootstrap.min.css">
<link rel="stylesheet" href="./css/bootstrap.cerulean.min.css">
<link rel="stylesheet" href="./css/bbs.common.css">
</head>
<body data-spy="scroll">
<form id="form1" runat="server">
<bbs:HeaderNavigation runat="server" LoginedAsAdmin="<%# IsLoginedAsAdmin() %>" ID="headnav"></bbs:HeaderNavigation>
<div class="container">
<% if (!IsThreadAvailable()) { %>
    <div class="row">
        <div class="col-md-12">
            <p class="alert alert-danger">エラー: そんなスレッドは存在しません</p>
        </div>
    </div>
<% } else { %>
	<div class="row">
		<div class="col-md-12">
			<h3><%: ThreadInfo.Title %></h3>
            <% foreach (var post in MatchedPosts) { %>
			<dl id="post<%= post.PostId %>" class="anchor-target">
				<dt><%= post.PostId %>:&nbsp;
                <% if (!String.IsNullOrEmpty(post.Email)) { %>
                <a href="mailto:<%: post.Email %>"><%: !String.IsNullOrEmpty(post.Name) ? post.Name : "名無し" %></a>
                <% } else { %>
                <%: !String.IsNullOrEmpty(post.Name) ? post.Name : "名無し" %>
                <% } %>&nbsp;
                <%= post.CreatedAt.HasValue ? post.CreatedAt.Value.ToString("yyyy/MM/dd (ddd) HH:mm:ss") : "????/??/?? (??) ??:??:??" %>&nbsp;
                <a href="/Edit.aspx?thread_id=<%= post.ThreadId %>&post_id=<%= post.PostId %>"><span class="glyphicon glyphicon-wrench"></span></a></dt>
				<dd><%= Regex.Replace(post.Text.ToHtml(), @"&gt;&gt;([\d,\-]+)", "<a href=\"?thread_id=" + ThreadInfo.Id + "&range=$1\">&gt;&gt;$1</a>")%></dd>
			</dl>
            <% } %>
		</div>
	</div>
	<div class="row">
		<div class="col-md-12">
			<h3 id="createPost" class="anchor-target">書き込み</h3>
            <bbs:AlertList runat="server" ID="PostAlerts"></bbs:AlertList>
			<div class="form-horizontal" role="form">
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
					<div class="col-sm-offset-2 col-sm-10"><asp:Button ID="PostButton" runat="server" Text="書き込み" class="btn btn-primary" PostBackUrl="#createPost" /></div>
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
