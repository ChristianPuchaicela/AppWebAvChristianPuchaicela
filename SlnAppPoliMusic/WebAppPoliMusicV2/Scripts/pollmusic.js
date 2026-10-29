/* ==========================================================================
   PoliMusic - reproductor persistente y utilidades de interfaz
   --------------------------------------------------------------------------
   - Barra de reproduccion fija que sobrevive a la navegacion (sessionStorage).
   - Cola de reproduccion con siguiente/anterior.
   - Registro del contador PLAYS contra Play.aspx (POST con token CSRF).
   - Modales, confirmaciones, buscador y avisos flotantes.

   No depende de jQuery ni de ninguna libreria externa.
   ========================================================================== */

(function () {
    'use strict';

    var STORE_KEY = 'polimusic.player.v2';
    var audio = null;
    var state = {
        src: '',
        title: '',
        artist: '',
        art: '',
        initials: '♪',
        queue: [],
        index: -1,
        time: 0,
        volume: 0.7
    };

    /* ------------------------------------------------------- utilidades -- */

    function $id(id) {
        return document.getElementById(id);
    }

    function fmt(seconds) {
        if (!isFinite(seconds) || seconds < 0) {
            return '0:00';
        }
        var m = Math.floor(seconds / 60);
        var s = Math.floor(seconds % 60);
        return m + ':' + (s < 10 ? '0' + s : s);
    }

    function toast(message, isError) {
        var el = $id('pmToast');
        if (!el) {
            return;
        }
        el.textContent = message;
        el.className = 'pm-toast show' + (isError ? ' error' : '');
        window.clearTimeout(el._timer);
        el._timer = window.setTimeout(function () {
            el.className = 'pm-toast';
        }, 3400);
    }

    window.pmToast = toast;

    /* ---------------------------------------------------- persistencia --- */

    function load() {
        try {
            var raw = window.sessionStorage.getItem(STORE_KEY);
            if (!raw) {
                return;
            }
            var parsed = JSON.parse(raw);
            if (!parsed || typeof parsed !== 'object') {
                return;
            }
            state.src = parsed.src || '';
            state.title = parsed.title || '';
            state.artist = parsed.artist || '';
            state.art = parsed.art || '';
            state.initials = parsed.initials || '♪';
            state.queue = parsed.queue || [];
            state.index = typeof parsed.index === 'number' ? parsed.index : -1;
            state.time = parsed.time || 0;
            state.volume = typeof parsed.volume === 'number' ? parsed.volume : 0.7;
        } catch (e) {
            /* almacenamiento no disponible: se ignora */
        }
    }

    function save() {
        try {
            if (audio && !isNaN(audio.currentTime)) {
                state.time = audio.currentTime;
            }
            window.sessionStorage.setItem(STORE_KEY, JSON.stringify(state));
        } catch (e) {
            /* modo privado: se ignora */
        }
    }

    /* -------------------------------------------------------- reproductor - */

    function buildAudio() {
        if (audio) {
            return audio;
        }
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
            if (cur) {
                cur.textContent = fmt(audio.currentTime);
            }
            if (dur) {
                dur.textContent = fmt(audio.duration);
            }
            save();
        });

        audio.addEventListener('loadedmetadata', function () {
            var dur = $id('pmTimeTotal');
            if (dur) {
                dur.textContent = fmt(audio.duration);
            }
        });

        audio.addEventListener('play', function () { updateToggle(true); });
        audio.addEventListener('pause', function () { updateToggle(false); });
        audio.addEventListener('ended', function () { next(); });

        audio.addEventListener('error', function () {
            var t = $id('pmPlayerTitle');
            if (t) {
                t.textContent = 'Audio no disponible';
            }
            toast('No se pudo reproducir el archivo de audio de esta cancion.', true);
            updateToggle(false);
        });

        return audio;
    }

    function updateToggle(playing) {
        var btn = $id('pmPlayPause');
        if (!btn) {
            return;
        }
        btn.innerHTML = playing
            ? '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="6" y="4" width="4" height="16" rx="1"/><rect x="14" y="4" width="4" height="16" rx="1"/></svg>'
            : '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M8 5v14l11-7z"/></svg>';
        btn.setAttribute('aria-label', playing ? 'Pausar' : 'Reproducir');
    }

    function setArt(url, initials) {
        var box = $id('pmPlayerArt');
        if (!box) {
            return;
        }
        if (url) {
            box.innerHTML = '<img src="' + url + '" alt="" onerror="this.style.display=\'none\'">';
        } else {
            box.innerHTML = '<span class="pm-art-initials">' + (initials || '♪') + '</span>';
        }
    }

    function render() {
        var t = $id('pmPlayerTitle');
        var a = $id('pmPlayerArtist');
        if (t) {
            t.textContent = state.title || 'Sin reproduccion';
        }
        if (a) {
            a.textContent = state.artist || 'PoliMusic';
        }
        setArt(state.art, state.initials);
        document.body.classList.add('pm-has-player');
        var content = document.querySelector('.pm-content');
        if (content) {
            content.classList.add('has-player');
        }
    }

    function countPlay(songId) {
        if (!songId) {
            return;
        }
        try {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', (window.pmBase || '') + 'Play.aspx', true);
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.send('id=' + encodeURIComponent(songId) +
                     '&__PoliMusicToken=' + encodeURIComponent(window.pmToken || ''));
        } catch (e) {
            /* el contador nunca debe romper la reproduccion */
        }
    }

    /**
     * Carga y reproduce una cancion.
     * @param {Object} song  {id, src, title, artist, art, initials}
     * @param {Array}  queue lista de canciones para siguiente/anterior
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
                if (queue[i].src === song.src) {
                    state.index = i;
                    break;
                }
            }
        }

        state.src = song.src;
        state.title = song.title || '';
        state.artist = song.artist || '';
        state.art = song.art || '';
        state.initials = song.initials || '♪';
        state.time = 0;

        a.src = song.src;
        a.currentTime = 0;
        render();

        var promise = a.play();
        if (promise && promise.catch) {
            promise.catch(function () {
                toast('El navegador bloqueo la reproduccion automatica. Pulse reproducir.', true);
                updateToggle(false);
            });
        }
        save();
        countPlay(song.id);
    }

    function toggle() {
        var a = buildAudio();
        if (!state.src) {
            toast('Seleccione una cancion para reproducir.');
            return;
        }
        if (a.paused) {
            a.play();
        } else {
            a.pause();
        }
    }

    function step(delta) {
        if (!state.queue.length) {
            return;
        }
        var i = state.index + delta;
        if (i < 0) {
            i = state.queue.length - 1;
        }
        if (i >= state.queue.length) {
            i = 0;
        }
        state.index = i;
        play(state.queue[i], null);
    }

    function next() {
        step(1);
    }

    function prev() {
        step(-1);
    }

    function seekTo(ratio) {
        var a = buildAudio();
        if (a.duration) {
            a.currentTime = a.duration * ratio;
        }
    }

    function setVolume(ratio) {
        var a = buildAudio();
        var v = Math.max(0, Math.min(1, ratio));
        a.volume = v;
        state.volume = v;
        var fill = $id('pmVolumeFill');
        if (fill) {
            fill.style.width = (v * 100) + '%';
        }
        save();
    }

    function restore() {
        if (!state.src) {
            return;
        }
        var a = buildAudio();
        a.src = state.src;
        render();
        var cur = $id('pmTimeCurrent');
        if (cur && state.time) {
            cur.textContent = fmt(state.time);
        }
        // Se restaura la posicion sin iniciar la reproduccion: el navegador
        // exige una accion del usuario para reproducir audio.
        a.addEventListener('loadedmetadata', function once() {
            a.removeEventListener('loadedmetadata', once);
            if (state.time && state.time < a.duration) {
                a.currentTime = state.time;
            }
        });
        var vol = $id('pmVolumeFill');
        if (vol) {
            vol.style.width = (state.volume * 100) + '%';
        }
        updateToggle(false);
    }

    /* ------------------------------------------------------------ eventos - */

    function closest(el, selector) {
        if (!el) {
            return null;
        }
        if (el.closest) {
            return el.closest(selector);
        }
        return null;
    }

    function bindPlayer() {
        var pp = $id('pmPlayPause');
        if (pp) {
            pp.addEventListener('click', toggle);
        }
        var nb = $id('pmNext');
        if (nb) {
            nb.addEventListener('click', next);
        }
        var pb = $id('pmPrev');
        if (pb) {
            pb.addEventListener('click', prev);
        }

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

        window.addEventListener('beforeunload', save);
    }

    function bindPlayButtons() {
        document.addEventListener('click', function (e) {
            var btn = closest(e.target, '[data-pm-play]');
            if (!btn) {
                return;
            }
            e.preventDefault();
            e.stopPropagation();

            var payload;
            try {
                payload = JSON.parse(btn.getAttribute('data-pm-play'));
            } catch (err) {
                toast('No se pudo leer los datos de la cancion.', true);
                return;
            }

            var queue = [];
            var rawQueue = btn.getAttribute('data-pm-queue');
            if (rawQueue) {
                try {
                    queue = JSON.parse(rawQueue);
                } catch (err2) {
                    queue = [];
                }
            }
            if (!queue.length) {
                queue = [payload];
            }
            play(payload, queue);
        });
    }

    function bindSearch() {
        var input = $id('pmSearchInput');
        if (!input) {
            return;
        }
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                var q = input.value.trim();
                window.location.href = (window.pmBase || '') + 'Canciones.aspx?q=' +
                    encodeURIComponent(q);
            }
        });
    }

    /**
     * Filtros de pagina sin formularios anidados: el boton lleva la URL base
     * en data-pm-filter y el valor se toma del input del mismo contenedor.
     */
    function applyFilter(button) {
        var wrap = closest(button, '.pm-filter');
        if (!wrap) {
            return;
        }
        var input = wrap.querySelector('input[name]');
        if (!input) {
            return;
        }
        var base = button.getAttribute('data-pm-filter') || window.location.pathname;
        var sep = base.indexOf('?') >= 0 ? '&' : '?';
        window.location.href = base + sep + encodeURIComponent(input.name) + '=' +
            encodeURIComponent(input.value);
    }

    function bindFilters() {
        document.addEventListener('click', function (e) {
            var btn = closest(e.target, '[data-pm-filter]');
            if (btn) {
                e.preventDefault();
                applyFilter(btn);
            }
        });

        var inputs = document.querySelectorAll('.pm-filter input[name]');
        for (var i = 0; i < inputs.length; i++) {
            inputs[i].addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    var btn = closest(this, '.pm-filter').querySelector('[data-pm-filter]');
                    if (btn) {
                        applyFilter(btn);
                    }
                }
            });
        }
    }

    function bindModals() {
        document.addEventListener('click', function (e) {
            var opener = closest(e.target, '[data-pm-modal-open]');
            if (opener) {
                e.preventDefault();
                var target = document.getElementById(opener.getAttribute('data-pm-modal-open'));
                if (target) {
                    target.classList.add('open');
                    var focusable = target.querySelector('input, select, textarea, button');
                    if (focusable) {
                        focusable.focus();
                    }
                }
                return;
            }

            var closer = closest(e.target, '[data-pm-modal-close]');
            if (closer) {
                e.preventDefault();
                var modal = closest(closer, '.pm-modal-backdrop');
                if (modal) {
                    modal.classList.remove('open');
                }
                return;
            }

            if (e.target.classList && e.target.classList.contains('pm-modal-backdrop')) {
                e.target.classList.remove('open');
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                var open = document.querySelector('.pm-modal-backdrop.open');
                if (open) {
                    open.classList.remove('open');
                }
            }
        });
    }

    function bindConfirm() {
        document.addEventListener('click', function (e) {
            var el = closest(e.target, '[data-pm-confirm]');
            if (!el) {
                return;
            }
            var msg = el.getAttribute('data-pm-confirm') || '¿Confirma esta accion?';
            if (!window.confirm(msg)) {
                e.preventDefault();
                e.stopPropagation();
            }
        });
    }

    function bindFlash() {
        var alert = document.querySelector('.alert');
        if (!alert) {
            return;
        }
        window.setTimeout(function () {
            alert.style.transition = 'opacity 400ms ease';
            alert.style.opacity = '0';
            window.setTimeout(function () {
                if (alert.parentNode) {
                    alert.parentNode.removeChild(alert);
                }
            }, 420);
        }, 6000);
    }

    /* ------------------------------------------------------------ arranque */

    document.addEventListener('DOMContentLoaded', function () {
        load();
        bindPlayer();
        bindPlayButtons();
        bindSearch();
        bindFilters();
        bindModals();
        bindConfirm();
        bindFlash();
        if (state.src) {
            restore();
        }
    });

    // API publica por si alguna pagina necesita controlar el reproductor.
    window.PoliMusicPlayer = {
        play: play,
        toggle: toggle,
        next: next,
        prev: prev,
        state: state
    };
})();
