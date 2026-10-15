<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AccesoDenegado.aspx.cs" Inherits="WebAppPoliMusicV1.AccesoDenegado" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Acceso denegado · PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
<form id="deniedForm" runat="server">
    <div class="pm-error-page">
        <div class="pm-auth-card" style="text-align:center;max-width:460px;">
            <h1>403</h1>
            <h2 style="font-size:1.15rem;">Acceso denegado</h2>
            <p style="color:#A9808B;font-size:0.88rem;">
                Su cuenta no tiene el rol necesario para ver esta sección.
                Las secciones de administración requieren el rol de administrador.
            </p>
            <% if (!string.IsNullOrEmpty(CurrentUserLabel)) { %>
            <p class="pm-note" style="text-align:left;margin-top:16px;">
                Sesión actual: <strong><%= H(CurrentUserLabel) %></strong> — rol <strong><%= H(CurrentRoleName) %></strong>.
            </p>
            <% } %>
            <div style="margin-top:22px;display:flex;gap:10px;justify-content:center;flex-wrap:wrap;">
                <a class="pm-btn pm-btn-accent" href="<%= ResolveUrl("~/Default.aspx") %>">Ir al inicio</a>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Logout.aspx") %>">Cerrar sesión</a>
            </div>
        </div>
    </div>
</form>
</body>
</html>
