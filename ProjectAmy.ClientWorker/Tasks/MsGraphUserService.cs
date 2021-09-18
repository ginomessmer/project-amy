using System.Threading.Tasks;
using Microsoft.Graph;

namespace ProjectAmy.ClientWorker.Tasks
{
    public class MsGraphUserService : IUserService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public MsGraphUserService(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        /// <param name="id"></param>
        /// <inheritdoc />
        public async Task<string> GetNameAsync(string id)
        {
            var user = await _graphServiceClient.Users[id].Request().GetAsync();
            return "";
        }
    }
}