// ===================================================
// CourtBook - Site JavaScript
// ===================================================

// ----- Mobile Menu -----
const hamburgerBtn = document.getElementById('hamburger-btn');
const closeMenuBtn = document.getElementById('close-menu-btn');
const mobileOverlay = document.getElementById('mobile-overlay');
const mobileMenu = document.getElementById('mobile-menu');

function openMobileMenu() {
    mobileOverlay?.classList.add('active');
    mobileMenu?.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeMobileMenu() {
    mobileOverlay?.classList.remove('active');
    mobileMenu?.classList.remove('active');
    document.body.style.overflow = '';
}

hamburgerBtn?.addEventListener('click', openMobileMenu);
closeMenuBtn?.addEventListener('click', closeMobileMenu);
mobileOverlay?.addEventListener('click', closeMobileMenu);

// Close mobile menu on Escape key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeMobileMenu();
    }
});

// ----- Toast Notification System -----
window.showToast = function (message, type = 'success', duration = 3000) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;

    const iconName = type === 'success' ? 'check-circle' : 'alert-circle';
    const iconColor = type === 'success' ? 'var(--cb-primary)' : 'var(--cb-rose)';

    toast.innerHTML = `
        <i data-lucide="${iconName}" style="width:20px;height:20px;color:${iconColor};flex-shrink:0"></i>
        <span>${message}</span>
    `;

    container.appendChild(toast);
    lucide.createIcons({ nodes: [toast] });

    // Trigger show animation
    requestAnimationFrame(() => {
        toast.classList.add('show');
    });

    // Auto remove after duration
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, duration);
};

// ----- Tab Switching -----
window.initTabs = function (containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return;

    const buttons = container.querySelectorAll('.tab-btn');
    const panels = container.querySelectorAll('.tab-panel');

    buttons.forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.dataset.tab;

            buttons.forEach(b => b.classList.remove('active'));
            panels.forEach(p => p.classList.remove('active'));

            btn.classList.add('active');
            document.getElementById(target)?.classList.add('active');
        });
    });
};

// ----- Modal -----
window.openModal = function (modalId) {
    document.getElementById(modalId)?.classList.add('active');
    document.body.style.overflow = 'hidden';
};

window.closeModal = function (modalId) {
    document.getElementById(modalId)?.classList.remove('active');
    document.body.style.overflow = '';
};

// Close modal on overlay click
document.querySelectorAll('.modal-overlay').forEach(overlay => {
    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) {
            overlay.classList.remove('active');
            document.body.style.overflow = '';
        }
    });
});

// Close modal on Escape key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        const activeModal = document.querySelector('.modal-overlay.active');
        if (activeModal) {
            activeModal.classList.remove('active');
            document.body.style.overflow = '';
        }
    }
});
