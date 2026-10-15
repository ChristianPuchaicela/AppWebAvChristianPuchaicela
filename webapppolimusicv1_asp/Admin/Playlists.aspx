<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Playlists.aspx.cs" Inherits="WebAppPoliMusicV1.Admin.AdminPlaylists" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Administración de playlists · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Playlists</h1>
            <p class="pm-page-sub">
                Todas las playlists del sistema. Total: <%= Playlists.Count %>.
                Como administrador puede editarlas y eliminarlas sin restricción de propiedad.
            </p>
        </div>
        <div style="display:flex;gap:9px;flex-wrap:wrap;">
            <button type="button" class="pm-btn pm-btn-accent" data-pm-modal-open="modalNew">Nueva playlist</button>
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Default.aspx") %>">← Panel</a>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <% if (Playlists.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("Sin playlists", "No hay playlists registradas en TBL_PLAYLIST.")) %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th style="width:56px;">ID</th>
                    <th>Título</th>
                    <th>Propietario</th>
                    <th style="width:120px;">Visibilidad</th>
                    <th style="width:100px;">Canciones</th>
                    <th style="width:120px;">Creada</th>
                    <th style="width:110px;">Acciones</th>
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

    <!-- Modal: nueva playlist -->
    <div class="pm-modal-backdrop" id="modalNew">
        <div class="pm-modal">
            <h2>Nueva playlist</h2>
            <div class="pm-field">
                <label for="<%= ddlNewOwner.ClientID %>">Propietario <span style="color:#CB8496">*</span></label>
                <asp:DropDownList ID="ddlNewOwner" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewTitle.ClientID %>">Título <span style="color:#CB8496">*</span></label>
                <asp:TextBox ID="txtNewTitle" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlNewType.ClientID %>">Visibilidad</label>
                <asp:DropDownList ID="ddlNewType" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlNewPhoto.ClientID %>">Portada</label>
                <asp:DropDownList ID="ddlNewPhoto" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnCreate" runat="server" Text="Crear playlist"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnCreate_Click" />
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

    <!-- Modal: editar playlist -->
    <% if (EditPlaylist != null) { %>
    <div class="pm-modal-backdrop open" id="modalEdit">
        <div class="pm-modal">
            <h2>Editar playlist #<%= EditPlaylist.IdPlaylist %></h2>
            <div class="pm-field">
                <label for="<%= ddlOwner.ClientID %>">Propietario <span style="color:#CB8496">*</span></label>
                <asp:DropDownList ID="ddlOwner" runat="server" CssClass="pm-select" />
                <span class="pm-hint">Cambiar el propietario transfiere la playlist a otro usuario.</span>
            </div>
            <div class="pm-field">
                <label for="<%= txtTitle.ClientID %>">Título <span style="color:#CB8496">*</span></label>
                <asp:TextBox ID="txtTitle" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlType.ClientID %>">Visibilidad</label>
                <asp:DropDownList ID="ddlType" runat="server" CssClass="pm-select" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlPhoto.ClientID %>">Portada</label>
                <asp:DropDownList ID="ddlPhoto" runat="server" CssClass="pm-select" />
            </div>

            <div class="pm-divider"></div>
            <h3>Canciones (<%= EditSongs.Count %>)</h3>
            <% if (EditSongs.Count == 0) { %>
                <div class="pm-empty-sm">Esta playlist no tiene canciones.</div>
            <% } else { %>
                <ul style="margin:0 0 18px;padding-left:20px;color:#D3AFB8;font-size:0.87rem;">
                    <% foreach (var s in EditSongs) { %>
                    <li><%= H(s.SongName) %></li>
                    <% } %>
                </ul>
            <% } %>

            <div class="pm-form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Guardar cambios"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnSave_Click" />
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Playlists.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>
    <% } %>

</asp:Content>
