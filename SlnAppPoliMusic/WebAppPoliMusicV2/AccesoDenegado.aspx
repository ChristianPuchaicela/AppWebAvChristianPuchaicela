<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccesoDenegado.aspx.cs" Inherits="WebAppPoliMusicV2.AccesoDenegado" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Acceso denegado &middot; PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="pm-error-page">
            <div class="pm-error-card">
                <div class="pm-error-code">403</div>
                <h1>Acceso denegado</h1>
                <p>Su cuenta no tiene permisos para abrir esta secci&oacute;n. Las secciones de administraci&oacute;n solo est&aacute;n disponibles para usuarios con rol de administrador.</p>
                <p class="pm-note" style="text-align:left;">
                    Recurso solicitado: <strong><%= Ui.E(FromPath) %></strong>
                </p>
                <p>
                    <a class="pm-btn" href="<%= ResolveUrl("~/Default.aspx") %>">Volver al inicio</a>
                    <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Logout.aspx") %>">Cambiar de cuenta</a>
                </p>
            </div>
        </div>
    </form>
</body>
</html>
