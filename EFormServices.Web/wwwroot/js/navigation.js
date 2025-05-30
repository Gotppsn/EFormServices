// EFormServices.Web/wwwroot/js/navigation.js
// Got code 30/05/2025
class NavigationManager {
    constructor() {
        this.updateInterval = 30000;
        this.statusCheckInterval = 60000;
        this.init();
    }

    init() {
        this.updatePendingCounts();
        this.checkServerStatus();
        this.setActiveNavigation();
        this.bindEvents();
        this.startPeriodicUpdates();
    }

    bindEvents() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('.dropdown-toggle')) {
                this.handleDropdownClick(e);
            }
        });

        window.addEventListener('beforeunload', () => {
            this.clearIntervals();
        });
    }

    setActiveNavigation() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
        
        navLinks.forEach(link => {
            const href = link.getAttribute('href') || link.getAttribute('asp-page');
            if (href && currentPath.includes(href)) {
                link.classList.add('active');
                const parentDropdown = link.closest('.dropdown');
                if (parentDropdown) {
                    parentDropdown.querySelector('.dropdown-toggle').classList.add('active');
                }
            }
        });
    }

    async updatePendingCounts() {
        try {
            const response = await fetch('/api/approvals/pending?pageSize=1');
            const data = await response.json();
            
            const pendingBadge = document.getElementById('pending-count');
            if (pendingBadge) {
                const count = data.totalCount || 0;
                pendingBadge.textContent = count;
                pendingBadge.style.display = count > 0 ? 'inline' : 'none';
                
                if (count > 0) {
                    pendingBadge.classList.remove('bg-secondary');
                    pendingBadge.classList.add('bg-warning', 'text-dark');
                } else {
                    pendingBadge.classList.remove('bg-warning', 'text-dark');
                    pendingBadge.classList.add('bg-secondary');
                }
            }
        } catch (error) {
            console.warn('Failed to update pending counts:', error);
        }
    }

    async checkServerStatus() {
        try {
            const response = await fetch('/api/health');
            const status = response.ok ? 'online' : 'degraded';
            this.updateServerStatus(status);
        } catch (error) {
            this.updateServerStatus('offline');
        }
    }

    updateServerStatus(status) {
        const statusEl = document.getElementById('server-status');
        if (!statusEl) return;

        statusEl.classList.remove('bg-success', 'bg-warning', 'bg-danger');
        
        switch (status) {
            case 'online':
                statusEl.textContent = 'Online';
                statusEl.classList.add('bg-success');
                break;
            case 'degraded':
                statusEl.textContent = 'Degraded';
                statusEl.classList.add('bg-warning');
                break;
            case 'offline':
                statusEl.textContent = 'Offline';
                statusEl.classList.add('bg-danger');
                break;
        }
    }

    handleDropdownClick(e) {
        const dropdown = e.target.closest('.dropdown');
        if (!dropdown) return;

        const menu = dropdown.querySelector('.dropdown-menu');
        if (!menu) return;

        const isVisible = menu.classList.contains('show');
        if (!isVisible) {
            this.loadDropdownContent(dropdown);
        }
    }

    async loadDropdownContent(dropdown) {
        const submissionsDropdown = dropdown.querySelector('a[asp-page="/Submissions/Index"]');
        if (submissionsDropdown) {
            await this.updateSubmissionCounts();
        }
    }

    async updateSubmissionCounts() {
        try {
            const response = await fetch('/api/dashboard/stats');
            const data = await response.json();
            
            const submissionItems = document.querySelectorAll('.dropdown-item[href*="submissions"]');
            submissionItems.forEach(item => {
                const badge = item.querySelector('.badge');
                if (badge) {
                    badge.textContent = data.overview.totalSubmissions || 0;
                }
            });
        } catch (error) {
            console.warn('Failed to update submission counts:', error);
        }
    }

    startPeriodicUpdates() {
        this.pendingInterval = setInterval(() => {
            this.updatePendingCounts();
        }, this.updateInterval);

        this.statusInterval = setInterval(() => {
            this.checkServerStatus();
        }, this.statusCheckInterval);
    }

    clearIntervals() {
        if (this.pendingInterval) clearInterval(this.pendingInterval);
        if (this.statusInterval) clearInterval(this.statusInterval);
    }
}

class NotificationManager {
    constructor() {
        this.notifications = [];
        this.maxNotifications = 5;
        this.defaultDuration = 5000;
        this.init();
    }

    init() {
        this.createContainer();
        this.bindEvents();
    }

    createContainer() {
        if (document.getElementById('notification-container')) return;

        const container = document.createElement('div');
        container.id = 'notification-container';
        container.className = 'position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    show(message, type = 'info', duration = this.defaultDuration) {
        const notification = this.createNotification(message, type, duration);
        this.addNotification(notification);
        return notification.id;
    }

    createNotification(message, type, duration) {
        const id = `notification-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const typeClass = this.getTypeClass(type);
        const icon = this.getTypeIcon(type);

        const notification = document.createElement('div');
        notification.id = id;
        notification.className = `toast align-items-center text-white ${typeClass} border-0 mb-2`;
        notification.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="${icon} me-2"></i>${this.escapeHtml(message)}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                        data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        const toast = new bootstrap.Toast(notification, {
            delay: duration,
            autohide: duration > 0
        });

        notification.addEventListener('hidden.bs.toast', () => {
            this.removeNotification(id);
        });

        return { id, element: notification, toast };
    }

    addNotification(notification) {
        if (this.notifications.length >= this.maxNotifications) {
            const oldest = this.notifications.shift();
            oldest.toast.hide();
        }

        this.notifications.push(notification);
        document.getElementById('notification-container').appendChild(notification.element);
        notification.toast.show();
    }

    removeNotification(id) {
        this.notifications = this.notifications.filter(n => n.id !== id);
        const element = document.getElementById(id);
        if (element) element.remove();
    }

    dismiss(id) {
        const notification = this.notifications.find(n => n.id === id);
        if (notification) {
            notification.toast.hide();
        }
    }

    clear() {
        this.notifications.forEach(n => n.toast.hide());
        this.notifications = [];
    }

    getTypeClass(type) {
        const classes = {
            success: 'bg-success',
            error: 'bg-danger',
            warning: 'bg-warning',
            info: 'bg-info',
            primary: 'bg-primary'
        };
        return classes[type] || classes.info;
    }

    getTypeIcon(type) {
        const icons = {
            success: 'fas fa-check-circle',
            error: 'fas fa-exclamation-triangle',
            warning: 'fas fa-exclamation-circle',
            info: 'fas fa-info-circle',
            primary: 'fas fa-bell'
        };
        return icons[type] || icons.info;
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    bindEvents() {
        window.addEventListener('online', () => {
            this.show('Connection restored', 'success', 3000);
        });

        window.addEventListener('offline', () => {
            this.show('Connection lost. Working offline.', 'warning', 0);
        });
    }
}

let navigationManager;
let notificationManager;

document.addEventListener('DOMContentLoaded', () => {
    navigationManager = new NavigationManager();
    notificationManager = new NotificationManager();
    
    window.showNotification = (message, type, duration) => 
        notificationManager.show(message, type, duration);
});