<!DOCTYPE html>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="BBS.Search" %>
<html>
<head>
<meta charset="utf-8">
<title>検索結果</title>
<meta http-equiv="X-UA-Compatible" content="IE=edge">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<link rel="stylesheet" href="./css/bootstrap.min.css">
<link rel="stylesheet" href="./css/bootstrap.cerulean.min.css">
<link rel="stylesheet" href="./css/bbs.common.css">
</head>
<body data-spy="scroll">
<form id="Form1" runat="server">
<div class="navbar navbar-default navbar-fixed-top" role="navigation">
	<div class="navbar-fluid">
		<div class="navbar-header">
			<button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar-collapse-top">
				<span class="sr-only">Toggle navigation</span>
				<span class="icon-bar"></span>
				<span class="icon-bar"></span>
				<span class="icon-bar"></span>
			</button>
			<a class="navbar-brand" href="/">掲示板</a>
		</div>
		<div class="collapse navbar-collapse" id="navbar-collapse-top">
			<ul class="nav navbar-nav">
			</ul>
            <% if (SearchAlerts.HasNoAlerts()) { %>
			<div class="navbar-form navbar-left" role="search">
				<div class="form-group">
                    <asp:TextBox ID="searchKeyword" runat="server" class="form-control" placeholder="書き込み検索" submitButton="SearchButton"></asp:TextBox>
                    <asp:Button ID="SearchButton" runat="server" Text="検索" PostBackUrl="/Search.aspx" class="btn btn-default" />
				</div>
			</div>
            <% } %>
			<ul class="nav navbar-nav navbar-right">
				<% if (IsLoginedAsAdmin()) { %><li><a href="/AdminLogout.aspx">管理者ログアウト</a></li><% } %>
				<% else { %><li><a href="/AdminLogin.aspx">管理者ログイン</a></li><% } %>
			</ul>
		</div>
	</div>
</div>
<div class="container">
<% if (!SearchAlerts.HasNoAlerts()) {  %>
    <div class="row">
        <div class="col-md-12">
            <bbs:AlertList ID="SearchAlerts" runat="server" />
            <div role="form" class="form-horizontal">
                <input type="hidden" name="advancedSearchForm" value="1">
                <h3>検索条件</h3>
                <div class="form-group">
                    <label for="searchInKeyword" class="col-sm-4 control-label">この内容を本文に含む</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchInKeyword" class="form-control" placeholder="この内容を本文に含む" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label for="searchInName" class="col-sm-4 control-label">この内容を名前に含む</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchInName" class="form-control" placeholder="この内容を名前に含む" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label for="searchInEmail" class="col-sm-4 control-label">この内容をメールアドレスに含む</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchInEmail" class="form-control" placeholder="この内容をメールアドレスに含む" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label for="searchInTitle" class="col-sm-4 control-label">この内容をスレッドタイトルに含む</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchInTitle" class="form-control" placeholder="この内容をタイトルに含む" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>

                <h3>除外条件</h3>
                <div class="form-group">
                    <label for="searchNotInKeyword" class="col-sm-4 control-label">この内容を本文に含まない</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchNotInKeyword" class="form-control" placeholder="この内容を本文に含まない" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label for="searchNotInName" class="col-sm-4 control-label">この内容を名前に含まない</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchNotInName" class="form-control" placeholder="この内容を名前に含まない" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label for="searchNotInEmail" class="col-sm-4 control-label">この内容をメールアドレスに含まない</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchNotInEmail" class="form-control" placeholder="この内容をメールアドレスに含まない" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label for="searchNotInTitle" class="col-sm-4 control-label">この内容をスレッドタイトルに含まない</label>
                    <div class="col-sm-8">
                        <asp:TextBox ID="searchNotInTitle" class="form-control" placeholder="この内容をタイトルに含まない" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>

                <h3>日時条件</h3>
                <p>日時条件は「年-月-日 時:分:秒」の形式で指定します。例: 「2014-01-01 00:00:00」</p>
                <div class="form-group">
                    <label for="searchSince" class="col-sm-4 control-label">指定した日時の範囲に絞り込む</label>
                    <div class="input-group">
                        <asp:TextBox ID="TextBox1" class="form-control" placeholder="この日時から" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                        <div class="input-group-addon">～</div>
                        <asp:TextBox ID="TextBox2" class="form-control" placeholder="この日時まで" submitButton="AdvanceSearchButton" runat="server"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-sm-offset-4 col-sm-9">
                        <button type="submit" class="btn btn-primary" id="AdvanceSearchButton">検索</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
<% } else { %>
	<div class="row">
		<div class="col-md-12">
			<h3 id="threadList" class="anchor-target">検索結果</h3>
            <p class="alert alert-info"><%= MatchedPosts.Count() %>件見つかりました (全<%= LastPageNumber %>ページ)</p>
	        <div class="row">
		        <div class="col-md-12">
                    <% foreach (var post in PagedPosts) { %>
			        <dl class="anchor-target">
				        <dt><a href="/Thread.aspx?thread_id=<%= post.Thread.Id %>"><%: post.Thread.Title%></a><br>
                        <%= post.PostId %>:&nbsp;
                        <% if (!String.IsNullOrEmpty(post.Email)) { %>
                        <a href="mailto:<%: post.Email %>"><%: !String.IsNullOrEmpty(post.Name) ? post.Name : "名無し" %></a>
                        <% } else { %>
                        <%: !String.IsNullOrEmpty(post.Name) ? post.Name : "名無し" %>
                        <% } %>&nbsp;
                        <%= post.CreatedAt.HasValue ? post.CreatedAt.Value.ToString("yyyy/MM/dd (ddd) HH:mm:ss") : "????/??/?? (??) ??:??:??" %></dt>
				        <dd><%= post.Text.ToHtml() %></dd>
			        </dl>
                    <% } %>
		        </div>
	        </div>
            <bbs:Paging runat="server" LastPage="<%# LastPageNumber %>" CurrentPage="<%# PageNumber %>" BaseUrl='<%# Request.Url.AbsolutePath.ToString() + "?keyword=" + HttpUtility.UrlEncode(Keyword) %>'></bbs:Paging>
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
