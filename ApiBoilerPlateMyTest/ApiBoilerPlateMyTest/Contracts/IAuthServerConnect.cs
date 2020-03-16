using System.Threading.Tasks;

namespace ApiBoilerPlateMyTest.Contracts
{
    public interface IAuthServerConnect
    {
        Task<string> RequestClientCredentialsTokenAsync();
    }
}
