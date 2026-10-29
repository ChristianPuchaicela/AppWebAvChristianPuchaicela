<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Canciones.aspx.cs" Inherits="WebAppPoliMusicV2.Canciones" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Canciones &middot; PoliMusic</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Canciones", Subtitle, string.Empty) %>

    <div class="pm-panel pm-filter" style="display:flex;gap:10px;flex-wrap:wrap;align-items:center;">
        <input class="pm-input" type="text" name="q" value="<%= Ui.A(SearchTerm) %>"
               placeholder="Buscar por nombre de canci&oacute;n&hellip;" style="flex:1 1 260px;" aria-label="Buscar canci&oacute;n" />
        <button type="button" class="pm-btn" data-pm-filter="<%= ResolveUrl("~/Canciones.aspx") %>">Buscar</button>
        <% if (SearchTerm.Length > 0) { %>
        <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Canciones.aspx") %>">Limpiar</a>
        <% } %>
    </div>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin resultados", "No hay canciones que coincidan con la busqueda. Pruebe con otro termino.") %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>Canci&oacute;n</th>
                    <th>Artista</th>
                    <th>&Aacute;lbum</th>
                    <th>Reproducciones</th>
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
