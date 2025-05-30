// EFormServices.Web/wwwroot/js/form-builder.js
// Got code 30/05/2025
class FormBuilder {
    constructor() {
        this.formData = {
            id: null,
            title: '',
            description: '',
            formType: 1,
            departmentId: null,
            fields: []
        };
        this.selectedField = null;
        this.fieldCounter = 0;
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadDepartments();
        if (this.getFormId()) {
            this.loadForm(this.getFormId());
        }
    }

    getFormId() {
        const pathParts = window.location.pathname.split('/');
        const id = pathParts[pathParts.length - 1];
        return !isNaN(id) ? parseInt(id) : null;
    }

    bindEvents() {
        document.querySelectorAll('.field-type').forEach(el => {
            el.addEventListener('click', () => this.addField(parseInt(el.dataset.type)));
        });

        document.getElementById('save-btn').addEventListener('click', () => this.saveForm());
        document.getElementById('publish-btn').addEventListener('click', () => this.publishForm());
        document.getElementById('preview-btn').addEventListener('click', () => this.previewForm());

        ['form-title', 'form-description', 'form-type', 'form-department'].forEach(id => {
            document.getElementById(id).addEventListener('change', (e) => {
                const prop = id.replace('form-', '');
                this.formData[prop === 'department' ? 'departmentId' : prop] = e.target.value || null;
            });
        });

        this.makeSortable();
    }

    async loadDepartments() {
        try {
            const response = await fetch('/api/departments');
            const departments = await response.json();
            const select = document.getElementById('form-department');
            departments.forEach(dept => {
                select.innerHTML += `<option value="${dept.id}">${dept.name}</option>`;
            });
        } catch (error) {
            console.error('Failed to load departments:', error);
        }
    }

    addField(fieldType) {
        this.fieldCounter++;
        const field = {
            id: `field_${this.fieldCounter}`,
            fieldType: fieldType,
            label: this.getDefaultLabel(fieldType),
            name: `field_${this.fieldCounter}`,
            description: '',
            isRequired: false,
            sortOrder: this.formData.fields.length + 1,
            settings: this.getDefaultSettings(fieldType),
            validationRules: {},
            options: fieldType === 6 || fieldType === 7 || fieldType === 8 ? [
                { label: 'Option 1', value: 'option1', isDefault: false },
                { label: 'Option 2', value: 'option2', isDefault: false }
            ] : []
        };

        this.formData.fields.push(field);
        this.renderField(field);
        this.hideEmptyMessage();
        this.selectField(field.id);
    }

    renderField(field) {
        const canvas = document.getElementById('form-canvas');
        const fieldHtml = this.generateFieldHtml(field);
        canvas.insertAdjacentHTML('beforeend', fieldHtml);
        
        const fieldEl = document.getElementById(field.id);
        fieldEl.addEventListener('click', () => this.selectField(field.id));
        fieldEl.querySelector('.delete-field').addEventListener('click', (e) => {
            e.stopPropagation();
            this.deleteField(field.id);
        });
    }

    generateFieldHtml(field) {
        const fieldTypeIcon = this.getFieldIcon(field.fieldType);
        const required = field.isRequired ? '<span class="text-danger">*</span>' : '';
        
        return `
            <div class="form-field" id="${field.id}" data-field-id="${field.id}">
                <div class="field-controls">
                    <button type="button" class="btn btn-sm btn-outline-danger delete-field">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
                <div class="d-flex align-items-center mb-2">
                    <i class="${fieldTypeIcon} me-2"></i>
                    <strong>${field.label}${required}</strong>
                </div>
                ${this.renderFieldPreview(field)}
                ${field.description ? `<small class="text-muted">${field.description}</small>` : ''}
            </div>
        `;
    }

    renderFieldPreview(field) {
        switch (field.fieldType) {
            case 1: return `<input type="text" class="form-control" placeholder="${field.settings.placeholder || 'Text input'}" disabled>`;
            case 2: return `<input type="email" class="form-control" placeholder="Email address" disabled>`;
            case 3: return `<input type="number" class="form-control" placeholder="Number" disabled>`;
            case 5: return `<textarea class="form-control" rows="3" placeholder="${field.settings.placeholder || 'Text area'}" disabled></textarea>`;
            case 6: return this.renderSelectOptions(field);
            case 7: return this.renderRadioOptions(field);
            case 8: return this.renderCheckboxOptions(field);
            case 9: return `<input type="date" class="form-control" disabled>`;
            case 12: return `<input type="file" class="form-control" disabled>`;
            default: return `<input type="text" class="form-control" disabled>`;
        }
    }

    renderSelectOptions(field) {
        return `
            <select class="form-select" disabled>
                <option>Choose an option...</option>
                ${field.options.map(opt => `<option>${opt.label}</option>`).join('')}
            </select>
        `;
    }

    renderRadioOptions(field) {
        return field.options.map((opt, i) => `
            <div class="form-check">
                <input class="form-check-input" type="radio" name="${field.id}" id="${field.id}_${i}" disabled>
                <label class="form-check-label" for="${field.id}_${i}">${opt.label}</label>
            </div>
        `).join('');
    }

    renderCheckboxOptions(field) {
        return field.options.map((opt, i) => `
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="${field.id}_${i}" disabled>
                <label class="form-check-label" for="${field.id}_${i}">${opt.label}</label>
            </div>
        `).join('');
    }

    selectField(fieldId) {
        document.querySelectorAll('.form-field').forEach(el => el.classList.remove('selected'));
        document.getElementById(fieldId).classList.add('selected');
        
        const field = this.formData.fields.find(f => f.id === fieldId);
        this.selectedField = field;
        this.renderFieldProperties(field);
    }

    renderFieldProperties(field) {
        const container = document.getElementById('field-properties');
        const supportsOptions = [6, 7, 8].includes(field.fieldType);
        
        container.innerHTML = `
            <div class="mb-3">
                <label class="form-label">Label</label>
                <input type="text" class="form-control" id="field-label" value="${field.label}">
            </div>
            <div class="mb-3">
                <label class="form-label">Field Name</label>
                <input type="text" class="form-control" id="field-name" value="${field.name}">
            </div>
            <div class="mb-3">
                <label class="form-label">Description</label>
                <textarea class="form-control" id="field-description" rows="2">${field.description}</textarea>
            </div>
            <div class="form-check mb-3">
                <input type="checkbox" class="form-check-input" id="field-required" ${field.isRequired ? 'checked' : ''}>
                <label class="form-check-label" for="field-required">Required</label>
            </div>
            ${field.fieldType === 1 || field.fieldType === 5 ? `
                <div class="mb-3">
                    <label class="form-label">Placeholder</label>
                    <input type="text" class="form-control" id="field-placeholder" value="${field.settings.placeholder || ''}">
                </div>
            ` : ''}
            ${supportsOptions ? `
                <div class="mb-3">
                    <label class="form-label">Options</label>
                    <div id="field-options">
                        ${field.options.map((opt, i) => `
                            <div class="input-group mb-2">
                                <input type="text" class="form-control option-label" value="${opt.label}" data-index="${i}">
                                <button type="button" class="btn btn-outline-danger remove-option" data-index="${i}">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        `).join('')}
                    </div>
                    <button type="button" class="btn btn-sm btn-outline-primary" id="add-option">Add Option</button>
                </div>
            ` : ''}
        `;

        this.bindFieldPropertyEvents();
    }

    bindFieldPropertyEvents() {
        const updateField = () => {
            if (!this.selectedField) return;
            
            this.selectedField.label = document.getElementById('field-label').value;
            this.selectedField.name = document.getElementById('field-name').value;
            this.selectedField.description = document.getElementById('field-description').value;
            this.selectedField.isRequired = document.getElementById('field-required').checked;
            
            const placeholderInput = document.getElementById('field-placeholder');
            if (placeholderInput) {
                this.selectedField.settings.placeholder = placeholderInput.value;
            }
            
            this.updateFieldInCanvas();
        };

        ['field-label', 'field-name', 'field-description', 'field-required'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.addEventListener('change', updateField);
        });

        const placeholderInput = document.getElementById('field-placeholder');
        if (placeholderInput) placeholderInput.addEventListener('change', updateField);

        document.querySelectorAll('.option-label').forEach(input => {
            input.addEventListener('change', (e) => {
                const index = parseInt(e.target.dataset.index);
                this.selectedField.options[index].label = e.target.value;
                this.selectedField.options[index].value = e.target.value.toLowerCase().replace(/\s+/g, '');
                this.updateFieldInCanvas();
            });
        });

        document.querySelectorAll('.remove-option').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const index = parseInt(e.target.closest('button').dataset.index);
                this.selectedField.options.splice(index, 1);
                this.renderFieldProperties(this.selectedField);
                this.updateFieldInCanvas();
            });
        });

        const addOptionBtn = document.getElementById('add-option');
        if (addOptionBtn) {
            addOptionBtn.addEventListener('click', () => {
                const optionNum = this.selectedField.options.length + 1;
                this.selectedField.options.push({
                    label: `Option ${optionNum}`,
                    value: `option${optionNum}`,
                    isDefault: false
                });
                this.renderFieldProperties(this.selectedField);
                this.updateFieldInCanvas();
            });
        }
    }

    updateFieldInCanvas() {
        if (!this.selectedField) return;
        
        const fieldEl = document.getElementById(this.selectedField.id);
        fieldEl.outerHTML = this.generateFieldHtml(this.selectedField);
        
        const newFieldEl = document.getElementById(this.selectedField.id);
        newFieldEl.addEventListener('click', () => this.selectField(this.selectedField.id));
        newFieldEl.querySelector('.delete-field').addEventListener('click', (e) => {
            e.stopPropagation();
            this.deleteField(this.selectedField.id);
        });
        newFieldEl.classList.add('selected');
    }

    deleteField(fieldId) {
        this.formData.fields = this.formData.fields.filter(f => f.id !== fieldId);
        document.getElementById(fieldId).remove();
        
        if (this.selectedField && this.selectedField.id === fieldId) {
            this.selectedField = null;
            document.getElementById('field-properties').innerHTML = '<p class="text-muted">Select a field to edit its properties</p>';
        }

        if (this.formData.fields.length === 0) {
            this.showEmptyMessage();
        }
    }

    makeSortable() {
        const canvas = document.getElementById('form-canvas');
        new Sortable(canvas, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            chosenClass: 'sortable-chosen',
            filter: '#empty-message',
            onEnd: (evt) => {
                const fieldId = evt.item.dataset.fieldId;
                const newIndex = evt.newIndex;
                
                const field = this.formData.fields.find(f => f.id === fieldId);
                this.formData.fields.splice(this.formData.fields.indexOf(field), 1);
                this.formData.fields.splice(newIndex, 0, field);
                
                this.formData.fields.forEach((field, index) => {
                    field.sortOrder = index + 1;
                });
            }
        });
    }

    async saveForm() {
        if (!this.validateForm()) return;
        
        try {
            const url = this.formData.id ? `/api/forms/${this.formData.id}` : '/api/forms';
            const method = this.formData.id ? 'PUT' : 'POST';
            
            const response = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.formData)
            });

            if (!response.ok) throw new Error('Failed to save form');

            const result = await response.json();
            this.formData.id = result.data.id;
            
            this.showToast('Form saved successfully', 'success');
            document.getElementById('publish-btn').style.display = 'inline-block';
            
        } catch (error) {
            console.error('Save error:', error);
            this.showToast('Failed to save form', 'error');
        }
    }

    async publishForm() {
        if (!this.formData.id) {
            await this.saveForm();
            if (!this.formData.id) return;
        }
        
        try {
            const response = await fetch(`/api/forms/${this.formData.id}/publish`, {
                method: 'POST'
            });

            if (!response.ok) throw new Error('Failed to publish form');
            
            this.showToast('Form published successfully', 'success');
            
        } catch (error) {
            console.error('Publish error:', error);
            this.showToast('Failed to publish form', 'error');
        }
    }

    previewForm() {
        const preview = document.getElementById('form-preview');
        preview.innerHTML = `
            <div class="card">
                <div class="card-header">
                    <h5>${this.formData.title || 'Untitled Form'}</h5>
                    ${this.formData.description ? `<p class="mb-0">${this.formData.description}</p>` : ''}
                </div>
                <div class="card-body">
                    ${this.formData.fields.map(field => this.generateFieldHtml(field)).join('')}
                </div>
            </div>
        `;
        
        new bootstrap.Modal(document.getElementById('preview-modal')).show();
    }

    validateForm() {
        if (!document.getElementById('form-title').value.trim()) {
            this.showToast('Form title is required', 'error');
            return false;
        }
        
        this.formData.title = document.getElementById('form-title').value;
        this.formData.description = document.getElementById('form-description').value;
        this.formData.formType = parseInt(document.getElementById('form-type').value);
        this.formData.departmentId = document.getElementById('form-department').value || null;
        
        return true;
    }

    async loadForm(formId) {
        try {
            const response = await fetch(`/api/forms/${formId}`);
            if (!response.ok) throw new Error('Form not found');
            
            const form = await response.json();
            this.formData = form.data;
            
            document.getElementById('form-title').value = this.formData.title;
            document.getElementById('form-description').value = this.formData.description || '';
            document.getElementById('form-type').value = this.formData.formType;
            document.getElementById('form-department').value = this.formData.departmentId || '';
            
            if (this.formData.fields && this.formData.fields.length > 0) {
                this.hideEmptyMessage();
                this.formData.fields.forEach(field => this.renderField(field));
            }
            
            if (this.formData.isPublished) {
                document.getElementById('publish-btn').style.display = 'inline-block';
            }
            
        } catch (error) {
            console.error('Load form error:', error);
            this.showToast('Failed to load form', 'error');
        }
    }

    getDefaultLabel(fieldType) {
        const labels = {
            1: 'Text Field', 2: 'Email Field', 3: 'Number Field', 5: 'Text Area',
            6: 'Dropdown', 7: 'Radio Button', 8: 'Checkbox', 9: 'Date Field', 12: 'File Upload'
        };
        return labels[fieldType] || 'Field';
    }

    getDefaultSettings(fieldType) {
        return { placeholder: '', defaultValue: '', isReadOnly: false, isVisible: true };
    }

    getFieldIcon(fieldType) {
        const icons = {
            1: 'fas fa-font', 2: 'fas fa-envelope', 3: 'fas fa-hashtag', 5: 'fas fa-align-left',
            6: 'fas fa-caret-down', 7: 'fas fa-dot-circle', 8: 'fas fa-check-square',
            9: 'fas fa-calendar', 12: 'fas fa-file-upload'
        };
        return icons[fieldType] || 'fas fa-question';
    }

    hideEmptyMessage() {
        document.getElementById('empty-message').style.display = 'none';
    }

    showEmptyMessage() {
        document.getElementById('empty-message').style.display = 'block';
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

if (typeof Sortable !== 'undefined') {
    document.addEventListener('DOMContentLoaded', () => new FormBuilder());
} else {
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js';
    script.onload = () => document.addEventListener('DOMContentLoaded', () => new FormBuilder());
    document.head.appendChild(script);
}