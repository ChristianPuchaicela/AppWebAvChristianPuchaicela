<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="PlaylistDetalle.aspx.cs" Inherits="WebAppPoliMusicV2.PlaylistDetalle" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server"><%= Ui.E(PageTitleText) %></asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

<% if (NotFound) { %>

    <%= Ui.PageHead("Playlist no disponible", "La playlist solicitada no existe o es privada.", string.Empty) %>
    <%= Ui.EmptyState("No se puede abrir esta playlist", "Verifique el enlace o vuelva al listado de playlists.") %>
    <p style="margin-top:20px;">
        <a class="pm-btn" href="<%= ResolveUrl("~/Playlists.aspx") %>">Volver a playlists</a>
    </p>

<% } else { %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <div class="pm-detail">
        <div class="pm-detail-art">
            <%= Ui.Art(PlaylistArt, PlaylistTitle) %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker">Playlist &middot; <%= IsOwner ? "Tuya" : "De " + Ui.E(OwnerName) %></div>
            <h1><%= Ui.E(PlaylistTitle) %></h1>
            <div class="pm-detail-sub">
                <%= VisibilityHtml %>
                &middot; <%= SongCount %> canci&oacute;n(es)
                &middot; Creada el <%= Ui.E(CreationDateText) %>
            </div>
            <div class="pm-detail-actions">
                <% if (IsOwner) { %>
                <button type="button" class="pm-btn" data-pm-modal-open="addSongs">Agregar canciones</button>
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-open="editPlaylist">Editar</button>
                <button type="submit" class="pm-btn pm-btn-danger" name="action" value="delete"
                        data-pm-confirm="¿Eliminar esta playlist y sus vínculos con canciones?">Eliminar</button>
                <% } %>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Playlists.aspx") %>">Volver</a>
            </div>
        </div>
    </div>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Playlist vacía", "Agrega canciones del catálogo con el botón \"Agregar canciones\".") %>
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

    <% if (IsOwner) { %>

    <%= Ui.CsrfField(CsrfToken) %>

    <!-- Modal: agregar canciones -->
    <div class="pm-modal-backdrop" id="addSongs" role="dialog" aria-modal="true" aria-labelledby="addSongsTitle">
        <div class="pm-modal">
            <button type="button" class="pm-btn pm-btn-ghost pm-btn-sm pm-modal-close" data-pm-modal-close="1">Cerrar</button>
            <h2 id="addSongsTitle">Agregar canciones</h2>
            <p class="pm-page-sub" style="margin-bottom:18px;">Seleccione las canciones del cat&aacute;logo que quiera sumar.</p>

            <% if (CatalogOptionsHtml.Length == 0) { %>
                <p class="pm-note">No quedan canciones por agregar: todas las del cat&aacute;logo ya est&aacute;n en esta playlist.</p>
            <% } else { %>
            <div class="pm-field" style="max-height:320px;overflow-y:auto;">
                <%= CatalogOptionsHtml %>
            </div>
            <div class="pm-form-actions">
                <button type="submit" class="pm-btn" name="action" value="addSongs">Agregar seleccionadas</button>
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
            <% } %>
        </div>
    </div>

    <!-- Modal: editar playlist -->
    <div class="pm-modal-backdrop" id="editPlaylist" role="dialog" aria-modal="true" aria-labelledby="editPlaylistTitle">
        <div class="pm-modal">
            <button type="button" class="pm-btn pm-btn-ghost pm-btn-sm pm-modal-close" data-pm-modal-close="1">Cerrar</button>
            <h2 id="editPlaylistTitle">Editar playlist</h2>

            <div class="pm-field">
                <label for="title">T&iacute;tulo *</label>
                <input class="pm-input" type="text" id="title" name="title" maxlength="50"
                       value="<%= Ui.A(PlaylistTitle) %>" required="required" />
            </div>

            <div class="pm-field">
                <label for="type">Visibilidad</label>
                <select class="pm-select" id="type" name="type">
                    <option value="0"<%= IsPrivate ? "" : " selected=\"selected\"" %>>P&uacute;blica</option>
                    <option value="1"<%= IsPrivate ? " selected=\"selected\"" : "" %>>Privada</option>
                </select>
            </div>

            <div class="pm-field">
                <label for="photo">Cambiar portada</label>
                <input class="pm-input" type="file" id="photo" name="photo" accept=".jpg,.jpeg,.png,.gif,.webp" />
                <span class="pm-hint">Si no sube una imagen se conserva la portada actual.</span>
            </div>

            <div class="pm-form-actions">
                <button type="submit" class="pm-btn" name="action" value="update">Guardar cambios</button>
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

    <% } %>

<% } %>

</asp:Content>
