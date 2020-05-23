using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses.TurtleTagReport
{
    public class ContentDto
    {
        public ContentDto()
        {
            detailItems = new List<DetailItemDto>();
        }

        public List<DetailItemDto> detailItems { get; set; }
    }
}
