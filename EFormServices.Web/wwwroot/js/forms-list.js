// EFormServices.Web/wwwroot/js/forms-list.js
// Got code 30/05/2025

class FormsManager {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 20;
        this.filters = {
            searchTerm: '',
            formType: '',
            status: '',
            department: '',
            sortBy: 'createdAt',
            sortDescending: true
        };
        this.selectedForms = new Set();
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadForms();
        this.loadDepartments();
        this.initQRCode();
    }

    bindEvents() {
        const searchInput = document.getElementById('search-input');
        const typeFilter = document.getElementById('type-filter');
        const statusFilter = document.getElementById('status-filter');
        const departmentFilter = document.getElementById('department-filter');
        const sortFilter = document.getElementById('sort-filter');

        if (searchInput) {
            searchInput.addEventListener('input', debounce((e) => {
                this.filters.searchTerm = e.target.value;
                this.currentPage = 1;
                this.loadForms();
            }, 500));
        }

        [typeFilter, statusFilter, departmentFilter, sortFilter].forEach(filter => {
            if (filter) {
                filter.addEventListener('change', (e) => {
                    const filterName = e.target.id.replace('-filter', '').replace('-', '');
                    if (filterName === 'sort') {
                        const [sortBy, order] = e.target.value.split('_');
                        this.filters.sortBy = sortBy;
                        this.filters.sortDescending = order === 'desc';
                    } else {
                        this.filters[filterName === 'type' ? 'formType' : filterName] = e.target.value;
                    }
                    this.currentPage = 1;
                    this.loadForms();
                });
            }
        });

        document.addEventListener('click', this.handleClick.bind(this));
    }

    async loadForms() {
        const tableBody = document.getElementById('forms-table-body');
        if (!tableBody) return;

        LoadingManager.show(tableBody.parentElement);

        try {
            const params = new URLSearchParams({
                page: this.currentPage,
                pageSize: this.pageSize,
                ...Object.fromEntries(Object.entries(this.filters).filter(([_, v]) => v !== ''))
            });

            const data = await api.get(`/forms?${params}`);
            this.renderForms(data.items);
            this.renderPagination(data);
        } catch (error) {
            NotificationManager.error('Failed to load forms');
            tableBody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Error loading forms</td></tr>';
        } finally {
            LoadingManager.hide(tableBody.parentElement);
        }
    }

    renderForms(forms) {
        const tableBody = document.getElementById('forms-table-body');
        if (!tableBody) return;

        if (!forms || forms.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No forms found</td></tr>';
            return;
        }

        tableBody.innerHTML = forms.map(form => `
            <tr data-form-id="${form.id}">
                <td>
                    <div class="d-flex align-items-center">
                        <input type="checkbox" class="form-check-input me-2" data-form-id="${form.id}">
                        <div>
                            <div class="fw-medium">${form.title}</div>
                            <small class="text-muted">${form.description || 'No description'}</small>
                        </div>
                    </div>
                </td>
                <td>
                    <span class="badge bg-secondary">${this.getFormTypeName(form.formType)}</span>
                </td>
                <td>${form.departmentName || 'All Departments'}</td>
                <td>
                    ${form.isPublished 
                        ? '<span class="badge bg-success">Published</span>' 
                        : '<span class="badge bg-warning">Draft</span>'
                    }
                    ${form.isActive 
                        ? '<span class="badge bg-primary ms-1">Active</span>' 
                        : '<span class="badge bg-secondary ms-1">Inactive</span>'
                    }
                </td>
                <td>
                    <span class="fw-medium">${form.submissionCount}</span>
                    ${form.submissionCount > 0 ? `<br><small class="text-muted">Last: ${formatDate(form.updatedAt)}</small>` : ''}
                </td>
                <td>
                    <div>${formatDate(form.createdAt)}</div>
                    <small class="text-muted">by ${form.createdByUserName}</small>
                </td>
                <td>
                    <div class="btn-group" role="group">
                        <button type="button" class="btn btn-sm btn-outline-primary" 
                                onclick="formsManager.editForm(${form.id})" title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-info" 
                                onclick="formsManager.viewForm(${form.id})" title="View">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-success" 
                                onclick="formsManager.shareForm(${form.id}, '${form.formKey}')" title="Share">
                            <i class="fas fa-share"></i>
                        </button>
                        ${!form.isPublished ? `
                            <button type="button" class="btn btn-sm btn-outline-warning" 
                                    onclick="formsManager.publishForm(${form.id})" title="Publish">
                                <i class="fas fa-paper-plane"></i>
                            </button>
                        ` : ''}
                        <button type="button" class="btn btn-sm btn-outline-danger" 
                                onclick="formsManager.deleteForm(${form.id})" title="Delete">
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
                    <a class="page-link" href="#" onclick="formsManager.goToPage(${currentPage - 1})">Previous</a>
                </li>
            `;
        }

        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(totalPages, currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            paginationHTML += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="formsManager.goToPage(${i})">${i}</a>
                </li>
            `;
        }

        if (currentPage < totalPages) {
            paginationHTML += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="formsManager.goToPage(${currentPage + 1})">Next</a>
                </li>
            `;
        }

        pagination.innerHTML = paginationHTML;
    }

    async loadDepartments() {
        try {
            const departments = await api.get('/departments');
            const select = document.getElementById('department-filter');
            if (select && departments) {
                const options = departments.map(dept => 
                    `<option value="${dept.id}">${dept.name}</option>`
                ).join('');
                select.innerHTML = `<option value="">All Departments</option>${options}`;
            }
        } catch (error) {
            console.error('Failed to load departments:', error);
        }
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadForms();
    }

    editForm(formId) {
        window.location.href = `/forms/builder/${formId}`;
    }

    viewForm(formId) {
        window.open(`/forms/${formId}`, '_blank');
    }

    shareForm(formId, formKey) {
        const url = `${window.location.origin}/submit/${formKey}`;
        document.getElementById('form-url').value = url;
        
        if (window.QRCode) {
            const qrContainer = document.getElementById('qr-code');
            qrContainer.innerHTML = '';
            QRCode.toCanvas(qrContainer, url, { width: 200 }, (error) => {
                if (error) console.error(error);
            });
        }

        ModalManager.show('share-modal');
    }

    async publishForm(formId) {
        try {
            await api.post(`/forms/${formId}/publish`);
            NotificationManager.success('Form published successfully');
            this.loadForms();
        } catch (error) {
            NotificationManager.error('Failed to publish form');
        }
    }

    deleteForm(formId) {
        this.selectedFormId = formId;
        ModalManager.show('delete-modal');
    }

    async confirmDelete() {
        if (!this.selectedFormId) return;

        try {
            await api.delete(`/forms/${this.selectedFormId}`);
            NotificationManager.success('Form deleted successfully');
            this.loadForms();
            ModalManager.hide('delete-modal');
        } catch (error) {
            NotificationManager.error('Failed to delete form');
        }
    }

    handleClick(event) {
        if (event.target.id === 'confirm-delete') {
            this.confirmDelete();
        } else if (event.target.id === 'copy-url') {
            this.copyFormUrl();
        }
    }

    copyFormUrl() {
        const urlInput = document.getElementById('form-url');
        if (urlInput) {
            urlInput.select();
            document.execCommand('copy');
            NotificationManager.success('URL copied to clipboard');
        }
    }

    initQRCode() {
        if (typeof QRCode === 'undefined') {
            const script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/npm/qrcode@1.5.3/build/qrcode.min.js';
            document.head.appendChild(script);
        }
    }

    getFormTypeName(type) {
        const types = {
            1: 'Survey',
            2: 'Application', 
            3: 'Request',
            4: 'Feedback',
            5: 'Registration',
            6: 'Assessment',
            7: 'Report',
            8: 'Custom'
        };
        return types[type] || 'Unknown';
    }
}

let formsManager;

document.addEventListener('DOMContentLoaded', () => {
    formsManager = new FormsManager();
});