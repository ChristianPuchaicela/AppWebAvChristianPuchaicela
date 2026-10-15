<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Registro.aspx.cs" Inherits="WebAppPoliMusicV1.Registro" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Crear cuenta · PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
<form id="regForm" runat="server">
    <div class="pm-auth" style="padding:32px 18px;">
        <div class="pm-auth-card" style="max-width:470px;">
            <div class="pm-auth-brand">
                <span class="pm-brand-mark">
                    <svg viewBox="0 0 24 24" fill="none" stroke="#F5EBDD" stroke-width="2" stroke-linecap="round">
                        <path d="M9 18V5l10-2v13" />
                        <circle cx="6" cy="18" r="3" fill="#F5EBDD" stroke="none" />
                        <circle cx="16" cy="16" r="3" fill="#F5EBDD" stroke="none" />
                    </svg>
                </span>
                <h1>Crear cuenta</h1>
                <p>Únase a PoliMusic como oyente</p>
            </div>

            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
                <asp:Label ID="lblMessage" runat="server" />
            </asp:Panel>

            <div class="pm-field">
                <label for="<%= txtUsername.ClientID %>">Nombre de usuario <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="pm-input" MaxLength="50"
                             placeholder="mínimo 4 caracteres" autocomplete="username" />
            </div>

            <div class="pm-field">
                <label for="<%= txtEmail.ClientID %>">Correo electrónico <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="pm-input" TextMode="Email" MaxLength="50"
                             placeholder="usuario@dominio.com" autocomplete="email" />
            </div>

            <div class="pm-field">
                <label for="<%= txtBirthday.ClientID %>">Fecha de nacimiento</label>
                <asp:TextBox ID="txtBirthday" runat="server" CssClass="pm-input" TextMode="Date" />
                <span class="pm-hint">Opcional. Se guarda en TBL_USER.BIRTHDAY.</span>
            </div>

            <div class="pm-field">
                <label for="<%= txtPassword.ClientID %>">Contraseña <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="pm-input" TextMode="Password"
                             MaxLength="128" placeholder="mínimo 8 caracteres" autocomplete="new-password" />
            </div>

            <div class="pm-field">
                <label for="<%= txtConfirm.ClientID %>">Confirmar contraseña <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtConfirm" runat="server" CssClass="pm-input" TextMode="Password"
                             MaxLength="128" placeholder="repita la contraseña" autocomplete="new-password" />
            </div>

            <div class="alert alert-info" style="font-size:0.82rem;">
                Las cuentas nuevas se crean siempre con el rol <strong>Usuario</strong>.
                El rol de administrador solo lo asigna un administrador existente.
            </div>

            <asp:Button ID="btnRegister" runat="server" Text="Crear mi cuenta"
                        CssClass="pm-btn pm-btn-accent pm-btn-block" OnClick="btnRegister_Click" />

            <div class="pm-auth-footer">
                ¿Ya tiene una cuenta?
                <a href="<%= ResolveUrl("~/Login.aspx") %>">Inicie sesión</a>
            </div>
        </div>
    </div>
</form>
</body>
</html>
