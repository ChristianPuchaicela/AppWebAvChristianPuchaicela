<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="WebAppPoliMusicV1.Default" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Inicio · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <section class="pm-hero">
        <h1>Bienvenido, <%= H(CurrentUsername) %></h1>
        <p>
            Este es su espacio musical en PoliMusic. Explore el catálogo, arme sus playlists
            y reproduzca canciones sin salir de la página.
        </p>
    </section>

    <div class="pm-stats">
        <div class="pm-stat">
            <div class="pm-stat-value"><%= SongTotal %></div>
            <div class="pm-stat-label">Canciones</div>
        </div>
        <div class="pm-stat">
            <div class="pm-stat-value"><%= ArtistTotal %></div>
            <div class="pm-stat-label">Artistas</div>
        </div>
        <div class="pm-stat">
            <div class="pm-stat-value"><%= AlbumTotal %></div>
            <div class="pm-stat-label">Álbumes</div>
        </div>
        <div class="pm-stat">
            <div class="pm-stat-value"><%= MyPlaylistTotal %></div>
            <div class="pm-stat-label">Mis playlists</div>
        </div>
    </div>

    <% if (!string.IsNullOrEmpty(AudioWarning)) { %>
    <div class="alert alert-warning"><%= H(AudioWarning) %></div>
    <% } %>

    <!-- Canciones destacadas -->
    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Canciones destacadas</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Canciones.aspx") %>">Ver todas →</a>
        </div>
        <% if (FeaturedSongs.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Todavía no hay canciones", "Cuando existan canciones en la base de datos aparecerán aquí.")) %>
        <% } else { %>
        <div class="pm-table-wrap">
            <table class="pm-table">
                <thead>
                    <tr>
                        <th style="width:64px;">#</th>
                        <th>Canción</th>
                        <th>Artista</th>
                        <th style="width:120px;">Reproducciones</th>
                        <th style="width:110px;">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var row in FeaturedRows) { %>
                        <%: new HtmlString(row) %>
                    <% } %>
                </tbody>
            </table>
        </div>
        <% } %>
    </section>

    <!-- Álbumes destacados -->
    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Álbumes</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Albumes.aspx") %>">Ver todos →</a>
        </div>
        <% if (Albums.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Sin álbumes", "Aún no se han registrado álbumes.")) %>
        <% } else { %>
        <div class="pm-grid">
            <% foreach (var card in AlbumCards) { %>
                <%: new HtmlString(card) %>
            <% } %>
        </div>
        <% } %>
    </section>

    <!-- Artistas -->
    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Artistas</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Artistas.aspx") %>">Ver todos →</a>
        </div>
        <% if (Artists.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Sin artistas", "Aún no se han registrado artistas.")) %>
        <% } else { %>
        <div class="pm-grid">
            <% foreach (var card in ArtistCards) { %>
                <%: new HtmlString(card) %>
            <% } %>
        </div>
        <% } %>
    </section>

    <!-- Mis playlists -->
    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Mis playlists</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Playlists.aspx") %>">Gestionar →</a>
        </div>
        <% if (MyPlaylists.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Todavía no tiene playlists", "Cree su primera playlist desde la sección Playlists.")) %>
        <% } else { %>
        <div class="pm-grid">
            <% foreach (var card in PlaylistCards) { %>
                <%: new HtmlString(card) %>
            <% } %>
        </div>
        <% } %>
    </section>

</asp:Content>
