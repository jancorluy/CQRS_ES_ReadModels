using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disruptor.ReadModel.Monitoring.Web.Models
{
    public class ReadModelState
    {
        public string ReadmodelType { get; set; }

        public long CommitPosition { get; set; }

        public long PreparePosition { get; set; }

        public DateTime LastComittedPosition { get; set; }
    }
}