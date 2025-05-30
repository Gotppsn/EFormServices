// EFormServices.Web/wwwroot/js/site.js
// Got code 30/05/2025
class ApiClient {
    constructor() {
        this.baseUrl = '';
        this.defaultHeaders = {
            'Content-Type': 'application/json'
        };
        this.requestInterceptors = [];
        this.responseInterceptors = [];
        this.setupInterceptors();
    }

    setupInterceptors() {
        this.addRequestInterceptor((config) => {
            const token = localStorage.getItem('accessToken');
            if (token) {
                config.headers['Authorization'] = `Bearer ${token}`;
            }
            return config;
        });

        this.addResponseInterceptor(
            (response) => response,
            async (error) => {
                if (error.status === 401) {
                    await this.handleTokenRefresh();
                    return this.retryRequest(error.config);
                }
                throw error;
            }
        );
    }

    addRequestInterceptor(interceptor) {
        this.requestInterceptors.push(interceptor);
    }

    addResponseInterceptor(onSuccess, onError) {
        this.responseInterceptors.push({ onSuccess, onError });
    }

    async request(url, options = {}) {
        const config = {
            url: `${this.baseUrl}${url}`,
            method: 'GET',
            headers: { ...this.defaultHeaders },
            ...options
        };

        for (const interceptor of this.requestInterceptors) {
            Object.assign(config, interceptor(config));
        }

        try {
            const response = await fetch(config.url, {
                method: config.method,
                headers: config.headers,
                body: config.body
            });

            const result = { ...response, config };

            for (const { onSuccess } of this.responseInterceptors) {
                if (onSuccess) await onSuccess(result);
            }

            return result;
        } catch (error) {
            const enrichedError = { ...error, config };
            
            for (const { onError } of this.responseInterceptors) {
                if (onError) {
                    try {
                        return await onError(enrichedError);
                    } catch (interceptorError) {
                        throw interceptorError;
                    }
                }
            }
            throw enrichedError;
        }
    }

    async handleTokenRefresh() {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
            this.redirectToLogin();
            return;
        }

        try {
            const response = await fetch('/api/auth/refresh', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ 
                    accessToken: localStorage.getItem('accessToken'),
                    refreshToken 
                })
            });

            if (response.ok) {
                const data = await response.json();
                localStorage.setItem('accessToken', data.accessToken);
                localStorage.setItem('refreshToken', data.refreshToken);
            } else {
                this.redirectToLogin();
            }
        } catch {
            this.redirectToLogin();
        }
    }

    redirectToLogin() {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
    }

    async retryRequest(config) {
        return this.request(config.url, config);
    }

    get(url, options = {}) {
        return this.request(url, { ...options, method: 'GET' });
    }

    post(url, data, options = {}) {
        return this.request(url, {
            ...options,
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    put(url, data, options = {}) {
        return this.request(url, {
            ...options,
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    delete(url, options = {}) {
        return this.request(url, { ...options, method: 'DELETE' });
    }

    upload(url, formData, options = {}) {
        const uploadHeaders = { ...this.defaultHeaders };
        delete uploadHeaders['Content-Type'];
        
        return this.request(url, {
            ...options,
            method: 'POST',
            headers: uploadHeaders,
            body: formData
        });
    }
}

class ValidationEngine {
    constructor() {
        this.rules = new Map();
        this.customValidators = new Map();
        this.messages = {
            required: 'This field is required',
            email: 'Please enter a valid email address',
            minLength: 'Minimum length is {0} characters',
            maxLength: 'Maximum length is {0} characters',
            pattern: 'Please enter a valid format',
            number: 'Please enter a valid number',
            url: 'Please enter a valid URL',
            date: 'Please enter a valid date'
        };
    }

    addRule(fieldName, validators) {
        this.rules.set(fieldName, validators);
    }

    addCustomValidator(name, validator) {
        this.customValidators.set(name, validator);
    }

    validateField(element) {
        const fieldName = element.name || element.id;
        const validators = this.rules.get(fieldName) || [];
        const value = element.value;
        
        for (const validator of validators) {
            const result = this.runValidator(validator, value, element);
            if (!result.isValid) {
                this.showFieldError(element, result.message);
                return false;
            }
        }
        
        this.clearFieldError(element);
        return true;
    }

    validateForm(form) {
        const elements = form.querySelectorAll('input, select, textarea');
        let isValid = true;
        
        elements.forEach(element => {
            if (!this.validateField(element)) {
                isValid = false;
            }
        });
        
        return isValid;
    }

    runValidator(validator, value, element) {
        if (typeof validator === 'string') {
            return this.runBuiltInValidator(validator, value);
        }
        
        if (typeof validator === 'object') {
            const { type, params = [], message } = validator;
            const result = this.runBuiltInValidator(type, value, ...params);
            if (!result.isValid && message) {
                result.message = this.formatMessage(message, params);
            }
            return result;
        }
        
        if (typeof validator === 'function') {
            return validator(value, element);
        }
        
        return { isValid: true };
    }

    runBuiltInValidator(type, value, ...params) {
        switch (type) {
            case 'required':
                return {
                    isValid: value.trim() !== '',
                    message: this.messages.required
                };
            case 'email':
                return {
                    isValid: !value || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
                    message: this.messages.email
                };
            case 'minLength':
                return {
                    isValid: !value || value.length >= params[0],
                    message: this.formatMessage(this.messages.minLength, params)
                };
            case 'maxLength':
                return {
                    isValid: !value || value.length <= params[0],
                    message: this.formatMessage(this.messages.maxLength, params)
                };
            case 'pattern':
                return {
                    isValid: !value || new RegExp(params[0]).test(value),
                    message: this.messages.pattern
                };
            case 'number':
                return {
                    isValid: !value || !isNaN(value),
                    message: this.messages.number
                };
            case 'url':
                try {
                    if (value) new URL(value);
                    return { isValid: true };
                } catch {
                    return { isValid: false, message: this.messages.url };
                }
            default:
                const customValidator = this.customValidators.get(type);
                if (customValidator) {
                    return customValidator(value, ...params);
                }
                return { isValid: true };
        }
    }

    formatMessage(message, params) {
        return message.replace(/\{(\d+)\}/g, (match, index) => params[index] || match);
    }

    showFieldError(element, message) {
        element.classList.add('is-invalid');
        
        let feedback = element.parentNode.querySelector('.invalid-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            element.parentNode.appendChild(feedback);
        }
        feedback.textContent = message;
    }

    clearFieldError(element) {
        element.classList.remove('is-invalid');
        const feedback = element.parentNode.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.textContent = '';
        }
    }
}

class StorageManager {
    constructor() {
        this.prefix = 'eformservices_';
        this.encrypted = true;
    }

    set(key, value, options = {}) {
        const { expiry, encrypt = this.encrypted } = options;
        const data = {
            value,
            timestamp: Date.now(),
            expiry: expiry ? Date.now() + expiry : null
        };
        
        const serialized = JSON.stringify(data);
        const stored = encrypt ? this.encrypt(serialized) : serialized;
        
        try {
            localStorage.setItem(this.prefix + key, stored);
            return true;
        } catch (error) {
            console.warn('Storage failed:', error);
            return false;
        }
    }

    get(key, defaultValue = null) {
        try {
            const stored = localStorage.getItem(this.prefix + key);
            if (!stored) return defaultValue;
            
            const decrypted = this.encrypted ? this.decrypt(stored) : stored;
            const data = JSON.parse(decrypted);
            
            if (data.expiry && Date.now() > data.expiry) {
                this.remove(key);
                return defaultValue;
            }
            
            return data.value;
        } catch (error) {
            console.warn('Storage retrieval failed:', error);
            return defaultValue;
        }
    }

    remove(key) {
        localStorage.removeItem(this.prefix + key);
    }

    clear() {
        Object.keys(localStorage)
            .filter(key => key.startsWith(this.prefix))
            .forEach(key => localStorage.removeItem(key));
    }

    encrypt(data) {
        return btoa(data);
    }

    decrypt(data) {
        return atob(data);
    }
}

class UtilityFunctions {
    static debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    static throttle(func, limit) {
        let inThrottle;
        return function(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }

    static formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    static formatDate(date, format = 'short') {
        const d = new Date(date);
        const formats = {
            short: { month: 'short', day: 'numeric', year: 'numeric' },
            long: { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' },
            time: { hour: '2-digit', minute: '2-digit' },
            datetime: { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' }
        };
        return d.toLocaleDateString('en-US', formats[format] || formats.short);
    }

    static generateId(prefix = '') {
        return prefix + Date.now().toString(36) + Math.random().toString(36).substr(2);
    }

    static escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    static copyToClipboard(text) {
        return navigator.clipboard ? 
            navigator.clipboard.writeText(text) :
            this.fallbackCopyToClipboard(text);
    }

    static fallbackCopyToClipboard(text) {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        textArea.style.top = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        
        return new Promise((resolve, reject) => {
            try {
                document.execCommand('copy');
                resolve();
            } catch (err) {
                reject(err);
            } finally {
                document.body.removeChild(textArea);
            }
        });
    }

    static downloadFile(data, filename, type = 'application/octet-stream') {
        const blob = new Blob([data], { type });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
    }

    static observeElement(element, callback, options = {}) {
        const observer = new IntersectionObserver(callback, {
            threshold: 0.1,
            ...options
        });
        observer.observe(element);
        return observer;
    }

    static lazyLoad(selector, callback) {
        const elements = document.querySelectorAll(selector);
        elements.forEach(element => {
            this.observeElement(element, (entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        callback(entry.target);
                        entry.target.observer?.unobserve(entry.target);
                    }
                });
            });
        });
    }
}

class ComponentRegistry {
    constructor() {
        this.components = new Map();
        this.autoInit = true;
    }

    register(name, componentClass, autoInit = true) {
        this.components.set(name, { componentClass, autoInit });
        
        if (autoInit && document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initComponent(name));
        } else if (autoInit) {
            this.initComponent(name);
        }
    }

    initComponent(name) {
        const component = this.components.get(name);
        if (!component) return;

        const elements = document.querySelectorAll(`[data-component="${name}"]`);
        elements.forEach(element => {
            if (!element._componentInstance) {
                element._componentInstance = new component.componentClass(element);
            }
        });
    }

    get(name) {
        return this.components.get(name);
    }

    initAll() {
        this.components.forEach((component, name) => {
            if (component.autoInit) {
                this.initComponent(name);
            }
        });
    }
}

const api = new ApiClient();
const validator = new ValidationEngine();
const storage = new StorageManager();
const utils = UtilityFunctions;
const components = new ComponentRegistry();

window.EFormServices = {
    api,
    validator,
    storage,
    utils,
    components
};

document.addEventListener('DOMContentLoaded', () => {
    components.initAll();
    
    document.querySelectorAll('form[data-validate]').forEach(form => {
        form.addEventListener('submit', (e) => {
            if (!validator.validateForm(form)) {
                e.preventDefault();
            }
        });
        
        form.querySelectorAll('input, select, textarea').forEach(field => {
            field.addEventListener('blur', () => validator.validateField(field));
        });
    });

    document.addEventListener('click', (e) => {
        if (e.target.matches('[data-copy]')) {
            const text = e.target.dataset.copy || e.target.textContent;
            utils.copyToClipboard(text).then(() => {
                const originalText = e.target.textContent;
                e.target.textContent = 'Copied!';
                setTimeout(() => e.target.textContent = originalText, 2000);
            });
        }
    });
});