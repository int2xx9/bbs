<!DOCTYPE html>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminLogin.aspx.cs" Inherits="BBS.AdminLogin" %>
<html>
<head>
<meta charset="utf-8">
<title>管理者ログイン</title>
<meta http-equiv="X-UA-Compatible" content="IE=edge">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<link rel="stylesheet" href="./css/bootstrap.min.css">
<link rel="stylesheet" href="./css/bootstrap.cerulean.min.css">
<link rel="stylesheet" href="./css/bbs.common.css">
</head>
<body data-spy="scroll">
<form id="form2" runat="server" defaultfocus="adminPassword">
<bbs:HeaderNavigation runat="server" ID="headnav" SearchBoxEnabled="false"></bbs:HeaderNavigation>
<div class="container">
	<div class="row">
		<div class="col-md-6 col-md-offset-3">
			<h3>管理者ログイン</h3>
            <bbs:AlertList runat="server" ID="LoginAlerts"></bbs:AlertList>
			<div class="form-horizontal" role="form">
				<div class="form-group">
					<label for="password" class="col-sm-3 control-label">パスワード</label>
					<div class="col-sm-9">
                        <asp:TextBox ID="adminPassword" runat="server" TextMode="Password" class="form-control" placeholder="パスワード" submitButton="loginButton"></asp:TextBox>
					</div>
				</div>
				<div class="form-group">
					<div class="col-sm-offset-3 col-sm-9">
                        <asp:Button ID="loginButton" runat="server" Text="ログイン" class="btn btn-primary" />
					</div>
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
