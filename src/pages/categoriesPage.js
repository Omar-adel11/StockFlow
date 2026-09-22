// src/pages/CategoriesPage.js
import * as categoryService from '../services/categoryService.js';
import * as categoryValidation from '../validation/categoryValidation.js';
import * as categoryState from '../state/categoryState.js';
import * as authService from '../services/authService.js';
import { escapeHtml } from '../utils/helpers.js';

let editingCategoryId = null;

// DOM Elements
const form = document.getElementById('category-form');
const formTitle = document.getElementById('form-title');
const nameInput = document.getElementById('category-name');
const descriptionInput = document.getElementById('category-description');
const submitBtn = document.getElementById('submit-btn');
const cancelBtn = document.getElementById('cancel-btn');
const formStatus = document.getElementById('form-status');

const categoriesList = document.getElementById('categories-list');
const categoriesEmpty = document.getElementById('categories-empty');
const categoriesLoading = document.getElementById('categories-loading');
const logoutBtn = document.getElementById('logout-btn');

document.addEventListener('DOMContentLoaded', init);

async function init() {
    if (logoutBtn) {
        logoutBtn.addEventListener('click', authService.logout);
    }

    form.addEventListener('submit', handleFormSubmit);
    cancelBtn.addEventListener('click', resetForm);

    await loadCategories();
}

async function loadCategories() {
    categoriesLoading.hidden = false;
    categoriesEmpty.hidden = true;
    categoriesList.innerHTML = '';

    try {
        const data = await categoryService.getAllCategories();
        categoryState.setCategories(data || []);
        renderCategories();
    } catch (error) {
        console.error('Failed to fetch categories:', error);
        formStatus.textContent = 'Failed to load categories. Please try again.';
    } finally {
        categoriesLoading.hidden = true;
    }
}

function renderCategories() {
    const categories = categoryState.getCategories();
    categoriesList.innerHTML = '';

    if (!categories || categories.length === 0) {
        categoriesEmpty.hidden = false;
        return;
    }

    categoriesEmpty.hidden = true;

    categories.forEach(category => {
        const card = document.createElement('div');
        card.className = 'card entity-card';
        card.innerHTML = `
            <div class="entity-card-header">
                <h3>${escapeHtml(category.name)}</h3>
            </div>
            <p class="entity-card-body">${escapeHtml(category.description || 'No description provided.')}</p>
            <div class="entity-card-actions">
                <button class="btn btn-secondary btn-sm edit-btn" data-id="${category.id}">Edit</button>
                <button class="btn btn-danger btn-sm delete-btn" data-id="${category.id}">Delete</button>
            </div>
        `;

        card.querySelector('.edit-btn').addEventListener('click', () => startEdit(category.id));
        card.querySelector('.delete-btn').addEventListener('click', () => handleDelete(category.id));

        categoriesList.appendChild(card);
    });
}

async function handleFormSubmit(event) {
    event.preventDefault();
    formStatus.textContent = '';

    const formData = new FormData(form);
    const errors = categoryValidation.ValidateCategoryForm(formData);

    if (Object.keys(errors).length > 0) {
        formStatus.textContent = Object.values(errors).join(' ');
        return;
    }

    const payload = {
        name: formData.get('name').trim(),
        description: formData.get('description') ? formData.get('description').trim() : ''
    };

    submitBtn.disabled = true;
    formStatus.textContent = editingCategoryId ? 'Updating category...' : 'Saving category...';

    try {
        if (editingCategoryId) {
            const updatedCategory = await categoryService.updateCategory(editingCategoryId, payload);
            categoryState.updateCategoryInState(updatedCategory || { id: editingCategoryId, ...payload });
            formStatus.textContent = 'Category updated successfully!';
        } else {
            const newCategory = await categoryService.createCategory(payload);
            categoryState.addCategoryToState(newCategory);
            formStatus.textContent = 'Category created successfully!';
        }

        renderCategories();
        resetForm();
    } catch (error) {
        console.error(error);
        formStatus.textContent = error.message || 'Operation failed.';
    } finally {
        submitBtn.disabled = false;
    }
}

function startEdit(id) {
    const category = categoryState.getCategoryById(id);
    if (!category) return;

    editingCategoryId = category.id;
    formTitle.textContent = 'Edit Category';
    nameInput.value = category.name || '';
    descriptionInput.value = category.description || '';

    submitBtn.textContent = 'Update Category';
    cancelBtn.hidden = false;
    formStatus.textContent = '';
}

function resetForm() {
    editingCategoryId = null;
    formTitle.textContent = 'Add New Category';
    form.reset();
    submitBtn.textContent = 'Save Category';
    cancelBtn.hidden = true;
    formStatus.textContent = '';
}

async function handleDelete(id) {
    if (!confirm('Are you sure you want to delete this category?')) return;

    try {
        await categoryService.deleteCategory(id);
        categoryState.removeCategoryFromState(id);
        renderCategories();

        if (editingCategoryId === id) {
            resetForm();
        }
    } catch (error) {
        console.error('Delete failed:', error);
        alert(error.message || 'Failed to delete category.');
    }
}