// EFormServices.Web/wwwroot/js/form-submission.js
// Got code 30/05/2025

class FormSubmissionEngine {
    constructor(formKey) {
        this.formKey = formKey;
        this.formData = null;
        this.currentStep = 0;
        this.submissionData = {};
        this.fileUploads = new Map();
        this.validationErrors = new Map();
        this.conditionalLogic = new Map();
        this.progressSaved = false;
        this.init();
    }

    async init() {
        await this.loadForm();
        this.renderForm();
        this.bindEvents();
        this.initConditionalLogic();
        this.loadSavedProgress();
    }

    async loadForm() {
        try {
            const response = await fetch(`/api/forms/public/${this.formKey}`);
            if (!response.ok) throw new Error('Form not found');
            
            this.formData = await response.json();
            document.title = `${this.formData.title} - Form Submission`;
        } catch (error) {
            this.showError('Form not available or has expired');
            throw error;
        }
    }

    renderForm() {
        const container = document.getElementById('form-container');
        if (!container || !this.formData) return;

        const steps = this.groupFieldsByStep();
        
        container.innerHTML = `
            <div class="card shadow-lg border-0">
                <div class="card-header bg-primary text-white">
                    <h3 class="mb-0">${this.formData.title}</h3>
                    ${this.formData.description ? `<p class="mb-0 mt-2">${this.formData.description}</p>` : ''}
                </div>
                
                ${this.formData.settings.showProgressBar && steps.length > 1 ? this.renderProgressBar(steps.length) : ''}
                
                <div class="card-body">
                    <form id="submission-form" novalidate>
                        ${steps.map((step, index) => this.renderStep(step, index)).join('')}
                        
                        <div class="form-navigation mt-4">
                            <div class="d-flex justify-content-between">
                                <button type="button" id="prev-btn" class="btn btn-outline-secondary" style="display: none;">
                                    <i class="fas fa-arrow-left me-2"></i>Previous
                                </button>
                                <div class="ms-auto">
                                    ${this.formData.settings.allowSaveAndContinue ? 
                                        '<button type="button" id="save-progress-btn" class="btn btn-outline-primary me-2">Save Progress</button>' : ''}
                                    <button type="button" id="next-btn" class="btn btn-primary">
                                        Next<i class="fas fa-arrow-right ms-2"></i>
                                    </button>
                                    <button type="submit" id="submit-btn" class="btn btn-success" style="display: none;">
                                        <i class="fas fa-paper-plane me-2"></i>Submit Form
                                    </button>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
                
                <div class="card-footer text-muted text-center">
                    <small>
                        <i class="fas fa-shield-alt me-1"></i>
                        Your information is secure and encrypted
                        ${this.formData.metadata.estimatedCompletionMinutes ? 
                            ` • Estimated time: ${this.formData.metadata.estimatedCompletionMinutes} minutes` : ''}
                    </small>
                </div>
            </div>
        `;

        this.updateStepVisibility();
        this.updateNavigation();
    }

    groupFieldsByStep() {
        if (!this.formData.fields) return [[]];
        
        const steps = [[]];
        let currentStep = 0;

        this.formData.fields.forEach(field => {
            if (field.fieldType === 20) {
                steps.push([]);
                currentStep++;
            } else {
                steps[currentStep].push(field);
            }
        });

        return steps.filter(step => step.length > 0);
    }

    renderProgressBar(totalSteps) {
        return `
            <div class="progress-container p-3 bg-light">
                <div class="step-indicator">
                    ${Array.from({length: totalSteps}, (_, i) => `
                        <div class="step ${i === 0 ? 'active' : ''}" data-step="${i}">
                            <div class="step-circle">${i + 1}</div>
                            <div class="step-label">Step ${i + 1}</div>
                        </div>
                    `).join('')}
                </div>
                <div class="progress mt-3">
                    <div class="progress-bar" role="progressbar" style="width: ${100/totalSteps}%"></div>
                </div>
            </div>
        `;
    }

    renderStep(fields, stepIndex) {
        return `
            <div class="form-step" data-step="${stepIndex}" ${stepIndex === 0 ? 'style="display: block;"' : ''}>
                ${fields.map(field => this.renderField(field)).join('')}
            </div>
        `;
    }

    renderField(field) {
        const isRequired = field.isRequired;
        const fieldId = `field_${field.id}`;
        const value = this.submissionData[field.name] || field.settings.defaultValue || '';

        let fieldHtml = '';
        
        switch (field.fieldType) {
            case 1: // Text
                fieldHtml = `<input type="text" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" placeholder="${field.settings.placeholder || ''}" 
                    ${isRequired ? 'required' : ''} ${field.settings.isReadOnly ? 'readonly' : ''}>`;
                break;
                
            case 2: // Email
                fieldHtml = `<input type="email" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" placeholder="${field.settings.placeholder || 'Enter email address'}" 
                    ${isRequired ? 'required' : ''}>`;
                break;
                
            case 3: // Number
                fieldHtml = `<input type="number" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" placeholder="${field.settings.placeholder || ''}" 
                    ${field.validationRules.minValue ? `min="${field.validationRules.minValue}"` : ''}
                    ${field.validationRules.maxValue ? `max="${field.validationRules.maxValue}"` : ''}
                    ${field.settings.step ? `step="${field.settings.step}"` : ''}
                    ${isRequired ? 'required' : ''}>`;
                break;
                
            case 4: // Phone
                fieldHtml = `<input type="tel" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" placeholder="${field.settings.placeholder || 'Enter phone number'}" 
                    ${isRequired ? 'required' : ''}>`;
                break;
                
            case 5: // TextArea
                fieldHtml = `<textarea class="form-control" id="${fieldId}" name="${field.name}" 
                    rows="${field.settings.rows || 4}" placeholder="${field.settings.placeholder || ''}" 
                    ${isRequired ? 'required' : ''}>${value}</textarea>`;
                break;
                
            case 6: // Dropdown
                fieldHtml = `<select class="form-select" id="${fieldId}" name="${field.name}" ${isRequired ? 'required' : ''}>
                    <option value="">Choose...</option>
                    ${field.options.map(option => 
                        `<option value="${option.value}" ${value === option.value ? 'selected' : ''}>${option.label}</option>`
                    ).join('')}
                </select>`;
                break;
                
            case 7: // Radio
                fieldHtml = field.options.map(option => `
                    <div class="form-check">
                        <input type="radio" class="form-check-input" id="${fieldId}_${option.value}" 
                            name="${field.name}" value="${option.value}" ${value === option.value ? 'checked' : ''} 
                            ${isRequired ? 'required' : ''}>
                        <label class="form-check-label" for="${fieldId}_${option.value}">${option.label}</label>
                    </div>
                `).join('');
                break;
                
            case 8: // Checkbox
                if (field.options.length === 1) {
                    fieldHtml = `
                        <div class="form-check">
                            <input type="checkbox" class="form-check-input" id="${fieldId}" name="${field.name}" 
                                value="${field.options[0].value}" ${value === field.options[0].value ? 'checked' : ''} 
                                ${isRequired ? 'required' : ''}>
                            <label class="form-check-label" for="${fieldId}">${field.options[0].label}</label>
                        </div>
                    `;
                } else {
                    fieldHtml = field.options.map(option => `
                        <div class="form-check">
                            <input type="checkbox" class="form-check-input" id="${fieldId}_${option.value}" 
                                name="${field.name}[]" value="${option.value}" 
                                ${Array.isArray(value) && value.includes(option.value) ? 'checked' : ''}>
                            <label class="form-check-label" for="${fieldId}_${option.value}">${option.label}</label>
                        </div>
                    `).join('');
                }
                break;
                
            case 9: // Date
                fieldHtml = `<input type="date" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" ${isRequired ? 'required' : ''}>`;
                break;
                
            case 10: // Time
                fieldHtml = `<input type="time" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" ${isRequired ? 'required' : ''}>`;
                break;
                
            case 11: // DateTime
                fieldHtml = `<input type="datetime-local" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" ${isRequired ? 'required' : ''}>`;
                break;
                
            case 12: // File Upload
                fieldHtml = `
                    <div class="file-upload-container">
                        <div class="file-drop-zone" data-field="${field.name}">
                            <input type="file" class="form-control d-none" id="${fieldId}" name="${field.name}" 
                                ${field.settings.allowMultiple ? 'multiple' : ''} 
                                ${field.validationRules.allowedFileTypes ? 
                                    `accept="${field.validationRules.allowedFileTypes.join(',')}"` : ''}
                                ${isRequired ? 'required' : ''}>
                            <div class="drop-zone-content text-center py-4">
                                <i class="fas fa-cloud-upload-alt fa-3x text-muted mb-3"></i>
                                <div class="h5">Drop files here or click to browse</div>
                                <div class="text-muted">
                                    ${field.validationRules.allowedFileTypes ? 
                                        `Allowed: ${field.validationRules.allowedFileTypes.join(', ')}` : 'All files allowed'}
                                    ${field.validationRules.maxFileSize ? 
                                        ` • Max size: ${field.validationRules.maxFileSize}MB` : ''}
                                </div>
                            </div>
                        </div>
                        <div class="file-list mt-2" id="${fieldId}_list"></div>
                    </div>
                `;
                break;
                
            default:
                fieldHtml = `<input type="text" class="form-control" id="${fieldId}" name="${field.name}" 
                    value="${value}" ${isRequired ? 'required' : ''}>`;
        }

        return `
            <div class="mb-4 field-container conditional-field" data-field-name="${field.name}" 
                 ${!field.settings.isVisible ? 'style="display: none;"' : ''}>
                <label class="form-label fw-medium" for="${fieldId}">
                    ${field.label}
                    ${isRequired ? '<span class="text-danger">*</span>' : ''}
                </label>
                ${field.description ? `<div class="form-text mb-2">${field.description}</div>` : ''}
                ${fieldHtml}
                <div class="invalid-feedback" id="${fieldId}_error"></div>
            </div>
        `;
    }

    bindEvents() {
        const form = document.getElementById('submission-form');
        const nextBtn = document.getElementById('next-btn');
        const prevBtn = document.getElementById('prev-btn');
        const submitBtn = document.getElementById('submit-btn');
        const saveProgressBtn = document.getElementById('save-progress-btn');

        if (form) {
            form.addEventListener('submit', this.handleSubmit.bind(this));
            form.addEventListener('input', debounce(this.handleFieldChange.bind(this), 300));
            form.addEventListener('change', this.handleFieldChange.bind(this));
        }

        if (nextBtn) nextBtn.addEventListener('click', this.nextStep.bind(this));
        if (prevBtn) prevBtn.addEventListener('click', this.prevStep.bind(this));
        if (saveProgressBtn) saveProgressBtn.addEventListener('click', this.saveProgress.bind(this));

        this.initFileUploads();
        this.bindKeyboardNavigation();
    }

    initFileUploads() {
        document.querySelectorAll('.file-drop-zone').forEach(zone => {
            const fieldName = zone.dataset.field;
            const input = zone.querySelector('input[type="file"]');
            
            zone.addEventListener('click', () => input.click());
            zone.addEventListener('dragover', (e) => {
                e.preventDefault();
                zone.classList.add('dragover');
            });
            zone.addEventListener('dragleave', () => zone.classList.remove('dragover'));
            zone.addEventListener('drop', (e) => {
                e.preventDefault();
                zone.classList.remove('dragover');
                this.handleFileUpload(e.dataTransfer.files, fieldName);
            });
            
            input.addEventListener('change', (e) => {
                this.handleFileUpload(e.target.files, fieldName);
            });
        });
    }

    bindKeyboardNavigation() {
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey || e.metaKey) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.nextStep();
                } else if (e.key === 's') {
                    e.preventDefault();
                    this.saveProgress();
                }
            }
        });
    }

    initConditionalLogic() {
        if (!this.formData.conditionalLogic) return;

        this.formData.conditionalLogic.forEach(logic => {
            this.conditionalLogic.set(logic.triggerFieldId, logic);
        });
    }

    handleFieldChange(event) {
        const field = event.target;
        const fieldName = field.name.replace('[]', '');
        
        this.updateSubmissionData(field);
        this.validateField(field);
        this.processConditionalLogic(fieldName);
    }

    updateSubmissionData(field) {
        const fieldName = field.name.replace('[]', '');
        
        if (field.type === 'checkbox') {
            if (field.name.endsWith('[]')) {
                const checkboxes = document.querySelectorAll(`input[name="${field.name}"]:checked`);
                this.submissionData[fieldName] = Array.from(checkboxes).map(cb => cb.value);
            } else {
                this.submissionData[fieldName] = field.checked ? field.value : '';
            }
        } else if (field.type === 'radio') {
            if (field.checked) {
                this.submissionData[fieldName] = field.value;
            }
        } else {
            this.submissionData[fieldName] = field.value;
        }
    }

    validateField(field) {
        const fieldName = field.name.replace('[]', '');
        const fieldConfig = this.formData.fields.find(f => f.name === fieldName);
        if (!fieldConfig) return;

        const errors = [];
        const value = this.submissionData[fieldName];

        if (fieldConfig.isRequired && (!value || (Array.isArray(value) && value.length === 0))) {
            errors.push('This field is required');
        }

        if (value && fieldConfig.validationRules) {
            const rules = fieldConfig.validationRules;
            
            if (rules.minLength && value.length < rules.minLength) {
                errors.push(`Minimum length is ${rules.minLength} characters`);
            }
            if (rules.maxLength && value.length > rules.maxLength) {
                errors.push(`Maximum length is ${rules.maxLength} characters`);
            }
            if (rules.pattern && !new RegExp(rules.pattern).test(value)) {
                errors.push(rules.customMessage || 'Invalid format');
            }
            if (rules.minValue && parseFloat(value) < rules.minValue) {
                errors.push(`Minimum value is ${rules.minValue}`);
            }
            if (rules.maxValue && parseFloat(value) > rules.maxValue) {
                errors.push(`Maximum value is ${rules.maxValue}`);
            }
        }

        this.displayFieldErrors(field, errors);
        
        if (errors.length > 0) {
            this.validationErrors.set(fieldName, errors);
        } else {
            this.validationErrors.delete(fieldName);
        }
    }

    displayFieldErrors(field, errors) {
        const errorElement = document.getElementById(`${field.id}_error`);
        if (!errorElement) return;

        if (errors.length > 0) {
            field.classList.add('is-invalid');
            errorElement.textContent = errors[0];
        } else {
            field.classList.remove('is-invalid');
            errorElement.textContent = '';
        }
    }

    processConditionalLogic(triggerFieldName) {
        const triggerField = this.formData.fields.find(f => f.name === triggerFieldName);
        if (!triggerField) return;

        const logic = this.conditionalLogic.get(triggerField.id);
        if (!logic) return;

        const triggerValue = this.submissionData[triggerFieldName];
        const shouldTrigger = this.evaluateCondition(logic.condition, triggerValue, logic.triggerValue);
        
        const targetField = document.querySelector(`[data-field-name="${logic.targetFieldName}"]`);
        if (!targetField) return;

        this.applyConditionalAction(targetField, logic.action, shouldTrigger);
    }

    evaluateCondition(operator, actualValue, expectedValue) {
        switch (operator) {
            case 'equals': return actualValue === expectedValue;
            case 'not_equals': return actualValue !== expectedValue;
            case 'contains': return actualValue && actualValue.includes(expectedValue);
            case 'greater_than': return parseFloat(actualValue) > parseFloat(expectedValue);
            case 'less_than': return parseFloat(actualValue) < parseFloat(expectedValue);
            case 'is_empty': return !actualValue || actualValue.length === 0;
            case 'is_not_empty': return actualValue && actualValue.length > 0;
            default: return false;
        }
    }

    applyConditionalAction(targetElement, action, shouldApply) {
        switch (action) {
            case 'show':
                targetElement.style.display = shouldApply ? 'block' : 'none';
                break;
            case 'hide':
                targetElement.style.display = shouldApply ? 'none' : 'block';
                break;
            case 'require':
                const input = targetElement.querySelector('input, select, textarea');
                if (input) input.required = shouldApply;
                break;
            case 'disable':
                const field = targetElement.querySelector('input, select, textarea');
                if (field) field.disabled = shouldApply;
                break;
        }
    }

    async handleFileUpload(files, fieldName) {
        const field = this.formData.fields.find(f => f.name === fieldName);
        if (!field) return;

        const fileList = document.getElementById(`field_${field.id}_list`);
        const maxSize = field.validationRules.maxFileSize * 1024 * 1024;
        const allowedTypes = field.validationRules.allowedFileTypes;

        for (const file of files) {
            if (maxSize && file.size > maxSize) {
                NotificationManager.error(`File ${file.name} is too large`);
                continue;
            }

            if (allowedTypes && !allowedTypes.some(type => file.name.toLowerCase().endsWith(type))) {
                NotificationManager.error(`File ${file.name} type not allowed`);
                continue;
            }

            const fileItem = this.createFileItem(file, fieldName);
            fileList.appendChild(fileItem);
            
            if (!this.fileUploads.has(fieldName)) {
                this.fileUploads.set(fieldName, []);
            }
            this.fileUploads.get(fieldName).push(file);
        }
    }

    createFileItem(file, fieldName) {
        const div = document.createElement('div');
        div.className = 'file-item';
        div.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fas fa-file me-2"></i>
                <div class="flex-grow-1">
                    <div class="fw-medium">${file.name}</div>
                    <small class="text-muted">${this.formatFileSize(file.size)}</small>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="this.parentElement.parentElement.remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        return div;
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    nextStep() {
        if (!this.validateCurrentStep()) return;

        const steps = document.querySelectorAll('.form-step');
        if (this.currentStep < steps.length - 1) {
            this.currentStep++;
            this.updateStepVisibility();
            this.updateNavigation();
            this.updateProgress();
        }
    }

    prevStep() {
        if (this.currentStep > 0) {
            this.currentStep--;
            this.updateStepVisibility();
            this.updateNavigation();
            this.updateProgress();
        }
    }

    validateCurrentStep() {
        const currentStepElement = document.querySelector(`.form-step[data-step="${this.currentStep}"]`);
        if (!currentStepElement) return true;

        const fields = currentStepElement.querySelectorAll('input, select, textarea');
        let isValid = true;

        fields.forEach(field => {
            this.validateField(field);
            if (field.classList.contains('is-invalid')) {
                isValid = false;
            }
        });

        return isValid;
    }

    updateStepVisibility() {
        document.querySelectorAll('.form-step').forEach((step, index) => {
            step.style.display = index === this.currentStep ? 'block' : 'none';
        });
    }

    updateNavigation() {
        const steps = document.querySelectorAll('.form-step');
        const prevBtn = document.getElementById('prev-btn');
        const nextBtn = document.getElementById('next-btn');
        const submitBtn = document.getElementById('submit-btn');

        if (prevBtn) prevBtn.style.display = this.currentStep > 0 ? 'block' : 'none';
        
        const isLastStep = this.currentStep === steps.length - 1;
        if (nextBtn) nextBtn.style.display = isLastStep ? 'none' : 'block';
        if (submitBtn) submitBtn.style.display = isLastStep ? 'block' : 'none';
    }

    updateProgress() {
        const steps = document.querySelectorAll('.form-step');
        const progressBar = document.querySelector('.progress-bar');
        const stepIndicators = document.querySelectorAll('.step');

        if (progressBar) {
            const progress = ((this.currentStep + 1) / steps.length) * 100;
            progressBar.style.width = `${progress}%`;
        }

        stepIndicators.forEach((indicator, index) => {
            indicator.classList.toggle('completed', index < this.currentStep);
            indicator.classList.toggle('active', index === this.currentStep);
        });
    }

    async handleSubmit(event) {
        event.preventDefault();

        if (!this.validateCurrentStep()) {
            NotificationManager.error('Please fix validation errors before submitting');
            return;
        }

        if (this.validationErrors.size > 0) {
            NotificationManager.error('Please fix all validation errors');
            return;
        }

        try {
            LoadingManager.show(document.getElementById('form-container'));

            const formData = new FormData();
            formData.append('formKey', this.formKey);
            formData.append('submissionData', JSON.stringify(this.submissionData));

            this.fileUploads.forEach((files, fieldName) => {
                files.forEach(file => {
                    formData.append(`files_${fieldName}`, file);
                });
            });

            const response = await fetch('/api/forms/submit', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) throw new Error('Submission failed');

            const result = await response.json();
            this.showSuccessModal(result);

        } catch (error) {
            NotificationManager.error('Failed to submit form. Please try again.');
        } finally {
            LoadingManager.hide(document.getElementById('form-container'));
        }
    }

    showSuccessModal(result) {
        document.getElementById('tracking-number').textContent = result.trackingNumber;
        
        const successMessage = document.getElementById('success-message');
        successMessage.textContent = this.formData.settings.successMessage || 
            'Thank you for your submission. We will review it and get back to you soon.';

        if (this.formData.settings.redirectUrl) {
            document.getElementById('redirect-info').style.display = 'block';
            let countdown = 5;
            const countdownElement = document.getElementById('countdown');
            
            const timer = setInterval(() => {
                countdown--;
                countdownElement.textContent = countdown;
                if (countdown <= 0) {
                    clearInterval(timer);
                    window.location.href = this.formData.settings.redirectUrl;
                }
            }, 1000);
        }

        const submitAnotherBtn = document.getElementById('submit-another');
        if (this.formData.settings.allowMultipleSubmissions) {
            submitAnotherBtn.style.display = 'block';
            submitAnotherBtn.addEventListener('click', () => {
                window.location.reload();
            });
        } else {
            submitAnotherBtn.style.display = 'none';
        }

        ModalManager.show('success-modal');
        this.clearSavedProgress();
    }

    saveProgress() {
        try {
            const progressData = {
                formKey: this.formKey,
                currentStep: this.currentStep,
                submissionData: this.submissionData,
                timestamp: new Date().toISOString()
            };

            StorageManager.set(`form_progress_${this.formKey}`, progressData, 7 * 24 * 60 * 60 * 1000);
            NotificationManager.success('Progress saved successfully');
            this.progressSaved = true;
        } catch (error) {
            NotificationManager.error('Failed to save progress');
        }
    }

    loadSavedProgress() {
        const savedProgress = StorageManager.get(`form_progress_${this.formKey}`);
        if (!savedProgress) return;

        if (confirm('Found saved progress. Would you like to continue where you left off?')) {
            this.currentStep = savedProgress.currentStep || 0;
            this.submissionData = savedProgress.submissionData || {};
            
            this.populateFormFields();
            this.updateStepVisibility();
            this.updateNavigation();
            this.updateProgress();
            
            NotificationManager.info('Progress restored successfully');
        }
    }

    populateFormFields() {
        Object.entries(this.submissionData).forEach(([fieldName, value]) => {
            const field = document.querySelector(`[name="${fieldName}"]`);
            if (!field) return;

            if (field.type === 'checkbox' || field.type === 'radio') {
                if (Array.isArray(value)) {
                    value.forEach(val => {
                        const checkbox = document.querySelector(`[name="${fieldName}[]"][value="${val}"]`);
                        if (checkbox) checkbox.checked = true;
                    });
                } else {
                    const input = document.querySelector(`[name="${fieldName}"][value="${value}"]`);
                    if (input) input.checked = true;
                }
            } else {
                field.value = value;
            }
        });
    }

    clearSavedProgress() {
        StorageManager.remove(`form_progress_${this.formKey}`);
    }

    showError(message) {
        const container = document.getElementById('form-container');
        container.innerHTML = `
            <div class="alert alert-danger text-center">
                <i class="fas fa-exclamation-triangle fa-3x mb-3"></i>
                <h4>Form Not Available</h4>
                <p>${message}</p>
                <a href="/" class="btn btn-primary">Return Home</a>
            </div>
        `;
    }
}

let formSubmissionEngine;

document.addEventListener('DOMContentLoaded', () => {
    const formKey = window.formKey;
    if (formKey) {
        formSubmissionEngine = new FormSubmissionEngine(formKey);
    }
});

window.addEventListener('beforeunload', (e) => {
    if (formSubmissionEngine && !formSubmissionEngine.progressSaved && 
        Object.keys(formSubmissionEngine.submissionData).length > 0) {
        e.preventDefault();
        e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
        return e.returnValue;
    }
});