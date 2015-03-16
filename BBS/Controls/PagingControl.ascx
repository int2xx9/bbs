<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PagingControl.ascx.cs" Inherits="BBS.Controls.PagingControl" %>
<nav class="text-center">
	<ul class="pagination">
        <% if (CurrentPage > 1) { %>
		<li><a href="<%= GetPageUrl(1) %>">&laquo;</a></li>
		<li><a href="<%= GetPageUrl(CurrentPage-1) %>">&lsaquo;</a></li>
        <% } else { %>
		<li class="disabled"><a href="javascript:void(0);">&laquo;</a></li>
		<li class="disabled"><a href="javascript:void(0);">&lsaquo;</a></li>
        <% } %>
        <% foreach (var page in GetPrevPageNumbers()) { %>
        <li><a href="<%= GetPageUrl(page) %>"><%= page %></a></li>
        <% } %>
        <li class="active"><a href="javascript:void(0);"><%= CurrentPage %> <span class="sr-only">(current)</span></a></li>
        <% foreach (var page in GetNextPageNumbers()) { %>
        <li><a href="<%= GetPageUrl(page) %>"><%= page %></a></li>
        <% } %>
        <% if (CurrentPage < LastPage) { %>
		<li><a href="<%= GetPageUrl(CurrentPage+1) %>">&rsaquo;</a></li>
		<li><a href="<%= GetPageUrl(LastPage) %>">&raquo;</a></li>
        <% } else { %>
		<li class="disabled"><a href="javascript:void(0);">&rsaquo;</a></li>
		<li class="disabled"><a href="javascript:void(0);">&raquo;</a></li>
        <% } %>
	</ul>
</nav>
