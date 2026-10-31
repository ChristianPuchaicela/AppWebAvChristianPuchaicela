<%@ Page Title="Canciones" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Song.aspx.cs" Inherits="WebAppPoliMusicV3.Song" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .pm-hero {
            background: linear-gradient(135deg, #1e1b4b 0%, #312e81 40%, #4338ca 100%);
            color: #ffffff;
            border-radius: 16px;
            padding: 2.2rem 2.5rem;
            margin-top: 1.5rem;
            margin-bottom: 2rem;
            box-shadow: 0 10px 25px -5px rgba(49, 46, 129, 0.35);
            position: relative;
            overflow: hidden;
        }
        .pm-hero::after {
            content: "\266B";
            position: absolute;
            right: 20px;
            bottom: -30px;
            font-size: 13rem;
            color: rgba(255, 255, 255, 0.05);
            pointer-events: none;
            font-family: serif;
        }
        .pm-badge-student {
            background-color: rgba(255, 255, 255, 0.18);
            border: 1px solid rgba(255, 255, 255, 0.3);
            backdrop-filter: blur(8px);
            color: #f8fafc;
            padding: 0.35rem 0.9rem;
            border-radius: 50px;
            font-size: 0.85rem;
            font-weight: 500;
            display: inline-flex;
            align-items: center;
            gap: 0.4rem;
        }
        .pm-stat-box {
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 1.1rem 1.4rem;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }
        .pm-stat-box:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(0, 0, 0, 0.08);
        }
        .pm-table-card {
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 4px 18px rgba(0, 0, 0, 0.05);
            margin-bottom: 5rem;
        }
        .pm-table thead th {
            background: #0f172a;
            color: #f8fafc;
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.78rem;
            letter-spacing: 0.05em;
            padding: 1rem 1.1rem;
            border: none;
        }
        .pm-table tbody tr {
            transition: background-color 0.15s ease;
        }
        .pm-table tbody tr:hover {
            background-color: #f1f5f9;
        }
        .pm-table tbody tr.is-playing {
            background-color: #e0e7ff !important;
        }
        .pm-play-btn {
            background: linear-gradient(135deg, #4f46e5, #6366f1);
            color: #fff;
            border: none;
            width: 38px;
            height: 38px;
            border-radius: 50%;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            box-shadow: 0 4px 10px rgba(79, 70, 229, 0.3);
            transition: all 0.2s ease;
        }
        .pm-play-btn:hover {
            background: linear-gradient(135deg, #4338ca, #4f46e5);
            transform: scale(1.08);
            color: #fff;
        }
        .pm-play-btn.is-active {
            background: #10b981;
            box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
        }
        .pm-audio-inline {
            height: 34px;
            max-width: 260px;
            outline: none;
        }
        .pm-badge-plays {
            background-color: #f1f5f9;
            color: #334155;
            border: 1px solid #cbd5e1;
            padding: 0.25rem 0.65rem;
            border-radius: 20px;
            font-size: 0.85rem;
            font-weight: 600;
        }

        /* Persistent Player Bar */
        .pm-player-bar {
            position: fixed;
            bottom: 0;
            left: 0;
            right: 0;
            background: #0f172a;
            color: #ffffff;
            border-top: 2px solid #6366f1;
            padding: 0.85rem 1.5rem;
            box-shadow: 0 -8px 24px rgba(0, 0, 0, 0.25);
            z-index: 1050;
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1.5rem;
        }
        .pm-player-track {
            display: flex;
            align-items: center;
            gap: 1rem;
            min-width: 240px;
        }
        .pm-player-icon {
            width: 44px;
            height: 44px;
            border-radius: 10px;
            background: linear-gradient(135deg, #6366f1, #a855f7);
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.3rem;
            box-shadow: 0 4px 10px rgba(99, 102, 241, 0.35);
        }
        .pm-player-controls {
            flex: 1;
            max-width: 650px;
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 0.3rem;
        }
        .pm-progress-wrap {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            width: 100%;
        }
        .pm-progress-bar {
            flex: 1;
            height: 6px;
            border-radius: 3px;
            background: #334155;
            cursor: pointer;
            position: relative;
            outline: none;
        }
        .pm-progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #6366f1, #a855f7);
            border-radius: 3px;
            width: 0%;
            transition: width 0.1s linear;
        }
        .pm-control-btns {
            display: flex;
            align-items: center;
            gap: 1.2rem;
        }
        .pm-ctrl-btn {
            background: transparent;
            border: none;
            color: #94a3b8;
            font-size: 1.1rem;
            cursor: pointer;
            transition: color 0.15s ease;
        }
        .pm-ctrl-btn:hover {
            color: #ffffff;
        }
        .pm-ctrl-play {
            background: #6366f1;
            color: #ffffff;
            width: 38px;
            height: 38px;
            border-radius: 50%;
            border: none;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            transition: transform 0.15s ease, background-color 0.15s ease;
        }
        .pm-ctrl-play:hover {
            background: #4f46e5;
            transform: scale(1.08);
            color: #ffffff;
        }
        .pm-time-text {
            font-size: 0.78rem;
            color: #94a3b8;
            min-width: 38px;
            text-align: center;
            font-family: monospace;
        }
        .pm-player-volume {
            display: flex;
            align-items: center;
            gap: 0.6rem;
            min-width: 160px;
            justify-content: flex-end;
        }
        .pm-volume-slider {
            width: 90px;
            accent-color: #6366f1;
            cursor: pointer;
        }
        @media (max-width: 768px) {
            .pm-player-bar {
                flex-direction: column;
                align-items: stretch;
                padding: 0.75rem 1rem;
            }
            .pm-player-volume {
                display: none;
            }
        }
    </style>

    <!-- Hero Header -->
    <div class="pm-hero">
        <div class="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-2">
            <div>
                <span class="pm-badge-student">
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                    Estudiante: <strong>Christian Puchaicela</strong>
                </span>
            </div>
            <span class="badge bg-light text-dark fw-semibold px-3 py-2 rounded-pill">
                Arquitectura N-Tier (Capa Web Forms 4.8)
            </span>
        </div>
        <h1 class="display-6 fw-bold mb-2">Cat&aacute;logo de Canciones</h1>
        <p class="lead mb-0 text-white-50 fs-6">
            Consulta en tiempo real mediante arquitectura en capas: <strong>Presentation &rarr; BLL &rarr; DAL &rarr; SQL Server</strong>.
            Reproduce tus pistas directamente o utiliza el reproductor multimedia integrado.
        </p>
    </div>

    <!-- Quick Stats & Filter Bar -->
    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="pm-stat-box d-flex align-items-center gap-3">
                <div style="font-size:2rem; color:#4f46e5;">&#127925;</div>
                <div>
                    <div class="text-muted small text-uppercase fw-bold">Canciones</div>
                    <asp:Label ID="lblTotalSongs" runat="server" CssClass="h4 mb-0 fw-bold text-dark" Text="0"></asp:Label>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="pm-stat-box d-flex align-items-center gap-3">
                <div style="font-size:2rem; color:#10b981;">&#9654;</div>
                <div>
                    <div class="text-muted small text-uppercase fw-bold">Reproducciones</div>
                    <asp:Label ID="lblTotalPlays" runat="server" CssClass="h4 mb-0 fw-bold text-dark" Text="0"></asp:Label>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="pm-stat-box">
                <div class="text-muted small text-uppercase fw-bold mb-2">B&uacute;squeda r&aacute;pida de canciones</div>
                <div class="input-group">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Buscar por t&iacute;tulo de canci&oacute;n..." />
                    <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-primary px-4 fw-semibold" Text="Buscar" OnClick="BtnSearch_Click" />
                    <asp:Button ID="btnClear" runat="server" CssClass="btn btn-outline-secondary" Text="Limpiar" OnClick="BtnClear_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Feedback Message -->
    <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-info d-block mb-3"></asp:Label>

    <!-- GridView Card Container -->
    <div class="pm-table-card">
        <div class="p-3 bg-light border-bottom d-flex justify-content-between align-items-center">
            <h5 class="mb-0 fw-bold text-dark">
                Listado Oficial de Canciones (SQL Server: <code>TBL_SONG</code>)
            </h5>
            <span class="badge bg-primary px-3 py-2 rounded-pill">
                Capa DAL + BLL Activa
            </span>
        </div>

        <div class="table-responsive">
            <asp:GridView ID="gvSongs" runat="server" AutoGenerateColumns="False" 
                          CssClass="table pm-table align-middle mb-0" 
                          GridLines="None" EmptyDataText="No se encontraron canciones en la base de datos."
                          OnRowDataBound="GvSongs_RowDataBound">
                <Columns>
                    <%-- Columna Play Directo / Indice --%>
                    <asp:TemplateField HeaderText="#" ItemStyle-Width="70px" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <button type="button" class="pm-play-btn" 
                                    data-id='<%# Eval("ID_SONG") %>'
                                    data-title='<%# Server.HtmlEncode(Eval("SONG_NAME").ToString()) %>'
                                    data-artist='<%# Server.HtmlEncode(Eval("ARTIST_LABEL") != DBNull.Value && !string.IsNullOrEmpty(Eval("ARTIST_LABEL").ToString()) ? Eval("ARTIST_LABEL").ToString() : "PoliMusic") %>'
                                    data-src='<%# ResolveSongUrl(Eval("SONG_PATH").ToString()) %>'
                                    onclick="playSongFromButton(this)"
                                    title="Reproducir pista">
                                &#9658;
                            </button>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Columna Nombre de Canción --%>
                    <asp:TemplateField HeaderText="Canci&oacute;n">
                        <ItemTemplate>
                            <div class="d-flex align-items-center gap-2">
                                <span class="badge bg-dark rounded-pill px-2 py-1">ID <%# Eval("ID_SONG") %></span>
                                <strong class="fs-6 text-dark"><%# Eval("SONG_NAME") %></strong>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Columna Artista --%>
                    <asp:TemplateField HeaderText="Artista">
                        <ItemTemplate>
                            <span class="text-secondary fw-semibold">
                                <%# Eval("ARTIST_LABEL") != DBNull.Value && !string.IsNullOrEmpty(Eval("ARTIST_LABEL").ToString()) ? Eval("ARTIST_LABEL") : "<span class='text-muted fst-italic'>Sin artista</span>" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Columna Álbum --%>
                    <asp:TemplateField HeaderText="&Aacute;lbum">
                        <ItemTemplate>
                            <span class="text-muted">
                                <%# Eval("ALBUM_TITLE") != DBNull.Value && !string.IsNullOrEmpty(Eval("ALBUM_TITLE").ToString()) ? Eval("ALBUM_TITLE") : "<span class='text-muted fst-italic'>General</span>" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Columna Reproducciones --%>
                    <asp:TemplateField HeaderText="Reproducciones" ItemStyle-Width="140px" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <span class="pm-badge-plays">
                                &#128266; <span id="plays-<%# Eval("ID_SONG") %>"><%# Eval("PLAYS") %></span>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Columna Reproductor HTML5 Integrado en GridView --%>
                    <asp:TemplateField HeaderText="Reproductor en Fila" ItemStyle-Width="280px">
                        <ItemTemplate>
                            <audio controls preload="none" class="pm-audio-inline w-100" 
                                   src='<%# ResolveSongUrl(Eval("SONG_PATH").ToString()) %>'
                                   onplay="onInlineAudioPlay(this, '<%# Eval("ID_SONG") %>')">
                                Tu navegador no soporta el elemento de audio.
                            </audio>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- Persistent Global Audio Player Bar -->
    <div id="playerBar" class="pm-player-bar">
        <div class="pm-player-track">
            <div class="pm-player-icon" id="playerIcon">&#127925;</div>
            <div>
                <div id="playerSongTitle" class="fw-bold text-truncate" style="max-width: 200px;">Selecciona una pista</div>
                <div id="playerSongArtist" class="small text-white-50 text-truncate" style="max-width: 200px;">PoliMusic Reproductor</div>
            </div>
        </div>

        <div class="pm-player-controls">
            <div class="pm-control-btns">
                <button type="button" class="pm-ctrl-btn" onclick="playPrevSong()" title="Anterior">&#9198;</button>
                <button type="button" class="pm-ctrl-play" id="btnMainPlay" onclick="togglePlayPause()" title="Reproducir/Pausar">&#9658;</button>
                <button type="button" class="pm-ctrl-btn" onclick="playNextSong()" title="Siguiente">&#9197;</button>
            </div>
            <div class="pm-progress-wrap">
                <span id="playerCurrentTime" class="pm-time-text">0:00</span>
                <div class="pm-progress-bar" id="progressBar" onclick="seekAudio(event)">
                    <div class="pm-progress-fill" id="progressFill"></div>
                </div>
                <span id="playerDuration" class="pm-time-text">0:00</span>
            </div>
        </div>

        <div class="pm-player-volume">
            <span style="font-size:1.1rem; cursor:pointer;" onclick="toggleMute()" id="volIcon">&#128266;</span>
            <input type="range" class="pm-volume-slider" id="volSlider" min="0" max="1" step="0.05" value="0.8" oninput="changeVolume(this.value)" />
        </div>
    </div>

    <!-- Hidden Master Audio Element -->
    <audio id="globalAudio" preload="none" style="display:none;"></audio>

    <!-- JavaScript Player Logic -->
    <script type="text/javascript">
        var audio = document.getElementById('globalAudio');
        var btnMainPlay = document.getElementById('btnMainPlay');
        var progressFill = document.getElementById('progressFill');
        var progressBar = document.getElementById('progressBar');
        var playerCurrentTime = document.getElementById('playerCurrentTime');
        var playerDuration = document.getElementById('playerDuration');
        var playerSongTitle = document.getElementById('playerSongTitle');
        var playerSongArtist = document.getElementById('playerSongArtist');
        var playerIcon = document.getElementById('playerIcon');
        var volSlider = document.getElementById('volSlider');
        var volIcon = document.getElementById('volIcon');

        var currentSongIndex = -1;
        var songQueue = [];

        // Build playlist queue from GridView rows
        function initQueue() {
            songQueue = [];
            var buttons = document.querySelectorAll('.pm-play-btn');
            buttons.forEach(function (btn, idx) {
                songQueue.push({
                    index: idx,
                    id: btn.getAttribute('data-id'),
                    title: btn.getAttribute('data-title'),
                    artist: btn.getAttribute('data-artist'),
                    src: btn.getAttribute('data-src'),
                    element: btn
                });
            });
        }

        window.addEventListener('DOMContentLoaded', function () {
            initQueue();
            audio.volume = volSlider.value;
        });

        function playSongFromButton(btn) {
            initQueue();
            var id = btn.getAttribute('data-id');
            var idx = songQueue.findIndex(function (s) { return s.id === id; });
            if (idx >= 0) {
                loadAndPlaySong(idx);
            }
        }

        function loadAndPlaySong(index) {
            if (index < 0 || index >= songQueue.length) return;
            currentSongIndex = index;
            var song = songQueue[index];

            // Pause any inline audios
            document.querySelectorAll('audio.pm-audio-inline').forEach(function (a) {
                if (!a.paused) a.pause();
            });

            audio.src = song.src;
            playerSongTitle.textContent = song.title;
            playerSongArtist.textContent = song.artist;
            playerIcon.innerHTML = "&#9835;";

            // Update UI buttons
            updateRowStyles();

            audio.play().then(function () {
                btnMainPlay.innerHTML = "&#10074;&#10074;";
                incrementPlayCount(song.id);
            }).catch(function (e) {
                console.log("Playback error: ", e);
            });
        }

        function togglePlayPause() {
            if (!audio.src) {
                if (songQueue.length > 0) {
                    loadAndPlaySong(0);
                }
                return;
            }
            if (audio.paused) {
                audio.play();
                btnMainPlay.innerHTML = "&#10074;&#10074;";
            } else {
                audio.pause();
                btnMainPlay.innerHTML = "&#9658;";
            }
            updateRowStyles();
        }

        function playNextSong() {
            if (songQueue.length === 0) return;
            var next = (currentSongIndex + 1) % songQueue.length;
            loadAndPlaySong(next);
        }

        function playPrevSong() {
            if (songQueue.length === 0) return;
            var prev = (currentSongIndex - 1 + songQueue.length) % songQueue.length;
            loadAndPlaySong(prev);
        }

        function updateRowStyles() {
            document.querySelectorAll('.pm-play-btn').forEach(function (b) {
                b.classList.remove('is-active');
                b.innerHTML = "&#9658;";
            });
            document.querySelectorAll('.pm-table tbody tr').forEach(function (r) {
                r.classList.remove('is-playing');
            });

            if (currentSongIndex >= 0 && currentSongIndex < songQueue.length) {
                var cur = songQueue[currentSongIndex];
                if (cur && cur.element) {
                    var tr = cur.element.closest('tr');
                    if (tr) tr.classList.add('is-playing');
                    if (!audio.paused) {
                        cur.element.classList.add('is-active');
                        cur.element.innerHTML = "&#10074;&#10074;";
                    }
                }
            }
        }

        // Time and Progress Bar updates
        audio.addEventListener('timeupdate', function () {
            if (audio.duration) {
                var pct = (audio.currentTime / audio.duration) * 100;
                progressFill.style.width = pct + '%';
                playerCurrentTime.textContent = formatTime(audio.currentTime);
                playerDuration.textContent = formatTime(audio.duration);
            }
        });

        audio.addEventListener('ended', function () {
            playNextSong();
        });

        function seekAudio(e) {
            if (!audio.duration) return;
            var rect = progressBar.getBoundingClientRect();
            var clickX = e.clientX - rect.left;
            var pct = clickX / rect.width;
            audio.currentTime = pct * audio.duration;
        }

        function changeVolume(val) {
            audio.volume = val;
            volIcon.innerHTML = val == 0 ? "&#128263;" : (val < 0.5 ? "&#128265;" : "&#128266;");
        }

        function toggleMute() {
            if (audio.volume > 0) {
                audio.dataset.prevVol = audio.volume;
                audio.volume = 0;
                volSlider.value = 0;
                volIcon.innerHTML = "&#128263;";
            } else {
                var prev = audio.dataset.prevVol || 0.8;
                audio.volume = prev;
                volSlider.value = prev;
                volIcon.innerHTML = "&#128266;";
            }
        }

        function onInlineAudioPlay(inlineEl, songId) {
            // If main audio is playing, pause it
            if (!audio.paused) {
                audio.pause();
                btnMainPlay.innerHTML = "&#9658;";
            }
            // Pause any other inline audios
            document.querySelectorAll('audio.pm-audio-inline').forEach(function (a) {
                if (a !== inlineEl && !a.paused) a.pause();
            });
            incrementPlayCount(songId);
        }

        function incrementPlayCount(songId) {
            if (!songId) return;
            fetch('Song.aspx/IncrementPlay', {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify({ idSong: parseInt(songId) })
            }).then(function (res) {
                return res.json();
            }).then(function (data) {
                if (data && data.d) {
                    var el = document.getElementById('plays-' + songId);
                    if (el) {
                        var curr = parseInt(el.textContent) || 0;
                        el.textContent = curr + 1;
                    }
                }
            }).catch(function (err) {
                console.log("Increment play failed (normal if offline): ", err);
            });
        }

        function formatTime(secs) {
            var m = Math.floor(secs / 60) || 0;
            var s = Math.floor(secs % 60) || 0;
            return m + ':' + (s < 10 ? '0' : '') + s;
        }
    </script>
</asp:Content>
