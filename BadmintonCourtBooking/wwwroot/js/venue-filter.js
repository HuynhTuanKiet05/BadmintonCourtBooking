document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('venue-search-form');
    const searchInput = document.getElementById('search-input');
    const minPriceInput = document.getElementById('min-price-input');
    const maxPriceInput = document.getElementById('max-price-input');
    const hoursFilter = document.getElementById('hours-filter');
    const slotsFilter = document.getElementById('available-today-filter');
    const sortSelect = document.getElementById('sort-select');
    const districtInput = document.getElementById('district-input');
    const districtChips = document.querySelectorAll('#district-filter-group .filter-chip');

    const btnFindNearMe = document.getElementById('btn-find-near-me');
    const userLatInput = document.getElementById('user-lat');
    const userLngInput = document.getElementById('user-lng');

    if (!form) {
        return;
    }

    let searchTimer;

    function submitForm() {
        if (form.requestSubmit) {
            form.requestSubmit();
            return;
        }

        form.submit();
    }

    if (btnFindNearMe && userLatInput && userLngInput && sortSelect) {
        if (userLatInput.value && userLngInput.value) {
            btnFindNearMe.classList.remove('btn-outline');
            btnFindNearMe.classList.add('btn-primary');
            btnFindNearMe.innerHTML = '<i data-lucide="map-pin" style="width:16px;height:16px;"></i> Đã định vị';
        }

        btnFindNearMe.addEventListener('click', function () {
            btnFindNearMe.disabled = true;
            btnFindNearMe.innerHTML = '<i data-lucide="loader" style="width:16px;height:16px;"></i> Định vị...';
            if (window.lucide) {
                window.lucide.createIcons();
            }

            navigator.geolocation.getCurrentPosition(
                function (position) {
                    userLatInput.value = position.coords.latitude;
                    userLngInput.value = position.coords.longitude;
                    sortSelect.value = 'distance';
                    submitForm();
                },
                function (error) {
                    alert('Không thể lấy vị trí hiện tại. Vui lòng cho phép quyền truy cập GPS.');
                    btnFindNearMe.disabled = false;
                    btnFindNearMe.classList.remove('btn-primary');
                    btnFindNearMe.classList.add('btn-outline');
                    btnFindNearMe.innerHTML = '<i data-lucide="map-pin" style="width:16px;height:16px;"></i> Gần tôi';
                    if (window.lucide) {
                        window.lucide.createIcons();
                    }
                },
                { enableHighAccuracy: true, timeout: 8000 }
            );
        });
    }

    minPriceInput?.addEventListener('change', submitForm);
    maxPriceInput?.addEventListener('change', submitForm);
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
