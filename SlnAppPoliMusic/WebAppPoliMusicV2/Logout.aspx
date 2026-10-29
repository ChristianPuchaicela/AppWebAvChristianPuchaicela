<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="WebAppPoliMusicV2.Logout" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Cerrando sesi&oacute;n &middot; PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="pm-error-page">
            <div class="pm-error-card">
                <div class="pm-error-code">&#9834;</div>
                <h1>Cerrando sesi&oacute;n</h1>
                <p>Su sesi&oacute;n se cerr&oacute; correctamente. Redirigiendo al inicio de sesi&oacute;n&hellip;</p>
                <p><a class="pm-btn" href="<%= ResolveUrl("~/Login.aspx") %>">Ir al inicio de sesi&oacute;n</a></p>
            </div>
        </div>
    </form>
</body>
</html>
