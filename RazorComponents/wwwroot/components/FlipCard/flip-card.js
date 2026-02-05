/**
 * Flip Card Component JavaScript
 * Handles flip functionality and auto-height equalization.
 */
(function () {
    'use strict';

    /**
     * Equalizes heights for auto-height cards so both faces match.
     */
    function equalizeCardHeights() {
        document.querySelectorAll('.card.auto-height').forEach(function (card) {
            var front = card.querySelector('.card-front');
            var back = card.querySelector('.card-back');

            if (!front || !back) return;

            // Reset heights to get natural heights
            card.style.setProperty('--card-height', 'auto');

            // Force a reflow to get accurate measurements
            front.style.height = 'fit-content';
            back.style.height = 'fit-content';

            // Get the natural heights
            var frontHeight = front.offsetHeight;
            var backHeight = back.offsetHeight;

            // Set the maximum height as a CSS custom property
            var maxHeight = Math.max(frontHeight, backHeight) + 'px';
            card.style.setProperty('--card-height', maxHeight);
        });
    }

    /**
     * Initializes flip button event listeners.
     */
    function initializeFlipButtons() {
        document.querySelectorAll('.rotate-button').forEach(function (button) {
            // Prevent duplicate listeners
            if (button.dataset.initialized) return;
            button.dataset.initialized = 'true';

            button.addEventListener('click', function (event) {
                event.stopPropagation();
                var card = this.closest('.card');
                if (card) {
                    card.classList.toggle('is-flipped');
                }
            });
        });
    }

    /**
     * Main initialization function.
     */
    function initialize() {
        equalizeCardHeights();
        initializeFlipButtons();
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

    // Re-equalize on window resize (debounced)
    var resizeTimeout;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(equalizeCardHeights, 100);
    });

    // Expose for dynamic content scenarios
    window.FlipCard = {
        initialize: initialize,
        equalizeHeights: equalizeCardHeights
    };
})();