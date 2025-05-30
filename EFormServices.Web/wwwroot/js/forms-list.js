// EFormServices.Web/wwwroot/js/forms-list.js
// Got code 30/05/2025
class FormsList {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 20;
        this.totalPages = 1;
        this.filters = {};
        this.searchDebounce = null;
        this.deleteFormId = null;
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadDepartments();
        this.loadForms();
    }

    bindEvents() {
        document.getElementById('search-input').addEventListener('input', (e) => {
            clearTimeout(this.searchDebounce);
            this.searchDebounce = setTimeout(() => {
                this.filters.searchTerm = e.target.value;
                this.resetPagination();
                this.loadForms();
            }, 500);
        });

        ['type-filter', 'status-filter', 'department-filter', 'sort-filter'].forEach(id => {
            document.getElementById(id).addEventListener('change', (e) => {
                const key = id.replace('-filter', '');
                this.filters[key === 'type' ? 'formType' : key === 'status' ? 'isPublished' : key === 'department' ? 'departmentId' : 'sort'] = e.target.value || null;
                this.resetPagination();
                this.loadForms();
            });
        });

        document.getElementById('confirm-delete').addEventListener('click', () => this.deleteForm());
        document.getElementById('copy-url').addEventListener('click', () => this.copyUrl());
    }

    async loadDepartments() {
        try {
            const response = await fetch('/api/departments');
            const departments = await response.json();
            const select = document.getElementById('department-filter');
            departments.forEach(dept => {
                select.innerHTML += `<option value="${dept.id}">${dept.name}</option>`;
            });
        } catch (error) {
            console.error('Failed to load departments:', error);
        }
    }

    async loadForms() {
        try {
            const params = new URLSearchParams({
                page: this.currentPage,
                pageSize: this.pageSize,
                ...this.filters
            });

            const [sortBy, sortDirection] = (this.filters.sort || 'createdAt_desc').split('_');
            params.set('sortBy', sortBy);
            params.set('sortDescending', sortDirection === 'desc');

            const response = await fetch(`/api/forms?${params}`);
            const result = await response.json();
            
            this.renderForms(result.data.items);
            this.renderPagination(result.data);
            
        } catch (error) {
            console.error('Failed to load forms:', error);
            this.renderError();
        }
    }

    renderForms(forms) {
        const tbody = document.getElementById('forms-table-body');
        
        if (!forms || forms.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-4 text-muted">
                        <i class="fas fa-file-alt fa-3x mb-3"></i>
                        <br>No forms found
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = forms.map(form => `
            <tr>
                <td>
                    <div>
                        <strong>${this.escapeHtml(form.title)}</strong>
                        ${form.description ? `<br><small class="text-muted">${this.escapeHtml(form.description.substring(0, 80))}${form.description.length > 80 ? '...' : ''}</small>` : ''}
                    </div>
                </td>
                <td>
                    <span class="badge bg-secondary">${this.getFormTypeLabel(form.formType)}</span>
                </td>
                <td>${form.departmentName || 'All Departments'}</td>
                <td>
                    ${form.isPublished 
                        ? '<span class="badge bg-success">Published</span>' 
                        : '<span class="badge bg-warning">Draft</span>'}
                    ${!form.isActive ? '<span class="badge bg-danger ms-1">Inactive</span>' : ''}
                </td>
                <td>
                    <span class="fw-bold">${form.submissionCount}</span>
                    ${form.submissionCount > 0 ? `<br><a href="/forms/${form.id}/submissions" class="small">View submissions</a>` : ''}
                </td>
                <td>
                    <div>${this.formatDate(form.createdAt)}</div>
                    <small class="text-muted">by ${this.escapeHtml(form.createdByUserName)}</small>
                </td>
                <td>
                    <div class="dropdown">
                        <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
                            <i class="fas fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="/forms/${form.id}"><i class="fas fa-eye me-2"></i>View</a></li>
                            <li><a class="dropdown-item" href="/forms/builder/${form.id}"><i class="fas fa-edit me-2"></i>Edit</a></li>
                            ${form.isPublished ? `
                                <li><button type="button" class="dropdown-item" onclick="formsList.shareForm(${form.id}, '${form.title}')">
                                    <i class="fas fa-share me-2"></i>Share
                                </button></li>
                                <li><button type="button" class="dropdown-item" onclick="formsList.duplicateForm(${form.id})">
                                    <i class="fas fa-copy me-2"></i>Duplicate
                                </button></li>
                            ` : ''}
                            <li><hr class="dropdown-divider"></li>
                            <li><button type="button" class="dropdown-item text-danger" onclick="formsList.confirmDelete(${form.id}, '${this.escapeHtml(form.title)}')">
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
                    <button class="page-link" onclick="formsList.goToPage(${this.currentPage - 1})">Previous</button>
                </li>
            `;
        }

        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(this.totalPages, this.currentPage + 2);

        if (startPage > 1) {
            paginationHtml += `<li class="page-item"><button class="page-link" onclick="formsList.goToPage(1)">1</button></li>`;
            if (startPage > 2) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
                <li class="page-item ${i === this.currentPage ? 'active' : ''}">
                    <button class="page-link" onclick="formsList.goToPage(${i})">${i}</button>
                </li>
            `;
        }

        if (endPage < this.totalPages) {
            if (endPage < this.totalPages - 1) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHtml += `<li class="page-item"><button class="page-link" onclick="formsList.goToPage(${this.totalPages})">${this.totalPages}</button></li>`;
        }

        if (this.currentPage < this.totalPages) {
            paginationHtml += `
                <li class="page-item">
                    <button class="page-link" onclick="formsList.goToPage(${this.currentPage + 1})">Next</button>
                </li>
            `;
        }

        pagination.innerHTML = paginationHtml;
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadForms();
    }

    resetPagination() {
        this.currentPage = 1;
    }

    confirmDelete(formId, formTitle) {
        this.deleteFormId = formId;
        document.querySelector('#delete-modal .modal-body p').textContent = `Are you sure you want to delete "${formTitle}"?`;
        new bootstrap.Modal(document.getElementById('delete-modal')).show();
    }

    async deleteForm() {
        try {
            const response = await fetch(`/api/forms/${this.deleteFormId}`, {
                method: 'DELETE'
            });

            if (!response.ok) throw new Error('Failed to delete form');

            this.showToast('Form deleted successfully', 'success');
            bootstrap.Modal.getInstance(document.getElementById('delete-modal')).hide();
            this.loadForms();
            
        } catch (error) {
            console.error('Delete error:', error);
            this.showToast('Failed to delete form', 'error');
        }
    }

    async duplicateForm(formId) {
        try {
            const response = await fetch(`/api/forms/${formId}/duplicate`, {
                method: 'POST'
            });

            if (!response.ok) throw new Error('Failed to duplicate form');

            const result = await response.json();
            this.showToast('Form duplicated successfully', 'success');
            window.location.href = `/forms/builder/${result.data.id}`;
            
        } catch (error) {
            console.error('Duplicate error:', error);
            this.showToast('Failed to duplicate form', 'error');
        }
    }

    shareForm(formId, formTitle) {
        const baseUrl = window.location.origin;
        const formUrl = `${baseUrl}/submit/${formId}`;
        
        document.getElementById('form-url').value = formUrl;
        document.querySelector('#share-modal .modal-title').textContent = `Share: ${formTitle}`;
        
        QRCode.toCanvas(document.getElementById('qr-code'), formUrl, {
            width: 200,
            margin: 2
        });

        new bootstrap.Modal(document.getElementById('share-modal')).show();
    }

    async copyUrl() {
        const urlInput = document.getElementById('form-url');
        try {
            await navigator.clipboard.writeText(urlInput.value);
            this.showToast('URL copied to clipboard', 'success');
        } catch (error) {
            urlInput.select();
            document.execCommand('copy');
            this.showToast('URL copied to clipboard', 'success');
        }
    }

    renderError() {
        document.getElementById('forms-table-body').innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-danger">
                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                    <br>Failed to load forms. Please try again.
                </td>
            </tr>
        `;
    }

    getFormTypeLabel(type) {
        const types = {
            1: 'Survey', 2: 'Application', 3: 'Request', 4: 'Feedback',
            5: 'Registration', 6: 'Assessment', 7: 'Report', 8: 'Custom'
        };
        return types[type] || 'Unknown';
    }

    formatDate(dateString) {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showToast(message, type) {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0 position-fixed top-0 end-0 m-3`;
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        document.body.appendChild(toast);
        new bootstrap.Toast(toast).show();
    }
}

let formsList;
document.addEventListener('DOMContentLoaded', () => {
    formsList = new FormsList();
});