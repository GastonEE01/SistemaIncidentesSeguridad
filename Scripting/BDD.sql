-- Base de datos
CREATE DATABASE SistemaGestionDeIncidentesSeguridad;
USE SistemaGestionDeIncidentesSeguridad;

-- Tabla: Usuario (ya la tienes, incluida por completitud)
CREATE TABLE Usuario (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    CorreoElectronico NVARCHAR(100) NOT NULL UNIQUE,
    Contrasenia NVARCHAR(255) NOT NULL,
    Rol INT NOT NULL
);

-- Insertar admins de ejemplo
INSERT INTO Usuario (Nombre, Apellido, CorreoElectronico, Contrasenia, Rol)
VALUES 
('Admin', 'Intermedio', 'adminintermedio@gmail.com', '0r9JDTsogYsTOgNyTTVFx1Hy677SvxM9xi3C//UM3jU=', 2),
('Admin', 'General', 'admingeneral@gmail.com', '0r9JDTsogYsTOgNyTTVFx1Hy677SvxM9xi3C//UM3jU=', 3);
--Contraseña: admin123

---------------------------------------------------
-- 1. Tabla: Categoria
---------------------------------------------------
CREATE TABLE Categoria (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL
);

---------------------------------------------------
-- 2. Tabla: Estado
---------------------------------------------------
CREATE TABLE Estado (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL
);

---------------------------------------------------
-- 3. Tabla: Prioridad
---------------------------------------------------
CREATE TABLE Prioridad (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL
);

---------------------------------------------------
-- 4. Tabla: Ticket
---------------------------------------------------
CREATE TABLE Ticket (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Titulo NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(MAX),
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    FechaResolucion DATETIME NULL,
    IdUsuario INT NOT NULL,       -- Quien reporta
    IdCategoria INT NOT NULL,
    IdEstado INT NOT NULL,
    IdPrioridad INT NOT NULL,
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    FOREIGN KEY (IdCategoria) REFERENCES Categoria(Id),
    FOREIGN KEY (IdEstado) REFERENCES Estado(Id),
    FOREIGN KEY (IdPrioridad) REFERENCES Prioridad(Id)
);

---------------------------------------------------
-- 5. Tabla: Comentario
---------------------------------------------------
CREATE TABLE Comentario (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdTicket INT NOT NULL,
    IdUsuario INT NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (IdTicket) REFERENCES Ticket(Id),
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

---------------------------------------------------
-- Insertar datos de ejemplo para Estado, Prioridad y Categoría
---------------------------------------------------

-- Estados
INSERT INTO Estado (Nombre) VALUES 
('Abierto'), 
('En Progreso'), 
('Cerrado');

-- Prioridades
INSERT INTO Prioridad (Nombre) VALUES 
('Baja'), 
('Media'), 
('Alta'), 
('Crítica');

-- Categorías
INSERT INTO Categoria (Nombre) VALUES 
('Malware'),
('Phishing', ),
('Acceso no autorizado'),
('Pérdida de datos');
