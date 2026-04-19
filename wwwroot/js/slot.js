let selected = [];

const input = document.getElementById("dateInput");

if (input) {
    let now = new Date();
    now.setSeconds(0);
    now.setMilliseconds(0);

    let max = new Date();
    max.setDate(now.getDate() + 4);

    function formatDate(date) {
        return date.toISOString().slice(0, 16);
    }

    input.min = formatDate(now);
    input.max = formatDate(max);
}

document.querySelectorAll('.slot-btn').forEach(btn => {
    btn.addEventListener('click', function () {

        const id = this.dataset.id;
        const rate = parseFloat(this.dataset.rate);

        if (selected.find(x => x.id == id)) {
            selected = selected.filter(x => x.id != id);
            this.classList.remove("btn-success");
            this.classList.add("btn-outline-success");
        } else {
            selected.push({ id: id, rate: rate });
            this.classList.remove("btn-outline-success");
            this.classList.add("btn-success");
        }

        document.getElementById("countDisplay").innerText = selected.length;

        const durationInput = document.getElementById("DurationHours").value;
        const duration = parseInt(durationInput);

        if (isNaN(duration) || duration <= 0) {
            document.getElementById("totalAmount").innerText = "৳ 0";
            return;
        }

        let total = 0;

        selected.forEach(s => {
            const rate = parseFloat(s.rate);

            if (!isNaN(rate)) {
                total += rate * duration;
            }
        });

        document.getElementById("totalAmount").innerText = "৳ " + total.toFixed(2);
    });
});

function bookSlots() {

    let isLoggedIn = document.getElementById("authData").getAttribute("data-auth");

    if (isLoggedIn !== "true") {
        alert("Please login first!");
        window.location.href = "/Account/Login";
        return;
    }

    const MAX_ALLOWED = 5;

    if (selected.length > MAX_ALLOWED) {
        alert("You can select maximum 5 slots only!");
        return;
    }

    const ids = selected.map(s => s.id).join(',');

    window.location.href = "/Booking/Checkout?slots=" + ids;
}