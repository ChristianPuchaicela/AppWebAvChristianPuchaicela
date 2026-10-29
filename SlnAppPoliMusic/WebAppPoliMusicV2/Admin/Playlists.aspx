<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Playlists.aspx.cs" Inherits="WebAppPoliMusicV2.AdminPlaylists" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Admin &middot; Playlists</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Playlists", "Todas las playlists del sistema, incluidas las privadas de cada usuario.", string.Empty) %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin playlists", "Todavía no se ha creado ninguna playlist.") %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th>Playlist</th>
                    <th>Propietario</th>
                    <th>Visibilidad</th>
                    <th>Canciones</th>
                    <th>Creada</th>
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
