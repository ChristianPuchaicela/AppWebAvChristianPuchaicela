<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Canciones.aspx.cs" Inherits="WebAppPoliMusicV1.Canciones" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Canciones · PoliMusic</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="pm-page-head">
        <div>
            <h1>Song List</h1>
            <p class="pm-page-sub">
                Listado de canciones disponibles en PoliMusic.
                <% if (!string.IsNullOrEmpty(SearchTerm)) { %>
                Resultados para «<%= H(SearchTerm) %>».
                <% } %>
            </p>
        </div>
        <% if (IsAdmin) { %>
        <a class="pm-btn" href="<%= ResolveUrl("~/Admin/Canciones.aspx") %>">Administrar canciones</a>
        <% } %>
    </div>

    <% if (!string.IsNullOrEmpty(AudioWarning)) { %>
    <div class="alert alert-warning"><%= H(AudioWarning) %></div>
    <% } %>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
        <asp:Label ID="lblMessage" runat="server" />
    </asp:Panel>

    <% if (Songs.Count == 0) { %>
        <%: new HtmlString(Ui.EmptyState("No se encontraron canciones",
            string.IsNullOrEmpty(SearchTerm)
              ? "La base de datos todavía no tiene canciones registradas."
              : "Pruebe con otro término de búsqueda.")) %>
    <% } else { %>
    <div class="pm-table-wrap">
        <table class="pm-table">
            <thead>
                <tr>
                    <th style="width:64px;">#</th>
                    <th>Canción</th>
                    <th>Álbum</th>
                    <th>Artista</th>
                    <th style="width:110px;">Reproducciones</th>
                    <th style="width:170px;">Acciones</th>
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

</asp:Content>
