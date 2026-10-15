<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Artistas.aspx.cs" Inherits="WebAppPoliMusicV1.Admin.AdminArtistas" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Administración de artistas · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Artistas</h1>
            <p class="pm-page-sub">Intérpretes del catálogo. Total: <%= Artists.Count %>.</p>
        </div>
        <div style="display:flex;gap:9px;flex-wrap:wrap;">
            <button type="button" class="pm-btn pm-btn-accent" data-pm-modal-open="modalNew">Nuevo artista</button>
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Default.aspx") %>">← Panel</a>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <% if (Artists.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("Sin artistas", "No hay artistas registrados en TBL_ARTIST.")) %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th style="width:56px;">ID</th>
                    <th>Artista</th>
                    <th>Imagen</th>
                    <th style="width:110px;">Canciones</th>
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

    <div class="pm-modal-backdrop" id="modalNew">
        <div class="pm-modal">
            <h2>Nuevo artista</h2>
            <div class="pm-field">
                <label for="<%= txtNewName.ClientID %>">Nombre <span style="color:#CB8496">*</span></label>
                <asp:TextBox ID="txtNewName" runat="server" CssClass="pm-input" MaxLength="100" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewLastname.ClientID %>">Apellido</label>
                <asp:TextBox ID="txtNewLastname" runat="server" CssClass="pm-input" MaxLength="100" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewImage.ClientID %>">Imagen</label>
                <asp:TextBox ID="txtNewImage" runat="server" CssClass="pm-input" MaxLength="255"
                             placeholder="~/ArtistImages/artist1.jpg" />
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnCreate" runat="server" Text="Crear artista"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnCreate_Click" />
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

    <% if (EditArtist != null) { %>
    <div class="pm-modal-backdrop open" id="modalEdit">
        <div class="pm-modal">
            <h2>Editar artista #<%= EditArtist.IdArtist %></h2>
            <div class="pm-field">
                <label for="<%= txtName.ClientID %>">Nombre <span style="color:#CB8496">*</span></label>
                <asp:TextBox ID="txtName" runat="server" CssClass="pm-input" MaxLength="100" />
            </div>
            <div class="pm-field">
                <label for="<%= txtLastname.ClientID %>">Apellido</label>
                <asp:TextBox ID="txtLastname" runat="server" CssClass="pm-input" MaxLength="100" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlImage.ClientID %>">Imagen</label>
                <asp:DropDownList ID="ddlImage" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-note" style="margin-bottom:16px;">
                Las canciones del artista se administran desde el módulo de canciones,
                donde se asigna la relación en TBL_PLAYER.
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Guardar cambios"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnSave_Click" />
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Artistas.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>
    <% } %>

</asp:Content>
