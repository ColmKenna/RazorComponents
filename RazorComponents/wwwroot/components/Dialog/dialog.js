// Dialog JS for reusable dialogs by dialog id

// Event name constants
const DIALOG_CONFIRMED_EVENT = 'dialogConfirmed';
const DIALOG_CANCELLED_EVENT = 'dialogCancelled';

function setupDialog(openBtn) {
  const dialogId = openBtn.dataset.dialogId;
  if (dialogId) {
    const dialog = document.getElementById(dialogId);
    if (dialog) {
      openBtn.addEventListener('click', () => handleOpenDialog(dialog));
      
      const cancelBtns = dialog.querySelectorAll('.cancelBtn');
      cancelBtns.forEach(btn => btn.addEventListener('click', () => handleCancel(dialog, btn)));
      
      const confirmBtns = dialog.querySelectorAll('.confirmBtn');
      confirmBtns.forEach(btn => btn.addEventListener('click', () => handleConfirm(dialog, btn)));
    }
  }
}

function handleOpenDialog(dialog) {
  dialog.showModal();
}

function handleCancel(dialog, btn) {
  const inputIds = btn.dataset.inputIds ? btn.dataset.inputIds.split(',') : [];
  // Collect input values before clearing
  const values = {};
  inputIds.forEach(id => {
    const input = document.getElementById(id);
    if (input) {
      values[id] = input.value;
    }
  });
  
  dialog.close();
  clearInputs(dialog);
  
  // Dispatch event with values
  const event = new CustomEvent(DIALOG_CANCELLED_EVENT, {
    detail: {
      dialogId: dialog.id,
      values: values
    }
  });
  dialog.dispatchEvent(event);
}

function handleConfirm(dialog, btn) {
  const inputIds = btn.dataset.inputIds ? btn.dataset.inputIds.split(',') : [];
  confirmedClicked(dialog, inputIds);
}

function confirmedClicked(dialog, inputIds) {
  const values = {};
  inputIds.forEach(id => {
    const input = document.getElementById(id);
    if (input) {
      values[id] = input.value;
    }
  });
  
  dialog.close();
  clearInputs(dialog);
  
  // Dispatch event with values
  const event = new CustomEvent(DIALOG_CONFIRMED_EVENT, {
    detail: {
      dialogId: dialog.id,
      values: values
    }
  });
  dialog.dispatchEvent(event);
}

function clearInputs(dialog) {
  const inputs = dialog.querySelectorAll('input');
  inputs.forEach(input => {
    input.value = '';
  });
}

// Initialize all dialogs once the DOM is fully loaded
document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('[data-dialog-id]').forEach(setupDialog);
});

// For testability (CommonJS environment)
if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    setupDialog,
    handleOpenDialog,
    handleCancel,
    handleConfirm,
    confirmedClicked,
    clearInputs
  };
}
