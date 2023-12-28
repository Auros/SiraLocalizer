using System.Collections.Generic;

namespace SiraLocalizer.Providers.CrowdinApi.Models
{
    internal class PaginatedDataResponse<T>
    {
        public IList<DataResponse<T>> data { get; set; }

        public Pagination pagination { get; set; }
    }
}
