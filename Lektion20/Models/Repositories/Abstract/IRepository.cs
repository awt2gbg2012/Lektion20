using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Lektion20.Models.Entities.Abstract;

namespace Lektion20.Models.Repositories.Abstract
{
    public interface IRepository<T> where T : class, IEntity
    {
        DbContext Model { get; }

        IQueryable<T> FindAll(Func<T, bool> filter = null);
        T FindByID(Guid id);
        void Save(T entity);
        void Delete(T entity);

        void Commit();
    }
}