<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SignUp.aspx.cs" Inherits="WebAppPoliMusicV2.SignUp" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="theme-color" content="#0E0409" />
    <title>Crear cuenta &middot; PoliMusic</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/Site.css") %>" />
</head>
<body>
<form id="formSignUp" runat="server" enctype="multipart/form-data">
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
                <h2>Cree su cuenta y empiece a armar sus playlists.</h2>
                <p>Su cuenta se registra con rol de usuario. El rol de administrador solo puede asignarlo un administrador existente desde el panel.</p>

                <div class="pm-auth-points">
                    <div class="pm-auth-point">
                        <span class="pm-dot">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
                        </span>
                        Contrase&ntilde;a cifrada con PBKDF2 y salt propio
                    </div>
                    <div class="pm-auth-point">
                        <span class="pm-dot">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
                        </span>
                        Playlists p&uacute;blicas o privadas
                    </div>
                    <div class="pm-auth-point">
                        <span class="pm-dot">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
                        </span>
                        Foto de perfil opcional
                    </div>
                </div>
            </div>

            <p style="position:relative;z-index:1;margin:0;font-size:0.76rem;color:rgba(247,235,238,0.55);">
                Escuela Polit&eacute;cnica Nacional &middot; Aplicaciones Web Avanzadas
            </p>
        </aside>

        <main class="pm-auth-main">
            <div class="pm-auth-card" style="max-width:480px;">
                <h2 class="pm-auth-title">Crear cuenta</h2>
                <p class="pm-auth-sub">Los campos marcados con * son obligatorios.</p>

                <%= Ui.Alert(ErrorMessage, true) %>

                <%= Ui.CsrfField(CsrfToken) %>

                <div class="pm-field">
                    <label for="username">Nombre de usuario *</label>
                    <input class="pm-input" type="text" id="username" name="username" maxlength="50"
                           value="<%= Ui.A(UserNameValue) %>" placeholder="Entre 4 y 50 caracteres"
                           autocomplete="username" required="required" />
                </div>

                <div class="pm-field">
                    <label for="email">Correo electr&oacute;nico *</label>
                    <input class="pm-input" type="email" id="email" name="email" maxlength="50"
                           value="<%= Ui.A(EmailValue) %>" placeholder="usuario@epn.edu.ec"
                           autocomplete="email" required="required" />
                </div>

                <div class="pm-form-grid">
                    <div class="pm-field">
                        <label for="password">Contrase&ntilde;a *</label>
                        <input class="pm-input" type="password" id="password" name="password"
                               placeholder="M&iacute;nimo 8 caracteres" autocomplete="new-password" required="required" />
                    </div>
                    <div class="pm-field">
                        <label for="confirm">Confirmar contrase&ntilde;a *</label>
                        <input class="pm-input" type="password" id="confirm" name="confirm"
                               placeholder="Repita la contrase&ntilde;a" autocomplete="new-password" required="required" />
                    </div>
                </div>

                <div class="pm-form-grid">
                    <div class="pm-field">
                        <label for="birthday">Fecha de nacimiento</label>
                        <input class="pm-input" type="date" id="birthday" name="birthday"
                               value="<%= Ui.A(BirthdayValue) %>" />
                    </div>
                    <div class="pm-field">
                        <label for="photo">Foto de perfil</label>
                        <input class="pm-input" type="file" id="photo" name="photo" accept=".jpg,.jpeg,.png,.gif,.webp" />
                        <span class="pm-hint">JPG, PNG, GIF o WEBP. M&aacute;ximo 5 MB.</span>
                    </div>
                </div>

                <button type="submit" class="pm-btn pm-btn-block">Registrarme</button>

                <div class="pm-auth-footer">
                    &iquest;Ya tiene cuenta? <a href="<%= ResolveUrl("~/Login.aspx") %>">Iniciar sesi&oacute;n</a>
                </div>
            </div>
        </main>
    </div>
</form>
</body>
</html>
