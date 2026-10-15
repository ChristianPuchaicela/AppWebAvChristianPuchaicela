<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Albumes.aspx.cs" Inherits="WebAppPoliMusicV1.Admin.AdminAlbumes" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Administración de álbumes · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Álbumes</h1>
            <p class="pm-page-sub">Discografía registrada. Total: <%= Albums.Count %>.</p>
        </div>
        <div style="display:flex;gap:9px;flex-wrap:wrap;">
            <button type="button" class="pm-btn pm-btn-accent" data-pm-modal-open="modalNew">Nuevo álbum</button>
            <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Default.aspx") %>">← Panel</a>
        </div>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <% if (Albums.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("Sin álbumes", "No hay álbumes registrados en TBL_ALBUM.")) %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th style="width:56px;">ID</th>
                    <th>Título</th>
                    <th style="width:100px;">Año</th>
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
            <h2>Nuevo álbum</h2>
            <div class="pm-field">
                <label for="<%= txtNewTitle.ClientID %>">Título <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtNewTitle" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewYear.ClientID %>">Año de lanzamiento</label>
                <asp:TextBox ID="txtNewYear" runat="server" CssClass="pm-input" TextMode="Number" Min="1900" Max="2100" />
            </div>
            <div class="pm-field">
                <label for="<%= txtNewImage.ClientID %>">Imagen</label>
                <asp:TextBox ID="txtNewImage" runat="server" CssClass="pm-input" MaxLength="255"
                             placeholder="~/albumImages/album1.png" />
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnCreate" runat="server" Text="Crear álbum"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnCreate_Click" />
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

    <% if (EditAlbum != null) { %>
    <div class="pm-modal-backdrop open" id="modalEdit">
        <div class="pm-modal">
            <h2>Editar álbum #<%= EditAlbum.IdAlbum %></h2>
            <div class="pm-field">
                <label for="<%= txtTitle.ClientID %>">Título <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtTitle" runat="server" CssClass="pm-input" MaxLength="50" />
            </div>
            <div class="pm-field">
                <label for="<%= txtYear.ClientID %>">Año de lanzamiento</label>
                <asp:TextBox ID="txtYear" runat="server" CssClass="pm-input" TextMode="Number" Min="1900" Max="2100" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlImage.ClientID %>">Imagen</label>
                <asp:DropDownList ID="ddlImage" runat="server" CssClass="pm-select" />
            </div>

            <div class="pm-divider"></div>
            <h3>Canciones del álbum (TBL_TRACKLIST)</h3>
            <% if (AlbumSongs.Count == 0) { %>
                <div class="pm-empty-sm">Este álbum no tiene canciones en su lista.</div>
            <% } else { %>
                <ul style="margin:0 0 18px;padding-left:20px;color:#C9B29A;font-size:0.87rem;">
                    <% foreach (var s in AlbumSongs) { %>
                    <li><%= H(s.SongName) %></li>
                    <% } %>
                </ul>
            <% } %>
            <div class="pm-note" style="margin-bottom:18px;">
                Para agregar o quitar canciones de este álbum, use el módulo de canciones
                y asigne el campo «Álbum».
            </div>

            <div class="pm-form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Guardar cambios"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnSave_Click" />
                <a class="pm-btn pm-btn-ghost" href="<%= ResolveUrl("~/Admin/Albumes.aspx") %>">Cancelar</a>
            </div>
        </div>
    </div>
    <% } %>

</asp:Content>
