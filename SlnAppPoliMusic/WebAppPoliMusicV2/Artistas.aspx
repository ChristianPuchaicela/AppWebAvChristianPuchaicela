<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Artistas.aspx.cs" Inherits="WebAppPoliMusicV2.Artistas" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Artistas &middot; PoliMusic</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

<% if (IsDetail) { %>

    <p style="margin-bottom:18px;">
        <a class="pm-more" href="<%= ResolveUrl("~/Artistas.aspx") %>">&larr; Volver a artistas</a>
    </p>

    <div class="pm-detail">
        <div class="pm-detail-art round">
            <%= Ui.Art(ArtistArt, ArtistName) %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker">Artista</div>
            <h1><%= Ui.E(ArtistName) %></h1>
            <div class="pm-detail-sub"><%= SongCount %> canci&oacute;n(es) en el cat&aacute;logo</div>
            <div class="pm-detail-actions">
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Canciones.aspx") %>">Ver todo el cat&aacute;logo</a>
                <% if (IsAdmin) { %>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Artistas.aspx?edit=" + ArtistId) %>">Editar artista</a>
                <% } %>
            </div>
        </div>
    </div>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin canciones", "Este artista todavia no tiene canciones asociadas en TBL_PLAYER.") %>
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

    <%= Ui.PageHead("Artistas", Subtitle, string.Empty) %>

    <div class="pm-panel pm-filter" style="display:flex;gap:10px;flex-wrap:wrap;align-items:center;">
        <input class="pm-input" type="text" name="q" value="<%= Ui.A(SearchTerm) %>"
               placeholder="Buscar artista por nombre o apellido&hellip;" style="flex:1 1 260px;" aria-label="Buscar artista" />
        <button type="button" class="pm-btn" data-pm-filter="<%= ResolveUrl("~/Artistas.aspx") %>">Buscar</button>
        <% if (SearchTerm.Length > 0) { %>
        <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Artistas.aspx") %>">Limpiar</a>
        <% } %>
    </div>

    <% if (GridHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin artistas", "No hay artistas que coincidan con la busqueda.") %>
    <% } else { %>
    <div class="pm-grid">
        <%= GridHtml %>
    </div>
    <% } %>

<% } %>

</asp:Content>
