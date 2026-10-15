<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Error.aspx.cs" Inherits="WebAppPoliMusicV1.ErrorPage" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Error · PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
<form id="errorForm" runat="server">
    <div class="pm-error-page">
        <div class="pm-auth-card" style="text-align:center;max-width:460px;">
            <h1><%= H(Code) %></h1>
            <h2 style="font-size:1.15rem;">No se pudo procesar la solicitud</h2>
            <p style="color:#A9808B;font-size:0.88rem;">
                <%= H(Detail) %>
            </p>
            <div style="margin-top:22px;display:flex;gap:10px;justify-content:center;flex-wrap:wrap;">
                <a class="pm-btn pm-btn-accent" href="<%= ResolveUrl("~/Default.aspx") %>">Volver al inicio</a>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Login.aspx") %>">Ir al inicio de sesión</a>
            </div>
        </div>
    </div>
</form>
</body>
</html>
