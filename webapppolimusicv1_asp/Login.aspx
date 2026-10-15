<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="WebAppPoliMusicV1.Login" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Iniciar sesión · PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
<form id="loginForm" runat="server">
    <div class="pm-auth">
        <div class="pm-auth-card">
            <div class="pm-auth-brand">
                <span class="pm-brand-mark">
                    <svg viewBox="0 0 24 24" fill="none" stroke="#F3E4E7" stroke-width="2" stroke-linecap="round">
                        <path d="M9 18V5l10-2v13" />
                        <circle cx="6" cy="18" r="3" fill="#F3E4E7" stroke="none" />
                        <circle cx="16" cy="16" r="3" fill="#F3E4E7" stroke="none" />
                    </svg>
                </span>
                <h1>PoliMusic</h1>
                <p>Tu música, con identidad propia</p>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
                <asp:Label ID="lblMessage" runat="server" />
            </asp:Panel>

            <div class="pm-field">
                <label for="<%= txtLogin.ClientID %>">Usuario o correo electrónico</label>
                <asp:TextBox ID="txtLogin" runat="server" CssClass="pm-input" MaxLength="50"
                             placeholder="usuario o correo" autocomplete="username" />
            </div>

            <div class="pm-field">
                <label for="<%= txtPassword.ClientID %>">Contraseña</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="pm-input" TextMode="Password"
                             MaxLength="128" placeholder="••••••••" autocomplete="current-password" />
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="Iniciar sesión"
                        CssClass="pm-btn pm-btn-accent pm-btn-block" OnClick="btnLogin_Click" />

            <% if (!string.IsNullOrEmpty(InfoMessage)) { %>
            <div class="alert alert-info" style="margin-top:18px;">
                <%= H(InfoMessage) %>
            </div>
            <% } %>

            <div class="pm-auth-footer">
                ¿No tiene una cuenta?
                <a href="<%= ResolveUrl("~/Registro.aspx") %>">Regístrese aquí</a>
            </div>
        </div>
    </div>
</form>
</body>
</html>
