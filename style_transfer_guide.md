# SmartWMS Style Transfer Guide

This guide contains the necessary configuration steps and CSS code to apply the SmartWMS look and feel to the **smartProduction** project.

## 1. Dependencies

The project uses **Radzen.Blazor** components and **Bootstrap**.

### NuGet Packages
Ensure the following packages are installed in `smartProduction.csproj`:

```xml
<PackageReference Include="Radzen.Blazor" Version="8.5.1" />
```

## 2. Global Styles (`App.razor`)

Add the following inside the `<head>` tag of your root layout (usually `App.razor` or `_Host.cshtml`):

```html
<!-- Bootstrap -->
<link rel="stylesheet" href="lib/bootstrap/dist/css/bootstrap.min.css" />
<!-- Radzen Material Dark Theme -->
<link rel="stylesheet" href="_content/Radzen.Blazor/css/material-dark.css">
<!-- Custom App Styles -->
<link rel="stylesheet" href="app.css" />
<!-- Scoped Styles -->
<link rel="stylesheet" href="smartProduction.styles.css" />
```

## 3. Custom CSS (`wwwroot/app.css`)

Copy this content into your `wwwroot/app.css` file:

```css
html, body {
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
}

a, .btn-link {
    color: #006bb7;
}

.btn-primary {
    color: #fff;
    background-color: #1b6ec2;
    border-color: #1861ac;
}

.btn:focus, .btn:active:focus, .btn-link.nav-link:focus, .form-control:focus, .form-check-input:focus {
  box-shadow: 0 0 0 0.1rem white, 0 0 0 0.25rem #258cfb;
}

.content {
    padding-top: 1.1rem;
}

h1:focus {
    outline: none;
}

.valid.modified:not([type=checkbox]) {
    outline: 1px solid #26b050;
}

.invalid {
    outline: 1px solid #e50000;
}

.validation-message {
    color: #e50000;
}

.blazor-error-boundary {
    background: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTYiIGhlaWdodD0iNDkiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIG92ZXJmbG93PSJoaWRkZW4iPjxkZWZzPjxjbGlwUGF0aCBpZD0iY2xpcDAiPjxyZWN0IHg9IjIzNSIgeT0iNTEiIHdpZHRoPSI1NiIgaGVpZ2h0PSI0OSIvPjwvY2xpcFBhdGg+PC9kZWZzPjxnIGNsaXAtcGF0aD0idXJsKCNjbGlwMCkiIHRyYW5zZm9ybT0idHJhbnNsYXRlKC0yMzUgLTUxKSI+PHBhdGggZD0iTTI2My41MDYgNTFDMjY0LjcxNyA1MSAyNjUuODEzIDUxLjQ4MzcgMjY2LjYwNiA1Mi4yNjU4TDI2Ny4wNTIgNTIuNzk4NyAyNjcuNTM5IDUzLjYyODMgMjkwLjE4NSA5Mi4xODMxIDI5MC41NDUgOTIuNzk1IDI5MC42NTYgOTIuOTk2QzI5MC44NzcgOTMuNTI5NSAyOTEgOTQuMDgxNSAyOTEgOTQuNjc4MiAyOTEgOTcuMDY1MSAyODkuMDM4IDk5IDI4Ni42MTcgOTlMMjQwLjM4MyA5OUMyMzcuOTYzIDk5IDIzNiA5Ny4wNjUxIDIzNiA5NC42NzgyIDIzNiA5NC4zNzk5IDIzNi4wMzEgOTQuMDg4NiAyMzYuMDg5IDkzLjgwNzJMMjM2LjMzOCA5My4wMTYyIDIzNi44NTggOTIuMTMxNCAyNTkuNDczIDUzLjYyOTQgMjU5Ljk2MSA1Mi43OTg1IDI2MC40MDcgNTIuMjY1OEMyNjEuMiA1MS40ODM3IDI2Mi4yOTYgNTEgMjYzLjUwNiA1MVpNMjYzLjU4NiA2Ni4wMTgzQzI2MC43MzcgNjYuMDE4MyAyNTkuMzEzIDY3LjEyNDUgMjU5LjMxMyA2OS4zMzcgMjU5LjMxMyA2OS42MTAyIDI1OS4zMzIgNjkuODYwOCAyNTkuMzcxIDcwLjA4ODdMMjYxLjc5NSA4NC4wMTYxIDI2NS4zOCA4NC4wMTYxIDI2Ny44MjEgNjkuNzQ3NUMyNjcuODYgNjkuNzMwOSAyNjcuODc5IDY5LjU4NzcgMjY3Ljg3OSA2OS4zMTc5IDI2Ny44NzkgNjcuMTE4MiAyNjYuNDQ4IDY2LjAxODMgMjYzLjU4NiA2Ni4wMTgzWk0yNjMuNTc2IDg2LjA1NDdDMjYxLjA0OSA4Ni4wNTQ3IDI1OS43ODYgODcuMzAwNSAyNTkuNzg2IDg5Ljc5MjEgMjU5Ljc4NiA5Mi4yODM3IDI2MS4wNDkgOTMuNTI5NSAyNjMuNTc2IDkzLjUyOTUgMjY2LjExNiA5My41Mjk1IDI2Ny4zODcgOTIuMjgzNyAyNjcuMzg3IDg5Ljc5MjEgMjY3LjM4NyA4Ny4zMDA1IDI2Ni4xMTYgODYuMDU0NyAyNjMuNTc2IDg2LjA1NDdaIiBmaWxsPSIjRkZFNTAwIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiLz48L2c+PC9zdmc+) no-repeat 1rem/1.8rem, #b32121;
    padding: 1rem 1rem 1rem 3.7rem;
    color: white;
}

.blazor-error-boundary::after {
    content: "An error has occurred."
}

.darker-border-checkbox.form-check-input {
    border-color: #929292;
}

.form-floating > .form-control-plaintext::placeholder, .form-floating > .form-control::placeholder {
    color: var(--bs-secondary-color);
    text-align: end;
}

.form-floating > .form-control-plaintext:focus::placeholder, .form-floating > .form-control:focus::placeholder {
    text-align: start;
}
```

## 4. Main Layout Styles (`MainLayout.razor.css`)

Apply these styles to your main layout CSS (e.g., `Components/Layout/MainLayout.razor.css`):

```css
.page {
    position: relative;
    display: flex;
    flex-direction: column;
}

main {
    flex: 1;
}

.sidebar {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
}

.top-row {
    background-color: #f7f7f7;
    border-bottom: 1px solid #d6d5d5;
    justify-content: flex-end;
    height: 3.5rem;
    display: flex;
    align-items: center;
}

.top-row ::deep a, .top-row ::deep .btn-link {
    white-space: nowrap;
    margin-left: 1.5rem;
    text-decoration: none;
}

.top-row ::deep a:hover, .top-row ::deep .btn-link:hover {
    text-decoration: underline;
}

.top-row ::deep a:first-child {
    overflow: hidden;
    text-overflow: ellipsis;
}

@media (max-width: 640.98px) {
    .top-row {
        justify-content: space-between;
    }

    .top-row ::deep a, .top-row ::deep .btn-link {
        margin-left: 0;
    }
}

@media (min-width: 641px) {
    .page {
        flex-direction: row;
    }

    .sidebar {
        width: 250px;
        height: 100vh;
        position: sticky;
        top: 0;
    }

    .top-row {
        position: sticky;
        top: 0;
        z-index: 1;
    }

    .top-row.auth ::deep a:first-child {
        flex: 1;
        text-align: right;
        width: 0;
    }

    .top-row, article {
        padding-left: 2rem !important;
        padding-right: 1.5rem !important;
    }
}

#blazor-error-ui {
    color-scheme: light only;
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    box-sizing: border-box;
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

#blazor-error-ui .dismiss {
    cursor: pointer;
    position: absolute;
    right: 0.75rem;
    top: 0.5rem;
}
```
