using System.Collections.Concurrent;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ReportRequestStore
    {
        private readonly ConcurrentDictionary<Guid, ReportRequestStatus> _requests = new();

        public void Add(ReportRequestStatus request)
        {
            _requests[request.RequestId] = request;
        }

        public List<ReportRequestStatus> GetAll()
        {
            return _requests.Values
                .OrderByDescending(x => x.RequestedOn)
                .ToList();
        }

        public ReportRequestStatus? Get(Guid requestId)
        {
            _requests.TryGetValue(requestId, out var request);

            return request;
        }

        public void Update(ReportRequestStatus request)
        {
            _requests[request.RequestId] = request;
        }
    }
}
