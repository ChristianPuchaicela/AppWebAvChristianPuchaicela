<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebAppPoliMusicV2.Default" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Inicio &middot; PoliMusic</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="pm-hero">
        <span class="pm-hero-kicker">
            <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" aria-hidden="true"><path d="M9 18V5l10-2v13"/><circle cx="6" cy="18" r="3"/><circle cx="16" cy="16" r="3"/></svg>
            Cat&aacute;logo PoliMusic
        </span>
        <h1>Hola, <%= Ui.E(DisplayName) %>. &iquest;Qu&eacute; escuchamos hoy?</h1>
        <p>Explora el cat&aacute;logo de canciones, artistas y &aacute;lbumes, reproduce desde la barra inferior y organiza tu m&uacute;sica en playlists p&uacute;blicas o privadas.</p>
        <div class="pm-hero-actions">
            <a class="pm-btn" href="<%= ResolveUrl("~/Canciones.aspx") %>">
                <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M8 5v14l11-7z"/></svg>
                Explorar canciones
            </a>
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Playlists.aspx") %>">Mis playlists</a>
            <% if (IsAdmin) { %>
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Default.aspx") %>">Panel de administraci&oacute;n</a>
            <% } %>
        </div>
    </section>

    <div class="pm-stats">
        <%= StatsHtml %>
    </div>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>M&aacute;s escuchadas</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Canciones.aspx") %>">Ver todas &rarr;</a>
        </div>
        <% if (TopSongsHtml.Length == 0) { %>
            <%= Ui.EmptyState("Todavía no hay canciones", "El catálogo está vacío o no se pudo leer la tabla TBL_SONG.") %>
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
                    <%= TopSongsHtml %>
                </tbody>
            </table>
        </div>
        <% } %>
    </section>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>&Aacute;lbumes</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Albumes.aspx") %>">Ver todos &rarr;</a>
        </div>
        <% if (AlbumsHtml.Length == 0) { %>
            <%= Ui.EmptyState("Sin álbumes", "Aún no se han registrado álbumes en el catálogo.") %>
        <% } else { %>
        <div class="pm-grid">
            <%= AlbumsHtml %>
        </div>
        <% } %>
    </section>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Artistas</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Artistas.aspx") %>">Ver todos &rarr;</a>
        </div>
        <% if (ArtistsHtml.Length == 0) { %>
            <%= Ui.EmptyState("Sin artistas", "Aún no se han registrado artistas en el catálogo.") %>
        <% } else { %>
        <div class="pm-grid">
            <%= ArtistsHtml %>
        </div>
        <% } %>
    </section>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Tus playlists</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Playlists.aspx") %>">Gestionar &rarr;</a>
        </div>
        <% if (PlaylistsHtml.Length == 0) { %>
            <%= Ui.EmptyState("Todavía no tienes playlists", "Crea tu primera playlist desde la sección Playlists y agrega canciones del catálogo.") %>
        <% } else { %>
        <div class="pm-grid">
            <%= PlaylistsHtml %>
        </div>
        <% } %>
    </section>

</asp:Content>
