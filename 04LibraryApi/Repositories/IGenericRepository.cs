﻿namespace _04LibraryApi.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll();

    Task<T> GetByIdAsync(int id);

    Task CreateAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    Task<bool> ExistAsync(int id);
}