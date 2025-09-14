-- Procedimiento almacenado para obtener pacientes creados después de una fecha específica

USE [VIVAPATIENTS]
GO

-- Eliminar el procedimiento si ya existe
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPatientsCreatedAfter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPatientsCreatedAfter]
GO

-- Crear el procedimiento almacenado
CREATE PROCEDURE [dbo].[GetPatientsCreatedAfter]
    @CreatedAfter DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar parámetro de entrada
    IF @CreatedAfter IS NULL
    BEGIN
        RAISERROR('El parámetro @CreatedAfter no puede ser NULL', 16, 1)
        RETURN
    END
    
    BEGIN TRY
        -- Seleccionar pacientes creados después de la fecha especificada
        SELECT 
            p.PatientId,
            p.DocumentType,
            p.DocumentNumber,
            p.FirstName,
            p.LastName,
            p.BirthDate,
            p.PhoneNumber,
            p.Email,
            p.CreatedAt
        FROM 
            Patients p
        WHERE 
            p.CreatedAt > @CreatedAfter
        ORDER BY 
            p.CreatedAt DESC, p.PatientId DESC
            
        -- Información adicional sobre la consulta
        SELECT 
            COUNT(*) AS TotalPatientsFound,
            @CreatedAfter AS FilterDate,
            GETUTCDATE() AS QueryExecutedAt
        FROM 
            Patients p
        WHERE 
            p.CreatedAt > @CreatedAfter
            
    END TRY
    BEGIN CATCH
        -- Manejo de errores
        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;

        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Otorgar permisos de ejecución
GRANT EXECUTE ON [dbo].[GetPatientsCreatedAfter] TO PUBLIC
GO

-- Ejemplo de uso:
-- EXEC GetPatientsCreatedAfter '2024-01-01'