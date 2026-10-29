<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebAppPoliMusicV2.AdminDefault" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Panel &middot; PoliMusic</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Panel de administración", "Resumen del catálogo y accesos directos a la gestión de datos.", string.Empty) %>

    <div class="pm-stats">
        <%= StatsHtml %>
    </div>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Gestión</h2>
        </div>
        <div class="pm-grid">
            <a class="pm-card" href="<%= ResolveUrl("~/Admin/Canciones.aspx") %>" style="text-decoration:none;">
                <div class="pm-card-art"><span class="pm-art-initials">♪</span></div>
                <div class="pm-card-title">Canciones</div>
                <div class="pm-card-sub">Alta, edición y borrado</div>
            </a>
            <a class="pm-card" href="<%= ResolveUrl("~/Admin/Artistas.aspx") %>" style="text-decoration:none;">
                <div class="pm-card-art"><span class="pm-art-initials">AR</span></div>
                <div class="pm-card-title">Artistas</div>
                <div class="pm-card-sub">Catálogo de artistas</div>
            </a>
            <a class="pm-card" href="<%= ResolveUrl("~/Admin/Albumes.aspx") %>" style="text-decoration:none;">
                <div class="pm-card-art"><span class="pm-art-initials">AL</span></div>
                <div class="pm-card-title">&Aacute;lbumes</div>
                <div class="pm-card-sub">Discografía</div>
            </a>
            <a class="pm-card" href="<%= ResolveUrl("~/Admin/Usuarios.aspx") %>" style="text-decoration:none;">
                <div class="pm-card-art"><span class="pm-art-initials">US</span></div>
                <div class="pm-card-title">Usuarios</div>
                <div class="pm-card-sub">Roles y cuentas</div>
            </a>
            <a class="pm-card" href="<%= ResolveUrl("~/Admin/Playlists.aspx") %>" style="text-decoration:none;">
                <div class="pm-card-art"><span class="pm-art-initials">PL</span></div>
                <div class="pm-card-title">Playlists</div>
                <div class="pm-card-sub">Todas las playlists</div>
            </a>
        </div>
    </section>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Últimos usuarios</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Admin/Usuarios.aspx") %>">Gestionar &rarr;</a>
        </div>
        <% if (UsersHtml.Length == 0) { %>
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
                    </tr>
                </thead>
                <tbody>
                    <%= UsersHtml %>
                </tbody>
            </table>
        </div>
        <% } %>
    </section>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Canciones más escuchadas</h2>
            <a class="pm-more" href="<%= ResolveUrl("~/Canciones.aspx") %>">Ver catálogo &rarr;</a>
        </div>
        <% if (SongsHtml.Length == 0) { %>
            <%= Ui.EmptyState("Sin canciones", "La tabla TBL_SONG está vacía.") %>
        <% } else { %>
        <div class="pm-table-wrap">
            <table class="pm-table">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Canción</th>
                        <th>Artista</th>
                        <th>Reproducciones</th>
                    </tr>
                </thead>
                <tbody>
                    <%= SongsHtml %>
                </tbody>
            </table>
        </div>
        <% } %>
    </section>

</asp:Content>
