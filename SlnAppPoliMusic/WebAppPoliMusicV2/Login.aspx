<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebAppPoliMusicV2.Login" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="theme-color" content="#0E0409" />
    <title>Iniciar sesi&oacute;n &middot; PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
<form id="formLogin" runat="server">
    <div class="pm-auth">

        <aside class="pm-auth-aside">
            <a class="pm-brand" href="<%= ResolveUrl("~/Login.aspx") %>">
                <span class="pm-brand-mark">
                    <svg viewBox="0 0 24 24" fill="none" stroke="#FDF4F6" stroke-width="2" stroke-linecap="round" aria-hidden="true">
                        <path d="M9 18V5l10-2v13" />
                        <circle cx="6" cy="18" r="3" fill="#FDF4F6" stroke="none" />
                        <circle cx="16" cy="16" r="3" fill="#FDF4F6" stroke="none" />
                    </svg>
                </span>
                <span class="pm-brand-text">
                    <span class="pm-brand-name">PoliMusic</span>
                    <span class="pm-brand-sub">Streaming</span>
                </span>
            </a>

            <div class="pm-auth-quote">
                <h2>Tu m&uacute;sica, tu cat&aacute;logo y tus playlists en un solo lugar.</h2>
                <p>Gestiona canciones, artistas y &aacute;lbumes con un cat&aacute;logo propio, y organiza lo que escuchas en playlists p&uacute;blicas o privadas.</p>

                <div class="pm-auth-points">
                    <div class="pm-auth-point">
                        <span class="pm-dot">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
                        </span>
                        Cat&aacute;logo de canciones, artistas y &aacute;lbumes
                    </div>
                    <div class="pm-auth-point">
                        <span class="pm-dot">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
                        </span>
                        Reproductor persistente entre p&aacute;ginas
                    </div>
                    <div class="pm-auth-point">
                        <span class="pm-dot">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
                        </span>
                        Playlists propias con control de privacidad
                    </div>
                </div>
            </div>

            <p style="position:relative;z-index:1;margin:0;font-size:0.76rem;color:rgba(247,235,238,0.55);">
                Escuela Polit&eacute;cnica Nacional &middot; Aplicaciones Web Avanzadas
            </p>
        </aside>

        <main class="pm-auth-main">
            <div class="pm-auth-card">
                <div class="pm-auth-brand">
                    <span class="pm-brand-mark" style="width:44px;height:44px;flex:0 0 44px;">
                        <svg viewBox="0 0 24 24" fill="none" stroke="#FDF4F6" stroke-width="2" stroke-linecap="round" aria-hidden="true">
                            <path d="M9 18V5l10-2v13" />
                            <circle cx="6" cy="18" r="3" fill="#FDF4F6" stroke="none" />
                            <circle cx="16" cy="16" r="3" fill="#FDF4F6" stroke="none" />
                        </svg>
                    </span>
                    <div>
                        <h1>PoliMusic</h1>
                        <p>Acceso al cat&aacute;logo</p>
                    </div>
                </div>

                <h2 class="pm-auth-title">Iniciar sesi&oacute;n</h2>
                <p class="pm-auth-sub">Ingrese con su usuario o correo registrado.</p>

                <%= Ui.Alert(FlashMessage, false) %>
                <%= Ui.Alert(ErrorMessage, true) %>

                <div class="pm-auth-form">
                    <%= Ui.CsrfField(CsrfToken) %>

                    <div class="pm-field">
                        <label for="username">Usuario o correo</label>
                        <input class="pm-input" type="text" id="username" name="username"
                               value="<%= Ui.A(LoginValue) %>" placeholder="usuario o correo@epn.edu.ec"
                               autocomplete="username" required="required" autofocus="autofocus" />
                    </div>

                    <div class="pm-field">
                        <label for="password">Contrase&ntilde;a</label>
                        <input class="pm-input" type="password" id="password" name="password"
                               placeholder="Su contrase&ntilde;a" autocomplete="current-password" required="required" />
                    </div>

                    <button type="submit" class="pm-btn pm-btn-block">Entrar</button>
                </div>

                <div class="pm-auth-footer">
                    &iquest;No tiene cuenta? <a href="<%= ResolveUrl("~/SignUp.aspx") %>">Crear una cuenta</a>
                </div>

                <div class="pm-demo">
                    <strong>Cuentas de prueba (BDD_PoliMusic.sql)</strong><br />
                    Administrador: <strong>userAdmin</strong> / 12345678<br />
                    Usuario: <strong>userNormal</strong> / 12345678<br />
                    Usuario del enunciado: <strong>epn.dis</strong> / PoliMusic2026
                </div>
            </div>
        </main>
    </div>
</form>
</body>
</html>
