/* ==========================================================================
   PoliMusic - Reproductor y utilidades de interfaz
   Depende de HTML5 Audio. La barra de reproduccion es persistente: el estado
   se guarda en sessionStorage y se restaura al navegar entre paginas.
   ========================================================================== */

(function () {
    'use strict';

    var STORE_KEY = 'polimusic.player.state';
    var audio = null;
    var state = { src: '', title: '', artist: '', art: '', queue: [], index: -1, time: 0, volume: 0.7 };

    /* ------------------------------------------------------- persistencia -- */

    function load() {
        try {
            var raw = sessionStorage.getItem(STORE_KEY);
            if (!raw) { return; }
            var parsed = JSON.parse(raw);
            if (parsed && typeof parsed === 'object') {
                state.src = parsed.src || '';
                state.title = parsed.title || '';
                state.artist = parsed.artist || '';
                state.art = parsed.art || '';
                state.queue = parsed.queue || [];
                state.index = typeof parsed.index === 'number' ? parsed.index : -1;
                state.time = parsed.time || 0;
                state.volume = typeof parsed.volume === 'number' ? parsed.volume : 0.7;
            }
        } catch (e) { /* almacenamiento no disponible */ }
    }

    function save() {
        try {
            if (audio && !isNaN(audio.currentTime)) { state.time = audio.currentTime; }
            sessionStorage.setItem(STORE_KEY, JSON.stringify(state));
        } catch (e) { /* ignorar */ }
    }

    /* ------------------------------------------------------------ toast ---- */

    function toast(message, isError) {
        var el = document.getElementById('pmToast');
        if (!el) { return; }
        el.textContent = message;
        el.className = 'pm-toast show' + (isError ? ' error' : '');
        clearTimeout(el._t);
        el._t = setTimeout(function () { el.className = 'pm-toast'; }, 3200);
    }

    window.pmToast = toast;

    /* --------------------------------------------------------- utilidades -- */

    function fmt(seconds) {
        if (!isFinite(seconds) || seconds < 0) { return '0:00'; }
        var m = Math.floor(seconds / 60);
        var s = Math.floor(seconds % 60);
        return m + ':' + (s < 10 ? '0' + s : s);
    }

    function $id(id) { return document.getElementById(id); }

    function setArt(url, initials) {
        var box = $id('pmPlayerArt');
        if (!box) { return; }
        if (url) {
            box.innerHTML = '<img src="' + url + '" alt="" onerror="this.style.display=\'none\'">';
        } else {
            box.innerHTML = '<span class="pm-art-initials">' + (initials || '♪') + '</span>';
        }
    }

    /* ------------------------------------------------------------ player --- */

    function buildAudio() {
        if (audio) { return audio; }
        audio = new Audio();
        audio.preload = 'metadata';
        audio.volume = state.volume;

        audio.addEventListener('timeupdate', function () {
            var fill = $id('pmProgressFill');
            var cur = $id('pmTimeCurrent');
            var dur = $id('pmTimeTotal');
            if (fill && audio.duration) {
                fill.style.width = ((audio.currentTime / audio.duration) * 100) + '%';
            }
            if (cur) { cur.textContent = fmt(audio.currentTime); }
            if (dur) { dur.textContent = fmt(audio.duration); }
            save();
        });

        audio.addEventListener('loadedmetadata', function () {
            var dur = $id('pmTimeTotal');
            if (dur) { dur.textContent = fmt(audio.duration); }
            var fill = $id('pmVolumeFill');
            if (fill) { fill.style.width = (audio.volume * 100) + '%'; }
        });

        audio.addEventListener('play', function () { updateToggle(true); });
        audio.addEventListener('pause', function () { updateToggle(false); });
        audio.addEventListener('ended', function () { next(); });

        audio.addEventListener('error', function () {
            var t = $id('pmPlayerTitle');
            if (t) { t.textContent = 'Audio no disponible'; }
            toast('No se pudo reproducir el archivo de audio de esta cancion.', true);
            updateToggle(false);
        });

        return audio;
    }

    function updateToggle(playing) {
        var btn = $id('pmPlayPause');
        if (!btn) { return; }
        btn.innerHTML = playing
            ? '<svg viewBox="0 0 24 24" fill="currentColor"><rect x="6" y="4" width="4" height="16" rx="1"/><rect x="14" y="4" width="4" height="16" rx="1"/></svg>'
            : '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>';
        btn.setAttribute('aria-label', playing ? 'Pausar' : 'Reproducir');
    }

    function render() {
        var t = $id('pmPlayerTitle');
        var a = $id('pmPlayerArtist');
        if (t) { t.textContent = state.title || 'Sin reproduccion'; }
        if (a) { a.textContent = state.artist || 'PoliMusic'; }
        setArt(state.art, '♪');
        document.body.classList.add('pm-has-player');
        var c = document.querySelector('.pm-content');
        if (c) { c.classList.add('has-player'); }
    }

    /**
     * Carga y reproduce una cancion.
     * @param {Object} song  {id, src, title, artist, art, initials}
     * @param {Array}  queue lista de canciones para navegacion siguiente/anterior
     */
    function play(song, queue) {
        if (!song || !song.src) {
            toast('Esta cancion no tiene un archivo de audio disponible.', true);
            return;
        }
        var a = buildAudio();

        if (queue && queue.length) {
            state.queue = queue;
            state.index = -1;
            for (var i = 0; i < queue.length; i++) {
                if (queue[i].src === song.src) { state.index = i; break; }
            }
        }

        state.src = song.src;
        state.title = song.title || '';
        state.artist = song.artist || '';
        state.art = song.art || '';
        state.time = 0;

        a.src = song.src;
        a.currentTime = 0;

        render();
        var p = a.play();
        if (p && p.catch) {
            p.catch(function () {
                toast('El navegador bloqueo la reproduccion automatica. Pulse reproducir.', true);
                updateToggle(false);
            });
        }
        save();

        // Registrar la reproduccion en el servidor (contador PLAYS).
        if (song.id) { countPlay(song.id); }
    }

    function countPlay(songId) {
        try {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', (window.pmBase || '') + 'Play.aspx?id=' + encodeURIComponent(songId), true);
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.send('token=' + encodeURIComponent(window.pmToken || ''));
        } catch (e) { /* el contador no debe romper la reproduccion */ }
    }

    function toggle() {
        var a = buildAudio();
        if (!state.src) { toast('Seleccione una cancion para reproducir.'); return; }
        if (a.paused) { a.play(); } else { a.pause(); }
    }

    function step(delta) {
        if (!state.queue.length) { return; }
        var i = state.index + delta;
        if (i < 0) { i = state.queue.length - 1; }
        if (i >= state.queue.length) { i = 0; }
        state.index = i;
        play(state.queue[i], null);
    }

    function next() { step(1); }
    function prev() { step(-1); }

    function seekTo(ratio) {
        var a = buildAudio();
        if (a.duration) { a.currentTime = a.duration * ratio; }
    }

    function setVolume(ratio) {
        var a = buildAudio();
        var v = Math.max(0, Math.min(1, ratio));
        a.volume = v;
        state.volume = v;
        var fill = $id('pmVolumeFill');
        if (fill) { fill.style.width = (v * 100) + '%'; }
        save();
    }

    function restore() {
        if (!state.src) { return; }
        var a = buildAudio();
        a.src = state.src;
        render();
        var cur = $id('pmTimeCurrent');
        if (cur && state.time) { cur.textContent = fmt(state.time); }
        // Se restaura la posicion sin iniciar la reproduccion: el navegador
        // exige una accion del usuario para reproducir audio.
        a.addEventListener('loadedmetadata', function once() {
            a.removeEventListener('loadedmetadata', once);
            if (state.time && state.time < a.duration) { a.currentTime = state.time; }
        });
        updateToggle(false);
    }

    /* ------------------------------------------------------ eventos UI ----- */

    function bind() {
        var pp = $id('pmPlayPause');
        if (pp) { pp.addEventListener('click', toggle); }
        var nb = $id('pmNext');
        if (nb) { nb.addEventListener('click', next); }
        var pb = $id('pmPrev');
        if (pb) { pb.addEventListener('click', prev); }

        var prog = $id('pmProgress');
        if (prog) {
            prog.addEventListener('click', function (e) {
                var r = prog.getBoundingClientRect();
                seekTo((e.clientX - r.left) / r.width);
            });
        }

        var vol = $id('pmVolume');
        if (vol) {
            vol.addEventListener('click', function (e) {
                var r = vol.getBoundingClientRect();
                setVolume((e.clientX - r.left) / r.width);
            });
        }

        // Botones "reproducir" de tarjetas y tablas.
        document.addEventListener('click', function (e) {
            var btn = e.target.closest ? e.target.closest('[data-pm-play]') : null;
            if (!btn) { return; }
            e.preventDefault();
            e.stopPropagation();
            var payload;
            try { payload = JSON.parse(btn.getAttribute('data-pm-play')); }
            catch (err) { return; }
            var queue = [];
            var qraw = btn.getAttribute('data-pm-queue');
            if (qraw) {
                try { queue = JSON.parse(qraw); } catch (err2) { queue = []; }
            }
            if (!queue.length) { queue = [payload]; }
            play(payload, queue);
        });

        window.addEventListener('beforeunload', save);
    }

    /* ------------------------------------------------ busqueda y modales ---- */

    function initSearch() {
        var input = $id('pmSearchInput');
        if (!input) { return; }
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                var q = input.value.trim();
                var base = window.pmBase || '';
                window.location.href = base + 'Canciones.aspx?q=' + encodeURIComponent(q);
            }
        });
    }

    function initModals() {
        document.addEventListener('click', function (e) {
            var opener = e.target.closest ? e.target.closest('[data-pm-modal-open]') : null;
            if (opener) {
                e.preventDefault();
                var target = document.getElementById(opener.getAttribute('data-pm-modal-open'));
                if (target) { target.classList.add('open'); }
                return;
            }
            var closer = e.target.closest ? e.target.closest('[data-pm-modal-close]') : null;
            if (closer) {
                e.preventDefault();
                var m = closer.closest('.pm-modal-backdrop');
                if (m) { m.classList.remove('open'); }
                return;
            }
            if (e.target.classList && e.target.classList.contains('pm-modal-backdrop')) {
                e.target.classList.remove('open');
            }
        });
    }

    function initConfirm() {
        document.addEventListener('click', function (e) {
            var el = e.target.closest ? e.target.closest('[data-pm-confirm]') : null;
            if (!el) { return; }
            var msg = el.getAttribute('data-pm-confirm') || '¿Confirma esta acción?';
            if (!window.confirm(msg)) {
                e.preventDefault();
                e.stopPropagation();
            }
        });
    }

    /* ---------------------------------------------------------- arranque --- */

    document.addEventListener('DOMContentLoaded', function () {
        load();
        bind();
        initSearch();
        initModals();
        initConfirm();
        if (state.src) { restore(); }
    });

    // API publica para el codigo de las paginas.
    window.PoliMusicPlayer = {
        play: play,
        toggle: toggle,
        next: next,
        prev: prev,
        state: state
    };
})();
