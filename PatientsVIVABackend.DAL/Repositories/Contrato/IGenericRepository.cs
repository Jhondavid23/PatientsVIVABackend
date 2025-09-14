using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PatientsVIVABackend.DAL.Repositories
{
    public interface IGenericRepository<TModel> where TModel : class
    {
        Task<IQueryable<TModel>> Query(Expression<Func<TModel, bool>> filter = null);
        Task<TModel> Get(Expression<Func<TModel, bool>> filter = null);
        Task<TModel> AddAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task<bool> DeleteAsync(TModel model);
        Task<List<TModel>> ExecuteStoredProcedureAsync<TModel>(string storedProcedureName, params SqlParameter[] parameters) where TModel : class;
    }
}
