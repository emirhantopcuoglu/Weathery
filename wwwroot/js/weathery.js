(function () {
    'use strict';

    const STORAGE_KEYS = {
        theme: 'weathery.theme',
        recents: 'weathery.recentCities'
    };
    const MAX_RECENTS = 5;

    // ---------- Tema ----------
    function applyTheme(theme) {
        document.body.setAttribute('data-theme', theme);
        const icon = document.querySelector('.theme-toggle i');
        if (icon) {
            icon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
        }
    }

    function initTheme() {
        const saved = localStorage.getItem(STORAGE_KEYS.theme);
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const theme = saved || (prefersDark ? 'dark' : 'light');
        applyTheme(theme);

        const toggle = document.querySelector('.theme-toggle');
        if (toggle) {
            toggle.addEventListener('click', () => {
                const next = document.body.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
                localStorage.setItem(STORAGE_KEYS.theme, next);
                applyTheme(next);
            });
        }
    }

    // ---------- Son aranan şehirler ----------
    function getRecents() {
        try {
            return JSON.parse(localStorage.getItem(STORAGE_KEYS.recents) || '[]');
        } catch { return []; }
    }

    function pushRecent(city) {
        if (!city) return;
        const trimmed = city.trim();
        if (!trimmed) return;
        const list = getRecents().filter(c => c.toLowerCase() !== trimmed.toLowerCase());
        list.unshift(trimmed);
        localStorage.setItem(STORAGE_KEYS.recents, JSON.stringify(list.slice(0, MAX_RECENTS)));
    }

    function removeRecent(city) {
        const list = getRecents().filter(c => c.toLowerCase() !== city.toLowerCase());
        localStorage.setItem(STORAGE_KEYS.recents, JSON.stringify(list));
    }

    function clearRecents() {
        localStorage.removeItem(STORAGE_KEYS.recents);
    }

    function escapeHtml(str) {
        return String(str).replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        }[c]));
    }

    async function fetchSuggestions(query) {
        const url = `https://geocoding-api.open-meteo.com/v1/search` +
                    `?name=${encodeURIComponent(query)}&count=6&language=tr&format=json`;
        try {
            const res = await fetch(url);
            if (!res.ok) return [];
            const data = await res.json();
            return data.results || [];
        } catch { return []; }
    }

    function debounce(fn, ms) {
        let t;
        return (...args) => {
            clearTimeout(t);
            t = setTimeout(() => fn(...args), ms);
        };
    }

    function initSearch() {
        const form = document.querySelector('.search-form');
        if (!form) return;

        const input = form.querySelector('input[name="city"]');
        const list = form.querySelector('.recent-suggestions');

        const params = new URLSearchParams(window.location.search);
        const submitted = params.get('city');
        if (submitted) pushRecent(submitted);

        if (!input || !list) return;

        let activeIndex = -1;
        let currentItems = [];

        function renderItems(items, mode) {
            list.innerHTML = '';
            currentItems = items;
            activeIndex = -1;

            if (items.length === 0) {
                if (mode === 'remote') {
                    list.innerHTML = '<div class="suggestion-empty">Sonuç bulunamadı</div>';
                    list.classList.add('is-open');
                } else {
                    list.classList.remove('is-open');
                }
                return;
            }

            items.forEach((item, i) => {
                const li = document.createElement('li');
                li.dataset.index = i;
                const icon = mode === 'recent' ? 'bi-clock-history' : 'bi-geo-alt';
                const meta = item.meta ? `<span class="suggestion-meta">${escapeHtml(item.meta)}</span>` : '';
                const removeBtn = mode === 'recent'
                    ? `<button class="suggestion-remove" type="button" aria-label="Geçmişten kaldır" title="Kaldır"><i class="bi bi-x-lg"></i></button>`
                    : '';
                li.innerHTML =
                    `<i class="bi ${icon}"></i>` +
                    `<span class="suggestion-name">${escapeHtml(item.label)}</span>` +
                    meta + removeBtn;

                li.addEventListener('mousedown', e => {
                    if (e.target.closest('.suggestion-remove')) {
                        e.preventDefault();
                        removeRecent(item.value);
                        showRecents();
                        return;
                    }
                    e.preventDefault();
                    input.value = item.value;
                    form.submit();
                });
                li.addEventListener('mouseenter', () => setActive(i));
                list.appendChild(li);
            });

            if (mode === 'recent' && items.length > 0) {
                const clearAll = document.createElement('div');
                clearAll.className = 'suggestion-clear';
                clearAll.innerHTML = `<i class="bi bi-trash"></i> Tüm geçmişi temizle`;
                clearAll.addEventListener('mousedown', e => {
                    e.preventDefault();
                    clearRecents();
                    list.classList.remove('is-open');
                });
                list.appendChild(clearAll);
            }

            list.classList.add('is-open');
        }

        function setActive(i) {
            activeIndex = i;
            list.querySelectorAll('li').forEach((el, idx) => {
                el.classList.toggle('is-active', idx === i);
            });
        }

        function showRecents() {
            const items = getRecents().map(city => ({ label: city, value: city, meta: '' }));
            renderItems(items, 'recent');
        }

        const search = debounce(async query => {
            if (input.value.trim() !== query) return;
            const results = await fetchSuggestions(query);
            if (input.value.trim() !== query) return;
            const items = results.map(r => ({
                label: r.name,
                value: r.name,
                meta: [r.admin1, r.country].filter(Boolean).join(', ')
            }));
            renderItems(items, 'remote');
        }, 220);

        input.addEventListener('focus', () => {
            const q = input.value.trim();
            if (q.length >= 2) search(q); else showRecents();
        });

        input.addEventListener('input', () => {
            const q = input.value.trim();
            if (q.length >= 2) search(q); else showRecents();
        });

        input.addEventListener('blur', () => setTimeout(() => list.classList.remove('is-open'), 150));

        input.addEventListener('keydown', e => {
            if (!list.classList.contains('is-open') || currentItems.length === 0) return;
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                setActive((activeIndex + 1) % currentItems.length);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                setActive(activeIndex <= 0 ? currentItems.length - 1 : activeIndex - 1);
            } else if (e.key === 'Enter' && activeIndex >= 0) {
                e.preventDefault();
                input.value = currentItems[activeIndex].value;
                form.submit();
            } else if (e.key === 'Escape') {
                list.classList.remove('is-open');
            }
        });

        form.addEventListener('submit', () => pushRecent(input.value));
    }

    // ---------- Sayı animasyonu ----------
    function animateCount(el) {
        const target = parseFloat(el.dataset.value);
        if (isNaN(target)) return;
        const duration = 900;
        const start = performance.now();
        const decimals = parseInt(el.dataset.decimals || '0', 10);
        const suffix = el.dataset.suffix || '';
        const from = 0;
        function step(now) {
            const t = Math.min(1, (now - start) / duration);
            const eased = 1 - Math.pow(1 - t, 3);
            const current = from + (target - from) * eased;
            el.textContent = current.toFixed(decimals) + suffix;
            if (t < 1) requestAnimationFrame(step);
        }
        requestAnimationFrame(step);
    }

    function initCountUp() {
        document.querySelectorAll('[data-count]').forEach(animateCount);
    }

    // ---------- Forecast: gün seçimi ----------
    function initDailySelector() {
        const dayCards = document.querySelectorAll('.day-card');
        const stripContainer = document.querySelector('[data-hourly-strips]');
        if (dayCards.length === 0 || !stripContainer) return;

        const strips = stripContainer.querySelectorAll('[data-strip-for]');

        function selectDay(date) {
            dayCards.forEach(c => c.classList.toggle('is-active', c.dataset.date === date));
            strips.forEach(s => s.style.display = s.dataset.stripFor === date ? '' : 'none');
        }

        dayCards.forEach(card => {
            card.addEventListener('click', () => selectDay(card.dataset.date));
        });

        if (dayCards[0]) selectDay(dayCards[0].dataset.date);
    }

    // ---------- Atmosferik efektler ----------
    function spawnEffect() {
        const condition = document.body.dataset.weather;
        const layer = document.querySelector('.fx-layer');
        if (!layer || !condition) return;
        layer.innerHTML = '';
        layer.className = 'fx-layer';

        if (condition === 'rain' || condition === 'thunder') {
            layer.classList.add('fx-rain');
            for (let i = 0; i < 60; i++) {
                const drop = document.createElement('span');
                drop.className = 'drop';
                drop.style.left = Math.random() * 100 + 'vw';
                drop.style.animationDuration = (0.5 + Math.random() * 0.7) + 's';
                drop.style.animationDelay = -Math.random() * 2 + 's';
                drop.style.opacity = 0.3 + Math.random() * 0.5;
                layer.appendChild(drop);
            }
        } else if (condition === 'snow') {
            layer.classList.add('fx-snow');
            const symbols = ['❄', '❅', '❆'];
            for (let i = 0; i < 40; i++) {
                const flake = document.createElement('span');
                flake.className = 'flake';
                flake.textContent = symbols[i % symbols.length];
                flake.style.left = Math.random() * 100 + 'vw';
                flake.style.fontSize = (10 + Math.random() * 14) + 'px';
                flake.style.animationDuration = (6 + Math.random() * 6) + 's';
                flake.style.animationDelay = -Math.random() * 8 + 's';
                layer.appendChild(flake);
            }
        } else if (condition === 'clouds' || condition === 'mist') {
            layer.classList.add('fx-clouds');
            for (let i = 0; i < 5; i++) {
                const cloud = document.createElement('span');
                cloud.className = 'cloud';
                const w = 200 + Math.random() * 220;
                cloud.style.width = w + 'px';
                cloud.style.height = (w * 0.45) + 'px';
                cloud.style.top = (5 + Math.random() * 70) + 'vh';
                cloud.style.animationDuration = (40 + Math.random() * 40) + 's';
                cloud.style.animationDelay = -Math.random() * 60 + 's';
                cloud.style.opacity = 0.6 + Math.random() * 0.3;
                layer.appendChild(cloud);
            }
        } else if (condition === 'clear-day') {
            const sun = document.createElement('div');
            sun.className = 'fx-sun';
            document.body.appendChild(sun);
        } else if (condition === 'clear-night') {
            layer.classList.add('fx-stars');
            for (let i = 0; i < 80; i++) {
                const s = document.createElement('span');
                s.className = 'star';
                const sz = Math.random() * 2 + 1;
                s.style.width = sz + 'px';
                s.style.height = sz + 'px';
                s.style.left = Math.random() * 100 + 'vw';
                s.style.top = Math.random() * 100 + 'vh';
                s.style.animationDelay = -Math.random() * 3 + 's';
                s.style.opacity = Math.random();
                layer.appendChild(s);
            }
        }
    }

    // ---------- Init ----------
    document.addEventListener('DOMContentLoaded', () => {
        initTheme();
        initSearch();
        initCountUp();
        initDailySelector();
        spawnEffect();
    });
})();
