using System.Threading.Tasks;

namespace ProjectAmy.ClientWorker.Tasks
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves the username based on the user's ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetNameAsync(string id);
    }
}