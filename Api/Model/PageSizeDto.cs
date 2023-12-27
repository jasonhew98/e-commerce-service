using System;

namespace Api.Model
{
    public class PageSizeDto
    {
        public readonly long Count;
        public readonly long PageSize;
        public readonly long NumberOfPages;

        public PageSizeDto(
            long count,
            long? pageSize)
        {
            Count = count;
            PageSize = (pageSize ?? 0) <= 0 ? count : pageSize.Value;
            NumberOfPages = count == 0 ? 1 : (long)Math.Ceiling((double)count / PageSize);
        }
    }
}
