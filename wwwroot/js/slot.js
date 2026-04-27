// ============================
// DATE RESTRICTION
// ============================

const dateInput = document.getElementById("dateInput");

if (dateInput) {

    let now = new Date();

    // Remove seconds & milliseconds
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

// ============================
// SLOT SELECTION
// ============================

let selected = [];

document.addEventListener("click", async function (e) {

    if (e.target.classList.contains("slot-btn") && !e.target.disabled) {

        const btn = e.target;
        const id = btn.dataset.id;
        const rate = parseFloat(btn.dataset.rate);

        // 🔥 IF ALREADY SELECTED → UNSELECT (NO LOCK CALL)
        if (selected.find(x => x.id == id)) {

            selected = selected.filter(x => x.id != id);
            btn.classList.remove("btn-success");
            btn.classList.add("btn-outline-success");

        } else {

            // 🔥 LOCK ONLY WHEN SELECTING
            const date = document.getElementById("dateInput").value;
            const duration = document.getElementById("durationInput").value;

            const response = await fetch('/Slot/LockSlot', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    slotId: parseInt(id),
                    startTime: date,
                    duration: parseInt(duration)
                })
            });

            if (!response.ok) {
                alert("Slot already selected by another user!");
                return;
            }

            selected.push({ id: id, rate: rate });
            btn.classList.remove("btn-outline-success");
            btn.classList.add("btn-success");
        }

        document.getElementById("countDisplay").innerText = selected.length;

        calculateTotal();
    }
});

// ============================
// CALCULATE TOTAL
// ============================

function calculateTotal() {

    const durationInput = document.getElementById("durationInput");
    const duration = parseInt(durationInput.value);

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

// ============================
// BOOK BUTTON
// ============================

function bookSlots() {

    let isLoggedIn = document.getElementById("authData").getAttribute("data-auth");

    if (isLoggedIn !== "true") {
        alert("Please login first!");
        return;
    }

    const MAX_ALLOWED = 5;

    if (selected.length > MAX_ALLOWED) {
        alert("You can select maximum 5 slots only!");
        return;
    }

    // ============================
    // DATE VALIDATION (FIXED)
    // ============================

    if (!dateInput || !dateInput.value) {
        alert("Please select date and time!");
        return;
    }

    const selectedDate = new Date(dateInput.value);
    const nowDate = new Date();

    let maxDate = new Date();
    maxDate.setDate(nowDate.getDate() + 4);

    if (isNaN(selectedDate.getTime())) {
        alert("Invalid date selected!");
        return;
    }

    if (selectedDate < nowDate || selectedDate > maxDate) {
        alert("Please select a valid date within 4 days!");
        return;
    }

    // ============================
    // DURATION VALIDATION
    // ============================

    const durationInput = document.getElementById("durationInput");
    const duration = durationInput.value;

    if (!duration || duration <= 0) {
        alert("Enter valid duration!");
        return;
    }

    if (selected.length === 0) {
        alert("Select at least one slot!");
        return;
    }

    // ============================
    // FINAL REDIRECT (FIXED)
    // ============================

    const ids = selected.map(s => s.id).join(',');
    const date = dateInput.value;

    console.log("Selected Date:", date); // DEBUG

    // 🔥 IMPORTANT FIX: encode date
    window.location.href =
        `/Booking/Checkout?slots=${ids}&duration=${duration}&date=${encodeURIComponent(date)}`;
}
function startTimers() {

    if (!lockTimes || lockTimes.length === 0) {
        console.log("No lock data found");
        return;
    }

    setInterval(() => {

        const now = new Date().getTime();

        lockTimes.forEach(lock => {

            console.log("LOCK:", lock); // 🔥 DEBUG

            const expire = new Date(lock.expireTime).getTime();
            const remaining = expire - now;

            const el = document.getElementById("timer-" + lock.slotId);

            if (!el) {
                console.log("No element for:", lock.slotId);
                return;
            }

            if (remaining <= 0) {
                el.innerText = "";
                return;
            }

            const minutes = Math.floor(remaining / 60000);
            const seconds = Math.floor((remaining % 60000) / 1000);

            el.innerText = `⏳ ${minutes}:${seconds.toString().padStart(2, '0')}`;
        });

    }, 1000);
}

startTimers();