function addItem(button) {

    var ds = button.getAttribute('data-source');
    var targetListId = button.getAttribute('data-target-ul');
    var addInput = button.getAttribute('data-input-id');
    var templateId = button.getAttribute('data-template-id');

    var list = document.getElementById(targetListId);
    
    var input = document.getElementById(addInput);
    var newValue = input.value.trim();

    if (newValue === "") {
        alert("Please enter a value for the new item.");
        return;
    }

    var index = list.children.length;
    var li = document.createElement('li');
    
    // Get the template content
    var template = document.getElementById(templateId);
    var templateContent = template.content.cloneNode(true);

    var inputs = templateContent.querySelectorAll('input');
    // Set the name attributes for the inputs
    inputs.forEach(function (input) {
        var name = input.name.replace(/\[0\]/g, `[${index}]`);
        input.name = name;
        input.value = newValue;
    });

    // Update the template content with the new values
    var span = templateContent.querySelector('.item-label');
    span.textContent = newValue;



    // Add the template content to the new li element
    li.appendChild(templateContent);
    list.appendChild(li);

    // Clear the input box after adding the item
    input.value = "";
}

function toggleEdit(button) {
    var li = button.parentElement;
    var label = li.querySelector('.item-label');
    var input = li.querySelector('.item-input');

    if (input.style.display === 'none') {
        input.style.display = 'inline';
        label.style.display = 'none';
        button.innerHTML = '<i class="fas fa-save"></i> Save';
    } else {
        label.textContent = input.value;
        input.style.display = 'none';
        label.style.display = 'inline';
        button.innerHTML = '<i class="fas fa-edit"></i> Edit';
    }
}

function markAsRemoved(button) {
    var li = button.parentElement;
    var input = li.querySelector('input[type="text"]');
    var deletedInput = li.querySelector('input[type="hidden"]');
    var editButton = li.querySelector('.edit-btn');

    if (li.classList.contains('removed')) {
        // Undo remove
        li.classList.remove('removed');
        button.classList.remove('undo');
        deletedInput.value = "";
        button.innerHTML = '<i class="fas fa-trash"></i> Remove';

        if (editButton) {
            editButton.disabled = false;
        }
    } else {
        // Mark as removed
        li.classList.add('removed');
        button.classList.add('undo');
        deletedInput.value = input.value;
        button.innerHTML = '<i class="fas fa-undo"></i> Undo';

        if (editButton) {
            editButton.disabled = true;
        }
    }
}
