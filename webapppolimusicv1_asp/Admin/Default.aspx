<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="WebAppPoliMusicV1.Admin.AdminDefault" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Panel de administración · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <section class="pm-hero">
        <h1>Panel de administración</h1>
        <p>
            Gestión completa del catálogo de PoliMusic. Como administrador puede crear,
            editar y eliminar usuarios, canciones, artistas, álbumes y playlists.
        </p>
    </section>

    <div class="pm-stats">
        <div class="pm-stat">
            <div class="pm-stat-value"><%= UserTotal %></div>
            <div class="pm-stat-label">Usuarios</div>
        </div>
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
    </div>

    <div class="pm-stats">
        <div class="pm-stat">
            <div class="pm-stat-value"><%= PlaylistTotal %></div>
            <div class="pm-stat-label">Playlists</div>
        </div>
        <div class="pm-stat">
            <div class="pm-stat-value"><%= AdminTotal %></div>
            <div class="pm-stat-label">Administradores</div>
        </div>
        <div class="pm-stat">
            <div class="pm-stat-value"><%= PlayableSongs %>/<%= SongTotal %></div>
            <div class="pm-stat-label">Canciones con audio</div>
        </div>
    </div>

    <% if (!string.IsNullOrEmpty(AudioNotice)) { %>
    <div class="alert alert-warning"><%= H(AudioNotice) %></div>
    <% } %>

    <div class="pm-section">
        <div class="pm-section-head"><h2>Módulos de administración</h2></div>
        <div class="pm-grid">
            <article class="pm-card">
                <a class="pm-card-link" href="<%= ResolveUrl("~/Admin/Usuarios.aspx") %>"></a>
                <div class="pm-card-art" style="aspect-ratio:16/9;">
                    <span class="pm-art-initials">US</span>
                </div>
                <div class="pm-card-title">Usuarios</div>
                <div class="pm-card-sub">Roles y cuentas</div>
            </article>
            <article class="pm-card">
                <a class="pm-card-link" href="<%= ResolveUrl("~/Admin/Canciones.aspx") %>"></a>
                <div class="pm-card-art" style="aspect-ratio:16/9;">
                    <span class="pm-art-initials">CA</span>
                </div>
                <div class="pm-card-title">Canciones</div>
                <div class="pm-card-sub">Catálogo y rutas de audio</div>
            </article>
            <article class="pm-card">
                <a class="pm-card-link" href="<%= ResolveUrl("~/Admin/Artistas.aspx") %>"></a>
                <div class="pm-card-art" style="aspect-ratio:16/9;">
                    <span class="pm-art-initials">AR</span>
                </div>
                <div class="pm-card-title">Artistas</div>
                <div class="pm-card-sub">Intérpretes</div>
            </article>
            <article class="pm-card">
                <a class="pm-card-link" href="<%= ResolveUrl("~/Admin/Albumes.aspx") %>"></a>
                <div class="pm-card-art" style="aspect-ratio:16/9;">
                    <span class="pm-art-initials">AL</span>
                </div>
                <div class="pm-card-title">Álbumes</div>
                <div class="pm-card-sub">Discografía</div>
            </article>
            <article class="pm-card">
                <a class="pm-card-link" href="<%= ResolveUrl("~/Admin/Playlists.aspx") %>"></a>
                <div class="pm-card-art" style="aspect-ratio:16/9;">
                    <span class="pm-art-initials">PL</span>
                </div>
                <div class="pm-card-title">Playlists</div>
                <div class="pm-card-sub">Todas las listas</div>
            </article>
        </div>
    </div>

    <div class="pm-panel">
        <h2>Últimos usuarios registrados</h2>
        <% if (RecentUsers.Count == 0) { %>
            <div class="pm-empty-sm">Sin usuarios registrados.</div>
        <% } else { %>
        <div class="pm-table-wrap" style="box-shadow:none;">
            <table class="pm-table">
                <thead>
                    <tr>
                        <th>Usuario</th>
                        <th>Correo</th>
                        <th style="width:140px;">Rol</th>
                        <th style="width:120px;">Playlists</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var row in RecentUserRows) { %>
                        <%: new HtmlString(row) %>
                    <% } %>
                </tbody>
            </table>
        </div>
        <% } %>
    </div>

</asp:Content>
