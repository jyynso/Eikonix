// Theme Toggle Functionality for Eikonix Platform
// Supports dark mode and light mode across all pages

(function () {
    'use strict';

    // Get saved theme from localStorage or default to 'light'
    const currentTheme = localStorage.getItem('theme') || 'light';

    // Apply theme on page load IMMEDIATELY (before DOM loads)
    if (currentTheme === 'dark') {
        document.documentElement.classList.add('dark-mode');
        if (document.body) {
            document.body.classList.add('dark-mode');
        }
    }

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function () {
        // Apply theme to body if not already applied
        if (currentTheme === 'dark' && !document.body.classList.contains('dark-mode')) {
            document.body.classList.add('dark-mode');
        }

        // Create theme toggle button if it doesn't exist
        let themeToggle = document.querySelector('.theme-toggle');
        if (!themeToggle) {
            themeToggle = createThemeToggle();
        }

        // Add click event to theme toggle button
        if (themeToggle) {
            // Remove any existing event listeners by cloning the button
            const newThemeToggle = themeToggle.cloneNode(true);
            themeToggle.parentNode.replaceChild(newThemeToggle, themeToggle);
            themeToggle = newThemeToggle;
            
            themeToggle.addEventListener('click', toggleTheme);
        }

        // Update icon based on current theme
        updateThemeIcon();
    });

    // Create theme toggle button
    function createThemeToggle() {
        const toggleButton = document.createElement('button');
        toggleButton.className = 'theme-toggle';
        toggleButton.setAttribute('aria-label', 'Toggle dark mode');
        toggleButton.innerHTML = '<i class="bx bx-moon"></i>';
        document.body.appendChild(toggleButton);
        return toggleButton;
    }

    // Toggle theme function
    function toggleTheme(e) {
        if (e) e.preventDefault();
        
        const body = document.body;
        const isDarkMode = body.classList.contains('dark-mode');

        if (isDarkMode) {
            // Switch to light mode
            body.classList.remove('dark-mode');
            localStorage.setItem('theme', 'light');
        } else {
            // Switch to dark mode
            body.classList.add('dark-mode');
            localStorage.setItem('theme', 'dark');
        }

        // Update icon
        updateThemeIcon();

        // Add subtle animation
        body.style.transition = 'background-color 0.3s ease, color 0.3s ease';
    }

    // Update theme icon
    function updateThemeIcon() {
        const themeToggle = document.querySelector('.theme-toggle');
        if (!themeToggle) return;

        const icon = themeToggle.querySelector('i');
        if (!icon) return;

        const isDarkMode = document.body.classList.contains('dark-mode');

        if (isDarkMode) {
            icon.className = 'bx bx-sun';
            themeToggle.setAttribute('aria-label', 'Switch to light mode');
        } else {
            icon.className = 'bx bx-moon';
            themeToggle.setAttribute('aria-label', 'Switch to dark mode');
        }
    }

    // Listen for system theme changes (optional)
    if (window.matchMedia) {
        const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
        
        darkModeQuery.addEventListener('change', function (e) {
            // Only apply if user hasn't manually set a preference
            if (!localStorage.getItem('theme')) {
                if (e.matches) {
                    document.body.classList.add('dark-mode');
                } else {
                    document.body.classList.remove('dark-mode');
                }
                updateThemeIcon();
            }
        });
    }
})();
