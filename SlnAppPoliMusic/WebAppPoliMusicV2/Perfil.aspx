<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Perfil.aspx.cs" Inherits="WebAppPoliMusicV2.Perfil" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Mi perfil &middot; PoliMusic</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <%= Ui.PageHead("Mi perfil", "Actualice sus datos personales y su contraseña.", string.Empty) %>

    <%= Ui.Alert(FlashMessage, FlashIsError) %>
    <%= Ui.Alert(ErrorMessage, true) %>

    <div class="pm-detail" style="align-items:center;margin-bottom:26px;">
        <div class="pm-detail-art round" style="width:120px;height:120px;flex:0 0 120px;">
            <%= Ui.Art(PhotoUrl, Username) %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker"><%= RoleName %></div>
            <h1><%= Ui.E(Username) %></h1>
            <div class="pm-detail-sub">
                <%= Ui.E(Email) %>
                <% if (BirthdayText.Length > 0) { %> &middot; Nacimiento: <%= Ui.E(BirthdayText) %><% } %>
                <% if (AgeText.Length > 0) { %> &middot; <%= Ui.E(AgeText) %><% } %>
            </div>
            <div class="pm-detail-actions">
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Playlists.aspx") %>">
                    Mis playlists (<%= PlaylistCount %>)
                </a>
                <% if (IsAdmin) { %>
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Default.aspx") %>">Panel de administración</a>
                <% } %>
            </div>
        </div>
    </div>

    <div class="pm-panel">
        <h2>Datos personales</h2>
        <p class="pm-page-sub" style="margin-bottom:20px;">
            El nombre de usuario y el correo deben ser únicos. El rol no se puede cambiar desde aquí.
        </p>

        <%= Ui.CsrfField(CsrfToken) %>

        <div class="pm-form-grid">
            <div class="pm-field">
                <label for="username">Nombre de usuario *</label>
                <input class="pm-input" type="text" id="username" name="username" maxlength="50"
                       value="<%= Ui.A(Username) %>" required="required" />
            </div>
            <div class="pm-field">
                <label for="email">Correo electrónico *</label>
                <input class="pm-input" type="email" id="email" name="email" maxlength="50"
                       value="<%= Ui.A(Email) %>" required="required" />
            </div>
            <div class="pm-field">
                <label for="birthday">Fecha de nacimiento</label>
                <input class="pm-input" type="date" id="birthday" name="birthday"
                       value="<%= Ui.A(BirthdayInput) %>" />
            </div>
            <div class="pm-field">
                <label for="photo">Foto de perfil</label>
                <input class="pm-input" type="file" id="photo" name="photo" accept=".jpg,.jpeg,.png,.gif,.webp" />
                <span class="pm-hint">JPG, PNG, GIF o WEBP. Máximo 5 MB.</span>
            </div>
        </div>

        <div class="pm-form-actions">
            <button type="submit" class="pm-btn" name="action" value="profile">Guardar datos</button>
        </div>
    </div>

    <div class="pm-panel">
        <h2>Cambiar contraseña</h2>
        <p class="pm-page-sub" style="margin-bottom:20px;">
            La contraseña se almacena cifrada con PBKDF2 y salt aleatorio.
        </p>

        <div class="pm-form-grid">
            <div class="pm-field">
                <label for="current">Contraseña actual *</label>
                <input class="pm-input" type="password" id="current" name="current"
                       autocomplete="current-password" required="required" />
            </div>
            <div class="pm-field">
                <label for="newpass">Nueva contraseña *</label>
                <input class="pm-input" type="password" id="newpass" name="newpass"
                       autocomplete="new-password" required="required" />
            </div>
            <div class="pm-field">
                <label for="confirmpass">Confirmar nueva contraseña *</label>
                <input class="pm-input" type="password" id="confirmpass" name="confirmpass"
                       autocomplete="new-password" required="required" />
            </div>
        </div>

        <div class="pm-form-actions">
            <button type="submit" class="pm-btn" name="action" value="password"
                    data-pm-confirm="¿Cambiar su contraseña ahora?">Cambiar contraseña</button>
        </div>
    </div>

</asp:Content>
