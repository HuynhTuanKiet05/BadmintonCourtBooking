document.addEventListener('DOMContentLoaded', function () {
    let selectedSlot = null;
    let activeDateButton = document.querySelector('.date-btn.active');

    const bookingConfig = window.bookingConfig || {};
    const dateButtons = Array.from(document.querySelectorAll('.date-btn'));
    const slotsTbody = document.getElementById('slots-tbody');

    const sidebarEmpty = document.getElementById('booking-sidebar-empty');
    const sidebarFilled = document.getElementById('booking-sidebar-filled');
    const sidebarCourtName = document.getElementById('sidebar-court-name');
    const sidebarDateLabel = document.getElementById('sidebar-date-label');
    const sidebarTimeLabel = document.getElementById('sidebar-time-label');
    const sidebarTotalPrice = document.getElementById('sidebar-total-price');
    const confirmBtnDesktop = document.getElementById('confirm-booking-btn-desktop');

    const mobileBar = document.getElementById('mobile-booking-bar');
    const mobileSummary = document.getElementById('mobile-booking-summary');
    const mobilePrice = document.getElementById('mobile-booking-price');
    const confirmBtnMobile = document.getElementById('confirm-booking-btn-mobile');

    const bookingForm = document.getElementById('create-booking-form');
    const bookingCourtIdInput = document.getElementById('booking-court-id');
    const bookingDateInput = document.getElementById('booking-date');
    const bookingStartTimeInput = document.getElementById('booking-start-time');

    const closeModalBtn = document.getElementById('close-modal-btn');
    const modalSubmitBtn = document.getElementById('modal-submit-btn');
    const modalCourtName = document.getElementById('modal-court-name');
    const modalDateLabel = document.getElementById('modal-date-label');
    const modalTimeLabel = document.getElementById('modal-time-label');
    const modalTotalPrice = document.getElementById('modal-total-price');

    function getActiveDateKey() {
        return activeDateButton?.getAttribute('data-datekey') || '';
    }

    function getActiveDateValue() {
        return activeDateButton?.getAttribute('data-date') || '';
    }

    function getActiveDateDisplay() {
        return activeDateButton?.getAttribute('data-fulldate') || getActiveDateKey();
    }

    function updateSelectionUI() {
        document.querySelectorAll('.slot-pill.slot-selected').forEach(btn => {
            btn.classList.remove('slot-selected');
            btn.classList.add('slot-available');
        });

        if (!selectedSlot) {
            if (sidebarFilled) {
                sidebarFilled.style.display = 'none';
            }
            if (sidebarEmpty) {
                sidebarEmpty.style.display = 'block';
            }
            if (mobileBar) {
                mobileBar.style.display = 'none';
            }
            return;
        }

        const activeButton = document.querySelector(`.slot-pill.slot-available[data-courtid="${selectedSlot.courtId}"][data-time="${selectedSlot.time}"]`);
        if (activeButton) {
            activeButton.classList.remove('slot-available');
            activeButton.classList.add('slot-selected');
        }

        if (sidebarEmpty) {
            sidebarEmpty.style.display = 'none';
        }
        if (sidebarFilled) {
            sidebarFilled.style.display = 'block';
        }
        if (sidebarCourtName) {
            sidebarCourtName.textContent = selectedSlot.courtName;
        }
        if (sidebarDateLabel) {
            sidebarDateLabel.textContent = getActiveDateDisplay();
        }
        if (sidebarTimeLabel) {
            sidebarTimeLabel.textContent = `${selectedSlot.time} – ${selectedSlot.nextTime}`;
        }
        if (sidebarTotalPrice) {
            sidebarTotalPrice.textContent = selectedSlot.priceFormatted;
        }

        if (mobileBar) {
            mobileBar.style.display = 'flex';
        }
        if (mobileSummary) {
            mobileSummary.textContent = `${selectedSlot.courtName} · ${selectedSlot.time} – ${selectedSlot.nextTime}`;
        }
        if (mobilePrice) {
            mobilePrice.textContent = selectedSlot.priceFormatted;
        }
    }

    function updateModalDetails() {
        if (!selectedSlot) {
            return;
        }

        if (modalCourtName) {
            modalCourtName.textContent = selectedSlot.courtName;
        }
        if (modalDateLabel) {
            modalDateLabel.textContent = getActiveDateDisplay();
        }
        if (modalTimeLabel) {
            modalTimeLabel.textContent = `${selectedSlot.time} – ${selectedSlot.nextTime}`;
        }
        if (modalTotalPrice) {
            modalTotalPrice.textContent = selectedSlot.priceFormatted;
        }
    }

    function redirectToBookingGate() {
        if (window.showToast && bookingConfig.redirectMessage) {
            window.showToast(bookingConfig.redirectMessage, 'error', 2500);
        }

        if (bookingConfig.redirectUrl) {
            window.setTimeout(function () {
                window.location.href = bookingConfig.redirectUrl;
            }, 350);
        }
    }

    function triggerBookingFlow() {
        if (!selectedSlot) {
            return;
        }

        if (!bookingConfig.canSubmit) {
            redirectToBookingGate();
            return;
        }

        updateModalDetails();

        if (window.openModal) {
            window.openModal('booking-modal-overlay');
        }
    }

    function applySlotStatus(button, status) {
        button.className = 'slot-pill w-full';
        button.disabled = true;

        switch (status) {
            case 'Available':
                button.classList.add('slot-available');
                button.disabled = false;
                button.textContent = 'Trống';
                break;
            case 'Pending':
                button.classList.add('slot-pending');
                button.textContent = 'Chờ duyệt';
                break;
            case 'Unavailable':
                button.classList.add('slot-booked');
                button.textContent = 'Đã qua';
                break;
            default:
                button.classList.add('slot-booked');
                button.textContent = 'Đã đặt';
                break;
        }
    }

    function renderSlotMatrix(matrix) {
        if (!slotsTbody) {
            return;
        }

        slotsTbody.querySelectorAll('.slot-pill').forEach(button => {
            const courtName = button.getAttribute('data-courtname');
            const time = button.getAttribute('data-time');
            const status = matrix[courtName]?.[time] || 'Booked';

            applySlotStatus(button, status);
        });
    }

    function fetchSlotsForDate(dateKey) {
        if (!slotsTbody || !window.venueId) {
            return;
        }

        slotsTbody.style.opacity = '0.5';

        fetch(`/Venue/GetSlots?id=${window.venueId}&dateKey=${encodeURIComponent(dateKey)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to load slots');
                }

                return response.json();
            })
            .then(matrix => {
                renderSlotMatrix(matrix);
                selectedSlot = null;
                updateSelectionUI();
            })
            .catch(error => {
                console.error(error);
                if (window.showToast) {
                    window.showToast('Không thể tải thông tin lịch đặt. Vui lòng thử lại.', 'error', 4000);
                }
            })
            .finally(() => {
                slotsTbody.style.opacity = '1';
            });
    }

    dateButtons.forEach(button => {
        button.addEventListener('click', function () {
            if (button === activeDateButton) {
                return;
            }

            dateButtons.forEach(item => item.classList.remove('active'));
            button.classList.add('active');
            activeDateButton = button;

            fetchSlotsForDate(getActiveDateKey());
        });
    });

    slotsTbody?.addEventListener('click', function (event) {
        const target = event.target.closest('.slot-pill.slot-available');
        if (!target || target.disabled) {
            return;
        }

        selectedSlot = {
            courtId: target.getAttribute('data-courtid'),
            courtName: target.getAttribute('data-courtname'),
            time: target.getAttribute('data-time'),
            nextTime: target.getAttribute('data-nexttime'),
            price: parseInt(target.getAttribute('data-price') || '0', 10),
            priceFormatted: target.getAttribute('data-priceformatted')
        };

        updateSelectionUI();
    });

    confirmBtnDesktop?.addEventListener('click', triggerBookingFlow);
    confirmBtnMobile?.addEventListener('click', triggerBookingFlow);

    closeModalBtn?.addEventListener('click', function () {
        if (window.closeModal) {
            window.closeModal('booking-modal-overlay');
        }
    });

    modalSubmitBtn?.addEventListener('click', function () {
        if (!selectedSlot) {
            return;
        }

        if (!bookingConfig.canSubmit) {
            redirectToBookingGate();
            return;
        }

        if (!bookingForm || !bookingCourtIdInput || !bookingDateInput || !bookingStartTimeInput) {
            return;
        }

        bookingCourtIdInput.value = selectedSlot.courtId || '';
        bookingDateInput.value = getActiveDateValue();
        bookingStartTimeInput.value = selectedSlot.time || '';

        modalSubmitBtn.disabled = true;
        modalSubmitBtn.textContent = 'Đang gửi...';
        bookingForm.submit();
    });

    updateSelectionUI();
});
