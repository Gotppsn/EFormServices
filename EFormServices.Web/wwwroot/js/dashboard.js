// EFormServices.Web/wwwroot/js/dashboard.js
// Got code 30/05/2025

class DashboardManager {
    constructor() {
        this.chart = null;
        this.refreshInterval = null;
        this.init();
    }

    async init() {
        await this.loadDashboardData();
        this.initChart();
        this.startAutoRefresh();
    }

    async loadDashboardData() {
        try {
            const data = await api.get('/forms/analytics');
            this.updateMetrics(data);
            this.updateTopForms(data.topForms);
            this.updateRecentActivity();
            return data;
        } catch (error) {
            NotificationManager.error('Failed to load dashboard data');
            console.error('Dashboard data error:', error);
        }
    }

    updateMetrics(data) {
        document.getElementById('total-forms').textContent = data.totalForms || 0;
        document.getElementById('active-forms').textContent = data.activeForms || 0;
        document.getElementById('total-submissions').textContent = data.totalSubmissions || 0;
        document.getElementById('pending-approvals').textContent = data.pendingApprovals || 0;
    }

    initChart() {
        const ctx = document.getElementById('submission-chart');
        if (!ctx) return;

        const data = {
            labels: this.getLast30Days(),
            datasets: [{
                label: 'Submissions',
                data: this.generateSubmissionData(),
                borderColor: '#0d6efd',
                backgroundColor: 'rgba(13, 110, 253, 0.1)',
                tension: 0.4,
                fill: true
            }]
        };

        this.chart = new Chart(ctx, {
            type: 'line',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 }
                    },
                    x: {
                        ticks: { maxTicksLimit: 7 }
                    }
                }
            }
        });
    }

    getLast30Days() {
        const days = [];
        for (let i = 29; i >= 0; i--) {
            const date = new Date();
            date.setDate(date.getDate() - i);
            days.push(date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
        }
        return days;
    }

    generateSubmissionData() {
        return Array.from({ length: 30 }, () => Math.floor(Math.random() * 20) + 5);
    }

    updateTopForms(topForms) {
        const container = document.getElementById('top-forms');
        if (!container || !topForms) return;

        container.innerHTML = topForms.map((form, index) => `
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <h6 class="mb-1">${form.title}</h6>
                    <small class="text-muted">Rank #${index + 1}</small>
                </div>
                <span class="badge bg-primary rounded-pill">${form.submissions}</span>
            </div>
        `).join('');
    }

    async updateRecentActivity() {
        const container = document.getElementById('recent-activity');
        if (!container) return;

        const activities = [
            { type: 'submission', user: 'John Doe', form: 'Leave Request', time: '2 minutes ago', icon: 'paper-plane' },
            { type: 'approval', user: 'Jane Smith', form: 'Expense Report', time: '15 minutes ago', icon: 'check-circle' },
            { type: 'creation', user: 'Bob Johnson', form: 'Survey Form', time: '1 hour ago', icon: 'plus-circle' },
            { type: 'rejection', user: 'Alice Brown', form: 'Training Request', time: '2 hours ago', icon: 'x-circle' }
        ];

        container.innerHTML = activities.map(activity => `
            <div class="d-flex align-items-center mb-3">
                <div class="me-3">
                    <i class="fas fa-${activity.icon} text-${this.getActivityColor(activity.type)}"></i>
                </div>
                <div class="flex-grow-1">
                    <div class="fw-medium">${activity.user}</div>
                    <div class="text-muted small">
                        ${this.getActivityAction(activity.type)} "${activity.form}"
                    </div>
                </div>
                <div class="text-muted small">${activity.time}</div>
            </div>
        `).join('');
    }

    getActivityColor(type) {
        const colors = {
            submission: 'primary',
            approval: 'success',
            creation: 'info',
            rejection: 'danger'
        };
        return colors[type] || 'secondary';
    }

    getActivityAction(type) {
        const actions = {
            submission: 'submitted',
            approval: 'approved',
            creation: 'created',
            rejection: 'rejected'
        };
        return actions[type] || 'updated';
    }

    startAutoRefresh() {
        this.refreshInterval = setInterval(async () => {
            const data = await this.loadDashboardData();
            if (this.chart && data?.submissionTrends) {
                this.chart.data.datasets[0].data = data.submissionTrends.map(t => t.count);
                this.chart.update();
            }
        }, 300000);
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }
}

class MetricsAnimation {
    static animateCounter(element, target, duration = 1000) {
        const start = parseInt(element.textContent) || 0;
        const increment = (target - start) / (duration / 16);
        let current = start;

        const timer = setInterval(() => {
            current += increment;
            if ((increment > 0 && current >= target) || (increment < 0 && current <= target)) {
                current = target;
                clearInterval(timer);
            }
            element.textContent = Math.floor(current);
        }, 16);
    }

    static animateMetrics() {
        const metrics = [
            { id: 'total-forms', target: 15 },
            { id: 'active-forms', target: 12 },
            { id: 'total-submissions', target: 247 },
            { id: 'pending-approvals', target: 8 }
        ];

        metrics.forEach(metric => {
            const element = document.getElementById(metric.id);
            if (element) {
                this.animateCounter(element, metric.target);
            }
        });
    }
}

class DashboardFilters {
    constructor() {
        this.dateRange = '30d';
        this.department = null;
        this.initFilters();
    }

    initFilters() {
        const dateRangeSelect = document.getElementById('date-range-filter');
        const departmentSelect = document.getElementById('department-filter');

        if (dateRangeSelect) {
            dateRangeSelect.addEventListener('change', (e) => {
                this.dateRange = e.target.value;
                this.applyFilters();
            });
        }

        if (departmentSelect) {
            departmentSelect.addEventListener('change', (e) => {
                this.department = e.target.value || null;
                this.applyFilters();
            });
        }
    }

    async applyFilters() {
        LoadingManager.show(document.querySelector('.container-fluid'));
        
        try {
            const params = new URLSearchParams();
            if (this.dateRange) params.append('range', this.dateRange);
            if (this.department) params.append('department', this.department);

            const data = await api.get(`/forms/analytics?${params}`);
            dashboard.updateMetrics(data);
            
            if (dashboard.chart && data.submissionTrends) {
                dashboard.chart.data.datasets[0].data = data.submissionTrends.map(t => t.count);
                dashboard.chart.update();
            }
        } catch (error) {
            NotificationManager.error('Failed to apply filters');
        } finally {
            LoadingManager.hide(document.querySelector('.container-fluid'));
        }
    }
}

let dashboard;
let dashboardFilters;

document.addEventListener('DOMContentLoaded', () => {
    dashboard = new DashboardManager();
    dashboardFilters = new DashboardFilters();
    
    setTimeout(() => {
        MetricsAnimation.animateMetrics();
    }, 500);
});

window.addEventListener('beforeunload', () => {
    if (dashboard) dashboard.destroy();
});