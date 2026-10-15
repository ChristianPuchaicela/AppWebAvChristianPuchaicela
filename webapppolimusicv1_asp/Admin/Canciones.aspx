<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Canciones.aspx.cs" Inherits="WebAppPoliMusicV1.Admin.AdminCanciones" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Administración de canciones · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Canciones</h1>
            <p class="pm-page-sub">Catálogo completo. Total: <%= Songs.Count %>.</p>
        </div>
        <div style="display:flex;gap:9px;flex-wrap:wrap;">
            <button type="button" class="pm-btn pm-btn-accent" data-pm-modal-open="modalNew">Nueva canción</button>
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Default.aspx") %>">← Panel</a>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <% if (Songs.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("Sin canciones", "No hay canciones registradas en TBL_SONG.")) %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th style="width:56px;">ID</th>
                    <th>Canción</th>
                    <th>Ruta (SONG_PATH)</th>
                    <th style="width:100px;">Audio</th>
                    <th style="width:90px;">Plays</th>
                    <th style="width:210px;">Acciones</th>
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

    <!-- Modal: nueva canción -->
    <div class="pm-modal-backdrop" id="modalNew">
        <div class="pm-modal">
            <h2>Nueva canción</h2>
            <div class="pm-field">
                <label for="<%= txtNewName.ClientID %>">Nombre <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtNewName" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewPath.ClientID %>">Ruta del archivo <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtNewPath" runat="server" CssClass="pm-input" MaxLength="255"
                             placeholder="~/songFiles/bensound-adventure.mp3" />
                <span class="pm-hint">
                    Use el formato de la base de datos: ~/songFiles/nombre.mp3.
                    El archivo debe existir en Assets/SongFiles.
                </span>
            </div>
            <div class="pm-field">
                <label for="<%= txtNewPlays.ClientID %>">Reproducciones iniciales</label>
                <asp:TextBox ID="txtNewPlays" runat="server" CssClass="pm-input" TextMode="Number" Text="0" />
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnCreate" runat="server" Text="Crear canción"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnCreate_Click" />
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

    <!-- Modal: editar canción -->
    <% if (EditSong != null) { %>
    <div class="pm-modal-backdrop open" id="modalEdit">
        <div class="pm-modal">
            <h2>Editar canción #<%= EditSong.IdSong %></h2>
            <div class="pm-field">
                <label for="<%= txtName.ClientID %>">Nombre <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtName" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtPath.ClientID %>">Ruta del archivo <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtPath" runat="server" CssClass="pm-input" MaxLength="255" />
            </div>
            <div class="pm-field">
                <label for="<%= txtPlays.ClientID %>">Reproducciones</label>
                <asp:TextBox ID="txtPlays" runat="server" CssClass="pm-input" TextMode="Number" />
            </div>

            <div class="pm-divider"></div>
            <h3>Relaciones</h3>

            <div class="pm-field">
                <label for="<%= ddlAlbum.ClientID %>">Álbum (TBL_TRACKLIST)</label>
                <asp:DropDownList ID="ddlAlbum" runat="server" CssClass="pm-select" />
                <span class="pm-hint">Seleccionar «Sin álbum» quita la relación actual.</span>
            </div>
            <div class="pm-field">
                <label for="<%= ddlArtist.ClientID %>">Artista (TBL_PLAYER)</label>
                <asp:DropDownList ID="ddlArtist" runat="server" CssClass="pm-select" />
                <span class="pm-hint">Seleccionar «Sin artista» quita la relación actual.</span>
            </div>

            <div class="pm-form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Guardar cambios"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnSave_Click" />
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Canciones.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>
    <% } %>

</asp:Content>
