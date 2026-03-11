namespace TravelWeb_API.DTO.ActivityDTO
{
    public class PagedResponseDTO<T>
    {
        public IEnumerable<T>? Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);


        public PagedResponseDTO(IEnumerable<T> data, int pageNumber, int totalRecords,int pageSize)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
        }
    }
}
