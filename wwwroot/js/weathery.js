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

    function initRecents() {
        const form = document.querySelector('.search-form');
        if (!form) return;

        const input = form.querySelector('input[name="city"]');
        const list = form.querySelector('.recent-suggestions');

        const params = new URLSearchParams(window.location.search);
        const submitted = params.get('city');
        if (submitted) pushRecent(submitted);

        if (!input || !list) return;

        function renderRecents() {
            const recents = getRecents();
            list.innerHTML = '';
            if (recents.length === 0) {
                list.classList.remove('is-open');
                return;
            }
            recents.forEach(city => {
                const li = document.createElement('li');
                li.innerHTML = `<i class="bi bi-clock-history"></i><span>${city}</span>`;
                li.addEventListener('mousedown', e => {
                    e.preventDefault();
                    input.value = city;
                    form.submit();
                });
                list.appendChild(li);
            });
            list.classList.add('is-open');
        }

        input.addEventListener('focus', renderRecents);
        input.addEventListener('blur', () => setTimeout(() => list.classList.remove('is-open'), 150));
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
        initRecents();
        initCountUp();
        initDailySelector();
        spawnEffect();
    });
})();
