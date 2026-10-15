<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Perfil.aspx.cs" Inherits="WebAppPoliMusicV1.Perfil" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Mi perfil · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Mi perfil</h1>
            <p class="pm-page-sub">Consulte y actualice sus datos de cuenta.</p>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <div class="pm-detail-head" style="align-items:center;">
        <div class="pm-detail-art round" style="width:130px;height:130px;flex:0 0 130px;">
            <% if (PhotoUrl != null) { %>
            <img src="<%= Url(PhotoUrl) %>" alt="Foto de perfil" />
            <% } else { %>
            <span class="pm-art-initials" style="font-size:2.2rem;"><%= H(Initials) %></span>
            <% } %>
        </div>
        <div class="pm-detail-meta">
            <div class="pm-kicker"><%= H(RoleName) %></div>
            <h1><%= H(User.Username) %></h1>
            <div class="pm-detail-sub">
                <%= H(User.Email) %>
                <% if (User.Birthday.HasValue) { %>
                · Nacimiento: <%= User.Birthday.Value.ToString("dd/MM/yyyy") %>
                <% } %>
            </div>
            <div class="pm-detail-actions">
                <span class="pm-badge <%= User.IsAdmin ? "pm-badge-admin" : "pm-badge-user" %>">
                    <%= H(RoleName) %>
                </span>
                <span class="pm-badge pm-badge-muted"><%= PlaylistCount %> playlist(s)</span>
            </div>
        </div>
    </div>

    <div class="pm-panel">
        <h2>Datos de la cuenta</h2>
        <div class="pm-form">
            <div class="pm-field">
                <label for="<%= txtUsername.ClientID %>">Nombre de usuario</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtEmail.ClientID %>">Correo electrónico</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="pm-input" TextMode="Email" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtBirthday.ClientID %>">Fecha de nacimiento</label>
                <asp:TextBox ID="txtBirthday" runat="server" CssClass="pm-input" TextMode="Date" />
            </div>

            <% if (CanChoosePhoto) { %>
            <div class="pm-field">
                <label for="<%= ddlPhoto.ClientID %>">Foto de perfil</label>
                <asp:DropDownList ID="ddlPhoto" runat="server" CssClass="pm-select" />
            </div>
            <% } %>

            <div class="pm-note" style="margin-bottom:18px;">
                Su rol (<strong><%= H(RoleName) %></strong>) solo puede modificarlo un administrador.
                El formulario no permite cambiarlo.
            </div>

            <div class="pm-form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Guardar cambios"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>

    <div class="pm-panel">
        <h2>Cambiar contraseña</h2>
        <div class="pm-form">
            <div class="pm-field">
                <label for="<%= txtCurrent.ClientID %>">Contraseña actual</label>
                <asp:TextBox ID="txtCurrent" runat="server" CssClass="pm-input" TextMode="Password"
                             MaxLength="128" autocomplete="current-password" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNew.ClientID %>">Nueva contraseña</label>
                <asp:TextBox ID="txtNew" runat="server" CssClass="pm-input" TextMode="Password"
                             MaxLength="128" autocomplete="new-password" />
                <span class="pm-hint">Mínimo 8 caracteres. Se guarda cifrada con PBKDF2 y salt.</span>
            </div>
            <div class="pm-field">
                <label for="<%= txtConfirm.ClientID %>">Confirmar nueva contraseña</label>
                <asp:TextBox ID="txtConfirm" runat="server" CssClass="pm-input" TextMode="Password"
                             MaxLength="128" autocomplete="new-password" />
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnChangePassword" runat="server" Text="Actualizar contraseña"
                            CssClass="pm-btn" OnClick="btnChangePassword_Click" />
            </div>
        </div>
    </div>

</asp:Content>
