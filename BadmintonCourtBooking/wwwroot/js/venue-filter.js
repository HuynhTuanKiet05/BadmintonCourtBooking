document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('venue-search-form');
    const searchInput = document.getElementById('search-input');
    const priceRange = document.getElementById('price-range');
    const priceDisplay = document.getElementById('price-slider-value');
    const hoursFilter = document.getElementById('hours-filter');
    const slotsFilter = document.getElementById('available-today-filter');
    const sortSelect = document.getElementById('sort-select');
    const districtInput = document.getElementById('district-input');
    const districtChips = document.querySelectorAll('#district-filter-group .filter-chip');

    if (!form) {
        return;
    }

    let searchTimer;

    function formatVND(value) {
        return new Intl.NumberFormat('vi-VN').format(value) + '₫';
    }

    function submitForm() {
        if (form.requestSubmit) {
            form.requestSubmit();
            return;
        }

        form.submit();
    }

    if (priceRange && priceDisplay) {
        priceDisplay.textContent = formatVND(parseInt(priceRange.value || '0', 10));

        priceRange.addEventListener('input', function (event) {
            priceDisplay.textContent = formatVND(parseInt(event.target.value || '0', 10));
        });

        priceRange.addEventListener('change', submitForm);
    }

    hoursFilter?.addEventListener('change', submitForm);
    slotsFilter?.addEventListener('change', submitForm);
    sortSelect?.addEventListener('change', submitForm);

    districtChips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            if (!districtInput) {
                return;
            }

            districtInput.value = chip.getAttribute('data-district') || '';
            submitForm();
        });
    });

    searchInput?.addEventListener('input', function () {
        window.clearTimeout(searchTimer);
        searchTimer = window.setTimeout(submitForm, 350);
    });
});
