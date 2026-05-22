document.addEventListener('DOMContentLoaded', function () {
    // DOM Elements
    const searchInput = document.getElementById('search-input');
    const districtChips = document.querySelectorAll('#district-filter-group .filter-chip');
    const priceRange = document.getElementById('price-range');
    const priceDisplay = document.getElementById('price-slider-value');
    const hoursFilter = document.getElementById('hours-filter');
    const slotsFilter = document.getElementById('available-today-filter');
    const sortSelect = document.getElementById('sort-select');
    const emptyState = document.getElementById('empty-state');
    const venueCount = document.getElementById('venue-count');
    const venuesContainer = document.getElementById('venues-container');
    const clearFiltersBtn = document.getElementById('clear-filters-btn');
    
    // Store original items and order
    const originalItems = Array.from(document.querySelectorAll('.venue-filter-item'));
    
    // Active filters state
    let activeFilters = {
        query: '',
        district: 'Tất cả',
        maxPrice: 250000,
        hours: 'any',
        onlyAvailable: false,
        sort: 'relevance'
    };

    // Initialize formatting function for VND
    function formatVND(value) {
        return new Intl.NumberFormat('vi-VN').format(value) + '₫';
    }

    // Main Filter & Sort function
    function applyFilters() {
        let visibleCount = 0;
        
        // 1. Filter
        const filteredItems = originalItems.map(item => {
            const name = item.getAttribute('data-name') || '';
            const district = item.getAttribute('data-district') || '';
            const price = parseInt(item.getAttribute('data-price') || '0', 10);
            const slots = item.getAttribute('data-slots') === 'true';
            const openType = item.getAttribute('data-open') || '';
            
            let isMatch = true;

            // Text search (name or district)
            if (activeFilters.query) {
                const searchLower = activeFilters.query.toLowerCase();
                const matchesText = name.includes(searchLower) || district.toLowerCase().includes(searchLower);
                if (!matchesText) isMatch = false;
            }

            // District
            if (activeFilters.district !== 'Tất cả') {
                if (district !== activeFilters.district) isMatch = false;
            }

            // Max Price
            if (price > activeFilters.maxPrice) {
                isMatch = false;
            }

            // Hours
            if (activeFilters.hours !== 'any') {
                if (openType !== activeFilters.hours) isMatch = false;
            }

            // Available slots today
            if (activeFilters.onlyAvailable && !slots) {
                isMatch = false;
            }

            // Toggle item visibility
            if (isMatch) {
                item.style.display = 'block';
                visibleCount++;
            } else {
                item.style.display = 'none';
            }

            return { item, isMatch, price, rating: parseFloat(item.getAttribute('data-rating') || '0'), originalIndex: originalItems.indexOf(item) };
        });

        // 2. Sort visible items
        const visibleItems = filteredItems.filter(x => x.isMatch);
        
        if (activeFilters.sort === 'price-asc') {
            visibleItems.sort((a, b) => a.price - b.price);
        } else if (activeFilters.sort === 'rating-desc') {
            visibleItems.sort((a, b) => b.rating - a.rating);
        } else {
            // relevance - restore original DOM order
            visibleItems.sort((a, b) => a.originalIndex - b.originalIndex);
        }

        // Re-append visible items in the sorted order
        visibleItems.forEach(x => {
            venuesContainer.appendChild(x.item);
        });

        // 3. Update count and empty state UI
        venueCount.textContent = visibleCount;
        if (visibleCount === 0) {
            venuesContainer.style.display = 'none';
            emptyState.style.display = 'block';
        } else {
            venuesContainer.style.display = 'grid';
            emptyState.style.display = 'none';
        }
    }

    // Event Listeners
    
    // Search input
    searchInput.addEventListener('input', function (e) {
        activeFilters.query = e.target.value;
        applyFilters();
    });

    // District chips
    districtChips.forEach(chip => {
        chip.addEventListener('click', function () {
            districtChips.forEach(c => c.classList.remove('active'));
            this.classList.add('active');
            activeFilters.district = this.getAttribute('data-district');
            applyFilters();
        });
    });

    // Price range slider
    priceRange.addEventListener('input', function (e) {
        const val = parseInt(e.target.value, 10);
        activeFilters.maxPrice = val;
        priceDisplay.textContent = formatVND(val);
        applyFilters();
    });

    // Hours selector
    hoursFilter.addEventListener('change', function (e) {
        activeFilters.hours = e.target.value;
        applyFilters();
    });

    // Available today checkbox
    slotsFilter.addEventListener('change', function (e) {
        activeFilters.onlyAvailable = e.target.checked;
        applyFilters();
    });

    // Sort selector
    sortSelect.addEventListener('change', function (e) {
        activeFilters.sort = e.target.value;
        applyFilters();
    });

    // Clear filters button
    function resetAllFilters() {
        searchInput.value = '';
        activeFilters.query = '';
        
        districtChips.forEach(c => c.classList.remove('active'));
        const defaultChip = document.querySelector('#district-filter-group .filter-chip[data-district="Tất cả"]');
        if (defaultChip) defaultChip.classList.add('active');
        activeFilters.district = 'Tất cả';
        
        priceRange.value = 250000;
        activeFilters.maxPrice = 250000;
        priceDisplay.textContent = formatVND(250000);
        
        hoursFilter.value = 'any';
        activeFilters.hours = 'any';
        
        slotsFilter.checked = false;
        activeFilters.onlyAvailable = false;
        
        sortSelect.value = 'relevance';
        activeFilters.sort = 'relevance';
        
        applyFilters();
    }

    clearFiltersBtn.addEventListener('click', resetAllFilters);
});
