<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Playlists.aspx.cs" Inherits="WebAppPoliMusicV1.Playlists" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Playlists · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Mis playlists</h1>
            <p class="pm-page-sub">Cree, edite y organice sus colecciones de canciones.</p>
        </div>
        <button type="button" class="pm-btn pm-btn-accent" data-pm-modal-open="modalNew">Nueva playlist</button>
    </div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-error" role="alert">
        <asp:Label ID="lblError" runat="server" />
    </asp:Panel>

    <!-- Mis playlists -->
    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Propias (<%= MyPlaylists.Count %>)</h2>
        </div>
        <% if (MyPlaylists.Count == 0) { %>
            <%: new HtmlString(Ui.EmptyState("Todavía no tiene playlists",
                "Use el botón «Nueva playlist» para crear su primera colección.")) %>
        <% } else { %>
        <div class="pm-grid">
            <% foreach (var card in MyCards) { %>
                <%: new HtmlString(card) %>
            <% } %>
        </div>
        <% } %>
    </section>

    <!-- Playlists publicas de otros usuarios -->
    <section class="pm-section">
        <div class="pm-section-head">
            <h2>Públicas de la comunidad</h2>
        </div>
        <% if (PublicCards.Count == 0) { %>
            <div class="pm-empty-sm">No hay playlists públicas de otros usuarios por ahora.</div>
        <% } else { %>
        <div class="pm-grid">
            <% foreach (var card in PublicCards) { %>
                <%: new HtmlString(card) %>
            <% } %>
        </div>
        <% } %>
    </section>

    <!-- Modal: nueva playlist -->
    <div class="pm-modal-backdrop" id="modalNew">
        <div class="pm-modal">
            <h2>Nueva playlist</h2>
            <div class="pm-field">
                <label for="<%= txtNewTitle.ClientID %>">Título <span style="color:#D98C74">*</span></label>
                <asp:TextBox ID="txtNewTitle" runat="server" CssClass="pm-input" MaxLength="50"
                             placeholder="Mi playlist" />
            </div>
            <div class="pm-field">
                <label for="<%= ddlNewType.ClientID %>">Visibilidad</label>
                <asp:DropDownList ID="ddlNewType" runat="server" CssClass="pm-select">
                    <asp:ListItem Value="0" Text="Pública — visible para todos" />
                    <asp:ListItem Value="1" Text="Privada — solo para mí" />
                </asp:DropDownList>
            </div>
            <div class="pm-field">
                <label for="<%= ddlNewPhoto.ClientID %>">Portada</label>
                <asp:DropDownList ID="ddlNewPhoto" runat="server" CssClass="pm-select" />
                <span class="pm-hint">Imágenes disponibles en Assets/PlaylistImages.</span>
            </div>
            <div class="pm-form-actions">
                <asp:Button ID="btnCreate" runat="server" Text="Crear playlist"
                            CssClass="pm-btn pm-btn-accent" OnClick="btnCreate_Click" />
                <button type="button" class="pm-btn pm-btn-ghost" data-pm-modal-close="1">Cancelar</button>
            </div>
        </div>
    </div>

</asp:Content>
