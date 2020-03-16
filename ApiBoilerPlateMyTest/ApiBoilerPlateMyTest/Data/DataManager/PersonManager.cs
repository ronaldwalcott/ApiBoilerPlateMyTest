using ApiBoilerPlateMyTest.Contracts;
using ApiBoilerPlateMyTest.Data.Entity;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace ApiBoilerPlateMyTest.Data.DataManager
{
    public class PersonManager : IPersonManager
    {
        private readonly ILogger<PersonManager> _logger;
        private readonly TestDBContext _context;

        public PersonManager(TestDBContext dbContext, ILogger<PersonManager> logger) 
        {
            _logger = logger;
            _context = dbContext;
        }
        public async Task<(IEnumerable<Person> Persons, Pagination Pagination)> GetPersons(UrlQueryParameters urlQueryParameters)
        {
            IEnumerable<Person> persons;
            int recordCount = 0;

            persons = await _context.Persons.ToListAsync();



            ////For SqlServer
            //var query = @"SELECT ID, FirstName, LastName, DateOfBirth FROM Person
            //                ORDER BY ID DESC
            //                OFFSET @Limit * (@Offset -1) ROWS
            //                FETCH NEXT @Limit ROWS ONLY";

            //var param = new DynamicParameters();
            //param.Add("Limit", urlQueryParameters.PageSize);
            //param.Add("Offset", urlQueryParameters.PageNumber);

            //if (urlQueryParameters.IncludeCount)
            //{
            //    query += " SELECT COUNT(ID) FROM Person";
            //    var pagedRows = await DbQueryMultipleAsync<Person>(query, param);

            //    persons = pagedRows.Data;
            //    recordCount = pagedRows.RecordCount;
            //}
            //else
            //{
            //    persons = await DbQueryAsync<Person>(query, param);
            //}



            var metadata = new Pagination
            {
                PageNumber = urlQueryParameters.PageNumber,
                PageSize = urlQueryParameters.PageSize,
                TotalRecords = recordCount

            };

            return (persons, metadata);

        }
        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.Persons.ToListAsync();
        }

        public async Task<Person> GetById(object id)
        {
            return await _context.Persons.Where(p => p.ID == Convert.ToInt32(id)).SingleOrDefaultAsync();
        }

        public async Task<long> CreateAsync(Person person)
        {
            var personAdd = new Person();
            personAdd.DateOfBirth = person.DateOfBirth;
            personAdd.FirstName = person.FirstName;
            personAdd.LastName = person.LastName;
            _context.Persons.Add(personAdd);

            return await _context.SaveChangesAsync();
        }
        public async Task<bool> UpdateAsync(Person person)
        {
            if (await ExistAsync(person))
            {
                var personUpdate = await _context.Persons.Where(p => p.ID == person.ID).SingleOrDefaultAsync();
                personUpdate.DateOfBirth = person.DateOfBirth;
                personUpdate.FirstName = person.FirstName;
                personUpdate.LastName = person.LastName;
                _context.Persons.Update(personUpdate);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> DeleteAsync(object id)
        {
            if (await _context.Persons.AnyAsync(p => p.ID == Convert.ToInt32(id)))
            {
                _context.Persons.Remove(await GetById(id));
                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }
        public async Task<bool> ExistAsync(object id)
        {
            return await _context.Persons.AnyAsync(p => p.ID == Convert.ToInt32(id));
        }

        //public async Task<bool> ExecuteWithTransactionScope(object entity)
        //{

        //    using (var dbCon = new SqlConnection(DbConnectionString))
        //    {
        //        await dbCon.OpenAsync();
        //        var transaction = await dbCon.BeginTransactionAsync();

        //        try
        //        {
        //            //Do stuff here Insert, Update or Delete
        //            Task q1 = dbCon.ExecuteAsync("<Your SQL Query here>");
        //            Task q2 = dbCon.ExecuteAsync("<Your SQL Query here>");
        //            Task q3 = dbCon.ExecuteAsync("<Your SQL Query here>");

        //            await Task.WhenAll(q1, q2, q3);

        //            //Commit the Transaction when all query are executed successfully

        //            await transaction.CommitAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            //Rollback the Transaction when any query fails
        //            transaction.Rollback();
        //            _logger.Log(LogLevel.Error, ex, "Error when trying to execute database operations within a scope.");

        //            return false;
        //        }
        //    }
        //    return true;
        //}

    }
}
