// EFormServices.Web/wwwroot/js/admin-users.js
// Got code 30/05/2025

class UserManager {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 20;
        this.filters = {
            searchTerm: '',
            departmentId: '',
            roleId: '',
            isActive: '',
            sortBy: 'lastName',
            sortDescending: false
        };
        this.selectedUsers = new Set();
        this.departments = [];
        this.roles = [];
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadUsers();
        this.loadDepartments();
        this.loadRoles();
    }

    bindEvents() {
        const searchInput = document.getElementById('search-input');
        const departmentFilter = document.getElementById('department-filter');
        const roleFilter = document.getElementById('role-filter');
        const statusFilter = document.getElementById('status-filter');
        const sortFilter = document.getElementById('sort-filter');

        if (searchInput) {
            searchInput.addEventListener('input', debounce((e) => {
                this.filters.searchTerm = e.target.value;
                this.currentPage = 1;
                this.loadUsers();
            }, 500));
        }

        [departmentFilter, roleFilter, statusFilter, sortFilter].forEach(filter => {
            if (filter) {
                filter.addEventListener('change', (e) => {
                    const filterName = e.target.id.replace('-filter', '');
                    if (filterName === 'sort') {
                        const [sortBy, order] = e.target.value.split('_');
                        this.filters.sortBy = sortBy;
                        this.filters.sortDescending = order === 'desc';
                    } else {
                        this.filters[filterName === 'department' ? 'departmentId' : 
                                   filterName === 'role' ? 'roleId' : 'isActive'] = e.target.value;
                    }
                    this.currentPage = 1;
                    this.loadUsers();
                });
            }
        });

        document.getElementById('user-form')?.addEventListener('submit', this.handleUserSubmit.bind(this));
        document.getElementById('generate-password')?.addEventListener('click', this.generatePassword.bind(this));
        document.getElementById('save-roles')?.addEventListener('click', this.saveUserRoles.bind(this));

        document.addEventListener('click', this.handleClick.bind(this));
    }

    async loadUsers() {
        const tableBody = document.getElementById('users-table-body');
        if (!tableBody) return;

        LoadingManager.show(tableBody.parentElement);

        try {
            const params = new URLSearchParams({
                page: this.currentPage,
                pageSize: this.pageSize,
                ...Object.fromEntries(Object.entries(this.filters).filter(([_, v]) => v !== ''))
            });

            const data = await api.get(`/users?${params}`);
            this.renderUsers(data.items);
            this.renderPagination(data);
            this.updateUserCount(data);
        } catch (error) {
            NotificationManager.error('Failed to load users');
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Error loading users</td></tr>';
        } finally {
            LoadingManager.hide(tableBody.parentElement);
        }
    }

    renderUsers(users) {
        const tableBody = document.getElementById('users-table-body');
        if (!tableBody) return;

        if (!users || users.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No users found</td></tr>';
            return;
        }

        tableBody.innerHTML = users.map(user => `
            <tr data-user-id="${user.id}">
                <td>
                    <div class="d-flex align-items-center">
                        <input type="checkbox" class="form-check-input me-2" data-user-id="${user.id}">
                        <div class="user-avatar me-3">
                            <div class="bg-primary rounded-circle d-flex align-items-center justify-content-center" 
                                 style="width: 40px; height: 40px; color: white; font-weight: bold;">
                                ${user.firstName.charAt(0)}${user.lastName.charAt(0)}
                            </div>
                        </div>
                        <div>
                            <div class="fw-medium">${user.fullName}</div>
                            <div class="text-muted small">${user.email}</div>
                            ${!user.emailConfirmed ? '<span class="badge bg-warning">Unverified</span>' : ''}
                        </div>
                    </div>
                </td>
                <td>${user.departmentName || '<span class="text-muted">No Department</span>'}</td>
                <td>
                    ${user.roles.map(role => `<span class="badge bg-secondary me-1">${role}</span>`).join('') || 
                      '<span class="text-muted">No Roles</span>'}
                </td>
                <td>
                    <span class="badge bg-${user.isActive ? 'success' : 'secondary'}">
                        ${user.isActive ? 'Active' : 'Inactive'}
                    </span>
                </td>
                <td>
                    ${user.lastLoginAt ? formatDate(user.lastLoginAt, 'datetime') : 
                      '<span class="text-muted">Never</span>'}
                </td>
                <td>
                    <div class="btn-group" role="group">
                        <button type="button" class="btn btn-sm btn-outline-primary" 
                                onclick="userManager.editUser(${user.id})" title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-info" 
                                onclick="userManager.editUserRoles(${user.id})" title="Manage Roles">
                            <i class="fas fa-user-tag"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-warning" 
                                onclick="userManager.resetPassword(${user.id})" title="Reset Password">
                            <i class="fas fa-key"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-${user.isActive ? 'secondary' : 'success'}" 
                                onclick="userManager.toggleUserStatus(${user.id})" 
                                title="${user.isActive ? 'Deactivate' : 'Activate'}">
                            <i class="fas fa-${user.isActive ? 'pause' : 'play'}"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-danger" 
                                onclick="userManager.deleteUser(${user.id})" title="Delete">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');
    }

    renderPagination(data) {
        const pagination = document.getElementById('pagination');
        if (!pagination) return;

        const totalPages = data.totalPages || 1;
        const currentPage = data.page || 1;

        if (totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let paginationHTML = '';

        if (currentPage > 1) {
            paginationHTML += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="userManager.goToPage(${currentPage - 1})">Previous</a>
                </li>
            `;
        }

        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(totalPages, currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            paginationHTML += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="userManager.goToPage(${i})">${i}</a>
                </li>
            `;
        }

        if (currentPage < totalPages) {
            paginationHTML += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="userManager.goToPage(${currentPage + 1})">Next</a>
                </li>
            `;
        }

        pagination.innerHTML = paginationHTML;
    }

    updateUserCount(data) {
        const showingCount = document.getElementById('showing-count');
        const totalCount = document.getElementById('total-count');
        
        if (showingCount && totalCount) {
            const showing = Math.min(data.pageSize, data.totalCount - (data.page - 1) * data.pageSize);
            showingCount.textContent = showing;
            totalCount.textContent = data.totalCount;
        }
    }

    async loadDepartments() {
        try {
            const departments = await api.get('/departments');
            this.departments = departments || [];
            
            const select = document.getElementById('department-filter');
            const modalSelect = document.querySelector('#user-modal select[name="departmentId"]');
            
            const options = this.departments.map(dept => 
                `<option value="${dept.id}">${dept.name}</option>`
            ).join('');
            
            if (select) {
                select.innerHTML = `<option value="">All Departments</option>${options}`;
            }
            if (modalSelect) {
                modalSelect.innerHTML = `<option value="">No Department</option>${options}`;
            }
        } catch (error) {
            console.error('Failed to load departments:', error);
        }
    }

    async loadRoles() {
        try {
            const roles = await api.get('/roles');
            this.roles = roles || [];
            
            const select = document.getElementById('role-filter');
            if (select) {
                const options = this.roles.map(role => 
                    `<option value="${role.id}">${role.name}</option>`
                ).join('');
                select.innerHTML = `<option value="">All Roles</option>${options}`;
            }

            this.renderRolesContainer();
        } catch (error) {
            console.error('Failed to load roles:', error);
        }
    }

    renderRolesContainer() {
        const container = document.getElementById('roles-container');
        if (!container) return;

        container.innerHTML = this.roles.map(role => `
            <div class="form-check">
                <input type="checkbox" class="form-check-input" name="roleIds" value="${role.id}" id="role-${role.id}">
                <label class="form-check-label" for="role-${role.id}">
                    <strong>${role.name}</strong>
                    ${role.description ? `<br><small class="text-muted">${role.description}</small>` : ''}
                </label>
            </div>
        `).join('');
    }

    async handleUserSubmit(event) {
        event.preventDefault();
        
        const formData = new FormData(event.target);
        const roleIds = Array.from(formData.getAll('roleIds')).map(id => parseInt(id));
        
        const userData = {
            email: formData.get('email'),
            firstName: formData.get('firstName'),
            lastName: formData.get('lastName'),
            password: formData.get('password'),
            departmentId: formData.get('departmentId') ? parseInt(formData.get('departmentId')) : null,
            roleIds: roleIds,
            externalId: formData.get('externalId')
        };

        try {
            LoadingManager.show(event.target);
            
            if (this.editingUserId) {
                await api.put(`/users/${this.editingUserId}`, userData);
                NotificationManager.success('User updated successfully');
            } else {
                await api.post('/users', userData);
                NotificationManager.success('User created successfully');
            }
            
            this.loadUsers();
            ModalManager.hide('user-modal');
            event.target.reset();
            this.editingUserId = null;
        } catch (error) {
            NotificationManager.error('Failed to save user');
        } finally {
            LoadingManager.hide(event.target);
        }
    }

    generatePassword() {
        const length = 12;
        const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
        let password = '';
        
        for (let i = 0; i < length; i++) {
            password += charset.charAt(Math.floor(Math.random() * charset.length));
        }
        
        const passwordInput = document.querySelector('#user-modal input[name="password"]');
        if (passwordInput) {
            passwordInput.value = password;
            passwordInput.type = 'text';
            
            setTimeout(() => {
                passwordInput.type = 'password';
            }, 3000);
            
            NotificationManager.info('Generated password will be hidden in 3 seconds');
        }
    }

    editUser(userId) {
        this.editingUserId = userId;
        const modal = ModalManager.show('user-modal');
        
        api.get(`/users/${userId}`).then(user => {
            document.querySelector('#user-modal input[name="firstName"]').value = user.firstName;
            document.querySelector('#user-modal input[name="lastName"]').value = user.lastName;
            document.querySelector('#user-modal input[name="email"]').value = user.email;
            document.querySelector('#user-modal select[name="departmentId"]').value = user.departmentId || '';
            document.querySelector('#user-modal input[name="externalId"]').value = user.externalId || '';
            
            const passwordField = document.querySelector('#user-modal input[name="password"]');
            passwordField.required = false;
            passwordField.placeholder = 'Leave blank to keep current password';
            
            document.querySelector('#user-modal .modal-title').textContent = 'Edit User';
        });
    }

    async editUserRoles(userId) {
        try {
            const user = await api.get(`/users/${userId}`);
            
            document.getElementById('edit-user-name').textContent = user.fullName;
            document.getElementById('edit-user-email').textContent = user.email;
            
            const container = document.getElementById('edit-roles-container');
            container.innerHTML = this.roles.map(role => `
                <div class="form-check">
                    <input type="checkbox" class="form-check-input" name="editRoleIds" 
                           value="${role.id}" id="edit-role-${role.id}"
                           ${user.roles.includes(role.name) ? 'checked' : ''}>
                    <label class="form-check-label" for="edit-role-${role.id}">
                        <strong>${role.name}</strong>
                        ${role.description ? `<br><small class="text-muted">${role.description}</small>` : ''}
                    </label>
                </div>
            `).join('');
            
            this.editingRolesUserId = userId;
            ModalManager.show('edit-roles-modal');
        } catch (error) {
            NotificationManager.error('Failed to load user roles');
        }
    }

    async saveUserRoles() {
        if (!this.editingRolesUserId) return;

        const formData = new FormData();
        const checkboxes = document.querySelectorAll('input[name="editRoleIds"]:checked');
        const roleIds = Array.from(checkboxes).map(cb => parseInt(cb.value));

        try {
            await api.put(`/users/${this.editingRolesUserId}/roles`, { roleIds });
            NotificationManager.success('User roles updated successfully');
            this.loadUsers();
            ModalManager.hide('edit-roles-modal');
        } catch (error) {
            NotificationManager.error('Failed to update user roles');
        }
    }

    async toggleUserStatus(userId) {
        try {
            await api.post(`/users/${userId}/toggle-status`);
            NotificationManager.success('User status updated successfully');
            this.loadUsers();
        } catch (error) {
            NotificationManager.error('Failed to update user status');
        }
    }

    async resetPassword(userId) {
        if (!confirm('Are you sure you want to reset this user\'s password?')) return;

        try {
            const result = await api.post(`/users/${userId}/reset-password`);
            NotificationManager.success(`Password reset. New password: ${result.newPassword}`);
        } catch (error) {
            NotificationManager.error('Failed to reset password');
        }
    }

    deleteUser(userId) {
        this.selectedUserId = userId;
        ModalManager.show('delete-modal');
    }

    async confirmDelete() {
        if (!this.selectedUserId) return;

        try {
            await api.delete(`/users/${this.selectedUserId}`);
            NotificationManager.success('User deleted successfully');
            this.loadUsers();
            ModalManager.hide('delete-modal');
        } catch (error) {
            NotificationManager.error('Failed to delete user');
        }
    }

    async deactivateUser() {
        if (!this.selectedUserId) return;

        try {
            await api.post(`/users/${this.selectedUserId}/deactivate`);
            NotificationManager.success('User deactivated successfully');
            this.loadUsers();
            ModalManager.hide('delete-modal');
        } catch (error) {
            NotificationManager.error('Failed to deactivate user');
        }
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadUsers();
    }

    handleClick(event) {
        if (event.target.id === 'confirm-delete') {
            this.confirmDelete();
        } else if (event.target.id === 'deactivate-user') {
            this.deactivateUser();
        }
    }
}

let userManager;

document.addEventListener('DOMContentLoaded', () => {
    userManager = new UserManager();
});