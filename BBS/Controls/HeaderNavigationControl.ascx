<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HeaderNavigationControl.ascx.cs" Inherits="BBS.Controls.HeaderNavigationControl" ClientIDMode="Static" %>
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
                <% foreach (var item in NavigationLinks) { %>
                <li><a href="<%: item.Key %>"><%: item.Value %></a></li>
                <% } %>
			</ul>
			<div class="navbar-form navbar-left" role="search">
                <% if (SearchBoxEnabled) { %>
				<div class="form-group">
                    <asp:TextBox id="searchKeyword" runat="server" class="form-control" placeholder="書き込み検索" submitButton="SearchButton"></asp:TextBox>
                    <asp:Button id="SearchButton" runat="server" Text="検索" PostBackUrl="/Search.aspx" class="btn btn-default" />
				</div>
                <% } %>
			</div>
			<ul class="nav navbar-nav navbar-right">
                <% if (LoginedAsAdmin.HasValue) { %>
				    <% if (LoginedAsAdmin.Value) { %><li><a href="/AdminLogout.aspx">管理者ログアウト</a></li><% } %>
				    <% else { %><li><a href="/AdminLogin.aspx">管理者ログイン</a></li><% } %>
                <% } %>
			</ul>
		</div>
	</div>
</div>
