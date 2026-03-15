using System;
using System.Collections.Generic;
using System.Linq;

namespace BonBon.McpServers.Clients.SeedWork;

public class PageInfoDto
{
    public int Total { get; set; }
    public int ItemPerPage { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;

    public IEnumerable<int> PageRange => CalculatePageRange(CurrentPage, Total, ItemPerPage);
    public int From => (CurrentPage - 1) * ItemPerPage + 1;
    public int To => CurrentPage * ItemPerPage > Total ? Total : CurrentPage * ItemPerPage;
    public int LastPage => Total % ItemPerPage == 0 ? Total / ItemPerPage : Total / ItemPerPage + 1;
    public int FirstPage => 1;
    public int? PreviousPage => CurrentPage > 1 ? CurrentPage - 1 : null;
    public int? NextPage => CurrentPage < LastPage ? CurrentPage + 1 : null;
    
    private static List<int> CalculatePageRange(int pageIndex, int total, int pageSize, int maxPages = 5)
    {
        var pageRange = new List<int>();

        // Calculate the total number of pages
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        // Determine the start and end of the range
        var start = Math.Max(1, pageIndex - maxPages / 2);
        var end = Math.Min(totalPages, pageIndex + maxPages / 2);

        // Adjust the range to ensure it always shows maxPages items when possible
        if (end - start + 1 < maxPages)
        {
            if (start == 1)
            {
                end = Math.Min(totalPages, start + maxPages - 1);
            }
            else if (end == totalPages)
            {
                start = Math.Max(1, end - maxPages + 1);
            }
        }

        // Populate the list with the page range
        for (var i = start; i <= end; i++)
        {
            pageRange.Add(i);
        }

        return pageRange;
    }
}