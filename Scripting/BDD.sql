create database SistemaGestionDeIncidentesSeguridad;
USE SistemaGestionDeIncidentesSeguridad;

CREATE TABLE Usuario (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
	Apellido NVARCHAR(100) NOT NULL,
    CorreoElectronico NVARCHAR(100) NOT NULL UNIQUE,
    Contrasenia NVARCHAR(255) NOT NULL,
    Rol INT NOT NULL
);

INSERT INTO Usuario (Nombre, Apellido, CorreoElectronico, Contrasenia, Rol)
VALUES ('Admin', 'Intermedio', 'adminintermedio@gmail.com', '0r9JDTsogYsTOgNyTTVFx1Hy677SvxM9xi3C//UM3jU=', 2);
VALUES ('Admin', 'General', 'admingeneral@gmail.com', '0r9JDTsogYsTOgNyTTVFx1Hy677SvxM9xi3C//UM3jU=', 3);

-- Contraseña de los 2 admin es: admin123
Select * from Usuario;
