// ============================
// 🔥 SIGNALR + INIT
// ============================

document.addEventListener("DOMContentLoaded", function () {

    const currentUserId = document
        .getElementById("authData")
        .getAttribute("data-userid");

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/slotHub")
        .build();

    connection.start()
        .then(() => console.log("SignalR Connected"))
        .catch(err => console.error(err));

    // 🔥 RECEIVE LIVE UPDATES
    connection.on("ReceiveSlotUpdate", function (data) {

        const btn = document.querySelector(`[data-id='${data.slotId}']`);
        if (!btn) return;
        const area = btn.dataset.area;

        // 🔥 Don't override your own selected slot
        if (btn.classList.contains("selected-slot") && data.status === "locked") return;

        if (data.status === "locked") {

            if (data.userId === currentUserId) return;

            // 🔥 ONLY VISUAL CHANGE
            btn.classList.add("border-warning");
            btn.disabled = true;
            updateCount(area, -1);
        }

        if (data.status === "available") {

            btn.classList.remove("border-warning");
            btn.disabled = false;

            updateCount(area, +1);
        }
    });

});


// ============================
// DATE RESTRICTION
// ============================

const dateInput = document.getElementById("dateInput");

if (dateInput) {
    let now = new Date();
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

    const btn = e.target.closest(".slot-btn");
    if (!btn || btn.style.pointerEvents === "none") return;

    const auth = document.getElementById("authData");

    if (auth.getAttribute("data-auth") !== "true") {
        Swal.fire({ icon: 'warning', title: 'Authentication Required', text: 'Please login first to select any slot', confirmButtonColor: '#2a2a72' });
        return;
    }

    

    const id = btn.dataset.id;
    const rate = parseFloat(btn.dataset.rate);
    const area = btn.dataset.area;

    // UNSELECT
    if (selected.find(x => x.id == id)) {

        selected = selected.filter(x => x.id != id);
        btn.classList.remove("selected-slot");
        updateCount(area, +1);
        
    } else {

        const date = document.getElementById("dateInput").value;
        const duration = document.getElementById("durationInput").value;

        const response = await fetch('/Slot/LockSlot', {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                slotId: parseInt(id),
                startTime: date,
                duration: parseInt(duration)
            })
        });

        selected.push({ id: id, rate: rate });
        btn.classList.add("selected-slot");
        const area = btn.dataset.area;
        updateCount(area, -1);
    }

    document.getElementById("countDisplay").innerText = selected.length;

    if (selected.length > 5) {
        Swal.fire({
            icon: 'error',
            title: 'Limit Exceeded',
            text: 'Maximum 5 slots allowed!',
            confirmButtonColor: '#ff007f'
        }).then(() => location.reload());
        return;
    }

    calculateTotal();
});


// ============================
// CALCULATE TOTAL
// ============================

function calculateTotal() {

    const duration = parseInt(document.getElementById("durationInput").value);

    if (!duration || duration <= 0) {
        document.getElementById("totalAmount").innerText = "৳ 0";
        return;
    }

    let total = selected.reduce((sum, s) => sum + s.rate * duration, 0);

    document.getElementById("totalAmount").innerText = "৳ " + total.toFixed(2);
}


// ============================
// BOOK BUTTON
// ============================

function bookSlots() {

    const auth = document.getElementById("authData");

    if (auth.getAttribute("data-auth") !== "true") {
        Swal.fire({ icon: 'warning', title: 'Oops...', text: 'Please login first!', confirmButtonColor: '#2a2a72' });
        return;
    }

    if (selected.length === 0) {
        Swal.fire({ icon: 'info', title: 'No Slots Selected', text: 'Please select at least one slot before continuing.', confirmButtonColor: '#009ffd' });
        return;
    }

    const date = dateInput.value;
    const duration = document.getElementById("durationInput").value;

    const ids = selected.map(s => s.id).join(',');

    window.location.href =
        `/Booking/Checkout?slots=${ids}&duration=${duration}&date=${encodeURIComponent(date)}`;
}

//Set Intervals 
setInterval(async () => {

    const auth = document.getElementById("authData");
    if (!auth || auth.getAttribute("data-auth") !== "true") return;

    try {
        // 🔥 CHECK LOCK STATUS FROM SERVER
        const res = await fetch('/Slot/CheckMyLocks');
        const data = await res.json();

        // 🔥 IF USER LOCK EXPIRED → RESET UI
        if (!data.active && selected.length > 0) {
            Swal.fire({
                icon: 'error',
                title: 'Time Expired',
                text: 'Your slot hold has expired!',
                confirmButtonColor: '#ff007f'
            }).then(() => {
                selected = [];
                location.reload();
            });
            return;
        }

    } catch (err) {
        console.error("CheckMyLocks error:", err);
    }

    // 🔥 TIMER DISPLAY
    if (!window.lockTimes || lockTimes.length === 0) return;

    const now = new Date().getTime();

    lockTimes.forEach(lock => {

        const el = document.getElementById("timer-" + lock.slotId);
        if (!el) return;

        const expire = new Date(lock.expireTime).getTime();
        const remaining = expire - now;

        if (remaining <= 0) {
            el.innerText = "";
            return;
        }

        const minutes = Math.floor(remaining / 60000);
        const seconds = Math.floor((remaining % 60000) / 1000);

        el.innerText = `⏳ ${minutes}:${seconds.toString().padStart(2, '0')}`;
    });

}, 1000);

// =========================
// UPDATE AVAILABLE COUNT
// =========================
function updateCount(area, change) {

    const el = document.getElementById("count-" + area);

    if (!el) return;

    let current = parseInt(el.innerText.replace(/\D/g, ''));

    current += change;

    if (current < 0) current = 0;

    el.innerText = "Available: " + current;
}