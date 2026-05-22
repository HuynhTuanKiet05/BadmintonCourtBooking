document.addEventListener('DOMContentLoaded', function () {
    // Current State
    let selectedSlot = null; // { courtId, courtName, time, nextTime, price, priceFormatted }
    let activeDateKey = document.querySelector('.date-btn.active')?.getAttribute('data-datekey') || '';

    // DOM Elements
    const dateButtons = document.querySelectorAll('.date-btn');
    const slotsTbody = document.getElementById('slots-tbody');
    
    // Sidebar Elements
    const sidebarEmpty = document.getElementById('booking-sidebar-empty');
    const sidebarFilled = document.getElementById('booking-sidebar-filled');
    const sidebarCourtName = document.getElementById('sidebar-court-name');
    const sidebarDateLabel = document.getElementById('sidebar-date-label');
    const sidebarTimeLabel = document.getElementById('sidebar-time-label');
    const sidebarTotalPrice = document.getElementById('sidebar-total-price');
    const confirmBtnDesktop = document.getElementById('confirm-booking-btn-desktop');

    // Mobile Elements
    const mobileBar = document.getElementById('mobile-booking-bar');
    const mobileSummary = document.getElementById('mobile-booking-summary');
    const mobilePrice = document.getElementById('mobile-booking-price');
    const confirmBtnMobile = document.getElementById('confirm-booking-btn-mobile');

    // Modal Elements
    const modalOverlay = document.getElementById('booking-modal-overlay');
    const closeModalBtn = document.getElementById('close-modal-btn');
    const modalSubmitBtn = document.getElementById('modal-submit-btn');
    const modalCourtName = document.getElementById('modal-court-name');
    const modalDateLabel = document.getElementById('modal-date-label');
    const modalTimeLabel = document.getElementById('modal-time-label');
    const modalTotalPrice = document.getElementById('modal-total-price');

    // ── Helper: Format Date Key for Display ──
    function formatDateDisplay(key) {
        // e.g. "Hôm nay · 20/05" -> "Hôm nay, 20/05/2026" (assuming year 2026 based on context)
        const parts = key.split(' · ');
        if (parts.length > 1) {
            return `${parts[0]}, ngày ${parts[1]}/2026`;
        }
        return key;
    }

    // ── Helper: Refresh Selection UI ──
    function updateSelectionUI() {
        // Reset all selected cells
        document.querySelectorAll('.slot-pill.slot-selected').forEach(btn => {
            btn.classList.remove('slot-selected');
            // Re-apply original class
            const isPending = btn.textContent.trim() === 'Chờ';
            if (isPending) {
                btn.classList.add('slot-pending');
            } else {
                btn.classList.add('slot-available');
            }
        });

        if (selectedSlot) {
            // Find selected button in the DOM and set class
            const activeBtn = document.querySelector(`.slot-pill[data-courtid="${selectedSlot.courtId}"][data-time="${selectedSlot.time}"]`);
            if (activeBtn) {
                activeBtn.classList.remove('slot-available', 'slot-pending');
                activeBtn.classList.add('slot-selected');
            }

            // Update Sidebar (Desktop)
            if (sidebarEmpty) sidebarEmpty.style.display = 'none';
            if (sidebarFilled) {
                sidebarFilled.style.display = 'block';
                sidebarCourtName.textContent = selectedSlot.courtName;
                sidebarDateLabel.textContent = formatDateDisplay(activeDateKey);
                sidebarTimeLabel.textContent = `${selectedSlot.time} – ${selectedSlot.nextTime}`;
                sidebarTotalPrice.textContent = selectedSlot.priceFormatted;
            }

            // Update Mobile Bar
            if (mobileBar) {
                mobileBar.style.display = 'flex';
                mobileSummary.textContent = `${selectedSlot.courtName} · ${selectedSlot.time} – ${selectedSlot.nextTime}`;
                mobilePrice.textContent = selectedSlot.priceFormatted;
            }
        } else {
            // Hide Sidebar selection details
            if (sidebarFilled) sidebarFilled.style.display = 'none';
            if (sidebarEmpty) sidebarEmpty.style.display = 'block';

            // Hide Mobile Bar
            if (mobileBar) mobileBar.style.display = 'none';
        }
    }

    // ── Event Listener: Clicking on Date Buttons ──
    dateButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            if (this.classList.contains('active')) return;

            dateButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            activeDateKey = this.getAttribute('data-datekey');
            selectedSlot = null; // Clear selection on date change
            updateSelectionUI();

            // Load slots dynamically for the selected date
            fetchSlotsForDate(activeDateKey);
        });
    });

    // ── Function: Fetch Slots via AJAX ──
    function fetchSlotsForDate(dateKey) {
        if (!window.venueId) return;

        // Show loading state or visual feedback
        slotsTbody.style.opacity = '0.5';

        fetch(`/Venue/GetSlots?id=${window.venueId}&dateKey=${encodeURIComponent(dateKey)}`)
            .then(res => {
                if (!res.ok) throw new Error("Failed to load slots");
                return res.json();
            })
            .then(data => {
                // Re-render slot matrix in table body
                renderSlotMatrix(data);
            })
            .catch(err => {
                console.error(err);
                if (window.showToast) {
                    window.showToast("Không thể tải thông tin lịch đặt. Vui lòng thử lại.", "error");
                }
            })
            .finally(() => {
                slotsTbody.style.opacity = '1';
            });
    }

    // ── Function: Render Slot Matrix from JSON ──
    function renderSlotMatrix(matrix) {
        // Iterate through all cells in the table body and update class, status, text
        const cells = slotsTbody.querySelectorAll('.slot-pill');
        
        cells.forEach(btn => {
            const courtId = btn.getAttribute('data-courtid');
            const courtName = btn.getAttribute('data-courtname');
            const time = btn.getAttribute('data-time');
            
            // In matrix, the key is the court NAME (e.g. Sân 1, Sân VIP)
            const courtSlots = matrix[courtName];
            if (courtSlots) {
                const status = courtSlots[time]; // "Available", "Booked", "Pending"
                
                // Clear existing classes
                btn.className = 'slot-pill w-full';
                btn.removeAttribute('disabled');

                if (status === 'Booked') {
                    btn.classList.add('slot-booked');
                    btn.disabled = true;
                    btn.textContent = 'Đã đặt';
                } else if (status === 'Pending') {
                    btn.classList.add('slot-pending');
                    btn.textContent = 'Chờ duyệt';
                } else {
                    btn.classList.add('slot-available');
                    btn.textContent = 'Trống';
                }
            }
        });

        // Attach event handlers back
        attachSlotSelectionListeners();
    }

    // ── Function: Attach Click Handlers to Slot Pills ──
    function attachSlotSelectionListeners() {
        const slotButtons = document.querySelectorAll('.slot-pill:not(.slot-booked)');
        
        slotButtons.forEach(btn => {
            // Remove previous event listener (by cloning or clear)
            btn.onclick = null;
            
            btn.addEventListener('click', function () {
                const courtId = this.getAttribute('data-courtid');
                const courtName = this.getAttribute('data-courtname');
                const time = this.getAttribute('data-time');
                const nextTime = this.getAttribute('data-nexttime');
                const price = parseInt(this.getAttribute('data-price') || '0', 10);
                const priceFormatted = this.getAttribute('data-priceformatted');

                selectedSlot = { courtId, courtName, time, nextTime, price, priceFormatted };
                updateSelectionUI();
            });
        });
    }

    // Initialize listeners
    attachSlotSelectionListeners();

    // ── Confirm Booking Trigger ──
    function triggerBookingModal() {
        if (!selectedSlot) return;

        // Populate Modal Fields
        modalCourtName.textContent = selectedSlot.courtName;
        modalDateLabel.textContent = formatDateDisplay(activeDateKey);
        modalTimeLabel.textContent = `${selectedSlot.time} – ${selectedSlot.nextTime}`;
        modalTotalPrice.textContent = selectedSlot.priceFormatted;

        // Open Modal
        if (window.openModal) {
            window.openModal('booking-modal-overlay');
        }
    }

    if (confirmBtnDesktop) confirmBtnDesktop.addEventListener('click', triggerBookingModal);
    if (confirmBtnMobile) confirmBtnMobile.addEventListener('click', triggerBookingModal);

    // ── Close Modal Handler ──
    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', function () {
            if (window.closeModal) {
                window.closeModal('booking-modal-overlay');
            }
        });
    }

    // ── Submit Booking Handler ──
    if (modalSubmitBtn) {
        modalSubmitBtn.addEventListener('click', function () {
            if (!selectedSlot) return;

            // Close confirmation modal
            if (window.closeModal) {
                window.closeModal('booking-modal-overlay');
            }

            // Simulate booking submission
            const savedSlot = { ...selectedSlot };
            
            // Visual feedback: convert the selected button into a "Chờ duyệt" state
            const targetBtn = document.querySelector(`.slot-pill[data-courtid="${savedSlot.courtId}"][data-time="${savedSlot.time}"]`);
            if (targetBtn) {
                targetBtn.className = 'slot-pill slot-pending w-full';
                targetBtn.textContent = 'Chờ duyệt';
            }

            // Reset state
            selectedSlot = null;
            updateSelectionUI();

            // Trigger success toast
            if (window.showToast) {
                window.showToast("Đã gửi yêu cầu đặt sân. Chủ sân sẽ xác nhận trong ít phút.", "success", 4000);
            }
        });
    }
});
