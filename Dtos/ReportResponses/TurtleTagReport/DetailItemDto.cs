using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses.TurtleTagReport
{
    public class DetailItemDto
    {
        public DetailItemDto()
        {
            tags = new List<DetailItemTagDto>();
        }

        public string seaTurtleId { get; set; }
        public string sidNumber { get; set; }
        public string seaTurtleName { get; set; }
        public string dateRelinquished { get; set; }
        public string strandingIdNumber { get; set; }
        public List<DetailItemTagDto> tags { get; set; }
    }
}
