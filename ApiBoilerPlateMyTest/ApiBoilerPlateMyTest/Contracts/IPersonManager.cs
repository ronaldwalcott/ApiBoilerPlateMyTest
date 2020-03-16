using ApiBoilerPlateMyTest.Data;
using ApiBoilerPlateMyTest.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiBoilerPlateMyTest.Contracts
{
    public interface IPersonManager : IRepository<Person>
    {
        Task<(IEnumerable<Person> Persons, Pagination Pagination)> GetPersons(UrlQueryParameters urlQueryParameters);

        //Add more class specific methods here when neccessary
    }
}
