<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Albumes.aspx.cs" Inherits="WebAppPoliMusicV2.Albumes" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">&Aacute;lbumes &middot; PoliMusic</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

<% if (IsDetail) { %>

    <p style="margin-bottom:18px;">
        <a class="pm-more" href="<%= ResolveUrl("~/Albumes.aspx") %>">&larr; Volver a &aacute;lbumes</a>
    </p>

    <div class="pm-detail">
        <div class="pm-detail-art">
            <%= Ui.Art(AlbumArt, AlbumTitle) %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker">&Aacute;lbum</div>
            <h1><%= Ui.E(AlbumTitle) %></h1>
            <div class="pm-detail-sub"><%= Ui.E(ReleaseLabel) %> &middot; <%= SongCount %> canci&oacute;n(es)</div>
            <div class="pm-detail-actions">
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Canciones.aspx") %>">Ver todo el cat&aacute;logo</a>
                <% if (IsAdmin) { %>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Albumes.aspx?edit=" + AlbumId) %>">Editar &aacute;lbum</a>
                <% } %>
            </div>
        </div>
    </div>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin canciones", "Este album todavia no tiene canciones asociadas en TBL_TRACKLIST.") %>
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

<% } else { %>

    <%= Ui.PageHead("Álbumes", Subtitle, string.Empty) %>

    <div class="pm-panel pm-filter" style="display:flex;gap:10px;flex-wrap:wrap;align-items:center;">
        <input class="pm-input" type="text" name="q" value="<%= Ui.A(SearchTerm) %>"
               placeholder="Buscar &aacute;lbum por t&iacute;tulo&hellip;" style="flex:1 1 260px;" aria-label="Buscar &aacute;lbum" />
        <button type="button" class="pm-btn" data-pm-filter="<%= ResolveUrl("~/Albumes.aspx") %>">Buscar</button>
        <% if (SearchTerm.Length > 0) { %>
        <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Albumes.aspx") %>">Limpiar</a>
        <% } %>
    </div>

    <% if (GridHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin álbumes", "No hay álbumes que coincidan con la búsqueda.") %>
    <% } else { %>
    <div class="pm-grid">
        <%= GridHtml %>
    </div>
    <% } %>

<% } %>

</asp:Content>
