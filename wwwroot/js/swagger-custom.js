// Obtener token CSRF y configurarlo en todas las solicitudes
function getCsrfToken() {
    return fetch('/antiforgery/token', {
        credentials: 'include'
    })
        .then(response => response.json())
        .then(data => data.token);
}

// Interceptar todas las solicitudes de Swagger
window.onload = function () {
    const oldFetch = window.fetch;
    window.fetch = function (url, options) {
        if (url.startsWith('/api') &&
            ['POST', 'PUT', 'DELETE', 'PATCH'].includes(options?.method?.toUpperCase())) {
            return getCsrfToken().then(token => {
                options = options || {};
                options.headers = options.headers || {};
                options.headers['X-RequestVerificationToken'] = token;
                options.credentials = 'include';
                return oldFetch(url, options);
            });
        }
        return oldFetch(url, options);
    };
};