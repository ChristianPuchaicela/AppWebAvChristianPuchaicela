<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PlaylistDetalle.aspx.cs" Inherits="WebAppPoliMusicV1.PlaylistDetalle" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server"><%= H(PageTitle) %> · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <% if (Playlist == null) { %>
        <%: new HtmlString(Ui.EmptyState("Playlist no disponible",
            "La playlist solicitada no existe o no tiene permiso para verla.")) %>
        <div style="margin-top:16px;">
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Playlists.aspx") %>">← Volver a mis playlists</a>
        </div>
    <% } else { %>

    <div class="pm-detail-head">
        <div class="pm-detail-art">
            <% if (ArtUrl != null) { %>
            <img src="<%= Url(ArtUrl) %>" alt="<%= H(Playlist.Title) %>" />
            <% } else { %>
            <span class="pm-art-initials"><%= H(Ui.InitialsOf(Playlist.Title)) %></span>
            <% } %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker"><%= IsOwner ? "Mi playlist" : "Playlist pública" %></div>
            <h1><%= H(Playlist.Title) %></h1>
            <div class="pm-detail-sub">
                Creada por <strong><%= H(Playlist.OwnerUsername) %></strong>
                el <%= Playlist.CreationDate.ToString("dd/MM/yyyy") %>
                · <%= Songs.Count %> canción(es)
                <% if (Playlist.IsPrivate) { %>
                · <span class="pm-badge pm-badge-private">Privada</span>
                <% } else { %>
                · <span class="pm-badge pm-badge-public">Pública</span>
                <% } %>
            </div>

            <div class="pm-detail-actions">
                <% if (QueueJson != "[]") { %>
                <button type="button" class="pm-btn pm-btn-accent"
                        data-pm-play='<%= FirstPlayJson %>'
                        data-pm-queue="<%= Url(QueueJson) %>">Reproducir</button>
                <% } else { %>
                <span class="pm-badge pm-badge-muted">Sin audio disponible</span>
                <% } %>

                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Playlists.aspx") %>">← Mis playlists</a>

                <% if (IsOwner) { %>
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-open="modalEdit">Editar</button>
                <a class="pm-btn pm-btn-danger"
                   href="<%= ResolveUrl("~/PlaylistDetalle.aspx?id=" + Playlist.IdPlaylist + "&action=delete") %>"
                   data-pm-confirm="¿Eliminar la playlist «<%= H(Playlist.Title) %>» y sus <%= Songs.Count %> canción(es) asociadas? Esta acción no se puede deshacer.">
                   Eliminar
                </a>
                <% } %>
            </div>
        </div>
    </div>

    <% if (Message != null) { %>
    <div class="alert <%= MessageIsError ? "alert-error" : "alert-success" %>"><%= H(Message) %></div>
    <% } %>

    <% if (IsOwner) { %>
    <div class="pm-panel">
        <h2>Agregar una canción</h2>
        <div class="pm-field" style="max-width:520px;">
            <label for="<%= ddlSong.ClientID %>">Canción</label>
            <asp:DropDownList ID="ddlSong" runat="server" CssClass="pm-select" />
            <span class="pm-hint">Las canciones ya incluidas no volverán a agregarse.</span>
        </div>
        <div class="pm-form-actions">
            <asp:Button ID="btnAddSong" runat="server" Text="Agregar a la playlist"
                        CssClass="pm-btn pm-btn-accent" OnClick="btnAddSong_Click" />
        </div>
    </div>
    <% } %>

    <section class="pm-section">
        <div class="pm-section-head"><h2>Canciones de la playlist</h2></div>

        <% if (Songs.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Playlist vacía",
                IsOwner ? "Agregue canciones con el selector de arriba."
                        : "Esta playlist todavía no tiene canciones.")) %>
        <% } else { %>
        <div class="pm-table-wrap">
            <table class="pm-table">
                <thead>
                    <tr>
                        <th style="width:64px;">#</th>
                        <th>Canción</th>
                        <th>Artista</th>
                        <th style="width:110px;">Reproducciones</th>
                        <th style="width:120px;">Acciones</th>
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

    <% if (IsOwner) { %>
    <!-- Modal: editar playlist -->
    <div class="pm-modal-backdrop" id="modalEdit">
        <div class="pm-modal">
            <h2>Editar playlist</h2>
            <div class="pm-field">
                <label for="<%= txtEditTitle.ClientID %>">Título <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtEditTitle" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlEditType.ClientID %>">Visibilidad</label>
                <asp:DropDownList ID="ddlEditType" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlEditPhoto.ClientID %>">Portada</label>
                <asp:DropDownList ID="ddlEditPhoto" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnSaveEdit" runat="server" Text="Guardar cambios"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnSaveEdit_Click" />
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>
    <% } %>

    <% } %>

</asp:Content>
