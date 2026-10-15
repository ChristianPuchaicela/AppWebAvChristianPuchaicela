<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Logout.aspx.cs" Inherits="WebAppPoliMusicV1.Logout" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Cerrando sesión · PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
<form id="logoutForm" runat="server">
    <div class="pm-auth">
        <div class="pm-auth-card" style="text-align:center;">
            <p style="color:#C9B29A;margin:0;">Cerrando sesión…</p>
        </div>
    </div>
</form>
</body>
</html>
