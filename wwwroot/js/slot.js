//console.log("JS LOADED");

// Date restricting
const dateInput = document.getElementById("dateInput");

if (dateInput) {

    let now = new Date();

    // remove seconds & milliseconds
    now.setSeconds(0);
    now.setMilliseconds(0);

    let max = new Date();
    max.setDate(now.getDate() + 4);

    function formatDate(date) {
        return date.toISOString().slice(0, 16);
    }

    dateInput.min = formatDate(now);
    dateInput.max = formatDate(max);
}

let selected = [];

// SLOT CLICK
document.addEventListener("click", function (e) {
    if (e.target.classList.contains("slot-btn")) {

        const btn = e.target;
        const id = btn.dataset.id;
        const rate = parseFloat(btn.dataset.rate);

        if (selected.find(x => x.id == id)) {
            selected = selected.filter(x => x.id != id);
            btn.classList.remove("btn-success");
            btn.classList.add("btn-outline-success");
        } else {
            selected.push({ id: id, rate: rate });
            btn.classList.remove("btn-outline-success");
            btn.classList.add("btn-success");
        }

        document.getElementById("countDisplay").innerText = selected.length;

        calculateTotal();
    }
});

// CALCULATE TOTAL
function calculateTotal() {

    const duration = parseInt(document.getElementById("durationInput").value);

    if (isNaN(duration) || duration <= 0) {
        document.getElementById("totalAmount").innerText = "৳ 0";
        return;
    }

    let total = 0;

    selected.forEach(s => {
        total += s.rate * duration;
    });

    document.getElementById("totalAmount").innerText = "৳ " + total.toFixed(2);
}

// BOOK BUTTON
function bookSlots() {
    let isLoggedIn = document.getElementById("authData").getAttribute("data-auth");

    if (isLoggedIn !== "true") {
        alert("Please login first!");
        //window.location.href = "/Slot/Availability";
        return;
    }
    const MAX_ALLOWED = 5;

    if (selected.length > MAX_ALLOWED) {
        alert("You can select maximum 5 slots only!");
        return;
    }
    const selectedDate = new Date(dateInput.value);
    const nowDate = new Date();

    let maxDate = new Date();
    maxDate.setDate(nowDate.getDate() + 4);

    if (selectedDate < nowDate || selectedDate > maxDate) {
        alert("Please select a valid date within 4 days!");
        return;
    }


    const duration = document.getElementById("durationInput").value;

    if (!duration || duration <= 0) {
        alert("Enter valid duration!");
        return;
    }

    if (selected.length === 0) {
        alert("Select at least one slot!");
        return;
    }

    const ids = selected.map(s => s.id).join(',');

    window.location.href = `/Booking/Checkout?slots=${ids}&duration=${duration}`;
}