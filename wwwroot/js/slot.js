let selectedSlots = [];
let maxSlots = 1; // default value

// Set max slots dynamically from Razor
function setMaxSlots(value) {
    maxSlots = value;
}

function selectSlot(element, slotId) {

    if (selectedSlots.includes(slotId)) {
        // Remove slot
        selectedSlots = selectedSlots.filter(id => id !== slotId);
        element.classList.remove("selected-slot");
    } else {

        if (selectedSlots.length >= maxSlots) {
            alert("You can only select " + maxSlots + " slots");
            return;
        }

        selectedSlots.push(slotId);
        element.classList.add("selected-slot");
    }

    // Update hidden input
    document.getElementById("SelectedSlots").value = selectedSlots.join(",");
}