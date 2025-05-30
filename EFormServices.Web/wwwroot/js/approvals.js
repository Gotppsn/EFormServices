// EFormServices.Web/wwwroot/js/approvals.js
// Got code 30/05/2025
class ApprovalManager {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 20;
        this.filters = {};
        this.selectedApprovals = new Set();
        this.currentApproval = null;
        this.refreshInterval = null;
        this.searchDebounce = null;
        this.init();
    }

    async init() {
        this.bindEvents();
        await this.loadFilters();
        await this.loadApprovals();
        await this.loadStats();
        this.startAutoRefresh();
    }

    bindEvents() {
        document.getElementById('search-input').addEventListener('input', (e) => {
            clearTimeout(this.searchDebounce);
            this.searchDebounce = setTimeout(() => {
                this.filters.searchTerm = e.target.value;
                this.resetPagination();
                this.loadApprovals();
            }, 500);
        });

        ['priority-filter', 'form-filter', 'step-filter', 'sort-filter'].forEach(id => {
            document.getElementById(id).addEventListener('change', (e) => {
                const key = id.replace('-filter', '');
                this.filters[key] = e.target.value || null;
                this.resetPagination();
                this.loadApprovals();
            });
        });

        document.getElementById('select-all').addEventListener('change', (e) => this.toggleSelectAll(e.target.checked));
        document.getElementById('header-checkbox').addEventListener('change', (e) => this.toggleSelectAll(e.target.checked));
        document.getElementById('refresh-btn').addEventListener('click', () => this.refreshData());
        document.getElementById('bulk-approve-btn').addEventListener('click', () => this.showBulkApproval());

        document.getElementById('approve-btn').addEventListener('click', () => this.processApproval(true));
        document.getElementById('reject-btn').addEventListener('click', () => this.processApproval(false));
        document.getElementById('confirm-bulk-approve').addEventListener('click', () => this.processBulkApproval());

        document.getElementById('approval-modal').addEventListener('hidden.bs.modal', () => this.resetApprovalModal());
    }

    async loadFilters() {
        try {
            const [formsResponse, stepsResponse] = await Promise.all([
                fetch('/api/forms?pageSize=100'),
                fetch('/api/workflows/steps')
            ]);

            const forms = await formsResponse.json();
            const steps = await stepsResponse.json();

            this.populateFormFilter(forms.data?.items || []);
            this.populateStepFilter(steps || []);
        } catch (error) {
            console.error('Failed to load filters:', error);
        }
    }

    populateFormFilter(forms) {
        const select = document.getElementById('form-filter');
        forms.forEach(form => {
            select.innerHTML += `<option value="${form.id}">${form.title}</option>`;
        });
    }

    populateStepFilter(steps) {
        const select = document.getElementById('step-filter');
        const uniqueSteps = [...new Set(steps.map(s => s.stepName))];
        uniqueSteps.forEach(step => {
            select.innerHTML += `<option value="${step}">${step}</option>`;
        });
    }

    async loadApprovals() {
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

            const response = await fetch(`/api/approvals/pending?${params}`);
            const result = await response.json();
            
            this.renderApprovals(result.items || []);
            this.renderPagination(result);
            this.updateSelectionUI();
            
        } catch (error) {
            console.error('Failed to load approvals:', error);
            this.renderError();
        }
    }

    async loadStats() {
        try {
            const response = await fetch('/api/approvals/stats');
            const stats = await response.json();
            
            document.getElementById('pending-count').textContent = stats.pendingCount || 0;
            document.getElementById('overdue-count').textContent = stats.overdueCount || 0;
            document.getElementById('my-queue-count').textContent = stats.myQueueCount || 0;
            document.getElementById('avg-time').textContent = stats.avgReviewTime || '0h';
        } catch (error) {
            console.error('Failed to load stats:', error);
        }
    }

    renderApprovals(approvals) {
        const tbody = document.getElementById('approvals-table-body');
        
        if (!approvals.length) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-4 text-muted">
                        <i class="fas fa-clipboard-check fa-3x mb-3"></i>
                        <br>No pending approvals found
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = approvals.map(approval => `
            <tr class="${this.selectedApprovals.has(approval.processId) ? 'row-selected' : ''}" 
                data-process-id="${approval.processId}">
                <td>
                    <input type="checkbox" class="form-check-input approval-checkbox" 
                           value="${approval.processId}" ${this.selectedApprovals.has(approval.processId) ? 'checked' : ''}>
                </td>
                <td>
                    <div>
                        <strong>${approval.trackingNumber}</strong>
                        <br><small class="text-muted">by ${this.escapeHtml(approval.submittedBy)}</small>
                    </div>
                </td>
                <td>
                    <div>
                        <strong>${this.escapeHtml(approval.formTitle)}</strong>
                        <br><small class="text-muted">${this.formatDate(approval.submittedAt)}</small>
                    </div>
                </td>
                <td>
                    <span class="badge bg-primary">${this.escapeHtml(approval.currentStep)}</span>
                </td>
                <td>
                    <span class="badge priority-badge ${this.getPriorityClass(approval.daysWaiting)}">
                        ${this.getPriorityText(approval.daysWaiting)}
                    </span>
                </td>
                <td>
                    <div>
                        <strong>${approval.daysWaiting} day${approval.daysWaiting !== 1 ? 's' : ''}</strong>
                        <br><small class="text-muted">Since ${this.formatDate(approval.submittedAt)}</small>
                    </div>
                </td>
                <td>
                    <div class="btn-group">
                        <button type="button" class="btn btn-sm btn-outline-primary" 
                                onclick="approvalManager.reviewSubmission(${approval.processId})">
                            <i class="fas fa-eye"></i> Review
                        </button>
                        <button type="button" class="btn btn-sm btn-success" 
                                onclick="approvalManager.quickApprove(${approval.processId})">
                            <i class="fas fa-check"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-danger" 
                                onclick="approvalManager.quickReject(${approval.processId})">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');

        this.bindCheckboxEvents();
    }

    bindCheckboxEvents() {
        document.querySelectorAll('.approval-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                const processId = parseInt(e.target.value);
                const row = e.target.closest('tr');
                
                if (e.target.checked) {
                    this.selectedApprovals.add(processId);
                    row.classList.add('row-selected');
                } else {
                    this.selectedApprovals.delete(processId);
                    row.classList.remove('row-selected');
                }
                
                this.updateSelectionUI();
            });
        });
    }

    updateSelectionUI() {
        const count = this.selectedApprovals.size;
        const bulkBtn = document.getElementById('bulk-approve-btn');
        
        if (count > 0) {
            bulkBtn.style.display = 'inline-block';
            bulkBtn.innerHTML = `<i class="fas fa-check"></i> Approve Selected (${count})`;
        } else {
            bulkBtn.style.display = 'none';
        }

        const allCheckboxes = document.querySelectorAll('.approval-checkbox');
        const checkedCount = document.querySelectorAll('.approval-checkbox:checked').length;
        const headerCheckbox = document.getElementById('header-checkbox');
        
        headerCheckbox.indeterminate = checkedCount > 0 && checkedCount < allCheckboxes.length;
        headerCheckbox.checked = checkedCount === allCheckboxes.length && allCheckboxes.length > 0;
    }

    toggleSelectAll(checked) {
        document.querySelectorAll('.approval-checkbox').forEach(checkbox => {
            const processId = parseInt(checkbox.value);
            const row = checkbox.closest('tr');
            
            checkbox.checked = checked;
            
            if (checked) {
                this.selectedApprovals.add(processId);
                row.classList.add('row-selected');
            } else {
                this.selectedApprovals.delete(processId);
                row.classList.remove('row-selected');
            }
        });
        
        this.updateSelectionUI();
    }

    async reviewSubmission(processId) {
        try {
            const response = await fetch(`/api/approvals/${processId}`);
            const approval = await response.json();
            
            this.currentApproval = approval;
            this.populateApprovalModal(approval);
            await this.loadSubmissionData(approval.submissionId);
            await this.loadApprovalHistory(processId);
            
            new bootstrap.Modal(document.getElementById('approval-modal')).show();
        } catch (error) {
            console.error('Failed to load approval details:', error);
            this.showToast('Failed to load approval details', 'error');
        }
    }

    populateApprovalModal(approval) {
        document.getElementById('review-form-title').textContent = approval.formTitle;
        document.getElementById('review-submitted-by').textContent = approval.submittedBy;
        document.getElementById('review-tracking-number').textContent = approval.trackingNumber;
        document.getElementById('review-submitted-at').textContent = this.formatDateTime(approval.submittedAt);
    }

    async loadSubmissionData(submissionId) {
        try {
            const response = await fetch(`/api/submissions/${submissionId}`);
            const submission = await response.json();
            
            const container = document.getElementById('submission-data');
            container.innerHTML = Object.entries(submission.values || {}).map(([key, value]) => `
                <div class="row mb-2">
                    <div class="col-md-4"><strong>${this.escapeHtml(key)}:</strong></div>
                    <div class="col-md-8">${this.escapeHtml(value)}</div>
                </div>
            `).join('');
            
            if (submission.files && submission.files.length) {
                container.innerHTML += `
                    <hr>
                    <h6>Attachments</h6>
                    ${submission.files.map(file => `
                        <div class="d-flex align-items-center mb-2">
                            <i class="fas fa-file me-2"></i>
                            <a href="/api/files/${file.id}" target="_blank">${file.fileName}</a>
                            <small class="text-muted ms-2">(${this.formatFileSize(file.fileSize)})</small>
                        </div>
                    `).join('')}
                `;
            }
        } catch (error) {
            console.error('Failed to load submission data:', error);
            document.getElementById('submission-data').innerHTML = 
                '<div class="text-danger">Failed to load submission data</div>';
        }
    }

    async loadApprovalHistory(processId) {
        try {
            const response = await fetch(`/api/approvals/${processId}/history`);
            const history = await response.json();
            
            if (history.length > 0) {
                const container = document.getElementById('approval-history');
                container.innerHTML = history.map(action => `
                    <div class="timeline-item ${action.action === 'Reject' ? 'rejected' : ''}">
                        <div class="d-flex justify-content-between">
                            <strong>${action.stepName}</strong>
                            <small class="text-muted">${this.formatDateTime(action.actionAt)}</small>
                        </div>
                        <div class="text-muted">
                            ${action.action} by ${action.actionBy}
                        </div>
                        ${action.comments ? `<div class="mt-1">"${this.escapeHtml(action.comments)}"</div>` : ''}
                    </div>
                `).join('');
                
                document.getElementById('approval-history-section').style.display = 'block';
            }
        } catch (error) {
            console.error('Failed to load approval history:', error);
        }
    }

    async processApproval(approve) {
        const comments = document.getElementById('approval-comments').value;
        
        if (!approve && !comments.trim()) {
            this.showToast('Comments are required for rejection', 'error');
            return;
        }

        try {
            const endpoint = approve ? 'approve' : 'reject';
            const response = await fetch(`/api/approvals/${this.currentApproval.processId}/${endpoint}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ comments })
            });

            if (!response.ok) throw new Error(`Failed to ${endpoint} submission`);

            this.showToast(`Submission ${approve ? 'approved' : 'rejected'} successfully`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('approval-modal')).hide();
            this.loadApprovals();
            this.loadStats();
            
        } catch (error) {
            console.error('Process approval error:', error);
            this.showToast(`Failed to ${approve ? 'approve' : 'reject'} submission`, 'error');
        }
    }

    async quickApprove(processId) {
        try {
            const response = await fetch(`/api/approvals/${processId}/approve`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ comments: 'Quick approval' })
            });

            if (!response.ok) throw new Error('Failed to approve submission');

            this.showToast('Submission approved successfully', 'success');
            this.loadApprovals();
            this.loadStats();
            
        } catch (error) {
            console.error('Quick approve error:', error);
            this.showToast('Failed to approve submission', 'error');
        }
    }

    async quickReject(processId) {
        const comments = prompt('Please provide a reason for rejection:');
        if (!comments) return;

        try {
            const response = await fetch(`/api/approvals/${processId}/reject`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ comments })
            });

            if (!response.ok) throw new Error('Failed to reject submission');

            this.showToast('Submission rejected successfully', 'success');
            this.loadApprovals();
            this.loadStats();
            
        } catch (error) {
            console.error('Quick reject error:', error);
            this.showToast('Failed to reject submission', 'error');
        }
    }

    showBulkApproval() {
        document.getElementById('bulk-count').textContent = this.selectedApprovals.size;
        new bootstrap.Modal(document.getElementById('bulk-action-modal')).show();
    }

    async processBulkApproval() {
        const comments = document.getElementById('bulk-comments').value;
        const processIds = Array.from(this.selectedApprovals);

        try {
            const promises = processIds.map(processId => 
                fetch(`/api/approvals/${processId}/approve`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ comments: comments || 'Bulk approval' })
                })
            );

            await Promise.all(promises);

            this.showToast(`${processIds.length} submissions approved successfully`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('bulk-action-modal')).hide();
            this.selectedApprovals.clear();
            this.loadApprovals();
            this.loadStats();
            
        } catch (error) {
            console.error('Bulk approval error:', error);
            this.showToast('Failed to approve some submissions', 'error');
        }
    }

    startAutoRefresh() {
        this.refreshInterval = setInterval(() => {
            this.loadStats();
            if (this.currentPage === 1 && !Object.keys(this.filters).length) {
                this.loadApprovals();
            }
        }, 30000);
    }

    refreshData() {
        this.loadApprovals();
        this.loadStats();
        this.showToast('Data refreshed', 'info', 2000);
    }

    renderPagination(data) {
        const pagination = document.getElementById('pagination');
        this.totalPages = data.totalPages || 1;
        
        if (this.totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let paginationHtml = '';
        
        if (this.currentPage > 1) {
            paginationHtml += `
                <li class="page-item">
                    <button class="page-link" onclick="approvalManager.goToPage(${this.currentPage - 1})">Previous</button>
                </li>
            `;
        }

        for (let i = Math.max(1, this.currentPage - 2); i <= Math.min(this.totalPages, this.currentPage + 2); i++) {
            paginationHtml += `
                <li class="page-item ${i === this.currentPage ? 'active' : ''}">
                    <button class="page-link" onclick="approvalManager.goToPage(${i})">${i}</button>
                </li>
            `;
        }

        if (this.currentPage < this.totalPages) {
            paginationHtml += `
                <li class="page-item">
                    <button class="page-link" onclick="approvalManager.goToPage(${this.currentPage + 1})">Next</button>
                </li>
            `;
        }

        pagination.innerHTML = paginationHtml;
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadApprovals();
    }

    resetPagination() {
        this.currentPage = 1;
    }

    resetApprovalModal() {
        document.getElementById('approval-comments').value = '';
        document.getElementById('approval-history-section').style.display = 'none';
        this.currentApproval = null;
    }

    getPriorityClass(days) {
        if (days >= 7) return 'overdue';
        if (days >= 5) return 'urgent';
        return 'normal';
    }

    getPriorityText(days) {
        if (days >= 7) return 'Overdue';
        if (days >= 5) return 'Urgent';
        return 'Normal';
    }

    formatDate(dateString) {
        return new Date(dateString).toLocaleDateString('en-US', { 
            year: 'numeric', month: 'short', day: 'numeric' 
        });
    }

    formatDateTime(dateString) {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric', month: 'short', day: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    renderError() {
        document.getElementById('approvals-table-body').innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-danger">
                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                    <br>Failed to load approvals. Please refresh the page.
                </td>
            </tr>
        `;
    }

    showToast(message, type, duration = 5000) {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} border-0 position-fixed top-0 end-0 m-3`;
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        document.body.appendChild(toast);
        new bootstrap.Toast(toast, { delay: duration }).show();
    }

    destroy() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
        }
    }
}

let approvalManager;
document.addEventListener('DOMContentLoaded', () => {
    approvalManager = new ApprovalManager();
});

window.addEventListener('beforeunload', () => {
    if (approvalManager) {
        approvalManager.destroy();
    }
});