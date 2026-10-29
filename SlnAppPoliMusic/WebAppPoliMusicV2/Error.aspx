<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="WebAppPoliMusicV2.ErrorPage" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Error &middot; PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="pm-error-page">
            <div class="pm-error-card">
                <div class="pm-error-code">500</div>
                <h1>Algo sali&oacute; mal</h1>
                <p>La aplicaci&oacute;n no pudo completar la operaci&oacute;n. Intente nuevamente; si el problema persiste, revise la conexi&oacute;n con la base de datos BDD_PoliMusic.</p>
                <p class="pm-note" style="text-align:left;"><%= Ui.E(Detail) %></p>
                <p>
                    <a class="pm-btn" href="<%= ResolveUrl("~/Default.aspx") %>">Volver al inicio</a>
                    <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Login.aspx") %>">Iniciar sesi&oacute;n</a>
                </p>
            </div>
        </div>
    </form>
</body>
</html>
