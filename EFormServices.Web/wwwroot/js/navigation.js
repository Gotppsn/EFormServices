// EFormServices.Web/wwwroot/js/navigation.js
// Got code 30/05/2025

class NavigationManager {
    constructor() {
        this.currentUser = null;
        this.pendingCount = 0;
        this.notifications = [];
        this.refreshInterval = null;
        this.init();
    }

    init() {
        this.loadUserProfile();
        this.updateActiveNavigation();
        this.startNotificationPolling();
        this.bindEvents();
    }

    async loadUserProfile() {
        try {
            const token = localStorage.getItem('accessToken');
            if (!token) return;

            const userProfile = await api.get('/auth/profile');
            this.currentUser = userProfile;
            this.updateUserInterface();
        } catch (error) {
            console.error('Failed to load user profile:', error);
        }
    }

    updateUserInterface() {
        if (!this.currentUser) return;

        const userNameElements = document.querySelectorAll('[data-user-name]');
        userNameElements.forEach(element => {
            element.textContent = `${this.currentUser.firstName} ${this.currentUser.lastName}`;
        });

        const organizationElements = document.querySelectorAll('[data-organization-name]');
        organizationElements.forEach(element => {
            element.textContent = this.currentUser.organizationName || 'Organization';
        });
    }

    updateActiveNavigation() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.nav-link');

        navLinks.forEach(link => {
            link.classList.remove('active');
            
            const href = link.getAttribute('href') || link.getAttribute('asp-page');
            if (href && this.isCurrentPage(currentPath, href)) {
                link.classList.add('active');
                
                const dropdown = link.closest('.dropdown');
                if (dropdown) {
                    const dropdownToggle = dropdown.querySelector('.dropdown-toggle');
                    if (dropdownToggle) {
                        dropdownToggle.classList.add('active');
                    }
                }
            }
        });
    }

    isCurrentPage(currentPath, linkPath) {
        if (linkPath === currentPath) return true;
        if (linkPath === '/' && currentPath === '/') return true;
        if (linkPath !== '/' && currentPath.startsWith(linkPath)) return true;
        return false;
    }

    async startNotificationPolling() {
        await this.updatePendingApprovals();
        await this.loadNotifications();

        this.refreshInterval = setInterval(async () => {
            await this.updatePendingApprovals();
            await this.loadNotifications();
        }, 30000);
    }

    async updatePendingApprovals() {
        try {
            const data = await api.get('/approvals/pending-count');
            this.pendingCount = data.count || 0;
            
            const badge = document.getElementById('pending-count');
            if (badge) {
                badge.textContent = this.pendingCount;
                badge.style.display = this.pendingCount > 0 ? 'inline' : 'none';
            }
        } catch (error) {
            console.error('Failed to update pending approvals:', error);
        }
    }

    async loadNotifications() {
        try {
            const data = await api.get('/notifications/recent');
            this.notifications = data.notifications || [];
            this.updateNotificationDropdown();
        } catch (error) {
            console.error('Failed to load notifications:', error);
        }
    }

    updateNotificationDropdown() {
        const dropdown = document.getElementById('notifications-dropdown');
        if (!dropdown) return;

        if (this.notifications.length === 0) {
            dropdown.innerHTML = `
                <li><span class="dropdown-item-text text-muted">No new notifications</span></li>
            `;
            return;
        }

        dropdown.innerHTML = this.notifications.map(notification => `
            <li>
                <a class="dropdown-item ${notification.isRead ? '' : 'fw-bold'}" 
                   href="#" onclick="navigationManager.markAsRead(${notification.id})">
                    <div class="d-flex align-items-start">
                        <div class="flex-shrink-0 me-2">
                            <i class="fas fa-${this.getNotificationIcon(notification.type)} text-${this.getNotificationColor(notification.type)}"></i>
                        </div>
                        <div class="flex-grow-1">
                            <div class="fw-medium">${notification.title}</div>
                            <div class="small text-muted">${notification.message}</div>
                            <div class="small text-muted">${formatDate(notification.createdAt, 'time')}</div>
                        </div>
                    </div>
                </a>
            </li>
        `).join('') + `
            <li><hr class="dropdown-divider"></li>
            <li><a class="dropdown-item text-center" href="/notifications">View All Notifications</a></li>
        `;

        const unreadCount = this.notifications.filter(n => !n.isRead).length;
        const notificationBadge = document.getElementById('notification-badge');
        if (notificationBadge) {
            notificationBadge.textContent = unreadCount;
            notificationBadge.style.display = unreadCount > 0 ? 'inline' : 'none';
        }
    }

    getNotificationIcon(type) {
        const icons = {
            submission: 'paper-plane',
            approval: 'check-circle',
            rejection: 'times-circle',
            comment: 'comment',
            assignment: 'user-plus',
            deadline: 'clock',
            system: 'info-circle'
        };
        return icons[type] || 'bell';
    }

    getNotificationColor(type) {
        const colors = {
            submission: 'primary',
            approval: 'success',
            rejection: 'danger',
            comment: 'info',
            assignment: 'warning',
            deadline: 'warning',
            system: 'secondary'
        };
        return colors[type] || 'primary';
    }

    async markAsRead(notificationId) {
        try {
            await api.post(`/notifications/${notificationId}/read`);
            
            const notification = this.notifications.find(n => n.id === notificationId);
            if (notification) {
                notification.isRead = true;
                this.updateNotificationDropdown();
            }
        } catch (error) {
            console.error('Failed to mark notification as read:', error);
        }
    }

    bindEvents() {
        const logoutLinks = document.querySelectorAll('[href="/api/auth/logout"]');
        logoutLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                this.logout();
            });
        });

        document.addEventListener('click', (e) => {
            if (e.target.matches('[data-toggle="sidebar"]')) {
                this.toggleSidebar();
            }
        });

        const searchInput = document.getElementById('global-search');
        if (searchInput) {
            searchInput.addEventListener('input', debounce((e) => {
                this.performGlobalSearch(e.target.value);
            }, 300));
        }
    }

    async logout() {
        try {
            await api.post('/auth/logout');
        } catch (error) {
            console.error('Logout API call failed:', error);
        } finally {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('userPreferences');
            
            if (this.refreshInterval) {
                clearInterval(this.refreshInterval);
            }
            
            window.location.href = '/login?logout=true';
        }
    }

    toggleSidebar() {
        const sidebar = document.getElementById('sidebar');
        const mainContent = document.getElementById('main-content');
        
        if (sidebar && mainContent) {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
            
            StorageManager.set('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        }
    }

    async performGlobalSearch(query) {
        if (!query || query.length < 2) {
            this.hideSearchResults();
            return;
        }

        try {
            const results = await api.get(`/search?q=${encodeURIComponent(query)}`);
            this.showSearchResults(results);
        } catch (error) {
            console.error('Global search failed:', error);
        }
    }

    showSearchResults(results) {
        let dropdown = document.getElementById('search-results-dropdown');
        if (!dropdown) {
            dropdown = document.createElement('div');
            dropdown.id = 'search-results-dropdown';
            dropdown.className = 'dropdown-menu show position-absolute';
            dropdown.style.cssText = 'top: 100%; left: 0; right: 0; z-index: 1000;';
            
            const searchContainer = document.getElementById('global-search').parentElement;
            searchContainer.style.position = 'relative';
            searchContainer.appendChild(dropdown);
        }

        if (!results || results.length === 0) {
            dropdown.innerHTML = '<div class="dropdown-item-text text-muted">No results found</div>';
            return;
        }

        dropdown.innerHTML = results.map(result => `
            <a class="dropdown-item" href="${result.url}">
                <div class="d-flex align-items-center">
                    <i class="fas fa-${result.icon} me-2 text-${result.color}"></i>
                    <div>
                        <div class="fw-medium">${result.title}</div>
                        <small class="text-muted">${result.description}</small>
                    </div>
                </div>
            </a>
        `).join('');

        dropdown.style.display = 'block';
    }

    hideSearchResults() {
        const dropdown = document.getElementById('search-results-dropdown');
        if (dropdown) {
            dropdown.style.display = 'none';
        }
    }

    checkServerStatus() {
        const statusIndicator = document.getElementById('server-status');
        if (!statusIndicator) return;

        fetch('/health')
            .then(response => {
                if (response.ok) {
                    statusIndicator.textContent = 'Online';
                    statusIndicator.className = 'badge bg-success';
                } else {
                    throw new Error('Server unhealthy');
                }
            })
            .catch(() => {
                statusIndicator.textContent = 'Offline';
                statusIndicator.className = 'badge bg-danger';
            });
    }

    destroy() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }
}

class BreadcrumbManager {
    static update(items) {
        const breadcrumb = document.getElementById('breadcrumb');
        if (!breadcrumb) return;

        const breadcrumbItems = items.map((item, index) => {
            const isLast = index === items.length - 1;
            return `
                <li class="breadcrumb-item ${isLast ? 'active' : ''}">
                    ${isLast ? item.text : `<a href="${item.url}">${item.text}</a>`}
                </li>
            `;
        }).join('');

        breadcrumb.innerHTML = breadcrumbItems;
    }

    static updateFromPath() {
        const path = window.location.pathname;
        const segments = path.split('/').filter(segment => segment);
        
        const items = [{ text: 'Home', url: '/dashboard' }];
        
        let currentPath = '';
        segments.forEach(segment => {
            currentPath += '/' + segment;
            const text = segment.charAt(0).toUpperCase() + segment.slice(1);
            items.push({ text, url: currentPath });
        });

        this.update(items);
    }
}

let navigationManager;

document.addEventListener('DOMContentLoaded', () => {
    navigationManager = new NavigationManager();
    BreadcrumbManager.updateFromPath();
    
    setInterval(() => {
        navigationManager.checkServerStatus();
    }, 60000);
});

window.addEventListener('beforeunload', () => {
    if (navigationManager) {
        navigationManager.destroy();
    }
});

document.addEventListener('click', (e) => {
    if (!e.target.closest('#global-search') && !e.target.closest('#search-results-dropdown')) {
        navigationManager?.hideSearchResults();
    }
});

window.NavigationManager = NavigationManager;
window.BreadcrumbManager = BreadcrumbManager;