<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Albumes.aspx.cs" Inherits="WebAppPoliMusicV2.AdminAlbumes" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Admin &middot; &Aacute;lbumes</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Álbumes", "Alta, edición y borrado de álbumes.",
                    "<button type=\"button\" class=\"pm-btn\" data-pm-modal-open=\"albumModal\">Nuevo álbum</button>") %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <% if (RowsHtml.Length == 0) { %>
        <%= Ui.EmptyState("Sin álbumes", "Registre el primer álbum con el botón \"Nuevo álbum\".") %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th>Álbum</th>
                    <th>Año</th>
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

    <div class="pm-modal-backdrop<%= ModalOpenClass %>" id="albumModal" role="dialog" aria-modal="true" aria-labelledby="albumModalTitle">
        <div class="pm-modal">
            <button type="button" class="pm-btn pm-btn-ghost pm-btn-sm pm-modal-close" data-pm-modal-close="1">Cerrar</button>
            <h2 id="albumModalTitle"><%= IsEditing ? "Editar álbum" : "Nuevo álbum" %></h2>

            <%= Ui.CsrfField(CsrfToken) %>
            <input type="hidden" name="id" value="<%= EditingId %>" />
            <input type="hidden" name="currentImage" value="<%= Ui.A(ImageValue) %>" />

            <div class="pm-form-grid">
                <div class="pm-field">
                    <label for="title">Título *</label>
                    <input class="pm-input" type="text" id="title" name="title" maxlength="50"
                           value="<%= Ui.A(TitleValue) %>" required="required" />
                </div>
                <div class="pm-field">
                    <label for="year">Año de lanzamiento</label>
                    <input class="pm-input" type="number" id="year" name="year" min="1900" max="2100"
                           value="<%= Ui.A(YearValue) %>" />
                </div>
            </div>

            <div class="pm-field">
                <label for="image">Portada</label>
                <input class="pm-input" type="file" id="image" name="image" accept=".jpg,.jpeg,.png,.gif,.webp" />
                <span class="pm-hint">
                    <%= IsEditing && ImageValue.Length > 0
                        ? "Imagen actual: " + Ui.E(ImageValue) + ". Si no sube otra, se conserva."
                        : "Opcional. Se guarda en la carpeta AlbumImages." %>
                </span>
            </div>

            <div class="pm-form-actions">
                <button type="submit" class="pm-btn" name="action" value="<%= IsEditing ? "update" : "create" %>">
                    <%= IsEditing ? "Guardar cambios" : "Registrar álbum" %>
                </button>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Albumes.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>

</asp:Content>
