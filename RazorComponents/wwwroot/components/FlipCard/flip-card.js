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
        document.querySelectorAll('.flip-card .card.auto-height').forEach(function (card) {
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
     * Initializes flip button event listeners using data attributes.
     */
    function initializeFlipButtons() {
        document.querySelectorAll('.flip-card [data-flip-card-button]').forEach(function (button) {
            // Prevent duplicate listeners
            if (button.dataset.initialized) return;
            button.dataset.initialized = 'true';

            button.addEventListener('click', function (event) {
                event.stopPropagation();
                var card = this.closest('.card');
                if (card) {
                    var isFlipped = card.classList.toggle('is-flipped');

                    // Update ARIA states (WARNING-2)
                    var front = card.querySelector('.card-front');
                    var back = card.querySelector('.card-back');
                    if (front) front.setAttribute('aria-hidden', isFlipped.toString());
                    if (back) back.setAttribute('aria-hidden', (!isFlipped).toString());

                    card.querySelectorAll('[data-flip-card-button]')
                        .forEach(function (b) {
                            b.setAttribute('aria-pressed', isFlipped.toString());
                        });
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
