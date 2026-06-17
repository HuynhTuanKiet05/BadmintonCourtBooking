document.addEventListener('DOMContentLoaded', function () {
    const listContainer = document.getElementById('notification-list');
    const badge = document.getElementById('notification-badge');
    const countBadge = document.getElementById('notification-count');

    if (!listContainer) return;

    function loadNotifications() {
        fetch('/Account/GetNotifications')
            .then(response => response.json())
            .then(data => {
                renderNotifications(data);
            })
            .catch(error => {
                console.error('Error loading notifications:', error);
                listContainer.innerHTML = `
                    <li style="padding: 1.5rem 1rem; text-align: center; color: #ef4444;">
                        Không thể tải thông báo.
                    </li>
                `;
            });
    }

    function renderNotifications(items) {
        if (!items || items.length === 0) {
            listContainer.innerHTML = `
                <li id="notification-empty" style="padding: 1.5rem 1rem; text-align: center; color: #64748b; font-style: italic;">
                    Không có thông báo nào.
                </li>
            `;
            badge.style.display = 'none';
            countBadge.style.display = 'none';
            return;
        }

        let html = '';
        items.forEach(item => {
            html += `
                <li class="notification-item" id="notif-item-${item.id}" style="padding: 0.75rem 1rem; border-bottom: 1px solid #f1f5f9; display: flex; justify-content: space-between; align-items: flex-start; gap: 0.75rem; transition: opacity 0.3s ease, transform 0.3s ease;">
                    <div style="display: flex; gap: 0.5rem; align-items: flex-start;">
                        <div style="width: 8px; height: 8px; background-color: #10b981; border-radius: 50%; margin-top: 4px; flex-shrink: 0;"></div>
                        <div style="text-align: left;">
                            <div style="font-weight: 600; color: #1e293b; line-height: 1.3;">${escapeHtml(item.title)}</div>
                            <div style="color: #475569; font-size: 0.75rem; margin-top: 0.125rem; line-height: 1.4;">${escapeHtml(item.content)}</div>
                            <div style="color: #94a3b8; font-size: 0.6875rem; margin-top: 0.25rem;">${escapeHtml(item.timeAgo)}</div>
                        </div>
                    </div>
                    <button class="delete-notif-btn" data-id="${item.id}" title="Xóa thông báo" style="border: none; background: none; padding: 4px; color: #94a3b8; cursor: pointer; border-radius: 4px; display: flex; align-items: center; justify-content: center; transition: all 0.2s;" onmouseover="this.style.color='#ef4444'; this.style.backgroundColor='#fee2e2';" onmouseout="this.style.color='#94a3b8'; this.style.backgroundColor='transparent';">
                        <i data-lucide="trash-2" style="width: 14px; height: 14px;"></i>
                    </button>
                </li>
            `;
        });

        listContainer.innerHTML = html;

        // Update count badges
        const count = items.length;
        badge.style.display = 'block';
        countBadge.textContent = count;
        countBadge.style.display = 'inline-block';

        // Bind delete events
        const deleteButtons = listContainer.querySelectorAll('.delete-notif-btn');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.stopPropagation(); // Prevent dropdown from closing
                const notifId = this.getAttribute('data-id');
                deleteNotification(notifId);
            });
        });

        if (window.lucide) {
            window.lucide.createIcons();
        }
    }

    function deleteNotification(id) {
        const itemEl = document.getElementById(`notif-item-${id}`);
        if (!itemEl) return;

        // Visual fade out first for responsiveness
        itemEl.style.opacity = '0';
        itemEl.style.transform = 'translateX(20px)';

        fetch(`/Account/DeleteNotification?id=${id}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                setTimeout(() => {
                    itemEl.remove();
                    updateBadgeCount();
                }, 300);
            } else {
                // Rollback if error
                itemEl.style.opacity = '1';
                itemEl.style.transform = 'none';
                alert(data.message || 'Không thể xóa thông báo.');
            }
        })
        .catch(error => {
            console.error('Error deleting notification:', error);
            itemEl.style.opacity = '1';
            itemEl.style.transform = 'none';
            alert('Lỗi kết nối hệ thống. Vui lòng thử lại.');
        });
    }

    function updateBadgeCount() {
        const remainingItems = listContainer.querySelectorAll('.notification-item');
        const count = remainingItems.length;

        if (count === 0) {
            listContainer.innerHTML = `
                <li id="notification-empty" style="padding: 1.5rem 1rem; text-align: center; color: #64748b; font-style: italic;">
                    Không có thông báo nào.
                </li>
            `;
            badge.style.display = 'none';
            countBadge.style.display = 'none';
        } else {
            countBadge.textContent = count;
        }
    }

    function escapeHtml(text) {
        if (!text) return '';
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function(m) { return map[m]; });
    }

    loadNotifications();
});
