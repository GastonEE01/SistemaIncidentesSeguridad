function getJwtToken() {
    return fetch('/Account/GetToken', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('No se pudo obtener el token JWT');
        }
        return response.json();
    })
    .then(data => data.token);
}

function makeAuthenticatedRequest(url, options = {}) {
    return getJwtToken()
        .then(token => {
            const headers = {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
                ...options.headers
            };

            return fetch(url, {
                ...options,
                headers
            });
        });
}

function getTicketsPendientes() {
    makeAuthenticatedRequest('/AdminIntermedio/api/tickets/pendientes')
        .then(response => response.json())
        .then(data => {
            console.log('Tickets pendientes:', data);
            // Aquí puedes actualizar la UI con los datos
        })
        .catch(error => {
            console.error('Error al obtener tickets pendientes:', error);
        });
}

function responderTicket(ticketId, nuevoEstado, contenidoComentario) {
    const requestBody = {
        NuevoEstado: nuevoEstado,
        ContenidoComentario: contenidoComentario
    };

    makeAuthenticatedRequest(`/AdminIntermedio/api/tickets/${ticketId}/responder`, {
        method: 'POST',
        body: JSON.stringify(requestBody)
    })
    .then(response => response.json())
    .then(data => {
        console.log('Respuesta:', data);
        if (data.message) {
            alert(data.message);
            location.reload(); 
        }
    })
    .catch(error => {
        console.error('Error al responder ticket:', error);
        alert('Error al responder el ticket');
    });
}

function checkAuthentication() {
    return getJwtToken()
        .then(token => {
            return true;
        })
        .catch(error => {
            window.location.href = '/Account/Login';
            return false;
        });
}

document.addEventListener('DOMContentLoaded', function() {
    if (window.location.pathname.includes('/AdminIntermedio') || 
        window.location.pathname.includes('/AdminGeneral')) {
        checkAuthentication();
    }
});
