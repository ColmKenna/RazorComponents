document.addEventListener('DOMContentLoaded', () => {
    const openButtons = document.querySelectorAll('[data-ck-tab-target]');

    openButtons.forEach(button => {
        const targetDialogId = button.getAttribute('data-ck-tab-target');
        const currentTarget = document.getElementById(targetDialogId);

        // Clone the button and set up its event listener
        const clonedButton = createClonedButton(button, currentTarget);
        currentTarget.insertAdjacentElement('beforebegin', clonedButton);

        // Set up the original button's event listener
        button.addEventListener('click', () => {
            openTab(button, targetDialogId);
        });
    });

    // Trigger the first button's click to open the initial tab
    if (openButtons.length > 0) openButtons[0].click();
});

/**
 * Creates a cloned button with the same behavior as the original.
 * @param {HTMLElement} button - The original button to clone.
 * @param {HTMLElement} target - The target element associated with the button.
 * @returns {HTMLElement} - The cloned button.
 */
function createClonedButton(button, target) {
    const clonedButton = button.cloneNode(true);
    clonedButton.addEventListener('click', () => {
        if (target.style.display === 'block') {
            target.style.display = 'none';
        } else {
            openTab(button, target.id);
        }
    });
    return clonedButton;
}

/**
 * Opens a tab and updates the display of related content and links.
 * @param {HTMLElement} button - The button triggering the tab open.
 * @param {string} tabId - The ID of the tab to display.
 */
function openTab(button, tabId) {
    const tabContainer = button.closest('.tab-container');

    // Hide all tab content
    const tabContents = tabContainer.getElementsByClassName("tab");
    Array.from(tabContents).forEach(tabContent => {
        tabContent.style.display = "none";
    });

    // Remove "active" class from all tab links
    const tabLinks = tabContainer.getElementsByClassName("tab-link");
    Array.from(tabLinks).forEach(tabLink => {
        tabLink.classList.remove("active");
    });

    // Show the current tab and mark the button as active
    const targetTab = tabContainer.querySelector(`#${tabId}`);
    if (targetTab) {
        targetTab.style.display = "block";
    }
    button.classList.add("active");
}