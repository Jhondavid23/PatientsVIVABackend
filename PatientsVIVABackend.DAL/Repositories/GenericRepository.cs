
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PatientsVIVABackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PatientsVIVABackend.DAL.Repositories
{
    public class GenericRepository<TModel> : IGenericRepository<TModel> where TModel : class
    {
        private readonly VivapatientsContext _dbVivaPatientsContext;

        public GenericRepository(VivapatientsContext dbVivaPatientsContext)
        {
            _dbVivaPatientsContext = dbVivaPatientsContext;
        }

        // Function to query the database with an optional filter
        // Funcion para consultar la base de datos con un filtro opcional
        public async Task<IQueryable<TModel>> Query(Expression<Func<TModel, bool>> filter = null)
        {
            try
            {
                IQueryable<TModel> modelo = filter == null ? _dbVivaPatientsContext.Set<TModel>() : _dbVivaPatientsContext.Set<TModel>().Where(filter);
                return modelo;
            }
            catch
            {
                throw;
            }
        }

        // Function to get a single record from the database based on a filter
        // Funcion para obtener un solo registro de la base de datos basado en un filtro
        public async Task<TModel> Get(Expression<Func<TModel, bool>> filter = null)
        {
            try
            {
                TModel modelo = await _dbVivaPatientsContext.Set<TModel>().FirstOrDefaultAsync(filter);
                return modelo;
            }
            catch
            {
                throw;
            }
        }

        // Function to add a new record to the database
        // Funcion para agregar un nuevo registro a la base de datos
        public async Task<TModel> AddAsync(TModel model)
        {
            try
            {
                //Base de datos.especificarElModeloATrabajar
                _dbVivaPatientsContext.Set<TModel>().Add(model);
                await _dbVivaPatientsContext.SaveChangesAsync();

                return model;
            }
            catch
            {
                throw;
            }
        }

        // Function to update an existing record in the database
        // Funcion para actualizar un registro existente en la base de datos
        public async Task<TModel> UpdateAsync(TModel model)
        {
            try
            {
                _dbVivaPatientsContext.Set<TModel>().Update(model);
                await _dbVivaPatientsContext.SaveChangesAsync();
                return model;
            }
            catch
            {
                throw;
            }
        }

        // Function to delete a record from the database
        // Funcion para eliminar un registro de la base de datos
        public async Task<bool> DeleteAsync(TModel model)
        {
            try
            {
                _dbVivaPatientsContext.Remove(model);
                await _dbVivaPatientsContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        // Método único para ejecutar procedimientos almacenados que devuelven datos (No escalares)
        // Single method to execute stored procedures that return data (not scalars)
        public async Task<List<TModel>> ExecuteStoredProcedureAsync<TModel>(string storedProcedureName, params SqlParameter[] parameters)
            where TModel : class
        {
            try
            {
                // Construir el comando SQL correctamente
                var sql = $"EXEC {storedProcedureName}";

                if (parameters != null && parameters.Length > 0)
                {
                    // Crear los placeholders de parámetros (@p0, @p1, etc.)
                    var paramPlaceholders = string.Join(", ", parameters.Select((p, index) => $"@p{index}"));
                    sql = $"EXEC {storedProcedureName} {paramPlaceholders}";

                    // Renombrar los parámetros para que coincidan con los placeholders
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        parameters[i].ParameterName = $"@p{i}";
                    }
                }

                return await _dbVivaPatientsContext.Set<TModel>()
                    .FromSqlRaw(sql, parameters)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
