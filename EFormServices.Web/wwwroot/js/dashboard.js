// EFormServices.Web/wwwroot/js/dashboard.js
// Got code 30/05/2025
class Dashboard {
    constructor() {
        this.chart = null;
        this.init();
    }

    async init() {
        await this.loadStats();
        await this.loadRecentActivity();
        this.initializeChart();
    }

    async loadStats() {
        try {
            const response = await fetch('/api/dashboard/stats');
            const data = await response.json();
            
            document.getElementById('total-forms').textContent = data.overview.totalForms;
            document.getElementById('active-forms').textContent = data.overview.activeForms;
            document.getElementById('total-submissions').textContent = data.overview.totalSubmissions;
            document.getElementById('pending-approvals').textContent = data.overview.pendingApprovals;

            this.renderTopForms(data.topForms);
            this.updateChart(data.submissionTrend);
        } catch (error) {
            console.error('Failed to load dashboard stats:', error);
            this.showError('Failed to load dashboard statistics');
        }
    }

    async loadRecentActivity() {
        try {
            const response = await fetch('/api/dashboard/recent-activity');
            const activities = await response.json();
            this.renderRecentActivity(activities);
        } catch (error) {
            console.error('Failed to load recent activity:', error);
            document.getElementById('recent-activity').innerHTML = '<p class="text-muted">Failed to load recent activity</p>';
        }
    }

    renderTopForms(forms) {
        const container = document.getElementById('top-forms');
        
        if (!forms || forms.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No forms available</p>';
            return;
        }

        container.innerHTML = forms.map(form => `
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <h6 class="mb-1">${this.escapeHtml(form.title)}</h6>
                    <small class="text-muted">${form.recentSubmissions} recent submissions</small>
                </div>
                <span class="badge bg-primary rounded-pill">${form.submissionCount}</span>
            </div>
        `).join('');
    }

    renderRecentActivity(activities) {
        const container = document.getElementById('recent-activity');
        
        if (!activities || activities.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No recent activity</p>';
            return;
        }

        container.innerHTML = activities.map(activity => `
            <div class="d-flex align-items-center mb-3">
                <div class="me-3">
                    <i class="fas fa-paper-plane text-primary"></i>
                </div>
                <div class="flex-grow-1">
                    <h6 class="mb-1">${this.escapeHtml(activity.formTitle)}</h6>
                    <p class="mb-1 text-muted">Submitted by ${this.escapeHtml(activity.submittedBy)}</p>
                    <small class="text-muted">${this.formatDateTime(activity.submittedAt)} • ${activity.trackingNumber}</small>
                </div>
                <div>
                    <span class="badge bg-${this.getStatusColor(activity.status)}">${activity.status}</span>
                </div>
            </div>
        `).join('');
    }

    initializeChart() {
        const ctx = document.getElementById('submission-chart').getContext('2d');
        this.chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Submissions',
                    data: [],
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                }
            }
        });
    }

    updateChart(trendData) {
        if (!this.chart || !trendData) return;

        const labels = trendData.map(item => this.formatDate(item.date));
        const data = trendData.map(item => item.count);

        this.chart.data.labels = labels;
        this.chart.data.datasets[0].data = data;
        this.chart.update();
    }

    formatDateTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMinutes = Math.floor(diffMs / 60000);

        if (diffMinutes < 1) return 'Just now';
        if (diffMinutes < 60) return `${diffMinutes}m ago`;
        if (diffMinutes < 1440) return `${Math.floor(diffMinutes / 60)}h ago`;
        if (diffMinutes < 10080) return `${Math.floor(diffMinutes / 1440)}d ago`;
        
        return date.toLocaleDateString();
    }

    formatDate(dateString) {
        return new Date(dateString).toLocaleDateString('en-US', { 
            month: 'short', 
            day: 'numeric' 
        });
    }

    getStatusColor(status) {
        const colors = {
            'Submitted': 'primary',
            'PendingApproval': 'warning',
            'Approved': 'success',
            'Rejected': 'danger',
            'Draft': 'secondary'
        };
        return colors[status] || 'secondary';
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showError(message) {
        const toast = document.createElement('div');
        toast.className = 'toast align-items-center text-white bg-danger border-0 position-fixed top-0 end-0 m-3';
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

document.addEventListener('DOMContentLoaded', () => {
    new Dashboard();
});