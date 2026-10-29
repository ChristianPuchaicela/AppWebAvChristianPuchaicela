<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Canciones.aspx.cs" Inherits="WebAppPoliMusicV2.AdminCanciones" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Admin &middot; Canciones</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Canciones", "Alta, edición y borrado de canciones del catálogo.",
                    "<button type=\"button\" class=\"pm-btn\" data-pm-modal-open=\"songModal\">Nueva canción</button>") %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin canciones", "Registre la primera canción con el botón \"Nueva canción\".") %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>Canción</th>
                    <th>Artista</th>
                    <th>Álbum</th>
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

    <div class="pm-modal-backdrop<%= ModalOpenClass %>" id="songModal" role="dialog" aria-modal="true" aria-labelledby="songModalTitle">
        <div class="pm-modal">
            <button type="button" class="pm-btn pm-btn-ghost pm-btn-sm pm-modal-close" data-pm-modal-close="1">Cerrar</button>
            <h2 id="songModalTitle"><%= IsEditing ? "Editar canción" : "Nueva canción" %></h2>
            <p class="pm-page-sub" style="margin-bottom:20px;">
                El nombre y la ruta se guardan en TBL_SONG; el artista y el álbum en TBL_PLAYER y TBL_TRACKLIST.
            </p>

            <%= Ui.CsrfField(CsrfToken) %>
            <input type="hidden" name="id" value="<%= EditingId %>" />

            <div class="pm-field">
                <label for="name">Nombre de la canción *</label>
                <input class="pm-input" type="text" id="name" name="name" maxlength="50"
                       value="<%= Ui.A(NameValue) %>" required="required" />
            </div>

            <div class="pm-form-grid">
                <div class="pm-field">
                    <label for="artistId">Artista</label>
                    <select class="pm-select" id="artistId" name="artistId">
                        <option value="0">— Sin artista —</option>
                        <%= ArtistOptionsHtml %>
                    </select>
                </div>
                <div class="pm-field">
                    <label for="albumId">Álbum</label>
                    <select class="pm-select" id="albumId" name="albumId">
                        <option value="0">— Sin álbum —</option>
                        <%= AlbumOptionsHtml %>
                    </select>
                </div>
            </div>

            <div class="pm-field">
                <label for="plays">Reproducciones</label>
                <input class="pm-input" type="number" id="plays" name="plays" min="0" value="<%= PlaysValue %>" />
            </div>

            <div class="pm-field">
                <label for="songFile">Archivo MP3</label>
                <input class="pm-input" type="file" id="songFile" name="songFile" accept=".mp3" />
                <span class="pm-hint">
                    <%= IsEditing
                        ? "Si no sube un archivo se conserva la ruta actual: " + Ui.E(PathValue)
                        : "Máximo 50 MB. Se guarda en la carpeta SongFiles." %>
                </span>
            </div>

            <div class="pm-field">
                <label for="path">Ruta del archivo</label>
                <input class="pm-input" type="text" id="path" name="path" maxlength="255"
                       value="<%= Ui.A(PathValue) %>" placeholder="~/SongFiles/mi-cancion.mp3" />
                <span class="pm-hint">Puede indicar una ruta existente en lugar de subir un archivo.</span>
            </div>

            <div class="pm-form-actions">
                <button type="submit" class="pm-btn" name="action" value="<%= IsEditing ? "update" : "create" %>">
                    <%= IsEditing ? "Guardar cambios" : "Registrar canción" %>
                </button>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Canciones.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>

</asp:Content>
