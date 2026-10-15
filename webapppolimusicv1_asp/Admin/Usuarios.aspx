<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Usuarios.aspx.cs" Inherits="WebAppPoliMusicV1.Admin.Usuarios" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Administración de usuarios · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Usuarios</h1>
            <p class="pm-page-sub">Alta, edición y baja de cuentas. Total: <%= Users.Count %>.</p>
        </div>
        <div style="display:flex;gap:9px;flex-wrap:wrap;">
            <button type="button" class="pm-btn pm-btn-accent" data-pm-modal-open="modalNew">Nuevo usuario</button>
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Default.aspx") %>">← Panel</a>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <% if (Users.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("Sin usuarios", "No hay cuentas registradas.")) %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th style="width:56px;">ID</th>
                    <th>Usuario</th>
                    <th>Correo</th>
                    <th style="width:140px;">Rol</th>
                    <th style="width:130px;">Nacimiento</th>
                    <th style="width:110px;">Playlists</th>
                    <th style="width:190px;">Acciones</th>
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

    <!-- Modal: nuevo usuario -->
    <div class="pm-modal-backdrop" id="modalNew">
        <div class="pm-modal">
            <h2>Nuevo usuario</h2>
            <div class="pm-field">
                <label for="<%= txtNewUsername.ClientID %>">Usuario <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtNewUsername" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewEmail.ClientID %>">Correo <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtNewEmail" runat="server" CssClass="pm-input" TextMode="Email" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewPassword.ClientID %>">Contraseña <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="pm-input" TextMode="Password" MaxLength="128" />
                <span class="pm-hint">Mínimo 8 caracteres. Se guarda cifrada.</span>
            </div>
            <div class="pm-field">
                <label for="<%= txtNewBirthday.ClientID %>">Fecha de nacimiento</label>
                <asp:TextBox ID="txtNewBirthday" runat="server" CssClass="pm-input" TextMode="Date" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlNewRole.ClientID %>">Rol</label>
                <asp:DropDownList ID="ddlNewRole" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnCreate" runat="server" Text="Crear usuario"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnCreate_Click" />
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

    <!-- Modal: editar usuario -->
    <% if (EditUser != null) { %>
    <div class="pm-modal-backdrop open" id="modalEdit">
        <div class="pm-modal">
            <h2>Editar usuario #<%= EditUser.IdUser %></h2>
            <div class="pm-field">
                <label for="<%= txtUsername.ClientID %>">Usuario <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtEmail.ClientID %>">Correo <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="pm-input" TextMode="Email" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtBirthday.ClientID %>">Fecha de nacimiento</label>
                <asp:TextBox ID="txtBirthday" runat="server" CssClass="pm-input" TextMode="Date" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlRole.ClientID %>">Rol</label>
                <asp:DropDownList ID="ddlRole" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlPhoto.ClientID %>">Foto de perfil</label>
                <asp:DropDownList ID="ddlPhoto" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-note" style="margin-bottom:16px;">
                La contraseña actual no se muestra ni se puede leer. Para cambiarla, use el
                restablecimiento de abajo.
            </div>
            <div class="pm-field">
                <label for="<%= txtResetPassword.ClientID %>">Nueva contraseña (opcional)</label>
                <asp:TextBox ID="txtResetPassword" runat="server" CssClass="pm-input" TextMode="Password" MaxLength="128" />
                <span class="pm-hint">Déjelo vacío para conservar la contraseña actual.</span>
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Guardar cambios"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnSave_Click" />
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Usuarios.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>
    <% } %>

</asp:Content>
