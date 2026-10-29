<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Playlists.aspx.cs" Inherits="WebAppPoliMusicV2.Playlists" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Playlists &middot; PoliMusic</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Playlists", "Organiza tus canciones favoritas. Las playlists privadas solo las ves tu.",
                    "<button type=\"button\" class=\"pm-btn\" data-pm-modal-open=\"newPlaylist\">Nueva playlist</button>") %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Mis playlists</h2>
            <span class="pm-more"><%= OwnCount %> playlist(s)</span>
        </div>
        <% if (OwnHtml.Length == 0) { %>
            <%= Ui.EmptyState("Todavía no tienes playlists", "Crea tu primera playlist y agrega canciones desde su detalle.") %>
        <% } else { %>
        <div class="pm-grid">
            <%= OwnHtml %>
        </div>
        <% } %>
    </section>

    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Playlists públicas de otros usuarios</h2>
            <span class="pm-more"><%= PublicCount %> playlist(s)</span>
        </div>
        <% if (PublicHtml.Length == 0) { %>
            <%= Ui.EmptyState("Sin playlists públicas", "Ningún otro usuario ha publicado playlists por ahora.") %>
        <% } else { %>
        <div class="pm-grid">
            <%= PublicHtml %>
        </div>
        <% } %>
    </section>

    <!-- Modal de creacion -->
    <div class="pm-modal-backdrop" id="newPlaylist" role="dialog" aria-modal="true" aria-labelledby="newPlaylistTitle">
        <div class="pm-modal">
            <button type="button" class="pm-btn pm-btn-ghost pm-btn-sm pm-modal-close" data-pm-modal-close="1">Cerrar</button>
            <h2 id="newPlaylistTitle">Nueva playlist</h2>
            <p class="pm-page-sub" style="margin-bottom:20px;">Elija un titulo y la visibilidad inicial.</p>

            <%= Ui.CsrfField(CsrfToken) %>

            <div class="pm-field">
                <label for="title">T&iacute;tulo *</label>
                <input class="pm-input" type="text" id="title" name="title" maxlength="50"
                       placeholder="Ej. Favoritas del laboratorio" required="required" />
            </div>

            <div class="pm-field">
                <label for="type">Visibilidad</label>
                <select class="pm-select" id="type" name="type">
                    <option value="0">P&uacute;blica &mdash; visible para todos</option>
                    <option value="1">Privada &mdash; solo para m&iacute;</option>
                </select>
            </div>

            <div class="pm-field">
                <label for="photo">Portada</label>
                <input class="pm-input" type="file" id="photo" name="photo" accept=".jpg,.jpeg,.png,.gif,.webp" />
                <span class="pm-hint">Opcional. Si no sube una imagen se usa la portada por defecto.</span>
            </div>

            <div class="pm-form-actions">
                <button type="submit" class="pm-btn" name="action" value="create">Crear playlist</button>
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

</asp:Content>
