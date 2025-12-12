/**
 * Add a new item to an edit-array container
 * @param {string} containerId - The ID of the edit-array container
 * @param {string} templateId - The ID of the template to clone
 */
function addNewItem(containerId, templateId) {
    const container = document.getElementById(containerId + '-items');
    const template = document.getElementById(templateId);
    const clone = template.content.cloneNode(true);

    // Get current count of items to use as new index
    const newIndex = container.children.length;

    // Replace '__index__' with the actual index in all input names and ids
    const allInputs = clone.querySelectorAll('*');
    allInputs.forEach(input => {
        if (input.name) {
            input.name = input.name.replace('__index__', newIndex);
        }
        if (input.id) {
            input.id = input.id.replace('__index__', newIndex);
        }
        
        // check if it has an attribute 'data-id'
        if (input.hasAttribute('data-id')) {
            const newValue = input.getAttribute('data-id').replace('__index__', newIndex);
            input.setAttribute('data-id', newValue);
        }
        
        // check if it has an attribute 'data-display-for'
        if (input.hasAttribute('data-display-for')) {
            const newValue = input.getAttribute('data-display-for').replace('__index__', newIndex);
            input.setAttribute('data-display-for', newValue);
        }
    });

    // Replace '__index__' in labels' for attribute
    const allLabels = clone.querySelectorAll('label[for]');
    allLabels.forEach(label => {
        if (label.htmlFor) {
            label.htmlFor = label.htmlFor.replace('__index__', newIndex);
        }
    });

    // Replace '__index__' in validation message elements
    const allValidationElements = clone.querySelectorAll('[data-valmsg-for]');
    allValidationElements.forEach(element => {
        if (element.getAttribute('data-valmsg-for')) {
            const newValue = element.getAttribute('data-valmsg-for').replace('__index__', newIndex);
            element.setAttribute('data-valmsg-for', newValue);
        }
    });

    

    // Set the ID for the new item
    const itemDiv = clone.querySelector('.edit-array-item');
    const itemId = `${containerId}-item-${newIndex}`;
    if (itemDiv) {
  
        itemDiv.id = itemId;

        // Update onclick handlers with the new ID
        const editBtn = itemDiv.querySelector('.edit-item-btn');
        if (editBtn) {
            editBtn.setAttribute('onclick', `toggleEditMode('${itemId}')`);
        }

        const doneBtn = itemDiv.querySelector('.done-edit-btn');
        if (doneBtn) {
            doneBtn.setAttribute('onclick', `toggleEditMode('${itemId}')`);
        }

        const deleteBtn = itemDiv.querySelector('.delete-item-btn');
        if (deleteBtn) {
            deleteBtn.setAttribute('onclick', `markForDeletion('${itemId}')`);
        }

        // For newly added items in display mode, show edit container first and hide display container
        const displayContainer = itemDiv.querySelector('.display-container');
        // set the id on the displayContainer
        displayContainer.id = `${itemId}-display`;
        const editContainer = itemDiv.querySelector('.edit-container');
        editContainer.id = `${itemId}-edit`;
        if (displayContainer && editContainer) {
            displayContainer.style.display = 'none';
            editContainer.style.display = 'block';
        }

        const hiddenInput = document.createElement('input');
        hiddenInput.type = 'hidden';
        hiddenInput.name = `__newItem__${newIndex}`;
        hiddenInput.value = 'true';
        hiddenInput.setAttribute('data-new-item-marker', 'true');
        hiddenInput.setAttribute('data-id', `__newItem__${newIndex}`);
        hiddenInput.setAttribute('data-display-for', `__newItem__${newIndex}`);

        // Append the hidden input to the edit container
        editContainer.appendChild(hiddenInput);
    }
    


    container.appendChild(clone);
    
    // disable the add button
    const addButton = document.getElementById(containerId + '-add');
    if (addButton) {
        addButton.disabled = true;
    }

    // add a cancel button to the new item
    const cancelButton = document.createElement('button');
    cancelButton.type = 'button';
    cancelButton.textContent = 'Cancel';
    cancelButton.className = 'btn btn-secondary';
    
    cancelButton.setAttribute('onclick', `markForDeletion('${itemId}')`);
    cancelButton.setAttribute('data-new-item-marker', 'true');
    cancelButton.setAttribute('data-cancel', `cancel`);
    
    const editContainer = itemDiv.querySelector('.edit-container');
    if (editContainer) {
        editContainer.appendChild(cancelButton);
    }
    
    // Re-parse validation for the new elements if jQuery validation is available
    if (typeof $.validator !== 'undefined' && $.validator.unobtrusive) {
        var $form = $(container.closest('form'));
        $form.removeData('validator');
        $form.removeData('unobtrusiveValidator');
        $.validator.unobtrusive.parse($form);
        // Ensure blur validation is attached to all inputs in the container
        $(container).find('input, select, textarea').off('blur.validate').on('blur.validate', function () {
            $(this).valid();
        });
    }
}

/**
 * Toggle between display and edit modes for an item
 * @param {string} itemId - The ID of the item to toggle
 */
function toggleEditMode(itemId) {
    const item = document.getElementById(itemId);
    if (!item) return;

    const containerId = getContainerIdFromItemId(itemId);
    const displayContainer = document.getElementById(`${itemId}-display`);
    const editContainer = document.getElementById(`${itemId}-edit`);

    if (displayContainer && editContainer) {
        if (displayContainer.style.display === 'none') {
            // Update display with current values
            updateDisplayFromForm(itemId);

            // Switch from edit to display
            displayContainer.style.display = 'block';
            editContainer.style.display = 'none';
        } else {
            // Switch from display to edit
            displayContainer.style.display = 'none';
            editContainer.style.display = 'block';
        }
    }
    // re-enable the add button
    if (containerId) {
        const addButton = document.getElementById(containerId + '-add');
        if (addButton) {
            addButton.disabled = false;
        }
    }

    const cancelButton = item.querySelector('button[data-cancel]');
    if (cancelButton) {
        cancelButton.remove();
    }
    
    
    // remove the hidden input for new items if it exists
    const hiddenInput = item.querySelector('input[data-new-item-marker]');
    if (hiddenInput) {
        hiddenInput.remove();
    }
}

function markForDeletion(itemId) {
    const item = document.getElementById(itemId);
    const deleteButton = item.querySelector('.delete-item-btn');
    const editButton = item.querySelector('.edit-item-btn');
    const isDeletedInput = item.querySelector('input[data-is-deleted-marker]');
    
    // Find the container by looking for parent with id ending in '-items' or containing 'edit-array'
    let container = item.closest('.edit-array-container');
    if (!container) {
        // Fallback: look for parent with id containing 'items'
        let current = item.parentElement;
        while (current) {
            if (current.id && current.id.includes('-items')) {
                container = current.parentElement;
                break;
            }
            current = current.parentElement;
        }
    }
    
    const containerId = container ? container.id : null;
    const newItemInput = item.querySelector('input[data-new-item-marker]');

    // don't mark for deletion, remove it all together
    if (newItemInput) {
        item.remove();
        if (containerId) {
            const addButton = document.getElementById(containerId + '-add');
            if (addButton) {
                addButton.disabled = false;
            }
        }
        return;
    }
    
    // remove the cancel button if it exists


    if (item) {
        if (item.getAttribute('data-deleted') === 'true') {
            // Undo deletion
            item.removeAttribute('data-deleted');
            item.classList.remove('deleted');
            if (isDeletedInput) {
                isDeletedInput.value = 'false';
            }
            if (deleteButton) {
                deleteButton.textContent = 'Delete';
            }
            if (editButton) {
                editButton.disabled = false;
            }
        } else {
            // Mark as deleted
            item.setAttribute('data-deleted', 'true');
            item.classList.add('deleted');
            if (isDeletedInput) {
                isDeletedInput.value = 'true';
            }
            if (deleteButton) {
                deleteButton.textContent = 'Undelete';
            }
            if (editButton) {
                editButton.disabled = true;
            }
        }
    }
}

/**
 * Update display values based on the current form input values
 * @param {string} itemId - The ID of the item to update
 */
function updateDisplayFromForm(itemId) {
    const item = document.getElementById(itemId);
    if (!item) return;

    const displayContainer = document.getElementById(`${itemId}-display`);
    const editContainer = document.getElementById(`${itemId}-edit`);

    if (displayContainer && editContainer) {
        const inputs = editContainer.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            const displayElement = displayContainer.querySelector(`[data-display-for="${input.id}"]`);
            if (displayElement) {
                displayElement.textContent = input.value;
            }
        });
    }
}

function moveItem(containerId, itemId, offset) {
    const container = document.getElementById(`${containerId}-items`);
    const item = document.getElementById(itemId);
    if (!container || !item || offset === 0) return;

    const items = Array.from(container.querySelectorAll('.edit-array-item'));
    const currentIndex = items.indexOf(item);
    if (currentIndex === -1) return;

    const targetIndex = currentIndex + offset;
    if (targetIndex < 0 || targetIndex >= items.length) return;

    const referenceItem = items[targetIndex];
    const insertBeforeNode = offset > 0 ? referenceItem.nextSibling : referenceItem;
    container.insertBefore(item, insertBeforeNode);

    renumberItems(containerId);
}

function renumberItems(containerId) {
    const container = document.getElementById(`${containerId}-items`);
    if (!container) return;

    const items = Array.from(container.querySelectorAll('.edit-array-item'));
    items.forEach((item, newIndex) => {
        const oldId = item.id;
        const newId = `${containerId}-item-${newIndex}`;

        updateAttributeWithIndex(item, 'id', newIndex, oldId, newId);

        const descendants = item.querySelectorAll('*');
        descendants.forEach(child => {
            updateAttributeWithIndex(child, 'id', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'name', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'for', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-id', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-display-for', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-valmsg-for', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'data-new-item-marker', newIndex, oldId, newId);
            updateAttributeWithIndex(child, 'aria-describedby', newIndex, oldId, newId);

            const onclick = child.getAttribute('onclick');
            if (onclick && oldId) {
                const updatedOnclick = onclick.replaceAll(oldId, newId);
                if (updatedOnclick !== onclick) {
                    child.setAttribute('onclick', updatedOnclick);
                }
            }
        });
    });
}

function updateAttributeWithIndex(element, attributeName, newIndex, oldId, newId) {
    if (!element) return;

    const currentValue = attributeName === 'for'
        ? element.htmlFor || element.getAttribute('for')
        : element.getAttribute(attributeName);

    if (!currentValue) return;

    const updatedValue = replaceIndexTokens(currentValue, newIndex, oldId, newId);
    if (updatedValue === currentValue) return;

    if (attributeName === 'for') {
        element.htmlFor = updatedValue;
    } else if (attributeName === 'id') {
        element.id = updatedValue;
    } else {
        element.setAttribute(attributeName, updatedValue);
    }
}

function replaceIndexTokens(value, newIndex, oldId, newId) {
    let updated = value;
    if (oldId && newId && updated.includes(oldId)) {
        updated = updated.replaceAll(oldId, newId);
    }

    updated = updated.replace(/\[\d+\]/g, `[${newIndex}]`);
    updated = updated.replace(/_(\d+)__/g, `_${newIndex}__`);
    updated = updated.replace(/__newItem__\d+/g, `__newItem__${newIndex}`);
    updated = updated.replace(/-item-\d+/g, `-item-${newIndex}`);

    return updated;
}

function getContainerIdFromItemId(itemId) {
    if (!itemId) return null;
    const match = itemId.match(/^(.*)-item-\d+$/);
    return match && match[1] ? match[1] : null;
}

