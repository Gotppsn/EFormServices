// EFormServices.Web/wwwroot/js/admin-users.js
// Got code 30/05/2025
class UserManagement {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 20;
        this.totalPages = 1;
        this.filters = {};
        this.searchDebounce = null;
        this.selectedUser = null;
        this.roles = [];
        this.departments = [];
        this.init();
    }

    async init() {
        this.bindEvents();
        await this.loadDepartments();
        await this.loadRoles();
        await this.loadUsers();
    }

    bindEvents() {
        document.getElementById('search-input').addEventListener('input', (e) => {
            clearTimeout(this.searchDebounce);
            this.searchDebounce = setTimeout(() => {
                this.filters.searchTerm = e.target.value;
                this.resetPagination();
                this.loadUsers();
            }, 500);
        });

        ['department-filter', 'role-filter', 'status-filter', 'sort-filter'].forEach(id => {
            document.getElementById(id).addEventListener('change', (e) => {
                const key = id.replace('-filter', '');
                this.filters[key === 'department' ? 'departmentId' : key === 'status' ? 'isActive' : key] = e.target.value || null;
                this.resetPagination();
                this.loadUsers();
            });
        });

        document.getElementById('user-form').addEventListener('submit', (e) => this.handleUserSubmit(e));
        document.getElementById('generate-password').addEventListener('click', () => this.generatePassword());
        document.getElementById('save-roles').addEventListener('click', () => this.saveUserRoles());
        document.getElementById('confirm-delete').addEventListener('click', () => this.deleteUser());
        document.getElementById('deactivate-user').addEventListener('click', () => this.toggleUserStatus(false));

        document.getElementById('user-modal').addEventListener('hidden.bs.modal', () => this.resetUserForm());
    }

    async loadDepartments() {
        try {
            const response = await fetch('/api/departments');
            this.departments = await response.json();
            
            const selects = document.querySelectorAll('select[name="departmentId"], #department-filter');
            selects.forEach(select => {
                if (select.id === 'department-filter') return;
                this.departments.forEach(dept => {
                    select.innerHTML += `<option value="${dept.id}">${dept.name}</option>`;
                });
            });

            const deptFilter = document.getElementById('department-filter');
            this.departments.forEach(dept => {
                deptFilter.innerHTML += `<option value="${dept.id}">${dept.name}</option>`;
            });
        } catch (error) {
            console.error('Failed to load departments:', error);
        }
    }

    async loadRoles() {
        try {
            const response = await fetch('/api/roles');
            this.roles = await response.json();
            
            const roleFilter = document.getElementById('role-filter');
            this.roles.forEach(role => {
                roleFilter.innerHTML += `<option value="${role.id}">${role.name}</option>`;
            });

            this.renderRolesContainer();
        } catch (error) {
            console.error('Failed to load roles:', error);
        }
    }

    renderRolesContainer() {
        const container = document.getElementById('roles-container');
        container.innerHTML = this.roles.map(role => `
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="roleIds" value="${role.id}" id="role_${role.id}">
                <label class="form-check-label" for="role_${role.id}">
                    <strong>${role.name}</strong>
                    ${role.description ? `<br><small class="text-muted">${role.description}</small>` : ''}
                </label>
            </div>
        `).join('');
    }

    async loadUsers() {
        try {
            const params = new URLSearchParams({
                page: this.currentPage,
                pageSize: this.pageSize,
                ...this.filters
            });

            if (this.filters.sort) {
                const [sortBy, sortDirection] = this.filters.sort.split('_');
                params.set('sortBy', sortBy);
                params.set('sortDescending', sortDirection === 'desc');
            }

            const response = await fetch(`/api/users?${params}`);
            const result = await response.json();
            
            this.renderUsers(result.data.items);
            this.renderPagination(result.data);
            this.updateCounts(result.data);
            
        } catch (error) {
            console.error('Failed to load users:', error);
            this.renderError();
        }
    }

    renderUsers(users) {
        const tbody = document.getElementById('users-table-body');
        
        if (!users || users.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center py-4 text-muted">
                        <i class="fas fa-users fa-3x mb-3"></i>
                        <br>No users found
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = users.map(user => `
            <tr class="${!user.isActive ? 'table-secondary' : ''}">
                <td>
                    <div class="d-flex align-items-center">
                        <div class="bg-primary rounded-circle d-flex align-items-center justify-content-center me-3" 
                             style="width: 40px; height: 40px;">
                            <i class="fas fa-user text-white"></i>
                        </div>
                        <div>
                            <strong>${this.escapeHtml(user.fullName)}</strong>
                            <br><small class="text-muted">${this.escapeHtml(user.email)}</small>
                        </div>
                    </div>
                </td>
                <td>${user.departmentName || '<span class="text-muted">No Department</span>'}</td>
                <td>
                    ${user.roles.map(role => `<span class="badge bg-secondary me-1">${role}</span>`).join('')}
                    ${!user.roles.length ? '<span class="text-muted">No roles</span>' : ''}
                </td>
                <td>
                    <span class="badge bg-${user.isActive ? 'success' : 'secondary'}">
                        ${user.isActive ? 'Active' : 'Inactive'}
                    </span>
                    ${!user.emailConfirmed ? '<br><small class="text-warning">Email not confirmed</small>' : ''}
                </td>
                <td>
                    ${user.lastLoginAt 
                        ? `<div>${this.formatDateTime(user.lastLoginAt)}</div>` 
                        : '<span class="text-muted">Never</span>'}
                </td>
                <td>
                    <div class="dropdown">
                        <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
                            <i class="fas fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu">
                            <li><button type="button" class="dropdown-item" onclick="userManager.editUser(${user.id})">
                                <i class="fas fa-edit me-2"></i>Edit User
                            </button></li>
                            <li><button type="button" class="dropdown-item" onclick="userManager.editUserRoles(${user.id}, '${this.escapeHtml(user.fullName)}', '${this.escapeHtml(user.email)}')">
                                <i class="fas fa-user-tag me-2"></i>Edit Roles
                            </button></li>
                            <li><hr class="dropdown-divider"></li>
                            <li><button type="button" class="dropdown-item ${user.isActive ? 'text-warning' : 'text-success'}" 
                                onclick="userManager.toggleUserStatus(${user.id}, ${!user.isActive})">
                                <i class="fas fa-${user.isActive ? 'pause' : 'play'} me-2"></i>
                                ${user.isActive ? 'Deactivate' : 'Activate'}
                            </button></li>
                            <li><button type="button" class="dropdown-item text-danger" onclick="userManager.confirmDelete(${user.id}, '${this.escapeHtml(user.fullName)}')">
                                <i class="fas fa-trash me-2"></i>Delete
                            </button></li>
                        </ul>
                    </div>
                </td>
            </tr>
        `).join('');
    }

    renderPagination(data) {
        this.totalPages = data.totalPages;
        const pagination = document.getElementById('pagination');
        
        if (this.totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let paginationHtml = '';
        
        if (this.currentPage > 1) {
            paginationHtml += `
                <li class="page-item">
                    <button class="page-link" onclick="userManager.goToPage(${this.currentPage - 1})">Previous</button>
                </li>
            `;
        }

        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(this.totalPages, this.currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
                <li class="page-item ${i === this.currentPage ? 'active' : ''}">
                    <button class="page-link" onclick="userManager.goToPage(${i})">${i}</button>
                </li>
            `;
        }

        if (this.currentPage < this.totalPages) {
            paginationHtml += `
                <li class="page-item">
                    <button class="page-link" onclick="userManager.goToPage(${this.currentPage + 1})">Next</button>
                </li>
            `;
        }

        pagination.innerHTML = paginationHtml;
    }

    updateCounts(data) {
        document.getElementById('showing-count').textContent = data.items.length;
        document.getElementById('total-count').textContent = data.totalCount;
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadUsers();
    }

    resetPagination() {
        this.currentPage = 1;
    }

    async handleUserSubmit(e) {
        e.preventDefault();
        
        const formData = new FormData(e.target);
        const roleIds = Array.from(formData.getAll('roleIds')).map(id => parseInt(id));
        
        const userData = {
            firstName: formData.get('firstName'),
            lastName: formData.get('lastName'),
            email: formData.get('email'),
            password: formData.get('password'),
            departmentId: formData.get('departmentId') ? parseInt(formData.get('departmentId')) : null,
            roleIds: roleIds,
            externalId: formData.get('externalId') || null
        };

        if (!this.validateUserData(userData)) return;

        try {
            const url = this.selectedUser ? `/api/users/${this.selectedUser.id}` : '/api/users';
            const method = this.selectedUser ? 'PUT' : 'POST';
            
            const response = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(userData)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.errors?.join(', ') || 'Failed to save user');
            }

            this.showToast(`User ${this.selectedUser ? 'updated' : 'created'} successfully`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('user-modal')).hide();
            this.loadUsers();
            
        } catch (error) {
            console.error('Save user error:', error);
            this.showToast(error.message || 'Failed to save user', 'error');
        }
    }

    validateUserData(data) {
        if (!data.firstName.trim()) {
            this.showToast('First name is required', 'error');
            return false;
        }
        if (!data.lastName.trim()) {
            this.showToast('Last name is required', 'error');
            return false;
        }
        if (!data.email.trim() || !this.isValidEmail(data.email)) {
            this.showToast('Valid email is required', 'error');
            return false;
        }
        if (!this.selectedUser && (!data.password || data.password.length < 8)) {
            this.showToast('Password must be at least 8 characters', 'error');
            return false;
        }
        if (!data.roleIds.length) {
            this.showToast('At least one role must be assigned', 'error');
            return false;
        }
        return true;
    }

    async editUser(userId) {
        try {
            const response = await fetch(`/api/users/${userId}`);
            const result = await response.json();
            this.selectedUser = result.data;
            
            const form = document.getElementById('user-form');
            form.firstName.value = this.selectedUser.firstName;
            form.lastName.value = this.selectedUser.lastName;
            form.email.value = this.selectedUser.email;
            form.departmentId.value = this.selectedUser.departmentId || '';
            form.externalId.value = this.selectedUser.externalId || '';
            
            form.password.parentNode.style.display = 'none';
            
            this.selectedUser.roles.forEach(roleName => {
                const role = this.roles.find(r => r.name === roleName);
                if (role) {
                    document.getElementById(`role_${role.id}`).checked = true;
                }
            });

            document.querySelector('#user-modal .modal-title').textContent = 'Edit User';
            new bootstrap.Modal(document.getElementById('user-modal')).show();
            
        } catch (error) {
            console.error('Edit user error:', error);
            this.showToast('Failed to load user data', 'error');
        }
    }

    editUserRoles(userId, userName, userEmail) {
        this.selectedUser = { id: userId };
        document.getElementById('edit-user-name').textContent = userName;
        document.getElementById('edit-user-email').textContent = userEmail;
        
        this.loadUserRoles(userId);
        new bootstrap.Modal(document.getElementById('edit-roles-modal')).show();
    }

    async loadUserRoles(userId) {
        try {
            const response = await fetch(`/api/users/${userId}`);
            const result = await response.json();
            const user = result.data;
            
            const container = document.getElementById('edit-roles-container');
            container.innerHTML = this.roles.map(role => `
                <div class="form-check mb-2">
                    <input class="form-check-input" type="checkbox" name="editRoleIds" value="${role.id}" 
                           id="edit_role_${role.id}" ${user.roles.includes(role.name) ? 'checked' : ''}>
                    <label class="form-check-label" for="edit_role_${role.id}">
                        <strong>${role.name}</strong>
                        ${role.description ? `<br><small class="text-muted">${role.description}</small>` : ''}
                    </label>
                </div>
            `).join('');
        } catch (error) {
            console.error('Load user roles error:', error);
            this.showToast('Failed to load user roles', 'error');
        }
    }

    async saveUserRoles() {
        const roleIds = Array.from(document.querySelectorAll('input[name="editRoleIds"]:checked'))
                            .map(cb => parseInt(cb.value));
        
        if (!roleIds.length) {
            this.showToast('At least one role must be assigned', 'error');
            return;
        }

        try {
            const response = await fetch(`/api/users/${this.selectedUser.id}/roles`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ roleIds })
            });

            if (!response.ok) throw new Error('Failed to update roles');

            this.showToast('User roles updated successfully', 'success');
            bootstrap.Modal.getInstance(document.getElementById('edit-roles-modal')).hide();
            this.loadUsers();
            
        } catch (error) {
            console.error('Save roles error:', error);
            this.showToast('Failed to update user roles', 'error');
        }
    }

    async toggleUserStatus(userId, activate = null) {
        if (activate === null) {
            userId = this.selectedUser.id;
            activate = false;
        }

        try {
            const endpoint = activate ? 'activate' : 'deactivate';
            const response = await fetch(`/api/users/${userId}/${endpoint}`, {
                method: 'POST'
            });

            if (!response.ok) throw new Error(`Failed to ${endpoint} user`);

            this.showToast(`User ${activate ? 'activated' : 'deactivated'} successfully`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('delete-modal'))?.hide();
            this.loadUsers();
            
        } catch (error) {
            console.error('Toggle status error:', error);
            this.showToast(`Failed to ${activate ? 'activate' : 'deactivate'} user`, 'error');
        }
    }

    confirmDelete(userId, userName) {
        this.selectedUser = { id: userId, name: userName };
        document.querySelector('#delete-modal .modal-body p').textContent = `Are you sure you want to delete "${userName}"?`;
        new bootstrap.Modal(document.getElementById('delete-modal')).show();
    }

    async deleteUser() {
        try {
            const response = await fetch(`/api/users/${this.selectedUser.id}`, {
                method: 'DELETE'
            });

            if (!response.ok) throw new Error('Failed to delete user');

            this.showToast('User deleted successfully', 'success');
            bootstrap.Modal.getInstance(document.getElementById('delete-modal')).hide();
            this.loadUsers();
            
        } catch (error) {
            console.error('Delete user error:', error);
            this.showToast('Failed to delete user', 'error');
        }
    }

    generatePassword() {
        const length = 12;
        const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
        let password = '';
        
        for (let i = 0; i < length; i++) {
            password += charset.charAt(Math.floor(Math.random() * charset.length));
        }
        
        document.querySelector('input[name="password"]').value = password;
        this.showToast('Password generated', 'info', 2000);
    }

    resetUserForm() {
        this.selectedUser = null;
        document.getElementById('user-form').reset();
        document.querySelectorAll('input[name="roleIds"]').forEach(cb => cb.checked = false);
        document.querySelector('input[name="password"]').parentNode.style.display = 'block';
        document.querySelector('#user-modal .modal-title').textContent = 'Add User';
    }

    renderError() {
        document.getElementById('users-table-body').innerHTML = `
            <tr>
                <td colspan="6" class="text-center py-4 text-danger">
                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                    <br>Failed to load users. Please try again.
                </td>
            </tr>
        `;
    }

    formatDateTime(dateString) {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showToast(message, type, duration = 5000) {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'error' ? 'danger' : type === 'info' ? 'info' : 'warning'} border-0 position-fixed top-0 end-0 m-3`;
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        document.body.appendChild(toast);
        new bootstrap.Toast(toast, { delay: duration }).show();
    }
}

let userManager;
document.addEventListener('DOMContentLoaded', () => {
    userManager = new UserManagement();
});