<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Usuarios.aspx.cs" Inherits="WebAppPoliMusicV2.AdminUsuarios" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Admin &middot; Usuarios</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Usuarios", "Cuentas registradas, roles y eliminación. Debe existir siempre al menos un administrador.", string.Empty) %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <p class="pm-note" style="margin-bottom:22px;">
        El registro público siempre crea usuarios con rol <strong>Usuario</strong>. Solo desde esta pantalla se puede
        promover o degradar una cuenta. No es posible eliminarse a sí mismo ni dejar el sistema sin administradores.
    </p>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin usuarios", "La tabla TBL_USER está vacía.") %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th>Usuario</th>
                    <th>Correo</th>
                    <th>Rol</th>
                    <th>Nacimiento</th>
                    <th>Playlists</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                <%= RowsHtml %>
            </tbody>
        </table>
    </div>
    <% } %>

</asp:Content>
