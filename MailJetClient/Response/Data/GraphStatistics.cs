using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailJet.Client.Response.Data
{
    public class GraphStatistics : DataItem
    {

        public int BlockedCount { get; set; }
        public int BouncedCount { get; set; }
        public int ClickedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int OpenedCount { get; set; }
        public int ProcessedCount { get; set; }
        public int QueuedCount { get; set; }
        public string RefTimestamp { get; set; }
        public int SendtimeStart { get; set; }
        public int SpamcomplaintCount { get; set; }
        public int UnsubscribedCount { get; set; }

    }
}
