<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Albumes.aspx.cs" Inherits="WebAppPoliMusicV1.Albumes" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Álbumes · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <% if (SelectedAlbum != null) { %>
    <div class="pm-detail-head">
        <div class="pm-detail-art">
            <% if (SelectedAlbumArt != null) { %>
            <img src="<%= Url(SelectedAlbumArt) %>" alt="<%= H(SelectedAlbum.TitleAlbum) %>" />
            <% } else { %>
            <span class="pm-art-initials"><%= H(Ui.InitialsOf(SelectedAlbum.TitleAlbum)) %></span>
            <% } %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker">Álbum</div>
            <h1><%= H(SelectedAlbum.TitleAlbum) %></h1>
            <div class="pm-detail-sub">
                <%= SelectedAlbum.ReleaseYear.HasValue ? SelectedAlbum.ReleaseYear.Value.ToString() : "Año desconocido" %>
                · <%= AlbumSongs.Count %> canción(es)
                <% if (!string.IsNullOrEmpty(ArtistNames)) { %>
                · <%= H(ArtistNames) %>
                <% } %>
            </div>
            <div class="pm-detail-actions">
                <% if (AlbumQueueJson != "[]") { %>
                <button type="button" class="pm-btn pm-btn-accent"
                        data-pm-play='<%= AlbumFirstPlayJson %>'
                        data-pm-queue="<%= Url(AlbumQueueJson) %>">
                    Reproducir álbum
                </button>
                <% } else { %>
                <span class="pm-badge pm-badge-muted">Sin audio disponible</span>
                <% } %>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Albumes.aspx") %>">← Todos los álbumes</a>
            </div>
        </div>
    </div>

    <section class="pm-section">
        <div class="pm-section-head"><h2>Lista de canciones</h2></div>
        <% if (AlbumSongs.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Álbum sin canciones",
                "Este álbum no tiene canciones en TBL_TRACKLIST.")) %>
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
    <div class="pm-page-head">
        <div>
            <h1>Álbumes</h1>
            <p class="pm-page-sub">Discografía disponible en la biblioteca.</p>
        </div>
        <% if (IsAdmin) { %>
        <a class="pm-btn" href="<%= ResolveUrl("~/Admin/Albumes.aspx") %>">Administrar álbumes</a>
        <% } %>
    </div>

    <% if (Albums.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("Sin álbumes", "La base de datos todavía no tiene álbumes registrados.")) %>
    <% } else { %>
    <div class="pm-grid">
        <% foreach (var card in Cards) { %>
            <%: new HtmlString(card) %>
        <% } %>
    </div>
    <% } %>
    <% } %>

</asp:Content>
