<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Artistas.aspx.cs" Inherits="WebAppPoliMusicV1.Artistas" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Artistas · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <% if (SelectedArtist != null) { %>
    <!-- Vista de detalle de un artista -->
    <div class="pm-detail-head">
        <div class="pm-detail-art round">
            <% if (SelectedArtistArt != null) { %>
            <img src="<%= Url(SelectedArtistArt) %>" alt="<%= H(SelectedArtist.FullName) %>" />
            <% } else { %>
            <span class="pm-art-initials"><%= H(Ui.InitialsOf(SelectedArtist.FullName)) %></span>
            <% } %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker">Artista</div>
            <h1><%= H(SelectedArtist.FullName) %></h1>
            <div class="pm-detail-sub">
                <%= ArtistSongCount %> canción(es) en su catálogo
                <% if (!string.IsNullOrEmpty(AlbumNames)) { %>
                · Álbumes: <%= H(AlbumNames) %>
                <% } %>
            </div>
            <div class="pm-detail-actions">
                <% if (ArtistQueueJson != "[]") { %>
                <button type="button" class="pm-btn pm-btn-accent"
                        data-pm-play='<%= ArtistFirstPlayJson %>'
                        data-pm-queue="<%= Url(ArtistQueueJson) %>">
                    Reproducir catálogo
                </button>
                <% } else { %>
                <span class="pm-badge pm-badge-muted">Sin audio disponible</span>
                <% } %>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Artistas.aspx") %>">← Todos los artistas</a>
            </div>
        </div>
    </div>

    <section class="pm-section">
        <div class="pm-section-head"><h2>Canciones de <%= H(SelectedArtist.FullName) %></h2></div>
        <% if (ArtistSongs.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Sin canciones asociadas",
                "Este artista no tiene canciones vinculadas en TBL_PLAYER.")) %>
        <% } else { %>
        <div class="pm-table-wrap">
            <table class="pm-table">
                <thead>
                    <tr>
                        <th style="width:64px;">#</th>
                        <th>Canción</th>
                        <th>Artista</th>
                        <th style="width:110px;">Reproducciones</th>
                        <th style="width:110px;">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var row in Rows) { %>
                        <%: new HtmlString(row) %>
                    <% } %>
                </tbody>
            </table>
        </div>
        <% } %>
    </section>

    <% } else { %>
    <!-- Listado de artistas -->
    <div class="pm-page-head">
        <div>
            <h1>Artistas</h1>
            <p class="pm-page-sub">Explore el catálogo musical por intérprete.</p>
        </div>
        <% if (IsAdmin) { %>
        <a class="pm-btn" href="<%= ResolveUrl("~/Admin/Artistas.aspx") %>">Administrar artistas</a>
        <% } %>
    </div>

    <% if (Artists.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("Sin artistas", "La base de datos todavía no tiene artistas registrados.")) %>
    <% } else { %>
    <div class="pm-grid">
        <% foreach (var card in Cards) { %>
            <%: new HtmlString(card) %>
        <% } %>
    </div>
    <% } %>
    <% } %>

</asp:Content>
