// EFormServices.Web/wwwroot/js/site.js
// Got code 30/05/2025

class EFormAPI {
    constructor() {
        this.baseUrl = '/api';
        this.token = localStorage.getItem('accessToken');
    }

    async request(url, options = {}) {
        const config = {
            headers: {
                'Content-Type': 'application/json',
                ...(this.token && { 'Authorization': `Bearer ${this.token}` }),
                ...options.headers
            },
            ...options
        };

        try {
            const response = await fetch(`${this.baseUrl}${url}`, config);
            
            if (response.status === 401) {
                await this.refreshToken();
                config.headers['Authorization'] = `Bearer ${this.token}`;
                return fetch(`${this.baseUrl}${url}`, config);
            }

            return response;
        } catch (error) {
            console.error('API request failed:', error);
            throw error;
        }
    }

    async refreshToken() {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
            this.redirectToLogin();
            return;
        }

        try {
            const response = await fetch(`${this.baseUrl}/auth/refresh`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ 
                    accessToken: this.token, 
                    refreshToken 
                })
            });

            if (response.ok) {
                const data = await response.json();
                this.token = data.accessToken;
                localStorage.setItem('accessToken', data.accessToken);
                localStorage.setItem('refreshToken', data.refreshToken);
            } else {
                this.redirectToLogin();
            }
        } catch (error) {
            this.redirectToLogin();
        }
    }

    redirectToLogin() {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
    }

    async get(url) {
        const response = await this.request(url);
        return response.json();
    }

    async post(url, data) {
        const response = await this.request(url, {
            method: 'POST',
            body: JSON.stringify(data)
        });
        return response.json();
    }

    async put(url, data) {
        const response = await this.request(url, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
        return response.json();
    }

    async delete(url) {
        const response = await this.request(url, { method: 'DELETE' });
        return response.ok;
    }
}

class ValidationEngine {
    static rules = {
        required: (value) => value?.trim() !== '',
        email: (value) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
        minLength: (value, min) => value?.length >= min,
        maxLength: (value, max) => value?.length <= max,
        pattern: (value, regex) => new RegExp(regex).test(value),
        number: (value) => !isNaN(value) && isFinite(value)
    };

    static validate(value, rules) {
        const errors = [];
        
        for (const rule of rules) {
            const [ruleName, ...params] = rule.split(':');
            const validator = this.rules[ruleName];
            
            if (validator && !validator(value, ...params)) {
                errors.push(this.getErrorMessage(ruleName, params));
            }
        }
        
        return errors;
    }

    static getErrorMessage(rule, params) {
        const messages = {
            required: 'This field is required',
            email: 'Please enter a valid email address',
            minLength: `Minimum length is ${params[0]} characters`,
            maxLength: `Maximum length is ${params[0]} characters`,
            pattern: 'Invalid format',
            number: 'Please enter a valid number'
        };
        return messages[rule] || 'Invalid value';
    }
}

class StorageManager {
    static set(key, value, expiry = null) {
        const item = {
            value,
            expiry: expiry ? Date.now() + expiry : null
        };
        localStorage.setItem(key, JSON.stringify(item));
    }

    static get(key) {
        const item = localStorage.getItem(key);
        if (!item) return null;

        const parsed = JSON.parse(item);
        if (parsed.expiry && Date.now() > parsed.expiry) {
            localStorage.removeItem(key);
            return null;
        }

        return parsed.value;
    }

    static remove(key) {
        localStorage.removeItem(key);
    }

    static clear() {
        localStorage.clear();
    }
}

class NotificationManager {
    static show(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, duration);
    }

    static success(message) {
        this.show(message, 'success');
    }

    static error(message) {
        this.show(message, 'danger');
    }

    static warning(message) {
        this.show(message, 'warning');
    }

    static info(message) {
        this.show(message, 'info');
    }
}

class LoadingManager {
    static show(element = document.body) {
        const spinner = document.createElement('div');
        spinner.className = 'loading-overlay';
        spinner.innerHTML = `
            <div class="d-flex align-items-center justify-content-center h-100">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `;
        
        const style = document.createElement('style');
        style.textContent = `
            .loading-overlay {
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: rgba(255,255,255,0.8);
                z-index: 1000;
            }
        `;
        
        if (!document.querySelector('style[data-loading]')) {
            style.setAttribute('data-loading', 'true');
            document.head.appendChild(style);
        }

        element.style.position = 'relative';
        element.appendChild(spinner);
        return spinner;
    }

    static hide(element) {
        const overlay = element.querySelector('.loading-overlay');
        if (overlay) overlay.remove();
    }
}

class ModalManager {
    static show(id) {
        const modal = document.getElementById(id);
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
            return bsModal;
        }
    }

    static hide(id) {
        const modal = document.getElementById(id);
        if (modal) {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) bsModal.hide();
        }
    }
}

const debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

const formatDate = (date, format = 'short') => {
    const options = {
        short: { year: 'numeric', month: 'short', day: 'numeric' },
        long: { year: 'numeric', month: 'long', day: 'numeric', weekday: 'long' },
        time: { hour: '2-digit', minute: '2-digit' },
        datetime: { 
            year: 'numeric', month: 'short', day: 'numeric',
            hour: '2-digit', minute: '2-digit'
        }
    };
    
    return new Intl.DateTimeFormat('en-US', options[format]).format(new Date(date));
};

const formatNumber = (number, options = {}) => {
    return new Intl.NumberFormat('en-US', options).format(number);
};

window.EFormAPI = EFormAPI;
window.ValidationEngine = ValidationEngine;
window.StorageManager = StorageManager;
window.NotificationManager = NotificationManager;
window.LoadingManager = LoadingManager;
window.ModalManager = ModalManager;
window.debounce = debounce;
window.formatDate = formatDate;
window.formatNumber = formatNumber;

window.api = new EFormAPI();