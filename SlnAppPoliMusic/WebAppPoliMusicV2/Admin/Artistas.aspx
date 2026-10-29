<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Artistas.aspx.cs" Inherits="WebAppPoliMusicV2.AdminArtistas" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Admin &middot; Artistas</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Artistas", "Alta, edición y borrado de artistas.",
                    "<button type=\"button\" class=\"pm-btn\" data-pm-modal-open=\"artistModal\">Nuevo artista</button>") %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin artistas", "Registre el primer artista con el botón \"Nuevo artista\".") %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th>Artista</th>
                    <th>Canciones</th>
                    <th>Imagen</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                <%= RowsHtml %>
            </tbody>
        </table>
    </div>
    <% } %>

    <div class="pm-modal-backdrop<%= ModalOpenClass %>" id="artistModal" role="dialog" aria-modal="true" aria-labelledby="artistModalTitle">
        <div class="pm-modal">
            <button type="button" class="pm-btn pm-btn-ghost pm-btn-sm pm-modal-close" data-pm-modal-close="1">Cerrar</button>
            <h2 id="artistModalTitle"><%= IsEditing ? "Editar artista" : "Nuevo artista" %></h2>

            <%= Ui.CsrfField(CsrfToken) %>
            <input type="hidden" name="id" value="<%= EditingId %>" />
            <input type="hidden" name="currentImage" value="<%= Ui.A(ImageValue) %>" />

            <div class="pm-form-grid">
                <div class="pm-field">
                    <label for="name">Nombre *</label>
                    <input class="pm-input" type="text" id="name" name="name" maxlength="100"
                           value="<%= Ui.A(NameValue) %>" required="required" />
                </div>
                <div class="pm-field">
                    <label for="lastname">Apellido</label>
                    <input class="pm-input" type="text" id="lastname" name="lastname" maxlength="100"
                           value="<%= Ui.A(LastnameValue) %>" />
                </div>
            </div>

            <div class="pm-field">
                <label for="image">Imagen</label>
                <input class="pm-input" type="file" id="image" name="image" accept=".jpg,.jpeg,.png,.gif,.webp" />
                <span class="pm-hint">
                    <%= IsEditing && ImageValue.Length > 0
                        ? "Imagen actual: " + Ui.E(ImageValue) + ". Si no sube otra, se conserva."
                        : "Opcional. Se guarda en la carpeta ArtistImages." %>
                </span>
            </div>

            <div class="pm-form-actions">
                <button type="submit" class="pm-btn" name="action" value="<%= IsEditing ? "update" : "create" %>">
                    <%= IsEditing ? "Guardar cambios" : "Registrar artista" %>
                </button>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Artistas.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>

</asp:Content>
