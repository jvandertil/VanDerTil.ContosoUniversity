// ajax-form.ts
// Vanilla-JS/TS rewrite of the ContosoUniversity jQuery ajax-form pattern.
// https://github.com/jbogard/ContosoUniversityDotNetCore-Pages/blob/main/ContosoUniversity/wwwroot/js/site.js
//
// Contract with the server (matches the ValidateModelAttribute discussed earlier):
//   Success -> 200 OK,          body: { redirectUrl: string }
//   Invalid -> 422 Unprocessable Entity, body: { errors: Record<string, string[]> }
//     where each key is the ModelState key (e.g. "Input.Email", matching asp-for's `name`)

interface SuccessResponse {
    redirectUrl: string;
}

interface ValidationErrorResponse {
    errors: Record<string, string[]>;
}

const NO_AJAX_SELECTOR = "[data-no-ajax]";
const SUMMARY_SELECTOR = "[data-validation-summary]";

function fieldSelectorFor(name: string): string {
    // CSS.escape handles the dots/brackets/colons that appear in
    // model-bound names like "Input.Email" or "Items[0].Name" —
    // no manual regex needed, unlike the jQuery original.
    return `[name="${CSS.escape(name)}"]`;
}

function fieldErrorSelectorFor(fieldName: string): string {
    // Finds the validation message span for a specific field using data-valmsg-for
    return `[data-valmsg-for="${CSS.escape(fieldName)}"]`;
}

function clearFieldErrors(form: HTMLFormElement): void {
    // Clear is-invalid class from fields
    form.querySelectorAll(".is-invalid").forEach(el => el.classList.remove("is-invalid"));

    // Clear error messages from field spans
    form.querySelectorAll<HTMLElement>("[data-valmsg-for]").forEach(el => {
        el.textContent = "";
        el.hidden = true;
    });
}

function highlightFieldErrors(form: HTMLFormElement, errors: Record<string, string[]>): void {
    for (const [fieldName, messages] of Object.entries(errors)) {
        const fieldElement = form.querySelector<HTMLElement>(fieldSelectorFor(fieldName));
        const errorElement = form.querySelector<HTMLElement>(fieldErrorSelectorFor(fieldName));

        if (fieldElement) {
            fieldElement.classList.add("is-invalid");
        }

        if (errorElement && messages.length > 0) {
            // Display the first error message in the field's error span
            errorElement.textContent = messages[0];
            errorElement.hidden = false;
        }
    }
}

function renderSummary(form: HTMLFormElement, errors: Record<string, string[]>): void {
    const summary = form.querySelector<HTMLElement>(SUMMARY_SELECTOR);
    if (!summary) return;

    // Only include errors that don't map to a field (empty string key or unmapped fields)
    const unmappedErrors = Object.entries(errors)
        .filter(([fieldName]) => !fieldName || !form.querySelector(fieldErrorSelectorFor(fieldName)))
        .flatMap(([, messages]) => messages);

    summary.innerHTML = "";
    summary.hidden = unmappedErrors.length === 0;

    if (unmappedErrors.length > 0) {
        const list = document.createElement("ul");
        for (const message of unmappedErrors) {
            const item = document.createElement("li");
            item.textContent = message;
            list.appendChild(item);
        }
        summary.appendChild(list);
    }
}

async function handleSubmit(form: HTMLFormElement, submitter: HTMLElement | null): Promise<void> {
    const submitButtons = form.querySelectorAll<HTMLButtonElement | HTMLInputElement>('[type="submit"]');
    submitButtons.forEach(btn => (btn.disabled = true));
    clearFieldErrors(form);

    try {
        // Passing `submitter` (2nd arg) ensures a clicked submit button's
        // name/value pair is included — plain `new FormData(form)` drops it,
        // which matters if a form has multiple submit buttons with distinct
        // name/value (e.g. "action=save" vs "action=saveAndContinue").
        const body = new FormData(form, submitter ?? undefined);

        const response = await fetch(form.action, {
            method: "POST",
            body, // browser sets multipart Content-Type + boundary automatically;
            // this also means file inputs "just work" with no special-casing.
            headers: { "X-Requested-With": "XMLHttpRequest" },
        });

        if (response.ok) {
            const data = (await response.json()) as SuccessResponse;
            window.location.href = data.redirectUrl;
            return; // navigating away — no need to re-enable buttons
        }

        if (response.status === 422) {
            const data = (await response.json()) as ValidationErrorResponse;
            highlightFieldErrors(form, data.errors);
            renderSummary(form, data.errors);
            window.scrollTo(0, 0);
        } else {
            // Not a validation failure — auth expired, server error, etc.
            // Don't try to render it as field errors; let it surface loudly.
            console.error(`Unexpected response (${response.status}) submitting form`, form);
            alert("Something went wrong submitting the form. Please try again.");
        }
    } catch (err) {
        console.error("Network error submitting form", err);
        alert("Could not reach the server. Check your connection and try again.");
    } finally {
        submitButtons.forEach(btn => (btn.disabled = false));
    }
}

// Single delegated listener for the whole document, same intent as the
// jQuery original's `$('form[method=post]')` binding, but re-evaluated per
// submit rather than bound once at page load — so it also covers forms
// injected into the DOM after this script runs.
document.addEventListener("submit", (event: SubmitEvent) => {
    const form = event.target as HTMLElement;
    if (!(form instanceof HTMLFormElement)) return;
    if (form.method.toLowerCase() !== "post") return;
    if (form.matches(NO_AJAX_SELECTOR) || form.closest(NO_AJAX_SELECTOR)) return;

    event.preventDefault();
    void handleSubmit(form, event.submitter);
});
